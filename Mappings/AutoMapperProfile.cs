using AutoMapper;
using backEnd.Models;
using backEnd.DTOs;

namespace backEnd.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Machinery, MachineryDto>().ReverseMap();
    }
}
