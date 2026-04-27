using backEnd.DTOs;

namespace backEnd.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> UpdateUserAsync(string id, UserDto dto);
    Task<bool> DeleteUserByEmailAsync(string email);
}
