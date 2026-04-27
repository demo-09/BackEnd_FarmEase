namespace backEnd.DTOs;

public class CartItemDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string ProductType { get; set; } = string.Empty;
}

public class AddToCartDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string ProductType { get; set; } = string.Empty;
}

public class UpdateCartQtyDto
{
    public int Quantity { get; set; }
}
