using FluentValidation;

namespace Cart.Application.Commands;

public class UpdateQuantityValidator : AbstractValidator<UpdateQuantityCommand>
{
    public UpdateQuantityValidator()
    {
        RuleFor(x => x.CartId).NotEmpty();
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
