using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lanka.IntegrationTests.Abstractions;
using Lanka.Modules.Users.Application.Users.GetUser;
using Lanka.Modules.Users.Application.Users.Login;

namespace Lanka.IntegrationTests.Users;

public class GetUserTests : BaseIntegrationTest
{
    public GetUserTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUser_ShouldReturnOk()
    {
        // Get user profile
        AccessTokenResponse accessToken = await this.GetAccessToken();

        this._httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);

        HttpResponseMessage profileResponse = await this._httpClient.GetAsync("users/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert
        UserResponse? profile = await profileResponse.Content.ReadFromJsonAsync<UserResponse>();
        profile.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUser_ShouldReturnUnauthorized_WhenAccessTokenIsNotProvided()
    {
        // Get user profile
        HttpResponseMessage profileResponse = await this._httpClient.GetAsync("users/profile");

        // Assert
        profileResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
