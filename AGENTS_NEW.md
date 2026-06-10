# AGENTS.md

## Проект

`nhl-manager` — Unity-игра для Android в жанре хоккейного менеджера.

Игрок управляет клубом NHL: составом, контрактами, зарплатной ведомостью, обменами, драфтом, развитием игроков, регулярным сезоном, плей-офф и межсезоньем.

Проект ориентируется на ruleset NHL сезона **2026-27** и CBA **2026-2030**.

---

## Главный ruleset проекта

Все новые механики должны проектироваться вокруг NHL 2026-27.

Базовые параметры:

- Ruleset: `NHL 2026-27`
- CBA: `NHL/NHLPA CBA 2026-2030`
- Regular season games per team: `84`
- Preseason games per team: `4`
- Salary cap upper limit: `104000000`
- Salary cap lower limit / floor: `76900000`
- League minimum salary: `850000`
- Maximum player salary: `20800000`
- Max contract years with own team: `7`
- Max contract years free agent: `6`
- Min NHL roster size: `20`
- Max NHL roster size: `23`

Не использовать старые значения:

- 82 игры
- 95_500_000 cap
- 70_600_000 floor
- 775_000 minimum salary
- 19_100_000 maximum player salary
- 8 лет максимального срока контракта

---

## Календарь сезона

Точные официальные даты NHL 2026-27 не должны хардкодиться как окончательные.

Использовать provisional-даты только как настройки прототипа:

- Preseason start: `2026-09-15`
- Preseason end: `2026-09-27`
- Regular season start: `2026-09-28`
- Regular season end: `2027-04-18`
- Trade deadline: `2027-03-05`
- Playoffs start: `2027-04-21`
- Stanley Cup Final expected end: `2027-06-21`
- Draft start: `2027-06-25`
- Draft end: `2027-06-26`
- Free agency start: `2027-07-01`

CalendarStatus должен быть:

```text
Provisional
```

Когда NHL опубликует официальный календарь 2026-27, даты нужно заменить централизованно через `LeagueCalendarConfig`, а не вручную в разных классах.

---

## Важное правило по данным NHL

Архитектура проекта должна поддерживать:

- реальные команды NHL;
- реальные дивизионы и конференции;
- реальные сезоны;
- реальные составы;
- реальные контракты;
- реальные статистические данные;
- реальные логотипы и фотографии игроков.

Но нельзя добавлять в репозиторий:

- официальные логотипы NHL и клубов;
- фотографии игроков;
- бренд-гайды;
- платные датасеты;
- закрытые API-ключи;
- приватные документы;
- лицензированные ассеты без разрешения.

Для прототипа разрешено использовать:

- текстовые названия команд;
- текстовые ID команд;
- тестовых игроков;
- сгенерированные контракты;
- сгенерированную статистику;
- placeholder-ассеты.

---

## Технологический стек

- Unity 6 LTS
- C#
- Android
- JSON save files
- Git
- VS Code
- Codex

---

## Основная архитектура

Использовать простую архитектуру:

```text
Assets/
├── Scenes/
├── Scripts/
│   ├── Core/
│   ├── Data/
│   ├── Gameplay/
│   ├── Managers/
│   ├── SaveSystem/
│   ├── Simulation/
│   └── UI/
├── Prefabs/
├── Art/
├── UI/
├── Resources/
└── ScriptableObjects/
```

---

## Разделы кода

### Core

Точка входа и состояние игры:

- `GameBootstrap`
- `GameSession`

### Data

Serializable-модели для JsonUtility:

- `GameState`
- `LeagueRulesData`
- `LeagueCalendarData`
- `PlayerData`
- `TeamData`
- `TeamFinanceData`
- `SeasonData`
- `ScheduleGameData`
- `TeamStandingData`
- `MatchResultData`
- `PlayerGameStatData`
- `PlayerSeasonStatsData`
- `PlayoffData`
- `PlayoffRoundData`
- `PlayoffSeriesData`

Все классы в `Data` должны использовать **public fields**, а не properties, потому что используется `JsonUtility`.

### Simulation

Чистая игровая логика:

- генерация команд;
- генерация игроков;
- генерация календаря;
- симуляция матчей;
- расчёт рейтинга команд;
- турнирная таблица;
- статистика игроков;
- плей-офф;
- контракты;
- salary cap;
- правила лиги;
- календарные правила.

### UI

Контроллеры экранов и строк:

- `MainMenuController`
- `TeamSelectController`
- `GameScreenController`
- `RosterController`
- `CalendarController`
- `StandingsController`
- `PlayerStatsController`
- `PlayoffsController`
- `ContractsController`

### SaveSystem

Сохранение и загрузка:

- `SaveLoadService`

Использовать:

```csharp
Application.persistentDataPath
JsonUtility
```

Не использовать базу данных на текущем этапе.

---

## Текущий статус проекта

Уже реализовано или предполагается реализованным:

- структура проекта;
- сцены `MainMenu`, `TeamSelect`, `Game`;
- выбор команды;
- сохранение и загрузка через JSON;
- экран клуба;
- экран состава;
- календарь регулярного сезона;
- турнирная таблица;
- симуляция игрового дня;
- статистика игроков;
- плей-офф;
- контракты;
- salary cap / salary floor;
- ruleset NHL 2026-27;
- centralized league rules;
- provisional league calendar.

---

## Приоритет разработки

Текущий приоритет:

```text
Сделать полноценный offline NHL manager prototype на Android.
```

Ближайшие крупные этапы:

1. Обмены игроков.
2. Проверка salary cap при обменах.
3. Экран предложений обмена.
4. Свободные агенты.
5. Драфт.
6. Межсезонье.
7. Новый сезон.
8. Развитие игроков.
9. Старение игроков.
10. История сезонов.
11. Достижения клуба.
12. Улучшение UI/UX.

---

## Правила разработки

- Не делать всю игру сразу.
- Делать одну задачу за раз.
- Не ломать уже работающий путь:

```text
MainMenu → TeamSelect → Game
```

- Не ломать сохранение и загрузку.
- Не ломать экран состава.
- Не ломать календарь.
- Не ломать таблицу.
- Не ломать статистику игроков.
- Не ломать плей-офф.
- Не ломать контракты.
- Не менять ruleset обратно на 2025-26.
- Не возвращать 82 игры.
- Не использовать старые salary cap values.

---

## Правила Codex

Перед изменениями Codex должен:

1. Изучить существующие файлы.
2. Не дублировать уже существующие классы.
3. Не создавать альтернативную архитектуру без необходимости.
4. Не удалять рабочую функциональность.
5. Не менять `AGENTS.md` и `README.md`, если задача явно не просит.
6. После выполнения показывать список созданных и изменённых файлов.
7. Коротко объяснять, как проверить результат в Unity.

---

## Правила C#

Использовать стиль:

```text
Classes: PascalCase
Methods: PascalCase
Public fields: PascalCase
Private fields: _camelCase
Local variables: camelCase
```

Пример:

```csharp
[Serializable]
public class PlayerData
{
    public string Id;
    public string FirstName;
    public string LastName;
    public int Salary;
}
```

---

## Правила Serializable data

Для классов, которые сохраняются через `JsonUtility`:

- использовать `[Serializable]`;
- использовать public fields;
- не использовать properties;
- списки инициализировать пустыми списками;
- всегда делать null-safe проверки.

Пример:

```csharp
[Serializable]
public class SeasonData
{
    public int SeasonYear;
    public int TargetGamesPerTeam;
    public List<ScheduleGameData> Schedule = new List<ScheduleGameData>();
}
```

---

## Правила сохранения

Сохранение должно содержать:

- выбранную команду;
- команды;
- игроков;
- контракты;
- league rules;
- league calendar;
- сезон;
- календарь;
- таблицу;
- историю матчей;
- статистику игроков;
- плей-офф;
- чемпиона.

Файл сохранения:

```text
save.json
```

Путь:

```csharp
Application.persistentDataPath
```

---

## Правила обратной совместимости

При загрузке старого сохранения нужно нормализовать данные:

- если `LeagueRules == null`, создать через `LeagueRulesConfig.CreateDefaultRules()`;
- если `LeagueCalendar == null`, создать через `LeagueCalendarConfig.CreateDefaultCalendar()`;
- если сезон был на 82 игры, пересоздать календарь на 84 игры;
- если salary ниже `850000`, поднять до `850000`;
- если salary выше `20800000`, ограничить `20800000`;
- если срок контракта выше `7`, ограничить `7`;
- если списки null, создать пустые списки;
- не сбрасывать выбранную команду без необходимости.

---

## League rules source of truth

Главным источником правил должен быть:

```text
LeagueRulesConfig
LeagueCalendarConfig
```

`SalaryCapConfig` можно оставить только как thin wrapper для обратной совместимости.

Не разбрасывать числа по коду.

Нельзя писать напрямую:

```csharp
104000000
76900000
850000
20800000
84
```

Вместо этого использовать:

```csharp
LeagueRulesData
LeagueRulesConfig
SalaryCapConfig
```

---

## NHL 2026-27 rules

Использовать:

```text
RulesSeasonStartYear = 2026
RulesetName = NHL 2026-27
CbaName = NHL/NHLPA CBA 2026-2030
RegularSeasonGamesPerTeam = 84
PreseasonGamesPerTeam = 4
SalaryCapUpperLimit = 104000000
SalaryCapLowerLimit = 76900000
LeagueMinimumSalary = 850000
MaximumPlayerSalary = 20800000
MaxContractYearsWithOwnTeam = 7
MaxContractYearsFreeAgent = 6
MinRosterSize = 20
MaxRosterSize = 23
HasPlayoffSalaryCap = true
AllowsDeferredSalary = false
UsesTravelingEmergencyGoalie = true
```

---

## Календарь 84 игр

Календарь должен генерировать 84 матча на команду.

Для 32 команд общее количество матчей:

```text
32 * 84 / 2 = 1344
```

Одна команда не должна играть два матча в один игровой день.

Календарь не должен использовать реальные даты NHL, пока официальный календарь не опубликован.

---

## Контракты

Контракты должны учитывать:

- минимум зарплаты;
- максимум зарплаты;
- salary cap;
- salary floor;
- максимальный срок переподписания;
- максимальный срок UFA-контракта.

Статусы контрактов:

```text
Signed
Expiring
RFA
UFA
```

Сгенерированные контракты должны иметь:

```csharp
IsGeneratedContract = true;
```

---

## Salary cap

Команда должна иметь:

- Payroll
- CapSpace
- FloorSpace
- IsOverCap
- IsBelowFloor

Формулы:

```text
Payroll = сумма Salary всех игроков
CapSpace = SalaryCapUpperLimit - Payroll
FloorSpace = Payroll - SalaryCapLowerLimit
IsOverCap = Payroll > SalaryCapUpperLimit
IsBelowFloor = Payroll < SalaryCapLowerLimit
```

---

## Плей-офф

Использовать NHL-style формат:

- 16 команд;
- 8 команд от Eastern Conference;
- 8 команд от Western Conference;
- топ-3 каждого дивизиона;
- 2 wild card в каждой конференции;
- серии best-of-seven;
- серия заканчивается при 4 победах;
- 4 раунда;
- чемпион Stanley Cup.

---

## Team structure

Использовать структуру NHL с 32 командами.

Eastern Conference / Atlantic:

- boston-bruins
- buffalo-sabres
- detroit-red-wings
- florida-panthers
- montreal-canadiens
- ottawa-senators
- tampa-bay-lightning
- toronto-maple-leafs

Eastern Conference / Metropolitan:

- carolina-hurricanes
- columbus-blue-jackets
- new-jersey-devils
- new-york-islanders
- new-york-rangers
- philadelphia-flyers
- pittsburgh-penguins
- washington-capitals

Western Conference / Central:

- chicago-blackhawks
- colorado-avalanche
- dallas-stars
- minnesota-wild
- nashville-predators
- st-louis-blues
- utah-mammoth
- winnipeg-jets

Western Conference / Pacific:

- anaheim-ducks
- calgary-flames
- edmonton-oilers
- los-angeles-kings
- san-jose-sharks
- seattle-kraken
- vancouver-canucks
- vegas-golden-knights

---

## UI screens

Основные экраны:

- MainMenu
- TeamSelect
- Dashboard
- Roster
- Contracts
- Calendar
- Standings
- PlayerStats
- Playoffs

Будущие экраны:

- Trades
- FreeAgency
- Draft
- Offseason
- PlayerDevelopment
- TeamHistory

---

## Editor scripts

Если нужно создать или обновить сцены, использовать:

```text
Assets/Scripts/Editor/InitialSceneCreator.cs
```

Не редактировать `.unity` файлы вручную текстом, если Unity Editor недоступен.

Главный пункт меню:

```text
Tools → NHL Manager → Create Initial Scenes
```

---

## Что нельзя делать

Нельзя:

- добавлять сервер;
- добавлять базу данных;
- добавлять сторонние плагины без явной задачи;
- добавлять реальные логотипы;
- добавлять фотографии игроков;
- добавлять API-ключи;
- подключать внешние NHL API;
- использовать реальные контракты игроков без отдельной задачи;
- менять ruleset обратно на 2025-26;
- возвращать 82 игры;
- удалять сохранение без команды пользователя;
- ломать существующие экраны.

---

## Что делать при сомнении

Если задача неоднозначная:

1. Сохранять текущую архитектуру.
2. Использовать NHL 2026-27 ruleset.
3. Использовать CBA 2026-2030.
4. Использовать 84 игры.
5. Использовать централизованные конфиги.
6. Не добавлять новые зависимости.
7. Делать минимальное рабочее изменение.
8. Объяснять результат после выполнения.