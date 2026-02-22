using AutoMapper;
using Ecommerce.Model.Order.Response;
using Order.Application.Entities;

namespace Order.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Entities.Order, OrderResponse>()
                .ForMember(dest => dest.Events, opt => opt.Ignore());
            CreateMap<OrderEvent, OrderEventResponse>();
        }
    }
}
