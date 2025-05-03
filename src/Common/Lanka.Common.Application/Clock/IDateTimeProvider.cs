namespace Lanka.Common.Application.Clock;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
