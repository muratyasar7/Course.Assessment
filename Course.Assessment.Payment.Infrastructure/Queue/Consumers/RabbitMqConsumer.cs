using System.Text;
using System.Text.Json;
using Course.Assessment.Payment.Domain.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using Shared.Contracts.Queue.Consumer;

public sealed class RabbitMqConsumer<TEvent>
    : IMessageConsumer<TEvent>, IDisposable
    where TEvent : IIntegrationEvent
{
    private readonly IChannel _channel;
    private readonly JsonSerializerOptions _serializerOptions;

    public RabbitMqConsumer(IChannel channel)
    {
        _channel = channel;

        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task ConsumeAsync(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        var exchangeName = typeof(TEvent).Name;
        var queueName = $"worker.{exchangeName}.queue";

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 4,
            global: false);

        await _channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Fanout,
            durable: true);

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: string.Empty);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());

                var message = JsonSerializer.Deserialize<TEvent>(
                    json,
                    _serializerOptions)!;

                await handler(message, cancellationToken);

                await _channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false);
            }
            catch (OperationCanceledException)
            {
                await _channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
            catch (Exception)
            {
                await _channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: false);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer);
        await Task.Delay(Timeout.Infinite, cancellationToken);

    }

    public void Dispose()
    {
        if (_channel.IsOpen)
            _channel.CloseAsync().GetAwaiter().GetResult();
    }
}
