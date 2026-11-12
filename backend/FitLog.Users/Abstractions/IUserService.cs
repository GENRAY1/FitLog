using FitLog.Shared.Modules.Users.Types.Dtos;

namespace FitLog.Users.Abstractions;

public interface IUserService
{
    /// <summary>Возвращает профиль или создаёт его (lazy-создание при первом обращении).</summary>
    Task<UserDto> GetOrCreateAsync(Guid userId, string email, CancellationToken cancellationToken);

    Task<UserDto?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);
}
