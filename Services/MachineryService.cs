using backEnd.Interfaces;
using backEnd.Models;
using backEnd.DTOs;
using backEnd.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using backEnd.Hubs;

namespace backEnd.Services;

public class MachineryService : IMachineryService
{
    private readonly IMachineryRepository _repo;
    private readonly IMapper _mapper;
    private readonly IHubContext<StockHub> _stockHub;

    public MachineryService(IMachineryRepository repo, IMapper mapper, IHubContext<StockHub> stockHub)
    {
        _repo   = repo;
        _mapper = mapper;
        _stockHub = stockHub;
    }

    public async Task<IEnumerable<MachineryDto>> GetAllMachineryAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<MachineryDto>>(items);
    }

    public async Task<Machinery> CreateMachineryAsync(Machinery machinery)
    {
        // Set primary image if available
        var primary = machinery.Media?.FirstOrDefault(m => m.IsPrimary) ?? machinery.Media?.FirstOrDefault();
        if (primary != null)
        {
            machinery.Image = primary.Url;
            primary.IsPrimary = true;
        }
        
        var created = await _repo.CreateAsync(machinery);
        
        // Notify Hub
        await _stockHub.Clients.All.SendAsync("ReceiveAbsoluteStock", created.Id, created.Quantity, "Machinery");
        
        return created;
    }

    public async Task<MachineryDto?> UpdateMachineryAsync(long id, MachineryDto dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return null;

        _mapper.Map(dto, existing);

        // Update primary image reference
        var primary = existing.Media?.FirstOrDefault(m => m.IsPrimary) ?? existing.Media?.FirstOrDefault();
        if (primary != null)
        {
            existing.Image = primary.Url;
            primary.IsPrimary = true;
        }

        await _repo.UpdateAsync(existing);
        
        // Notify Hub
        await _stockHub.Clients.All.SendAsync("ReceiveAbsoluteStock", existing.Id, existing.Quantity, "Machinery");
        
        return _mapper.Map<MachineryDto>(existing);
    }

    public async Task<bool> DeleteMachineryAsync(long id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return false;

        await _repo.DeleteAsync(item);
        return true;
    }

    public async Task ResetInventoryAsync()
    {
        // 1. Clear everything
        await _repo.ClearAllAsync();

        // 2. Seed default products
        var defaultItems = new List<Machinery>
        {
            new Machinery {
                Name = "John Deere 5050D",
                Price = 850000,
                Quantity = 10,
                Category = "Tractor",
                Condition = "Brand New",
                Description = "50 HP, Power Steering, Multi Plate Oil Immersed Brakes.",
                Image = "https://images.unsplash.com/photo-1594411643194-e575bc862803?w=800"
            },
            new Machinery {
                Name = "Mahindra Arjun 555",
                Price = 720000,
                Quantity = 15,
                Category = "Tractor",
                Condition = "Brand New",
                Description = "High productivity tractor for multi-crop operations.",
                Image = "https://images.unsplash.com/photo-1594913785162-e678536f9661?w=800"
            },
            new Machinery {
                Name = "Swaraj 744 FE",
                Price = 680000,
                Quantity = 20,
                Category = "Tractor",
                Condition = "Brand New",
                Description = "Highly reliable tractor for heavy duty farming.",
                Image = "https://images.unsplash.com/photo-1592982537447-7440770cbfc9?w=800"
            },
            new Machinery {
                Name = "Laser Land Leveler",
                Price = 250000,
                Quantity = 5,
                Category = "Implements",
                Condition = "Premium",
                Description = "Advanced precision land leveling for better irrigation.",
                Image = "https://images.unsplash.com/photo-1589923188900-85dae523342b?w=800"
            },
            new Machinery {
                Name = "Paddy Transplanter",
                Price = 300000,
                Quantity = 8,
                Category = "Seeding",
                Condition = "Brand New",
                Description = "High speed automatic rice seedling transplanter.",
                Image = "https://images.unsplash.com/photo-1599940824399-b87987cb9c2a?w=800"
            },
            new Machinery {
                Name = "Solar Water Pump",
                Price = 150000,
                Quantity = 30,
                Category = "Irrigation",
                Condition = "Eco-Friendly",
                Description = "High efficiency solar-powered irrigation pump.",
                Image = "https://images.unsplash.com/photo-1585832770485-e289c1e67040?w=800"
            },
            new Machinery {
                Name = "Power Tiller 15HP",
                Price = 120000,
                Quantity = 12,
                Category = "Small Scale",
                Condition = "Compact",
                Description = "Powerful tiller for small farms and orchards.",
                Image = "https://images.unsplash.com/photo-1416879595882-3373a0480b5b?w=800"
            }
        };

        foreach (var item in defaultItems)
        {
            await _repo.CreateAsync(item);
        }

        // 3. Broadcast global refresh to all clients
        await _stockHub.Clients.All.SendAsync("ReceiveRefresh");
    }

    public Machinery MapToModel(MachineryDto dto)
    {
        return _mapper.Map<Machinery>(dto);
    }
}
