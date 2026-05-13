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
    private readonly INotificationService _notificationService;
    private readonly IActivityService _activityService;

    // Static dictionary to mock OTP storage (Email/Phone -> OTP)
    private static readonly ConcurrentDictionary<string, string> _otpStore = new();
    
    // Static dictionary to cache unverified users during signup (Email -> RegisterDto)
    private static readonly ConcurrentDictionary<string, RegisterDto> _unverifiedUsers = new();

    public AuthService(IAuthRepository repo, JwtHelper jwtHelper, IMapper mapper, INotificationService notificationService, IActivityService activityService)
    {
        _repo      = repo;
        _jwtHelper = jwtHelper;
        _mapper    = mapper;
        _notificationService = notificationService;
        _activityService = activityService;
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
            Avatar = dto.Avatar,
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
            Avatar = newUser.Avatar,
            JoinedDate = newUser.JoinedDate
        };
    }

    public async Task<string?> InitiateRegistrationAsync(RegisterDto dto)
    {
        if (await _repo.EmailExistsAsync(dto.Email))
        {
            return null; // Email already in use
        }

        // Cache the registration info
        _unverifiedUsers[dto.Email] = dto;

        // Generate a 6-digit code.
        var code = new Random().Next(100000, 999999).ToString();
        _otpStore[dto.Email] = code;

        // Send OTP
        try 
        {
            if (dto.Email.Contains("@"))
            {
                await _notificationService.SendEmailAsync(
                    dto.Email, 
                    "Verify Your FarmEase Account", 
                    $"Your FarmEase verification code is: <b>{code}</b>. It is valid for 5 minutes.");
            }
            else
            {
                await _notificationService.SendSmsAsync(dto.Email, $"Your FarmEase verification code is: {code}");
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't crash - allow the user to see the mock OTP in console for now
            Console.WriteLine($"NOTIFICATION ERROR: {ex.Message}");
        }

        return code;
    }

    public async Task<AuthResponseDto?> VerifyOtpRegistrationAsync(VerifyOtpDto dto)
    {
        if (_otpStore.TryGetValue(dto.EmailOrPhone, out var storedCode))
        {
            if (storedCode == dto.OtpCode)
            {
                // OTP matches! Clear it.
                _otpStore.TryRemove(dto.EmailOrPhone, out _);

                // Retrieve the cached registration info
                if (_unverifiedUsers.TryGetValue(dto.EmailOrPhone, out var registerDto))
                {
                    _unverifiedUsers.TryRemove(dto.EmailOrPhone, out _);

                    // Create the user in the database
                    var user = new User
                    {
                        FullName = registerDto.FullName,
                        Email = registerDto.Email,
                        Phone = registerDto.Phone,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                        Role = registerDto.Role ?? "customer",
                        Avatar = registerDto.Avatar,
                        Bio = registerDto.Bio,
                        Address = registerDto.Address,
                        JoinedDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
                    };

                    await _repo.CreateAsync(user);

                    await _activityService.LogActivityAsync("Signup", $"New user registered: {user.FullName} ({user.Role})", user.Email, user.FullName);

                    var token = _jwtHelper.GenerateToken(user);
                    return new AuthResponseDto
                    {
                        Token = token,
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        Role = user.Role,
                        Phone = user.Phone,
                        Address = user.Address,
                        BirthDate = user.BirthDate,
                        Bio = user.Bio,
                        Avatar = user.Avatar,
                        JoinedDate = user.JoinedDate
                    };
                }
            }
        }
        return null;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _repo.GetByEmailAsync(dto.Email);

        bool passwordValid = false;
        try 
        {
            passwordValid = user != null && !string.IsNullOrEmpty(user.PasswordHash) && BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        }
        catch 
        {
            passwordValid = false; 
        }

        if (user == null || !passwordValid)
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
            Avatar     = user.Avatar,
            JoinedDate = user.JoinedDate
        };
    }

    public async Task<AuthResponseDto?> GoogleLoginAsync(GoogleLoginDto dto)
    {
        try
        {
            Console.WriteLine("[GOOGLE AUTH]: Validating token...");
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { "360775516641-oeppopoi7lbfues9mfnvcreciuin7u97.apps.googleusercontent.com" }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
            Console.WriteLine($"[GOOGLE AUTH]: Token valid for {payload.Email}");
            
            var user = await _repo.GetByEmailAsync(payload.Email);

            if (user == null)
            {
                Console.WriteLine($"[GOOGLE AUTH]: Creating new user for {payload.Email}");
                // Register the user
                var allowedRoles = new[] { "farmer", "customer" };
                var role = allowedRoles.Contains(dto.Role?.ToLower()) ? dto.Role.ToLower() : "customer";

                user = new User
                {
                    FullName = payload.Name ?? payload.GivenName ?? payload.Email,
                    Email = payload.Email,
                    PasswordHash = "GOOGLE_USER_" + Guid.NewGuid().ToString("N"), // Safe non-empty hash
                    Role = role,
                    Phone = "",
                    Address = "",
                    BirthDate = "",
                    Bio = "Registered via Google",
                    Avatar = payload.Picture ?? dto.Avatar ?? $"https://ui-avatars.com/api/?name={payload.Name}&background=random",
                    JoinedDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
                };

                await _repo.CreateAsync(user);
                await _activityService.LogActivityAsync("Signup", $"User signed up via Google: {user.FullName}", user.Email, user.FullName);
            }
            else
            {
                Console.WriteLine($"[GOOGLE AUTH]: User {payload.Email} found. Logging in...");
                await _activityService.LogActivityAsync("Login", $"User logged in via Google: {user.FullName}", user.Email, user.FullName);
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
                Avatar     = user.Avatar,
                JoinedDate = user.JoinedDate
            };
        }
        catch (InvalidJwtException ex)
        {
            Console.WriteLine($"[GOOGLE AUTH ERROR] Invalid Token: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GOOGLE AUTH ERROR] Unexpected: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }

    public async Task<string?> InitiateLoginAsync(InitiateLoginDto dto)
    {
        // Check if user exists by email or phone
        var user = await _repo.GetByEmailOrPhoneAsync(dto.EmailOrPhone);

        bool passwordValid = false;
        try 
        {
            passwordValid = user != null && !string.IsNullOrEmpty(user.PasswordHash) && BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        }
        catch 
        {
            passwordValid = false; 
        }

        if (user == null || !passwordValid)
        {
            // Invalid credentials, don't generate OTP
            return null;
        }

        // Generate a 6-digit code.
        var code = new Random().Next(100000, 999999).ToString();
        
        _otpStore[dto.EmailOrPhone] = code;

        // Send OTP
        try 
        {
            if (dto.EmailOrPhone.Contains("@"))
            {
                await _notificationService.SendEmailAsync(
                    dto.EmailOrPhone, 
                    "Your FarmEase Login OTP", 
                    $"Your OTP for FarmEase is: <b>{code}</b>. It is valid for 5 minutes.");
            }
            else
            {
                await _notificationService.SendSmsAsync(dto.EmailOrPhone, $"Your FarmEase OTP is: {code}");
            }
        }
        catch (Exception ex)
        {
            // Log error but continue so the API doesn't return 500
            Console.WriteLine($"NOTIFICATION ERROR: {ex.Message}");
        }

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

                // Find user by email or phone. We know they exist because we verified them in InitiateLoginAsync.
                var user = await _repo.GetByEmailOrPhoneAsync(dto.EmailOrPhone);

                if (user != null)
                {
                    await _activityService.LogActivityAsync("Login", $"User logged in via OTP: {user.FullName}", user.Email, user.FullName);

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
                        Avatar = user.Avatar,
                        JoinedDate = user.JoinedDate
                    };
                }
            }
        }

        return null;
    }
}
