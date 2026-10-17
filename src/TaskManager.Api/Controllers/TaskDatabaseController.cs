using Microsoft.AspNetCore.Mvc;
using TaskManager.Infrastructure.Queries;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/task-database")]
public sealed class TaskDatabaseController(
    TaskSummaryQuery taskSummaryQuery,
    TaskCountByTeamQuery taskCountQuery,
    TeamTaskReportQuery teamTaskReportQuery) : ControllerBase
{
    // GET: api/task-database/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        CancellationToken cancellationToken)
    {
        var result = await taskSummaryQuery.GetAllAsync(
            cancellationToken);

        return Ok(result);
    }

    // GET: api/task-database/teams/1/task-count
    [HttpGet("teams/{teamId:int}/task-count")]
    public async Task<IActionResult> GetTaskCountByTeam(
        int teamId,
        CancellationToken cancellationToken)
    {
        if (teamId <= 0)
        {
            return BadRequest("Team ID must be greater than zero.");
        }

        var count = await taskCountQuery.GetCountAsync(
            teamId,
            cancellationToken);

        return Ok(new
        {
            TeamId = teamId,
            TaskCount = count
        });
    }

    // GET: api/task-database/teams/1/report
    [HttpGet("teams/{teamId:int}/report")]
    public async Task<IActionResult> GetTeamReport(
        int teamId,
        CancellationToken cancellationToken)
    {
        if (teamId <= 0)
        {
            return BadRequest("Team ID must be greater than zero.");
        }

        var result = await teamTaskReportQuery.GetByTeamAsync(
            teamId,
            cancellationToken);

        return Ok(result);
    }
}