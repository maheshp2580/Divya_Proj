using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Polly;
using SmartJobRunner.Application.Interfaces;
using SmartJobRunner.Domain.Enums;

namespace SmartJobRunner.Infrastructure.BackgroundJobs;

public class JobWorker
{
    private readonly IApplicationDbContext _context;
    private readonly IAiAnalysisService _aiAnalysisService;
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

    public JobWorker(IApplicationDbContext context, IAiAnalysisService aiAnalysisService)
    {
        _context = context;
        _aiAnalysisService = aiAnalysisService;
    }

    public async Task ExecuteAsync(Guid executionId)
    {
        var execution = await _context.JobExecutions
            .Include(x => x.JobDefinition)
            .FirstOrDefaultAsync(x => x.Id == executionId);

        if (execution == null) return;

        execution.Status = ExecutionStatus.Running;
        await _context.SaveChangesAsync(default);

        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                execution.JobDefinition.RetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(execution.JobDefinition.BaseDelaySeconds, retryAttempt))
            );

        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                if (execution.JobDefinition.SimulateFailureForDemo)
                {
                    if (new Random().Next(0, 10) < 5)
                        throw new Exception("Transient demo error: Database connection timeout.");
                    else
                        throw new InvalidOperationException("Permanent demo error: Invalid data.");
                }

                if (execution.JobDefinition.JobType == JobType.Http)
                {
                    var url = execution.JobDefinition.Url;
                    if (string.IsNullOrEmpty(url)) throw new Exception("HTTP Job requires a valid URL.");

                    var request = new HttpRequestMessage(new HttpMethod(execution.JobDefinition.HttpMethod ?? "GET"), url);
                    
                    if (!string.IsNullOrEmpty(execution.JobDefinition.Payload))
                    {
                        request.Content = new StringContent(execution.JobDefinition.Payload, Encoding.UTF8, "application/json");
                    }

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                }
                else if (execution.JobDefinition.JobType == JobType.Database)
                {
                    // Simulated DB call
                    await Task.Delay(1000);
                    var spName = execution.JobDefinition.StoredProcedureName ?? "UNKNOWN_PROC";
                    Console.WriteLine($"[DB Simulation] Executed Stored Procedure: {spName}");
                }

                execution.Status = ExecutionStatus.Succeeded;
                execution.CompletedAt = DateTime.UtcNow;
            });
            await _context.SaveChangesAsync(default);
        }
        catch (Exception ex)
        {
            execution.Status = ExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            execution.CompletedAt = DateTime.UtcNow;
            
            execution.AIAnalysis = await _aiAnalysisService.AnalyzeFailureAsync(execution.JobDefinition.Name, ex.Message);
            await _context.SaveChangesAsync(default);
            
            throw;
        }
    }
}
