using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.DeleteAccountData;

internal sealed class DeleteInstagramAccountDataCommandHandler
    : ICommandHandler<DeleteInstagramAccountDataCommand>
{
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly IMongoCleanupService _mongoCleanupService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteInstagramAccountDataCommandHandler(
        IInstagramAccountRepository instagramAccountRepository,
        IMongoCleanupService mongoCleanupService,
        IUnitOfWork unitOfWork
    )
    {
        this._instagramAccountRepository = instagramAccountRepository;
        this._mongoCleanupService = mongoCleanupService;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteInstagramAccountDataCommand request, CancellationToken cancellationToken)
    {
        InstagramAccount? instagramAccount = await this._instagramAccountRepository.GetByUserIdWithTokenAsync(
            new UserId(request.UserId),
            cancellationToken
        );

        if (instagramAccount is null)
        {
            return Result.Failure(InstagramAccountErrors.NotFound);
        }

        this._instagramAccountRepository.Remove(instagramAccount);

        await Task.WhenAll(
            this._mongoCleanupService.DeleteByInstagramAccountIdAsync(instagramAccount.Id.Value, cancellationToken),
            this._mongoCleanupService.DeleteByUserIdAsync(instagramAccount.UserId.Value, cancellationToken)
        );

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
