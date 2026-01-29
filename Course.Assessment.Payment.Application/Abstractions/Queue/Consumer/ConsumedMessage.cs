namespace Course.Assessment.Payment.Application.Abstractions.Queue.Consumer
{
    public sealed class ConsumedMessage
    {
        public string MessageId { get; init; } = default!;
        public string EventType { get; init; } = default!;
        public string Payload { get; init; } = default!;
        public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
    }
}
