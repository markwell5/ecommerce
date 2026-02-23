using Cart.Application.DTOs;
using MediatR;

namespace Cart.Application.Queries;

public record GetCartQuery(string CartId) : IRequest<CartDto?>;
