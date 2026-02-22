using AutoMapper;
using Ecommerce.Model.Stock.Response;
using Stock.Application.Entities;

namespace Stock.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<StockItem, StockResponse>();
        }
    }
}
