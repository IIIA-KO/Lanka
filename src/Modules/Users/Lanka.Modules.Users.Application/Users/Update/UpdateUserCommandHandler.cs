using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Users.GetUser;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Users.Update;

internal sealed class UpdateUserCommandHandler
    : ICommandHandler<UpdateUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork
    )
    {
        this._userRepository = userRepository;
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
