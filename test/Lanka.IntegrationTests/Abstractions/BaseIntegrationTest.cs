using System.Net.Http.Json;
using Bogus;
using Lanka.IntegrationTests.Users;
using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Presentation.Users;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Lanka.IntegrationTests.Abstractions;

#pragma warning disable CA1515 // Type can be made internal
[Collection(nameof(IntegrationTestCollection))]
public class BaseIntegrationTest
#pragma warning restore CA1515
{
    private readonly IServiceScope _scope;
    protected readonly ISender _sender;
    protected static readonly Faker Faker = new();
    protected readonly HttpClient _httpClient;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        this._scope = factory.Services.CreateScope();
        this._sender = this._scope.ServiceProvider.GetRequiredService<ISender>();
        this._httpClient = factory.CreateClient();
    }

    public void Dispose()
    {
        this._scope.Dispose();
    }


    protected async Task<AccessTokenResponse> GetAccessToken()
    {
        HttpResponseMessage loginResponse = await this._httpClient.PostAsJsonAsync(
            "users/login",
            new Login.LoginUserRequest
            {
                Email = UserData.RegisterTestUserRequest.Email,
                Password = UserData.RegisterTestUserRequest.Password
            }
        );

        AccessTokenResponse? accessTokenResponse
            = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

        return accessTokenResponse;
    }
}
