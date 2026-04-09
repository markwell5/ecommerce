using AutoMapper;
using Ecommerce.Model.Return.Response;
using Return.Application.Entities;

namespace Return.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<ReturnRequest, ReturnResponse>();
            CreateMap<Entities.ReturnShipment, Ecommerce.Model.Return.Response.ReturnShipmentResponse>();
        }
    }
}
