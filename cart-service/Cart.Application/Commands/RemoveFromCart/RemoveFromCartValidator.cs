using FluentValidation;

namespace Cart.Application.Commands;

public class RemoveFromCartValidator : AbstractValidator<RemoveFromCartCommand>
{
    public RemoveFromCartValidator()
    {
        RuleFor(x => x.CartId).NotEmpty();
        RuleFor(x => x.ProductId).GreaterThan(0);
    }
}
