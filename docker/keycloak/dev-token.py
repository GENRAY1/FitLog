#!/usr/bin/env python3
"""Тестовый токен из dev-Keycloak для ручной проверки API (без браузера).

  python3 docker/keycloak/dev-token.py get [--client fitlog-app] [--scope "openid profile email"] [--out FILE]
  python3 docker/keycloak/dev-token.py cleanup

get      регистрирует временного пользователя dev-token-<hex>@example.test через форму регистрации
         (Authorization Code + PKCE, как настоящий клиент) и печатает access token в stdout
         (или пишет в файл с правами 0600, если задан --out). Email пользователя идёт в stderr.
cleanup  удаляет временных пользователей dev-token-* из Keycloak и их профили из БД fitlog_users.
         Остальных пользователей не трогает.

Нужны поднятые `docker compose up` (Keycloak и PostgreSQL). Секретов в скрипте нет: пароль админа
берётся внутри контейнера Keycloak, пароль БД читается из .env и никуда не печатается.
Только для разработки.
"""
import argparse
import base64
import hashlib
import html
import json
import os
import re
import secrets
import subprocess
import sys
import urllib.error
import urllib.parse
import urllib.request

REALM_URL = os.environ.get("KC_REALM_URL", "http://localhost:5001/realms/fitlog")
OIDC = f"{REALM_URL}/protocol/openid-connect"
EMAIL_PREFIX = "dev-token-"
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))

# Redirect URI, разрешённые в realm для клиентов (см. docker/keycloak/realm-export.json).
DEFAULT_REDIRECTS = {
    "fitlog-app": "http://localhost:8081/cb",
    "fitlog-swagger": "http://localhost:5084/swagger/oauth2-redirect.html",
}


class _NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, *args, **kwargs):
        return None


def _http(url, cookies, data=None):
    """GET/POST без редиректов. Cookies ведём вручную: Keycloak ставит Secure даже по http://localhost."""
    body = urllib.parse.urlencode(data).encode() if data is not None else None
    request = urllib.request.Request(url, data=body)
    if cookies:
        request.add_header("Cookie", "; ".join(f"{k}={v}" for k, v in cookies.items()))
    opener = urllib.request.build_opener(_NoRedirect)
    try:
        response = opener.open(request)
    except urllib.error.HTTPError as error:
        response = error
    for header in response.headers.get_all("Set-Cookie") or []:
        name, _, value = header.split(";")[0].partition("=")
        cookies[name] = value
    return response.status, response.headers, response.read().decode()


def cmd_get(args):
    redirect = args.redirect or DEFAULT_REDIRECTS.get(args.client)
    if not redirect:
        sys.exit(f"Для клиента {args.client} задайте --redirect.")

    verifier = secrets.token_urlsafe(48)
    challenge = base64.urlsafe_b64encode(hashlib.sha256(verifier.encode()).digest()).rstrip(b"=").decode()
    query = urllib.parse.urlencode({
        "client_id": args.client, "response_type": "code", "scope": args.scope, "redirect_uri": redirect,
        "code_challenge": challenge, "code_challenge_method": "S256", "state": "dev",
    })

    cookies = {}
    _, _, page = _http(f"{OIDC}/registrations?{query}", cookies)
    form = re.search(r'<form[^>]+action="([^"]+)"', page)
    if not form:
        sys.exit("Страница регистрации не открылась: Keycloak запущен? Правильные client и redirect?")

    email = f"{EMAIL_PREFIX}{secrets.token_hex(4)}@example.test"
    password = secrets.token_urlsafe(14) + "Aa1!"
    status, headers, _ = _http(html.unescape(form.group(1)), cookies,
                               {"email": email, "password": password, "password-confirm": password})
    code = urllib.parse.parse_qs(urllib.parse.urlparse(headers.get("Location", "")).query).get("code", [None])[0]
    if not code:
        sys.exit(f"Регистрация не вернула code (HTTP {status}).")

    _, _, token_body = _http(f"{OIDC}/token", {}, {
        "grant_type": "authorization_code", "client_id": args.client, "code": code,
        "redirect_uri": redirect, "code_verifier": verifier,
    })
    token = json.loads(token_body).get("access_token")
    if not token:
        sys.exit(f"Обмен кода не удался: {token_body}")

    print(f"Пользователь: {email}", file=sys.stderr)
    if args.out:
        fd = os.open(args.out, os.O_WRONLY | os.O_CREAT | os.O_TRUNC, 0o600)
        with os.fdopen(fd, "w") as file:
            file.write(token)
        print(f"Токен записан в {args.out}", file=sys.stderr)
    else:
        print(token)


def _read_env(name):
    try:
        with open(os.path.join(ROOT, ".env"), encoding="utf-8") as file:
            for line in file:
                if line.startswith(f"{name}="):
                    return line.rstrip("\n").split("=", 1)[1]
    except FileNotFoundError:
        pass
    return None


def cmd_cleanup(_args):
    kcadm = (
        'K=/opt/keycloak/bin/kcadm.sh; '
        '$K config credentials --server http://localhost:8080 --realm master '
        '--user "$KC_BOOTSTRAP_ADMIN_USERNAME" --password "$KC_BOOTSTRAP_ADMIN_PASSWORD" >/dev/null 2>&1 || exit 1; '
        f'for id in $($K get users -r fitlog -q "email={EMAIL_PREFIX}" --fields id --format csv --noquotes); do '
        '$K delete users/$id -r fitlog && echo deleted; done'
    )
    result = subprocess.run(["docker", "compose", "exec", "-T", "keycloak", "bash", "-c", kcadm],
                            cwd=ROOT, capture_output=True, text=True)
    if result.returncode != 0:
        sys.exit("Не удалось подключиться к Keycloak (docker compose запущен?).")
    print(f"Keycloak: удалено пользователей {result.stdout.count('deleted')}")

    password = _read_env("FITLOG_DB_PASSWORD")
    if password:
        result = subprocess.run(
            ["docker", "compose", "exec", "-T", "-e", f"PGPASSWORD={password}", "postgres", "psql", "-h", "127.0.0.1",
             "-U", "fitlog", "-d", "fitlog_users", "-tAc", f"delete from users where email like '{EMAIL_PREFIX}%'"],
            cwd=ROOT, capture_output=True, text=True)
        # Таблицы может ещё не быть (миграции не применены): это не ошибка.
        print("БД fitlog_users:", result.stdout.strip() or "таблицы users нет или профилей не было")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = parser.add_subparsers(dest="command", required=True)
    get = sub.add_parser("get", help="получить access token временного пользователя")
    get.add_argument("--client", default="fitlog-app", choices=sorted(DEFAULT_REDIRECTS))
    get.add_argument("--scope", default="openid profile email")
    get.add_argument("--redirect", help="redirect_uri (по умолчанию из realm для выбранного клиента)")
    get.add_argument("--out", help="записать токен в файл (права 0600) вместо stdout")
    get.set_defaults(func=cmd_get)
    clean = sub.add_parser("cleanup", help="удалить временных пользователей dev-token-*")
    clean.set_defaults(func=cmd_cleanup)
    args = parser.parse_args()
    args.func(args)


if __name__ == "__main__":
    main()
