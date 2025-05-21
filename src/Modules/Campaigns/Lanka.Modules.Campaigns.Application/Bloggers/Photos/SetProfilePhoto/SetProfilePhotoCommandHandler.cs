using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Abstractions.Photos;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Bloggers.Photos;

namespace Lanka.Modules.Campaigns.Application.Bloggers.Photos.SetProfilePhoto;

internal sealed class SetProfilePhotoCommandHandler : ICommandHandler<SetProfilePhotoCommand>
{
    private readonly IPhotoAccessor _photoAccessor;
    private readonly IBloggerRepository _bloggerRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public SetProfilePhotoCommandHandler(
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

    public async Task<Result> Handle(SetProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        Result<Photo> addPhotoResult = await this._photoAccessor.AddPhotoAsync(request.File);
        
        if (addPhotoResult.IsFailure)
        {
            return Result.Failure(addPhotoResult.Error);
        }
        
        Photo photo = addPhotoResult.Value;

        Blogger blogger = await this._bloggerRepository.GetByIdAsync(
            new BloggerId(this._userContext.GetUserId()),
            cancellationToken
        );

        if (blogger!.ProfilePhoto is not null)
        {
            Result deletePhotoResult = await this._photoAccessor.DeletePhotoAsync(blogger.ProfilePhoto.Id);
            
            if (deletePhotoResult.IsFailure)
            {
                return deletePhotoResult;
            }
        }
        
        blogger.SetProfilePhoto(photo);
        
        await this._unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
