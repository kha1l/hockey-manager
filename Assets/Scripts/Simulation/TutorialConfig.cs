using System.Collections.Generic;

public static class TutorialConfig
{
    public const int CurrentTutorialVersion = 1;

    public const string StepOpenDashboard = "OpenDashboard";
    public const string StepOpenRoster = "OpenRoster";
    public const string StepOpenLineup = "OpenLineup";
    public const string StepSimulateFirstDay = "SimulateFirstDay";
    public const string StepOpenStandings = "OpenStandings";
    public const string StepOpenContracts = "OpenContracts";
    public const string StepOpenOwner = "OpenOwner";
    public const string StepSaveGame = "SaveGame";

    public const string PanelDashboard = "Dashboard";
    public const string PanelRoster = "Roster";
    public const string PanelLineup = "Lineup";
    public const string PanelCalendar = "Calendar";
    public const string PanelStandings = "Standings";
    public const string PanelContracts = "Contracts";
    public const string PanelExtensions = "Extensions";
    public const string PanelFreeAgency = "FreeAgency";
    public const string PanelDraft = "Draft";
    public const string PanelScouting = "Scouting";
    public const string PanelOwner = "Owner";
    public const string PanelNews = "News";
    public const string PanelHistory = "History";
    public const string PanelDiagnostics = "Diagnostics";
    public const string PanelGmCareer = "GmCareer";

    public static List<TutorialStepData> GetDefaultSteps()
    {
        return new List<TutorialStepData>
        {
            CreateStep(StepOpenDashboard, "Открой Dashboard", "Dashboard показывает главное состояние команды, alerts и быстрые переходы.", PanelDashboard, "Открыть Dashboard", 1),
            CreateStep(StepOpenRoster, "Проверь состав", "Посмотри игроков, статусы, контракты, morale и injuries.", PanelRoster, "Открыть состав", 2),
            CreateStep(StepOpenLineup, "Проверь линии", "Линии определяют, кто играет в матчах.", PanelLineup, "Открыть линии", 3),
            CreateStep(StepSimulateFirstDay, "Симулируй первый день", "Нажми симуляцию игрового дня, чтобы увидеть результат и изменения.", PanelDashboard, "Симулировать игровой день", 4),
            CreateStep(StepOpenStandings, "Открой таблицу", "Таблица показывает положение команды в сезоне.", PanelStandings, "Открыть таблицу", 5),
            CreateStep(StepOpenContracts, "Проверь контракты", "Игроки с истекающими контрактами могут уйти, если их не продлить.", PanelContracts, "Открыть контракты", 6),
            CreateStep(StepOpenOwner, "Посмотри цели владельца", "Цели владельца влияют на доверие и job security.", PanelOwner, "Открыть владельца", 7),
            CreateStep(StepSaveGame, "Сохрани игру", "Сейв сохраняет прогресс карьеры.", "Save", "Сохранить игру", 8)
        };
    }

    public static TutorialHintData GetHintForPanel(string panelId)
    {
        string normalized = NormalizePanelId(panelId);
        if (normalized == PanelDashboard)
        {
            return CreateHint(normalized, "Центр управления", "Здесь видны alerts, новости, цели владельца, cap space и быстрые переходы.", 100);
        }

        if (normalized == PanelRoster)
        {
            return CreateHint(normalized, "Состав", "Следи за статусами Pro/Farm, травмами, morale, контрактами и усталостью.", 95);
        }

        if (normalized == PanelLineup)
        {
            return CreateHint(normalized, "Линии", "В линиях выбирается активный состав. Травмированные и не-Pro игроки делают lineup invalid.", 95);
        }

        if (normalized == PanelContracts)
        {
            return CreateHint(normalized, "Контракты", "Игроки с 1 годом контракта скоро потребуют продления или выйдут на рынок.", 90);
        }

        if (normalized == PanelExtensions)
        {
            return CreateHint(normalized, "Продления", "Interest зависит от morale, роли, игрового времени, результатов и WantsTrade.", 88);
        }

        if (normalized == PanelFreeAgency)
        {
            return CreateHint(normalized, "Свободные агенты", "Игроки оценивают не только деньги, но и роль, cap fit и командные потребности.", 86);
        }

        if (normalized == PanelDraft)
        {
            return CreateHint(normalized, "Драфт", "DraftRank, scouting accuracy и risk помогают оценить проспектов.", 84);
        }

        if (normalized == PanelScouting)
        {
            return CreateHint(normalized, "Скаутинг", "Скаутинг сужает диапазоны Overall/Potential и показывает риск развития.", 84);
        }

        if (normalized == PanelOwner)
        {
            return CreateHint(normalized, "Владелец", "Цели владельца определяют ожидания и влияют на доверие к GM.", 82);
        }

        if (normalized == PanelNews)
        {
            return CreateHint(normalized, "Новости", "Здесь отображаются важные события лиги и твоей команды.", 80);
        }

        if (normalized == PanelDiagnostics)
        {
            return CreateHint(normalized, "Diagnostics", "Используй Validate и Repair Safe для проверки сейва во время разработки.", 78);
        }

        if (normalized == PanelHistory)
        {
            return CreateHint(normalized, "История", "История сезонов, награды, рекорды и Hall of Fame появятся после развития карьеры.", 76);
        }

        return CreateHint(normalized, "Подсказка", "Открой помощь, чтобы увидеть checklist первых действий и контекст панели.", 40);
    }

    public static string NormalizePanelId(string panelId)
    {
        if (string.IsNullOrEmpty(panelId))
        {
            return PanelDashboard;
        }

        string value = panelId.Trim();
        if (value == "DashboardPanel")
        {
            return PanelDashboard;
        }

        if (value.EndsWith("Panel"))
        {
            value = value.Substring(0, value.Length - "Panel".Length);
        }

        return value;
    }

    private static TutorialStepData CreateStep(string stepId, string title, string description, string targetPanel, string actionLabel, int order)
    {
        return new TutorialStepData
        {
            StepId = stepId,
            Title = title,
            Description = description,
            TargetPanel = targetPanel,
            ActionLabel = actionLabel,
            IsOptional = false,
            Order = order,
            CompletedAtUtc = ""
        };
    }

    private static TutorialHintData CreateHint(string panelId, string title, string body, int priority)
    {
        return new TutorialHintData
        {
            HintId = "hint-" + NormalizePanelId(panelId),
            PanelId = NormalizePanelId(panelId),
            Title = title,
            Body = body,
            Priority = priority,
            CanDismiss = true
        };
    }
}
