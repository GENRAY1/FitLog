namespace FitLog.Shared.Common.Auth;

/// <summary>Имена политик авторизации для атрибута <c>[Authorize(Policy = ...)]</c>.</summary>
public static class AuthPolicies
{
    /// <summary>Вошедший пользователь с ролью <see cref="AuthRoles.User"/>.</summary>
    public const string User = "user";
}
