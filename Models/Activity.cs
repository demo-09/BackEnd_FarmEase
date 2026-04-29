using System.ComponentModel.DataAnnotations;

namespace backEnd.Models;

public class Activity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ActionType { get; set; } = string.Empty; // e.g. "Login", "Signup", "Order"
    public string Details { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
