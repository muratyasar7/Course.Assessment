using System.Collections.Generic;

namespace Course.Assessment.Order.Domain.Options
{
    public sealed class MessagePublishOptions
    {
        public string Destination { get; init; } = default!;
        public string? Key { get; init; }
        public IDictionary<string, string>? Headers { get; init; }
    }
}
