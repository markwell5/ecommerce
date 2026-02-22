using FluentValidation;
using User.Application.Commands;

namespace User.Application.Validators
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Request.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
