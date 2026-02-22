using AutoMapper;
using Ecommerce.Model.User.Request;
using Ecommerce.Model.User.Response;
using User.Application.Entities;

namespace User.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<ApplicationUser, UserResponse>()
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber));

            CreateMap<Address, AddressResponse>();

            CreateMap<AddressRequest, Address>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}
