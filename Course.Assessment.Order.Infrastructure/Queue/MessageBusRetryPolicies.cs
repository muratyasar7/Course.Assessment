using System;
using Polly;
using Polly.Retry;

public static class MessageBusRetryPolicies
{
    public static AsyncRetryPolicy Create()
    {
        return Policy
            .Handle<Exception>(IsTransient)
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static bool IsTransient(Exception ex)
    {
        return ex switch
        {
            // Kafka
            Confluent.Kafka.ProduceException<string, string> => true,

            // Redis
            StackExchange.Redis.RedisConnectionException => true,
            StackExchange.Redis.RedisTimeoutException => true,

            // RabbitMQ
            RabbitMQ.Client.Exceptions.BrokerUnreachableException => true,
            RabbitMQ.Client.Exceptions.AlreadyClosedException => true,

            _ => false
        };
    }
}
