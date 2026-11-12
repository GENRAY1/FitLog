using System.Security.Claims;
using FitLog.Shared.Common.Auth;
using Microsoft.AspNetCore.Http;

namespace FitLog.Shared.Infrastructure.Auth;

/// <summary>Реализация <see cref="ICurrentUser"/> поверх <c>HttpContext.User</c>.</summary>
internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid UserId =>
        Guid.TryParse(Principal?.FindFirst(ClaimNames.Subject)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("The authenticated user has no email (claim sub is missing or invalid).");

    public string Email =>
        Principal?.FindFirst(ClaimNames.Email)?.Value is { Length: > 0 } email
            ? email
            : throw new InvalidOperationException("The authenticated user has no email (claim email is missing or empty).");
}
