using Cart.Application.DTOs;
using MediatR;

namespace Cart.Application.Commands;

public record AddToCartCommand(string CartId, long ProductId, int Quantity) : IRequest<CartDto>;
