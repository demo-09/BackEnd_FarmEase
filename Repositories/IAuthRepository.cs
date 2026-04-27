using backEnd.Models;

namespace backEnd.Repositories;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
}
