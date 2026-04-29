namespace backEnd.DTOs;

public class RegisterDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
    public required string Phone { get; set; }
    public required string Address { get; set; }
    public required string BirthDate { get; set; }
    public required string Bio { get; set; }// "admin", "farmer", "customer"
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
    public required string Phone { get; set; }
    public required string Address { get; set; }
    public required string BirthDate { get; set; }
    public required string Bio { get; set; }
    public required string JoinedDate { get; set; }
}

public class SendOtpDto
{
    public required string EmailOrPhone { get; set; }
}

public class VerifyOtpDto
{
    public required string EmailOrPhone { get; set; }
    public required string OtpCode { get; set; }
}
