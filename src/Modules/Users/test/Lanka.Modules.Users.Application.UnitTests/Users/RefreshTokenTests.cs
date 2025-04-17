using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Application.Users.RefreshToken;
using Lanka.Modules.Users.Domain.Users;
using NSubstitute;

namespace Lanka.Modules.Users.Application.UnitTests.Users;

public class RefreshTokenTests
{
    private static RefreshTokenCommand Command => new(UserData.Token.RefreshToken);

    private readonly IIdentityProviderService _identityProviderServiceMock;

    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenTests()
    {
        this._identityProviderServiceMock = Substitute.For<IIdentityProviderService>();
        this._handler = new RefreshTokenCommandHandler(this._identityProviderServiceMock);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnAccessTokenResponse()
    {
        // Arrange

        this._identityProviderServiceMock
            .RefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(UserData.Token);

        // Act
        Result<AccessTokenResponse> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(UserData.Token);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        this._identityProviderServiceMock
            .RefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AccessTokenResponse>(IdentityProviderErrors.InvalidCredentials));

        // Act
        Result<AccessTokenResponse> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityProviderErrors.InvalidCredentials);
    }
}
