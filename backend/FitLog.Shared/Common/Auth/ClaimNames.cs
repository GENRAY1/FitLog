namespace FitLog.Shared.Common.Auth;

/// <summary>
/// Имена стандартных claims access token (OpenID Connect). Входящие claims не переименовываются
/// (<c>MapInboundClaims = false</c>), поэтому имена совпадают с JWT.
/// </summary>
public static class ClaimNames
{
    /// <summary>Идентификатор пользователя на сервере авторизации.</summary>
    public const string Subject = "sub";

    public const string Email = "email";
}
