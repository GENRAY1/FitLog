# Frontend (Expo, React Native, TypeScript)

## Команды (из папки `frontend/`)

```bash
npx expo install <package>  # ВСЕГДА использовать вместо npm/yarn/pnpm/bun add — подбирает версии, совместимые с SDK
npx expo start              # запустить dev-сервер
npx expo lint               # линтинг
npx tsc --noEmit            # проверка типов
npx expo-doctor             # диагностика проблем с зависимостями и конфигурацией
npx expo install --fix      # исправить несовместимые версии пакетов
```

## Архитектура

@docs/architecture.md

## Правила

- Импорты внутри проекта через алиас `@/`, не через длинные относительные пути.
- Не вызывать `npm install <package>`, только `npx expo install`.
