using backEnd.Data;
using backEnd.DTOs;
using backEnd.Interfaces;
using backEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace backEnd.Services;

public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;

    public WishlistService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WishlistItemDto>> GetWishlistAsync(string userId)
    {
        var items = await _context.WishlistItems
            .Where(w => w.UserId == userId)
            .ToListAsync();

        return items.Select(w => new WishlistItemDto
        {
            Id = w.Id,
            ProductId = w.ProductId,
            ProductName = w.ProductName,
            Price = w.Price,
            ImageUrl = w.ImageUrl,
            InStock = w.InStock,
            Category = w.Category
        });
    }

    public async Task<WishlistItemDto> AddToWishlistAsync(string userId, AddToWishlistDto itemDto)
    {
        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == itemDto.ProductId);

        if (existing != null)
        {
            return new WishlistItemDto
            {
                Id = existing.Id,
                ProductId = existing.ProductId,
                ProductName = existing.ProductName,
                Price = existing.Price,
                ImageUrl = existing.ImageUrl,
                InStock = existing.InStock,
                Category = existing.Category
            };
        }

        var newItem = new WishlistItem
        {
            UserId = userId,
            ProductId = itemDto.ProductId,
            ProductName = itemDto.ProductName,
            Price = itemDto.Price,
            ImageUrl = itemDto.ImageUrl,
            InStock = itemDto.InStock,
            Category = itemDto.Category
        };

        _context.WishlistItems.Add(newItem);
        await _context.SaveChangesAsync();

        return new WishlistItemDto
        {
            Id = newItem.Id,
            ProductId = newItem.ProductId,
            ProductName = newItem.ProductName,
            Price = newItem.Price,
            ImageUrl = newItem.ImageUrl,
            InStock = newItem.InStock,
            Category = newItem.Category
        };
    }

    public async Task RemoveFromWishlistAsync(long id, string userId)
    {
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (item != null)
        {
            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearWishlistAsync(string userId)
    {
        var items = await _context.WishlistItems.Where(w => w.UserId == userId).ToListAsync();
        if (items.Any())
        {
            _context.WishlistItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
