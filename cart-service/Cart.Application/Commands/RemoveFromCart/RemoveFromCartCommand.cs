using Cart.Application.DTOs;
using MediatR;

namespace Cart.Application.Commands;

public record RemoveFromCartCommand(string CartId, long ProductId) : IRequest<CartDto?>;
