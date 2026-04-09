using AutoMapper;
using Ecommerce.Model.Loyalty.Response;
using Loyalty.Application.Entities;

namespace Loyalty.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<LoyaltyAccount, LoyaltyAccountResponse>();
            CreateMap<PointsTransaction, PointsTransactionResponse>();
        }
    }
}
