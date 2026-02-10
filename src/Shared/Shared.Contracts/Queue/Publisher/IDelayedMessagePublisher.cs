using System;
using System.Collections.Generic;
using System.Text;
using Shared.Contracts.Events;

namespace Shared.Contracts.Queue.Publisher
{
    public interface IDelayedMessagePublisher
    {
        Task PublishAsync<T>(T message, DateTimeOffset executeAtUtc, CancellationToken ct = default) where T : IIntegrationEvent;
    }
}
