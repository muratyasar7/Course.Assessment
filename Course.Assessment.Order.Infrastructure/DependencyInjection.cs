using System;
using Course.Assessment.Order.Application.Abstractions.Data;
using Course.Assessment.Order.Application.Abstractions.Queue;
using Course.Assessment.Order.Application.Clock;
using Course.Assessment.Order.Domain.Abstractions;
using Course.Assessment.Order.Domain.Order;
using Course.Assessment.Order.Infrastructure.Clock;
using Course.Assessment.Order.Infrastructure.Queue;
using Course.Assessment.Order.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Analytics.Infrastructure.Outbox;
using Quartz;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Course.Assessment.Order.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        AddQueue(services,configuration);

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

    private static void AddQueue(IServiceCollection services,IConfiguration configuration)
    {
        Console.WriteLine(configuration.GetConnectionString("RabbitMq") ?? throw new ArgumentNullException(nameof(configuration)));
        Console.WriteLine(configuration.GetConnectionString("Kafka") ?? throw new ArgumentNullException(nameof(configuration)));
        Console.WriteLine(configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException(nameof(configuration)));
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(configuration.GetConnectionString("RabbitMq") ?? throw new ArgumentNullException(nameof(configuration)))
            };
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddSingleton(sp =>
        {
            var connectionString = configuration.GetConnectionString("Kafka") ?? throw new ArgumentNullException(nameof(configuration));
            var producerConfig = new Confluent.Kafka.ProducerConfig
            {
                BootstrapServers = connectionString
            };
            return new Confluent.Kafka.ProducerBuilder<string, string>(producerConfig).Build();
        });
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConn = configuration.GetConnectionString("Redis") ??
                            throw new ArgumentNullException(nameof(configuration));
            return ConnectionMultiplexer.Connect(redisConn);
        });
        services.AddScoped<IMessageBus, KafkaMessageBus>();
        services.AddScoped<IMessageBus, RabbitMqMessageBus>();
        services.AddScoped<IMessageBus, RedisStreamMessageBus>();
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
