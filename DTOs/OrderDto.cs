namespace backEnd.DTOs;

public class OrderDto
{
    public long Id { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string ProductType { get; set; } = string.Empty;
}

public class CreateOrderDto
{
    // The cart items will typically be converted to an order on the backend.
    // Or we provide the cart item IDs or simply checkout the entire cart.
    // For simplicity, we can pass a list of items to order if we are checking out directly,
    // or just tell the backend to checkout the user's entire cart.
    public bool CheckoutFromCart { get; set; } = true;
}
