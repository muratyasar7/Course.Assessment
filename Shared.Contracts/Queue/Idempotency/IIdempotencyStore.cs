namespace Shared.Contracts.Queue.Idempotency
{
    public interface IIdempotencyStore
    {
        Task<bool> ExistsAsync(Guid eventId);
        Task MarkProcessedAsync(Guid eventId, TimeSpan? ttl = null);
    }

}
