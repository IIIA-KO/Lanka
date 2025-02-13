namespace Lanka.Common.Application.Clock
{
    public interface IDateTimeProvider
    {
        public DateTimeOffset UtcNow { get; }
    }
}
