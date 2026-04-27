using Microsoft.AspNetCore.Mvc;
using backEnd.Interfaces;
using backEnd.Models;
using backEnd.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;

namespace backEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessagesService _messagesService;
    private readonly IUserService _userService;

    public MessagesController(IMessagesService messagesService, IUserService userService)
    {
        _messagesService = messagesService;
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto dto)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var msg = await _messagesService.SendMessageAsync(email, dto);
        return Ok(msg);
    }

    [HttpGet("history/{otherEmail}")]
    public async Task<IActionResult> GetHistory(string otherEmail)
    {
        var myEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(myEmail)) return Unauthorized();

        var history = await _messagesService.GetChatHistoryAsync(myEmail, otherEmail);
        return Ok(history);
    }

    [HttpGet("contacts")]
    public async Task<IActionResult> GetContacts()
    {
        var myEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(myEmail)) return Unauthorized();

        var users = await _userService.GetAllUsersAsync();
        
        // Exclude the current user so they don't chat with themselves
        var result = users.Where(u => u.Email != myEmail).ToList();

        return Ok(result.Select(u => new { u.Id, u.FullName, u.Email, u.Role }));
    }
}
