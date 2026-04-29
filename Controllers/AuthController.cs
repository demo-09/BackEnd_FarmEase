using Microsoft.AspNetCore.Mvc;
using backEnd.Interfaces;
using backEnd.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace backEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (result == null) return BadRequest(new { message = "Email already exists." });
        
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result == null) return Unauthorized(new { message = "Invalid email or password." });

        return Ok(result);
    }

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        var result = await _authService.GoogleLoginAsync(dto);
        if (result == null) return Unauthorized(new { message = "Invalid Google token." });

        return Ok(result);
    }

    [HttpPost("send-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
    {
        var code = await _authService.GenerateOtpAsync(dto);
        return Ok(new { message = "OTP generated successfully", mockOtp = code });
    }

    [HttpPost("verify-otp-login")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtpLogin([FromBody] VerifyOtpDto dto)
    {
        var result = await _authService.VerifyOtpLoginAsync(dto);
        if (result == null) return Unauthorized(new { message = "Invalid OTP." });

        return Ok(result);
    }
}
