using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Course.Assessment.Order.Application.Abstractions.Queue;
using Course.Assessment.Order.Domain.Options;
using Polly.Retry;
using RabbitMQ.Client;
using Shared.Contracts.Events;

namespace Course.Assessment.Order.Infrastructure.Queue
{
    public sealed class RabbitMqMessageBus : IMessageBus, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly AsyncRetryPolicy _retryPolicy;

        public RabbitMqMessageBus(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _retryPolicy = MessageBusRetryPolicies.Create();
        }

        public static Task<RabbitMqMessageBus> CreateAsync(IConnection connection, CancellationToken cancellationToken = default)
        {
            // CreateModel is sync; keep factory for symmetry
            return Task.FromResult(new RabbitMqMessageBus(connection));
        }

        public async Task PublishAsync<T>(
            T message,
            MessagePublishOptions options,
            CancellationToken cancellationToken = default)
            where T : IIntegrationEvent
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (options == null) throw new ArgumentNullException(nameof(options));
            cancellationToken.ThrowIfCancellationRequested();

            await _retryPolicy.ExecuteAsync(async ct =>
            {
                // Serialize outside sync publish
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                // Create a short-lived channel for this publish.
                // We use 'var' so the IModel type is not part of the public API.
                using var channel = await _connection.CreateChannelAsync();

                // Create properties synchronously
                //var properties = channel.CreateBasicProperties();
                //properties.MessageId = message.EventId.ToString();
                //properties.Type = message.EventType;
                //properties.Timestamp = new AmqpTimestamp(new DateTimeOffset(message.OccurredAt).ToUnixTimeSeconds());

                //if (options.Headers != null && options.Headers.Any())
                //{
                //    properties.Headers = options.Headers.ToDictionary(k => k.Key, v => (object)v.Value);
                //}

                // BasicPublish is synchronous. Run on thread-pool to avoid blocking caller.
                await Task.Run(async () =>
                {
                    ct.ThrowIfCancellationRequested();
                    await channel.BasicPublishAsync(
                        exchange: options.Destination,
                        routingKey: string.Empty,
                        mandatory: false,
                        body: body);
                }, ct);

            }, cancellationToken).ConfigureAwait(false);
        }

        // If this class does not own the connection, do not close it here.
        // Implement IAsyncDisposable so callers can dispose this bus if they want.
        public ValueTask DisposeAsync()
        {
            // Nothing to dispose here because we don't own the connection.
            // If you want the bus to also close the connection, do it explicitly here.
            return ValueTask.CompletedTask;
        }
    }
}