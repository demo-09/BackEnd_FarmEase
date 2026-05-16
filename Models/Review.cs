using System.ComponentModel.DataAnnotations;

namespace backEnd.Models;

public class Review
{
    [Key]
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ProductType { get; set; } = "Machinery"; // "Machinery" or "AgriItem"
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
