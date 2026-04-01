using AutoMapper;
using Ecommerce.Model.Product.Request;
using Ecommerce.Model.Product.Response;

namespace Product.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateProductRequest, Entities.Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SearchVector, opt => opt.Ignore())
                .ForMember(dest => dest.ProductCategories, opt => opt.Ignore());
            CreateMap<Entities.Product, ProductResponse>();
        }
    }
}
