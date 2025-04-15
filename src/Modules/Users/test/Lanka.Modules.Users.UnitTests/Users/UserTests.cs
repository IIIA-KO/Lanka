using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Domain.Users;
using Lanka.Modules.Users.Domain.Users.BirthDates;
using Lanka.Modules.Users.Domain.Users.DomainEvents;
using Lanka.Modules.Users.UnitTests.Abstractions;

namespace Lanka.Modules.Users.UnitTests.Users;

public class UserTests : BaseTest
{
    [Fact]
    public void Create_ShouldReturnUser()
    {
        // Act
        Result<User> user = User.Create(
            UserData.FirstName,
            UserData.LastName,
            UserData.Email,
            UserData.BirthDate,
            Guid.NewGuid().ToString()
        );

        // Assert
        user.Should().NotBeNull();
        user.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnUser_WithMemberRole()
    {
        // Act
        User user = User.Create(
            UserData.FirstName,
            UserData.LastName,
            UserData.Email,
            UserData.BirthDate,
            Guid.NewGuid().ToString()
        ).Value;

        // Assert
        user.Roles.Single().Should().Be(Role.Member);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenFirstNameIsInvalid()
    {
        // Act
        Result<User> result = User.Create(
            string.Empty,
            UserData.LastName,
            UserData.Email,
            UserData.BirthDate,
            Guid.NewGuid().ToString()
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenLastNameIsInvalid()
    {
        // Act
        Result<User> result = User.Create(
            UserData.FirstName,
            string.Empty,
            UserData.Email,
            UserData.BirthDate,
            Guid.NewGuid().ToString()
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenEmailIsInvalid()
    {
        // Act
        Result<User> result = User.Create(
            UserData.FirstName,
            UserData.LastName,
            string.Empty,
            UserData.BirthDate,
            Guid.NewGuid().ToString()
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenUserIsUnderage()
    {
        // Arrange
        var underageBirthDate = DateOnly.FromDateTime(
            DateTime.Now.AddYears(BirthDate.MinimumAge - 1)
        );

        // Act
        Result<User> result = User.Create(
            UserData.FirstName,
            UserData.LastName,
            UserData.Email,
            underageBirthDate,
            Guid.NewGuid().ToString()
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent_WhenUserCreated()
    {
        // Act
        User user = User.Create(
            Faker.Person.FirstName,
            Faker.Person.LastName,
            Faker.Internet.Email(),
            DateOnly.FromDateTime(Faker.Person.DateOfBirth.Date),
            Guid.NewGuid().ToString()
        ).Value;

        // Assert
        UserCreatedDomainEvent domainEvent =
            AssertDomainEventWasPublished<UserCreatedDomainEvent>(user);

        domainEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void Update_ShouldRaiseDomainEvent_WhenUserUpdated()
    {
        // Arrange
        User user = User.Create(
            Faker.Person.FirstName,
            Faker.Person.LastName,
            Faker.Internet.Email(),
            DateOnly.FromDateTime(Faker.Person.DateOfBirth.Date),
            Guid.NewGuid().ToString()
        ).Value;


        // Act
        user.Update("new-firstname", user.LastName.Value, user.BirthDate.Value);

        // Assert
        UserUpdatedDomainEvent domainEvent =
            AssertDomainEventWasPublished<UserUpdatedDomainEvent>(user);

        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.FirstName.Should().Be(user.FirstName.Value);
        domainEvent.LastName.Should().Be(user.LastName.Value);
        domainEvent.BirthDate.Should().Be(user.BirthDate.Value);
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenFirstNameIsInvalid()
    {
        // Arrange
        User user = User.Create(
            UserData.FirstName,
            UserData.LastName,
            UserData.Email,
            UserData.BirthDate,
            Guid.NewGuid().ToString()
        ).Value;

        // Act
        Result result = user.Update(string.Empty, user.LastName.Value, user.BirthDate.Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenLastNameIsInvalid()
    {
        // Arrange
        User user = User.Create(
            UserData.FirstName,
            UserData.LastName,
            UserData.Email,
            UserData.BirthDate,
            Guid.NewGuid().ToString()
        ).Value;

        // Act
        Result result = user.Update(user.FirstName.Value, string.Empty, user.BirthDate.Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void Update_ShouldNotRaiseDomainEvent_WhenUserNotUpdated()
    {
        // Arrange
        User user = User.Create(
            Faker.Person.FirstName,
            Faker.Person.LastName,
            Faker.Internet.Email(),
            DateOnly.FromDateTime(Faker.Person.DateOfBirth.Date),
            Guid.NewGuid().ToString()
        ).Value;

        user.ClearDomainEvents();

        // Act
        Result result = user.Update(user.FirstName.Value, user.LastName.Value, user.BirthDate.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.DomainEvents.Should().BeEmpty();
    }
}
