using backEnd.DTOs;

namespace backEnd.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<AuthResponseDto?> GoogleLoginAsync(GoogleLoginDto dto);
    
    Task<string?> GenerateOtpAsync(SendOtpDto dto);
    Task<AuthResponseDto?> VerifyOtpLoginAsync(VerifyOtpDto dto);
}
