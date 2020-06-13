using AutoMapper;
using Product.Service.Commands;
using Product.Service.Domain;

namespace Product.Service.Config
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateProductCommand, ProductDto>();
        }
    }
}
