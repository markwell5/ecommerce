using FluentValidation;
using User.Application.Commands;

namespace User.Application.Validators
{
    public class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
    {
        public AddAddressCommandValidator()
        {
            RuleFor(x => x.Request.Line1)
                .NotEmpty().WithMessage("Address line 1 is required")
                .MaximumLength(200);

            RuleFor(x => x.Request.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100);

            RuleFor(x => x.Request.PostCode)
                .NotEmpty().WithMessage("Post code is required")
                .MaximumLength(20);

            RuleFor(x => x.Request.Country)
                .NotEmpty().WithMessage("Country is required")
                .MaximumLength(100);
        }
    }

    public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
    {
        public UpdateAddressCommandValidator()
        {
            RuleFor(x => x.Request.Line1)
                .NotEmpty().WithMessage("Address line 1 is required")
                .MaximumLength(200);

            RuleFor(x => x.Request.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100);

            RuleFor(x => x.Request.PostCode)
                .NotEmpty().WithMessage("Post code is required")
                .MaximumLength(20);

            RuleFor(x => x.Request.Country)
                .NotEmpty().WithMessage("Country is required")
                .MaximumLength(100);
        }
    }
}
