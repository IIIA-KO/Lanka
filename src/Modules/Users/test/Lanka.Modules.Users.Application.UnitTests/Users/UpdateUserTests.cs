using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Users.GetUser;
using Lanka.Modules.Users.Application.Users.UpdateUser;
using Lanka.Modules.Users.Domain.Users;
using NSubstitute;

namespace Lanka.Modules.Users.Application.UnitTests.Users;

public class UpdateUserTests
{
    private static UpdateUserCommand Command => new(
        Guid.NewGuid(),
        UserData.FirstName,
        UserData.LastName,
        UserData.BirthDate
    );
    
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    
    private readonly UpdateUserCommandHandler _handler;
    
    public UpdateUserTests()
    {
        this._userRepositoryMock = Substitute.For<IUserRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        this._handler = new UpdateUserCommandHandler(
            this._userRepositoryMock,
            this._userContextMock,
            this._unitOfWorkMock
        );
    }
    
    [Fact]
    public async Task UpdateUser_ShouldReturnSuccess()
    {
        // Arrange
        User user = UserData.CreateUser();
        this._userRepositoryMock.GetByIdAsync(
                Arg.Any<UserId>(),
                Arg.Any<CancellationToken>()
            ).Returns(user);

        this._userContextMock.GetIdentityId().Returns(user.IdentityId);
        
        // Act
        Result<UserResponse> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
    
    [Fact]
    public async Task UpdateUser_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        this._userRepositoryMock.GetByIdAsync(
                Arg.Any<UserId>(),
                Arg.Any<CancellationToken>()
            ).Returns((User?)null);
        
        // Act
        Result<UserResponse> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }
    
    [Fact]
    public async Task UpdateUser_ShouldReturnFailure_WhenNotAuthorized()
    {
        // Arrange
        this._userRepositoryMock.GetByIdAsync(
            Arg.Any<UserId>(),
            Arg.Any<CancellationToken>()
        ).Returns(UserData.CreateUser());
        
        this._userContextMock.GetUserId().Returns(Guid.NewGuid());
        
        // Act
        Result<UserResponse> result = await this._handler.Handle(Command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NotAuthorized);
    }
    
    [Fact]
    public async Task UpdateUser_ShouldReturnFailure_WhenUpdateFails()
    {
        // Arrange
        this._userRepositoryMock.GetByIdAsync(
                Arg.Any<UserId>(),
                Arg.Any<CancellationToken>()
            ).Returns(UserData.CreateUser());
        
        this._userContextMock.GetUserId().Returns(Command.UserId);
        
        var command = new UpdateUserCommand(
            Command.UserId,
            string.Empty,
            string.Empty,
            UserData.BirthDate
        );
        
        // Act
        Result<UserResponse> result = await this._handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}
