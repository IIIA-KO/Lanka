using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Application.Users.GetUser;
using Lanka.Modules.Users.Application.Users.Update;
using Lanka.Modules.Users.Domain.Users;
using NSubstitute;

namespace Lanka.Modules.Users.Application.UnitTests.Users;

#pragma warning disable CA1515 // Type can be made internal
public class UpdateUserTests
#pragma warning restore CA1515
{
    private static UpdateUserCommand Command => new(
        Guid.NewGuid(),
        UserData.FirstName,
        UserData.LastName,
        UserData.BirthDate
    );

    private readonly IUserRepository _userRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserTests()
    {
        this._userRepositoryMock = Substitute.For<IUserRepository>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new UpdateUserCommandHandler(
            this._userRepositoryMock,
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
}
