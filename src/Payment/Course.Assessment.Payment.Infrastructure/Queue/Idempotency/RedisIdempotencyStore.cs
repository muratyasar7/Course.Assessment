using System;
using System.Collections.Generic;
using System.Text;
using Shared.Contracts.Queue.Idempotency;
using StackExchange.Redis;

namespace Course.Assessment.Payment.Infrastructure.Queue.Idempotency
{
    public sealed class RedisIdempotencyStore : IIdempotencyStore
    {
        private readonly IDatabase _db;
        private readonly string _servicePrefix;

        public RedisIdempotencyStore(IConnectionMultiplexer connection)
        {
            _db = connection.GetDatabase();
            _servicePrefix = "payment";
        }

        public async Task<bool> ExistsAsync(Guid eventId)
        {
            return await _db.KeyExistsAsync(GetKey(eventId));
        }

        public async Task MarkProcessedAsync(Guid eventId, TimeSpan? ttl = null)
        {
            await _db.StringSetAsync(GetKey(eventId), "processed", ttl ?? TimeSpan.FromDays(7));
        }

        private string GetKey(Guid eventId) => $"idempotency:{_servicePrefix}:{eventId}";
    }
}
