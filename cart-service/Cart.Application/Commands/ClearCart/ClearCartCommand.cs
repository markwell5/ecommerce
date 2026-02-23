using MediatR;

namespace Cart.Application.Commands;

public record ClearCartCommand(string CartId) : IRequest<bool>;
