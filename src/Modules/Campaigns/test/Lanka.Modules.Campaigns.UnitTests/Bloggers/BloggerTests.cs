using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Bloggers.DomainEvents;
using Lanka.Modules.Campaigns.UnitTests.Abstractions;

namespace Lanka.Modules.Campaigns.UnitTests.Bloggers;

public class BloggerTests : BaseTest
{
    [Fact]
    public void Create_ShouldReturnBlogger()
    {
        // Act
        var blogger = Blogger.Create(
            Guid.NewGuid(),
            BloggerData.FirstName,
            BloggerData.LastName,
            BloggerData.Email,
            BloggerData.BirthDate
        );

        // Assert
        blogger.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldUpdateBlogger()
    {
        // Arrange
        var blogger = Blogger.Create(
            Guid.NewGuid(),
            BloggerData.FirstName,
            BloggerData.LastName,
            BloggerData.Email,
            BloggerData.BirthDate
        );

        // Act
        Result result = blogger.Update(
            "UpdatedFirstName",
            BloggerData.LastName,
            BloggerData.BirthDate,
            BloggerData.Bio,
            blogger.Category.Name
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        blogger.FirstName.Value.Should().Be("UpdatedFirstName");
    }

    [Fact]
    public void Update_ShouldReturnSuccess_WhenValuesAreSame()
    {
        // Arrange
        var blogger = Blogger.Create(
            Guid.NewGuid(),
            BloggerData.FirstName,
            BloggerData.LastName,
            BloggerData.Email,
            BloggerData.BirthDate
        );

        // Act
        Result result = blogger.Update(
            blogger.FirstName.Value,
            blogger.LastName.Value,
            blogger.BirthDate.Value,
            bio: string.Empty,
            blogger.Category.Name
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenValuesInvalid()
    {
        // Arrange
        var blogger = Blogger.Create(
            Guid.NewGuid(),
            BloggerData.FirstName,
            BloggerData.LastName,
            BloggerData.Email,
            BloggerData.BirthDate
        );

        // Act
        Result result = blogger.Update(
            string.Empty,
            blogger.LastName.Value,
            DateOnly.FromDateTime(DateTime.Today),
            BloggerData.Bio,
            blogger.Category.Name
        );

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldRaiseDomainEvent()
    {
        // Arrange
        var blogger = Blogger.Create(
            Guid.NewGuid(),
            BloggerData.FirstName,
            BloggerData.LastName,
            BloggerData.Email,
            BloggerData.BirthDate
        );

        // Act
        Result result = blogger.Update(
            "UpdatedFirstName",
            "UpdatedLastName",
            blogger.BirthDate.Value,
            BloggerData.Bio,
            blogger.Category.Name
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertDomainEventWasPublished<BloggerUpdatedDomainEvent>(blogger);
    }
}
