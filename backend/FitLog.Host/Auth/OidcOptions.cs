using System.ComponentModel.DataAnnotations;

namespace FitLog.Host.Auth;

/// <summary>
/// Настройки сервера авторизации
/// секция <c>Authentication:Oidc</c>. Секретов здесь нет: клиенты public (Authorization Code + PKCE).
/// </summary>
public sealed class OidcOptions
{
    public const string SectionName = "Authentication:Oidc";

    /// <summary>
    /// Адрес издателя токенов (issuer). Должен точно совпадать с claim <c>iss</c> токенов
    /// и поддерживать OIDC discovery (<c>/.well-known/openid-configuration</c>).
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Authority { get; init; } = "";

    /// <summary>
    /// Адрес discovery-документа (<c>/.well-known/openid-configuration</c>), по которому backend забирает настройки и ключи.
    /// Задаётся явно, так как может отличаться от <see cref="Authority"/> (например, в контейнере, где <c>localhost</c>
    /// указывает не на сервер авторизации). Издатель токенов берётся из самого документа и должен совпадать с <c>iss</c>.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string MetadataAddress { get; init; } = "";

    /// <summary>Ожидаемое значение claim <c>aud</c>.</summary>
    [Required(AllowEmptyStrings = false)]
    public string Audience { get; init; } = "";

    /// <summary>
    /// Путь к ролям в токене. Одно имя (<c>roles</c>) означает claim-массив ролей, путь через точку
    /// (<c>realm_access.roles</c>) означает массив внутри JSON-объекта claim.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string RolesClaimPath { get; init; } = "roles";

    /// <summary>По умолчанию HTTPS обязателен; в Development можно отключить (сервер авторизации на http://localhost).</summary>
    public bool RequireHttpsMetadata { get; init; } = true;

    /// <summary>OAuth2-клиент для Swagger UI (только Development).</summary>
    public string SwaggerClientId { get; init; } = "";

    /// <summary>Authorization endpoint для Swagger UI (только Development), адрес берётся из discovery-документа сервера.</summary>
    public string SwaggerAuthorizationEndpoint { get; init; } = "";

    /// <summary>Token endpoint для Swagger UI (только Development), адрес берётся из discovery-документа сервера.</summary>
    public string SwaggerTokenEndpoint { get; init; } = "";
}
