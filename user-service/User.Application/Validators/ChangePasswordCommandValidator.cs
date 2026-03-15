using FluentValidation;
using User.Application.Commands;

namespace User.Application.Validators
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.Request.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required")
                .MaximumLength(128);

            RuleFor(x => x.Request.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(8).WithMessage("New password must be at least 8 characters")
                .MaximumLength(128);
        }
    }
}
