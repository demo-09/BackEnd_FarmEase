using backEnd.Interfaces;
using backEnd.Models;
using backEnd.DTOs;
using backEnd.Repositories;
using AutoMapper;

namespace backEnd.Services;

public class MachineryService : IMachineryService
{
    private readonly IMachineryRepository _repo;
    private readonly IMapper _mapper;

    public MachineryService(IMachineryRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MachineryDto>> GetAllMachineryAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<MachineryDto>>(items);
    }

    public async Task<Machinery> CreateMachineryAsync(Machinery machinery)
        => await _repo.CreateAsync(machinery);

    public async Task<MachineryDto?> UpdateMachineryAsync(long id, MachineryDto dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return null;

        _mapper.Map(dto, existing);
        await _repo.UpdateAsync(existing);

        return _mapper.Map<MachineryDto>(existing);
    }

    public async Task<bool> DeleteMachineryAsync(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return false;

        await _repo.DeleteAsync(item);
        return true;
    }
}
