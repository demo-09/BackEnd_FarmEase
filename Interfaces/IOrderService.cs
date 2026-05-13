using backEnd.DTOs;

namespace backEnd.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
    Task<OrderDto?> GetOrderByIdAsync(long orderId, string userId);
    Task<OrderDto> CreateOrderFromCartAsync(string userId, CreateOrderDto dto);
    Task<OrderDto> CreateOrderDirectlyAsync(string userId, CreateOrderDto dto);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<IEnumerable<OrderItemDto>> GetFarmerOrdersAsync(string farmerEmail);
}
