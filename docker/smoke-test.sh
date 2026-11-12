#!/bin/bash
# Живая проверка поднятого стека (docker compose up): backend отвечает и принимает реальный токен Keycloak.
# Ловит ошибки, которые не видны на «мусорном» токене (например, недоступный из контейнера jwks_uri).
# Токен берётся через docker/keycloak/dev-token.py, временный пользователь удаляется в конце. Только для dev.
set -uo pipefail

cd "$(dirname "$0")/.."
API="${API_URL:-http://localhost:${API_PORT:-5000}}"
fail=0

check() { # описание, ожидаемый код, фактический код
  if [[ "$2" == "$3" ]]; then echo "OK    $1 ($3)"; else echo "FAIL  $1: ожидали $2, получили $3"; fail=1; fi
}

check "/health" 200 "$(curl -s -o /dev/null -w '%{http_code}' "$API/health")"
check "/users/me без токена" 401 "$(curl -s -o /dev/null -w '%{http_code}' "$API/users/me")"

token="$(python3 docker/keycloak/dev-token.py get --client fitlog-swagger 2>/dev/null)"
if [[ -z "$token" ]]; then
  echo "FAIL  не удалось получить токен (Keycloak поднят?)"; fail=1
else
  check "/users/me с токеном" 200 "$(curl -s -o /dev/null -w '%{http_code}' -H "Authorization: Bearer $token" "$API/users/me")"
fi

python3 docker/keycloak/dev-token.py cleanup >/dev/null 2>&1
exit $fail
