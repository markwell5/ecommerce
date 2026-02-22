using FluentValidation;
using Order.Application.Commands;

namespace Order.Application.Validators
{
    public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
    {
        public PlaceOrderCommandValidator()
        {
            RuleFor(x => x.Request.CustomerId)
                .NotEmpty().WithMessage("CustomerId is required")
                .MaximumLength(200);

            RuleFor(x => x.Request.Items)
                .NotEmpty().WithMessage("At least one item is required");

            RuleForEach(x => x.Request.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).GreaterThan(0);
                item.RuleFor(i => i.Quantity).GreaterThan(0);
                item.RuleFor(i => i.UnitPrice).GreaterThan(0);
                item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);
            });
        }
    }
}
