namespace Shared.Contracts.Options
{
    public sealed class MessagePublishOptions
    {
        public string Topic { get; init; } = default!;
        public string? Key { get; init; }
        public IDictionary<string, string>? Headers { get; init; }
    }
}
