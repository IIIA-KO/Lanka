using FluentValidation;
using Lanka.Modules.Users.Domain.Users.BirthDates;
using Lanka.Modules.Users.Domain.Users.Emails;
using Lanka.Modules.Users.Domain.Users.FirstNames;
using Lanka.Modules.Users.Domain.Users.LastNames;

namespace Lanka.Modules.Users.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        this.RuleFor(c => c.FirstName)
            .NotEmpty()
            .NotNull()
            .MinimumLength(2)
            .MaximumLength(FirstName.MaxLength);
            
        this.RuleFor(c => c.LastName)
            .NotEmpty()
            .NotNull()
            .MinimumLength(2)
            .MaximumLength(LastName.MaxLength);

        this.RuleFor(c => c.BirthDate)
            .NotEmpty()
            .WithMessage("Birth Date is required.")
            .Must(BirthDate.Validate)
            .WithMessage($"You must be at least {BirthDate.MinimumAge} years old.");
        
        this.RuleFor(c => c.Email)
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .MaximumLength(Email.MaxLength);
            
        this.RuleFor(c => c.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password length must be at least 8.")
            .MaximumLength(16)
            .WithMessage("Password length must not exceed 16.")
            .Matches(@"[A-Z]+")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]+")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]+")
            .WithMessage("Password must contain at least one number.")
            .Matches(@"[\!\?\*\.\$]+")
            .WithMessage("Password must contain at least one (!? *.$).");
    }
}
