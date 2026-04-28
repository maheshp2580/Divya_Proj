using System;
using System.Collections.Generic;
using SmartJobRunner.Domain.Enums;

namespace SmartJobRunner.Domain.Entities;

public class JobDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JobType JobType { get; set; } = JobType.Http;
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Manual;
    public bool IsActive { get; set; } = true;
    public string? CronExpression { get; set; }
    
    // HTTP Job Properties
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public string? Payload { get; set; }
    
    // Database Job Properties
    public string? StoredProcedureName { get; set; }
    
    // Configurable resilience
    public int RetryCount { get; set; } = 3;
    public int BaseDelaySeconds { get; set; } = 2; // base delay for exponential backoff
    
    // Simulate failing work occasionally
    public bool SimulateFailureForDemo { get; set; } = false;
    
    public ICollection<JobExecution> Executions { get; set; } = new List<JobExecution>();
}
