using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartJobRunner.Application.Interfaces;
using SmartJobRunner.Domain.Entities;

namespace SmartJobRunner.Application.Jobs.Commands;

public record ExecuteJobCommand(Guid JobDefinitionId) : IRequest<Guid>;

public class ExecuteJobCommandHandler : IRequestHandler<ExecuteJobCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IJobSchedulerService _schedulerService;

    public ExecuteJobCommandHandler(IApplicationDbContext context, IJobSchedulerService schedulerService)
    {
        _context = context;
        _schedulerService = schedulerService;
    }

    public async Task<Guid> Handle(ExecuteJobCommand request, CancellationToken cancellationToken)
    {
        var jobDef = await _context.JobDefinitions
            .FirstOrDefaultAsync(x => x.Id == request.JobDefinitionId, cancellationToken);
            
        if (jobDef == null)
            throw new Exception("Job Definition not found."); // In real app, use better exceptions

        var execution = new JobExecution
        {
            JobDefinitionId = jobDef.Id,
            TriggeredBy = "Manual",
            Status = Domain.Enums.ExecutionStatus.Pending,
            StartedAt = DateTime.UtcNow
        };

        _context.JobExecutions.Add(execution);
        await _context.SaveChangesAsync(cancellationToken);

        // Enqueue to background worker
        await _schedulerService.EnqueueJobExecutionAsync(execution.Id);

        return execution.Id;
    }
}
