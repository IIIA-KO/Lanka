using Lanka.Common.Domain;
using Lanka.Modules.Users.Domain.Users.BirthDates;
using Lanka.Modules.Users.Domain.Users.DomainEvents;
using Lanka.Modules.Users.Domain.Users.Emails;
using Lanka.Modules.Users.Domain.Users.FirstNames;
using Lanka.Modules.Users.Domain.Users.LastNames;

namespace Lanka.Modules.Users.Domain.Users;

public class User : Entity<UserId>
{
    private readonly List<Role> _roles = [];

    public FirstName FirstName { get; private set; }

    public LastName LastName { get; private set; }

    public Email Email { get; private set; }

    public BirthDate BirthDate { get; private set; }

    public string IdentityId { get; private set; }

    public DateTimeOffset? InstagramAccountLinkedOnUtc { get; private set; }

    public IReadOnlyCollection<Role> Roles => this._roles;

    private User() { }

    private User(
        UserId id,
        FirstName firstName,
        LastName lastName,
        Email email,
        BirthDate birthDate,
        string identityId
    )
    {
        this.Id = id;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.Email = email;
        this.BirthDate = birthDate;
        this.IdentityId = identityId;
    }

    public static Result<User> Create(
        string firstName,
        string lastName,
        string email,
        DateOnly birthDate,
        string identityId
    )
    {
        Result<(FirstName, LastName, Email, BirthDate)> validationResult =
            Validate(firstName, lastName, email, birthDate);

        if (validationResult.IsFailure)
        {
            return Result.Failure<User>(validationResult.Error);
        }

        (FirstName _firstName, LastName _lastName, Email _email, BirthDate _birthDate) = validationResult.Value;

        var user = new User(UserId.New(), _firstName, _lastName, _email, _birthDate, identityId);

        user._roles.Add(Role.Member);

        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));

        return user;
    }

    public Result Update(
        string firstName,
        string lastName,
        DateOnly birthDate
    )
    {
        if (this.FirstName.Value == firstName
            && this.LastName.Value == lastName
            && this.BirthDate.Value == birthDate)
        {
            return Result.Success();
        }

        this.FirstName = FirstName.Create(firstName).Value;
        this.LastName = LastName.Create(lastName).Value;
        this.BirthDate = BirthDate.Create(birthDate).Value;

        return Result.Success();
    }

    private static Result<(FirstName, LastName, Email, BirthDate)> Validate(
        string firstName,
        string lastName,
        string email,
        DateOnly birthDate
    )
    {
        Result<FirstName> firstNameResult = FirstName.Create(firstName);
        Result<LastName> lastNameResult = LastName.Create(lastName);
        Result<Email> emailResult = Email.Create(email);
        Result<BirthDate> birthDateResult = BirthDate.Create(birthDate);

        return new ValidationBuilder()
            .Add(firstNameResult)
            .Add(lastNameResult)
            .Add(emailResult)
            .Add(birthDateResult)
            .Build(() =>
                (
                    firstNameResult.Value,
                    lastNameResult.Value,
                    emailResult.Value,
                    birthDateResult.Value
                )
            );
    }

    public void Deleted()
    {
        this.RaiseDomainEvent(new UserDeletedDomainEvent(this.Id));
    }

    public void LinkInstagramAccount(string code)
    {
        this.InstagramAccountLinkedOnUtc = DateTimeOffset.UtcNow;
        this.RaiseDomainEvent(new InstagramAccountLinkedDomainEvent(this.Id, code));
    }

    public void RenewInstagramAccess(string code)
    {
        this.RaiseDomainEvent(new InstagramAccessRenewedDomainEvent(this.Id, code));
    }

    public void LoggedIn()
    {
        this.RaiseDomainEvent(new UserLoggedInDomainEvent(this.Id, DateTimeOffset.UtcNow));
    }
}
