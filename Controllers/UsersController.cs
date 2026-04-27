using Microsoft.AspNetCore.Mvc;
using backEnd.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace backEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] backEnd.DTOs.UserDto dto)
    {
        var success = await _userService.UpdateUserAsync(id, dto);
        if (!success) return NotFound(new { message = "User not found" });

        return Ok(new { message = "User updated successfully" });
    }

    [HttpDelete("{email}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteUser(string email)
    {
        var success = await _userService.DeleteUserByEmailAsync(email);
        if (!success) return NotFound();

        return NoContent();
    }
}
