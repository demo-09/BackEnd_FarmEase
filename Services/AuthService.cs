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
    // LOGIN
    // =========================================================

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user =
            await _repo.GetByEmailOrPhoneAsync(dto.Email);

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


}