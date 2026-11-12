# OAuth 2.0 + Keycloak: утверждённые решения

Постановка задачи: `docs/prompts/oauth-pkce.md`. Инфраструктура (этап A, готова): `docs/infrastructure.md`. Здесь решения, принятые при согласовании плана 12.11.2026, для этапов B и C.

## Общие

- **Единый адрес Keycloak:** `http://localhost:8080` для web, Swagger, backend и Android (эмулятор через `adb reverse tcp:8080` и `tcp:5084`, не `10.0.2.2`). `iss` = `<KC_HOSTNAME>/realms/fitlog`, Authority backend и `EXPO_PUBLIC_KEYCLOAK_URL` должны совпадать с ним.
- **Audience:** backend проверяет `aud = fitlog-api` (его добавляет mapper `fitlog-api-audience` в клиентах).
- **Схема мобильного приложения:** сменить `frontend` на `fitlog` в `frontend/app.json` (этап C). Затрагивает нативную часть `android/`, пересборку dev build делает разработчик. Redirect `fitlog://*` в realm уже есть.
- **Ротация refresh token** (`revokeRefreshToken`): после каждого refresh сохранять новый токен, параллельные refresh недопустимы.

## Этап B (backend)

- Создать `Directory.Build.props` и `Directory.Packages.props` (их требует `architecture.md`, но их нет), проекты `FitLog.Shared` и `FitLog.Users`, добавить в `FitLog.sln`.
- JwtBearer: `MapInboundClaims = false`, роли из `realm_access.roles` в `ClaimTypes.Role` (`OnTokenValidated`), fallback policy `RequireAuthenticatedUser`, политика `user`. Конфигурация `Authentication:Keycloak:*`.
- Публичный технический endpoint: `MapHealthChecks("/health").AllowAnonymous()`.
- Swagger: OAuth2 Authorization Code, `OAuthClientId("fitlog-swagger")`, `OAuthUsePkce()`, security requirement только на endpoints без `[AllowAnonymous]`. Риск: API схем в Swashbuckle 11 / новом `Microsoft.OpenApi`.
- `ICurrentUser` в `Shared/Common/` (`UserId` из `sub`, `Email`), реализация на `IHttpContextAccessor`.
- `FitLog.Users`: `UserProfile` (`Id` = `sub`, `Email`, `CreatedAt`), `GET /users/me` с lazy-созданием, при гонке ловить нарушение PK и перечитывать. Контракт `IUsersModule` и DTO в `Shared/Modules/Users/`.
- Строка подключения `ConnectionStrings:Users` в user-secrets. **Миграцию не создавать:** сообщить разработчику имя `InitialUsers` и модель (`users`: `Id` uuid PK, `Email` varchar(320) уникальный, `CreatedAt` timestamptz).
- Известное ограничение: email профиля не синхронизируется с Keycloak после смены.

## Этап C (frontend)

- Пакеты только через `npx expo install`: `expo-auth-session`, `expo-web-browser`, `expo-secure-store`, `expo-crypto`, затем `npx expo-doctor`.
- Слои: сессия (`guest | authenticated | loading`) в `shared/store/`, `AuthGate` в `shared/components/`, `useRequireAuth` в `shared/hooks/`. Axios в `shared/api/` принимает обработчики (`getAccessToken`, `refresh`, `onAuthFailed`), регистрирует их `features/auth` (`shared` не импортирует `features`).
- Токены: мобильные в `expo-secure-store`. Web: access token в памяти, refresh token в `sessionStorage`. `offline_access` только на мобильных, на web не запрашивается.
- Refresh: axios interceptor, один общий promise на пачку 401. Неудача: сессия `guest`.
- Logout: явный revoke (`/protocol/openid-connect/revoke`) + `end_session`, очистка хранилища, состояние `guest`.
- Возврат к действию после входа: pending-действие в памяти, на web popup-режим `expo-auth-session`.
- Риски: `web.output: "server"` (защитить `sessionStorage`/`SecureStore` от серверного рендеринга); cleartext HTTP до `localhost` работает только в debug-сборке Android.

## Вне объёма (оставлено на потом)

Защищённые маршруты Expo Router (`Stack.Protected`), реальное разделение функций гость/авторизованный, перенос локальных данных гостя (локального хранилища пока нет), `verifyEmail`/сброс пароля/SMTP, роль `admin`, production-режим Keycloak, iOS, автотесты.
