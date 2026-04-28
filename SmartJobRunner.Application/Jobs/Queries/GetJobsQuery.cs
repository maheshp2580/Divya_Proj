using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartJobRunner.Application.Interfaces;

namespace SmartJobRunner.Application.Jobs.Queries;

public record JobDefinitionDto(Guid Id, string Name, string Description, string JobType, string ScheduleType, bool IsActive, string? CronExpression);

public record GetJobsQuery : IRequest<List<JobDefinitionDto>>;

public class GetJobsQueryHandler : IRequestHandler<GetJobsQuery, List<JobDefinitionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetJobsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<JobDefinitionDto>> Handle(GetJobsQuery request, CancellationToken cancellationToken)
    {
        return await _context.JobDefinitions
            .Select(x => new JobDefinitionDto(x.Id, x.Name, x.Description, x.JobType.ToString(), x.ScheduleType.ToString(), x.IsActive, x.CronExpression))
            .ToListAsync(cancellationToken);
    }
}
