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

    // OTP STORE
    private static readonly ConcurrentDictionary<string, string> _otpStore = new();

    // OTP EXPIRY STORE
    private static readonly ConcurrentDictionary<string, DateTime> _otpExpiryStore = new();

    // CACHE UNVERIFIED USERS
    private static readonly ConcurrentDictionary<string, RegisterDto> _unverifiedUsers = new();

    public AuthService(
        IAuthRepository repo,
        JwtHelper jwtHelper,
        IMapper mapper,
        INotificationService notificationService,
        IActivityService activityService
    )
    {
        _repo = repo;
        _jwtHelper = jwtHelper;
        _mapper = mapper;
        _notificationService = notificationService;
        _activityService = activityService;
    }

    // =========================================================
    // REGISTER
    // =========================================================

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _repo.EmailExistsAsync(dto.Email))
            return null;

        var allowedRoles = new[] { "farmer", "customer" };

        var role =
            allowedRoles.Contains(dto.Role?.ToLower())
            ? dto.Role.ToLower()
            : "customer";

        var newUser = new User
        {
            Id = Guid.NewGuid().ToString(),

            FullName = dto.FullName,

            Email = dto.Email,

            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(dto.Password),

            Role = role,

            Avatar = dto.Avatar,

            Phone = dto.Phone,

            Address = dto.Address,

            BirthDate = dto.BirthDate,

            Bio = dto.Bio,

            JoinedDate =
                DateTime.UtcNow.ToString("yyyy-MM-dd")
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

    // =========================================================
    // INITIATE REGISTER OTP
    // =========================================================

    public async Task<string?> InitiateRegistrationAsync(RegisterDto dto)
    {
        if (await _repo.EmailExistsAsync(dto.Email))
        {
            return null;
        }

        // CACHE USER
        _unverifiedUsers[dto.Email] = dto;

        // GENERATE OTP
        var code =
            new Random()
            .Next(100000, 999999)
            .ToString();

        _otpStore[dto.Email] = code;

        // OTP EXPIRY (5 MINUTES)
        _otpExpiryStore[dto.Email] =
            DateTime.UtcNow.AddMinutes(5);

        try
        {
            if (dto.Email.Contains("@"))
            {
                var body = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #e0e0e0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>

    <div style='background: #16a34a; padding: 30px; text-align: center;'>
        <h1 style='color: white; margin: 0; font-size: 26px;'>
            FarmEase OTP Verification ??
        </h1>
    </div>

    <div style='padding: 30px; line-height: 1.6; color: #333;'>

        <p style='font-size: 18px;'>
            Hello {dto.FullName} ?????
        </p>

        <p>
            Your OTP for FarmEase account verification is:
        </p>

        <div style='background: #f0fdf4; border: 2px dashed #16a34a; padding: 25px; text-align: center; border-radius: 12px; margin: 25px 0;'>

            <h2 style='margin: 0; color: #16a34a; font-size: 42px; letter-spacing: 8px;'>
                {code}
            </h2>

        </div>

        <p>
            This OTP is valid for 
            <strong>5 minutes</strong>.
        </p>

        <p style='color: #ef4444;'>
            ?? Never share this OTP with anyone.
        </p>

        <div style='text-align: center; margin-top: 35px;'>

            <a href='https://front-end-farm-ease.vercel.app/#/login'
               style='background: #16a34a;
                      color: white;
                      padding: 14px 30px;
                      text-decoration: none;
                      border-radius: 30px;
                      font-weight: bold;
                      display: inline-block;'>

                Open FarmEase ??

            </a>

        </div>

    </div>

    <div style='background: #f1f5f9; padding: 20px; text-align: center; font-size: 12px; color: #64748b;'>

        <p style='margin: 0;'>
            FarmEase - Empowering Farmers ??
        </p>

    </div>

</div>";

                await _notificationService.SendEmailAsync(
                    dto.Email,
                    "Verify Your FarmEase Account ??",
                    body
                );

                Console.WriteLine($"REGISTER OTP: {code}");
            }
            else
            {
                await _notificationService.SendSmsAsync(
                    dto.Email,
                    $"Your FarmEase OTP is: {code}"
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NOTIFICATION ERROR: {ex.Message}");
        }

        return code;
    }

    // =========================================================
    // VERIFY REGISTER OTP
    // =========================================================

    public async Task<AuthResponseDto?> VerifyOtpRegistrationAsync(
        VerifyOtpDto dto
    )
    {
        if (_otpStore.TryGetValue(dto.EmailOrPhone, out var storedCode))
        {
            // CHECK EXPIRY
            if (
                _otpExpiryStore.TryGetValue(
                    dto.EmailOrPhone,
                    out var expiryTime
                )
            )
            {
                if (DateTime.UtcNow > expiryTime)
                {
                    _otpStore.TryRemove(dto.EmailOrPhone, out _);

                    _otpExpiryStore.TryRemove(
                        dto.EmailOrPhone,
                        out _
                    );

                    return null;
                }
            }

            // VERIFY OTP
            if (storedCode == dto.OtpCode)
            {
                _otpStore.TryRemove(dto.EmailOrPhone, out _);

                _otpExpiryStore.TryRemove(
                    dto.EmailOrPhone,
                    out _
                );

                if (
                    _unverifiedUsers.TryGetValue(
                        dto.EmailOrPhone,
                        out var registerDto
                    )
                )
                {
                    _unverifiedUsers.TryRemove(
                        dto.EmailOrPhone,
                        out _
                    );

                    var allowedRoles =
                        new[] { "farmer", "customer" };

                    var role =
                        allowedRoles.Contains(
                            registerDto.Role?.ToLower()
                        )
                        ? registerDto.Role.ToLower()
                        : "customer";

                    var user = new User
                    {
                        Id = Guid.NewGuid().ToString(),

                        FullName = registerDto.FullName,

                        Email = registerDto.Email,

                        Phone = registerDto.Phone,

                        PasswordHash =
                            BCrypt.Net.BCrypt.HashPassword(
                                registerDto.Password
                            ),

                        Role = role,

                        Avatar = registerDto.Avatar,

                        Bio = registerDto.Bio,

                        Address = registerDto.Address,

                        BirthDate = registerDto.BirthDate,

                        JoinedDate =
                            DateTime.UtcNow.ToString("yyyy-MM-dd")
                    };

                    await _repo.CreateAsync(user);

                    await _activityService.LogActivityAsync(
                        "Signup",
                        $"New user registered: {user.FullName}",
                        user.Email,
                        user.FullName
                    );

                    return new AuthResponseDto
                    {
                        Token =
                            _jwtHelper.GenerateToken(user),

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

    // =========================================================
    // LOGIN
    // =========================================================

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user =
            await _repo.GetByEmailAsync(dto.Email);

        bool passwordValid = false;

        try
        {
            passwordValid =
                user != null &&
                !string.IsNullOrEmpty(user.PasswordHash) &&
                BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    user.PasswordHash
                );
        }
        catch
        {
            passwordValid = false;
        }

        if (user == null || !passwordValid)
            return null;

        return new AuthResponseDto
        {
            Token =
                _jwtHelper.GenerateToken(user),

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

    // =========================================================
    // GOOGLE LOGIN
    // =========================================================

    public async Task<AuthResponseDto?> GoogleLoginAsync(
        GoogleLoginDto dto
    )
    {
        try
        {
            var settings =
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[]
                    {
                        "360775516641-oeppopoi7lbfues9mfnvcreciuin7u97.apps.googleusercontent.com"
                    }
                };

            var payload =
                await GoogleJsonWebSignature.ValidateAsync(
                    dto.IdToken,
                    settings
                );

            var user =
                await _repo.GetByEmailAsync(payload.Email);

            if (user == null)
            {
                var allowedRoles =
                    new[] { "farmer", "customer" };

                var role =
                    allowedRoles.Contains(dto.Role?.ToLower())
                    ? dto.Role.ToLower()
                    : "customer";

                user = new User
                {
                    Id = Guid.NewGuid().ToString(),

                    FullName =
                        payload.Name ??
                        payload.GivenName ??
                        payload.Email,

                    Email = payload.Email,

                    PasswordHash =
                        "GOOGLE_USER_" +
                        Guid.NewGuid().ToString("N"),

                    Role = role,

                    Phone = "",

                    Address = "",

                    BirthDate = "",

                    Bio = "Registered via Google",

                    Avatar =
                        payload.Picture ??
                        dto.Avatar ??
                        $"https://ui-avatars.com/api/?name={payload.Name}&background=random",

                    JoinedDate =
                        DateTime.UtcNow.ToString("yyyy-MM-dd")
                };

                await _repo.CreateAsync(user);

                await _activityService.LogActivityAsync(
                    "Signup",
                    $"User signed up via Google: {user.FullName}",
                    user.Email,
                    user.FullName
                );
            }
            else
            {
                await _activityService.LogActivityAsync(
                    "Login",
                    $"User logged in via Google: {user.FullName}",
                    user.Email,
                    user.FullName
                );
            }

            return new AuthResponseDto
            {
                Token =
                    _jwtHelper.GenerateToken(user),

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
        catch (Exception ex)
        {
            Console.WriteLine($"GOOGLE LOGIN ERROR: {ex.Message}");
            return null;
        }
    }

    // =========================================================
    // INITIATE LOGIN OTP
    // =========================================================

    public async Task<string?> InitiateLoginAsync(
        InitiateLoginDto dto
    )
    {
        var user =
            await _repo.GetByEmailOrPhoneAsync(
                dto.EmailOrPhone
            );

        bool passwordValid = false;

        try
        {
            passwordValid =
                user != null &&
                !string.IsNullOrEmpty(user.PasswordHash) &&
                BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    user.PasswordHash
                );
        }
        catch
        {
            passwordValid = false;
        }

        if (user == null || !passwordValid)
        {
            return null;
        }

        var code =
            new Random()
            .Next(100000, 999999)
            .ToString();

        _otpStore[dto.EmailOrPhone] = code;

        _otpExpiryStore[dto.EmailOrPhone] =
            DateTime.UtcNow.AddMinutes(5);

        try
        {
            if (dto.EmailOrPhone.Contains("@"))
            {
                var body = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border-radius: 12px; overflow: hidden; border: 1px solid #ddd;'>

    <div style='background: #16a34a; padding: 30px; text-align: center;'>

        <h1 style='color: white; margin: 0;'>
            FarmEase Login OTP ??
        </h1>

    </div>

    <div style='padding: 30px;'>

        <p>
            Your login OTP is:
        </p>

        <div style='text-align:center; margin: 30px 0;'>

            <h2 style='font-size: 40px; color:#16a34a; letter-spacing: 8px;'>
                {code}
            </h2>

        </div>

        <p>
            OTP valid for 5 minutes.
        </p>

        <p style='color:red;'>
            Never share your OTP.
        </p>

    </div>

</div>";

                await _notificationService.SendEmailAsync(
                    dto.EmailOrPhone,
                    "FarmEase Login OTP ??",
                    body
                );

                Console.WriteLine($"LOGIN OTP: {code}");
            }
            else
            {
                await _notificationService.SendSmsAsync(
                    dto.EmailOrPhone,
                    $"Your FarmEase login OTP is: {code}"
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NOTIFICATION ERROR: {ex.Message}");
        }

        return code;
    }

    // =========================================================
    // VERIFY LOGIN OTP
    // =========================================================

    public async Task<AuthResponseDto?> VerifyOtpLoginAsync(
        VerifyOtpDto dto
    )
    {
        if (_otpStore.TryGetValue(dto.EmailOrPhone, out var storedCode))
        {
            if (
                _otpExpiryStore.TryGetValue(
                    dto.EmailOrPhone,
                    out var expiryTime
                )
            )
            {
                if (DateTime.UtcNow > expiryTime)
                {
                    _otpStore.TryRemove(dto.EmailOrPhone, out _);

                    _otpExpiryStore.TryRemove(
                        dto.EmailOrPhone,
                        out _
                    );

                    return null;
                }
            }

            if (storedCode == dto.OtpCode)
            {
                _otpStore.TryRemove(dto.EmailOrPhone, out _);

                _otpExpiryStore.TryRemove(
                    dto.EmailOrPhone,
                    out _
                );

                var user =
                    await _repo.GetByEmailOrPhoneAsync(
                        dto.EmailOrPhone
                    );

                if (user != null)
                {
                    await _activityService.LogActivityAsync(
                        "Login",
                        $"User logged in via OTP: {user.FullName}",
                        user.Email,
                        user.FullName
                    );

                    return new AuthResponseDto
                    {
                        Token =
                            _jwtHelper.GenerateToken(user),

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