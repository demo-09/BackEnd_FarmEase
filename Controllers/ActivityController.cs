using backEnd.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,Admin")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllActivities()
    {
        var activities = await _activityService.GetAllActivitiesAsync();
        return Ok(activities);
    }

    [HttpPost("log")]
    public async Task<IActionResult> LogActivity([FromBody] ActivityDto dto)
    {
        // Get user details from JWT claims if available
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Guest";
        var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Guest";

        await _activityService.LogActivityAsync(dto.ActionType, dto.Details, email, name);
        return Ok();
    }
}

public class ActivityDto
{
    public string ActionType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
