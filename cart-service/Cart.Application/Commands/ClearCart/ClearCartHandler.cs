using Cart.Application.Interfaces;
using MediatR;

namespace Cart.Application.Commands;

public class ClearCartHandler : IRequestHandler<ClearCartCommand, bool>
{
    private readonly ICartRepository _repository;

    public ClearCartHandler(ICartRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetCartAsync(request.CartId);
        if (cart is null) return false;

        await _repository.DeleteCartAsync(request.CartId);
        return true;
    }
}
