namespace FitLog.Shared.Modules.Users.Types.Dtos;

public sealed record UserDto(Guid Id, string Email, DateTimeOffset CreatedAt);
