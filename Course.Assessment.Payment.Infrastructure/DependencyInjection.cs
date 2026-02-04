using Bookify.Infrastructure.Repositories;
using Confluent.Kafka;
using Course.Assessment.Payment.Application.Abstractions.Data;
using Course.Assessment.Payment.Application.Clock;
using Course.Assessment.Payment.Domain.Abstractions;
using Course.Assessment.Payment.Domain.Payment;
using Course.Assessment.Payment.Infrastructure.Clock;
using Course.Assessment.Payment.Infrastructure.Order;
using Course.Assessment.Payment.Infrastructure.Queue.Consumers;
using Course.Assessment.Payment.Infrastructure.Queue.Consumers.HostedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Analytics.Infrastructure.Outbox;
using Quartz;
using Shared.Contracts.Events;
using Shared.Contracts.Events.Order;
using Shared.Contracts.Queue;
using Shared.Contracts.Queue.Consumer;
using Shared.Contracts.QueueMessageEventModels.v1.Order;
using StackExchange.Redis;

namespace Course.Assessment.Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        AddRedis(services, configuration);

        AddPersistence(services, configuration);

        AddConsumers(services);

        AddHostedServices(services);

        AddHealthChecks(services, configuration);

        AddBackgroundJobs(services, configuration);

        return services;
    }
    private static void AddHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("PaymentDb")!);
        //TODO: Add More
    }

    private static void AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
    {
        var conf = configuration.GetSection("Outbox");
        services.Configure<OutboxOptions>(options =>
        {
            configuration.GetSection("Outbox").Bind(options);
        });

        services.AddQuartz();

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.ConfigureOptions<ProcessOutboxMessagesJobSetup>();
    }
    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("PaymentDb") ??
                                  throw new ArgumentNullException(nameof(configuration));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
    }


    private static void AddConsumers(IServiceCollection services)
    {
        
        services.AddScoped<IIntegrationEventHandler<OrderCreatedIntegrationEvent>, OrderCreatedIntegrationEventHandler>();
        services.AddScoped<IIntegrationEventHandler<OrderCanceledIntegrationEvent>, OrderCanceledIntegrationEventHandler>();
        //services.AddScoped(typeof(IMessageConsumer<>), typeof(KafkaConsumer<>));
        services.AddScoped(typeof(IMessageConsumer<>), typeof(RedisStreamConsumer<>));
        //services.AddScoped(typeof(IRabbitMqMessageConsumer<>), typeof(RabbitMqConsumer<>));
        //services.AddScoped(typeof(IRedisStreamConsumer<>), typeof(RedisStreamConsumer<>));
    }

    private static void AddRedis(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConn = configuration.GetConnectionString("Redis") ??
                            throw new ArgumentNullException(nameof(configuration));
            return ConnectionMultiplexer.Connect(redisConn);
        });
    }

    private static void AddHostedServices(IServiceCollection services)
    {
        services.AddScoped<OrderCreatedIntegrationEventHandler>();
        services.AddScoped<OrderCanceledIntegrationEventHandler>();
        services.AddHostedService<OrderCreatedConsumerHostedService>();
        services.AddHostedService<OrderCanceledConsumerHostedService>();
    }

}
