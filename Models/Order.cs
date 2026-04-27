using System.ComponentModel.DataAnnotations;

namespace backEnd.Models;

public class Order
{
    [Key]
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
    
    public List<OrderItem> Items { get; set; } = new();
}
