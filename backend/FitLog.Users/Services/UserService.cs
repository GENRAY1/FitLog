using FitLog.Shared.Common.Auth;
using FitLog.Shared.Modules.Users.Types.Dtos;
using FitLog.Users.Abstractions;
using FitLog.Users.Entities;
using FitLog.Users.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace FitLog.Users.Services;

internal sealed class UserService(
    DatabaseContext db,
    TimeProvider timeProvider) : IUserService
{
    public Task<UserDto?> FindByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserDto(u.Id, u.Email, u.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<UserDto> GetOrCreateAsync(Guid userId, string email, CancellationToken cancellationToken)
    {
        var existing = await FindByIdAsync(userId, cancellationToken);
        
        if (existing is not null)
        {
            return existing;
        }
        

        var user = User.Create(userId, email, timeProvider.GetUtcNow());
        
        db.Users.Add(user);

        await db.SaveChangesAsync(cancellationToken);

        return new UserDto(user.Id, user.Email, user.CreatedAt);
    }
}
