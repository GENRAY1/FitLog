#!/usr/bin/env python3
"""Токены сессии Claude Code одной строкой: in/cache_w/cache_r/out.

    python3 sessions/scripts/tokens.py [файл.jsonl]   # по умолчанию последняя сессия проекта

Считает по message.id (последняя запись), т.к. сообщение дублируется в транскрипте.
Если есть записи субагентов (isSidechain), добавляет их отдельным полем.
"""
import json
import sys
from pathlib import Path


def transcript() -> Path:
    if len(sys.argv) > 1:
        return Path(sys.argv[1])
    slug = str(Path.cwd().resolve()).replace("/", "-")
    files = sorted((Path.home() / ".claude/projects" / slug).glob("*.jsonl"),
                   key=lambda p: p.stat().st_mtime)
    if not files:
        sys.exit("нет транскриптов")
    return files[-1]


last = {}
for line in transcript().open(encoding="utf-8"):
    try:
        m = (json.loads(line).get("message"))
        side = json.loads(line).get("isSidechain", False)
    except json.JSONDecodeError:
        continue
    if isinstance(m, dict) and m.get("id") and "usage" in m:
        last[m["id"]] = (bool(side), m["usage"])


def fmt(side):
    s = [0] * 4
    for sc, u in last.values():
        if sc == side:
            s[0] += u.get("input_tokens", 0)
            s[1] += u.get("cache_creation_input_tokens", 0)
            s[2] += u.get("cache_read_input_tokens", 0)
            s[3] += u.get("output_tokens", 0)
    return "/".join(map(str, s))


out = f"main {fmt(False)}"
if any(sc for sc, _ in last.values()):
    out += f" | subagents {fmt(True)}"
print(out)
