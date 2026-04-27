using System.ComponentModel.DataAnnotations;

namespace backEnd.Models;

public class Machinery
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Image { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
