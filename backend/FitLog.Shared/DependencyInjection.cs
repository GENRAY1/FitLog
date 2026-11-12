using FitLog.Shared.Common.Auth;
using FitLog.Shared.Infrastructure;
using FitLog.Shared.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitLog.Shared;

public static class DependencyInjection
{
    /// <summary>Регистрирует общие сервисы, нужные нескольким модулям.</summary>
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.Configure<ApplicationSettings>(configuration.GetSection(ApplicationSettings.SectionName));
        services.AddHostedService<DatabaseMigrationService>();
        return services;
    }
}
