namespace Lanka.Common.Infrastructure.EventBus;

public sealed class RabbitMqSettings
{
    public string Host { get; }

    public string Username { get; }

    public string Password { get; }

    public RabbitMqSettings(string connectionString)
    {
        this.Host = connectionString;

        var uri = new Uri(connectionString);
        string[] userInfo = uri.UserInfo.Split(':', 2);

        this.Username = userInfo.Length > 0 && !string.IsNullOrEmpty(userInfo[0])
            ? Uri.UnescapeDataString(userInfo[0])
            : "guest";

        this.Password = userInfo.Length > 1
            ? Uri.UnescapeDataString(userInfo[1])
            : "guest";
    }
}
