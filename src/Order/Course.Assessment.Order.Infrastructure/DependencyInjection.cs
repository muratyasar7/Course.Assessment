using System;
using Confluent.Kafka;
using Course.Assessment.Order.Application.Abstractions.Data;
using Course.Assessment.Order.Application.Clock;
using Course.Assessment.Order.Domain.Abstractions;
using Course.Assessment.Order.Domain.Order;
using Course.Assessment.Order.Infrastructure.Clock;
using Course.Assessment.Order.Infrastructure.Queue.Publisher;
using Course.Assessment.Order.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Analytics.Infrastructure.Outbox;
using Quartz;
using RabbitMQ.Client;
using Shared.Contracts.Queue.Publisher;
using StackExchange.Redis;

namespace Course.Assessment.Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        var queueSystem = configuration.GetSection("QueueSystem").Value;


        switch (queueSystem)
        {
            case "RabbitMq":
                AddRabbitMqQueue(services, configuration);
                break;
            case "Kafka":
                AddKafkaQueue(services, configuration);
                break;
            case "RedisStreams":
                AddRedisQueue(services, configuration);
                break;
            default:
                throw new ArgumentException(nameof(queueSystem));
        }

        AddPersistence(services, configuration);

        AddHealthChecks(services, configuration);

        AddBackgroundJobs(services, configuration);

        return services;
    }
    private static void AddHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("OrderDb")!);
    }

    private static void AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(options =>
        {
            configuration.GetSection("Outbox").Bind(options);
        });
        services.AddQuartz();

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.ConfigureOptions<ProcessOutboxMessagesJobSetup>();
    }
    private static void AddRabbitMqQueue(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();
        services.AddSingleton<IConnectionFactory>(_ =>
        {
            var connection = configuration.GetConnectionString("RabbitMq") ?? throw new ArgumentNullException("Rabbit Mq config missing");
            return new ConnectionFactory()
            {
                Uri = new Uri(connection)
            };
        });

        services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<IConnectionFactory>();
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddScoped(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();
            return connection.CreateChannelAsync().GetAwaiter().GetResult();
        });

    }
    private static void AddKafkaQueue(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Kafka")
            ?? throw new ArgumentNullException("Kafka connection string missing");

        services.AddSingleton(sp =>
        {
            ProducerConfig producerConfig = new()
            {
                BootstrapServers = connectionString,
                Acks = Acks.All,
                EnableIdempotence = true
            };

            return new ProducerBuilder<string, string>(producerConfig)
                .Build();
        });

        services.AddScoped<IMessagePublisher, KafkaMessagePublisher>();
    }
    private static void AddRedisQueue(IServiceCollection services, IConfiguration configuration)
    {
        Console.WriteLine(configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException(nameof(configuration)));
       
       
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConn = configuration.GetConnectionString("Redis") ??
                            throw new ArgumentNullException(nameof(configuration));
            return ConnectionMultiplexer.Connect(redisConn);
        });
        services.AddScoped<IMessagePublisher, RedisStreamMessagePublisher>();
    }
    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("OrderDb") ??
                                  throw new ArgumentNullException(nameof(configuration));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
    }

}
