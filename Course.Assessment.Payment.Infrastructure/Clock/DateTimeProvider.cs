using Course.Assessment.Payment.Application.Clock;

namespace Course.Assessment.Payment.Infrastructure.Clock
{
    internal sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
