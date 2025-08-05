using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Application.Instagram;

internal sealed class UpdateInstagramDataCommandHandler : ICommandHandler<UpdateInstagramDataCommand>
{
    private readonly IBloggerRepository _bloggerRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public UpdateInstagramDataCommandHandler(IBloggerRepository bloggerRepository, IUnitOfWork unitOfWork)
    {
        this._bloggerRepository = bloggerRepository;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateInstagramDataCommand request, CancellationToken cancellationToken)
    {
        Blogger? blogger = await this._bloggerRepository.GetByIdAsync(
            new BloggerId(request.UserId),
            cancellationToken
        );

        if (blogger is null)
        {
            return Result.Failure(BloggerErrors.NotFound);
        }
        
        blogger.UpdateInstagramData(
            request.Username,
            request.FollowersCount,
            request.MediaCount
        );

        await this._unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
