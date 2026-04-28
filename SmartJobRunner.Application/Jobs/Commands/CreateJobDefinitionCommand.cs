using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmartJobRunner.Application.Interfaces;
using SmartJobRunner.Domain.Entities;
using SmartJobRunner.Domain.Enums;

namespace SmartJobRunner.Application.Jobs.Commands;

public class CreateJobDefinitionCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JobType JobType { get; set; }
    public ScheduleType ScheduleType { get; set; }
    public string? CronExpression { get; set; }
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public string? Payload { get; set; }
    public string? StoredProcedureName { get; set; }
    public int RetryCount { get; set; }
    public int BaseDelaySeconds { get; set; }
    public bool SimulateFailureForDemo { get; set; }
}

public class CreateJobDefinitionCommandHandler : IRequestHandler<CreateJobDefinitionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateJobDefinitionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateJobDefinitionCommand request, CancellationToken cancellationToken)
    {
        var jobDef = new JobDefinition
        {
            Name = request.Name,
            Description = request.Description,
            JobType = request.JobType,
            ScheduleType = request.ScheduleType,
            CronExpression = request.CronExpression,
            HttpMethod = request.HttpMethod,
            Url = request.Url,
            Payload = request.Payload,
            StoredProcedureName = request.StoredProcedureName,
            RetryCount = request.RetryCount,
            BaseDelaySeconds = request.BaseDelaySeconds,
            SimulateFailureForDemo = request.SimulateFailureForDemo,
            IsActive = true
        };

        _context.JobDefinitions.Add(jobDef);
        await _context.SaveChangesAsync(cancellationToken);

        return jobDef.Id;
    }
}
