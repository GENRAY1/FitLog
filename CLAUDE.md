# FitLog

FitLog — fullstack-приложение «дневник тренировок». Монорепа: клиент для iOS, Android и web (Expo) и REST API (ASP.NET Core).

## Карта репозитория

| Папка | Что это | Инструкции |
|---|---|---|
| `backend/` | ASP.NET Core Web API, .NET 10, решение `FitLog.sln` | `backend/CLAUDE.md` |
| `frontend/` | Expo (React Native + Expo Router), TypeScript | `frontend/CLAUDE.md` |
| `docker/`, `docker-compose.yml` | Dev-инфраструктура: PostgreSQL + Keycloak (realm `fitlog`) + backend API. Переменные в `.env` (шаблон `.env.example`) | `docs/infrastructure.md` |
| `sessions/` | Журнал рабочих сессий (не код приложения) | `sessions/README.md` |

## Общие соглашения

- Язык общения, комментариев к коду и документации: русский. Идентификаторы и технические термины: на английском.
- Секреты, ключи и строки подключения в репозиторий не коммитить.
