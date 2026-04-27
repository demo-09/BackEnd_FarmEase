using backEnd.DTOs;
using backEnd.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backEnd.Interfaces;

public interface IMachineryService
{
    Task<IEnumerable<MachineryDto>> GetAllMachineryAsync();
    Task<Machinery> CreateMachineryAsync(Machinery machinery);
    Task<MachineryDto?> UpdateMachineryAsync(long id, MachineryDto dto);
    Task<bool> DeleteMachineryAsync(long id);
}
