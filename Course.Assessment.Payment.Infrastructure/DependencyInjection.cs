using Bookify.Infrastructure.Repositories;
using Course.Assessment.Order.Infrastructure.Queue;
using Course.Assessment.Payment.Application.Abstractions.Data;
using Course.Assessment.Payment.Application.Abstractions.Queue;
using Course.Assessment.Payment.Application.Clock;
using Course.Assessment.Payment.Domain.Abstractions;
using Course.Assessment.Payment.Domain.Payment;
using Course.Assessment.Payment.Infrastructure.Clock;
using Course.Assessment.Payment.Infrastructure.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Analytics.Infrastructure.Outbox;
using Quartz;

namespace Course.Assessment.Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        AddPersistence(services, configuration);

        AddServiceBus(services);

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

    private static void AddServiceBus(IServiceCollection services)
    {
        services.AddScoped<IMessageBus, KafkaMessageBus>();
        services.AddScoped<IMessageBus, RabbitMqMessageBus>();
        services.AddScoped<IMessageBus, RedisStreamMessageBus>();

    }

}
