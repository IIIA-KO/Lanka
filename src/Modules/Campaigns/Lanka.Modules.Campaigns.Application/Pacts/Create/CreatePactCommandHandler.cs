using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Pacts.Create;

internal sealed class CreatePactCommandHandler
    : ICommandHandler<CreatePactCommand, Guid>
{
    private readonly IPactRepository _pactRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePactCommandHandler(
        IPactRepository pactRepository,
        IUserContext userContext, 
        IUnitOfWork unitOfWork
    )
    {
        this._pactRepository = pactRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreatePactCommand request, CancellationToken cancellationToken)
    {
        var bloggerId = new BloggerId(this._userContext.GetUserId());

        Pact? pact = await this._pactRepository.GetByBloggerIdAsync(
            bloggerId,
            cancellationToken
        );
        
        if (pact is not null)
        {
            return Result.Failure<Guid>(PactErrors.Duplicate);
        }

        Result<Pact> result = Pact.Create(bloggerId, request.Content);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }
        
        pact = result.Value;

        this._pactRepository.Add(pact);
        
        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return pact.Id.Value;
    }
}
