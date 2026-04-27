using backEnd.Models;

namespace backEnd.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
}
