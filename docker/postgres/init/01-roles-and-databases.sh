#!/bin/bash
# Создаёт роли и базы данных. Запускается образом postgres ОДИН раз, на пустом volume.
# Пароли приходят из переменных окружения (см. .env.example), в репозитории их нет.
#
# Схема доступа:
#   роль keycloak -> только база keycloak
#   роль fitlog   -> только базы модулей backend (FITLOG_MODULE_DBS)
# У PUBLIC по умолчанию есть право CONNECT на любую базу, поэтому оно отзывается у каждой базы.
set -euo pipefail

: "${KEYCLOAK_DB_PASSWORD:?}" "${FITLOG_DB_PASSWORD:?}"
MODULE_DBS="${FITLOG_MODULE_DBS:-fitlog_users}"

run_sql() {
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres "$@"
}

# Пароли передаются как psql-переменные и подставляются через :'name' (корректное экранирование).
run_sql -v kc_pass="$KEYCLOAK_DB_PASSWORD" -v fl_pass="$FITLOG_DB_PASSWORD" <<'SQL'
CREATE ROLE keycloak LOGIN PASSWORD :'kc_pass';
CREATE ROLE fitlog   LOGIN PASSWORD :'fl_pass';

CREATE DATABASE keycloak OWNER keycloak;
REVOKE ALL ON DATABASE keycloak FROM PUBLIC;
SQL

# Имена баз попадают в SQL как идентификаторы, поэтому допускаем только безопасные символы.
for db in $MODULE_DBS; do
  if [[ ! "$db" =~ ^[a-z_][a-z0-9_]*$ ]]; then
    echo "Недопустимое имя базы в FITLOG_MODULE_DBS: '$db'" >&2
    exit 1
  fi
  run_sql -c "CREATE DATABASE \"$db\" OWNER fitlog;" \
          -c "REVOKE ALL ON DATABASE \"$db\" FROM PUBLIC;"
done
