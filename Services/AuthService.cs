using backEnd.Interfaces;
using backEnd.Models;
using backEnd.DTOs;
using backEnd.Helpers;
using backEnd.Repositories;
using AutoMapper;
using Google.Apis.Auth;
using System.Collections.Concurrent;

namespace backEnd.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _repo;
    private readonly JwtHelper _jwtHelper;
    private readonly IMapper _mapper;

    // Static dictionary to mock OTP storage (Email/Phone -> OTP)
    private static readonly ConcurrentDictionary<string, string> _otpStore = new();

    public AuthService(IAuthRepository repo, JwtHelper jwtHelper, IMapper mapper)
    {
        _repo      = repo;
        _jwtHelper = jwtHelper;
        _mapper    = mapper;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _repo.EmailExistsAsync(dto.Email))
            return null;

        // ?? Restrict role
        var allowedRoles = new[] { "farmer", "customer" };
        var role = allowedRoles.Contains(dto.Role.ToLower()) ? dto.Role.ToLower() : "customer";

        var newUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role,

            // ? FIXED (Important)
            Phone = dto.Phone,
            Address = dto.Address,
            BirthDate = dto.BirthDate,
            Bio = dto.Bio,

            JoinedDate = DateTime.Now.ToString("yyyy-MM-dd")
        };

        await _repo.CreateAsync(newUser);

        return new AuthResponseDto
        {
            Token = _jwtHelper.GenerateToken(newUser),
            Id = newUser.Id,
            Email = newUser.Email,
            FullName = newUser.FullName,
            Role = newUser.Role,
            Phone = newUser.Phone,
            Address = newUser.Address,
            BirthDate = newUser.BirthDate,
            Bio = newUser.Bio,
            JoinedDate = newUser.JoinedDate
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _repo.GetByEmailAsync(dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return new AuthResponseDto
        {
            Token      = _jwtHelper.GenerateToken(user),
            Id         = user.Id,
            Email      = user.Email,
            FullName   = user.FullName,
            Role       = user.Role,
            Phone      = user.Phone,
            Address    = user.Address,
            BirthDate  = user.BirthDate,
            Bio        = user.Bio,
            JoinedDate = user.JoinedDate
        };
    }

    public async Task<AuthResponseDto?> GoogleLoginAsync(GoogleLoginDto dto)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { "360775516641-oeppopoi7lbfues9mfnvcreciuin7u97.apps.googleusercontent.com" }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
            
            var user = await _repo.GetByEmailAsync(payload.Email);

            if (user == null)
            {
                // Register the user
                var allowedRoles = new[] { "farmer", "customer" };
                var role = allowedRoles.Contains(dto.Role.ToLower()) ? dto.Role.ToLower() : "customer";

                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FullName = payload.Name ?? payload.Email,
                    Email = payload.Email,
                    PasswordHash = "", // Empty password for Google users
                    Role = role,
                    Phone = "",
                    Address = "",
                    BirthDate = "",
                    Bio = "Registered via Google",
                    Avatar = payload.Picture,
                    JoinedDate = DateTime.Now.ToString("yyyy-MM-dd")
                };

                await _repo.CreateAsync(user);
            }

            return new AuthResponseDto
            {
                Token      = _jwtHelper.GenerateToken(user),
                Id         = user.Id,
                Email      = user.Email,
                FullName   = user.FullName,
                Role       = user.Role,
                Phone      = user.Phone,
                Address    = user.Address,
                BirthDate  = user.BirthDate,
                Bio        = user.Bio,
                JoinedDate = user.JoinedDate
            };
        }
        catch (InvalidJwtException)
        {
            // Token is invalid
            return null;
        }
    }

    public async Task<string?> GenerateOtpAsync(SendOtpDto dto)
    {
        // For simplicity, checking if user exists by email or phone.
        // Assuming GetByEmailAsync can be extended to check both, 
        // or we just query by email since phone isn't unique indexed currently.
        // Actually, let's allow generating OTP for any email/phone just for the prototype,
        // or we can strictly check by email.
        
        // Since prototype "login with number and email", let's assume if it's an email, we find by email.
        // We will just generate a 6 digit code.
        var code = new Random().Next(100000, 999999).ToString();
        
        _otpStore[dto.EmailOrPhone] = code;

        return code;
    }

    public async Task<AuthResponseDto?> VerifyOtpLoginAsync(VerifyOtpDto dto)
    {
        if (_otpStore.TryGetValue(dto.EmailOrPhone, out var storedCode))
        {
            if (storedCode == dto.OtpCode)
            {
                // OTP matches! Clear it.
                _otpStore.TryRemove(dto.EmailOrPhone, out _);

                // Find user by email or phone. If doesn't exist, we should ideally register them.
                // For this prototype, let's try getting by email first.
                var user = await _repo.GetByEmailAsync(dto.EmailOrPhone);

                if (user == null)
                {
                    // If they logged in with OTP but don't exist, auto-register them like Google Login
                    user = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        FullName = "User " + dto.EmailOrPhone,
                        Email = dto.EmailOrPhone.Contains("@") ? dto.EmailOrPhone : dto.EmailOrPhone + "@mock.com",
                        PasswordHash = "",
                        Role = "customer",
                        Phone = dto.EmailOrPhone.Contains("@") ? "" : dto.EmailOrPhone,
                        Address = "",
                        BirthDate = "",
                        Bio = "Registered via OTP",
                        JoinedDate = DateTime.Now.ToString("yyyy-MM-dd")
                    };
                    await _repo.CreateAsync(user);
                }

                return new AuthResponseDto
                {
                    Token = _jwtHelper.GenerateToken(user),
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    Phone = user.Phone,
                    Address = user.Address,
                    BirthDate = user.BirthDate,
                    Bio = user.Bio,
                    JoinedDate = user.JoinedDate
                };
            }
        }

        return null;
    }
}
