using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backEnd.Models;

public class OrderItem
{
    [Key]
    public long Id { get; set; }
    public long OrderId { get; set; }
    [JsonIgnore]
    public Order? Order { get; set; }
    
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string ProductType { get; set; } = string.Empty;
}
