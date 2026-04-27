using backEnd.DTOs;
using backEnd.Models;

namespace backEnd.Interfaces;

public interface ICartService
{
    Task<IEnumerable<CartItemDto>> GetCartAsync(string userId);
    Task<CartItemDto> AddToCartAsync(string userId, AddToCartDto itemDto);
    Task<CartItemDto> UpdateQuantityAsync(long id, string userId, int quantity);
    Task RemoveFromCartAsync(long id, string userId);
    Task ClearCartAsync(string userId);
}
