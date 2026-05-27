using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Abstractions.Photos;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Campaigns;

namespace Lanka.Modules.Campaigns.Application.Bloggers.Delete;

internal sealed class DeleteBloggerCommandHandler : ICommandHandler<DeleteBloggerCommand>
{
    private readonly IPhotoAccessor _photoAccessor;
    private readonly IBloggerRepository _bloggerRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBloggerCommandHandler(
        IPhotoAccessor photoAccessor,
        IBloggerRepository bloggerRepository,
        ICampaignRepository campaignRepository,
        IUnitOfWork unitOfWork
    )
    {
        this._photoAccessor = photoAccessor;
        this._bloggerRepository = bloggerRepository;
        this._campaignRepository = campaignRepository;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteBloggerCommand request, CancellationToken cancellationToken)
    {
        Blogger? blogger = await this._bloggerRepository.GetByIdAsync(
            new BloggerId(request.BloggerId),
            cancellationToken
        );

        if (blogger is null)
        {
            return Result.Failure(BloggerErrors.NotFound);
        }

        bool hasActiveCampaigns = await this._campaignRepository.HasActiveCampaignsAsync(
            blogger.Id,
            cancellationToken);

        if (hasActiveCampaigns)
        {
            return Result.Failure(BloggerErrors.ActiveCampaignsExist);
        }

        if (blogger.ProfilePhoto is not null)
        {
            await this._photoAccessor.DeletePhotoAsync(blogger.ProfilePhoto.Id);
        }

        blogger.Delete(DateTimeOffset.UtcNow);

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
