using AutoMapper;
using Cart.Application.DTOs;
using Cart.Application.Interfaces;
using MediatR;

namespace Cart.Application.Commands;

public class RemoveFromCartHandler : IRequestHandler<RemoveFromCartCommand, CartDto?>
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;

    public RemoveFromCartHandler(ICartRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CartDto?> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetCartAsync(request.CartId);
        if (cart is null) return null;

        cart.Items.RemoveAll(i => i.ProductId == request.ProductId);
        cart.LastModifiedAt = DateTime.UtcNow;
        await _repository.SaveCartAsync(cart);

        return _mapper.Map<CartDto>(cart);
    }
}
