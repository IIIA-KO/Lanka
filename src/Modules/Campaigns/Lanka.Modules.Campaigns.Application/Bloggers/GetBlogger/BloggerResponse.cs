using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;

public sealed class BloggerResponse
{
    public Guid Id { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }

    public string Email { get; init; }

    public DateOnly BirthDate { get; init; }

    public string Bio { get; init; }

    public Guid? PactId { get; init; }

    public string? ProfilePhotoUri { get; init; }

    public string? InstagramUsername { get; init; }

    public int? InstagramFollowersCount { get; init; }

    public int? InstagramMediaCount { get; init; }

    public string Category { get; init; }

    public static BloggerResponse FromBlogger(Blogger blogger)
    {
        return new BloggerResponse
        {
            Id = blogger.Id.Value,
            FirstName = blogger.FirstName.Value,
            LastName = blogger.LastName.Value,
            Email = blogger.Email.Value,
            BirthDate = blogger.BirthDate.Value,
            Bio = blogger.Bio.Value,
            PactId = blogger.Pact?.Id.Value,
            ProfilePhotoUri = blogger.ProfilePhoto?.Uri.ToString(),
            InstagramUsername = blogger.InstagramMetadata.Username,
            InstagramFollowersCount = blogger.InstagramMetadata.FollowersCount,
            InstagramMediaCount = blogger.InstagramMetadata.MediaCount,
            Category = blogger.Category.Name
        };
    }
}
