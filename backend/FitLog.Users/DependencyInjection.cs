using FitLog.Shared.Modules.Users.Abstractions;
using FitLog.Users.Abstractions;
using FitLog.Users.Infrastructure.DataAccess;
using FitLog.Users.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FitLog.Users;

public static class DependencyInjection
{
    /// <summary>Имя строки подключения модуля (<c>ConnectionStrings:Users</c>).</summary>
    public const string ConnectionStringName = "Users";

    /// <summary>Регистрирует модуль Users: БД, сервисы, контроллеры и реализацию <see cref="IUsersModule"/>.</summary>
    public static IServiceCollection AddUsers(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string ConnectionStrings:{ConnectionStringName} is not configured " +
                "(set it via user-secrets or the ConnectionStrings__Users environment variable).");

        services.AddDbContext<DatabaseContext>(options => options
            .UseNpgsql(connectionString)
            // Таблицы, колонки, ключи и индексы в snake_case (users, created_at, pk_users).
            .UseSnakeCaseNamingConvention());

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUsersModule, UsersModuleService>();
        

        return services;
    }
}
