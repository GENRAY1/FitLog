# Архитектура backend

Модульный монолит: один процесс (`FitLog.Host`), независимые модули, у каждого своя БД.

## Стек

- .NET 10, C# (`Nullable`, `ImplicitUsings`), ASP.NET Core Web API на контроллерах.
- PostgreSQL, EF Core + Npgsql.
- Swagger (Swashbuckle).
- Пакеты централизованно: `Directory.Packages.props`, `Directory.Build.props`.

## Структура

```
backend/
  FitLog.Host/            # Program.cs, подключение модулей, Swagger, middleware, ошибки, логирование
  FitLog.Shared/
    Modules/<Module>/     # публичный контракт модуля
      Abstractions/       # I<Module>Module.cs
      Types/Dtos/         # DTO для вызовов между модулями
    Common/               # общее для всех модулей
    Infrastructure/       # реализации общих технических сервисов (если надо)
    DependencyInjection.cs
  FitLog.<Module>/        # например, FitLog.Workouts
    Entities/             # сущности БД
    Controllers/          # контроллеры модуля
      Contracts/          # Request/Response контроллеров
    Features/<Feature>/
      Abstractions/       # интерфейсы сервисов
      Services/           # реализации
      Types/
        Dtos/             # DTO сервисов
    Infrastructure/
      DataAccess/         # DatabaseContext, Configurations/, Migrations/, SeedingData/
      ExternalApis/       # клиенты сторонних API
    Common/               # общее для фич модуля
    DependencyInjection.cs  # Add<Module>(services, configuration)
```


## Правила

**Модули**
- `.csproj` модуля ссылается только на `FitLog.Shared`, не на другие модули. `FitLog.Host` ссылается на все.
- В `Shared` только то, что нужно нескольким модулям. Логика конкретного модуля там запрещена.
- Модуль регистрируется одним методом `Add<Module>`. `Program.cs` только вызывает эти методы.
- В `Host` нет бизнес-логики.

**Межмодульное общение**
- Только через интерфейс `Shared/Modules/<Module>/Abstractions/I<Module>Module.cs` и DTO из `Types/Dtos/`.
- Реализация интерфейса лежит в модуле-владельце и регистрируется в его `DependencyInjection.cs`.

**База данных**
- Своя БД, `DatabaseContext` и строка подключения на модуль.
- Внешних ключей и JOIN между модулями нет. Сущность другого модуля: только по идентификатору, данные через интерфейс модуля.
- Миграции: `Infrastructure/DataAccess/Migrations`. Создаёт и правит разработчик, не Claude.

**Доступ к данным**
- Сервис использует `DatabaseContext` напрямую.
- Чтение: `AsNoTracking()` и проекция в DTO через `Select`.
- Конфигурация сущностей только через `IEntityTypeConfiguration<T>` в `DataAccess/Configurations/`.
- Состояние, связанное с инвариантами, меняется методами сущности, а не присваиванием полей из сервиса.

**Сервисы и контроллеры**
- Логика только в сервисах. Интерфейс в `Abstractions/`, реализация в `Services/`. Зависеть от интерфейса.
- Контроллеры тонкие: разобрать запрос, вызвать сервис, вернуть результат.
- Сущности БД в ответах API не использовать. DTO сервиса и контракт контроллера разные типы.

## Открытые решения

- Обработка ошибок (Result или исключения).
- Применение миграций в dev и prod.
- Аутентификация и авторизация.
- Тесты.
- Версионирование API.
- Логирование.
