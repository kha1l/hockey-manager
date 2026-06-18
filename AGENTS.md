# AGENTS.md

Короткий контекст для Codex. Подробности держать в коде и README, не раздувать этот файл.

## Проект

`nhl-manager` — Unity 6 LTS Android-менеджер хоккейного клуба. Текущий бренд в игре: `Continental Hockey Manager`.

Цель MVP: офлайн-игра с новой игрой, выбором команды, клубным экраном, составом, матчами, календарем, таблицей, плей-офф и JSON-сохранением.

## Стек

Unity 6 LTS, C#, Android, `JsonUtility`, Git.

## Папки

- `Assets/Scenes` — сцены.
- `Assets/Scripts/Core` — `GameBootstrap`, `GameSession`.
- `Assets/Scripts/Data` — serializable data, только public fields для `JsonUtility`.
- `Assets/Scripts/Simulation` — правила, сезон, матчи, roster, injuries, contracts.
- `Assets/Scripts/UI` — контроллеры экранов и row views.
- `Assets/Scripts/SaveSystem` — save/load/migrations.
- `Assets/Scripts/Editor` — генерация сцен и Android build helpers.
- `Assets/Resources/Teams` и `Assets/Resources/Seeds` — runtime seed/assets.

## Правила работы

- Делать маленькие безопасные шаги.
- Сначала искать существующий сервис/модель, потом добавлять новое.
- Не менять `.unity` текстом; обновлять сцены через Editor-скрипты.
- Не добавлять сервер, БД, внешние API, сторонние плагины и реальные лицензированные ассеты без явной задачи.
- Не коммитить `Library`, `Temp`, `Logs`, `.utmp`, APK/AAB и локальные IDE-файлы.
- После изменений кратко писать, что сделано и как проверить.

## Кодстайл

- Types/methods: `PascalCase`.
- Private fields: `_camelCase`.
- Locals: `camelCase`.
- Data classes для save: `[Serializable]` + public fields, не properties.
