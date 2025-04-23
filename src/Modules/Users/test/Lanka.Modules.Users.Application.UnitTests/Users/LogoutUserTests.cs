using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Users.Logout;
using Lanka.Modules.Users.Domain.Users;
using NSubstitute;

namespace Lanka.Modules.Users.Application.UnitTests.Users;

public class LogoutUserTests
{
    private static LogoutUserCommand Command => new();
    
    private readonly IUserContext _userContextMock;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IIdentityProviderService _identityProviderServiceMock;
    
    private readonly LogoutUserCommandHandler _handler;

    public LogoutUserTests()
    {
        this._userContextMock = Substitute.For<IUserContext>();
        this._userRepositoryMock = Substitute.For<IUserRepository>();
        this._identityProviderServiceMock = Substitute.For<IIdentityProviderService>();
        
        this._handler = new LogoutUserCommandHandler(
            this._userContextMock,
            this._userRepositoryMock,
            this._identityProviderServiceMock
        );
    }
    
    [Fact]
    public async Task Handle_ShouldReturnSuccess()
    {
        // Arrange
        this._userContextMock.GetUserId().Returns(Guid.NewGuid());
        
        this._userRepositoryMock.GetByIdAsync(
                Arg.Any<UserId>(), Arg.Any<CancellationToken>()
            )
            .Returns(UserData.CreateUser());
        
        this._identityProviderServiceMock.TerminateUserSessionAsync(
                Arg.Any<string>(), Arg.Any<CancellationToken>()
            )
            .Returns(Result.Success());
        
        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnUserNotFoundError_WhenUserIsNull()
    {
        // Arrange
        this._userContextMock.GetUserId().Returns(Guid.NewGuid());
        
        this._userRepositoryMock.GetByIdAsync(
                Arg.Any<UserId>(), Arg.Any<CancellationToken>()
            )
            .Returns((User?) null);
        
        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnError_WhenTerminateUserSessionFails()
    {
        // Arrange
        this._userContextMock.GetUserId().Returns(Guid.NewGuid());
        
        this._userRepositoryMock.GetByIdAsync(
                Arg.Any<UserId>(), Arg.Any<CancellationToken>()
            )
            .Returns(UserData.CreateUser());
        
        this._identityProviderServiceMock.TerminateUserSessionAsync(
                Arg.Any<string>(), Arg.Any<CancellationToken>()
            )
            .Returns(Result.Failure(IdentityProviderErrors.FailedToTerminateSession));
        
        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityProviderErrors.FailedToTerminateSession);
    }
}
