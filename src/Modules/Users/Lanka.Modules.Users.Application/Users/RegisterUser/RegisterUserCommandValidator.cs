using FluentValidation;

namespace Evently.Modules.Users.Application.Users.RegisterUser
{
    internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            this.RuleFor(c => c.FirstName).NotEmpty();
            this.RuleFor(c => c.LastName).NotEmpty();
            this.RuleFor(c => c.Email).EmailAddress();
            this.RuleFor(c => c.Password).MinimumLength(6);
        }
    }
}
