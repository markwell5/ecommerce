using AutoMapper;
using Ecommerce.Model.Product.Request;
using Ecommerce.Model.Product.Response;

namespace Product.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateProductRequest, Entities.Product>();
            CreateMap<Entities.Product, ProductResponse>();
        }
    }
}
