using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Shared.Contracts;
using Shared.Contracts.Events;

namespace Course.Assessment.Payment.Infrastructure.Queue.Consumers;

public sealed class KafkaConsumer<TEvent> : IMessageConsumer<TEvent>, IDisposable where TEvent : IIntegrationEvent
{
    private readonly IConfiguration _configuration;
    private IConsumer<Ignore, string>? _consumer;

    public KafkaConsumer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task StartAsync(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"] ?? typeof(TEvent).Name,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

        var topicName = typeof(TEvent).Name;
        _consumer.Subscribe(topicName);

        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);
                    if (result?.Message?.Value is null)
                        continue;

                    var message =
                        JsonSerializer.Deserialize<TEvent>(result.Message.Value)!;

                    await handler(message, cancellationToken);

                    _consumer.Commit(result); // ACK
                }
                catch (OperationCanceledException)
                {
                    // graceful shutdown
                }
                catch (Exception)
                {
                    // logging + DLQ topic burada olur
                }
            }
        }, cancellationToken);
    }

    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
    }
}
