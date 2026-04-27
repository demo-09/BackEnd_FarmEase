using System.ComponentModel.DataAnnotations;

namespace backEnd.Models;

public class CartItem
{
    [Key]
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string ProductType { get; set; } = string.Empty;
}
