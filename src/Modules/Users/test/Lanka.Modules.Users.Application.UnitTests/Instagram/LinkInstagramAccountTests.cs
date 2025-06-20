using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Instagram.Link;
using Lanka.Modules.Users.Application.UnitTests.Users;
using Lanka.Modules.Users.Domain.Users;
using NSubstitute;

namespace Lanka.Modules.Users.Application.UnitTests.Instagram;

#pragma warning disable CA1515
public class LinkInstagramAccountTests
{
    private static LinkInstagramAccountCommand Command => new("test_code_value");

    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IIdentityProviderService _identityProviderServiceMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly LinkInstagramAccountCommandHandler _handler;

    public LinkInstagramAccountTests()
    {
        this._userRepositoryMock = Substitute.For<IUserRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._identityProviderServiceMock = Substitute.For<IIdentityProviderService>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new LinkInstagramAccountCommandHandler(
            this._userRepositoryMock,
            this._userContextMock,
            this._identityProviderServiceMock,
            this._unitOfWorkMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAccountNotYetLinked()
    {
        // Arrange
        User user = UserData.CreateUser();
        Guid userId = user.Id.Value;

        this._userContextMock.GetUserId().Returns(userId);

        this._userRepositoryMock.GetByIdAsync(
            Arg.Is<UserId>(id => id.Value == userId),
            Arg.Any<CancellationToken>()
        ).Returns(user);

        this._identityProviderServiceMock.IsExternalAccountLinkedAsync(
            user.IdentityId,
            ProviderName.Instagram,
            Arg.Any<CancellationToken>()
        ).Returns(false);

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await this._unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAccountAlreadyLinked()
    {
        // Arrange
        User user = UserData.CreateUser();
        Guid userId = user.Id.Value;

        this._userContextMock.GetUserId().Returns(userId);

        this._userRepositoryMock.GetByIdAsync(
            Arg.Is<UserId>(id => id.Value == userId),
            Arg.Any<CancellationToken>()
        ).Returns(user);

        this._identityProviderServiceMock.IsExternalAccountLinkedAsync(
            user.IdentityId,
            ProviderName.Instagram,
            Arg.Any<CancellationToken>()
        ).Returns(true);

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityProviderErrors.ExternalIdentityProviderAlreadyLinked);
        await this._unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
