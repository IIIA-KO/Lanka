using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Lanka.IntegrationTests.Abstractions;
using Lanka.Modules.Users.Presentation.Users;

namespace Lanka.IntegrationTests.Users;

public class LoginTests : BaseIntegrationTest
{
    private const string Email = "login@test.com";
    private const string Password = "Pa$$w0rd";
    
    public LoginTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task Login_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var request = new Login.LoginUserRequest
        {
            Email = UserData.RegisterTestUserRequest.Email,
            Password = UserData.RegisterTestUserRequest.Password
        };

        // Act
        HttpResponseMessage response = await this._httpClient.PostAsJsonAsync("users/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new Login.LoginUserRequest
        {
            Email = Email,
            Password = Password
        };

        // Act
        HttpResponseMessage response = await this._httpClient.PostAsJsonAsync("users/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsIncorrect()
    {
        // Arrange
        var request = new Login.LoginUserRequest
        {
            Email = UserData.RegisterTestUserRequest.Email,
            Password = "WrongPassword"
        };

        // Act
        HttpResponseMessage response = await this._httpClient.PostAsJsonAsync("users/login", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
