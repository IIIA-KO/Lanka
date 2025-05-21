using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Abstractions.Photos;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Bloggers.Photos;

namespace Lanka.Modules.Campaigns.Application.Bloggers.Photos.DeleteProfilePhoto;

internal sealed class DeleteProfilePhotoCommandHandler
    : ICommandHandler<DeleteProfilePhotoCommand>
{
    private readonly IPhotoAccessor _photoAccessor;
    private readonly IBloggerRepository _bloggerRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProfilePhotoCommandHandler(
        IPhotoAccessor photoAccessor,
        IBloggerRepository bloggerRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork
    )
    {
        this._photoAccessor = photoAccessor;
        this._bloggerRepository = bloggerRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteProfilePhotoCommand request,
        CancellationToken cancellationToken
    )
    {
        Blogger blogger = await this._bloggerRepository.GetByIdAsync(
            new BloggerId(this._userContext.GetUserId()),
            cancellationToken
        );

        if (blogger!.ProfilePhoto is null)
        {
            return Result.Failure(PhotoErrors.PhotoNotFound);
        }

        Result deletePhotoResult = await this._photoAccessor.DeletePhotoAsync(
            blogger.ProfilePhoto.Id
        );

        if (deletePhotoResult.IsFailure)
        {
            return deletePhotoResult;
        }

        blogger.RemoveProfilePhoto();

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
