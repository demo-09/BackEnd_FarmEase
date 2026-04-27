using backEnd.Data;
using backEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace backEnd.Repositories;

public class MachineryRepository : IMachineryRepository
{
    private readonly AppDbContext _context;

    public MachineryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Machinery>> GetAllAsync()
        => await _context.Machineries.ToListAsync();

    public async Task<Machinery?> GetByIdAsync(long id)
        => await _context.Machineries.FindAsync(id);

    public async Task<Machinery> CreateAsync(Machinery machinery)
    {
        _context.Machineries.Add(machinery);
        await _context.SaveChangesAsync();
        return machinery;
    }

    public async Task<Machinery> UpdateAsync(Machinery machinery)
    {
        await _context.SaveChangesAsync();
        return machinery;
    }

    public async Task DeleteAsync(Machinery machinery)
    {
        _context.Machineries.Remove(machinery);
        await _context.SaveChangesAsync();
    }
}
