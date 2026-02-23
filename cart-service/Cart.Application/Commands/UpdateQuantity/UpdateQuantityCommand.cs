using Cart.Application.DTOs;
using MediatR;

namespace Cart.Application.Commands;

public record UpdateQuantityCommand(string CartId, long ProductId, int Quantity) : IRequest<CartDto?>;
