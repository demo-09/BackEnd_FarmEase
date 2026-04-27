using System.Security.Claims;
using backEnd.DTOs;
using backEnd.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _cartService.GetCartAsync(GetUserId());
        return Ok(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        var item = await _cartService.AddToCartAsync(GetUserId(), dto);
        return Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuantity(long id, [FromBody] UpdateCartQtyDto dto)
    {
        var item = await _cartService.UpdateQuantityAsync(id, GetUserId(), dto.Quantity);
        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromCart(long id)
    {
        await _cartService.RemoveFromCartAsync(id, GetUserId());
        return NoContent();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync(GetUserId());
        return NoContent();
    }
}
