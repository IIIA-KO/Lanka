using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Application.Bloggers.CreateBlogger;

internal sealed class CreateBloggerCommandHandler
    : ICommandHandler<CreateBloggerCommand>
{
    private readonly IBloggerRepository _bloggerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBloggerCommandHandler(IBloggerRepository bloggerRepository, IUnitOfWork unitOfWork)
    {
        this._bloggerRepository = bloggerRepository;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateBloggerCommand request, CancellationToken cancellationToken)
    {
        var blogger = new Blogger(
            request.BloggerId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.BirthDate
        );
            
        this._bloggerRepository.Add(blogger);
            
        await this._unitOfWork.SaveChangesAsync(cancellationToken);
            
        return Result.Success();
    }
}
