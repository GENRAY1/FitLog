namespace FitLog.Shared.Common.Auth;

/// <summary>
/// Роли приложения. Совпадают с ролями в токене сервера авторизации (путь задаёт <c>Authentication:Oidc:RolesClaimPath</c>).
/// Чтобы добавить роль, заведите константу здесь и политику в <see cref="AuthPolicies"/>.
/// </summary>
public static class AuthRoles
{
    public const string User = "user";
}
