using backEnd.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet("all")]
    [Authorize(Roles = "admin,Admin")]
    public async Task<IActionResult> GetAllActivities()
    {
        var activities = await _activityService.GetAllActivitiesAsync();
        return Ok(activities);
    }

    [HttpPost("log")]
    [AllowAnonymous]
    public async Task<IActionResult> LogActivity([FromBody] ActivityDto dto)
    {
        // Get user details from JWT claims if available, else from DTO
        var email = User.Identity?.IsAuthenticated == true 
            ? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
            : dto.UserEmail;
            
        var name = User.Identity?.IsAuthenticated == true 
            ? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.FindFirst("FullName")?.Value 
            : dto.UserFullName;

        await _activityService.LogActivityAsync(dto.ActionType, dto.Details, email ?? "Guest", name ?? "Guest");
        return Ok();
    }
}

public class ActivityDto
{
    public string ActionType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }
}
