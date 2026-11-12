# Dev-инфраструктура: PostgreSQL, Keycloak и backend

Keycloak — Identity Provider (вход и регистрация по email/паролю, выдача JWT). Вместе с ним в `docker-compose.yml` поднимается один общий контейнер PostgreSQL: отдельные базы для Keycloak и для модулей backend. Там же собирается и запускается backend API (контейнер `backend`, Dockerfile `backend/FitLog.Host/Dockerfile`). Frontend запускается на хосте и в compose не входит.

## Быстрый старт

```bash
cp .env.example .env      # затем задайте пароли в .env (файл в .gitignore, не коммитить)
docker compose up -d --wait
```

| Что | Адрес |
|---|---|
| Админка Keycloak | http://localhost:5001 (логин и пароль из `KC_BOOTSTRAP_ADMIN_*` в `.env`) |
| Страница аккаунта и регистрации | http://localhost:5001/realms/fitlog/account |
| OIDC discovery | http://localhost:5001/realms/fitlog/.well-known/openid-configuration |
| PostgreSQL | `localhost:5432` (порт `POSTGRES_PORT`) |
| Backend API / Swagger | http://localhost:5000 / http://localhost:5000/swagger (порт `API_PORT`) |

Остановить: `docker compose down` (данные сохраняются в volume `fitlog_pgdata`). Полный сброс вместе с данными: `docker compose down -v`.

## Backend в контейнере

- Запускается с `ASPNETCORE_ENVIRONMENT=Development` (Swagger, авто-применение миграций) после готовности PostgreSQL и Keycloak. Пересборка после правок кода: `docker compose up -d --build backend`.
- Настройки подключения и OIDC задаются переменными окружения в `docker-compose.yml` и переопределяют `appsettings.Development.json`: строка подключения указывает на хост `postgres`.
- `Authentication:Oidc:Authority` остаётся публичным (`KC_HOSTNAME`), чтобы совпадать с `iss`, а discovery-документ контейнер берёт по внутреннему адресу `http://keycloak:8080` (`Authentication:Oidc:MetadataAddress`), так как `localhost` внутри контейнера — не Keycloak. При запуске на хосте адрес тоже задан явно в `appsettings.Development.json` (`http://localhost:5001/...`).
- В discovery-документе есть ещё `jwks_uri` (ключи подписи): он тоже должен быть доступен из контейнера. Для этого у Keycloak включён `KC_HOSTNAME_BACKCHANNEL_DYNAMIC`: backchannel-адреса строятся по хосту запроса (`keycloak:8080`), а `issuer` остаётся публичным. Без этого токены отвергаются с ошибкой `The issuer ... is invalid`.

- Проверка живого стека: `docker/smoke-test.sh` (health, 401 без токена, 200 на `/users/me` с настоящим токеном из `dev-token.py`).

## Базы и роли PostgreSQL

Создаются скриптом `docker/postgres/init/01-roles-and-databases.sh`, пароли берутся из `.env`.

| Роль | База | Доступ |
|---|---|---|
| `keycloak` | `keycloak` | только своя база |
| `fitlog` | базы из `FITLOG_MODULE_DBS` (по умолчанию `fitlog_users`) | только базы модулей |

- Правило архитектуры «своя БД на модуль»: на каждый модуль backend отдельная база `fitlog_<module>`. Таблицы Keycloak и приложения не смешиваются.
- У каждой базы отозвано `CONNECT` у `PUBLIC`: роль `keycloak` не может подключиться к базам приложения и наоборот. Названия баз при этом видны в `\l` всем ролям (так устроен каталог PostgreSQL), закрыт именно доступ.
- Скрипт выполняется **только на пустом volume**. Чтобы добавить базу позже: создать вручную (`CREATE DATABASE fitlog_workouts OWNER fitlog; REVOKE ALL ON DATABASE fitlog_workouts FROM PUBLIC;`) или пересоздать volume (`down -v`, данные будут потеряны).
- Строка подключения модуля Users для backend лежит в `backend/FitLog.Host/appsettings.Development.json` (`ConnectionStrings:Users`, вместе с dev-паролем). Пароль в ней должен совпадать с `FITLOG_DB_PASSWORD` из `.env`: при смене пароля обновите оба места и пересоздайте volume (`down -v`). Формат: `Host=localhost;Port=5432;Database=fitlog_users;Username=fitlog;Password=<FITLOG_DB_PASSWORD>`. В других средах задаётся переменной окружения `ConnectionStrings__Users`.
- Образ PostgreSQL 18: volume монтируется в `/var/lib/postgresql` (в ≤17 было `/var/lib/postgresql/data`).

## Realm `fitlog`

Импортируется из `docker/keycloak/realm-export.json` при первом старте (без секретов и пользователей). Импорт выполняется только если realm ещё не существует: после правки JSON выполните `docker compose down -v` (или удалите realm в админке) и запустите заново.

- **Регистрация и вход:** саморегистрация включена, форма содержит только email, пароль и подтверждение пароля. Email и есть логин (`registrationEmailAsUsername`), дубликаты email запрещены. Имя и фамилия не обязательны и при регистрации не показываются. Внешних Identity Providers (соцсети) нет.
- **Роли:** realm role `user` и `offline_access` назначаются всем новым пользователям (default roles). Роли `admin` нет.
- **Клиенты:**
  - `fitlog-app` — public, Standard flow, PKCE S256 обязателен, Direct Access Grants выключен. Redirect URI: `fitlog://*` (схема мобильного приложения) и `http://localhost:8081/*` (web). Scope `offline_access` — Optional, клиент запрашивает его явно.
  - `fitlog-swagger` — public, PKCE S256, redirect на Swagger UI backend (`http://localhost:5000/...`, `http://localhost:5084/...` и `https://localhost:7148/...`). Offline access не нужен.
- **Audience:** в оба клиента добавлен protocol mapper `fitlog-api-audience`, он кладёт `fitlog-api` в `aud` access token (по умолчанию Keycloak ставит только `account`). Backend проверяет именно это значение.
- **Время жизни:**

  | Параметр | Значение |
  |---|---|
  | Access token | 5 минут |
  | SSO Session Idle / Max | 30 минут / 10 часов |
  | Offline Session Idle | 30 дней |
  | Offline Session Max (включён) | 90 дней |
  | Revoke Refresh Token | включён (ротация при каждом обновлении, повторное использование запрещено) |

  Из-за ротации клиент обязан сохранять новый refresh/offline token после **каждого** обновления и не делать параллельных обновлений.
- **Проверка offline access:** чтобы увидеть, что сессия переживает SSO, временно уменьшите `SSO Session Idle` и `SSO Session Max` в админке (Realm settings → Sessions) до нескольких минут. Offline-сессия пользователя видна в админке: Users → пользователь → Sessions (или Clients → `fitlog-app` → Sessions → Offline).

## Адрес Keycloak и claim `iss`

Backend сверяет `iss` токена с настройкой Authority, поэтому у Keycloak должен быть **один** публичный адрес для всех клиентов. Он задаётся `KC_HOSTNAME` в `.env` (по умолчанию `http://localhost:5001`), `iss` = `<KC_HOSTNAME>/realms/fitlog`.

| Клиент | Какой адрес использовать |
|---|---|
| Web (Expo на `localhost:8081`) | `http://localhost:5001` |
| Swagger UI backend | `http://localhost:5001` |
| backend (Authority) | `http://localhost:5001/realms/fitlog` |
| Android-эмулятор | `http://localhost:5001` + проброс портов: `adb reverse tcp:5001 tcp:5001` и `adb reverse tcp:5000 tcp:5000` (API) |

Почему не `10.0.2.2`: браузер на хосте не откроет `10.0.2.2`, а `iss` у web и Android разошёлся бы. Кроме того, Keycloak ставит cookies с флагом `Secure` даже по HTTP, а браузеры принимают такие cookies по HTTP только для `localhost`: при LAN-адресе по HTTP вход через браузер, скорее всего, не заработает (в этой конфигурации не проверялось).

**Физическое устройство по Wi-Fi** (без `adb reverse`): в `.env` задать `KC_BIND=0.0.0.0` и `KC_HOSTNAME=http://<LAN IP>:5001`, затем везде (backend Authority, `EXPO_PUBLIC_OIDC_AUTHORITY`) использовать тот же адрес. Из-за `Secure` cookies может понадобиться HTTPS. Подключение по USB с `adb reverse` проще и надёжнее.

## Ограничения dev-конфигурации и что отложено

- Keycloak запущен в `start-dev` (HTTP, dev-база не используется, данные в PostgreSQL). Для production нужны `start`, HTTPS и внешний hostname.
- **Подтверждение email (`verifyEmail`) и сброс пароля выключены:** нет SMTP. Включить вместе с настройкой SMTP (`verifyEmail` и `resetPasswordAllowed` в realm).
- Защита от перебора паролей (`bruteForceProtected`) включена с настройками по умолчанию.
- Язык страниц входа русский (`defaultLocale: ru`, поддерживается только `ru`, переключателя языка нет). Чтобы добавить язык, дополните `supportedLocales` в realm (Realm settings → Localization).
- Пароли администратора и баз только в `.env`; в git лежит `.env.example`.
