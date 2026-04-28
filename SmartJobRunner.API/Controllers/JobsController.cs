using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartJobRunner.Application.Jobs.Commands;
using SmartJobRunner.Application.Jobs.Queries;

namespace SmartJobRunner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateJob(CreateJobDefinitionCommand command)
    {
        var jobId = await _mediator.Send(command);
        return Ok(new { JobId = jobId });
    }

    [HttpPost("{id}/execute")]
    public async Task<IActionResult> ExecuteJob(Guid id)
    {
        var executionId = await _mediator.Send(new ExecuteJobCommand(id));
        return Ok(new { ExecutionId = executionId });
    }
    
    [HttpPost("{id}/schedule")]
    public async Task<IActionResult> ScheduleJob(Guid id, [FromBody] string cronExpression)
    {
        var success = await _mediator.Send(new ScheduleJobCommand(id, cronExpression));
        if (!success) return NotFound();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs()
    {
        var jobs = await _mediator.Send(new GetJobsQuery());
        return Ok(jobs);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetJob(Guid id)
    {
        var job = await _mediator.Send(new GetJobDetailsQuery(id));
        if (job == null) return NotFound();
        return Ok(job);
    }

    [HttpGet("{id}/executions")]
    public async Task<IActionResult> GetJobExecutions(Guid id)
    {
        var executions = await _mediator.Send(new GetJobExecutionsQuery(id));
        return Ok(executions);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJob(Guid id)
    {
        var success = await _mediator.Send(new DeleteJobCommand(id));
        if (!success) return NotFound();
        return NoContent();
    }
    
    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleJobStatus(Guid id)
    {
        var success = await _mediator.Send(new ToggleJobStatusCommand(id));
        if (!success) return NotFound();
        return Ok();
    }
}
