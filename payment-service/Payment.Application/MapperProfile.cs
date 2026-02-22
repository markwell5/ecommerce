using AutoMapper;
using Ecommerce.Model.Payment.Response;

namespace Payment.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Entities.Payment, PaymentResponse>();
        }
    }
}
