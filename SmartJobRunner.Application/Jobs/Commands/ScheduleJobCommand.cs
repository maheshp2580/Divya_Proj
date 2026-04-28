using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartJobRunner.Application.Interfaces;

namespace SmartJobRunner.Application.Jobs.Commands;

public record ScheduleJobCommand(Guid JobDefinitionId, string CronExpression) : IRequest<bool>;

public class ScheduleJobCommandHandler : IRequestHandler<ScheduleJobCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IJobSchedulerService _schedulerService;

    public ScheduleJobCommandHandler(IApplicationDbContext context, IJobSchedulerService schedulerService)
    {
        _context = context;
        _schedulerService = schedulerService;
    }

    public async Task<bool> Handle(ScheduleJobCommand request, CancellationToken cancellationToken)
    {
        var jobDef = await _context.JobDefinitions.FirstOrDefaultAsync(x => x.Id == request.JobDefinitionId, cancellationToken);
        if (jobDef == null) return false;

        jobDef.CronExpression = request.CronExpression;
        jobDef.ScheduleType = Domain.Enums.ScheduleType.Recurring;
        await _context.SaveChangesAsync(cancellationToken);

        _schedulerService.ScheduleRecurringJob(jobDef.Id, request.CronExpression);
        return true;
    }
}
