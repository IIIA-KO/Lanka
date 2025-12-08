using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Bloggers.Categories;
using Lanka.Modules.Campaigns.Domain.Bloggers.Photos;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Bloggers;

internal static class BloggerData
{
    public static Blogger CreateBlogger()
    {
        return Blogger.Create(
            Guid.NewGuid(),
            FirstName,
            LastName,
            Email,
            BirthDate
        );
    }

    public static string FirstName = "FirstName";

    public static string LastName = "LastName";

    public static string Email = "test1@lanka.com";

    public static string Bio =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

    public static string Category = "None";

    public static DateOnly BirthDate => new(DateTime.Now.Year - 18, 1, 1);

    public static Photo OldProfilePhoto => new("old-id", new Uri("https://example.com/old-photo.jpg"));

    public static Photo NewProfilePhoto => new("new-id", new Uri("https://example.com/new-photo.jpg"));
}
