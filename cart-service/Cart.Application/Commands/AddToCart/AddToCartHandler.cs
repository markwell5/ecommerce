using AutoMapper;
using Cart.Application.DTOs;
using Cart.Application.Interfaces;
using Cart.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cart.Application.Commands;

public class AddToCartHandler : IRequestHandler<AddToCartCommand, CartDto>
{
    private readonly ICartRepository _repository;
    private readonly IProductCatalogClient _productCatalog;
    private readonly IMapper _mapper;
    private readonly ILogger<AddToCartHandler> _logger;

    public AddToCartHandler(
        ICartRepository repository,
        IProductCatalogClient productCatalog,
        IMapper mapper,
        ILogger<AddToCartHandler> logger)
    {
        _repository = repository;
        _productCatalog = productCatalog;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CartDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var product = await _productCatalog.GetProductAsync(request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} not found");

        var cart = await _repository.GetCartAsync(request.CartId) ?? new Models.Cart { Id = request.CartId };

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = request.Quantity,
                UnitPrice = product.Price
            });
        }

        cart.LastModifiedAt = DateTime.UtcNow;
        await _repository.SaveCartAsync(cart);

        _logger.LogInformation("Added product {ProductId} (qty {Quantity}) to cart {CartId}",
            request.ProductId, request.Quantity, request.CartId);

        return _mapper.Map<CartDto>(cart);
    }
}
