using Lanka.Modules.Campaigns.Domain.Bloggers;

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

    public static DateOnly BirthDate => new(DateTime.Now.Year - 18, 1, 1);
}
