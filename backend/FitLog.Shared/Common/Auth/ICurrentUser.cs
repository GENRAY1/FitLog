namespace FitLog.Shared.Common.Auth;

/// <summary>
/// Текущий пользователь запроса. Контроллеры и сервисы берут данные отсюда,
/// а не читают claims из <c>HttpContext</c> вручную.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>Идентификатор пользователя (claim <c>sub</c>). Бросает исключение для неавторизованного запроса.</summary>
    Guid UserId { get; }

    /// <summary>Email из токена (claim <c>email</c>). Бросает исключение, если claim отсутствует или пустой.</summary>
    string Email { get; }
}
