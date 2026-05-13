using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backEnd.Data;
using backEnd.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using Microsoft.AspNetCore.SignalR;
using backEnd.Hubs;

namespace backEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgriItemsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<StockHub> _stockHub;

    public AgriItemsController(AppDbContext context, IHubContext<StockHub> stockHub)
    {
        _context = context;
        _stockHub = stockHub;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgriItem>>> GetAgriItems()
    {
        return await _context.AgriItems.ToListAsync();
    }

    [HttpGet("my-items")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AgriItem>>> GetMyItems()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        return await _context.AgriItems
            .Where(i => i.OwnerEmail == email)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "farmer,Farmer,admin,Admin")]
    public async Task<ActionResult<AgriItem>> CreateAgriItem(AgriItem item)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(email))
        {
            item.OwnerEmail = email;
        }

        _context.AgriItems.Add(item);
        await _context.SaveChangesAsync();

        // Notify Hub
        await _stockHub.Clients.All.SendAsync("ReceiveAbsoluteStock", item.Id, item.Quantity, "AgriItem");

        return CreatedAtAction(nameof(GetAgriItems), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateAgriItem(long id, AgriItem item)
    {
        if (id != item.Id) return BadRequest();

        var existing = await _context.AgriItems.FindAsync(id);
        if (existing == null) return NotFound();

        // Check ownership
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (existing.OwnerEmail != email && !User.IsInRole("admin") && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync();

        // Notify Hub
        await _stockHub.Clients.All.SendAsync("ReceiveAbsoluteStock", existing.Id, existing.Quantity, "AgriItem");

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteAgriItem(long id)
    {
        var item = await _context.AgriItems.FindAsync(id);
        if (item == null) return NotFound();

        // Check ownership
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (item.OwnerEmail != email && !User.IsInRole("admin") && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        _context.AgriItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
