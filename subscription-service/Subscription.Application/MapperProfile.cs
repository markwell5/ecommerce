using AutoMapper;
using Ecommerce.Model.Subscription.Response;

namespace Subscription.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Entities.Subscription, SubscriptionResponse>();
            CreateMap<Entities.RenewalHistory, RenewalHistoryResponse>();
        }
    }
}
