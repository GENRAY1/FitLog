namespace FitLog.Shared.Infrastructure;

/// <summary>Общие настройки приложения (секция <c>ApplicationSettings</c>).</summary>
public class ApplicationSettings
{
    public const string SectionName = "ApplicationSettings";

    /// <summary>
    /// Применять миграции всех модулей при старте приложения. По умолчанию выключено:
    /// включается в Development (<c>appsettings.Development.json</c>), в других средах осознанно.
    /// </summary>
    public bool PgAutoMigrations { get; init; }
}
