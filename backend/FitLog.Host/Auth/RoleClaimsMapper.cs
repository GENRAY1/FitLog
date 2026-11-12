using System.Security.Claims;
using System.Text.Json;

namespace FitLog.Host.Auth;

/// <summary>
/// Переносит роли из токена (по пути <see cref="OidcOptions.RolesClaimPath"/>) в <see cref="ClaimTypes.Role"/>,
/// чтобы работали <c>[Authorize(Roles = ...)]</c> и политики с <c>RequireRole</c>.
/// </summary>
internal static class RoleClaimsMapper
{
    public static void MapToRoleClaims(ClaimsIdentity identity, string rolesClaimPath)
    {
        var path = rolesClaimPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (path.Length == 0)
        {
            return;
        }

        // Claim-массив (например, "roles") JwtBearer раскладывает на несколько claims одного типа.
        if (path.Length == 1)
        {
            foreach (var claim in identity.FindAll(path[0]).ToList())
            {
                AddRole(identity, claim.Value);
            }

            return;
        }

        // Вложенный путь (например, "realm_access.roles"): первый сегмент — claim с JSON-объектом.
        var json = identity.FindFirst(path[0])?.Value;
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var element = document.RootElement;
            foreach (var segment in path.Skip(1))
            {
                if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(segment, out element))
                {
                    return;
                }
            }

            if (element.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var role in element.EnumerateArray())
            {
                if (role.ValueKind == JsonValueKind.String)
                {
                    AddRole(identity, role.GetString());
                }
            }
        }
        catch (JsonException)
        {
            // Подпись токена уже проверена; claim с неожиданным форматом просто не даёт ролей.
        }
    }

    private static void AddRole(ClaimsIdentity identity, string? role)
    {
        if (!string.IsNullOrEmpty(role))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
    }
}
