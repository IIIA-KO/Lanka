using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Users.GetUser;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Users.UpdateUser;

internal sealed class UpdateUserCommandHandler
    : ICommandHandler<UpdateUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IUserContext userContext, 
        IUnitOfWork unitOfWork
    )
    {
        this._userRepository = userRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<UserResponse>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await this._userRepository.GetByIdAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound);
        }

        if (this._userContext.GetIdentityId() != user.IdentityId)
        {
            return Result.Failure<UserResponse>(Error.NotAuthorized);
        }

        Result result = user.Update(
            request.FirstName,
            request.LastName,
            request.BirthDate
        );

        if (result.IsFailure)
        {
            return Result.Failure<UserResponse>(result.Error);
        }
        
        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return UserResponse.FromUser(user);
    }
}
