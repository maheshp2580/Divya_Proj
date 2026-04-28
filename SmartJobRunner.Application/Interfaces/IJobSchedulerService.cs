using System;
using System.Threading.Tasks;

namespace SmartJobRunner.Application.Interfaces;

public interface IJobSchedulerService
{
    Task EnqueueJobExecutionAsync(Guid executionId);
    void ScheduleRecurringJob(Guid jobDefinitionId, string cronExpression);
    void RemoveRecurringJob(Guid jobDefinitionId);
}
