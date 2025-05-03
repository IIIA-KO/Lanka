using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;
using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Application.Bloggers.UpdateBlogger;

internal sealed class UpdateBloggerCommandHandler
    : ICommandHandler<UpdateBloggerCommand, BloggerResponse>
{
    private readonly IBloggerRepository _bloggerRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBloggerCommandHandler(
        IBloggerRepository bloggerRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork
    )
    {
        this._bloggerRepository = bloggerRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<BloggerResponse>> Handle(UpdateBloggerCommand request, CancellationToken cancellationToken)
    {
        Guid userId = this._userContext.GetUserId();
        Blogger? blogger = await this._bloggerRepository.GetByIdAsync(
            new BloggerId(userId),
            cancellationToken
        );

        if (blogger is null)
        {
            return Result.Failure<BloggerResponse>(BloggerErrors.NotFound);
        }
        
        if (this._userContext.GetUserId() != blogger.Id.Value)
        {
            return Result.Failure<BloggerResponse>(Error.NotAuthorized);
        }

        Result result = blogger.Update(
            request.FirstName,
            request.LastName,
            request.BirthDate,
            request.Bio
        );
        
        if (result.IsFailure)
        {
            return Result.Failure<BloggerResponse>(result.Error);
        }

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return BloggerResponse.FromBloger(blogger);
    }
}
