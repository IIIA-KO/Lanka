using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Domain.Users;
using Lanka.Modules.Users.Domain.Users.BirthDates;
using Lanka.Modules.Users.Domain.Users.Emails;
using Lanka.Modules.Users.Domain.Users.FirstNames;
using Lanka.Modules.Users.Domain.Users.LastNames;

namespace Lanka.Modules.Users.Application.Users.RegisterUser
{
    internal sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, UserId>
    {
        private readonly IIdentityProviderService _identityProviderService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserCommandHandler(
            IIdentityProviderService identityProviderService,
            IUserRepository userRepository, 
            IUnitOfWork unitOfWork
        )
        {
            this._identityProviderService = identityProviderService;
            this._userRepository = userRepository;
            this._unitOfWork = unitOfWork;
        }

        public async Task<Result<UserId>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            Result<string> result = await this._identityProviderService.RegisterUserAsync(
                new UserModel(request.Email, request.Password, request.FirstName, request.LastName),
                cancellationToken);

            if (result.IsFailure)
            {
                return Result.Failure<UserId>(result.Error);
            }
            
            Result<User> userCreateResult = User.Create(
                request.FirstName,
                request.LastName,
                request.Email,
                request.BirthDate,
                result.Value
            );

            if (userCreateResult.IsFailure)
            {
                return Result.Failure<UserId>(userCreateResult.Error);
            }
            
            User user = userCreateResult.Value;
            
            this._userRepository.Add(user);

            await this._unitOfWork.SaveChangesAsync(cancellationToken);

            return user.Id;
        }
    }
}
