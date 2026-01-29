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
    }
}
