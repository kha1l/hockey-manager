using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class InitialSceneCreator
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string TeamSelectScenePath = "Assets/Scenes/TeamSelect.unity";
    private const string GameScenePath = "Assets/Scenes/Game.unity";

    [MenuItem("Tools/NHL Manager/Create Initial Scenes")]
    public static void CreateInitialScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EnsureScenesFolder();

        CreateMainMenuScene();
        CreateTeamSelectScene();
        CreateGameScene();
        AddScenesToBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("NHL Manager: initial scenes created.");
    }

    private static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        MainMenuController controller = new GameObject("MainMenuController").AddComponent<MainMenuController>();

        CreateText(canvas.transform, "Title", "Hockey Manager", 44, new Vector2(0f, 170f), new Vector2(520f, 80f));

        Button newGameButton = CreateButton(canvas.transform, "NewGameButton", "Новая игра", new Vector2(0f, 70f), new Vector2(420f, 50f));
        UnityEventTools.AddPersistentListener(newGameButton.onClick, controller.StartNewGame);

        Button loadGameButton = CreateButton(canvas.transform, "LoadGameButton", "Загрузить", new Vector2(0f, 10f), new Vector2(420f, 50f));
        UnityEventTools.AddPersistentListener(loadGameButton.onClick, controller.LoadGame);

        Button settingsButton = CreateButton(canvas.transform, "SettingsButton", "Настройки", new Vector2(0f, -50f), new Vector2(420f, 50f));
        UnityEventTools.AddPersistentListener(settingsButton.onClick, controller.OpenSettings);

        Button exitButton = CreateButton(canvas.transform, "ExitButton", "Выход", new Vector2(0f, -110f), new Vector2(420f, 50f));
        UnityEventTools.AddPersistentListener(exitButton.onClick, controller.ExitGame);

        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
    }

    private static void CreateTeamSelectScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "TeamSelect";

        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        TeamSelectController controller = new GameObject("TeamSelectController").AddComponent<TeamSelectController>();

        CreateText(canvas.transform, "Title", "Выбор команды", 40, new Vector2(0f, 820f), new Vector2(640f, 80f));

        Transform teamsContainer = CreateTeamScrollView(canvas.transform);
        TeamButtonView teamButtonTemplate = CreateTeamButtonTemplate(teamsContainer);
        controller.Configure(teamsContainer, teamButtonTemplate);

        Button backButton = CreateButton(canvas.transform, "BackButton", "Назад", new Vector2(0f, -820f), new Vector2(320f, 56f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.BackToMainMenu);

        EditorSceneManager.SaveScene(scene, TeamSelectScenePath);
    }

    private static void CreateGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Game";

        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        GameBootstrap gameBootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
        GameScreenController gameScreenController = new GameObject("GameScreenController").AddComponent<GameScreenController>();

        GameObject dashboardPanel = CreatePanel(canvas.transform, "DashboardPanel", Vector2.zero, new Vector2(900f, 1500f));
        Text currentDayText;
        Text gamesSimulatedText;
        Text nextGameText;
        Text lastMatchResultText;
        Text seasonRulesText;
        Text financeText;
        Text leagueDateText;
        Text tradeStatusText;
        Text freeAgencyStatusText;
        Text selectedTeamText = CreateDashboardPanel(
            dashboardPanel.transform,
            gameScreenController,
            out currentDayText,
            out gamesSimulatedText,
            out nextGameText,
            out lastMatchResultText,
            out seasonRulesText,
            out financeText,
            out leagueDateText,
            out tradeStatusText,
            out freeAgencyStatusText);

        GameObject rosterPanel = CreatePanel(canvas.transform, "RosterPanel", Vector2.zero, new Vector2(920f, 1580f));
        RosterController rosterController = rosterPanel.AddComponent<RosterController>();
        CreateRosterPanel(rosterPanel.transform, gameScreenController, rosterController);

        GameObject lineupPanel = CreatePanel(canvas.transform, "LineupPanel", Vector2.zero, new Vector2(920f, 1580f));
        LineupController lineupController = lineupPanel.AddComponent<LineupController>();
        CreateLineupPanel(lineupPanel.transform, gameScreenController, lineupController);

        GameObject rolesPanel = CreatePanel(canvas.transform, "RolesPanel", Vector2.zero, new Vector2(920f, 1580f));
        RolesController rolesController = rolesPanel.AddComponent<RolesController>();
        CreateRolesPanel(rolesPanel.transform, gameScreenController, rolesController);

        GameObject tacticsPanel = CreatePanel(canvas.transform, "TacticsPanel", Vector2.zero, new Vector2(920f, 1580f));
        TacticsController tacticsController = tacticsPanel.AddComponent<TacticsController>();
        CreateTacticsPanel(tacticsPanel.transform, gameScreenController, tacticsController);

        GameObject injuriesPanel = CreatePanel(canvas.transform, "InjuriesPanel", Vector2.zero, new Vector2(920f, 1580f));
        InjuriesController injuriesController = injuriesPanel.AddComponent<InjuriesController>();
        CreateInjuriesPanel(injuriesPanel.transform, gameScreenController, injuriesController);

        GameObject contractsPanel = CreatePanel(canvas.transform, "ContractsPanel", Vector2.zero, new Vector2(920f, 1580f));
        ContractsController contractsController = contractsPanel.AddComponent<ContractsController>();
        CreateContractsPanel(contractsPanel.transform, gameScreenController, contractsController);

        GameObject tradesPanel = CreatePanel(canvas.transform, "TradesPanel", Vector2.zero, new Vector2(920f, 1580f));
        TradesController tradesController = tradesPanel.AddComponent<TradesController>();
        CreateTradesPanel(tradesPanel.transform, gameScreenController, tradesController);

        GameObject freeAgencyPanel = CreatePanel(canvas.transform, "FreeAgencyPanel", Vector2.zero, new Vector2(920f, 1580f));
        FreeAgencyController freeAgencyController = freeAgencyPanel.AddComponent<FreeAgencyController>();
        CreateFreeAgencyPanel(freeAgencyPanel.transform, gameScreenController, freeAgencyController);

        GameObject draftPanel = CreatePanel(canvas.transform, "DraftPanel", Vector2.zero, new Vector2(920f, 1580f));
        DraftController draftController = draftPanel.AddComponent<DraftController>();
        CreateDraftPanel(draftPanel.transform, gameScreenController, draftController);

        GameObject prospectRightsPanel = CreatePanel(canvas.transform, "ProspectRightsPanel", Vector2.zero, new Vector2(920f, 1580f));
        ProspectRightsController prospectRightsController = prospectRightsPanel.AddComponent<ProspectRightsController>();
        CreateProspectRightsPanel(prospectRightsPanel.transform, gameScreenController, prospectRightsController);

        GameObject offseasonPanel = CreatePanel(canvas.transform, "OffseasonPanel", Vector2.zero, new Vector2(920f, 1580f));
        OffseasonController offseasonController = offseasonPanel.AddComponent<OffseasonController>();
        CreateOffseasonPanel(offseasonPanel.transform, gameScreenController, offseasonController);

        GameObject developmentPanel = CreatePanel(canvas.transform, "DevelopmentPanel", Vector2.zero, new Vector2(920f, 1580f));
        DevelopmentController developmentController = developmentPanel.AddComponent<DevelopmentController>();
        CreateDevelopmentPanel(developmentPanel.transform, gameScreenController, developmentController);

        GameObject calendarPanel = CreatePanel(canvas.transform, "CalendarPanel", Vector2.zero, new Vector2(920f, 1580f));
        CalendarController calendarController = calendarPanel.AddComponent<CalendarController>();
        CreateCalendarPanel(calendarPanel.transform, gameScreenController, calendarController);

        GameObject standingsPanel = CreatePanel(canvas.transform, "StandingsPanel", Vector2.zero, new Vector2(920f, 1580f));
        StandingsController standingsController = standingsPanel.AddComponent<StandingsController>();
        CreateStandingsPanel(standingsPanel.transform, gameScreenController, standingsController);

        GameObject playerStatsPanel = CreatePanel(canvas.transform, "PlayerStatsPanel", Vector2.zero, new Vector2(920f, 1580f));
        PlayerStatsController playerStatsController = playerStatsPanel.AddComponent<PlayerStatsController>();
        CreatePlayerStatsPanel(playerStatsPanel.transform, gameScreenController, playerStatsController);

        GameObject playoffsPanel = CreatePanel(canvas.transform, "PlayoffsPanel", Vector2.zero, new Vector2(920f, 1580f));
        PlayoffsController playoffsController = playoffsPanel.AddComponent<PlayoffsController>();
        CreatePlayoffsPanel(playoffsPanel.transform, gameScreenController, playoffsController);

        gameBootstrap.Configure(selectedTeamText);
        gameScreenController.Configure(
            gameBootstrap,
            selectedTeamText,
            currentDayText,
            gamesSimulatedText,
            nextGameText,
            lastMatchResultText,
            seasonRulesText,
            financeText,
            leagueDateText,
            tradeStatusText,
            freeAgencyStatusText,
            dashboardPanel,
            rosterPanel,
            lineupPanel,
            tacticsPanel,
            contractsPanel,
            tradesPanel,
            freeAgencyPanel,
            draftPanel,
            prospectRightsPanel,
            offseasonPanel,
            developmentPanel,
            rolesPanel,
            calendarPanel,
            injuriesPanel,
            standingsPanel,
            playerStatsPanel,
            playoffsPanel,
            rosterController,
            contractsController,
            tradesController,
            freeAgencyController,
            draftController,
            prospectRightsController,
            offseasonController,
            developmentController,
            rolesController,
            lineupController,
            tacticsController,
            injuriesController,
            calendarController,
            standingsController,
            playerStatsController,
            playoffsController);

        rosterPanel.SetActive(false);
        lineupPanel.SetActive(false);
        rolesPanel.SetActive(false);
        tacticsPanel.SetActive(false);
        injuriesPanel.SetActive(false);
        contractsPanel.SetActive(false);
        tradesPanel.SetActive(false);
        freeAgencyPanel.SetActive(false);
        draftPanel.SetActive(false);
        prospectRightsPanel.SetActive(false);
        offseasonPanel.SetActive(false);
        developmentPanel.SetActive(false);
        calendarPanel.SetActive(false);
        standingsPanel.SetActive(false);
        playerStatsPanel.SetActive(false);
        playoffsPanel.SetActive(false);

        EditorSceneManager.SaveScene(scene, GameScenePath);
    }

    private static Text CreateDashboardPanel(
        Transform parent,
        GameScreenController controller,
        out Text currentDayText,
        out Text gamesSimulatedText,
        out Text nextGameText,
        out Text lastMatchResultText,
        out Text seasonRulesText,
        out Text financeText,
        out Text leagueDateText,
        out Text tradeStatusText,
        out Text freeAgencyStatusText)
    {
        CreateText(parent, "Title", "Экран клуба", 42, new Vector2(0f, 685f), new Vector2(760f, 64f));
        Text selectedTeamText = CreateText(parent, "SelectedTeamText", "Команда не выбрана", 23, new Vector2(0f, 636f), new Vector2(820f, 42f));
        seasonRulesText = CreateText(parent, "SeasonRulesText", "Сезон: 2026-27 | Сезон карьеры: 1 | Игр: 84\nФаза: RegularSeason | Следующий сезон пока недоступен | Архивных сезонов: 0\nРазвитие игроков: 0 изменений за последний сезон\nСостав на матч: валиден\nТактика: Balanced | PP 0 | PK 0", 15, new Vector2(0f, 557f), new Vector2(820f, 138f));
        leagueDateText = CreateText(parent, "LeagueDateText", "Дата лиги: 2026-09-28 | Trade deadline: 2027-03-05", 18, new Vector2(0f, 478f), new Vector2(820f, 34f));
        tradeStatusText = CreateText(parent, "TradeStatusText", "Обмены доступны", 18, new Vector2(0f, 446f), new Vector2(820f, 34f));
        freeAgencyStatusText = CreateText(parent, "FreeAgencyStatusText", "Free agency: закрыта", 17, new Vector2(0f, 414f), new Vector2(820f, 34f));
        currentDayText = CreateText(parent, "CurrentDayText", "Игровой день: 1", 18, new Vector2(0f, 382f), new Vector2(820f, 34f));
        gamesSimulatedText = CreateText(parent, "GamesSimulatedText", "Матчей сыграно в лиге: 0 / 1344", 18, new Vector2(0f, 350f), new Vector2(820f, 34f));
        nextGameText = CreateText(parent, "NextGameText", "Следующий матч: ...", 18, new Vector2(0f, 318f), new Vector2(820f, 34f));
        lastMatchResultText = CreateText(parent, "LastMatchResultText", "Матчей ещё не было", 18, new Vector2(0f, 286f), new Vector2(820f, 34f));
        financeText = CreateText(parent, "FinanceText", "Зарплатная ведомость: 0 / 104 000 000\nМесто под потолком: 104 000 000\nМинимальный порог: 76 900 000\nПрава на проспектов: 0\nСостав: 23 / 23\nТравмы: 0\nСредняя готовность состава: 100\nСамый уставший: нет данных\nСтартовый вратарь: нет данных", 12, new Vector2(0f, 202f), new Vector2(820f, 148f));

        float buttonY = 96f;
        Button rosterButton = CreateDashboardButton(parent, "RosterButton", "Состав", ref buttonY);
        UnityEventTools.AddPersistentListener(rosterButton.onClick, controller.ShowRoster);

        Button lineupButton = CreateDashboardButton(parent, "LineupButton", "Линии", ref buttonY);
        UnityEventTools.AddPersistentListener(lineupButton.onClick, controller.ShowLineup);

        Button rolesButton = CreateDashboardButton(parent, "RolesButton", "Роли", ref buttonY);
        UnityEventTools.AddPersistentListener(rolesButton.onClick, controller.ShowRoles);

        Button tacticsButton = CreateDashboardButton(parent, "TacticsButton", "Тактика", ref buttonY);
        UnityEventTools.AddPersistentListener(tacticsButton.onClick, controller.ShowTactics);

        Button injuriesButton = CreateDashboardButton(parent, "InjuriesButton", "Травмы", ref buttonY);
        UnityEventTools.AddPersistentListener(injuriesButton.onClick, controller.ShowInjuries);

        Button contractsButton = CreateDashboardButton(parent, "ContractsButton", "Контракты", ref buttonY);
        UnityEventTools.AddPersistentListener(contractsButton.onClick, controller.ShowContracts);

        Button tradesButton = CreateDashboardButton(parent, "TradesButton", "Обмены", ref buttonY);
        UnityEventTools.AddPersistentListener(tradesButton.onClick, controller.ShowTrades);

        Button draftButton = CreateDashboardButton(parent, "DraftButton", "Драфт", ref buttonY);
        UnityEventTools.AddPersistentListener(draftButton.onClick, controller.ShowDraft);

        Button prospectRightsButton = CreateDashboardButton(parent, "ProspectRightsButton", "Права на проспектов", ref buttonY);
        UnityEventTools.AddPersistentListener(prospectRightsButton.onClick, controller.ShowProspectRights);

        Button freeAgencyButton = CreateDashboardButton(parent, "FreeAgencyButton", "Свободные агенты", ref buttonY);
        UnityEventTools.AddPersistentListener(freeAgencyButton.onClick, controller.ShowFreeAgency);

        Button offseasonButton = CreateDashboardButton(parent, "OffseasonButton", "Межсезонье", ref buttonY);
        UnityEventTools.AddPersistentListener(offseasonButton.onClick, controller.ShowOffseason);

        Button developmentButton = CreateDashboardButton(parent, "DevelopmentButton", "Развитие", ref buttonY);
        UnityEventTools.AddPersistentListener(developmentButton.onClick, controller.ShowDevelopment);

        Button calendarButton = CreateDashboardButton(parent, "CalendarButton", "Календарь", ref buttonY);
        UnityEventTools.AddPersistentListener(calendarButton.onClick, controller.ShowCalendar);

        Button standingsButton = CreateDashboardButton(parent, "StandingsButton", "Таблица", ref buttonY);
        UnityEventTools.AddPersistentListener(standingsButton.onClick, controller.ShowStandings);

        Button playerStatsButton = CreateDashboardButton(parent, "PlayerStatsButton", "Статистика", ref buttonY);
        UnityEventTools.AddPersistentListener(playerStatsButton.onClick, controller.ShowPlayerStats);

        Button playoffsButton = CreateDashboardButton(parent, "PlayoffsButton", "Плей-офф", ref buttonY);
        UnityEventTools.AddPersistentListener(playoffsButton.onClick, controller.ShowPlayoffs);

        Button simulateButton = CreateDashboardButton(parent, "SimulateMatchButton", "Симулировать игровой день", ref buttonY);
        UnityEventTools.AddPersistentListener(simulateButton.onClick, controller.SimulateMatch);

        Button simulateSeasonButton = CreateDashboardButton(parent, "SimulateSeasonButton", "Симулировать сезон до конца", ref buttonY);
        UnityEventTools.AddPersistentListener(simulateSeasonButton.onClick, controller.SimulateRegularSeasonToEnd);

        Button simulateToDraftButton = CreateDashboardButton(parent, "SimulateToDraftButton", "Симулировать до драфта", ref buttonY);
        UnityEventTools.AddPersistentListener(simulateToDraftButton.onClick, controller.SimulateToDraftForTesting);

        Button simulateToFreeAgencyButton = CreateDashboardButton(parent, "SimulateToFreeAgencyButton", "Симулировать до free agency", ref buttonY);
        UnityEventTools.AddPersistentListener(simulateToFreeAgencyButton.onClick, controller.SimulateToFreeAgencyForTesting);

        Button saveButton = CreateDashboardButton(parent, "SaveButton", "Сохранить", ref buttonY);
        UnityEventTools.AddPersistentListener(saveButton.onClick, controller.SaveGame);

        Button deleteSaveButton = CreateDashboardButton(parent, "DeleteSaveButton", "Удалить сохранение", ref buttonY);
        UnityEventTools.AddPersistentListener(deleteSaveButton.onClick, controller.DeleteSave);

        Button mainMenuButton = CreateDashboardButton(parent, "MainMenuButton", "Главное меню", ref buttonY);
        UnityEventTools.AddPersistentListener(mainMenuButton.onClick, controller.BackToMainMenu);

        return selectedTeamText;
    }

    private static void CreateRosterPanel(Transform parent, GameScreenController controller, RosterController rosterController)
    {
        CreateText(parent, "Title", "Состав команды", 40, new Vector2(0f, 720f), new Vector2(760f, 70f));

        CreateRosterHeader(parent);

        Transform playersContainer = CreateRosterScrollView(parent);
        PlayerRowView playerRowTemplate = CreatePlayerRowTemplate(playersContainer);
        rosterController.Configure(playersContainer, playerRowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 56f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateLineupPanel(Transform parent, GameScreenController controller, LineupController lineupController)
    {
        CreateText(parent, "Title", "Линии", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text statusText = CreateText(parent, "StatusText", "Состав на матч: ...", 15, new Vector2(0f, 650f), new Vector2(860f, 125f));
        Text ratingsText = CreateText(parent, "RatingsText", "Offense: 0 | Defense: 0 | Goalie: 0 | Total: 0", 15, new Vector2(0f, 555f), new Vector2(860f, 62f));
        Text selectedSlotText = CreateText(parent, "SelectedSlotText", "Выбранный слот: выберите слот в линиях", 16, new Vector2(0f, 505f), new Vector2(860f, 34f));
        Text selectedPlayerText = CreateText(parent, "SelectedPlayerText", "Выбранный игрок: выберите игрока для назначения", 15, new Vector2(0f, 460f), new Vector2(860f, 48f));

        CreateText(parent, "LineupSlotsLabel", "Активные слоты", 18, new Vector2(-230f, 420f), new Vector2(410f, 34f));
        CreateText(parent, "EligiblePlayersLabel", "Доступные игроки", 18, new Vector2(230f, 420f), new Vector2(410f, 34f));
        Transform lineupSlotsContainer = CreateDraftScrollView(parent, "LineupSlotsScrollView", new Vector2(420f, 720f), new Vector2(-230f, 40f));
        Transform eligiblePlayersContainer = CreateDraftScrollView(parent, "EligiblePlayersScrollView", new Vector2(420f, 720f), new Vector2(230f, 40f));
        LineupSlotRowView lineupSlotRowTemplate = CreateLineupSlotRowTemplate(lineupSlotsContainer);
        LineupEligiblePlayerRowView eligiblePlayerRowTemplate = CreateLineupEligiblePlayerRowTemplate(eligiblePlayersContainer);

        CreateText(parent, "ScratchesLabel", "Запасные", 18, new Vector2(0f, -350f), new Vector2(860f, 34f));
        Transform scratchesContainer = CreateDraftScrollView(parent, "ScratchPlayersScrollView", new Vector2(860f, 260f), new Vector2(0f, -510f));
        ScratchPlayerRowView scratchRowTemplate = CreateScratchPlayerRowTemplate(scratchesContainer);

        lineupController.Configure(
            statusText,
            ratingsText,
            selectedSlotText,
            selectedPlayerText,
            lineupSlotsContainer,
            eligiblePlayersContainer,
            scratchesContainer,
            lineupSlotRowTemplate,
            eligiblePlayerRowTemplate,
            scratchRowTemplate,
            controller);

        Button assignButton = CreateButton(parent, "AssignPlayerButton", "Назначить игрока", new Vector2(-330f, -700f), new Vector2(250f, 50f));
        SetButtonFontSize(assignButton, 15);
        UnityEventTools.AddPersistentListener(assignButton.onClick, controller.AssignSelectedPlayerToSelectedSlot);

        Button swapGoaliesButton = CreateButton(parent, "SwapGoaliesButton", "Поменять вратарей", new Vector2(-110f, -700f), new Vector2(250f, 50f));
        SetButtonFontSize(swapGoaliesButton, 15);
        UnityEventTools.AddPersistentListener(swapGoaliesButton.onClick, controller.SwapGoalies);

        Button clearButton = CreateButton(parent, "ClearSelectionButton", "Очистить выбор", new Vector2(110f, -700f), new Vector2(250f, 50f));
        SetButtonFontSize(clearButton, 15);
        UnityEventTools.AddPersistentListener(clearButton.onClick, controller.ClearLineupSelection);

        Button autoButton = CreateButton(parent, "AutoBuildLineupButton", "Автосостав", new Vector2(330f, -700f), new Vector2(220f, 50f));
        SetButtonFontSize(autoButton, 15);
        UnityEventTools.AddPersistentListener(autoButton.onClick, controller.AutoBuildLineup);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -760f), new Vector2(320f, 50f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateRolesPanel(Transform parent, GameScreenController controller, RolesController rolesController)
    {
        CreateText(parent, "Title", "Роли и игровое время", 40, new Vector2(0f, 735f), new Vector2(860f, 64f));
        Text summaryText = CreateText(parent, "SummaryText", "Команда\nСреднее TOI активных: 00:00", 17, new Vector2(0f, 642f), new Vector2(860f, 126f));
        Text selectedPlayerText = CreateText(parent, "SelectedPlayerText", "Выберите игрока для изменения роли", 16, new Vector2(0f, 530f), new Vector2(860f, 96f));

        CreateText(parent, "PlayersLabel", "Игроки", 18, new Vector2(0f, 465f), new Vector2(860f, 34f));
        Transform playersContainer = CreateDraftScrollView(parent, "RolePlayersScrollView", new Vector2(860f, 550f), new Vector2(0f, 170f));
        RolePlayerRowView rowTemplate = CreateRolePlayerRowTemplate(playersContainer);

        rolesController.Configure(summaryText, selectedPlayerText, playersContainer, rowTemplate, controller);

        float firstColumnX = -230f;
        float secondColumnX = 230f;
        float y = -160f;
        Button sniperButton = CreateButton(parent, "SniperButton", "Sniper", new Vector2(firstColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(sniperButton, 14);
        UnityEventTools.AddPersistentListener(sniperButton.onClick, controller.SetSelectedPlayerRoleSniper);

        Button offensiveDefenseButton = CreateButton(parent, "OffensiveDefenseButton", "Offensive D", new Vector2(secondColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(offensiveDefenseButton, 14);
        UnityEventTools.AddPersistentListener(offensiveDefenseButton.onClick, controller.SetSelectedPlayerRoleOffensiveDefenseman);

        y -= 50f;
        Button playmakerButton = CreateButton(parent, "PlaymakerButton", "Playmaker", new Vector2(firstColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(playmakerButton, 14);
        UnityEventTools.AddPersistentListener(playmakerButton.onClick, controller.SetSelectedPlayerRolePlaymaker);

        Button defensiveDefenseButton = CreateButton(parent, "DefensiveDefenseButton", "Defensive D", new Vector2(secondColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(defensiveDefenseButton, 14);
        UnityEventTools.AddPersistentListener(defensiveDefenseButton.onClick, controller.SetSelectedPlayerRoleDefensiveDefenseman);

        y -= 50f;
        Button powerForwardButton = CreateButton(parent, "PowerForwardButton", "Power Forward", new Vector2(firstColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(powerForwardButton, 14);
        UnityEventTools.AddPersistentListener(powerForwardButton.onClick, controller.SetSelectedPlayerRolePowerForward);

        Button twoWayDefenseButton = CreateButton(parent, "TwoWayDefenseButton", "Two-Way D", new Vector2(secondColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(twoWayDefenseButton, 14);
        UnityEventTools.AddPersistentListener(twoWayDefenseButton.onClick, controller.SetSelectedPlayerRoleTwoWayDefenseman);

        y -= 50f;
        Button twoWayForwardButton = CreateButton(parent, "TwoWayForwardButton", "Two-Way Forward", new Vector2(firstColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(twoWayForwardButton, 14);
        UnityEventTools.AddPersistentListener(twoWayForwardButton.onClick, controller.SetSelectedPlayerRoleTwoWayForward);

        Button stayAtHomeButton = CreateButton(parent, "StayAtHomeDefenseButton", "Stay-at-home D", new Vector2(secondColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(stayAtHomeButton, 14);
        UnityEventTools.AddPersistentListener(stayAtHomeButton.onClick, controller.SetSelectedPlayerRoleStayAtHomeDefenseman);

        y -= 50f;
        Button grinderButton = CreateButton(parent, "GrinderButton", "Grinder", new Vector2(firstColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(grinderButton, 14);
        UnityEventTools.AddPersistentListener(grinderButton.onClick, controller.SetSelectedPlayerRoleGrinder);

        Button starterGoalieButton = CreateButton(parent, "StarterGoalieButton", "Starter Goalie", new Vector2(secondColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(starterGoalieButton, 14);
        UnityEventTools.AddPersistentListener(starterGoalieButton.onClick, controller.SetSelectedPlayerRoleStarterGoalie);

        y -= 50f;
        Button depthForwardButton = CreateButton(parent, "DepthForwardButton", "Depth Forward", new Vector2(firstColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(depthForwardButton, 14);
        UnityEventTools.AddPersistentListener(depthForwardButton.onClick, controller.SetSelectedPlayerRoleDepthForward);

        Button backupGoalieButton = CreateButton(parent, "BackupGoalieButton", "Backup Goalie", new Vector2(secondColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(backupGoalieButton, 14);
        UnityEventTools.AddPersistentListener(backupGoalieButton.onClick, controller.SetSelectedPlayerRoleBackupGoalie);

        y -= 50f;
        Button depthGoalieButton = CreateButton(parent, "DepthGoalieButton", "Depth Goalie", new Vector2(secondColumnX, y), new Vector2(270f, 44f));
        SetButtonFontSize(depthGoalieButton, 14);
        UnityEventTools.AddPersistentListener(depthGoalieButton.onClick, controller.SetSelectedPlayerRoleDepthGoalie);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -735f), new Vector2(320f, 50f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateTacticsPanel(Transform parent, GameScreenController controller, TacticsController tacticsController)
    {
        CreateText(parent, "Title", "Тактика", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text presetText = CreateText(parent, "PresetText", "Команда\nPreset: Balanced", 20, new Vector2(0f, 660f), new Vector2(860f, 82f));
        Text parametersText = CreateText(parent, "ParametersText", "OffensiveFocus: 50 | DefensiveFocus: 50\nAggressiveness: 45 | Tempo: 50\nShootingFrequency: 50 | RiskLevel: 45", 16, new Vector2(0f, 560f), new Vector2(860f, 92f));
        Text ratingsText = CreateText(parent, "RatingsText", "PP rating: 0 | PK rating: 0\nСпецбригады: ...", 16, new Vector2(0f, 455f), new Vector2(860f, 100f));

        CreateText(parent, "PowerPlayLabel", "Power Play units", 18, new Vector2(0f, 382f), new Vector2(860f, 34f));
        Transform powerPlayContainer = CreateDraftScrollView(parent, "PowerPlayUnitsScrollView", new Vector2(860f, 250f), new Vector2(0f, 235f));
        PowerPlayUnitRowView powerPlayRowTemplate = CreatePowerPlayUnitRowTemplate(powerPlayContainer);

        CreateText(parent, "PenaltyKillLabel", "Penalty Kill units", 18, new Vector2(0f, 74f), new Vector2(860f, 34f));
        Transform penaltyKillContainer = CreateDraftScrollView(parent, "PenaltyKillUnitsScrollView", new Vector2(860f, 250f), new Vector2(0f, -72f));
        PenaltyKillUnitRowView penaltyKillRowTemplate = CreatePenaltyKillUnitRowTemplate(penaltyKillContainer);

        tacticsController.Configure(
            presetText,
            parametersText,
            ratingsText,
            powerPlayContainer,
            penaltyKillContainer,
            powerPlayRowTemplate,
            penaltyKillRowTemplate);

        Button balancedButton = CreateButton(parent, "BalancedButton", "Balanced", new Vector2(-315f, -515f), new Vector2(190f, 52f));
        UnityEventTools.AddPersistentListener(balancedButton.onClick, controller.SetBalancedTactics);

        Button offensiveButton = CreateButton(parent, "OffensiveButton", "Offensive", new Vector2(-105f, -515f), new Vector2(190f, 52f));
        UnityEventTools.AddPersistentListener(offensiveButton.onClick, controller.SetOffensiveTactics);

        Button defensiveButton = CreateButton(parent, "DefensiveButton", "Defensive", new Vector2(105f, -515f), new Vector2(190f, 52f));
        UnityEventTools.AddPersistentListener(defensiveButton.onClick, controller.SetDefensiveTactics);

        Button aggressiveButton = CreateButton(parent, "AggressiveButton", "Aggressive", new Vector2(315f, -515f), new Vector2(190f, 52f));
        UnityEventTools.AddPersistentListener(aggressiveButton.onClick, controller.SetAggressiveTactics);

        Button autoButton = CreateButton(parent, "AutoBuildSpecialTeamsButton", "Автоспецбригады", new Vector2(-180f, -620f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(autoButton.onClick, controller.AutoBuildSpecialTeams);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(180f, -620f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateInjuriesPanel(
        Transform parent,
        GameScreenController controller,
        InjuriesController injuriesController)
    {
        CreateText(parent, "Title", "Травмы", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text summaryText = CreateText(parent, "SummaryText", "Команда: ...\nАктивных травм: 0\nСтатус состава: ...", 18, new Vector2(0f, 620f), new Vector2(860f, 130f));

        CreateText(parent, "ActiveInjuriesLabel", "Активные травмы", 22, new Vector2(0f, 510f), new Vector2(860f, 42f));
        Transform activeInjuriesContainer = CreateDraftScrollView(parent, "ActiveInjuriesScrollView", new Vector2(860f, 500f), new Vector2(0f, 225f));
        InjuryRowView activeInjuryRowTemplate = CreateInjuryRowTemplate(activeInjuriesContainer, "ActiveInjuryRowTemplate");

        CreateText(parent, "InjuryHistoryLabel", "История травм", 22, new Vector2(0f, -70f), new Vector2(860f, 42f));
        Transform historyContainer = CreateDraftScrollView(parent, "InjuryHistoryScrollView", new Vector2(860f, 430f), new Vector2(0f, -330f));
        InjuryRowView historyRowTemplate = CreateInjuryRowTemplate(historyContainer, "InjuryHistoryRowTemplate");

        injuriesController.Configure(
            summaryText,
            activeInjuriesContainer,
            historyContainer,
            activeInjuryRowTemplate,
            historyRowTemplate);

        Button autoButton = CreateButton(parent, "AutoBuildLineupButton", "Автосостав", new Vector2(-180f, -705f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(autoButton.onClick, controller.AutoBuildLineup);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(180f, -705f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateContractsPanel(Transform parent, GameScreenController controller, ContractsController contractsController)
    {
        CreateText(parent, "Title", "Контракты", 40, new Vector2(0f, 720f), new Vector2(760f, 70f));
        Text financeText = CreateText(parent, "FinanceText", "Season ruleset: 2026-27\nSalaryCapUpperLimit: 104 000 000\nSalaryCapLowerLimit: 76 900 000\nLeagueMinimumSalary: 850 000\nMaximumPlayerSalary: 20 800 000", 18, new Vector2(0f, 570f), new Vector2(860f, 210f));
        Text messageText = CreateText(parent, "MessageText", "", 20, new Vector2(0f, 438f), new Vector2(840f, 44f));

        Transform contractsContainer = CreateContractsScrollView(parent);
        ContractRowView contractRowTemplate = CreateContractRowTemplate(contractsContainer);
        contractsController.Configure(financeText, messageText, contractsContainer, contractRowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 56f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateTradesPanel(Transform parent, GameScreenController controller, TradesController tradesController)
    {
        CreateText(parent, "Title", "Обмены", 40, new Vector2(0f, 730f), new Vector2(760f, 64f));
        Text dateText = CreateText(parent, "DateText", "Дата лиги: 2026-09-28\nTrade deadline: 2027-03-05", 18, new Vector2(0f, 660f), new Vector2(860f, 60f));
        Text statusText = CreateText(parent, "StatusText", "Обмены доступны", 18, new Vector2(0f, 607f), new Vector2(860f, 34f));
        Text selectedUserPlayerText = CreateText(parent, "SelectedUserPlayerText", "Ваш игрок: Выберите игрока", 15, new Vector2(0f, 574f), new Vector2(860f, 28f));
        Text selectedUserPickText = CreateText(parent, "SelectedUserPickText", "Ваш пик: Выберите пик", 15, new Vector2(0f, 544f), new Vector2(860f, 28f));
        Text selectedOtherTeamText = CreateText(parent, "SelectedOtherTeamText", "Команда-соперник: Выберите команду для обмена", 15, new Vector2(0f, 514f), new Vector2(860f, 28f));
        Text selectedOtherPlayerText = CreateText(parent, "SelectedOtherPlayerText", "Игрок соперника: Выберите игрока", 15, new Vector2(0f, 484f), new Vector2(860f, 28f));
        Text selectedOtherPickText = CreateText(parent, "SelectedOtherPickText", "Пик соперника: Выберите пик", 15, new Vector2(0f, 454f), new Vector2(860f, 28f));

        CreateText(parent, "UserPlayersLabel", "Ваши игроки", 17, new Vector2(-230f, 414f), new Vector2(410f, 28f));
        CreateText(parent, "TeamsLabel", "Команды", 17, new Vector2(230f, 414f), new Vector2(410f, 28f));
        CreateText(parent, "UserPicksLabel", "Ваши пики", 17, new Vector2(-230f, 128f), new Vector2(410f, 28f));
        CreateText(parent, "OtherPlayersLabel", "Игроки соперника", 17, new Vector2(230f, 128f), new Vector2(410f, 28f));
        CreateText(parent, "OtherPicksLabel", "Пики соперника", 17, new Vector2(-230f, -176f), new Vector2(410f, 28f));
        CreateText(parent, "HistoryLabel", "История обменов", 17, new Vector2(230f, -176f), new Vector2(410f, 28f));

        Transform userPlayersContainer = CreateTradeScrollView(parent, "UserPlayersScrollView", new Vector2(420f, 250f), new Vector2(-230f, 270f));
        Transform otherTeamsContainer = CreateTradeScrollView(parent, "OtherTeamsScrollView", new Vector2(420f, 250f), new Vector2(230f, 270f));
        Transform userPicksContainer = CreateTradeScrollView(parent, "UserPicksScrollView", new Vector2(420f, 250f), new Vector2(-230f, -15f));
        Transform otherPlayersContainer = CreateTradeScrollView(parent, "OtherPlayersScrollView", new Vector2(420f, 250f), new Vector2(230f, -15f));
        Transform otherPicksContainer = CreateTradeScrollView(parent, "OtherPicksScrollView", new Vector2(420f, 270f), new Vector2(-230f, -340f));
        Transform historyContainer = CreateTradeScrollView(parent, "TradeHistoryScrollView", new Vector2(420f, 270f), new Vector2(230f, -340f));

        TradePlayerRowView userPlayerRowTemplate = CreateTradePlayerRowTemplate(userPlayersContainer, "UserPlayerRowTemplate");
        TradeDraftPickRowView userPickRowTemplate = CreateTradeDraftPickRowTemplate(userPicksContainer, "UserPickRowTemplate");
        TradeTeamRowView teamRowTemplate = CreateTradeTeamRowTemplate(otherTeamsContainer);
        TradePlayerRowView otherPlayerRowTemplate = CreateTradePlayerRowTemplate(otherPlayersContainer, "OtherPlayerRowTemplate");
        TradeDraftPickRowView otherPickRowTemplate = CreateTradeDraftPickRowTemplate(otherPicksContainer, "OtherPickRowTemplate");
        TradeHistoryRowView historyRowTemplate = CreateTradeHistoryRowTemplate(historyContainer);

        tradesController.Configure(
            dateText,
            statusText,
            selectedUserPlayerText,
            selectedUserPickText,
            selectedOtherTeamText,
            selectedOtherPlayerText,
            selectedOtherPickText,
            userPlayersContainer,
            userPicksContainer,
            otherTeamsContainer,
            otherPlayersContainer,
            otherPicksContainer,
            historyContainer,
            userPlayerRowTemplate,
            userPickRowTemplate,
            teamRowTemplate,
            otherPlayerRowTemplate,
            otherPickRowTemplate,
            historyRowTemplate,
            controller);

        Button proposeButton = CreateButton(parent, "ProposeTradeButton", "Предложить обмен", new Vector2(0f, -650f), new Vector2(520f, 54f));
        UnityEventTools.AddPersistentListener(proposeButton.onClick, controller.ExecuteSelectedTrade);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateFreeAgencyPanel(Transform parent, GameScreenController controller, FreeAgencyController freeAgencyController)
    {
        CreateText(parent, "Title", "Свободные агенты", 40, new Vector2(0f, 730f), new Vector2(760f, 64f));
        Text statusText = CreateText(parent, "StatusText", "Рынок свободных агентов откроется после завершения драфта", 18, new Vector2(0f, 630f), new Vector2(860f, 120f));
        Text financeText = CreateText(parent, "FinanceText", "Salary cap: 104 000 000\nPayroll: 0\nCap space: 104 000 000\nRoster size: 0 / 23", 17, new Vector2(0f, 520f), new Vector2(860f, 90f));
        Text selectedFreeAgentText = CreateText(parent, "SelectedFreeAgentText", "Выбранный свободный агент: не выбран", 17, new Vector2(0f, 430f), new Vector2(860f, 44f));

        CreateText(parent, "FreeAgentsLabel", "Рынок UFA", 20, new Vector2(0f, 378f), new Vector2(860f, 36f));
        Transform freeAgentsContainer = CreateFreeAgencyScrollView(parent);
        FreeAgentRowView freeAgentRowTemplate = CreateFreeAgentRowTemplate(freeAgentsContainer);

        CreateText(parent, "HistoryLabel", "История подписаний", 20, new Vector2(0f, -270f), new Vector2(860f, 36f));
        Transform historyContainer = CreateFreeAgentHistoryScrollView(parent);
        FreeAgentSigningHistoryRowView historyRowTemplate = CreateFreeAgentSigningHistoryRowTemplate(historyContainer);

        freeAgencyController.Configure(
            statusText,
            financeText,
            selectedFreeAgentText,
            freeAgentsContainer,
            historyContainer,
            freeAgentRowTemplate,
            historyRowTemplate,
            controller);

        Button signButton = CreateButton(parent, "SignFreeAgentButton", "Подписать выбранного игрока", new Vector2(0f, -620f), new Vector2(520f, 54f));
        UnityEventTools.AddPersistentListener(signButton.onClick, controller.SignSelectedFreeAgent);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -710f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateDraftPanel(Transform parent, GameScreenController controller, DraftController draftController)
    {
        CreateText(parent, "Title", "Драфт", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text statusText = CreateText(parent, "StatusText", "Драфт станет доступен после завершения плей-офф", 16, new Vector2(0f, 635f), new Vector2(860f, 120f));
        Text currentPickText = CreateText(parent, "CurrentPickText", "Текущий выбор: ...", 16, new Vector2(0f, 538f), new Vector2(860f, 40f));
        Text selectedProspectText = CreateText(parent, "SelectedProspectText", "Выбранный проспект: не выбран", 16, new Vector2(0f, 498f), new Vector2(860f, 36f));

        CreateText(parent, "ProspectsLabel", "Доступные проспекты", 17, new Vector2(-230f, 456f), new Vector2(410f, 28f));
        CreateText(parent, "RecentPicksLabel", "Последние выборы", 17, new Vector2(230f, 456f), new Vector2(410f, 28f));
        CreateText(parent, "DraftRightsLabel", "Права вашей команды", 17, new Vector2(230f, 148f), new Vector2(410f, 28f));
        CreateText(parent, "OwnedPicksLabel", "Ваши драфт-пики", 17, new Vector2(230f, -158f), new Vector2(410f, 28f));

        Transform prospectsContainer = CreateDraftScrollView(parent, "ProspectsScrollView", new Vector2(420f, 900f), new Vector2(-230f, -10f));
        Transform recentPicksContainer = CreateDraftScrollView(parent, "RecentPicksScrollView", new Vector2(420f, 275f), new Vector2(230f, 300f));
        Transform draftRightsContainer = CreateDraftScrollView(parent, "DraftRightsScrollView", new Vector2(420f, 275f), new Vector2(230f, -5f));
        Transform ownedPicksContainer = CreateDraftScrollView(parent, "OwnedPicksScrollView", new Vector2(420f, 250f), new Vector2(230f, -310f));

        ProspectRowView prospectRowTemplate = CreateProspectRowTemplate(prospectsContainer);
        DraftPickRowView draftPickRowTemplate = CreateDraftPickRowTemplate(recentPicksContainer);
        DraftRightsRowView draftRightsRowTemplate = CreateDraftRightsRowTemplate(draftRightsContainer);

        draftController.Configure(
            statusText,
            currentPickText,
            selectedProspectText,
            prospectsContainer,
            recentPicksContainer,
            draftRightsContainer,
            ownedPicksContainer,
            prospectRowTemplate,
            draftPickRowTemplate,
            draftRightsRowTemplate,
            controller);

        Button draftButton = CreateButton(parent, "DraftProspectButton", "Выбрать проспекта", new Vector2(-230f, -665f), new Vector2(410f, 52f));
        UnityEventTools.AddPersistentListener(draftButton.onClick, controller.DraftSelectedProspect);

        Button autoDraftButton = CreateButton(parent, "AutoDraftButton", "Автовыбор до моего пика", new Vector2(230f, -665f), new Vector2(410f, 52f));
        UnityEventTools.AddPersistentListener(autoDraftButton.onClick, controller.AutoDraftUntilUserPick);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -730f), new Vector2(320f, 52f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateProspectRightsPanel(
        Transform parent,
        GameScreenController controller,
        ProspectRightsController prospectRightsController)
    {
        CreateText(parent, "Title", "Права на проспектов", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text teamInfoText = CreateText(parent, "TeamInfoText", "Команда: ...", 16, new Vector2(0f, 620f), new Vector2(860f, 120f));
        Text selectedProspectText = CreateText(parent, "SelectedProspectText", "Выбранный проспект: не выбран", 16, new Vector2(0f, 500f), new Vector2(860f, 92f));

        CreateText(parent, "RightsLabel", "DraftRights", 18, new Vector2(0f, 420f), new Vector2(860f, 34f));
        Transform rightsContainer = CreateDraftScrollView(parent, "ProspectRightsScrollView", new Vector2(860f, 480f), new Vector2(0f, 160f));
        ProspectRightsRowView rightsRowTemplate = CreateProspectRightsRowTemplate(rightsContainer);

        CreateText(parent, "HistoryLabel", "История подписаний проспектов", 18, new Vector2(0f, -105f), new Vector2(860f, 34f));
        Transform historyContainer = CreateDraftScrollView(parent, "ProspectSigningHistoryScrollView", new Vector2(860f, 380f), new Vector2(0f, -350f));
        ProspectSigningHistoryRowView historyRowTemplate = CreateProspectSigningHistoryRowTemplate(historyContainer);

        prospectRightsController.Configure(
            teamInfoText,
            selectedProspectText,
            rightsContainer,
            historyContainer,
            rightsRowTemplate,
            historyRowTemplate,
            controller);

        Button signButton = CreateButton(parent, "SignProspectToElcButton", "Подписать ELC", new Vector2(0f, -610f), new Vector2(520f, 52f));
        UnityEventTools.AddPersistentListener(signButton.onClick, controller.SignSelectedProspectToElc);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -700f), new Vector2(320f, 52f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateOffseasonPanel(
        Transform parent,
        GameScreenController controller,
        OffseasonController offseasonController)
    {
        CreateText(parent, "Title", "Межсезонье", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text statusText = CreateText(parent, "StatusText", "Фаза: RegularSeason\nТекущий сезон: 2026-27\nСледующий сезон: 2027-28\nСтатус: Завершите плей-офф и драфт", 18, new Vector2(0f, 595f), new Vector2(860f, 180f));
        Text summaryText = CreateText(parent, "SummaryText", "Expiring: 0 | RFA: 0 | UFA: 0\nRoster: 0 / 23\nPayroll: 0 | Cap space: 104 000 000", 18, new Vector2(0f, 430f), new Vector2(860f, 100f));

        CreateText(parent, "HistoryLabel", "История сезонов", 22, new Vector2(0f, 340f), new Vector2(860f, 42f));
        Transform historyContainer = CreateDraftScrollView(parent, "SeasonHistoryScrollView", new Vector2(860f, 560f), new Vector2(0f, 30f));
        SeasonHistoryRowView historyRowTemplate = CreateSeasonHistoryRowTemplate(historyContainer);

        offseasonController.Configure(
            statusText,
            summaryText,
            historyContainer,
            historyRowTemplate);

        Button startNextSeasonButton = CreateButton(parent, "StartNextSeasonButton", "Начать следующий сезон", new Vector2(0f, -600f), new Vector2(520f, 54f));
        UnityEventTools.AddPersistentListener(startNextSeasonButton.onClick, controller.StartNextSeason);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -700f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateDevelopmentPanel(
        Transform parent,
        GameScreenController controller,
        DevelopmentController developmentController)
    {
        CreateText(parent, "Title", "Развитие игроков", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text summaryText = CreateText(parent, "SummaryText", "История развития пока пуста. Завершите сезон и начните следующий.", 18, new Vector2(0f, 610f), new Vector2(860f, 150f));

        CreateText(parent, "ChangesLabel", "Последние изменения", 22, new Vector2(0f, 500f), new Vector2(860f, 42f));
        Transform changesContainer = CreateDraftScrollView(parent, "DevelopmentChangesScrollView", new Vector2(860f, 1000f), new Vector2(0f, -35f));
        DevelopmentRowView rowTemplate = CreateDevelopmentRowTemplate(changesContainer);

        developmentController.Configure(
            summaryText,
            changesContainer,
            rowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateCalendarPanel(Transform parent, GameScreenController controller, CalendarController calendarController)
    {
        CreateText(parent, "Title", "Календарь", 40, new Vector2(0f, 720f), new Vector2(760f, 70f));
        CreateCalendarHeader(parent);

        Transform gamesContainer = CreateCalendarScrollView(parent);
        ScheduleGameRowView gameRowTemplate = CreateScheduleGameRowTemplate(gamesContainer);
        calendarController.Configure(gamesContainer, gameRowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 56f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateStandingsPanel(Transform parent, GameScreenController controller, StandingsController standingsController)
    {
        CreateText(parent, "Title", "Турнирная таблица", 40, new Vector2(0f, 720f), new Vector2(760f, 70f));
        CreateStandingsHeader(parent);

        Transform standingsContainer = CreateStandingsScrollView(parent);
        StandingRowView standingRowTemplate = CreateStandingRowTemplate(standingsContainer);
        standingsController.Configure(standingsContainer, standingRowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 56f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreatePlayerStatsPanel(Transform parent, GameScreenController controller, PlayerStatsController playerStatsController)
    {
        CreateText(parent, "Title", "Статистика игроков", 40, new Vector2(0f, 720f), new Vector2(760f, 70f));

        Transform statsContainer = CreatePlayerStatsScrollView(parent);
        PlayerStatsRowView statsRowTemplate = CreatePlayerStatsRowTemplate(statsContainer);
        playerStatsController.Configure(statsContainer, statsRowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 56f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreatePlayoffsPanel(Transform parent, GameScreenController controller, PlayoffsController playoffsController)
    {
        CreateText(parent, "Title", "Плей-офф", 40, new Vector2(0f, 720f), new Vector2(760f, 70f));
        Text statusText = CreateText(parent, "StatusText", "Плей-офф станет доступен после завершения регулярного сезона", 24, new Vector2(0f, 650f), new Vector2(840f, 56f));

        Transform seriesContainer = CreatePlayoffsScrollView(parent);
        PlayoffSeriesRowView seriesRowTemplate = CreatePlayoffSeriesRowTemplate(seriesContainer);
        playoffsController.Configure(statusText, seriesContainer, seriesRowTemplate);

        Button simulateButton = CreateButton(parent, "SimulatePlayoffButton", "Симулировать матч плей-офф", new Vector2(0f, -650f), new Vector2(520f, 56f));
        UnityEventTools.AddPersistentListener(simulateButton.onClick, controller.SimulatePlayoffGame);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 56f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        return canvas;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static GameObject CreatePanel(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panelObject = new GameObject(objectName);
        panelObject.transform.SetParent(parent, false);

        RectTransform rectTransform = panelObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        Image image = panelObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.1f, 0.13f, 0.92f);

        return panelObject;
    }

    private static Text CreateText(Transform parent, string objectName, string textValue, int fontSize, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        Text text = textObject.AddComponent<Text>();
        text.text = textValue;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        return text;
    }

    private static Button CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = new GameObject(objectName);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text buttonText = CreateText(buttonObject.transform, "Text", label, 24, Vector2.zero, rectTransform.sizeDelta);
        buttonText.color = Color.black;

        return button;
    }

    private static Button CreateDashboardButton(Transform parent, string objectName, string label, ref float y)
    {
        Button button = CreateButton(parent, objectName, label, new Vector2(0f, y), new Vector2(560f, 34f));
        SetButtonFontSize(button, 19);

        y -= 36f;
        return button;
    }

    private static void SetButtonFontSize(Button button, int fontSize)
    {
        Text buttonText = button == null ? null : button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.fontSize = fontSize;
        }
    }

    private static Transform CreateTeamScrollView(Transform parent)
    {
        return CreateVerticalScrollView(parent, "TeamScrollView", new Vector2(780f, 1280f), new Vector2(0f, -40f), 8f);
    }

    private static Transform CreateRosterScrollView(Transform parent)
    {
        return CreateVerticalScrollView(parent, "PlayerScrollView", new Vector2(840f, 1120f), new Vector2(0f, 0f), 6f);
    }

    private static Transform CreateContractsScrollView(Transform parent)
    {
        return CreateVerticalScrollView(parent, "ContractsScrollView", new Vector2(880f, 1000f), new Vector2(0f, -90f), 6f);
    }

    private static Transform CreateTradeScrollView(Transform parent, string objectName, Vector2 size, Vector2 anchoredPosition)
    {
        return CreateVerticalScrollView(parent, objectName, size, anchoredPosition, 5f);
    }

    private static Transform CreateFreeAgencyScrollView(Transform parent)
    {
        return CreateVerticalScrollView(parent, "FreeAgencyScrollView", new Vector2(860f, 590f), new Vector2(0f, 60f), 5f);
    }

    private static Transform CreateFreeAgentHistoryScrollView(Transform parent)
    {
        return CreateVerticalScrollView(parent, "FreeAgentHistoryScrollView", new Vector2(860f, 250f), new Vector2(0f, -425f), 5f);
    }

    private static Transform CreateDraftScrollView(Transform parent, string objectName, Vector2 size, Vector2 anchoredPosition)
    {
        return CreateVerticalScrollView(parent, objectName, size, anchoredPosition, 5f);
    }

    private static Transform CreateCalendarScrollView(Transform parent)
    {
        return CreateVerticalScrollView(parent, "CalendarScrollView", new Vector2(840f, 1120f), new Vector2(0f, 0f), 6f);
    }

    private static Transform CreateStandingsScrollView(Transform parent)
    {
        return CreateVerticalScrollView(parent, "StandingsScrollView", new Vector2(860f, 1120f), new Vector2(0f, 0f), 6f);
    }

    private static Transform CreatePlayerStatsScrollView(Transform parent)
    {
        return CreateVerticalScrollView(parent, "PlayerStatsScrollView", new Vector2(860f, 1240f), new Vector2(0f, -40f), 6f);
    }

    private static Transform CreatePlayoffsScrollView(Transform parent)
    {
        return CreateVerticalScrollView(parent, "PlayoffsScrollView", new Vector2(860f, 1120f), new Vector2(0f, -20f), 6f);
    }

    private static Transform CreateVerticalScrollView(Transform parent, string objectName, Vector2 size, Vector2 anchoredPosition, float spacing)
    {
        GameObject scrollViewObject = new GameObject(objectName);
        scrollViewObject.transform.SetParent(parent, false);

        RectTransform scrollViewRect = scrollViewObject.AddComponent<RectTransform>();
        scrollViewRect.sizeDelta = size;
        scrollViewRect.anchoredPosition = anchoredPosition;

        Image background = scrollViewObject.AddComponent<Image>();
        background.color = new Color(0.04f, 0.05f, 0.07f, 0.65f);

        ScrollRect scrollRect = scrollViewObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewportObject = new GameObject("Viewport");
        viewportObject.transform.SetParent(scrollViewObject.transform, false);

        RectTransform viewportRect = viewportObject.AddComponent<RectTransform>();
        Stretch(viewportRect, new Vector2(18f, 18f), new Vector2(-18f, -18f));

        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);

        Mask mask = viewportObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content");
        contentObject.transform.SetParent(viewportObject.transform, false);

        RectTransform contentRect = contentObject.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layoutGroup = contentObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = spacing;
        layoutGroup.padding = new RectOffset(0, 0, 0, 0);
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        ContentSizeFitter contentSizeFitter = contentObject.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        return contentObject.transform;
    }

    private static TeamButtonView CreateTeamButtonTemplate(Transform parent)
    {
        GameObject buttonObject = new GameObject("TeamButtonTemplate");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(720f, 72f);

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 72f;
        layoutElement.minHeight = 72f;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text abbreviationText = CreateText(buttonObject.transform, "AbbreviationText", "ANA", 24, new Vector2(-285f, 0f), new Vector2(90f, 52f));
        Text cityText = CreateText(buttonObject.transform, "CityText", "Anaheim", 20, new Vector2(-60f, 14f), new Vector2(420f, 28f));
        Text nameText = CreateText(buttonObject.transform, "NameText", "Ducks", 24, new Vector2(-60f, -16f), new Vector2(420f, 32f));

        abbreviationText.color = Color.black;
        cityText.color = Color.black;
        nameText.color = Color.black;

        TeamButtonView teamButtonView = buttonObject.AddComponent<TeamButtonView>();
        teamButtonView.Configure(button, cityText, nameText, abbreviationText);

        buttonObject.SetActive(false);
        return teamButtonView;
    }

    private static void CreateRosterHeader(Transform parent)
    {
        GameObject headerObject = new GameObject("RosterHeader");
        headerObject.transform.SetParent(parent, false);

        RectTransform rectTransform = headerObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(840f, 42f);
        rectTransform.anchoredPosition = new Vector2(0f, 620f);

        CreateRosterHeaderText(headerObject.transform, "NameHeader", "Игрок", new Vector2(-300f, 0f), new Vector2(230f, 40f));
        CreateRosterHeaderText(headerObject.transform, "PositionHeader", "Поз", new Vector2(-160f, 0f), new Vector2(50f, 40f));
        CreateRosterHeaderText(headerObject.transform, "AgeHeader", "Возраст", new Vector2(-105f, 0f), new Vector2(60f, 40f));
        CreateRosterHeaderText(headerObject.transform, "OverallHeader", "OVR / EFF", new Vector2(35f, 0f), new Vector2(210f, 40f));
        CreateRosterHeaderText(headerObject.transform, "PotentialHeader", "POT / COND / FAT", new Vector2(285f, 0f), new Vector2(280f, 40f));
    }

    private static void CreateCalendarHeader(Transform parent)
    {
        GameObject headerObject = new GameObject("CalendarHeader");
        headerObject.transform.SetParent(parent, false);

        RectTransform rectTransform = headerObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 42f);
        rectTransform.anchoredPosition = new Vector2(0f, 620f);

        CreateRosterHeaderText(headerObject.transform, "MatchupHeader", "Матчи лиги", Vector2.zero, new Vector2(800f, 40f));
    }

    private static void CreateStandingsHeader(Transform parent)
    {
        GameObject headerObject = new GameObject("StandingsHeader");
        headerObject.transform.SetParent(parent, false);

        RectTransform rectTransform = headerObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(840f, 42f);
        rectTransform.anchoredPosition = new Vector2(0f, 620f);

        CreateRosterHeaderText(headerObject.transform, "PlaceHeader", "#", new Vector2(-390f, 0f), new Vector2(45f, 40f));
        CreateRosterHeaderText(headerObject.transform, "TeamHeader", "Команда", new Vector2(-205f, 0f), new Vector2(280f, 40f));
        CreateRosterHeaderText(headerObject.transform, "GamesHeader", "И", new Vector2(-35f, 0f), new Vector2(45f, 40f));
        CreateRosterHeaderText(headerObject.transform, "WinsHeader", "В", new Vector2(25f, 0f), new Vector2(45f, 40f));
        CreateRosterHeaderText(headerObject.transform, "LossesHeader", "П", new Vector2(85f, 0f), new Vector2(45f, 40f));
        CreateRosterHeaderText(headerObject.transform, "OvertimeLossesHeader", "ОТ", new Vector2(145f, 0f), new Vector2(55f, 40f));
        CreateRosterHeaderText(headerObject.transform, "PointsHeader", "О", new Vector2(210f, 0f), new Vector2(45f, 40f));
        CreateRosterHeaderText(headerObject.transform, "GoalsForHeader", "ЗШ", new Vector2(275f, 0f), new Vector2(55f, 40f));
        CreateRosterHeaderText(headerObject.transform, "GoalsAgainstHeader", "ПШ", new Vector2(345f, 0f), new Vector2(55f, 40f));
    }

    private static void CreateRosterHeaderText(Transform parent, string objectName, string value, Vector2 anchoredPosition, Vector2 size)
    {
        Text text = CreateText(parent, objectName, value, 18, anchoredPosition, size);
        text.color = new Color(0.78f, 0.86f, 1f, 1f);
    }

    private static PlayerRowView CreatePlayerRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("PlayerRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(840f, 58f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 58f;
        layoutElement.minHeight = 58f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text nameText = CreatePlayerRowText(rowObject.transform, "NameText", "Player 1", new Vector2(-300f, 0f), new Vector2(230f, 48f));
        Text positionText = CreatePlayerRowText(rowObject.transform, "PositionText", "C", new Vector2(-160f, 0f), new Vector2(50f, 48f));
        Text ageText = CreatePlayerRowText(rowObject.transform, "AgeText", "19", new Vector2(-105f, 0f), new Vector2(60f, 48f));
        Text overallText = CreatePlayerRowText(rowObject.transform, "OverallText", "OVR 60 EFF 60", new Vector2(35f, 0f), new Vector2(210f, 48f));
        Text potentialText = CreatePlayerRowText(rowObject.transform, "PotentialText", "POT 70 | COND 100 | FAT 0", new Vector2(285f, 0f), new Vector2(280f, 48f));
        nameText.fontSize = 14;
        overallText.fontSize = 13;
        potentialText.fontSize = 12;

        PlayerRowView rowView = rowObject.AddComponent<PlayerRowView>();
        rowView.Configure(nameText, positionText, ageText, overallText, potentialText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static ContractRowView CreateContractRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ContractRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(840f, 74f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 74f;
        layoutElement.minHeight = 74f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player 1 | C | 19 лет | OVR 60 | $850 000 | 2 г. | Signed", new Vector2(-105f, 0f), new Vector2(600f, 62f));
        infoText.fontSize = 15;
        infoText.alignment = TextAnchor.MiddleLeft;

        Button extendButton = CreateButton(rowObject.transform, "ExtendButton", "Продлить +1 год", new Vector2(305f, 0f), new Vector2(210f, 52f));
        Text extendButtonText = extendButton.GetComponentInChildren<Text>();
        if (extendButtonText != null)
        {
            extendButtonText.fontSize = 16;
        }

        ContractRowView rowView = rowObject.AddComponent<ContractRowView>();
        rowView.Configure(infoText, extendButton);

        rowObject.SetActive(false);
        return rowView;
    }

    private static TradePlayerRowView CreateTradePlayerRowTemplate(Transform parent, string objectName)
    {
        GameObject rowObject = new GameObject(objectName);
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 58f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 58f;
        layoutElement.minHeight = 58f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player 1 | C | 19 | OVR 60 | $850 000 | 2 г.", Vector2.zero, new Vector2(360f, 50f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        TradePlayerRowView rowView = rowObject.AddComponent<TradePlayerRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static TradeDraftPickRowView CreateTradeDraftPickRowTemplate(Transform parent, string objectName)
    {
        GameObject rowObject = new GameObject(objectName);
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 58f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 58f;
        layoutElement.minHeight = 58f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "2027 | Round 1 | from Team | owner Team | Value 700", Vector2.zero, new Vector2(360f, 50f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        TradeDraftPickRowView rowView = rowObject.AddComponent<TradeDraftPickRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static TradeTeamRowView CreateTradeTeamRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("TradeTeamRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 58f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 58f;
        layoutElement.minHeight = 58f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Anaheim Ducks (ANA) | Payroll $0 | Cap $104 000 000", Vector2.zero, new Vector2(360f, 50f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        TradeTeamRowView rowView = rowObject.AddComponent<TradeTeamRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static TradeHistoryRowView CreateTradeHistoryRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("TradeHistoryRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 70f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 70f;
        layoutElement.minHeight = 70f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Team A <-> Team B | Player 1 <-> Player 2 | Accepted", Vector2.zero, new Vector2(360f, 62f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        TradeHistoryRowView rowView = rowObject.AddComponent<TradeHistoryRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static FreeAgentRowView CreateFreeAgentRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("FreeAgentRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 66f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 66f;
        layoutElement.minHeight = 66f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Free Agent 1 | C | 24 лет | OVR 70 | POT 80 | $1 000 000 | 1 г. | UFA", Vector2.zero, new Vector2(790f, 58f));
        infoText.fontSize = 13;
        infoText.alignment = TextAnchor.MiddleLeft;

        FreeAgentRowView rowView = rowObject.AddComponent<FreeAgentRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static FreeAgentSigningHistoryRowView CreateFreeAgentSigningHistoryRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("FreeAgentSigningHistoryRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 76f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 76f;
        layoutElement.minHeight = 76f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Free Agent 1 | Team | $1 000 000 | 1 г. | Accepted", Vector2.zero, new Vector2(790f, 54f));
        infoText.fontSize = 13;
        infoText.alignment = TextAnchor.MiddleLeft;

        FreeAgentSigningHistoryRowView rowView = rowObject.AddComponent<FreeAgentSigningHistoryRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static ProspectRowView CreateProspectRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ProspectRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 58f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 58f;
        layoutElement.minHeight = 58f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "#1 | Prospect 001 | C | Canada | 18 | OVR 70 | POT 90 | R1", Vector2.zero, new Vector2(360f, 50f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        ProspectRowView rowView = rowObject.AddComponent<ProspectRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static DraftPickRowView CreateDraftPickRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("DraftPickRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 58f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 58f;
        layoutElement.minHeight = 58f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "#1 | R1.1 | original Team | owner Team | Prospect 001 | Done", Vector2.zero, new Vector2(360f, 50f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        DraftPickRowView rowView = rowObject.AddComponent<DraftPickRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static DraftRightsRowView CreateDraftRightsRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("DraftRightsRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 58f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 58f;
        layoutElement.minHeight = 58f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Prospect 001 | C | 18 | OVR 70 | POT 90 | R1 | #1", Vector2.zero, new Vector2(360f, 50f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        DraftRightsRowView rowView = rowObject.AddComponent<DraftRightsRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static ProspectRightsRowView CreateProspectRightsRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ProspectRightsRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 66f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 66f;
        layoutElement.minHeight = 66f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Prospect 001 | C | 18 | OVR 70 | POT 90 | R1 | #1 | 3 г. ELC", Vector2.zero, new Vector2(790f, 58f));
        infoText.fontSize = 13;
        infoText.alignment = TextAnchor.MiddleLeft;

        ProspectRightsRowView rowView = rowObject.AddComponent<ProspectRightsRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static ProspectSigningHistoryRowView CreateProspectSigningHistoryRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ProspectSigningHistoryRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 68f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 68f;
        layoutElement.minHeight = 68f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Prospect 001 | Team | $850 000 | 3 г. | Accepted", Vector2.zero, new Vector2(790f, 60f));
        infoText.fontSize = 13;
        infoText.alignment = TextAnchor.MiddleLeft;

        ProspectSigningHistoryRowView rowView = rowObject.AddComponent<ProspectSigningHistoryRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static InjuryRowView CreateInjuryRowTemplate(Transform parent, string objectName)
    {
        GameObject rowObject = new GameObject(objectName);
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 72f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 72f;
        layoutElement.minHeight = 72f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player 1 | Team | C | Lower Body Injury | Minor | 5 дн. | Active", Vector2.zero, new Vector2(790f, 64f));
        infoText.fontSize = 13;
        infoText.alignment = TextAnchor.MiddleLeft;

        InjuryRowView rowView = rowObject.AddComponent<InjuryRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static SeasonHistoryRowView CreateSeasonHistoryRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("SeasonHistoryRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 68f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 68f;
        layoutElement.minHeight = 68f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "2026-27 | Чемпион: Team | User Team | Очки: 100 | Место: 8 | Плей-офф: да", Vector2.zero, new Vector2(790f, 60f));
        infoText.fontSize = 13;
        infoText.alignment = TextAnchor.MiddleLeft;

        SeasonHistoryRowView rowView = rowObject.AddComponent<SeasonHistoryRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static DevelopmentRowView CreateDevelopmentRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("DevelopmentRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 72f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 72f;
        layoutElement.minHeight = 72f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player 1 | Team | RosterPlayer | C | 22 | OVR 70 -> 73 (+3) | POT 84 -> 84 | Growth", Vector2.zero, new Vector2(790f, 64f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        DevelopmentRowView rowView = rowObject.AddComponent<DevelopmentRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static LineupSlotRowView CreateLineupSlotRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("LineupSlotRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 66f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 66f;
        layoutElement.minHeight = 66f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Forward 1 LW | Player | OVR/EFF/COND/FAT", Vector2.zero, new Vector2(360f, 58f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        LineupSlotRowView rowView = rowObject.AddComponent<LineupSlotRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static LineupEligiblePlayerRowView CreateLineupEligiblePlayerRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("LineupEligiblePlayerRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 66f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 66f;
        layoutElement.minHeight = 66f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player | Pos | OVR/POT/EFF/COND/FAT", Vector2.zero, new Vector2(360f, 58f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        LineupEligiblePlayerRowView rowView = rowObject.AddComponent<LineupEligiblePlayerRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static RolePlayerRowView CreateRolePlayerRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("RolePlayerRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 66f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 66f;
        layoutElement.minHeight = 66f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player | Pos | Role | Usage | TOI | COND/FAT", Vector2.zero, new Vector2(790f, 58f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        RolePlayerRowView rowView = rowObject.AddComponent<RolePlayerRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static ForwardLineRowView CreateForwardLineRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ForwardLineRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 62f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 62f;
        layoutElement.minHeight = 62f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Звено 1 | LW OVR/EFF/COND | C OVR/EFF/COND | RW OVR/EFF/COND | AVG", Vector2.zero, new Vector2(790f, 70f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        ForwardLineRowView rowView = rowObject.AddComponent<ForwardLineRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static DefensePairRowView CreateDefensePairRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("DefensePairRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 72f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 72f;
        layoutElement.minHeight = 72f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Пара 1 | LD OVR/EFF/COND | RD OVR/EFF/COND | AVG", Vector2.zero, new Vector2(790f, 66f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        DefensePairRowView rowView = rowObject.AddComponent<DefensePairRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static ScratchPlayerRowView CreateScratchPlayerRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ScratchPlayerRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 48f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 48f;
        layoutElement.minHeight = 48f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player | Pos | Age | OVR | EFF | COND | FAT | POT | Salary", Vector2.zero, new Vector2(790f, 42f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        ScratchPlayerRowView rowView = rowObject.AddComponent<ScratchPlayerRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static PowerPlayUnitRowView CreatePowerPlayUnitRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("PowerPlayUnitRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 88f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 88f;
        layoutElement.minHeight = 88f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "PP1 | Player 1, Player 2, Player 3, Player 4, Player 5 | AVG", Vector2.zero, new Vector2(790f, 80f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        PowerPlayUnitRowView rowView = rowObject.AddComponent<PowerPlayUnitRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static PenaltyKillUnitRowView CreatePenaltyKillUnitRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("PenaltyKillUnitRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 82f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 82f;
        layoutElement.minHeight = 82f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "PK1 | Player 1, Player 2, Player 3, Player 4 | AVG", Vector2.zero, new Vector2(790f, 74f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        PenaltyKillUnitRowView rowView = rowObject.AddComponent<PenaltyKillUnitRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static ScheduleGameRowView CreateScheduleGameRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ScheduleGameRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(800f, 56f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 56f;
        layoutElement.minHeight = 56f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text descriptionText = CreatePlayerRowText(rowObject.transform, "DescriptionText", "День 1. Team A vs Team B — Не сыгран", Vector2.zero, new Vector2(780f, 44f));

        ScheduleGameRowView rowView = rowObject.AddComponent<ScheduleGameRowView>();
        rowView.Configure(descriptionText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static StandingRowView CreateStandingRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("StandingRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(840f, 56f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 56f;
        layoutElement.minHeight = 56f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text placeText = CreatePlayerRowText(rowObject.transform, "PlaceText", "1", new Vector2(-390f, 0f), new Vector2(45f, 44f));
        Text teamNameText = CreatePlayerRowText(rowObject.transform, "TeamNameText", "Toronto Maple Leafs", new Vector2(-205f, 0f), new Vector2(280f, 44f));
        Text gamesPlayedText = CreatePlayerRowText(rowObject.transform, "GamesPlayedText", "0", new Vector2(-35f, 0f), new Vector2(45f, 44f));
        Text winsText = CreatePlayerRowText(rowObject.transform, "WinsText", "0", new Vector2(25f, 0f), new Vector2(45f, 44f));
        Text lossesText = CreatePlayerRowText(rowObject.transform, "LossesText", "0", new Vector2(85f, 0f), new Vector2(45f, 44f));
        Text overtimeLossesText = CreatePlayerRowText(rowObject.transform, "OvertimeLossesText", "0", new Vector2(145f, 0f), new Vector2(55f, 44f));
        Text pointsText = CreatePlayerRowText(rowObject.transform, "PointsText", "0", new Vector2(210f, 0f), new Vector2(45f, 44f));
        Text goalsForText = CreatePlayerRowText(rowObject.transform, "GoalsForText", "0", new Vector2(275f, 0f), new Vector2(55f, 44f));
        Text goalsAgainstText = CreatePlayerRowText(rowObject.transform, "GoalsAgainstText", "0", new Vector2(345f, 0f), new Vector2(55f, 44f));

        StandingRowView rowView = rowObject.AddComponent<StandingRowView>();
        rowView.Configure(
            placeText,
            teamNameText,
            gamesPlayedText,
            winsText,
            lossesText,
            overtimeLossesText,
            pointsText,
            goalsForText,
            goalsAgainstText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static PlayerStatsRowView CreatePlayerStatsRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("PlayerStatsRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(840f, 56f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 56f;
        layoutElement.minHeight = 56f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text text = CreatePlayerRowText(rowObject.transform, "Text", "Статистики пока нет. Симулируйте игровой день.", Vector2.zero, new Vector2(820f, 44f));
        PlayerStatsRowView rowView = rowObject.AddComponent<PlayerStatsRowView>();
        rowView.Configure(text);

        rowObject.SetActive(false);
        return rowView;
    }

    private static PlayoffSeriesRowView CreatePlayoffSeriesRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("PlayoffSeriesRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(840f, 56f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 56f;
        layoutElement.minHeight = 56f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text text = CreatePlayerRowText(rowObject.transform, "Text", "Первый раунд | Team A 0 - 0 Team B | Идёт", Vector2.zero, new Vector2(820f, 44f));
        PlayoffSeriesRowView rowView = rowObject.AddComponent<PlayoffSeriesRowView>();
        rowView.Configure(text);

        rowObject.SetActive(false);
        return rowView;
    }

    private static Text CreatePlayerRowText(Transform parent, string objectName, string value, Vector2 anchoredPosition, Vector2 size)
    {
        Text text = CreateText(parent, objectName, value, 18, anchoredPosition, size);
        text.color = Color.black;
        return text;
    }

    private static void Stretch(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }

    private static void AddScenesToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MainMenuScenePath, true),
            new EditorBuildSettingsScene(TeamSelectScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.path == MainMenuScenePath || scene.path == TeamSelectScenePath || scene.path == GameScenePath)
            {
                continue;
            }

            scenes.Add(scene);
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void EnsureScenesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }
}
