using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers.Bios;
using Lanka.Modules.Campaigns.Domain.Bloggers.BirthDates;
using Lanka.Modules.Campaigns.Domain.Bloggers.DomainEvents;
using Lanka.Modules.Campaigns.Domain.Bloggers.Emails;
using Lanka.Modules.Campaigns.Domain.Bloggers.FirstNames;
using Lanka.Modules.Campaigns.Domain.Bloggers.LastNames;
using Lanka.Modules.Campaigns.Domain.Bloggers.Photos;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Domain.Bloggers;

public class Blogger : Entity<BloggerId>
{
    public FirstName FirstName { get; private set; }

    public LastName LastName { get; private set; }

    public Email Email { get; private set; }

    public BirthDate BirthDate { get; private set; }

    public Bio Bio { get; private set; }

    public Pact? Pact { get; init; }

    public Photo? ProfilePhoto { get; private set; }

    public InstagramMetadata InstagramMetadata { get; private set; }

    private Blogger() { }

    private Blogger(
        BloggerId bloggerId,
        FirstName firstName,
        LastName lastName,
        Email email,
        BirthDate birthDate,
        Bio bio
    )
    {
        this.Id = bloggerId;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.Email = email;
        this.BirthDate = birthDate;
        this.Bio = bio;
        this.InstagramMetadata = new InstagramMetadata(null, null, null);
    }

    public static Blogger Create(
        Guid bloggerId,
        string firstName,
        string lastName,
        string email,
        DateOnly birthDate
    )
    {
        var blogger = new Blogger(
            new BloggerId(bloggerId),
            FirstName.Create(firstName).Value,
            LastName.Create(lastName).Value,
            Email.Create(email).Value,
            BirthDate.Create(birthDate).Value,
            Bio.Create(string.Empty).Value
        );

        blogger.RaiseDomainEvent(new BloggerCreatedDomainEvent(blogger.Id));

        return blogger;
    }

    public Result Update(
        string firstName,
        string lastName,
        DateOnly birthDate,
        string bio
    )
    {
        if (this.FirstName.Value == firstName
            && this.LastName.Value == lastName
            && this.BirthDate.Value == birthDate
            && this.Bio.Value == bio)
        {
            return Result.Success();
        }

        Result<(FirstName, LastName, BirthDate, Bio)> validationResult =
            Validate(firstName, lastName, birthDate, bio);

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        this.FirstName = validationResult.Value.Item1;
        this.LastName = validationResult.Value.Item2;
        this.BirthDate = validationResult.Value.Item3;
        this.Bio = validationResult.Value.Item4;

        this.RaiseDomainEvent(new BloggerUpdatedDomainEvent(this.Id));

        return Result.Success();
    }

    public void Delete()
    {
        this.RaiseDomainEvent(new BloggerDeletedDomainEvent(this.Id));
    }

    public void UpdateInstagramData(string? username, int? followersCount, int? mediaCount)
    {
        this.InstagramMetadata = new InstagramMetadata(username, followersCount, mediaCount);
    }

    private static Result<(FirstName, LastName, BirthDate, Bio)> Validate(
        string firstName,
        string lastName,
        DateOnly birthDate,
        string bio
    )
    {
        Result<FirstName> firstNameResult = FirstName.Create(firstName);
        Result<LastName> lastNameResult = LastName.Create(lastName);
        Result<BirthDate> birthDateResult = BirthDate.Create(birthDate);
        Result<Bio> bioResult = Bio.Create(bio);

        return new ValidationBuilder()
            .Add(firstNameResult)
            .Add(lastNameResult)
            .Add(birthDateResult)
            .Add(bioResult)
            .Build(() =>
                (
                    firstNameResult.Value,
                    lastNameResult.Value,
                    birthDateResult.Value,
                    bioResult.Value
                )
            );
    }

    public void SetProfilePhoto(Photo photo)
    {
        this.ProfilePhoto = photo;
    }

    public void RemoveProfilePhoto()
    {
        this.ProfilePhoto = null;
    }
}
