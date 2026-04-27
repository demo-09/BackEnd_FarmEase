using backEnd.DTOs;

namespace backEnd.Interfaces;

public interface IWishlistService
{
    Task<IEnumerable<WishlistItemDto>> GetWishlistAsync(string userId);
    Task<WishlistItemDto> AddToWishlistAsync(string userId, AddToWishlistDto itemDto);
    Task RemoveFromWishlistAsync(long id, string userId);
    Task ClearWishlistAsync(string userId);
}
