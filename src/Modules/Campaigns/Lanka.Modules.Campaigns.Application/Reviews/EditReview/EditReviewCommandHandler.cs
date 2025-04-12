using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Reviews.GetReview;
using Lanka.Modules.Campaigns.Domain.Reviews;

namespace Lanka.Modules.Campaigns.Application.Reviews.EditReview;

internal sealed class EditReviewCommandHandler
    : ICommandHandler<EditReviewCommand, ReviewResponse>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public EditReviewCommandHandler(
        IReviewRepository reviewRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork
    )
    {
        this._reviewRepository = reviewRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<ReviewResponse>> Handle(EditReviewCommand request, CancellationToken cancellationToken)
    {
        Review? review = await this._reviewRepository.GetByIdAsync(
            new ReviewId(request.ReviewId),
            cancellationToken
        );

        if (review is null)
        {
            return Result.Failure<ReviewResponse>(ReviewErrors.NotFound);
        }

        if (this._userContext.GetUserId() != review.ClientId.Value)
        {
            return Result.Failure<ReviewResponse>(ReviewErrors.NotOwner);
        }

        Result result = review.Update(request.Rating, request.Comment);

        if (result.IsFailure)
        {
            return Result.Failure<ReviewResponse>(result.Error);
        }

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReviewResponse(
            review.Id.Value,
            review.Rating.Value,
            review.Comment.Value,
            review.CreatedOnUtc
        );
    }
}
