using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitLog.Shared.Infrastructure;

/// <summary>
/// Применяет неприменённые миграции всех модулей при старте приложения.
/// Работает только при <see cref="ApplicationSettings.PgAutoMigrations"/> = <c>true</c>.
/// Контексты ищутся в сборках <c>FitLog.*</c> и должны быть зарегистрированы в DI (<c>AddDbContext</c>).
/// </summary>
internal sealed class DatabaseMigrationService(
    IServiceProvider serviceProvider,
    ILogger<DatabaseMigrationService> logger,
    IOptions<ApplicationSettings> applicationSettings
    ) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if(!applicationSettings.Value.PgAutoMigrations) return;
        
        using var scope = serviceProvider.CreateScope();
        
        var dbContextTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => 
                typeof(DbContext).IsAssignableFrom(t) 
                && !t.IsAbstract
                && t != typeof(DbContext))
            .ToList();

        foreach (var type in dbContextTypes)
        {
            var db = scope.ServiceProvider.GetService(type) as DbContext;
            if (db == null)
                continue;

            var name = type.Name;
            var pending = await db.Database.GetPendingMigrationsAsync(cancellationToken);

            if (pending.Any())
            {
                logger.LogInformation($"Applying migrations for {name}...");
                await db.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                logger.LogInformation($"No pending migrations for {db.GetType().Assembly}.");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
