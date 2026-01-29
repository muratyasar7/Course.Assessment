using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared.Contracts.Events;
using Shared.Contracts.Events.Order;
using Shared.Contracts.Options;
using Shared.Contracts.Queue;
using Shared.Contracts.QueueMessageEventModels.v1.Order;

namespace Bus.Shared;

public class RabbitMqBusService(ServiceBusOption busOption) : IBusService
{
    public static Dictionary<object, string> ExchangeList = new();
    private IChannel? _channelWithAck;
    private IChannel? _channelWithNoAck;
    private IConnection? _connection;

    static RabbitMqBusService()
    {
        AddExchange<OrderCanceledIntegrationEvent>();
        AddExchange<OrderCreatedIntegrationEvent>();
    }

    private static void AddExchange<T>() where T : IIntegrationEvent
    {
        ExchangeList.Add(typeof(T), GetExchangeName<T>());
    }

    public async Task PublishWithNoAck<T>(T message) where T : IIntegrationEvent
    {
        var exchangeName = GetExchangeName<T>();

        await _channelWithNoAck!.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, true, false);

        var eventAsJsonData = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(eventAsJsonData);

        var properties = new BasicProperties { Persistent = true };

        await _channelWithNoAck.BasicPublishAsync(exchangeName, string.Empty, false, properties, body);
    }

    public Task<IChannel> CreateChannel()
    {
        return _connection!.CreateChannelAsync();
    }

    public async Task PublishWithAck<T>(T message, Dictionary<string, object>? headers = null) where T : IIntegrationEvent
    {
        var exchangeName = GetExchangeName<T>();

        await _channelWithAck!.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, true, false);

        var eventAsJsonData = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(eventAsJsonData);

        var properties = new BasicProperties { Persistent = true };
        if (headers is not null) properties.Headers = headers;

        const int maxRetries = 3;
        var attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                attempt++;
                await _channelWithAck.BasicPublishAsync(exchangeName, string.Empty, true, properties, body);
                break;
            }
            catch (Exception) when (attempt < maxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)));
            }
        }
    }

    public async Task Init()
    {
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(busOption.RabbitMqConnectionString)
        };
        _connection = await connectionFactory.CreateConnectionAsync();

        _channelWithAck = await _connection!.CreateChannelAsync(new CreateChannelOptions(true, true));
        _channelWithNoAck = await _connection!.CreateChannelAsync();
    }

    public static string GetExchangeName<T>()
    {
        return $"{typeof(T).Name.ToLower()}-exchange";
    }

    public async Task CreateExchanges()
    {
        var channel = await _connection!.CreateChannelAsync();
        foreach (var exchange in ExchangeList)
            await channel.ExchangeDeclareAsync(exchange.Value, ExchangeType.Fanout, true, false);
        await channel.DisposeAsync();
    }
}