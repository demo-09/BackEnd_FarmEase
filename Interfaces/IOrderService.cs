using backEnd.DTOs;

namespace backEnd.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
    Task<OrderDto?> GetOrderByIdAsync(long orderId, string userId);
    Task<OrderDto> CreateOrderFromCartAsync(string userId);
    Task<OrderDto> CreateOrderDirectlyAsync(string userId, List<AddToCartDto> items);
}
