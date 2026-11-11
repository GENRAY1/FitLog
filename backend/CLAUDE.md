# Backend (ASP.NET Core, .NET 10)
- Nullable и ImplicitUsings включены.

## Команды (из папки `backend/`)
```bash
dotnet build FitLog.sln          # сборка
```

## Архитектура
@docs/architecture.md

## Правила
- Миграции БД (когда появится EF Core) создаёт и правит разработчик, не Claude.
- Пакеты добавлять через `dotnet add package`, версии не править вручную в `.csproj` без причины.
