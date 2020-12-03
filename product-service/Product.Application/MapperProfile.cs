using AutoMapper;
using Ecommerce.Model.Product.Request;
using Ecommerce.Model.Product.Response;
using Product.Application.Domain;

namespace Product.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateProductRequest, ProductDto>();
            CreateMap<ProductDto, ProductResponse>();
        }
    }
}
