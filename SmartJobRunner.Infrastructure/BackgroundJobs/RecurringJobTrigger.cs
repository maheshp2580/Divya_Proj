using System;
using System.Threading.Tasks;
using SmartJobRunner.Application.Interfaces;
using SmartJobRunner.Domain.Entities;
using SmartJobRunner.Domain.Enums;

namespace SmartJobRunner.Infrastructure.BackgroundJobs;

// This class creates the Execution record before enqueuing the actual worker so we have a trace.
public class RecurringJobTrigger
{
    private readonly IApplicationDbContext _context;
    private readonly IJobSchedulerService _schedulerService;

    public RecurringJobTrigger(IApplicationDbContext context, IJobSchedulerService schedulerService)
    {
        _context = context;
        _schedulerService = schedulerService;
    }

    public async Task TriggerAsync(Guid jobDefinitionId)
    {
        var execution = new JobExecution
        {
            JobDefinitionId = jobDefinitionId,
            TriggeredBy = "Scheduled",
            Status = ExecutionStatus.Pending,
            StartedAt = DateTime.UtcNow
        };

        _context.JobExecutions.Add(execution);
        await _context.SaveChangesAsync(default);

        await _schedulerService.EnqueueJobExecutionAsync(execution.Id);
    }
}
