using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Application.Bloggers.UpdateBlogger;

internal sealed class UpdateBloggerCommandHandler
    : ICommandHandler<UpdateBloggerCommand>
{
    private readonly IBloggerRepository _bloggerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBloggerCommandHandler(
        IBloggerRepository bloggerRepository,
        IUnitOfWork unitOfWork
    )
    {
        this._bloggerRepository = bloggerRepository;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateBloggerCommand request, CancellationToken cancellationToken)
    {
        Blogger? blogger = await this._bloggerRepository.GetByIdAsync(
            new BloggerId(request.BloggerId),
            cancellationToken
        );

        if (blogger is null)
        {
            return Result.Failure(BloggerErrors.NotFound);
        }

        blogger.Update(
            request.FirstName,
            request.LastName,
            request.BirthDate
        );

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
