using backEnd.Data;
using backEnd.DTOs;
using backEnd.Interfaces;
using backEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace backEnd.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CartItemDto>> GetCartAsync(string userId)
    {
        var items = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return items.Select(c => new CartItemDto
        {
            Id = c.Id,
            ProductId = c.ProductId,
            ProductName = c.ProductName,
            Price = c.Price,
            Quantity = c.Quantity,
            ImageUrl = c.ImageUrl,
            Category = c.Category
        });
    }

    public async Task<CartItemDto> AddToCartAsync(string userId, AddToCartDto itemDto)
    {
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == itemDto.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += itemDto.Quantity;
            await _context.SaveChangesAsync();
            return new CartItemDto
            {
                Id = existingItem.Id,
                ProductId = existingItem.ProductId,
                ProductName = existingItem.ProductName,
                Price = existingItem.Price,
                Quantity = existingItem.Quantity,
                ImageUrl = existingItem.ImageUrl,
                Category = existingItem.Category
            };
        }

        var newItem = new CartItem
        {
            UserId = userId,
            ProductId = itemDto.ProductId,
            ProductName = itemDto.ProductName,
            Price = itemDto.Price,
            Quantity = itemDto.Quantity,
            ImageUrl = itemDto.ImageUrl,
            Category = itemDto.Category
        };

        _context.CartItems.Add(newItem);
        await _context.SaveChangesAsync();

        return new CartItemDto
        {
            Id = newItem.Id,
            ProductId = newItem.ProductId,
            ProductName = newItem.ProductName,
            Price = newItem.Price,
            Quantity = newItem.Quantity,
            ImageUrl = newItem.ImageUrl,
            Category = newItem.Category
        };
    }

    public async Task<CartItemDto> UpdateQuantityAsync(long id, string userId, int quantity)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
        if (item == null) throw new KeyNotFoundException("Cart item not found.");
        
        item.Quantity = quantity;
        await _context.SaveChangesAsync();
        
        return new CartItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Price = item.Price,
            Quantity = item.Quantity,
            ImageUrl = item.ImageUrl,
            Category = item.Category
        };
    }

    public async Task RemoveFromCartAsync(long id, string userId)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(string userId)
    {
        var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
        if (items.Any())
        {
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
