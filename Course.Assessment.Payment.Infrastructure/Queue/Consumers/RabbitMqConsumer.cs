using System.Text;
using System.Text.Json;
using Bus.Shared;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts;
using Shared.Contracts.Events;
using Shared.Contracts.Queue;
public sealed class RabbitMqConsumer<TEvent> : IMessageConsumer<TEvent> where TEvent : IIntegrationEvent
{
    private readonly IBusService _busService;
    private IChannel? _channel;

    public RabbitMqConsumer(IBusService busService)
    {
        _busService = busService;
    }

    public async Task StartAsync(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        _channel = await _busService.CreateChannel();

        await _channel.BasicQosAsync(0, 4, true, cancellationToken);

        var queueName = $"worker.{typeof(TEvent).Name}.queue";
        var exchangeName = RabbitMqBusService.GetExchangeName<TEvent>();

        await _channel.QueueDeclareAsync(queueName, true, false, false, cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(queueName, exchangeName, string.Empty, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<TEvent>(json)!;

                await handler(message, cancellationToken);

                await _channel.BasicAckAsync(args.DeliveryTag, false);
            }
            catch
            {
                await _channel.BasicRejectAsync(args.DeliveryTag, true);
            }
        };

        await _channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken);
    }
}
