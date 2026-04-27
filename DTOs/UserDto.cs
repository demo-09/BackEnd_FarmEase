namespace backEnd.DTOs;

public class UserDto
{
    public required string Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }

    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? BirthDate { get; set; }
    public string? Bio { get; set; }
    public string? JoinedDate { get; set; }
    public string? Avatar { get; set; }
    public string? WishlistJson { get; set; }
    public string? CartJson { get; set; }
}
