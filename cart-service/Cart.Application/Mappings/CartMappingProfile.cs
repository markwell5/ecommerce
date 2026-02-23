using AutoMapper;
using Cart.Application.DTOs;
using Cart.Application.Models;

namespace Cart.Application.Mappings;

public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {
        CreateMap<Models.Cart, CartDto>();
        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.LineTotal, opt => opt.MapFrom(s => s.UnitPrice * s.Quantity));
    }
}
