using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.IntegrationTests.Abstractions;
using Lanka.Modules.Campaigns.Application.Bloggers.GetBlogger;
using Lanka.Modules.Users.Application.Users.GetUser;
using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Presentation.Users;

namespace Lanka.IntegrationTests.Users;

public class UpdateUserTests : BaseIntegrationTest
{
    public UpdateUserTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    public static readonly TheoryData<string, string, DateOnly> InvalidRequests = new()
    {
        { string.Empty, Faker.Name.LastName(), UserData.ValidBirthDate },
        { Faker.Name.FirstName(), string.Empty, UserData.ValidBirthDate },
        { Faker.Name.FirstName(), Faker.Name.LastName(), DateOnly.FromDateTime(DateTime.Today) }
    };

    [Fact]
    public async Task UpdateUser_ShouldReturnOk()
    {
        // Get user profile
        AccessTokenResponse accessToken = await this.GetAccessToken();

        this._httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);

        HttpResponseMessage profileResponse = await this._httpClient.GetAsync("users/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Update user
        UserResponse? profile = await profileResponse.Content.ReadFromJsonAsync<UserResponse>();
        profile.Should().NotBeNull();

        var request = new UpdateUser.UpdateUserRequest
        {
            FirstName = "New FirstName",
            LastName = "New LastName",
            BirthDate = UserData.RegisterTestUserRequest.BirthDate
        };

        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"users/{profile.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Get user profile
        AccessTokenResponse accessToken = await this.GetAccessToken();

        this._httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);

        // Update user
        var request = new UpdateUser.UpdateUserRequest
        {
            FirstName = "New FirstName",
            LastName = "New LastName",
            BirthDate = UserData.RegisterTestUserRequest.BirthDate
        };

        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"users/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenRequestIsInvalid(
        string firstName,
        string lastName,
        DateOnly birthDate
    )
    {
        // Arrange
        AccessTokenResponse accessToken = await this.GetAccessToken();

        this._httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);

        HttpResponseMessage profileResponse = await this._httpClient.GetAsync("users/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        UserResponse? profile = await profileResponse.Content.ReadFromJsonAsync<UserResponse>();
        profile.Should().NotBeNull();

        var request = new UpdateUser.UpdateUserRequest
        {
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate
        };

        // Act
        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"users/{profile.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_ShouldPropagateCampaignsModule()
    {
        // Arrange
        AccessTokenResponse accessToken = await this.GetAccessToken();

        this._httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);

        HttpResponseMessage profileResponse = await this._httpClient.GetAsync("users/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        UserResponse? profile = await profileResponse.Content.ReadFromJsonAsync<UserResponse>();
        profile.Should().NotBeNull();

        var request = new UpdateUser.UpdateUserRequest
        {
            FirstName = "New FirstName",
            LastName = "New LastName",
            BirthDate = UserData.RegisterTestUserRequest.BirthDate
        };

        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"users/{profile.Id}", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get blogger
        Result<BloggerResponse> bloggerResult = await Poller.WaitAsync(
            TimeSpan.FromSeconds(15),
            async () =>
            {
                var query = new GetBloggerQuery(profile.Id);

                Result<BloggerResponse> bloggerResult = await this._sender.Send(query);

                return bloggerResult;
            }
        );

        // Assert
        bloggerResult.IsSuccess.Should().BeTrue();
        bloggerResult.Value.Should().NotBeNull();
        bloggerResult.Value.FirstName.Should().Be(request.FirstName);
        bloggerResult.Value.LastName.Should().Be(request.LastName);
        bloggerResult.Value.BirthDate.Should().Be(request.BirthDate);
    }
}
