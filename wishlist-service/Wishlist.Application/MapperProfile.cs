using AutoMapper;
using Ecommerce.Model.Wishlist.Response;
using Wishlist.Application.Entities;

namespace Wishlist.Application
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Entities.Wishlist, WishlistResponse>();
            CreateMap<WishlistItem, WishlistItemResponse>();
        }
    }
}
