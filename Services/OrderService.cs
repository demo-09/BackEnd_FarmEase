using backEnd.Data;
using backEnd.DTOs;
using backEnd.Interfaces;
using backEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace backEnd.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(o => new OrderDto
        {
            Id = o.Id,
            TotalAmount = o.TotalAmount,
            OrderDate = o.OrderDate,
            Status = o.Status,
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList()
        });
    }

    public async Task<OrderDto?> GetOrderByIdAsync(long orderId, string userId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null) return null;

        return new OrderDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList()
        };
    }

    public async Task<OrderDto> CreateOrderFromCartAsync(string userId)
    {
        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            throw new InvalidOperationException("Cart is empty.");

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = "Completed", // For demo purposes, we automatically complete it
            TotalAmount = cartItems.Sum(c => c.Price * c.Quantity),
            Items = cartItems.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                ProductName = c.ProductName,
                Price = c.Price,
                Quantity = c.Quantity,
                ImageUrl = c.ImageUrl
            }).ToList()
        };

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cartItems); // Empty cart after placing order
        await _context.SaveChangesAsync();

        return new OrderDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList()
        };
    }

    public async Task<OrderDto> CreateOrderDirectlyAsync(string userId, List<AddToCartDto> items)
    {
        if (!items.Any())
            throw new InvalidOperationException("No items provided.");

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = "Completed",
            TotalAmount = items.Sum(i => i.Price * i.Quantity),
            Items = items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return new OrderDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList()
        };
    }
}
