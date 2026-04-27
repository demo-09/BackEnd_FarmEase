using System.ComponentModel.DataAnnotations;

namespace backEnd.Models;

public class WishlistItem
{
    [Key]
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool InStock { get; set; }
    public string? Category { get; set; }
}
