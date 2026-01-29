using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Shared.Contracts;
using Shared.Contracts.Events;
using StackExchange.Redis;

namespace Course.Assessment.Payment.Infrastructure.Queue.Consumers;

public sealed class RedisStreamConsumer<TEvent> : IMessageConsumer<TEvent> where TEvent : IIntegrationEvent
{
    private readonly IConnectionMultiplexer _redis;

    private readonly string _streamName;
    private readonly string _groupName;
    private readonly string _consumerName;

    public RedisStreamConsumer(
        IConfiguration configuration,
        IConnectionMultiplexer redis)
    {
        _redis = redis;

        _streamName = configuration["Redis:StreamName"] ?? typeof(TEvent).Name;
        _groupName = configuration["Redis:GroupName"] ?? "default-group";
        _consumerName = configuration["Redis:ConsumerName"]
            ?? Environment.MachineName;
    }

    public async Task StartAsync(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();

        try
        {
            await db.StreamCreateConsumerGroupAsync(
                _streamName,
                _groupName,
                StreamPosition.NewMessages);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // group zaten var → ignore
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var entries = await db.StreamReadGroupAsync(
                    _streamName,
                    _groupName,
                    _consumerName,
                    StreamPosition.Beginning,
                    count: 10);

                if (entries.Length == 0)
                {
                    await Task.Delay(500, cancellationToken);
                    continue;
                }

                foreach (var entry in entries)
                {
                    try
                    {
                        var json = entry.Values
                            .First(x => x.Name == "data")
                            .Value
                            .ToString();

                        var message =
                            JsonSerializer.Deserialize<TEvent>(json!)!;

                        await handler(message, cancellationToken);

                        await db.StreamAcknowledgeAsync(
                            _streamName,
                            _groupName,
                            entry.Id);
                    }
                    catch
                    {
                        // ACK yok → message pending kalır
                        // retry / dead-letter burada
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // graceful shutdown
            }
            catch (Exception)
            {
                // logging
            }
        }
    }
}
