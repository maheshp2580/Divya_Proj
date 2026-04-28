using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartJobRunner.Application.Interfaces;
using SmartJobRunner.Domain.Enums;

namespace SmartJobRunner.Application.Jobs.Queries;

public record JobDetailsDto(
    Guid Id, string Name, string Description, string JobType,
    string ScheduleType, bool IsActive, string? CronExpression,
    string? HttpMethod, string? Url, string? Payload,
    string? StoredProcedureName, int RetryCount, int BaseDelaySeconds, bool SimulateFailureForDemo);

public record GetJobDetailsQuery(Guid JobDefinitionId) : IRequest<JobDetailsDto?>;

public class GetJobDetailsQueryHandler : IRequestHandler<GetJobDetailsQuery, JobDetailsDto?>
{
    private readonly IApplicationDbContext _context;

    public GetJobDetailsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JobDetailsDto?> Handle(GetJobDetailsQuery request, CancellationToken cancellationToken)
    {
        var job = await _context.JobDefinitions.FirstOrDefaultAsync(x => x.Id == request.JobDefinitionId, cancellationToken);
        if (job == null) return null;

        return new JobDetailsDto(
            job.Id, job.Name, job.Description, job.JobType.ToString(),
            job.ScheduleType.ToString(), job.IsActive, job.CronExpression,
            job.HttpMethod, job.Url, job.Payload,
            job.StoredProcedureName, job.RetryCount, job.BaseDelaySeconds, job.SimulateFailureForDemo);
    }
}
