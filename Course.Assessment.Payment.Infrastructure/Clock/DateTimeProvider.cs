using Course.Assessment.Payment.Application.Clock;

namespace Course.Assessment.Payment.Infrastructure.Clock
{
    internal sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
