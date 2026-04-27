using backEnd.Models;

namespace backEnd.Repositories;

public interface IMachineryRepository
{
    Task<IEnumerable<Machinery>> GetAllAsync();
    Task<Machinery?> GetByIdAsync(long id);
    Task<Machinery> CreateAsync(Machinery machinery);
    Task<Machinery> UpdateAsync(Machinery machinery);
    Task DeleteAsync(Machinery machinery);
}
