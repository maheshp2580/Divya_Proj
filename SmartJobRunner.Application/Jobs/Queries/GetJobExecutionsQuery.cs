using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartJobRunner.Application.Interfaces;

namespace SmartJobRunner.Application.Jobs.Queries;

public record JobExecutionDto(Guid Id, string TriggeredBy, string Status, DateTime StartedAt, DateTime? CompletedAt, string? ErrorMessage, string? AIAnalysis);

public record GetJobExecutionsQuery(Guid JobDefinitionId) : IRequest<List<JobExecutionDto>>;

public class GetJobExecutionsQueryHandler : IRequestHandler<GetJobExecutionsQuery, List<JobExecutionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetJobExecutionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<JobExecutionDto>> Handle(GetJobExecutionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.JobExecutions
            .Where(x => x.JobDefinitionId == request.JobDefinitionId)
            .OrderByDescending(x => x.StartedAt)
            .Select(x => new JobExecutionDto(x.Id, x.TriggeredBy, x.Status.ToString(), x.StartedAt, x.CompletedAt, x.ErrorMessage, x.AIAnalysis))
            .ToListAsync(cancellationToken);
    }
}
