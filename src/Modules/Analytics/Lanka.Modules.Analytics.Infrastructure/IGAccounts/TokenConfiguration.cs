using Lanka.Modules.Analytics.Domain.IGAccounts;
using Lanka.Modules.Analytics.Domain.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Analytics.Infrastructure.IGAccounts;

internal sealed class TokenConfiguration : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.ToTable("tokens");
        
        builder.HasKey(token => token.Id);

        builder
            .Property(token => token.BloggerId)
            .HasConversion(bloggerId => bloggerId.Value, value => new BloggerId(value))
            .IsRequired();
    }
}
