# Повторы действий

Счётчик по тегам. На третьем повторе заводим скилл, скрипт или правило и записываем, во что превратили.

| Тег | Счётчик | Сессии | Во что превращено |
|---|---|---|---|
| `install-deps` | 1 | 12.11.2026 | – |
| `verify-compat` | 1 | 12.11.2026 | – |
| `setup-infra` | 2 | 12.11.2026 (×2) | – |
| `verify-live` | 3 | 12.11.2026 (×3) | скрипт `docker/smoke-test.sh` (создан на третьем повторе), строка в `docs/infrastructure.md` |
| `write-docs` | 2 | 12.11.2026 (×2) | – |
| `add-backend-module` | 1 | 12.11.2026 | – |
| `refactor-rename` | 2 | 12.11.2026 (`UserProfile`→`User`, Keycloak→OIDC) | – |
| `get-test-token` | 7 | 12.11.2026 | скрипт `docker/keycloak/dev-token.py` (создан на третьем повторе) и раздел в `docs/infrastructure.md` |
| `debug-auth` | 1 | 12.11.2026 | – |
