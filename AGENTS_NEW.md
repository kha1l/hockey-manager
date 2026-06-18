# AGENTS_NEW.md

Компактный расширенный контекст. Если есть конфликт, следовать `AGENTS.md` и текущему коду.

## Ruleset

Проект использует NHL-like ruleset 2026-27 / CBA 2026-2030 как внутреннюю модель прототипа:

- regular season: 84 игры на команду;
- preseason: 4 игры;
- salary cap: `104000000`;
- floor: `76900000`;
- league minimum: `850000`;
- max player salary: `20800000`;
- max contract years: own team `7`, free agent `6`;
- NHL roster: min `20`, max `23`.

Не возвращать старые значения: 82 игры, cap 95.5M, floor 70.6M, minimum 775k, max salary 19.1M, max own-team term 8.

## Calendar

Даты сезона 2026-27 provisional и должны жить централизованно:

- preseason: `2026-09-15`..`2026-09-27`;
- regular season: `2026-09-28`..`2027-04-18`;
- trade deadline: `2027-03-05`;
- playoffs: start `2027-04-21`;
- draft: `2027-06-25`..`2027-06-26`;
- free agency: `2027-07-01`.

Не хардкодить даты по разным классам; использовать конфиги календаря/правил.

## Runtime Scope

Уже есть или считается частью MVP:

- MainMenu -> TeamSelect -> Game;
- fictional league teams/assets;
- JSON save/load и migrations;
- dashboard, roster, lineup, pre-game, live match, post-game;
- schedule, standings, stats, playoffs;
- contracts, cap/floor, trades, free agents, draft, ELC;
- farm/reserve, waivers, injuries, fatigue, roles, special teams;
- season transition, development/regression, history/news/diagnostics.

При доработках не ломать этот путь и существующие сохранения.

## Assets

Можно использовать сгенерированные placeholder assets. Нельзя добавлять реальные логотипы, фото игроков, платные датасеты, закрытые API-ключи и внешние NHL API без отдельной задачи.

Runtime assets лежат в:

- `Assets/Resources/Teams/{team_id}/logo|home|away|full.png`;
- `Assets/Resources/Seeds/league_seed.json`.

Не держать дубли этих ассетов вне `Resources`, если они не используются.

## Editor/Build

- Сцены обновлять через `Assets/Scripts/Editor/InitialSceneCreator.cs`.
- Android helpers: `AndroidBuildSettingsApplier`, `AndroidApkBuilder`.
- APK/AAB не коммитить.
- Если Unity Editor открыт, batch build запускать из temp-копии проекта.
