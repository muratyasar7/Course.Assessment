using System;
using Course.Assessment.Order.Application.Clock;

namespace Course.Assessment.Order.Infrastructure.Clock
{
    internal sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
