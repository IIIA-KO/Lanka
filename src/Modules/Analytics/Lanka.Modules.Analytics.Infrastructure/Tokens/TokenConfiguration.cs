using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens.AccessTokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Analytics.Infrastructure.Tokens;

internal sealed class TokenConfiguration : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.ToTable("tokens");

        builder.HasKey(token => token.Id);

        builder
            .Property(token => token.Id)
            .HasConversion(tokenId => tokenId.Value, value => new TokenId(value))
            .IsRequired();

        builder
            .Property(token => token.AccessToken)
            .HasConversion(
                accessToken => accessToken.Value,
                value => AccessToken.Create(value).Value
            )
            .IsRequired();

        builder
            .Property(token => token.UserId)
            .HasConversion(userId => userId.Value, value => new UserId(value))
            .IsRequired();

        builder
            .Property(token => token.InstagramAccountId)
            .HasConversion(igAccountId => igAccountId.Value, value => new InstagramAccountId(value))
            .IsRequired();

        builder
            .HasOne(token => token.InstagramAccount)
            .WithOne(ig => ig.Token)
            .HasForeignKey<Token>(token => token.InstagramAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
