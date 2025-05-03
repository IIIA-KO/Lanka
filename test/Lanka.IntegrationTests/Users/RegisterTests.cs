using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.IntegrationTests.Abstractions;
using Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;
using Lanka.Modules.Users.Presentation.Users;

namespace Lanka.IntegrationTests.Users;

public class RegisterTests : BaseIntegrationTest
{
    public RegisterTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    public static readonly TheoryData<string, string, string, string, DateOnly> InvalidRequests = new()
    {
        { string.Empty, Faker.Name.LastName(), Faker.Internet.Email(), UserData.Password, UserData.ValidBirthDate },
        { Faker.Name.FirstName(), string.Empty, Faker.Internet.Email(), UserData.Password, UserData.ValidBirthDate },
        { Faker.Name.FirstName(), Faker.Name.LastName(), string.Empty, UserData.Password, UserData.ValidBirthDate },
        { Faker.Name.FirstName(), Faker.Name.LastName(), Faker.Internet.Email(), "weakpassword", UserData.ValidBirthDate },
        { Faker.Name.FirstName(), Faker.Name.LastName(), Faker.Internet.Email(), UserData.Password, DateOnly.FromDateTime(DateTime.Today) },
    };
    
    [Fact]
    public async Task Register_ShouldReturnOk()
    {
        // Arrange
        var request = new RegisterUser.RegisterUserRequest
        {
            FirstName = UserData.RegisterTestUserRequest.FirstName,
            LastName = UserData.RegisterTestUserRequest.LastName,
            Email = Faker.Internet.Email(),
            Password = UserData.RegisterTestUserRequest.Password,
            BirthDate = UserData.RegisterTestUserRequest.BirthDate
        };

        // Act
        HttpResponseMessage response = await this._httpClient.PostAsJsonAsync("users/register", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailIsAlreadyTaken()
    {
        // Arrange
        var request = new RegisterUser.RegisterUserRequest
        {
            FirstName = UserData.RegisterTestUserRequest.FirstName,
            LastName = UserData.RegisterTestUserRequest.LastName,
            Email = UserData.RegisterTestUserRequest.Email,
            Password = UserData.RegisterTestUserRequest.Password,
            BirthDate = UserData.RegisterTestUserRequest.BirthDate
        };
        
        // Act
        HttpResponseMessage response = await this._httpClient.PostAsJsonAsync("users/register", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async Task Register_ShouldReturnBadRequest_WhenRequestIsInvalid(
        string firstName,
        string lastName,
        string email,
        string password,
        DateOnly birthDate
    )
    {
        // Arrange
        var request = new RegisterUser.RegisterUserRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = password,
            BirthDate = birthDate
        };

        // Act
        HttpResponseMessage response = await this._httpClient.PostAsJsonAsync("users/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldPropagateCampaignsModule()
    {
        // Register user
        var request = new RegisterUser.RegisterUserRequest
        {
            FirstName = UserData.RegisterTestUserRequest.FirstName,
            LastName = UserData.RegisterTestUserRequest.LastName,
            Email = Faker.Internet.Email(),
            Password = UserData.RegisterTestUserRequest.Password,
            BirthDate = UserData.RegisterTestUserRequest.BirthDate
        };

        HttpResponseMessage response = await this._httpClient.PostAsJsonAsync("users/register", request);

        Guid userId = await response.Content.ReadFromJsonAsync<Guid>();

        response.IsSuccessStatusCode.Should().BeTrue();

        // Get blogger
        Result<BloggerResponse> bloggerResult = await Poller.WaitAsync(
            TimeSpan.FromSeconds(15),
            async () =>
            {
                var query = new GetBloggerQuery(userId);

                Result<BloggerResponse> bloggerResult = await this._sender.Send(query);

                return bloggerResult;
            }
        );

        // Assert
        bloggerResult.IsSuccess.Should().BeTrue();
        bloggerResult.Value.Should().NotBeNull();
    }
}
