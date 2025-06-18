using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Instagram.FinishLinking;
using Lanka.Modules.Users.Application.UnitTests.Users;
using Lanka.Modules.Users.Domain.Users;
using NSubstitute;

namespace Lanka.Modules.Users.Application.UnitTests.Instagram;

#pragma warning disable CA1515
public class FinishInstagramLinkingTests
{
    private static FinishInstagramLinkingCommand Command => new(
        Guid.NewGuid(),
        "example_username",
        "1234567890"
    );

    private readonly IUserRepository _userRepositoryMock;
    private readonly IIdentityProviderService _identityProviderServiceMock;

    private readonly FinishInstagramLinkingCommandHandler _handler;

    public FinishInstagramLinkingTests()
    {
        this._userRepositoryMock = Substitute.For<IUserRepository>();
        this._identityProviderServiceMock = Substitute.For<IIdentityProviderService>();

        this._handler = new FinishInstagramLinkingCommandHandler(
            this._userRepositoryMock,
            this._identityProviderServiceMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess()
    {
        // Arrange
        User user = UserData.CreateUser();
        this._userRepositoryMock.GetByIdAsync(
            Arg.Any<UserId>(),
            Arg.Any<CancellationToken>()
        ).Returns(user);

        this._identityProviderServiceMock
            .LinkExternalAccountToUserAsync(
                user.IdentityId,
                ProviderName.Instagram,
                Command.IgId,
                Command.Username,
                Arg.Any<CancellationToken>()
            )
            .Returns(Result.Success());

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        this._userRepositoryMock.GetByIdAsync(
            Arg.Any<UserId>(),
            Arg.Any<CancellationToken>()
        ).Returns((User?)null);

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenLinkingFails()
    {
        // Arrange
        User user = UserData.CreateUser();
        this._userRepositoryMock.GetByIdAsync(
            Arg.Any<UserId>(),
            Arg.Any<CancellationToken>()
        ).Returns(user);

        this._identityProviderServiceMock.LinkExternalAccountToUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        ).Returns(IdentityProviderErrors.InvalidCredentials);

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Should().Be(IdentityProviderErrors.InvalidCredentials);
    }
}
