using Bookify.Infrastructure.Repositories;
using Course.Assessment.Payment.Application.Abstractions.Data;
using Course.Assessment.Payment.Application.Clock;
using Course.Assessment.Payment.Domain.Abstractions;
using Course.Assessment.Payment.Domain.Payment;
using Course.Assessment.Payment.Infrastructure.Clock;
using Course.Assessment.Payment.Infrastructure.Order;
using Course.Assessment.Payment.Infrastructure.Payment;
using Course.Assessment.Payment.Infrastructure.Queue.Consumers;
using Course.Assessment.Payment.Infrastructure.Queue.Consumers.HostedServices;
using Course.Assessment.Payment.Infrastructure.Queue.DelayedPublishers;
using Course.Assessment.Payment.Infrastructure.Queue.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Analytics.Infrastructure.Outbox;
using Quartz;
using Quartz.Simpl;
using RabbitMQ.Client;
using Shared.Contracts.Events;
using Shared.Contracts.Events.Order;
using Shared.Contracts.Events.Payment;
using Shared.Contracts.Queue.Consumer;
using Shared.Contracts.Queue.Idempotency;
using Shared.Contracts.Queue.Publisher;
using Shared.Contracts.QueueMessageEventModels.v1.Order;
using StackExchange.Redis;

namespace Course.Assessment.Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        AddRedis(services,configuration);
        var queueSystem = configuration.GetSection("QueueSystem").Value;
        switch (queueSystem)
        {
            case "RabbitMq":
                AddRabbitMqConsumerDependecies(services, configuration);
                break;
            case "Kafka":
                AddKafkaConsumerDependecies(services);
                break;
            case "RedisStreams":
                AddRedisConsumerDependecies(services, configuration);
                break;
            default:
                throw new ArgumentException(nameof(queueSystem));
        }

        AddQuartz(services, configuration);

        AddIdempotecyDependecies(services);

        AddPersistence(services, configuration);

        AddConsumers(services);

        AddDelayedPublisher(services);

        AddHostedServices(services);

        AddHealthChecks(services, configuration);

        AddBackgroundJobs(services, configuration);

        return services;
    }
    private static void AddHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("PaymentDb")!);
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
        services.AddScoped<IIntegrationEventHandler<PaymentCheckIntegrationEvent>, PaymentCheckIntegrationEventHandler>();
    }

    private static void AddRabbitMqConsumerDependecies(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionFactory>(_ =>
        {
            var connection = configuration.GetConnectionString("RabbitMq") ?? throw new ArgumentNullException(nameof(configuration));
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
        services.AddScoped(typeof(IMessageConsumer<>), typeof(RabbitMqConsumer<>));
    }

    private static void AddKafkaConsumerDependecies(IServiceCollection services)
    {
        services.AddScoped(typeof(IMessageConsumer<>), typeof(KafkaConsumer<>));
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

    private static void AddRedisConsumerDependecies(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(typeof(IMessageConsumer<>), typeof(RedisStreamConsumer<>));
    }

    private static void AddHostedServices(IServiceCollection services)
    {
        services.AddScoped<OrderCreatedIntegrationEventHandler>();
        services.AddScoped<OrderCanceledIntegrationEventHandler>();
        services.AddHostedService<OrderCreatedConsumerHostedService>();
        services.AddHostedService<OrderCanceledConsumerHostedService>();
    }

    private static void AddIdempotecyDependecies(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
    }

    private static void AddQuartz(IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(q =>
        {
            q.UseJobFactory<MicrosoftDependencyInjectionJobFactory>();

            q.UsePersistentStore(s =>
            {
                s.UsePostgres(postgres =>
                {
                    var connectionString= configuration.GetConnectionString("PaymentDb") ?? throw new ArgumentNullException(nameof(configuration));
                    postgres.ConnectionString = $"{connectionString};SearchPath=quartz"; 
                    postgres.TablePrefix = "quartz.qrtz_";
                    
                });
                
                s.UseNewtonsoftJsonSerializer();
            });
        });
        services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);
    }

    private static void AddDelayedPublisher(IServiceCollection services)
    {
        services.AddScoped<IDelayedMessagePublisher, QuartzDelayedMessagePublisher>();
    }

}
