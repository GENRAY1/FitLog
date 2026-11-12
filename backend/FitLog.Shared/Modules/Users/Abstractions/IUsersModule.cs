using FitLog.Shared.Modules.Users.Types.Dtos;

namespace FitLog.Shared.Modules.Users.Abstractions;

/// <summary>
/// Публичный контракт модуля Users. Другие модули (например, Workouts) получают пользователя
/// только через него: без внешних ключей и JOIN на таблицы Users, другие модули хранят лишь <c>UserId</c>.
/// </summary>
public interface IUsersModule
{
    /// <summary>Профиль по идентификатору или <c>null</c>, если не найден.</summary>
    Task<UserDto?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
