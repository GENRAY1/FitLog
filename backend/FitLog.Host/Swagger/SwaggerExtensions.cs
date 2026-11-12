using FitLog.Host.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FitLog.Host.Swagger;

public static class SwaggerExtensions
{
    private const string SecuritySchemeName = "oauth2";

    /// <summary>Swagger с OAuth2 (Authorization Code + PKCE через сервер авторизации, настройки в <see cref="OidcOptions"/>).</summary>
    public static IServiceCollection AddSwaggerWithOAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var oidc = configuration.GetSection(OidcOptions.SectionName).Get<OidcOptions>() ?? new OidcOptions();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // Схема нужна только Swagger UI (Development): без endpoints её нет, а UseSwaggerWithOAuth сообщит об этом.
            if (Uri.TryCreate(oidc.SwaggerAuthorizationEndpoint, UriKind.Absolute, out var authorizationUrl)
                && Uri.TryCreate(oidc.SwaggerTokenEndpoint, UriKind.Absolute, out var tokenUrl))
            {
                options.AddSecurityDefinition(SecuritySchemeName, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Description = "Вход через сервер авторизации (Authorization Code + PKCE).",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = authorizationUrl,
                            TokenUrl = tokenUrl,
                            Scopes = new Dictionary<string, string>
                            {
                                ["openid"] = "OpenID Connect",
                                ["profile"] = "Профиль",
                                ["email"] = "Email",
                            },
                        },
                    },
                });
            }

            options.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerWithOAuth(this WebApplication app)
    {
        var oidc = app.Services.GetRequiredService<IOptions<OidcOptions>>().Value;
        if (string.IsNullOrWhiteSpace(oidc.SwaggerClientId)
            || string.IsNullOrWhiteSpace(oidc.SwaggerAuthorizationEndpoint)
            || string.IsNullOrWhiteSpace(oidc.SwaggerTokenEndpoint))
        {
            throw new InvalidOperationException(
                $"Swagger OAuth requires {OidcOptions.SectionName}:SwaggerClientId, " +
                $"{OidcOptions.SectionName}:SwaggerAuthorizationEndpoint and {OidcOptions.SectionName}:SwaggerTokenEndpoint.");
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.OAuthClientId(oidc.SwaggerClientId);
            options.OAuthUsePkce();
            options.OAuthScopes("openid", "profile", "email");
        });

        return app;
    }

    /// <summary>Добавляет требование авторизации (кнопка-замок) ко всем методам, кроме помеченных <c>[AllowAnonymous]</c>.</summary>
    private sealed class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
            if (metadata.OfType<IAllowAnonymous>().Any())
            {
                return;
            }

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SecuritySchemeName, context.Document)] = [],
            });
        }
    }
}
