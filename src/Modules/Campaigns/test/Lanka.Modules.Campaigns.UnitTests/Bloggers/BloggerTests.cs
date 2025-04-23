using FluentAssertions;
using Lanka.Modules.Campaigns.Domain.Bloggers;
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
        blogger.Update(
            "UpdatedFirstName",
            BloggerData.LastName,
            BloggerData.BirthDate
        );

        // Assert
        blogger.Should().NotBeNull();
        blogger.FirstName.Value.Should().Be("UpdatedFirstName");
    }
}
