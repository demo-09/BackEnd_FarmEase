using System.Security.Claims;
using backEnd.DTOs;
using backEnd.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _orderService.GetUserOrdersAsync(GetUserId());
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(long id)
    {
        var order = await _orderService.GetOrderByIdAsync(id, GetUserId());
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
            var userId = GetUserId();
            if (userId == "guest") return Unauthorized("Please log in to place an order.");

            if (dto.CheckoutFromCart)
            {
                var order = await _orderService.CreateOrderFromCartAsync(userId, dto);
                return Ok(order);
            }
            
            if (dto.Items != null && dto.Items.Any())
            {
                var order = await _orderService.CreateOrderDirectlyAsync(userId, dto);
                return Ok(order);
            }

            return BadRequest("Please provide items or checkout from cart.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("all")]
    [Authorize(Roles = "admin,Admin")]
    public async Task<IActionResult> GetOrdersAdmin()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }
}
