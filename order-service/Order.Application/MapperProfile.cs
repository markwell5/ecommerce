using AutoMapper;
using Ecommerce.Model.Order.Response;
using Order.Application.Entities;

namespace Order.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Entities.Order, OrderResponse>();
            CreateMap<OrderEvent, OrderEventResponse>();
        }
    }
}
