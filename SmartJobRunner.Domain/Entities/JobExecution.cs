using System;
using SmartJobRunner.Domain.Enums;

namespace SmartJobRunner.Domain.Entities;

public class JobExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid JobDefinitionId { get; set; }
    public JobDefinition JobDefinition { get; set; } = null!;
    
    public string TriggeredBy { get; set; } = "Manual"; // Manual or Scheduled
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    public string? ErrorMessage { get; set; }
    public string? AIAnalysis { get; set; }
}
