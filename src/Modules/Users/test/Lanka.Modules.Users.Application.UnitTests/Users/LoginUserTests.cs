using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Domain.Users;
using Lanka.Modules.Users.Domain.Users.Emails;
using NSubstitute;

namespace Lanka.Modules.Users.Application.UnitTests.Users;

public class LoginUserTests
{
    private static LoginUserCommand Command => new(UserData.Email, UserData.Password);
    
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IIdentityProviderService _identityProviderServiceMock;
    
    private readonly LoginUserCommandHandler _handler;

    public LoginUserTests()
    {
        this._userRepositoryMock = Substitute.For<IUserRepository>();
        this._identityProviderServiceMock = Substitute.For<IIdentityProviderService>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        this._handler = new LoginUserCommandHandler(
            this._userRepositoryMock,
            this._unitOfWorkMock,
            this._identityProviderServiceMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessToken()
    {
        // Arrange
        this._userRepositoryMock.GetByEmailAsync(
                Arg.Any<Email>(), Arg.Any<CancellationToken>()
            )
            .Returns(UserData.CreateUser());

        this._identityProviderServiceMock.GetAccessTokenAsync(
                Arg.Any<Email>(), Arg.Any<string>(), Arg.Any<CancellationToken>()
            )
            .Returns(UserData.Token);   
        
        // Act
        Result<AccessTokenResponse> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnUserNotFoundError_WhenUserIsNull()
    {
        // Arrange
        this._userRepositoryMock.GetByEmailAsync(
                Arg.Any<Email>(), Arg.Any<CancellationToken>()
            )
            .Returns((User?) null);
        
        // Act
        Result<AccessTokenResponse> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnInvalidCredentialsError_WhenIdentityProviderReturnsFailure()
    {
        // Arrange
        this._userRepositoryMock.GetByEmailAsync(
                Arg.Any<Email>(), Arg.Any<CancellationToken>()
            )
            .Returns(UserData.CreateUser());

        this._identityProviderServiceMock.GetAccessTokenAsync(
                Arg.Any<Email>(), 
                Arg.Any<string>(), 
                Arg.Any<CancellationToken>()
            )
            .Returns(Result.Failure<AccessTokenResponse>(IdentityProviderErrors.InvalidCredentials));
        
        // Act
        Result<AccessTokenResponse> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityProviderErrors.InvalidCredentials);
    }
}
