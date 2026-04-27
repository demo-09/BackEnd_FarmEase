using backEnd.Interfaces;
using backEnd.DTOs;
using backEnd.Repositories;
using AutoMapper;

namespace backEnd.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;

    public UserService(IUserRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<bool> UpdateUserAsync(string id, UserDto dto)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return false;

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Role = dto.Role;
        
        if (dto.Phone != null) user.Phone = dto.Phone;
        if (dto.Address != null) user.Address = dto.Address;
        if (dto.BirthDate != null) user.BirthDate = dto.BirthDate;
        if (dto.Bio != null) user.Bio = dto.Bio;
        if (dto.JoinedDate != null) user.JoinedDate = dto.JoinedDate;
        if (dto.Avatar != null) user.Avatar = dto.Avatar;
        if (dto.WishlistJson != null) user.WishlistJson = dto.WishlistJson;
        if (dto.CartJson != null) user.CartJson = dto.CartJson;

        await _repo.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeleteUserByEmailAsync(string email)
    {
        var user = await _repo.GetByEmailAsync(email);
        if (user == null) return false;

        await _repo.DeleteAsync(user);
        return true;
    }
}
