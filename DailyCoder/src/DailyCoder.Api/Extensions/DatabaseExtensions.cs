using Microsoft.EntityFrameworkCore;

namespace DailyCoder.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task MigrateDatabaseAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DailyCoder.Api.Database.ApplicationDbContext>();
        try
        {
          await dbContext.Database.MigrateAsync(cancellationToken);
            app.Logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception e)
        {
            app.Logger.LogError(e, "An error occurred while migrating the database.");
            throw;
        }

    }
}
