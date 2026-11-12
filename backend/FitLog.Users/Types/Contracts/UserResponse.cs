namespace FitLog.Users.Types.Contracts;

/// <summary>Ответ <c>GET /users/me</c>.</summary>
public sealed record UserResponse(Guid Id, string Email, DateTimeOffset CreatedAt);
