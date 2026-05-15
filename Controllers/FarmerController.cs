using backEnd.Interfaces;
using backEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backEnd.Controllers;

[Authorize(Roles = "farmer,Farmer,admin,Admin")]
[ApiController]
[Route("api/[controller]")]
public class FarmerController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly AppDbContext _context;

    public FarmerController(IOrderService orderService, AppDbContext context)
    {
        _orderService = orderService;
        _context = context;
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetMySales()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email)) return Unauthorized("Email claim missing from token.");
        var sales = await _orderService.GetFarmerOrdersAsync(email);
        Console.WriteLine(email);
        return Ok(sales);
    }

    [HttpGet("listings")]
    public async Task<IActionResult> GetMyListings()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email)) return Unauthorized("Email claim missing from token.");

        var machineries = await _context.Machineries
            .Where(m => m.OwnerEmail == email)
            .ToListAsync();

        var agriItems = await _context.AgriItems
            .Where(a => a.OwnerEmail == email)
            .ToListAsync();

        var listings = new
        {
            Machinery = machineries,
            AgriItems = agriItems
        };

        return Ok(listings);
    }
}
