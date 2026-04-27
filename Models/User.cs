using System.ComponentModel.DataAnnotations;

namespace backEnd.Models;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string BirthDate { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // e.g. "admin", "farmer", "customer"
    public string JoinedDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string? Avatar { get; set; }
    
    // JSON arrays for storing user items simply without complex relations 
    public string WishlistJson { get; set; } = "[]";
    public string CartJson { get; set; } = "[]";
}
