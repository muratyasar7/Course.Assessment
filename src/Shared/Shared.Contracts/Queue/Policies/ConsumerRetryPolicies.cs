using Polly;
using Polly.Retry;

public static class ConsumerRetryPolicies
{
    public static AsyncRetryPolicy Create()
    {
        return Policy
            .Handle<Exception>(IsTransient)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    if (exception != null)
                        Console.WriteLine(exception.Message);
                    Console.WriteLine("Consumer retry {RetryCount}", retryCount);
                });
    }

    private static bool IsTransient(Exception ex)
    {
        return ex switch
        {
            Confluent.Kafka.KafkaException kafkaEx
                when kafkaEx.Error.IsError => true,

            StackExchange.Redis.RedisConnectionException => true,
            StackExchange.Redis.RedisTimeoutException => true,

            RabbitMQ.Client.Exceptions.BrokerUnreachableException => true,
            RabbitMQ.Client.Exceptions.AlreadyClosedException => true,

            TimeoutException => true,
            TaskCanceledException => true,

            _ => false
        };
    }
}
