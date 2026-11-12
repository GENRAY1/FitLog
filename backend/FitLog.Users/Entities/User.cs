namespace FitLog.Users.Entities;

/// <summary>
/// Пользователь в БД приложения
/// здесь только то, что нужно приложению. <see cref="Id"/> совпадает с claim <c>sub</c> токена.
/// </summary>
public sealed class User
{
    public const int EmailMaxLength = 320;

    // Для EF Core.
    private User()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Создаёт профиль по данным токена. <paramref name="createdAt"/> должен быть в UTC.</summary>
    public static User Create(Guid id, string email, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User id must not be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email must not be empty.", nameof(email));
        }

        if (email.Length > EmailMaxLength)
        {
            throw new ArgumentException($"Email must not be longer than {EmailMaxLength} characters.", nameof(email));
        }

        if (createdAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Creation date must be in UTC.", nameof(createdAt));
        }

        return new User { Id = id, Email = email, CreatedAt = createdAt };
    }
}
