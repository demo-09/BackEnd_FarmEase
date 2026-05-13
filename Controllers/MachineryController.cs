using Microsoft.AspNetCore.Mvc;
using backEnd.Interfaces;
using backEnd.Models;
using backEnd.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;

namespace backEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachineryController : ControllerBase
{
    private readonly IMachineryService _machineryService;

    public MachineryController(IMachineryService machineryService)
    {
        _machineryService = machineryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllMachinery()
    {
        var items = await _machineryService.GetAllMachineryAsync();
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "admin,Admin,farmer,Farmer")]
    public async Task<IActionResult> CreateMachinery([FromBody] MachineryDto machineryDto)
    {
        // Extract owner email from JWT
        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var machinery = _machineryService.MapToModel(machineryDto); // We'll add this helper or use mapper directly
        if (!string.IsNullOrEmpty(userEmail))
        {
            machinery.OwnerEmail = userEmail;
        }

        var created = await _machineryService.CreateMachineryAsync(machinery);
        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,Admin")]
    public async Task<IActionResult> UpdateMachinery(long id, [FromBody] MachineryDto dto)
    {
        var updated = await _machineryService.UpdateMachineryAsync(id, dto);
        if (updated == null) return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,Admin")]
    public async Task<IActionResult> DeleteMachinery(long id)
    {
        var success = await _machineryService.DeleteMachineryAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }

    [HttpPost("reset")]
    [Authorize(Roles = "admin,Admin")]
    public async Task<IActionResult> ResetInventory()
    {
        await _machineryService.ResetInventoryAsync();
        return Ok(new { message = "Inventory reset to default values successfully." });
    }
}
