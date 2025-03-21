using FluentValidation;
using Lanka.Modules.Campaigns.Domain.Campaigns.Descriptions;
using Lanka.Modules.Campaigns.Domain.Campaigns.Names;

namespace Lanka.Modules.Campaigns.Application.Campaigns.Pend;

internal sealed class PendCampaignCommandValidator : AbstractValidator<PendCampaignCommand>
{
    public PendCampaignCommandValidator()
    {
        this.RuleFor(c => c.Name)
            .NotNull()
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(Name.MaxLength);
            
        this
            .RuleFor(c => c.Description)
            .NotNull()
            .NotEmpty()
            .MaximumLength(Description.MaxLength);
    }
}