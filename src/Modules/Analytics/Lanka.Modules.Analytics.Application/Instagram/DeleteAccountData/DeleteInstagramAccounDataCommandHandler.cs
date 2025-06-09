using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.DeleteAccountData;

internal sealed class DeleteInstagramAccounDataCommandHandler
    : ICommandHandler<DeleteInstagramAccountDataCommand>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteInstagramAccounDataCommandHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IUnitOfWork unitOfWork
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteInstagramAccountDataCommand request, CancellationToken cancellationToken)
    {
        InstagramAccount? igAccount = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (igAccount is null)
        {
            return Result.Failure(InstagramAccountErrors.NotFound);
        }

        this._instagramAccountRepository.Remove(igAccount);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
