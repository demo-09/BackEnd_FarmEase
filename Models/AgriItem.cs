using System.ComponentModel.DataAnnotations;

namespace backEnd.Models;

public class AgriItem
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = "admin@farmease.com";
}
