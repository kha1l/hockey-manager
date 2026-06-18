# Continental Hockey Manager

Unity 6 LTS Android-прототип хоккейного менеджера. Репозиторий пока называется `nhl-manager`, но игровой бренд и UI используют `Continental Hockey Manager`.

## Что уже есть

- Новая игра и выбор команды.
- Клубный dashboard.
- Состав, линии, автозамены и травмы.
- Live/simulated match flow.
- Календарь на 84 игры, таблица, статистика и плей-офф.
- Контракты, salary cap/floor, farm/reserve, waivers.
- Драфт, ELC, free agents, trades.
- JSON save/load с миграциями.
- Сгенерированная лига, seed и placeholder-ассеты.

## Стек

- Unity 6 LTS
- C#
- Android
- `JsonUtility`
- Git / GitHub

## Структура

```text
Assets/
  Scenes/
  Resources/
    Seeds/
    Teams/
  Scripts/
    Core/
    Data/
    Editor/
    SaveSystem/
    Simulation/
    UI/
```

## Важные правила

- Для save data использовать `[Serializable]` и public fields.
- Не редактировать `.unity` вручную; сцены генерируются Editor-скриптами.
- Не коммитить `Library`, `Temp`, `Logs`, `.utmp`, APK/AAB и локальные IDE-файлы.
- Не добавлять реальные лицензированные хоккейные ассеты или внешние API без отдельной задачи.

## Сцены

Генерация/обновление сцен в Unity:

```text
Tools -> Continental Hockey Manager -> Create Initial Scenes
```

Актуальный путь игры:

```text
MainMenu -> TeamSelect -> Game
```

## Android APK

Основной helper:

```text
Assets/Scripts/Editor/AndroidApkBuilder.cs
```

Если проект открыт в Unity Editor, batch-сборку лучше делать из временной копии проекта.
