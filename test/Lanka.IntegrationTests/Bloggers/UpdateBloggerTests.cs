using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.IntegrationTests.Abstractions;
using Lanka.IntegrationTests.Users;
using Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;
using Lanka.Modules.Campaigns.Presentation.Bloggers;
using Lanka.Modules.Users.Application.Users.GetUser;
using Lanka.Modules.Users.Application.Users.Login;

namespace Lanka.IntegrationTests.Bloggers;

public class UpdateBloggerTests : BaseIntegrationTest
{
    public UpdateBloggerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    public static readonly TheoryData<string, string, DateOnly, string> InvalidRequests = new()
    {
        { string.Empty, Faker.Name.LastName(), BloggerData.ValidBirthDate, Faker.Lorem.Sentence() },
        { Faker.Name.FirstName(), string.Empty, BloggerData.ValidBirthDate, Faker.Lorem.Sentence() },
        { Faker.Name.FirstName(), Faker.Name.LastName(), BloggerData.InvalidBirthDate, Faker.Lorem.Sentence() },
        { Faker.Name.FirstName(), Faker.Name.LastName(), BloggerData.ValidBirthDate, null! },
        { Faker.Name.FirstName(), Faker.Name.LastName(), BloggerData.ValidBirthDate, Faker.Lorem.Sentences(10) }
    };

    [Fact]
    public async Task UpdateBlogger_ShouldReturnOk()
    {
        // Wait for the blogger to be created via integration event
        AccessTokenResponse accessToken = await this.GetAccessToken();

        this._httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);

        HttpResponseMessage profileResponse = await this._httpClient.GetAsync("users/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        UserResponse? profile = await profileResponse.Content.ReadFromJsonAsync<UserResponse>();
        profile.Should().NotBeNull();

        Result<BloggerResponse> bloggerCreated = await Poller.WaitAsync(
            TimeSpan.FromSeconds(15),
            async () =>
            {
                var query = new GetBloggerQuery(profile.Id);

                Result<BloggerResponse> bloggerResult = await this._sender.Send(query);

                return bloggerResult;
            }
        );
        bloggerCreated.IsSuccess.Should().BeTrue();

        // Update user
        var request = new UpdateBlogger.UpdateBloggerRequest
        {
            FirstName = "New FirstName",
            LastName = "New LastName",
            BirthDate = UserData.RegisterTestUserRequest.BirthDate,
            Bio = "New Bio"
        };

        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"bloggers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }


    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenRequestIsInvalid(
        string firstName,
        string lastName,
        DateOnly birthDate,
        string bio
    )
    {
        // Wait for the blogger to be created via integration event
        AccessTokenResponse accessToken = await this.GetAccessToken();

        this._httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);

        HttpResponseMessage profileResponse = await this._httpClient.GetAsync("users/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        UserResponse? profile = await profileResponse.Content.ReadFromJsonAsync<UserResponse>();
        profile.Should().NotBeNull();

        Result<BloggerResponse> bloggerCreated = await Poller.WaitAsync(
            TimeSpan.FromSeconds(15),
            async () =>
            {
                var query = new GetBloggerQuery(profile.Id);

                Result<BloggerResponse> bloggerResult = await this._sender.Send(query);

                return bloggerResult;
            }
        );
        bloggerCreated.IsSuccess.Should().BeTrue();

        // Update user
        var request = new UpdateBlogger.UpdateBloggerRequest
        {
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate,
            Bio = bio
        };

        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"bloggers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_ShouldPropagateUsersModule()
    {
        // Wait for the blogger to be created via integration event
        AccessTokenResponse accessToken = await this.GetAccessToken();

        this._httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);

        HttpResponseMessage profileResponse = await this._httpClient.GetAsync("users/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        UserResponse? profile = await profileResponse.Content.ReadFromJsonAsync<UserResponse>();
        profile.Should().NotBeNull();

        Result<BloggerResponse> bloggerCreated = await Poller.WaitAsync(
            TimeSpan.FromSeconds(15),
            async () =>
            {
                var query = new GetBloggerQuery(profile.Id);

                Result<BloggerResponse> bloggerResult = await this._sender.Send(query);

                return bloggerResult;
            }
        );
        bloggerCreated.IsSuccess.Should().BeTrue();

        // Update user
        var request = new UpdateBlogger.UpdateBloggerRequest
        {
            FirstName = "New FirstName",
            LastName = "New LastName",
            BirthDate = UserData.RegisterTestUserRequest.BirthDate,
            Bio = "New Bio"
        };

        await this._httpClient.PutAsJsonAsync($"bloggers", request);

        // Get user

        Result<UserResponse> userResult = await Poller.WaitForExpectedResultAsync(
            TimeSpan.FromSeconds(15),
            async () =>
            {
                var query = new GetUserQuery(profile.Id);
                return await this._sender.Send(query);
            },
            result => result.Value.FirstName == request.FirstName
                      && result.Value.LastName == request.LastName
                      && result.Value.BirthDay == request.BirthDate
        );

        // Assert
        userResult.IsSuccess.Should().BeTrue();
        userResult.Value.Should().NotBeNull();
        userResult.Value.FirstName.Should().Be(request.FirstName);
        userResult.Value.LastName.Should().Be(request.LastName);
        userResult.Value.BirthDay.Should().Be(request.BirthDate);
    }
}
