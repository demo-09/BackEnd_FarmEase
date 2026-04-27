namespace backEnd.DTOs;

public class WishlistItemDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool InStock { get; set; }
    public string? Category { get; set; }
}

public class AddToWishlistDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool InStock { get; set; }
    public string? Category { get; set; }
}
