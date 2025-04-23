using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Users.RegisterUser;
using Lanka.Modules.Users.Domain.Users;
using NSubstitute;

namespace Lanka.Modules.Users.Application.UnitTests.Users;

public class RegisterUserTests
{
    private static RegisterUserCommand Command => new(
        UserData.Email,
        UserData.Password,
        UserData.FirstName,
        UserData.LastName,
        UserData.BirthDate
    );
    
    private readonly IIdentityProviderService _identityProviderServiceMock;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserTests()
    {
        this._identityProviderServiceMock = Substitute.For<IIdentityProviderService>();
        this._userRepositoryMock = Substitute.For<IUserRepository>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        this._handler = new RegisterUserCommandHandler(
            this._identityProviderServiceMock,
            this._userRepositoryMock,
            this._unitOfWorkMock
        );
    }
    
    [Fact]
    public async Task RegisterUser_ShouldReturnSuccess()
    {
        // Arrange
        this._identityProviderServiceMock.RegisterUserAsync(
                Arg.Any<UserModel>(),
                Arg.Any<CancellationToken>()
            ).Returns(UserData.IdentityId);
        
        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task RegisterUser_ShouldReturnFailure_WhenIdentityProviderFails()
    {
        // Arrange
        this._identityProviderServiceMock.RegisterUserAsync(
                Arg.Any<UserModel>(),
                Arg.Any<CancellationToken>()
            ).Returns(Result.Failure<string>(IdentityProviderErrors.EmailIsNotUnique));
        
        // Act
        Result<Guid> result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IdentityProviderErrors.EmailIsNotUnique);
    }
    
    [Fact]
    public async Task RegisterUser_ShouldReturnFailure_WhenUserCreationFails()
    {
        // Arrange
        this._identityProviderServiceMock.RegisterUserAsync(
                Arg.Any<UserModel>(),
                Arg.Any<CancellationToken>()
            ).Returns(UserData.IdentityId);

        var command = new RegisterUserCommand(
            string.Empty, 
            "123",
            UserData.FirstName,
            UserData.LastName,
            UserData.BirthDate
        );
        
        // Act
        Result<Guid> result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}
