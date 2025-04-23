using FluentValidation;
using Lanka.Modules.Users.Domain.Users.FirstNames;
using Lanka.Modules.Users.Domain.Users.LastNames;

namespace Lanka.Modules.Users.Application.Users.UpdateUser;

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        this.RuleFor(c => c.UserId)
            .NotNull()
            .NotEmpty()
            .WithMessage("UserId is required");
        
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
            .WithMessage("Birth Date is required.");
    }
}
