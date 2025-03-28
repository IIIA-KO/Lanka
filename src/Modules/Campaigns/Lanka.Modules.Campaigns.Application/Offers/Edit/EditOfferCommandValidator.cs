using FluentValidation;
using Lanka.Common.Contracts.Currencies;

namespace Lanka.Modules.Campaigns.Application.Offers.Edit;

internal sealed class EditOfferCommandValidator : AbstractValidator<EditOfferCommand>
{
    public EditOfferCommandValidator()
    {
        this.RuleFor(c => c.PriceAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price amount must be greater than or equal to 0");
        
        this.RuleFor(c => c.PriceCurrency)
            .IsEnumName(typeof(CurrencyCode), caseSensitive: false)
            .WithMessage("Price currency is not valid");
        
        this.RuleFor(c => c.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("Name is required");
        
        this.RuleFor(c => c.Description)
            .NotNull()
            .NotEmpty()
            .WithMessage("Description is required");
    }
}
