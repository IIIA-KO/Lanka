using System.Net.Http.Json;

namespace Lanka.Modules.Users.Infrastructure.Identity
{
    internal sealed class KeyCloakClient
    {
        private readonly HttpClient _httpClient;

        public KeyCloakClient(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        internal async Task<string> RegisterUserAsync(UserRepresentation user, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage httpResponseMessage = await this._httpClient.PostAsJsonAsync(
                "users",
                user,
                cancellationToken);

            httpResponseMessage.EnsureSuccessStatusCode();

            return ExtractIdentityIdFromLocationHeader(httpResponseMessage);
        }

        private static string ExtractIdentityIdFromLocationHeader(
            HttpResponseMessage httpResponseMessage)
        {
            const string usersSegmentName = "users/";

            string? locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery 
                                     ?? throw new InvalidOperationException("Location header is null");

            int userSegmentValueIndex = locationHeader.IndexOf(
                usersSegmentName,
                StringComparison.InvariantCultureIgnoreCase);

            string identityId = locationHeader.Substring(userSegmentValueIndex + usersSegmentName.Length);

            return identityId;
        }
    }
}
