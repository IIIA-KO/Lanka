using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Reviews;

namespace Lanka.Modules.Campaigns.Application.Reviews.Delete;

internal sealed class DeleteReviewCommandHandler : ICommandHandler<DeleteReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReviewCommandHandler(
        IReviewRepository reviewRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork
    )
    {
        this._reviewRepository = reviewRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        Review? review = await this._reviewRepository.GetByIdAsync(
            new ReviewId(request.ReviewId),
            cancellationToken
        );

        if (review is null)
        {
            return Result.Failure(ReviewErrors.NotFound);
        }

        if (this._userContext.GetUserId() != review.ClientId.Value)
        {
            return Result.Failure(ReviewErrors.NotOwner);
        }

        this._reviewRepository.Remove(review);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
