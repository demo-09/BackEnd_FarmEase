using System.Security.Claims;
using backEnd.DTOs;
using backEnd.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var items = await _wishlistService.GetWishlistAsync(GetUserId());
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistDto dto)
    {
        var item = await _wishlistService.AddToWishlistAsync(GetUserId(), dto);
        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromWishlist(long id)
    {
        await _wishlistService.RemoveFromWishlistAsync(id, GetUserId());
        return NoContent();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearWishlist()
    {
        await _wishlistService.ClearWishlistAsync(GetUserId());
        return NoContent();
    }
}
