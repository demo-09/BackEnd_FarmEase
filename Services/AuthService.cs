using backEnd.Interfaces;
using backEnd.Models;
using backEnd.DTOs;
using backEnd.Helpers;
using backEnd.Repositories;
using AutoMapper;
using Google.Apis.Auth;

namespace backEnd.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _repo;
    private readonly JwtHelper _jwtHelper;
    private readonly IMapper _mapper;

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
}
