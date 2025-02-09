using Lanka.Common.Application.Clock;

namespace Lanka.Common.Infrastructure.Clock
{
    internal sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
