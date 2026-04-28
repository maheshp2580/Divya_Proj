using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartJobRunner.Application.Interfaces;
using SmartJobRunner.Domain.Enums;

namespace SmartJobRunner.Application.Jobs.Commands;

public record ToggleJobStatusCommand(Guid Id) : IRequest<bool>;

public class ToggleJobStatusCommandHandler : IRequestHandler<ToggleJobStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IJobSchedulerService _schedulerService;

    public ToggleJobStatusCommandHandler(IApplicationDbContext context, IJobSchedulerService schedulerService)
    {
        _context = context;
        _schedulerService = schedulerService;
    }

    public async Task<bool> Handle(ToggleJobStatusCommand request, CancellationToken cancellationToken)
    {
        var jobDef = await _context.JobDefinitions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (jobDef == null) return false;

        jobDef.IsActive = !jobDef.IsActive; // Toggle it

        // Auto-pause or resume the Hangfire recurring cron job
        if (jobDef.ScheduleType == ScheduleType.Recurring)
        {
            if (!jobDef.IsActive)
            {
                _schedulerService.RemoveRecurringJob(jobDef.Id);
            }
            else if (!string.IsNullOrEmpty(jobDef.CronExpression))
            {
                _schedulerService.ScheduleRecurringJob(jobDef.Id, jobDef.CronExpression);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
