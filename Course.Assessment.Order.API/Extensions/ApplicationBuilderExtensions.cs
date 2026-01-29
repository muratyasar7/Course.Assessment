using Course.Assessment.Order.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Course.Assessment.Order.API.Extensions;

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
