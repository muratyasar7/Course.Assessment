using Course.Assessment.Payment.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Course.Assessment.Payment.API.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        dbContext.Database.Migrate();

        try
        {
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "QuartzTables.sql");
            if (File.Exists(scriptPath))
            {
                var quartzSql = File.ReadAllText(scriptPath);
                dbContext.Database.ExecuteSqlRaw(quartzSql);
                Console.WriteLine("Quartz schema and tables checked/created successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while creating Quartz tables: {ex.Message}");
        }
    }
}
