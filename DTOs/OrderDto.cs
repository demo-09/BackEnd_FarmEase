namespace backEnd.DTOs;

public class OrderDto
{
    public long Id { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RequesterName { get; set; }
    public string? RequesterEmail { get; set; }
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
    public long? OrderId { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string? Category { get; set; }
    public int StockLeft { get; set; }
}

public class CreateOrderDto
{
    public bool CheckoutFromCart { get; set; } = true;
    public string TransactionId { get; set; } = "COD";
    public string ShippingAddress { get; set; } = string.Empty;
    public List<AddToCartDto>? Items { get; set; } // For direct checkout
}
