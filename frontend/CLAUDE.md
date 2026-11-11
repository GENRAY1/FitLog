# Frontend (Expo, React Native, TypeScript)

## Команды (из папки `frontend/`)

```bash
npx expo install <package>  # подбирает версии, совместимые с SDK
npx expo start              # запустить dev-сервер
npx expo lint               # линтинг
npx tsc --noEmit            # проверка типов
npx expo-doctor             # диагностика проблем с зависимостями и конфигурацией
npx expo install --fix      # исправить несовместимые версии пакетов
```

## Архитектура

@docs/architecture.md

## Правила
- Не вызывать `npm install <package>`, только `npx expo install`.
