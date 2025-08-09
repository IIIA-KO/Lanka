using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Tokens;
using Lanka.Modules.Analytics.Domain.Tokens.AccessTokens;
using Lanka.Modules.Analytics.Infrastructure.Encryption;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Analytics.Infrastructure.Tokens;

internal sealed class TokenConfiguration : IEntityTypeConfiguration<Token>
{
    private readonly EncryptionService _encryptionService;

    public TokenConfiguration(EncryptionService encryptionService)
    {
        this._encryptionService = encryptionService;
    }

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
                accessToken => this._encryptionService.Encrypt(accessToken.Value),
                value => AccessToken.Create(this._encryptionService.Decrypt(value)).Value
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
