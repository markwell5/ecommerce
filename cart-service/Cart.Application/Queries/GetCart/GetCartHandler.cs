using AutoMapper;
using Cart.Application.DTOs;
using Cart.Application.Interfaces;
using MediatR;

namespace Cart.Application.Queries;

public class GetCartHandler : IRequestHandler<GetCartQuery, CartDto?>
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;

    public GetCartHandler(ICartRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CartDto?> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetCartAsync(request.CartId);
        return cart is null ? null : _mapper.Map<CartDto>(cart);
    }
}
