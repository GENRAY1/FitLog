using FitLog.Shared.Modules.Users.Abstractions;
using FitLog.Shared.Modules.Users.Types.Dtos;
using FitLog.Users.Abstractions;

namespace FitLog.Users.Services;

/// <summary>Реализация публичного контракта модуля (см. <see cref="IUsersModule"/>).</summary>
internal sealed class UsersModuleService(IUserService users) : IUsersModule
{
    public async Task<UserDto?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await users.FindByIdAsync(userId, cancellationToken);
    }
}
