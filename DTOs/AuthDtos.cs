namespace backEnd.DTOs;

public class RegisterDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? BirthDate { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string JoinedDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string? Avatar { get; set; }

}

public class LoginDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class GoogleLoginDto
{
    public required string IdToken { get; set; }
    public string Role { get; set; } = "customer";
}

public class AuthResponseDto
{
    public required string Token { get; set; }
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required string Role { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? BirthDate { get; set; }
    public string? Bio { get; set; }
    public string? Avatar { get; set; }
    public required string JoinedDate { get; set; }
}

public class InitiateLoginDto
{
    public required string EmailOrPhone { get; set; }
    public required string Password { get; set; }
}

public class VerifyOtpDto
{
    public required string EmailOrPhone { get; set; }
    public required string OtpCode { get; set; }
}
