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
using Shared.Contracts.Queue;
using Shared.Contracts.Queue.Consumer;
using Shared.Contracts.QueueMessageEventModels.v1.Order;

namespace Course.Assessment.Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

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
        services.AddScoped(typeof(IMessageConsumer<>), typeof(KafkaConsumer<>));
        //services.AddScoped(typeof(IRabbitMqMessageConsumer<>), typeof(RabbitMqConsumer<>));
        //services.AddScoped(typeof(IRedisStreamConsumer<>), typeof(RedisStreamConsumer<>));
    }

    private static void AddHostedServices(IServiceCollection services)
    {
        services.AddScoped<OrderCreatedIntegrationEventHandler>();
        services.AddHostedService<OrderCreatedConsumerHostedService>();
    }

    //private static void AddKafkaConsumer(IServiceCollection services, IConfiguration configuration)
    //{
    //    var connectionString =
    //       configuration.GetConnectionString("Kafka")
    //       ?? throw new ArgumentNullException("Kafka connection string missing");
    //    services.AddSingleton(new ConsumerConfig
    //    {
    //        BootstrapServers = connectionString,
    //        GroupId = "payment-service",
    //        AutoOffsetReset = AutoOffsetReset.Earliest,
    //        EnableAutoCommit = false
    //    });

    //    services.AddScoped<IIntegrationEventRouter, IntegrationEventRouter>();

    //    services.AddHostedService<KafkaConsumer>();
    //}

}
