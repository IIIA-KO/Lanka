using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Tokens;
using Lanka.Modules.Analytics.UnitTests.Abstractions;

namespace Lanka.Modules.Analytics.UnitTests.Tokens;

#pragma warning disable CA1515
public class TokenTests : BaseTest // Consider making public types internal
{
    [Fact]
    public void Create_ShouldReturnToken()
    {
        // Act
        Result<Token> token = Token.Create(
            TokenData.UserId,
            TokenData.AccessToken,
            TokenData.ExpiresAtUtc,
            new InstagramAccountId(TokenData.InstagramAccountId)
        );

        // Assert
        token.Should().NotBeNull();
        token.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenAccessTokenIsInvalid()
    {
        // Act
        Result<Token> result = Token.Create(
            TokenData.UserId,
            string.Empty, // Invalid access token
            TokenData.ExpiresAtUtc,
            new InstagramAccountId(TokenData.InstagramAccountId)
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}
