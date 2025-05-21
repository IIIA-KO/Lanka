using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Users.Delete;

internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IIdentityProviderService _identityProviderService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        IIdentityProviderService identityProviderService,
        IUnitOfWork unitOfWork
    )
    {
        this._userContext = userContext;
        this._userRepository = userRepository;
        this._identityProviderService = identityProviderService;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        User user = await this._userRepository.GetByIdAsync(
            new UserId(this._userContext.GetUserId()),
            cancellationToken
        );

        Result result = await this._identityProviderService.DeleteUserAsync(user!.IdentityId, cancellationToken);
        
        if (result.IsFailure)
        {
            return result;
        }
        
        this._userRepository.Remove(user);
        
        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
