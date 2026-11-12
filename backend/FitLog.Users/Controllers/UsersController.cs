using FitLog.Shared.Common.Auth;
using FitLog.Users.Abstractions;
using FitLog.Users.Types.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitLog.Users.Controllers;

[ApiController]
[Route("users")]
[Authorize(Policy = AuthPolicies.User)]
public sealed class UsersController(ICurrentUser currentUser, IUserService users) : ControllerBase
{
    /// <summary>Профиль текущего пользователя. При первом вызове создаётся по claims токена.</summary>
    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> GetMe(CancellationToken cancellationToken)
    {
        var user = await users.GetOrCreateAsync(currentUser.UserId, currentUser.Email, cancellationToken);
        
        return new UserResponse(user.Id, user.Email, user.CreatedAt);
    }
}
