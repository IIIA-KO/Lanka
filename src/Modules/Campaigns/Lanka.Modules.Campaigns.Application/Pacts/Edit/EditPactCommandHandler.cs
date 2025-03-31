using System.Security.AccessControl;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Pacts.GetBloggerPact;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Pacts.Edit;

internal sealed class EditPactCommandHandler
    : ICommandHandler<EditPactCommand, PactResponse>
{
    private readonly IPactRepository _pactRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public EditPactCommandHandler(
        IPactRepository pactRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork
    )
    {
        this._pactRepository = pactRepository;
        this._userContext = userContext;
        this._unitOfWork = unitOfWork;
    }

    public async Task<Result<PactResponse>> Handle(EditPactCommand request, CancellationToken cancellationToken)
    {
        Pact? pact = await this._pactRepository.GetByBloggerIdAsync(
            new BloggerId(this._userContext.GetUserId()),
            cancellationToken
        );

        if (pact is null)
        {
            return Result.Failure<PactResponse>(PactErrors.NotFound);
        }

        Result result = pact.Update(request.Content);

        if (result.IsFailure)
        {
            return Result.Failure<PactResponse>(result.Error);
        }

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return new PactResponse(
            pact.Id.Value,
            pact.BloggerId.Value,
            pact.Content.Value,
            []
        );
    }
}
