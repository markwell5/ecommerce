using AutoMapper;
using Ecommerce.Model.GiftCard.Response;
using GiftCard.Application.Entities;

namespace GiftCard.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<GiftCardEntity, GiftCardResponse>();
            CreateMap<GiftCardTransaction, GiftCardTransactionResponse>();
        }
    }
}
