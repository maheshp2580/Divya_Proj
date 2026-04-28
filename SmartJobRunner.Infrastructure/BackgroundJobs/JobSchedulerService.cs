using System;
using System.Threading.Tasks;
using Hangfire;
using SmartJobRunner.Application.Interfaces;

namespace SmartJobRunner.Infrastructure.BackgroundJobs;

public class JobSchedulerService : IJobSchedulerService
{
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly IRecurringJobManager _recurringJobs;

    public JobSchedulerService(IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobs)
    {
        _backgroundJobs = backgroundJobs;
        _recurringJobs = recurringJobs;
    }

    public Task EnqueueJobExecutionAsync(Guid executionId)
    {
        _backgroundJobs.Enqueue<JobWorker>(x => x.ExecuteAsync(executionId));
        return Task.CompletedTask;
    }

    public void ScheduleRecurringJob(Guid jobDefinitionId, string cronExpression)
    {
        // For recurring jobs, we enqueue a trigger that creates a JobExecution first.
        _recurringJobs.AddOrUpdate<RecurringJobTrigger>(
            jobDefinitionId.ToString(), 
            x => x.TriggerAsync(jobDefinitionId), 
            cronExpression);
    }

    public void RemoveRecurringJob(Guid jobDefinitionId)
    {
        _recurringJobs.RemoveIfExists(jobDefinitionId.ToString());
    }
}
