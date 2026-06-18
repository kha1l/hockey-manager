# AGENTS.md

Короткий контекст для Codex. Держать файл компактным; детали искать в коде и `README.md`.

## Проект

`nhl-manager` — Unity 6 LTS Android-игра. Внутриигровой бренд: `Continental Hockey Manager`.

MVP: офлайн-хоккейный менеджер с новой игрой, выбором команды, главным экраном клуба, составом, матчами, календарем, таблицей, плей-офф, контрактами, ростером, травмами и JSON-сохранением.

## Стек

Unity 6 LTS, C#, Android, `JsonUtility`, Git, VS Code.

## Правила и календарь

Используется NHL-like ruleset 2026-27 / CBA 2026-2030 как внутренняя модель прототипа:

- regular season: 84 игры на команду;
- preseason: 4 игры;
- salary cap: `104000000`;
- floor: `76900000`;
- league minimum: `850000`;
- max player salary: `20800000`;
- max contract years: own team `7`, free agent `6`;
- NHL roster: min `20`, max `23`.

Не возвращать старые значения: 82 игры, cap 95.5M, floor 70.6M, minimum 775k, max own-team term 8.

Provisional dates: preseason `2026-09-15..2026-09-27`, regular season `2026-09-28..2027-04-18`, trade deadline `2027-03-05`, playoffs start `2027-04-21`, draft `2027-06-25..2027-06-26`, free agency `2027-07-01`. Не хардкодить даты по разным классам; использовать централизованные config/services.

## Папки

- `Assets/Scenes` — сцены.
- `Assets/Scripts/Core` — `GameBootstrap`, `GameSession`.
- `Assets/Scripts/Data` — serializable data для save.
- `Assets/Scripts/Simulation` — rules, season, match, roster, injuries, contracts, trades, draft, ELC, waivers.
- `Assets/Scripts/UI` — screen controllers и row views.
- `Assets/Scripts/SaveSystem` — save/load/migrations/validation.
- `Assets/Scripts/Editor` — генерация сцен и Android build helpers.
- `Assets/Resources/Teams` и `Assets/Resources/Seeds` — runtime assets/seed.

## Runtime Scope

Не ломать путь `MainMenu -> TeamSelect -> Game`, существующие сохранения и уже подключенные системы: fictional teams/assets, dashboard, roster/lineup, pre-game/live/post-game, schedule, standings, stats, playoffs, contracts, cap/floor, trades, free agents, draft, ELC, farm/reserve, waivers, injuries, fatigue, roles, special teams, season transition, development/regression, history/news/diagnostics.

## Правила работы

- Делать маленькие безопасные шаги; не строить всю игру сразу.
- Сначала искать существующий сервис/модель, потом добавлять новое.
- Для `JsonUtility`: `[Serializable]` + public fields, не properties.
- Не менять `.unity` вручную текстом; сцены обновлять через `Assets/Scripts/Editor/InitialSceneCreator.cs`.
- Не добавлять сервер, БД, внешние API, сторонние плагины, реальные логотипы/фото/NHL assets, закрытые ключи или большие ассеты без явной задачи.
- Runtime team assets держать в `Assets/Resources/Teams/{team_id}/logo|home|away|full.png`; не хранить дубли вне `Resources`, если они не используются.
- Не коммитить `Library`, `Temp`, `Logs`, `.utmp`, APK/AAB/APKS и локальные IDE-файлы.
- Если Unity Editor открыт, batch build запускать из temp-копии проекта.
- После изменений кратко писать, что сделано и как проверить.

## Кодстайл

- Types/methods: `PascalCase`.
- Private fields: `_camelCase`.
- Locals: `camelCase`.
