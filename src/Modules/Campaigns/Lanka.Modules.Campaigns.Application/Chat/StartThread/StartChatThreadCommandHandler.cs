using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Chat.GetThreads;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Chat;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Chat.StartThread;

internal sealed class StartChatThreadCommandHandler
    : ICommandHandler<StartChatThreadCommand, ChatThreadResponse>
{
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IChatThreadRepository _chatThreadRepository;
    private readonly IBloggerRepository _bloggerRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IPactRepository _pactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StartChatThreadCommandHandler(
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IChatThreadRepository chatThreadRepository,
        IBloggerRepository bloggerRepository,
        ICampaignRepository campaignRepository,
        IOfferRepository offerRepository,
        IPactRepository pactRepository,
        IUnitOfWork unitOfWork)
    {
        this._userContext = userContext;
        this._dateTimeProvider = dateTimeProvider;
        this._chatThreadRepository = chatThreadRepository;
        this._bloggerRepository = bloggerRepository;
        this._campaignRepository = campaignRepository;
        this._offerRepository = offerRepository;
        this._pactRepository = pactRepository;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<ChatThreadResponse>> Handle(
        StartChatThreadCommand request,
        CancellationToken cancellationToken)
    {
        var currentBloggerId = new BloggerId(this._userContext.GetUserId());
        var otherBloggerId = new BloggerId(request.ParticipantBloggerId);

        Blogger? otherBlogger = await this._bloggerRepository.GetByIdAsync(otherBloggerId, cancellationToken);
        if (otherBlogger is null)
        {
            return Result.Failure<ChatThreadResponse>(BloggerErrors.NotFound);
        }

        Campaign? campaign = null;
        CampaignId? campaignId = null;
        if (request.CampaignId.HasValue)
        {
            campaignId = new CampaignId(request.CampaignId.Value);
            campaign = await this._campaignRepository.GetByIdAsync(campaignId, cancellationToken);

            if (campaign is null)
            {
                return Result.Failure<ChatThreadResponse>(CampaignErrors.NotFound);
            }

            if (!campaign.ClientId.Equals(currentBloggerId) && !campaign.CreatorId.Equals(currentBloggerId))
            {
                return Result.Failure<ChatThreadResponse>(Error.NotAuthorized);
            }

            if (!campaign.ClientId.Equals(otherBloggerId) && !campaign.CreatorId.Equals(otherBloggerId))
            {
                return Result.Failure<ChatThreadResponse>(Error.NotAuthorized);
            }
        }

        Offer? offer = null;
        OfferId? offerId = null;
        if (request.OfferId.HasValue)
        {
            offerId = new OfferId(request.OfferId.Value);
            offer = await this._offerRepository.GetByIdAsync(offerId, cancellationToken);

            if (offer is null)
            {
                return Result.Failure<ChatThreadResponse>(OfferErrors.NotFound);
            }

            Pact? pact = await this._pactRepository.GetByIdAsync(offer.PactId, cancellationToken);
            if (pact is null || !pact.BloggerId.Equals(otherBloggerId))
            {
                return Result.Failure<ChatThreadResponse>(Error.NotAuthorized);
            }
        }

        ChatThread? thread = await this._chatThreadRepository.GetByParticipantsAsync(
            currentBloggerId,
            otherBloggerId,
            campaignId,
            offerId,
            cancellationToken);

        if (thread is null)
        {
            Result<ChatThread> threadResult = ChatThread.Create(
                currentBloggerId,
                otherBloggerId,
                this._dateTimeProvider.UtcNow,
                campaignId,
                offerId);

            if (threadResult.IsFailure)
            {
                return Result.Failure<ChatThreadResponse>(threadResult.Error);
            }

            thread = threadResult.Value;
            this._chatThreadRepository.Add(thread);

            await this._unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ChatThreadResponse(
            thread.Id.Value,
            otherBlogger.Id.Value,
            otherBlogger.FirstName.Value,
            otherBlogger.LastName.Value,
            otherBlogger.InstagramMetadata.Username,
            otherBlogger.ProfilePhoto?.Uri.ToString(),
            thread.CampaignId?.Value,
            campaign?.Name.Value,
            thread.OfferId?.Value,
            offer?.Name.Value,
            null,
            false,
            null,
            0,
            thread.UpdatedAtUtc.UtcDateTime);
    }
}
