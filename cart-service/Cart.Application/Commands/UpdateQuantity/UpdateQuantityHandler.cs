using AutoMapper;
using Cart.Application.DTOs;
using Cart.Application.Interfaces;
using MediatR;

namespace Cart.Application.Commands;

public class UpdateQuantityHandler : IRequestHandler<UpdateQuantityCommand, CartDto?>
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;

    public UpdateQuantityHandler(ICartRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CartDto?> Handle(UpdateQuantityCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetCartAsync(request.CartId);
        if (cart is null) return null;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (item is null) return null;

        item.Quantity = request.Quantity;
        cart.LastModifiedAt = DateTime.UtcNow;
        await _repository.SaveCartAsync(cart);

        return _mapper.Map<CartDto>(cart);
    }
}
