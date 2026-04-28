using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartJobRunner.Application.Interfaces;

namespace SmartJobRunner.Application.Jobs.Commands;

public record DeleteJobCommand(Guid Id) : IRequest<bool>;

public class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IJobSchedulerService _schedulerService;

    public DeleteJobCommandHandler(IApplicationDbContext context, IJobSchedulerService schedulerService)
    {
        _context = context;
        _schedulerService = schedulerService;
    }

    public async Task<bool> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
    {
        var jobDef = await _context.JobDefinitions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (jobDef == null) return false;

        if (jobDef.ScheduleType == Domain.Enums.ScheduleType.Recurring)
        {
            _schedulerService.RemoveRecurringJob(jobDef.Id);
        }

        _context.JobDefinitions.Remove(jobDef);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
