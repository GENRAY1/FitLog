using FitLog.Shared.Common.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace FitLog.Host.Auth;

public static class AuthenticationExtensions
{
    /// <summary>
    /// JWT-аутентификация через сервер авторизации (OpenID Connect) и авторизация: по умолчанию все endpoints
    /// требуют вход, публичные помечаются <c>[AllowAnonymous]</c> явно.
    /// </summary>
    public static IServiceCollection AddOidcAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OidcOptions>()
            .Bind(configuration.GetSection(OidcOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Параметры берём из OidcOptions (с валидацией), а не из локальной копии конфигурации.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<OidcOptions>>((jwt, oidc) =>
            {
                var settings = oidc.Value;

                jwt.Authority = settings.Authority;
                jwt.RequireHttpsMetadata = settings.RequireHttpsMetadata;

                // Claims остаются с именами из JWT (sub, email и т. д.), а не переименовываются в ClaimTypes.*.
                jwt.MapInboundClaims = false;

                jwt.TokenValidationParameters.ValidateIssuer = true;
                jwt.TokenValidationParameters.ValidateAudience = true;
                jwt.TokenValidationParameters.ValidAudience = settings.Audience;
                jwt.TokenValidationParameters.ValidateLifetime = true;
                jwt.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);

                // Запрос без токена или с невалидным токеном получает 401 (JwtBearer не делает редирект).
                jwt.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
                        {
                            RoleClaimsMapper.MapToRoleClaims(identity, settings.RolesClaimPath);
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(AuthPolicies.User, policy => policy.RequireRole(AuthRoles.User));

        return services;
    }
}
