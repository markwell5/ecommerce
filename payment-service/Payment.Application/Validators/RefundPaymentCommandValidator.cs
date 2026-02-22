using FluentValidation;
using Payment.Application.Commands;

namespace Payment.Application.Validators
{
    public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
    {
        public RefundPaymentCommandValidator()
        {
            RuleFor(x => x.PaymentId).GreaterThan(0);
        }
    }
}
