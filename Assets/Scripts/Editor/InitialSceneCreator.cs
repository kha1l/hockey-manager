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

    [MenuItem("Tools/Continental Hockey Manager/Create Initial Scenes")]
    public static void CreateInitialScenes()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
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

        Debug.Log("Continental Hockey Manager: initial scenes created.");
    }

    private static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        Canvas canvas = CreateCanvas();
        CreateUiCamera();
        CreateEventSystem();

        MainMenuController controller = new GameObject("MainMenuController").AddComponent<MainMenuController>();

        CreateText(canvas.transform, "Title", FictionalLeagueConfig.GameTitle, 44, new Vector2(0f, 170f), new Vector2(760f, 80f));

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
        CreateUiCamera();
        CreateEventSystem();

        CreateTeamSelectBackground(canvas.transform);
        GameObject rootPanel = CreatePanel(canvas.transform, "TeamSelectRoot", Vector2.zero, new Vector2(940f, 1640f));
        rootPanel.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.05f, 0.18f);
        TeamSelectController controller = rootPanel.AddComponent<TeamSelectController>();

        Image backgroundTintImage = CreateStretchImage(rootPanel.transform, "TeamColorBackdrop", new Color(0.12f, 0.18f, 0.24f, 0.18f), Vector2.zero, Vector2.zero);
        GameObject accentPanel = CreatePanel(rootPanel.transform, "TeamAccentPanel", new Vector2(225f, 150f), new Vector2(420f, 830f));
        Image accentTintImage = accentPanel.GetComponent<Image>();
        accentTintImage.color = new Color(0.2f, 0.28f, 0.38f, 0.10f);

        GameObject selectionRoot = new GameObject("TeamSelectionContent");
        selectionRoot.transform.SetParent(rootPanel.transform, false);
        RectTransform selectionRect = selectionRoot.AddComponent<RectTransform>();
        Stretch(selectionRect, Vector2.zero, Vector2.zero);

        CreateText(selectionRoot.transform, "ModeText", "ВЫБОР КОМАНДЫ", 22, new Vector2(-245f, 650f), new Vector2(420f, 46f));
        CreateText(selectionRoot.transform, "CountryText", FictionalLeagueConfig.LeagueDisplayName, 20, new Vector2(-245f, 590f), new Vector2(430f, 42f));

        GameObject clubCard = CreatePanel(selectionRoot.transform, "ClubCard", new Vector2(-245f, 110f), new Vector2(440f, 760f));
        clubCard.GetComponent<Image>().color = new Color(0.065f, 0.075f, 0.105f, 0.88f);
        GameObject clubAccent = CreatePanel(selectionRoot.transform, "ClubCardAccent", new Vector2(-245f, 110f), new Vector2(452f, 772f));
        clubAccent.transform.SetAsFirstSibling();
        clubAccent.GetComponent<Image>().color = new Color(0.05f, 0.76f, 0.78f, 0.22f);

        Text teamNameText = CreateText(clubCard.transform, "TeamNameText", "Moscow Stars", 34, new Vector2(0f, 285f), new Vector2(390f, 92f));
        Image logoImage = CreateTeamLogoImage(clubCard.transform, "TeamLogoImage", new Vector2(0f, 45f), new Vector2(340f, 340f));
        logoImage.color = Color.clear;
        Text ratingText = CreateText(clubCard.transform, "TeamRatingText", "", 1, new Vector2(0f, -160f), new Vector2(10f, 10f));
        Text identityText = CreateText(clubCard.transform, "TeamIdentityText", "", 1, new Vector2(0f, -175f), new Vector2(10f, 10f));
        Text counterText = CreateText(clubCard.transform, "TeamCounterText", "1 / 32", 17, new Vector2(0f, -305f), new Vector2(160f, 36f));

        Button previousButton = CreateButton(selectionRoot.transform, "PreviousTeamButton", "<", new Vector2(-440f, 40f), new Vector2(58f, 220f));
        StyleDarkButton(previousButton, 34);
        Button nextButton = CreateButton(selectionRoot.transform, "NextTeamButton", ">", new Vector2(440f, 40f), new Vector2(58f, 220f));
        StyleDarkButton(nextButton, 34);

        GameObject kitCard = CreatePanel(selectionRoot.transform, "KitCard", new Vector2(225f, 290f), new Vector2(330f, 520f));
        kitCard.GetComponent<Image>().color = new Color(0.075f, 0.075f, 0.115f, 0.90f);
        CreateText(kitCard.transform, "KitLabel", "ФОРМА", 15, new Vector2(0f, 220f), new Vector2(260f, 34f));
        CreateText(kitCard.transform, "KitTitle", "HOME KIT", 24, new Vector2(0f, 180f), new Vector2(260f, 42f));
        Image playerImage = CreateTeamLogoImage(kitCard.transform, "TeamPlayerImage", new Vector2(0f, -40f), new Vector2(285f, 395f));
        playerImage.color = Color.clear;

        GameObject divisionCard = CreatePanel(selectionRoot.transform, "DivisionCard", new Vector2(225f, -80f), new Vector2(330f, 160f));
        divisionCard.GetComponent<Image>().color = new Color(0.075f, 0.075f, 0.115f, 0.88f);
        CreateText(divisionCard.transform, "DivisionLabel", "КОНФЕРЕНЦИЯ / ДИВИЗИОН", 13, new Vector2(0f, 52f), new Vector2(300f, 30f));
        Text conferenceBlockText = CreateText(divisionCard.transform, "ConferenceBlockText", "Western Conference\nCapital Division", 19, new Vector2(0f, -18f), new Vector2(300f, 88f));

        GameObject ratingCard = CreatePanel(selectionRoot.transform, "RatingCard", new Vector2(225f, -270f), new Vector2(330f, 170f));
        ratingCard.GetComponent<Image>().color = new Color(0.10f, 0.09f, 0.12f, 0.90f);
        CreateText(ratingCard.transform, "RatingLabel", "СИЛА СОСТАВА", 16, new Vector2(0f, 50f), new Vector2(300f, 38f));
        Text ratingBlockText = CreateText(ratingCard.transform, "RatingBlockText", "80\nСильный состав", 30, new Vector2(0f, -18f), new Vector2(300f, 96f));

        Text prewarmStatusText = CreateText(selectionRoot.transform, "PrewarmStatusText", "Данные новой игры готовы", 15, new Vector2(0f, -425f), new Vector2(820f, 36f));
        Button selectButton = CreateButton(selectionRoot.transform, "SelectTeamButton", "Выбрать команду", new Vector2(0f, -515f), new Vector2(540f, 72f));
        StylePrimaryButton(selectButton, 25);
        Button backButton = CreateButton(selectionRoot.transform, "BackButton", "Назад", new Vector2(0f, -600f), new Vector2(320f, 56f));
        StyleDarkButton(backButton, 20);
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.BackToMainMenu);

        Slider loadingSlider;
        Text loadingTitleText;
        Text loadingStatusText;
        Text loadingPercentText;
        GameObject loadingPanel = CreateTeamSelectLoadingPanel(rootPanel.transform, out loadingSlider, out loadingTitleText, out loadingStatusText, out loadingPercentText);

        controller.ConfigureCarousel(
            selectionRoot,
            loadingPanel,
            loadingSlider,
            loadingTitleText,
            loadingStatusText,
            loadingPercentText,
            backgroundTintImage,
            accentTintImage,
            logoImage,
            playerImage,
            teamNameText,
            identityText,
            ratingText,
            conferenceBlockText,
            ratingBlockText,
            counterText,
            prewarmStatusText,
            previousButton,
            nextButton,
            selectButton);

        EditorSceneManager.SaveScene(scene, TeamSelectScenePath);
    }

    private static void CreateGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Game";

        Canvas canvas = CreateCanvas();
        CreateUiCamera();
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
        Text cpuRosterAiText;
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
            out freeAgencyStatusText,
            out cpuRosterAiText);

        GameObject rosterPanel = CreatePanel(canvas.transform, "RosterPanel", Vector2.zero, new Vector2(920f, 1580f));
        RosterController rosterController = rosterPanel.AddComponent<RosterController>();
        CreateRosterPanel(rosterPanel.transform, gameScreenController, rosterController);

        GameObject lineupPanel = CreatePanel(canvas.transform, "LineupPanel", Vector2.zero, new Vector2(920f, 1580f));
        LineupController lineupController = lineupPanel.AddComponent<LineupController>();
        CreateLineupPanel(lineupPanel.transform, gameScreenController, lineupController);

        GameObject rolesPanel = CreatePanel(canvas.transform, "RolesPanel", Vector2.zero, new Vector2(920f, 1580f));
        RolesController rolesController = rolesPanel.AddComponent<RolesController>();
        CreateRolesPanel(rolesPanel.transform, gameScreenController, rolesController);

        GameObject moralePanel = CreatePanel(canvas.transform, "MoralePanel", Vector2.zero, new Vector2(920f, 1580f));
        MoraleController moraleController = moralePanel.AddComponent<MoraleController>();
        CreateMoralePanel(moralePanel.transform, gameScreenController, moraleController);

        GameObject leadershipPanel = CreatePanel(canvas.transform, "LeadershipPanel", Vector2.zero, new Vector2(920f, 1580f));
        LeadershipController leadershipController = leadershipPanel.AddComponent<LeadershipController>();
        CreateLeadershipPanel(leadershipPanel.transform, gameScreenController, leadershipController);

        GameObject staffPanel = CreatePanel(canvas.transform, "StaffPanel", Vector2.zero, new Vector2(920f, 1580f));
        StaffController staffController = staffPanel.AddComponent<StaffController>();
        CreateStaffPanel(staffPanel.transform, gameScreenController, staffController);

        GameObject tacticsPanel = CreatePanel(canvas.transform, "TacticsPanel", Vector2.zero, new Vector2(920f, 1580f));
        TacticsController tacticsController = tacticsPanel.AddComponent<TacticsController>();
        CreateTacticsPanel(tacticsPanel.transform, gameScreenController, tacticsController);

        GameObject injuriesPanel = CreatePanel(canvas.transform, "InjuriesPanel", Vector2.zero, new Vector2(920f, 1580f));
        InjuriesController injuriesController = injuriesPanel.AddComponent<InjuriesController>();
        CreateInjuriesPanel(injuriesPanel.transform, gameScreenController, injuriesController);

        GameObject contractsPanel = CreatePanel(canvas.transform, "ContractsPanel", Vector2.zero, new Vector2(920f, 1580f));
        ContractsController contractsController = contractsPanel.AddComponent<ContractsController>();
        CreateContractsPanel(contractsPanel.transform, gameScreenController, contractsController);

        GameObject extensionsPanel = CreatePanel(canvas.transform, "ExtensionsPanel", Vector2.zero, new Vector2(920f, 1580f));
        ExtensionsController extensionsController = extensionsPanel.AddComponent<ExtensionsController>();
        CreateExtensionsPanel(extensionsPanel.transform, gameScreenController, extensionsController);

        GameObject tradesPanel = CreatePanel(canvas.transform, "TradesPanel", Vector2.zero, new Vector2(920f, 1580f));
        TradesController tradesController = tradesPanel.AddComponent<TradesController>();
        CreateTradesPanel(tradesPanel.transform, gameScreenController, tradesController);

        GameObject scoutingPanel = CreatePanel(canvas.transform, "ScoutingPanel", Vector2.zero, new Vector2(920f, 1580f));
        ScoutingController scoutingController = scoutingPanel.AddComponent<ScoutingController>();
        CreateScoutingPanel(scoutingPanel.transform, gameScreenController, scoutingController);

        GameObject freeAgencyPanel = CreatePanel(canvas.transform, "FreeAgencyPanel", Vector2.zero, new Vector2(920f, 1580f));
        FreeAgencyController freeAgencyController = freeAgencyPanel.AddComponent<FreeAgencyController>();
        CreateFreeAgencyPanel(freeAgencyPanel.transform, gameScreenController, freeAgencyController);

        GameObject draftPanel = CreatePanel(canvas.transform, "DraftPanel", Vector2.zero, new Vector2(920f, 1580f));
        DraftController draftController = draftPanel.AddComponent<DraftController>();
        CreateDraftPanel(draftPanel.transform, gameScreenController, draftController);

        GameObject prospectRightsPanel = CreatePanel(canvas.transform, "ProspectRightsPanel", Vector2.zero, new Vector2(920f, 1580f));
        ProspectRightsController prospectRightsController = prospectRightsPanel.AddComponent<ProspectRightsController>();
        CreateProspectRightsPanel(prospectRightsPanel.transform, gameScreenController, prospectRightsController);

        GameObject organizationPanel = CreatePanel(canvas.transform, "OrganizationPanel", Vector2.zero, new Vector2(920f, 1580f));
        OrganizationController organizationController = organizationPanel.AddComponent<OrganizationController>();
        CreateOrganizationPanel(organizationPanel.transform, gameScreenController, organizationController);

        GameObject waiversPanel = CreatePanel(canvas.transform, "WaiversPanel", Vector2.zero, new Vector2(920f, 1580f));
        WaiversController waiversController = waiversPanel.AddComponent<WaiversController>();
        CreateWaiversPanel(waiversPanel.transform, gameScreenController, waiversController);

        GameObject offseasonPanel = CreatePanel(canvas.transform, "OffseasonPanel", Vector2.zero, new Vector2(920f, 1580f));
        OffseasonController offseasonController = offseasonPanel.AddComponent<OffseasonController>();
        CreateOffseasonPanel(offseasonPanel.transform, gameScreenController, offseasonController);

        GameObject ownerPanel = CreatePanel(canvas.transform, "OwnerPanel", Vector2.zero, new Vector2(920f, 1580f));
        OwnerController ownerController = ownerPanel.AddComponent<OwnerController>();
        CreateOwnerPanel(ownerPanel.transform, gameScreenController, ownerController);

        GameObject gmCareerPanel = CreatePanel(canvas.transform, "GmCareerPanel", Vector2.zero, new Vector2(920f, 1580f));
        GmCareerController gmCareerController = gmCareerPanel.AddComponent<GmCareerController>();
        CreateGmCareerPanel(gmCareerPanel.transform, gameScreenController, gmCareerController);

        GameObject diagnosticsPanel = CreatePanel(canvas.transform, "DiagnosticsPanel", Vector2.zero, new Vector2(920f, 1580f));
        DiagnosticsController diagnosticsController = diagnosticsPanel.AddComponent<DiagnosticsController>();
        CreateDiagnosticsPanel(diagnosticsPanel.transform, gameScreenController, diagnosticsController);

        GameObject developmentPanel = CreatePanel(canvas.transform, "DevelopmentPanel", Vector2.zero, new Vector2(920f, 1580f));
        DevelopmentController developmentController = developmentPanel.AddComponent<DevelopmentController>();
        CreateDevelopmentPanel(developmentPanel.transform, gameScreenController, developmentController);

        GameObject historyPanel = CreatePanel(canvas.transform, "HistoryPanel", Vector2.zero, new Vector2(920f, 1580f));
        HistoryController historyController = historyPanel.AddComponent<HistoryController>();
        CreateHistoryPanel(historyPanel.transform, gameScreenController, historyController);

        GameObject newsPanel = CreatePanel(canvas.transform, "NewsPanel", Vector2.zero, new Vector2(920f, 1580f));
        NewsController newsController = newsPanel.AddComponent<NewsController>();
        CreateNewsPanel(newsPanel.transform, gameScreenController, newsController);

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

        GameObject preGamePanel = CreatePanel(canvas.transform, "PreGamePanel", Vector2.zero, new Vector2(920f, 1580f));
        PreGameController preGameController = preGamePanel.AddComponent<PreGameController>();
        CreatePreGamePanel(preGamePanel.transform, gameScreenController, preGameController);

        GameObject liveMatchPanel = CreatePanel(canvas.transform, "LiveMatchPanel", Vector2.zero, new Vector2(920f, 1580f));
        LiveMatchController liveMatchController = liveMatchPanel.AddComponent<LiveMatchController>();
        CreateLiveMatchPanel(liveMatchPanel.transform, gameScreenController, liveMatchController);

        GameObject postGameSummaryPanel = CreatePanel(canvas.transform, "PostGameSummaryPanel", Vector2.zero, new Vector2(920f, 1580f));
        PostGameSummaryController postGameSummaryController = postGameSummaryPanel.AddComponent<PostGameSummaryController>();
        CreatePostGameSummaryPanel(postGameSummaryPanel.transform, gameScreenController, postGameSummaryController);

        GameObject tutorialHintObject = CreateTutorialHintOverlay(canvas.transform, gameScreenController, out TutorialHintView tutorialHintView);
        GameObject tutorialPanel = CreatePanel(canvas.transform, "TutorialPanel", Vector2.zero, new Vector2(900f, 980f));
        TutorialController tutorialController = tutorialPanel.AddComponent<TutorialController>();
        Text tutorialTitleText;
        Text tutorialSummaryText;
        Text tutorialChecklistText;
        Text tutorialHintText;
        Button tutorialDismissHintButton;
        Button tutorialDisableButton;
        Button tutorialResetButton;
        CreateTutorialPanel(
            tutorialPanel.transform,
            gameScreenController,
            tutorialController,
            out tutorialTitleText,
            out tutorialSummaryText,
            out tutorialChecklistText,
            out tutorialHintText,
            out tutorialDismissHintButton,
            out tutorialDisableButton,
            out tutorialResetButton);
        GameObject busyOverlayPanel = CreateBusyOverlay(canvas.transform, out Text busyOverlayText);

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
            cpuRosterAiText,
            dashboardPanel,
            rosterPanel,
            lineupPanel,
            tacticsPanel,
            contractsPanel,
            extensionsPanel,
            tradesPanel,
            scoutingPanel,
            freeAgencyPanel,
            draftPanel,
            prospectRightsPanel,
            organizationPanel,
            waiversPanel,
            offseasonPanel,
            ownerPanel,
            gmCareerPanel,
            diagnosticsPanel,
            historyPanel,
            newsPanel,
            developmentPanel,
            rolesPanel,
            moralePanel,
            leadershipPanel,
            staffPanel,
            calendarPanel,
            injuriesPanel,
            standingsPanel,
            playerStatsPanel,
            playoffsPanel,
            rosterController,
            contractsController,
            extensionsController,
            tradesController,
            scoutingController,
            freeAgencyController,
            draftController,
            prospectRightsController,
            organizationController,
            waiversController,
            offseasonController,
            ownerController,
            gmCareerController,
            diagnosticsController,
            historyController,
            newsController,
            developmentController,
            rolesController,
            moraleController,
            leadershipController,
            staffController,
            lineupController,
            tacticsController,
            injuriesController,
            calendarController,
            standingsController,
            playerStatsController,
            playoffsController);
        gameScreenController.ConfigureTutorial(
            tutorialPanel,
            tutorialController,
            tutorialHintView,
            tutorialTitleText,
            tutorialSummaryText,
            tutorialChecklistText,
            tutorialHintText,
            tutorialDismissHintButton,
            tutorialDisableButton,
            tutorialResetButton);
        gameScreenController.ConfigureLiveMatch(
            preGamePanel,
            liveMatchPanel,
            postGameSummaryPanel,
            preGameController,
            liveMatchController,
            postGameSummaryController);
        gameScreenController.BusyOverlayPanel = busyOverlayPanel;
        gameScreenController.BusyOverlayText = busyOverlayText;

        rosterPanel.SetActive(false);
        lineupPanel.SetActive(false);
        rolesPanel.SetActive(false);
        moralePanel.SetActive(false);
        leadershipPanel.SetActive(false);
        staffPanel.SetActive(false);
        tacticsPanel.SetActive(false);
        injuriesPanel.SetActive(false);
        contractsPanel.SetActive(false);
        extensionsPanel.SetActive(false);
        tradesPanel.SetActive(false);
        scoutingPanel.SetActive(false);
        freeAgencyPanel.SetActive(false);
        draftPanel.SetActive(false);
        prospectRightsPanel.SetActive(false);
        tutorialPanel.SetActive(false);
        tutorialHintObject.SetActive(false);
        busyOverlayPanel.SetActive(false);
        organizationPanel.SetActive(false);
        waiversPanel.SetActive(false);
        offseasonPanel.SetActive(false);
        ownerPanel.SetActive(false);
        gmCareerPanel.SetActive(false);
        diagnosticsPanel.SetActive(false);
        historyPanel.SetActive(false);
        newsPanel.SetActive(false);
        developmentPanel.SetActive(false);
        calendarPanel.SetActive(false);
        standingsPanel.SetActive(false);
        playerStatsPanel.SetActive(false);
        playoffsPanel.SetActive(false);
        preGamePanel.SetActive(false);
        liveMatchPanel.SetActive(false);
        postGameSummaryPanel.SetActive(false);

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
        out Text freeAgencyStatusText,
        out Text cpuRosterAiText)
    {
        if (UseCompactDashboard())
        {
            return CreateCompactDashboardPanel(
                parent,
                controller,
                out currentDayText,
                out gamesSimulatedText,
                out nextGameText,
                out lastMatchResultText,
                out seasonRulesText,
                out financeText,
                out leagueDateText,
                out tradeStatusText,
                out freeAgencyStatusText,
                out cpuRosterAiText);
        }

        CreateText(parent, "Title", "Экран клуба", 42, new Vector2(0f, 685f), new Vector2(760f, 64f));
        Button helpButton = CreateDashboardActionButton(parent, "DashboardHelpButton", "?", new Vector2(395f, 685f), new Vector2(58f, 48f));
        UnityEventTools.AddPersistentListener(helpButton.onClick, controller.ShowTutorial);
        Image currentTeamLogo = CreateTeamLogoImage(parent, "CurrentTeamLogo", new Vector2(-365f, 634f), new Vector2(70f, 70f));
        controller.CurrentTeamLogoImage = currentTeamLogo;
        Text selectedTeamText = CreateText(parent, "SelectedTeamText", "Команда не выбрана", 23, new Vector2(40f, 646f), new Vector2(720f, 36f));
        Text currentTeamIdentityText = CreateText(parent, "CurrentTeamIdentityText", FictionalLeagueConfig.LeagueDisplayName, 14, new Vector2(40f, 614f), new Vector2(720f, 30f));
        controller.CurrentTeamIdentityText = currentTeamIdentityText;
        seasonRulesText = CreateText(parent, "SeasonRulesText", "Сезон: 2026-27 | Сезон карьеры: 1 | Игр: 84\nАрхивных сезонов: 0\nРазвитие игроков: 0 изменений за последний сезон\nСостав на матч: валиден\nТактика: Balanced | PP 0 | PK 0", 15, new Vector2(0f, 557f), new Vector2(820f, 138f));
        leagueDateText = CreateText(parent, "LeagueDateText", "Дата лиги: 2026-09-28 | Trade deadline: 2027-03-05", 18, new Vector2(0f, 478f), new Vector2(820f, 34f));
        tradeStatusText = CreateText(parent, "TradeStatusText", "Обмены доступны", 18, new Vector2(0f, 446f), new Vector2(820f, 34f));
        freeAgencyStatusText = CreateText(parent, "FreeAgencyStatusText", "Free agency: закрыта", 17, new Vector2(0f, 414f), new Vector2(820f, 34f));
        currentDayText = CreateText(parent, "CurrentDayText", "", 18, new Vector2(0f, 382f), new Vector2(820f, 34f));
        gamesSimulatedText = CreateText(parent, "GamesSimulatedText", "Матчей сыграно в лиге: 0 / 1344", 18, new Vector2(0f, 350f), new Vector2(820f, 34f));
        nextGameText = CreateText(parent, "NextGameText", "Следующий матч: ...", 18, new Vector2(0f, 318f), new Vector2(820f, 34f));
        lastMatchResultText = CreateText(parent, "LastMatchResultText", "Матчей ещё не было", 18, new Vector2(0f, 286f), new Vector2(820f, 34f));
        financeText = CreateText(parent, "FinanceText", "Зарплатная ведомость: 0 / 104 000 000\nМесто под потолком: 104 000 000\nМинимальный порог: 76 900 000\nПрава на проспектов: 0\nСостав: 23 / 23\nТравмы: 0\nСредняя готовность состава: 100\nFree agency: 0 available | best: none | recent offers: 0\nCaptain: none | A: none / none\nLeadership: Average (+1 morale, +1 chemistry)\nHead Coach: none - Balanced\nStaff: Off +0, Def +0, Dev +0, Morale +0, Chemistry +0\nTeam chemistry: 60 Average | Mod +0\nBest unit: none | Worst unit: none\nСамый уставший: нет данных\nСтартовый вратарь: нет данных", 10, new Vector2(0f, 188f), new Vector2(820f, 184f));
        cpuRosterAiText = CreateText(parent, "CpuRosterAiText", "CPU roster AI: отчётов пока нет", 11, new Vector2(0f, 82f), new Vector2(820f, 62f));

        Button mainGroupButton = CreateDashboardActionButton(parent, "DashboardMainGroupButton", "Main", new Vector2(-350f, 24f), new Vector2(150f, 44f));
        UnityEventTools.AddPersistentListener(mainGroupButton.onClick, controller.SelectDashboardMain);
        Button teamGroupButton = CreateDashboardActionButton(parent, "DashboardTeamGroupButton", "Команда", new Vector2(-210f, 24f), new Vector2(150f, 44f));
        UnityEventTools.AddPersistentListener(teamGroupButton.onClick, controller.SelectDashboardTeam);
        Button seasonGroupButton = CreateDashboardActionButton(parent, "DashboardSeasonGroupButton", "Сезон", new Vector2(-70f, 24f), new Vector2(150f, 44f));
        UnityEventTools.AddPersistentListener(seasonGroupButton.onClick, controller.SelectDashboardSeason);
        Button officeGroupButton = CreateDashboardActionButton(parent, "DashboardOfficeGroupButton", "Офис", new Vector2(70f, 24f), new Vector2(150f, 44f));
        UnityEventTools.AddPersistentListener(officeGroupButton.onClick, controller.SelectDashboardOffice);
        Button marketGroupButton = CreateDashboardActionButton(parent, "DashboardMarketGroupButton", "Рынок", new Vector2(210f, 24f), new Vector2(150f, 44f));
        UnityEventTools.AddPersistentListener(marketGroupButton.onClick, controller.SelectDashboardMarket);
        Button historyGroupButton = CreateDashboardActionButton(parent, "DashboardHistoryGroupButton", "История", new Vector2(350f, 24f), new Vector2(150f, 44f));
        UnityEventTools.AddPersistentListener(historyGroupButton.onClick, controller.SelectDashboardHistory);

        Text alertsText = CreateText(parent, "DashboardAlertsContainer", "Alerts: нет критичных предупреждений", 11, new Vector2(0f, 42f), new Vector2(820f, 42f));
        alertsText.alignment = TextAnchor.MiddleCenter;
        Text latestNewsText = CreateText(parent, "DashboardLatestNewsText", "News: пока нет", 11, new Vector2(0f, 0f), new Vector2(820f, 42f));
        latestNewsText.alignment = TextAnchor.MiddleCenter;

        Transform mainActions = CreateDashboardActionGroup(parent, "DashboardMainActions");
        Transform teamActions = CreateDashboardActionGroup(parent, "DashboardTeamActions");
        Transform seasonActions = CreateDashboardActionGroup(parent, "DashboardSeasonActions");
        Transform officeActions = CreateDashboardActionGroup(parent, "DashboardOfficeActions");
        Transform marketActions = CreateDashboardActionGroup(parent, "DashboardMarketActions");
        Transform historyActions = CreateDashboardActionGroup(parent, "DashboardHistoryActions");
        Transform systemActions = CreateDashboardActionGroup(parent, "DashboardSystemActions");

        CreateText(mainActions, "MainGroupLabel", "Быстрый доступ", 16, new Vector2(0f, -54f), new Vector2(720f, 28f));
        Button mainRosterButton = CreateDashboardActionButton(mainActions, "MainRosterButton", "Состав", new Vector2(-270f, -104f));
        UnityEventTools.AddPersistentListener(mainRosterButton.onClick, controller.ShowRoster);
        Button mainLineupButton = CreateDashboardActionButton(mainActions, "MainLineupButton", "Линии", new Vector2(0f, -104f));
        UnityEventTools.AddPersistentListener(mainLineupButton.onClick, controller.ShowLineup);
        Button mainContractsButton = CreateDashboardActionButton(mainActions, "MainContractsButton", "Контракты", new Vector2(270f, -104f));
        UnityEventTools.AddPersistentListener(mainContractsButton.onClick, controller.ShowContracts);
        Button mainCalendarButton = CreateDashboardActionButton(mainActions, "MainCalendarButton", "Календарь", new Vector2(-270f, -164f));
        UnityEventTools.AddPersistentListener(mainCalendarButton.onClick, controller.ShowCalendar);
        Button mainStandingsButton = CreateDashboardActionButton(mainActions, "MainStandingsButton", "Таблица", new Vector2(0f, -164f));
        UnityEventTools.AddPersistentListener(mainStandingsButton.onClick, controller.ShowStandings);
        Button mainNewsButton = CreateDashboardActionButton(mainActions, "MainNewsButton", "Новости", new Vector2(270f, -164f));
        UnityEventTools.AddPersistentListener(mainNewsButton.onClick, controller.ShowNews);
        Button playNextMatchButton = CreateDashboardActionButton(mainActions, "PlayNextUserMatchButton", "Играть матч", new Vector2(0f, -224f));
        UnityEventTools.AddPersistentListener(playNextMatchButton.onClick, controller.PlayNextUserMatch);

        CreateText(teamActions, "TeamGroupLabel", "Команда", 16, new Vector2(0f, -54f), new Vector2(720f, 28f));
        Button rosterButton = CreateDashboardActionButton(teamActions, "RosterButton", "Состав", new Vector2(-270f, -104f));
        UnityEventTools.AddPersistentListener(rosterButton.onClick, controller.ShowRoster);
        Button organizationButton = CreateDashboardActionButton(teamActions, "OrganizationButton", "Организация", new Vector2(0f, -104f));
        UnityEventTools.AddPersistentListener(organizationButton.onClick, controller.ShowOrganization);
        Button lineupButton = CreateDashboardActionButton(teamActions, "LineupButton", "Линии", new Vector2(270f, -104f));
        UnityEventTools.AddPersistentListener(lineupButton.onClick, controller.ShowLineup);
        Button rolesButton = CreateDashboardActionButton(teamActions, "RolesButton", "Роли", new Vector2(-270f, -164f));
        UnityEventTools.AddPersistentListener(rolesButton.onClick, controller.ShowRoles);
        Button moraleButton = CreateDashboardActionButton(teamActions, "MoraleButton", "Мораль", new Vector2(0f, -164f));
        UnityEventTools.AddPersistentListener(moraleButton.onClick, controller.ShowMorale);
        Button injuriesButton = CreateDashboardActionButton(teamActions, "InjuriesButton", "Травмы", new Vector2(270f, -164f));
        UnityEventTools.AddPersistentListener(injuriesButton.onClick, controller.ShowInjuries);
        Button leadershipButton = CreateDashboardActionButton(teamActions, "LeadershipButton", "Капитаны", new Vector2(-135f, -224f));
        UnityEventTools.AddPersistentListener(leadershipButton.onClick, controller.ShowLeadership);
        Button staffButton = CreateDashboardActionButton(teamActions, "StaffButton", "Тренеры", new Vector2(135f, -224f));
        UnityEventTools.AddPersistentListener(staffButton.onClick, controller.ShowStaff);

        CreateText(seasonActions, "SeasonGroupLabel", "Сезон", 16, new Vector2(0f, -54f), new Vector2(720f, 28f));
        Button calendarButton = CreateDashboardActionButton(seasonActions, "CalendarButton", "Календарь", new Vector2(-270f, -104f));
        UnityEventTools.AddPersistentListener(calendarButton.onClick, controller.ShowCalendar);
        Button standingsButton = CreateDashboardActionButton(seasonActions, "StandingsButton", "Таблица", new Vector2(0f, -104f));
        UnityEventTools.AddPersistentListener(standingsButton.onClick, controller.ShowStandings);
        Button playerStatsButton = CreateDashboardActionButton(seasonActions, "PlayerStatsButton", "Статистика", new Vector2(270f, -104f));
        UnityEventTools.AddPersistentListener(playerStatsButton.onClick, controller.ShowPlayerStats);
        Button playoffsButton = CreateDashboardActionButton(seasonActions, "PlayoffsButton", "Плей-офф", new Vector2(-135f, -164f));
        UnityEventTools.AddPersistentListener(playoffsButton.onClick, controller.ShowPlayoffs);
        Button tacticsButton = CreateDashboardActionButton(seasonActions, "TacticsButton", "Тактика", new Vector2(135f, -164f));
        UnityEventTools.AddPersistentListener(tacticsButton.onClick, controller.ShowTactics);

        CreateText(officeActions, "OfficeGroupLabel", "Офис", 16, new Vector2(0f, -54f), new Vector2(720f, 28f));
        Button contractsButton = CreateDashboardActionButton(officeActions, "ContractsButton", "Контракты", new Vector2(-270f, -104f));
        UnityEventTools.AddPersistentListener(contractsButton.onClick, controller.ShowContracts);
        Button extensionsButton = CreateDashboardActionButton(officeActions, "ExtensionsButton", "Продления", new Vector2(0f, -104f));
        UnityEventTools.AddPersistentListener(extensionsButton.onClick, controller.ShowExtensions);
        Button ownerButton = CreateDashboardActionButton(officeActions, "OwnerButton", "Владелец", new Vector2(270f, -104f));
        UnityEventTools.AddPersistentListener(ownerButton.onClick, controller.ShowOwner);
        Button gmCareerButton = CreateDashboardActionButton(officeActions, "GmCareerButton", "Карьера GM", new Vector2(-135f, -164f));
        UnityEventTools.AddPersistentListener(gmCareerButton.onClick, controller.ShowGmCareer);
        Button developmentButton = CreateDashboardActionButton(officeActions, "DevelopmentButton", "Развитие", new Vector2(135f, -164f));
        UnityEventTools.AddPersistentListener(developmentButton.onClick, controller.ShowDevelopment);

        CreateText(marketActions, "MarketGroupLabel", "Рынок", 16, new Vector2(0f, -54f), new Vector2(720f, 28f));
        Button tradesButton = CreateDashboardActionButton(marketActions, "TradesButton", "Обмены", new Vector2(-270f, -104f));
        UnityEventTools.AddPersistentListener(tradesButton.onClick, controller.ShowTrades);
        Button freeAgencyButton = CreateDashboardActionButton(marketActions, "FreeAgencyButton", "FA", new Vector2(0f, -104f));
        UnityEventTools.AddPersistentListener(freeAgencyButton.onClick, controller.ShowFreeAgency);
        Button waiversButton = CreateDashboardActionButton(marketActions, "WaiversButton", "Waivers", new Vector2(270f, -104f));
        UnityEventTools.AddPersistentListener(waiversButton.onClick, controller.ShowWaivers);
        Button draftButton = CreateDashboardActionButton(marketActions, "DraftButton", "Драфт", new Vector2(-270f, -164f));
        UnityEventTools.AddPersistentListener(draftButton.onClick, controller.ShowDraft);
        Button scoutingButton = CreateDashboardActionButton(marketActions, "ScoutingButton", "Скаутинг", new Vector2(0f, -164f));
        UnityEventTools.AddPersistentListener(scoutingButton.onClick, controller.ShowScouting);
        Button prospectRightsButton = CreateDashboardActionButton(marketActions, "ProspectRightsButton", "Права", new Vector2(270f, -164f));
        UnityEventTools.AddPersistentListener(prospectRightsButton.onClick, controller.ShowProspectRights);

        CreateText(historyActions, "HistoryGroupLabel", "История", 16, new Vector2(0f, -54f), new Vector2(720f, 28f));
        Button newsButton = CreateDashboardActionButton(historyActions, "NewsButton", "Новости", new Vector2(-270f, -104f));
        UnityEventTools.AddPersistentListener(newsButton.onClick, controller.ShowNews);
        Button historyButton = CreateDashboardActionButton(historyActions, "HistoryButton", "История", new Vector2(0f, -104f));
        UnityEventTools.AddPersistentListener(historyButton.onClick, controller.ShowHistory);
        Button offseasonButton = CreateDashboardActionButton(historyActions, "OffseasonButton", "Межсезонье", new Vector2(270f, -104f));
        UnityEventTools.AddPersistentListener(offseasonButton.onClick, controller.ShowOffseason);

        CreateText(systemActions, "SystemGroupLabel", "Система", 15, new Vector2(0f, -302f), new Vector2(720f, 24f));
        Button simulateButton = CreateDashboardActionButton(systemActions, "SimulateMatchButton", "Сим день", new Vector2(-270f, -350f));
        UnityEventTools.AddPersistentListener(simulateButton.onClick, controller.SimulateMatch);
        Button simulateSeasonButton = CreateDashboardActionButton(systemActions, "SimulateSeasonButton", "Сим сезон", new Vector2(0f, -350f));
        UnityEventTools.AddPersistentListener(simulateSeasonButton.onClick, controller.SimulateRegularSeasonToEnd);
        Button saveButton = CreateDashboardActionButton(systemActions, "SaveButton", "Сохранить", new Vector2(270f, -350f));
        UnityEventTools.AddPersistentListener(saveButton.onClick, controller.SaveGame);
        Button simulateToDraftButton = CreateDashboardActionButton(systemActions, "SimulateToDraftButton", "До драфта", new Vector2(-270f, -410f));
        UnityEventTools.AddPersistentListener(simulateToDraftButton.onClick, controller.SimulateToDraftForTesting);
        Button simulateToFreeAgencyButton = CreateDashboardActionButton(systemActions, "SimulateToFreeAgencyButton", "До FA", new Vector2(0f, -410f));
        UnityEventTools.AddPersistentListener(simulateToFreeAgencyButton.onClick, controller.SimulateToFreeAgencyForTesting);
        Button deleteSaveButton = CreateDashboardActionButton(systemActions, "DeleteSaveButton", "Удалить save", new Vector2(270f, -410f));
        UnityEventTools.AddPersistentListener(deleteSaveButton.onClick, controller.DeleteSave);
        Button diagnosticsButton = CreateDashboardActionButton(systemActions, "DiagnosticsButton", "Diagnostics", new Vector2(-170f, -470f));
        UnityEventTools.AddPersistentListener(diagnosticsButton.onClick, controller.ShowDiagnostics);
        Button mainMenuButton = CreateDashboardActionButton(systemActions, "MainMenuButton", "Главное меню", new Vector2(170f, -470f));
        UnityEventTools.AddPersistentListener(mainMenuButton.onClick, controller.BackToMainMenu);

        teamActions.gameObject.SetActive(false);
        seasonActions.gameObject.SetActive(false);
        officeActions.gameObject.SetActive(false);
        marketActions.gameObject.SetActive(false);
        historyActions.gameObject.SetActive(false);

        return selectedTeamText;
    }

    private static bool UseCompactDashboard()
    {
        return true;
    }

    private static Text CreateCompactDashboardPanel(
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
        out Text freeAgencyStatusText,
        out Text cpuRosterAiText)
    {
        parent.GetComponent<Image>().color = new Color(0.025f, 0.035f, 0.052f, 0.96f);

        CreateText(parent, "Title", "Главная", 40, new Vector2(0f, 705f), new Vector2(760f, 58f));

        Button homeTab = CreateDashboardActionButton(parent, "TopNavHomeButton", "Главная", new Vector2(-375f, 642f), new Vector2(135f, 46f));
        UnityEventTools.AddPersistentListener(homeTab.onClick, controller.ShowDashboard);
        Button calendarTab = CreateDashboardActionButton(parent, "TopNavCalendarButton", "Календарь", new Vector2(-225f, 642f), new Vector2(135f, 46f));
        UnityEventTools.AddPersistentListener(calendarTab.onClick, controller.ShowCalendar);
        Button standingsTab = CreateDashboardActionButton(parent, "TopNavStandingsButton", "Таблица", new Vector2(-75f, 642f), new Vector2(135f, 46f));
        UnityEventTools.AddPersistentListener(standingsTab.onClick, controller.ShowStandings);
        Button teamTab = CreateDashboardActionButton(parent, "TopNavTeamButton", "Команда", new Vector2(75f, 642f), new Vector2(135f, 46f));
        UnityEventTools.AddPersistentListener(teamTab.onClick, controller.ShowOrganization);
        Button statsTab = CreateDashboardActionButton(parent, "TopNavStatsButton", "Статистика", new Vector2(225f, 642f), new Vector2(135f, 46f));
        UnityEventTools.AddPersistentListener(statsTab.onClick, controller.ShowPlayerStats);
        Button rosterTab = CreateDashboardActionButton(parent, "TopNavRosterButton", "Состав", new Vector2(375f, 642f), new Vector2(135f, 46f));
        UnityEventTools.AddPersistentListener(rosterTab.onClick, controller.ShowLineup);
        StylePrimaryButton(homeTab, 15);
        StyleDarkButton(calendarTab, 15);
        StyleDarkButton(standingsTab, 15);
        StyleDarkButton(teamTab, 15);
        StyleDarkButton(statsTab, 15);
        StyleDarkButton(rosterTab, 15);

        leagueDateText = CreateText(parent, "LeagueDateText", "Дата лиги: ...", 15, new Vector2(0f, 600f), new Vector2(760f, 30f));

        GameObject teamCard = CreatePanel(parent, "CompactTeamCard", new Vector2(0f, 500f), new Vector2(840f, 190f));
        teamCard.GetComponent<Image>().color = new Color(0.065f, 0.078f, 0.105f, 0.92f);
        Image currentTeamLogo = CreateTeamLogoImage(teamCard.transform, "CurrentTeamLogo", new Vector2(-315f, 0f), new Vector2(135f, 135f));
        controller.CurrentTeamLogoImage = currentTeamLogo;
        Text selectedTeamText = CreateText(teamCard.transform, "SelectedTeamText", "Команда не выбрана", 30, new Vector2(70f, 36f), new Vector2(600f, 58f));
        selectedTeamText.alignment = TextAnchor.MiddleLeft;
        Text currentTeamIdentityText = CreateText(teamCard.transform, "CurrentTeamIdentityText", FictionalLeagueConfig.LeagueDisplayName, 15, new Vector2(70f, -28f), new Vector2(600f, 58f));
        currentTeamIdentityText.alignment = TextAnchor.MiddleLeft;
        currentTeamIdentityText.supportRichText = true;
        controller.CurrentTeamIdentityText = currentTeamIdentityText;

        GameObject statsCard = CreatePanel(parent, "TeamStatsCard", new Vector2(-225f, 285f), new Vector2(390f, 205f));
        statsCard.GetComponent<Image>().color = new Color(0.07f, 0.085f, 0.115f, 0.90f);
        CreateText(statsCard.transform, "StatsTitle", "СТАТИСТИКА КОМАНДЫ", 16, new Vector2(0f, 72f), new Vector2(340f, 30f));
        seasonRulesText = CreateText(statsCard.transform, "SeasonRulesText", "Победы: 0\nПоражения: 0\nОчки: 0", 22, new Vector2(0f, -10f), new Vector2(340f, 130f));
        seasonRulesText.alignment = TextAnchor.MiddleLeft;

        GameObject nextGameCard = CreatePanel(parent, "NextGameCard", new Vector2(225f, 285f), new Vector2(390f, 205f));
        nextGameCard.GetComponent<Image>().color = new Color(0.07f, 0.085f, 0.115f, 0.90f);
        CreateText(nextGameCard.transform, "NextGameTitle", "БЛИЖАЙШИЙ МАТЧ", 16, new Vector2(0f, 72f), new Vector2(340f, 30f));
        nextGameText = CreateText(nextGameCard.transform, "NextGameText", "Следующий матч: ...", 18, new Vector2(0f, 12f), new Vector2(340f, 80f));
        nextGameText.alignment = TextAnchor.MiddleCenter;
        lastMatchResultText = CreateText(nextGameCard.transform, "LastMatchResultText", "Матчей ещё не было", 13, new Vector2(0f, -64f), new Vector2(340f, 42f));

        GameObject divisionCard = CreatePanel(parent, "DivisionStandingsCard", new Vector2(-225f, 15f), new Vector2(390f, 300f));
        divisionCard.GetComponent<Image>().color = new Color(0.07f, 0.085f, 0.115f, 0.90f);
        CreateText(divisionCard.transform, "DivisionTitle", "ТАБЛИЦА ДИВИЗИОНА", 16, new Vector2(0f, 118f), new Vector2(340f, 30f));
        financeText = CreateText(divisionCard.transform, "FinanceText", "1. Команда 0-0-0 0", 12, new Vector2(0f, -22f), new Vector2(340f, 230f));
        financeText.alignment = TextAnchor.UpperLeft;
        financeText.supportRichText = true;

        GameObject topPlayersCard = CreatePanel(parent, "TopPlayersCard", new Vector2(225f, 15f), new Vector2(390f, 300f));
        topPlayersCard.GetComponent<Image>().color = new Color(0.07f, 0.085f, 0.115f, 0.90f);
        CreateText(topPlayersCard.transform, "TopPlayersTitle", "ЛУЧШИЕ ИГРОКИ", 16, new Vector2(0f, 118f), new Vector2(340f, 30f));
        cpuRosterAiText = CreateText(topPlayersCard.transform, "CpuRosterAiText", "1. Игрок", 12, new Vector2(0f, -22f), new Vector2(340f, 230f));
        cpuRosterAiText.alignment = TextAnchor.UpperLeft;

        GameObject seasonCard = CreatePanel(parent, "SeasonInfoCard", new Vector2(0f, -200f), new Vector2(840f, 120f));
        seasonCard.GetComponent<Image>().color = new Color(0.055f, 0.068f, 0.092f, 0.90f);
        currentDayText = CreateText(seasonCard.transform, "CurrentDayText", "", 16, new Vector2(-275f, 22f), new Vector2(250f, 38f));
        gamesSimulatedText = CreateText(seasonCard.transform, "GamesSimulatedText", "", 16, new Vector2(0f, 22f), new Vector2(270f, 38f));
        tradeStatusText = CreateText(seasonCard.transform, "TradeStatusText", "", 13, new Vector2(-170f, -30f), new Vector2(360f, 32f));
        freeAgencyStatusText = CreateText(seasonCard.transform, "FreeAgencyStatusText", "", 13, new Vector2(220f, -30f), new Vector2(360f, 32f));

        Button playMatchButton = CreateButton(parent, "PlayNextUserMatchButton", "Играть матч", new Vector2(-225f, -365f), new Vector2(360f, 70f));
        StylePrimaryButton(playMatchButton, 24);
        UnityEventTools.AddPersistentListener(playMatchButton.onClick, controller.PlayNextUserMatch);

        Button simulateMatchButton = CreateButton(parent, "SimulateMatchButton", "Симулировать матч", new Vector2(225f, -365f), new Vector2(360f, 70f));
        StyleDarkButton(simulateMatchButton, 22);
        UnityEventTools.AddPersistentListener(simulateMatchButton.onClick, controller.SimulateMatch);

        Button nextDayButton = CreateButton(parent, "AdvanceOneDayButton", "Следующий день", new Vector2(-225f, -365f), new Vector2(360f, 70f));
        StylePrimaryButton(nextDayButton, 22);
        UnityEventTools.AddPersistentListener(nextDayButton.onClick, controller.AdvanceOneDay);

        Button nextMatchButton = CreateButton(parent, "AdvanceToNextUserMatchButton", "Следующий матч", new Vector2(225f, -365f), new Vector2(360f, 70f));
        StyleDarkButton(nextMatchButton, 22);
        UnityEventTools.AddPersistentListener(nextMatchButton.onClick, controller.AdvanceToNextUserMatch);

        Button homeButton = CreateButton(parent, "HomeButton", "Главная", new Vector2(-170f, -455f), new Vector2(300f, 52f));
        StyleDarkButton(homeButton, 18);
        UnityEventTools.AddPersistentListener(homeButton.onClick, controller.ShowDashboard);

        Button playoffsButton = CreateButton(parent, "CompactPlayoffsButton", "Плей-офф", new Vector2(170f, -455f), new Vector2(300f, 52f));
        StyleDarkButton(playoffsButton, 18);
        UnityEventTools.AddPersistentListener(playoffsButton.onClick, controller.ShowPlayoffs);

        return selectedTeamText;
    }

    private static void CreateRosterPanel(Transform parent, GameScreenController controller, RosterController rosterController)
    {
        CreateText(parent, "Title", "Состав команды", 40, new Vector2(0f, 720f), new Vector2(760f, 70f));
        Text summaryText = CreateText(parent, "RosterSummaryText", "Pro 0/23 | Farm 0 | Reserve 0", 16, new Vector2(0f, 655f), new Vector2(860f, 58f));

        CreateRosterHeader(parent);

        Transform playersContainer = CreateRosterScrollView(parent);
        PlayerRowView playerRowTemplate = CreatePlayerRowTemplate(playersContainer);
        rosterController.Configure(summaryText, playersContainer, playerRowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 56f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateLineupPanel(Transform parent, GameScreenController controller, LineupController lineupController)
    {
        CreateText(parent, "Title", "Линии", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text statusText = CreateText(parent, "StatusText", "Состав на матч: ...", 15, new Vector2(0f, 650f), new Vector2(860f, 125f));
        Text ratingsText = CreateText(parent, "RatingsText", "Offense: 0 | Defense: 0 | Goalie: 0 | Total: 0\nTeam Chemistry: 60 Average | Mod +0", 15, new Vector2(0f, 555f), new Vector2(860f, 72f));
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

        Button assignButton = CreateButton(parent, "AssignPlayerButton", "Назначить игрока", new Vector2(-330f, -660f), new Vector2(250f, 50f));
        SetButtonFontSize(assignButton, 15);
        UnityEventTools.AddPersistentListener(assignButton.onClick, controller.AssignSelectedPlayerToSelectedSlot);

        Button swapGoaliesButton = CreateButton(parent, "SwapGoaliesButton", "Поменять вратарей", new Vector2(-110f, -660f), new Vector2(250f, 50f));
        SetButtonFontSize(swapGoaliesButton, 15);
        UnityEventTools.AddPersistentListener(swapGoaliesButton.onClick, controller.SwapGoalies);

        Button clearButton = CreateButton(parent, "ClearSelectionButton", "Очистить выбор", new Vector2(110f, -660f), new Vector2(250f, 50f));
        SetButtonFontSize(clearButton, 15);
        UnityEventTools.AddPersistentListener(clearButton.onClick, controller.ClearLineupSelection);

        Button autoButton = CreateButton(parent, "AutoBuildLineupButton", "Автосостав", new Vector2(330f, -660f), new Vector2(220f, 50f));
        SetButtonFontSize(autoButton, 15);
        UnityEventTools.AddPersistentListener(autoButton.onClick, controller.AutoBuildLineup);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 50f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.BackFromTeamEdit);
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

    private static void CreateMoralePanel(Transform parent, GameScreenController controller, MoraleController moraleController)
    {
        CreateText(parent, "Title", "Мораль", 40, new Vector2(0f, 735f), new Vector2(860f, 64f));
        Text summaryText = CreateText(parent, "SummaryText", "Average morale: 70", 17, new Vector2(0f, 635f), new Vector2(860f, 140f));
        Text selectedPlayerText = CreateText(parent, "SelectedPlayerText", "Выберите игрока", 15, new Vector2(0f, 500f), new Vector2(860f, 128f));

        CreateText(parent, "PlayersLabel", "Игроки", 18, new Vector2(0f, 410f), new Vector2(860f, 34f));
        Transform playersContainer = CreateDraftScrollView(parent, "MoralePlayersScrollView", new Vector2(860f, 520f), new Vector2(0f, 125f));
        MoralePlayerRowView playerRowTemplate = CreateMoralePlayerRowTemplate(playersContainer);

        CreateText(parent, "EventsLabel", "История морали", 18, new Vector2(0f, -170f), new Vector2(860f, 34f));
        Transform eventsContainer = CreateDraftScrollView(parent, "MoraleEventsScrollView", new Vector2(860f, 360f), new Vector2(0f, -385f));
        MoraleEventRowView eventRowTemplate = CreateMoraleEventRowTemplate(eventsContainer);

        moraleController.Configure(
            summaryText,
            selectedPlayerText,
            playersContainer,
            eventsContainer,
            playerRowTemplate,
            eventRowTemplate,
            controller);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 52f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateLeadershipPanel(Transform parent, GameScreenController controller, LeadershipController leadershipController)
    {
        CreateText(parent, "Title", "Капитаны и лидерство", 40, new Vector2(0f, 735f), new Vector2(860f, 64f));
        Text summaryText = CreateText(parent, "SummaryText", "Captain: none\nLeadership: Average", 17, new Vector2(0f, 620f), new Vector2(860f, 150f));
        Text selectedPlayerText = CreateText(parent, "SelectedPlayerText", "Выберите игрока", 15, new Vector2(0f, 485f), new Vector2(860f, 120f));

        CreateText(parent, "CandidatesLabel", "Leadership candidates", 18, new Vector2(0f, 392f), new Vector2(860f, 34f));
        Transform candidatesContainer = CreateDraftScrollView(parent, "LeadershipCandidatesScrollView", new Vector2(860f, 650f), new Vector2(0f, 38f));
        LeadershipCandidateRowView candidateRowTemplate = CreateLeadershipCandidateRowTemplate(candidatesContainer);

        leadershipController.Configure(
            summaryText,
            selectedPlayerText,
            candidatesContainer,
            candidateRowTemplate,
            controller);

        Button autoButton = CreateButton(parent, "AutoAssignButton", "Auto Assign", new Vector2(-330f, -650f), new Vector2(180f, 48f));
        SetButtonFontSize(autoButton, 16);
        UnityEventTools.AddPersistentListener(autoButton.onClick, controller.AutoAssignCaptains);

        Button captainButton = CreateButton(parent, "AssignCaptainButton", "Назначить капитаном", new Vector2(-120f, -650f), new Vector2(220f, 48f));
        SetButtonFontSize(captainButton, 15);
        UnityEventTools.AddPersistentListener(captainButton.onClick, controller.AssignSelectedPlayerAsCaptain);

        Button alternateButton = CreateButton(parent, "AssignAlternateButton", "Назначить ассистентом", new Vector2(130f, -650f), new Vector2(240f, 48f));
        SetButtonFontSize(alternateButton, 15);
        UnityEventTools.AddPersistentListener(alternateButton.onClick, controller.AssignSelectedPlayerAsAlternate);

        Button clearButton = CreateButton(parent, "ClearCaptaincyButton", "Снять роль", new Vector2(350f, -650f), new Vector2(180f, 48f));
        SetButtonFontSize(clearButton, 15);
        UnityEventTools.AddPersistentListener(clearButton.onClick, controller.ClearSelectedPlayerCaptaincy);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 52f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateStaffPanel(Transform parent, GameScreenController controller, StaffController staffController)
    {
        CreateText(parent, "Title", "Тренерский штаб", 40, new Vector2(0f, 735f), new Vector2(860f, 64f));
        Text summaryText = CreateText(parent, "SummaryText", "Head Coach: none\nStaff overall: 0", 17, new Vector2(0f, 622f), new Vector2(860f, 150f));
        Text effectsText = CreateText(parent, "EffectsText", "Staff effects: Off 0 | Def 0 | PP 0 | PK 0 | Dev 0", 16, new Vector2(0f, 505f), new Vector2(860f, 110f));

        CreateText(parent, "StaffMembersLabel", "Staff members", 18, new Vector2(0f, 412f), new Vector2(860f, 34f));
        Transform staffContainer = CreateDraftScrollView(parent, "StaffMembersScrollView", new Vector2(860f, 660f), new Vector2(0f, 48f));
        StaffMemberRowView rowTemplate = CreateStaffMemberRowTemplate(staffContainer);

        staffController.Configure(summaryText, effectsText, staffContainer, rowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 52f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateTacticsPanel(Transform parent, GameScreenController controller, TacticsController tacticsController)
    {
        CreateText(parent, "Title", "Тактика", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text presetText = CreateText(parent, "PresetText", "Команда\nPreset: Balanced", 20, new Vector2(0f, 660f), new Vector2(860f, 82f));
        Text parametersText = CreateText(parent, "ParametersText", "OffensiveFocus: 50 | DefensiveFocus: 50\nAggressiveness: 45 | Tempo: 50\nShootingFrequency: 50 | RiskLevel: 45", 16, new Vector2(0f, 560f), new Vector2(860f, 92f));
        Text ratingsText = CreateText(parent, "RatingsText", "PP rating: 0 | PK rating: 0\nSpecial teams chemistry: 60 Average\nСпецбригады: ...", 16, new Vector2(0f, 455f), new Vector2(860f, 110f));

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
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.BackFromTeamEdit);
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

        Button extensionsButton = CreateButton(parent, "ExtensionsButton", "Продления", new Vector2(-210f, -720f), new Vector2(300f, 56f));
        UnityEventTools.AddPersistentListener(extensionsButton.onClick, controller.ShowExtensions);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(210f, -720f), new Vector2(300f, 56f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateExtensionsPanel(
        Transform parent,
        GameScreenController controller,
        ExtensionsController extensionsController)
    {
        CreateText(parent, "Title", "Продление контрактов", 40, new Vector2(0f, 735f), new Vector2(860f, 64f));
        Text summaryText = CreateText(parent, "SummaryText", "Eligible: 0 | Pending UFA: 0 | Pending RFA: 0", 15, new Vector2(0f, 620f), new Vector2(860f, 150f));
        Text selectedPlayerText = CreateText(parent, "SelectedPlayerText", "Выберите игрока для продления", 14, new Vector2(0f, 475f), new Vector2(860f, 130f));
        Text offerText = CreateText(parent, "OfferText", "Текущее предложение: expected offer", 16, new Vector2(0f, 378f), new Vector2(860f, 42f));

        CreateText(parent, "CandidatesLabel", "Кандидаты", 18, new Vector2(-230f, 335f), new Vector2(410f, 34f));
        CreateText(parent, "OfferHistoryLabel", "История предложений", 18, new Vector2(230f, 335f), new Vector2(410f, 34f));
        Transform candidatesContainer = CreateDraftScrollView(parent, "ExtensionCandidatesScrollView", new Vector2(420f, 770f), new Vector2(-230f, -65f));
        Transform offersContainer = CreateDraftScrollView(parent, "ExtensionOffersScrollView", new Vector2(420f, 770f), new Vector2(230f, -65f));
        ExtensionCandidateRowView candidateRowTemplate = CreateExtensionCandidateRowTemplate(candidatesContainer);
        ExtensionOfferRowView offerRowTemplate = CreateExtensionOfferRowTemplate(offersContainer);

        extensionsController.Configure(
            summaryText,
            selectedPlayerText,
            offerText,
            candidatesContainer,
            offersContainer,
            candidateRowTemplate,
            offerRowTemplate,
            controller);

        Button expectedButton = CreateButton(parent, "OfferExpectedButton", "Offer Expected", new Vector2(-340f, -555f), new Vector2(170f, 48f));
        SetButtonFontSize(expectedButton, 14);
        UnityEventTools.AddPersistentListener(expectedButton.onClick, controller.OfferSelectedPlayerExpectedExtension);

        Button minimumButton = CreateButton(parent, "OfferMinimumButton", "Offer Minimum", new Vector2(-170f, -555f), new Vector2(170f, 48f));
        SetButtonFontSize(minimumButton, 14);
        UnityEventTools.AddPersistentListener(minimumButton.onClick, controller.OfferSelectedPlayerMinimumExtension);

        Button plusButton = CreateButton(parent, "OfferPlusTenButton", "Offer +10%", new Vector2(0f, -555f), new Vector2(170f, 48f));
        SetButtonFontSize(plusButton, 14);
        UnityEventTools.AddPersistentListener(plusButton.onClick, controller.OfferSelectedPlayerExpectedPlusTenExtension);

        Button oneYearButton = CreateButton(parent, "OfferOneYearButton", "Offer 1 Year", new Vector2(170f, -555f), new Vector2(170f, 48f));
        SetButtonFontSize(oneYearButton, 14);
        UnityEventTools.AddPersistentListener(oneYearButton.onClick, controller.OfferSelectedPlayerOneYearExtension);

        Button lowballButton = CreateButton(parent, "OfferLowballButton", "Lowball", new Vector2(340f, -555f), new Vector2(170f, 48f));
        SetButtonFontSize(lowballButton, 14);
        UnityEventTools.AddPersistentListener(lowballButton.onClick, controller.OfferSelectedPlayerLowballExtension);

        Button offerButton = CreateButton(parent, "OfferButton", "Offer", new Vector2(-180f, -640f), new Vector2(280f, 54f));
        UnityEventTools.AddPersistentListener(offerButton.onClick, controller.OfferSelectedPlayerExtension);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(180f, -640f), new Vector2(280f, 54f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateTradesPanel(Transform parent, GameScreenController controller, TradesController tradesController)
    {
        CreateText(parent, "Title", "Обмены", 40, new Vector2(0f, 730f), new Vector2(760f, 64f));
        Text dateText = CreateText(parent, "DateText", "Дата лиги: 2026-09-28\nTrade deadline: 2027-03-05", 17, new Vector2(0f, 680f), new Vector2(860f, 54f));
        Text statusText = CreateText(parent, "StatusText", "Обмены доступны", 17, new Vector2(0f, 636f), new Vector2(860f, 30f));
        Text selectedUserPlayerText = CreateText(parent, "SelectedUserPlayerText", "Ваш игрок: Выберите игрока", 14, new Vector2(0f, 608f), new Vector2(860f, 26f));
        Text selectedUserPickText = CreateText(parent, "SelectedUserPickText", "Ваш пик: Выберите пик", 14, new Vector2(0f, 582f), new Vector2(860f, 26f));
        Text selectedOtherTeamText = CreateText(parent, "SelectedOtherTeamText", "Команда-соперник: Выберите команду для обмена", 14, new Vector2(0f, 556f), new Vector2(860f, 26f));
        Text selectedOtherPlayerText = CreateText(parent, "SelectedOtherPlayerText", "Игрок соперника: Выберите игрока", 14, new Vector2(0f, 530f), new Vector2(860f, 26f));
        Text selectedOtherPickText = CreateText(parent, "SelectedOtherPickText", "Пик соперника: Выберите пик", 14, new Vector2(0f, 504f), new Vector2(860f, 26f));
        Text tradePartnerNeedsText = CreateText(parent, "TradePartnerNeedsText", "Team needs: выберите CPU-команду", 13, new Vector2(0f, 456f), new Vector2(860f, 64f));
        Text tradeAiDecisionText = CreateText(parent, "TradeAiDecisionText", "AI decision: предложений пока нет", 13, new Vector2(0f, 392f), new Vector2(860f, 52f));

        CreateText(parent, "UserPlayersLabel", "Ваши игроки", 16, new Vector2(-230f, 345f), new Vector2(410f, 26f));
        CreateText(parent, "TeamsLabel", "Команды", 16, new Vector2(230f, 345f), new Vector2(410f, 26f));
        CreateText(parent, "UserPicksLabel", "Ваши пики", 16, new Vector2(-230f, 120f), new Vector2(410f, 26f));
        CreateText(parent, "OtherPlayersLabel", "Игроки соперника", 16, new Vector2(230f, 120f), new Vector2(410f, 26f));
        CreateText(parent, "OtherPicksLabel", "Пики соперника", 16, new Vector2(-230f, -105f), new Vector2(410f, 26f));
        CreateText(parent, "TradeBlockLabel", "Trade block", 16, new Vector2(230f, -105f), new Vector2(410f, 26f));
        CreateText(parent, "HistoryLabel", "История обменов", 16, new Vector2(0f, -370f), new Vector2(860f, 26f));

        Transform userPlayersContainer = CreateTradeScrollView(parent, "UserPlayersScrollView", new Vector2(420f, 190f), new Vector2(-230f, 238f));
        Transform otherTeamsContainer = CreateTradeScrollView(parent, "OtherTeamsScrollView", new Vector2(420f, 190f), new Vector2(230f, 238f));
        Transform userPicksContainer = CreateTradeScrollView(parent, "UserPicksScrollView", new Vector2(420f, 190f), new Vector2(-230f, 13f));
        Transform otherPlayersContainer = CreateTradeScrollView(parent, "OtherPlayersScrollView", new Vector2(420f, 190f), new Vector2(230f, 13f));
        Transform otherPicksContainer = CreateTradeScrollView(parent, "OtherPicksScrollView", new Vector2(420f, 190f), new Vector2(-230f, -212f));
        Transform tradeBlockContainer = CreateTradeScrollView(parent, "TradeBlockListContainer", new Vector2(420f, 190f), new Vector2(230f, -212f));
        Transform historyContainer = CreateTradeScrollView(parent, "TradeHistoryScrollView", new Vector2(860f, 220f), new Vector2(0f, -500f));

        TradePlayerRowView userPlayerRowTemplate = CreateTradePlayerRowTemplate(userPlayersContainer, "UserPlayerRowTemplate");
        TradeDraftPickRowView userPickRowTemplate = CreateTradeDraftPickRowTemplate(userPicksContainer, "UserPickRowTemplate");
        TradeTeamRowView teamRowTemplate = CreateTradeTeamRowTemplate(otherTeamsContainer);
        TradePlayerRowView otherPlayerRowTemplate = CreateTradePlayerRowTemplate(otherPlayersContainer, "OtherPlayerRowTemplate");
        TradeDraftPickRowView otherPickRowTemplate = CreateTradeDraftPickRowTemplate(otherPicksContainer, "OtherPickRowTemplate");
        TradeBlockPlayerRowView tradeBlockRowTemplate = CreateTradeBlockPlayerRowTemplate(tradeBlockContainer);
        TradeHistoryRowView historyRowTemplate = CreateTradeHistoryRowTemplate(historyContainer);

        tradesController.Configure(
            dateText,
            statusText,
            selectedUserPlayerText,
            selectedUserPickText,
            selectedOtherTeamText,
            selectedOtherPlayerText,
            selectedOtherPickText,
            tradePartnerNeedsText,
            tradeAiDecisionText,
            userPlayersContainer,
            userPicksContainer,
            otherTeamsContainer,
            otherPlayersContainer,
            otherPicksContainer,
            tradeBlockContainer,
            historyContainer,
            userPlayerRowTemplate,
            userPickRowTemplate,
            teamRowTemplate,
            otherPlayerRowTemplate,
            otherPickRowTemplate,
            tradeBlockRowTemplate,
            historyRowTemplate,
            controller);

        Button proposeButton = CreateButton(parent, "ProposeTradeButton", "Предложить обмен", new Vector2(0f, -665f), new Vector2(520f, 54f));
        UnityEventTools.AddPersistentListener(proposeButton.onClick, controller.ExecuteSelectedTrade);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -735f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateFreeAgencyPanel(Transform parent, GameScreenController controller, FreeAgencyController freeAgencyController)
    {
        CreateText(parent, "Title", "Свободные агенты", 40, new Vector2(0f, 730f), new Vector2(760f, 64f));
        Text statusText = CreateText(parent, "StatusText", "Рынок свободных агентов откроется после завершения драфта", 18, new Vector2(0f, 630f), new Vector2(860f, 120f));
        Text financeText = CreateText(parent, "FinanceText", "Salary cap: 104 000 000\nPayroll: 0\nCap space: 104 000 000\nRoster size: 0 / 23", 17, new Vector2(0f, 520f), new Vector2(860f, 90f));
        Text selectedFreeAgentText = CreateText(parent, "SelectedFreeAgentText", "Выбранный свободный агент: не выбран", 15, new Vector2(0f, 430f), new Vector2(860f, 86f));

        CreateText(parent, "FreeAgentsLabel", "Рынок UFA", 20, new Vector2(0f, 378f), new Vector2(860f, 36f));
        Transform freeAgentsContainer = CreateFreeAgencyScrollView(parent);
        FreeAgentRowView freeAgentRowTemplate = CreateFreeAgentRowTemplate(freeAgentsContainer);

        CreateText(parent, "HistoryLabel", "История офферов", 20, new Vector2(0f, -270f), new Vector2(860f, 36f));
        Transform historyContainer = CreateFreeAgentHistoryScrollView(parent);
        FreeAgentOfferRowView historyRowTemplate = CreateFreeAgentOfferRowTemplate(historyContainer);

        freeAgencyController.Configure(
            statusText,
            financeText,
            selectedFreeAgentText,
            freeAgentsContainer,
            historyContainer,
            freeAgentRowTemplate,
            historyRowTemplate,
            controller);

        Button expectedButton = CreateButton(parent, "OfferExpectedButton", "Offer Expected", new Vector2(-330f, -610f), new Vector2(190f, 48f));
        SetButtonFontSize(expectedButton, 14);
        UnityEventTools.AddPersistentListener(expectedButton.onClick, controller.OfferSelectedFreeAgentExpectedContract);

        Button minimumButton = CreateButton(parent, "OfferMinimumButton", "Offer Minimum", new Vector2(-110f, -610f), new Vector2(190f, 48f));
        SetButtonFontSize(minimumButton, 14);
        UnityEventTools.AddPersistentListener(minimumButton.onClick, controller.OfferSelectedFreeAgentMinimumContract);

        Button plusTenButton = CreateButton(parent, "OfferPlusTenButton", "Offer +10%", new Vector2(110f, -610f), new Vector2(190f, 48f));
        SetButtonFontSize(plusTenButton, 14);
        UnityEventTools.AddPersistentListener(plusTenButton.onClick, controller.OfferSelectedFreeAgentPlusTenPercent);

        Button customButton = CreateButton(parent, "OfferCustomButton", "Offer Custom", new Vector2(330f, -610f), new Vector2(190f, 48f));
        SetButtonFontSize(customButton, 14);
        UnityEventTools.AddPersistentListener(customButton.onClick, controller.OfferSelectedFreeAgentCustomContract);

        Button cpuButton = CreateButton(parent, "RunCpuFreeAgencyButton", "Run CPU Free Agency", new Vector2(-170f, -675f), new Vector2(300f, 48f));
        SetButtonFontSize(cpuButton, 14);
        UnityEventTools.AddPersistentListener(cpuButton.onClick, controller.RunCpuFreeAgencySignings);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(220f, -675f), new Vector2(260f, 48f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateScoutingPanel(Transform parent, GameScreenController controller, ScoutingController scoutingController)
    {
        CreateText(parent, "Title", "Скаутинг", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text summaryText = CreateText(parent, "SummaryText", "Scouting actions: 0 | Reports: 0", 15, new Vector2(0f, 612f), new Vector2(860f, 180f));
        Text selectedProspectText = CreateText(parent, "SelectedProspectText", "Выберите проспекта", 15, new Vector2(0f, 470f), new Vector2(860f, 96f));

        CreateText(parent, "ProspectsLabel", "Draft prospects", 18, new Vector2(-230f, 392f), new Vector2(410f, 34f));
        CreateText(parent, "ReportsLabel", "Recent scouting reports", 18, new Vector2(230f, 392f), new Vector2(410f, 34f));

        Transform prospectsContainer = CreateDraftScrollView(parent, "ScoutingProspectsScrollView", new Vector2(420f, 820f), new Vector2(-230f, -30f));
        Transform reportsContainer = CreateDraftScrollView(parent, "ScoutingReportsScrollView", new Vector2(420f, 820f), new Vector2(230f, -30f));
        ScoutingProspectRowView prospectRowTemplate = CreateScoutingProspectRowTemplate(prospectsContainer);
        ScoutingReportRowView reportRowTemplate = CreateScoutingReportRowTemplate(reportsContainer);

        scoutingController.Configure(
            summaryText,
            selectedProspectText,
            prospectsContainer,
            reportsContainer,
            prospectRowTemplate,
            reportRowTemplate,
            controller);

        Button scoutPlayerButton = CreateButton(parent, "ScoutPlayerButton", "Scout Player", new Vector2(-330f, -635f), new Vector2(240f, 48f));
        SetButtonFontSize(scoutPlayerButton, 15);
        UnityEventTools.AddPersistentListener(scoutPlayerButton.onClick, controller.ScoutSelectedProspect);

        Button scoutTopButton = CreateButton(parent, "ScoutTopProspectsButton", "Scout Top Prospects", new Vector2(-110f, -635f), new Vector2(260f, 48f));
        SetButtonFontSize(scoutTopButton, 14);
        UnityEventTools.AddPersistentListener(scoutTopButton.onClick, controller.ScoutTopProspects);

        Button scoutForwardsButton = CreateButton(parent, "ScoutForwardsButton", "Scout Forwards", new Vector2(120f, -635f), new Vector2(220f, 48f));
        SetButtonFontSize(scoutForwardsButton, 14);
        UnityEventTools.AddPersistentListener(scoutForwardsButton.onClick, controller.ScoutForwards);

        Button scoutDefenseButton = CreateButton(parent, "ScoutDefensemenButton", "Scout Defensemen", new Vector2(335f, -635f), new Vector2(240f, 48f));
        SetButtonFontSize(scoutDefenseButton, 14);
        UnityEventTools.AddPersistentListener(scoutDefenseButton.onClick, controller.ScoutDefensemen);

        Button scoutGoaliesButton = CreateButton(parent, "ScoutGoaliesButton", "Scout Goalies", new Vector2(-150f, -700f), new Vector2(240f, 48f));
        SetButtonFontSize(scoutGoaliesButton, 15);
        UnityEventTools.AddPersistentListener(scoutGoaliesButton.onClick, controller.ScoutGoalies);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(150f, -700f), new Vector2(240f, 48f));
        SetButtonFontSize(backButton, 16);
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

    private static void CreateOrganizationPanel(
        Transform parent,
        GameScreenController controller,
        OrganizationController organizationController)
    {
        CreateText(parent, "Title", "Организация", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text summaryText = CreateText(parent, "SummaryText", "Pro roster: 0 / 23 | Farm: 0 | Reserve: 0", 16, new Vector2(0f, 625f), new Vector2(860f, 130f));
        Text selectedPlayerText = CreateText(parent, "SelectedPlayerText", "Игрок не выбран", 16, new Vector2(0f, 505f), new Vector2(860f, 90f));

        CreateText(parent, "NhlLabel", "Pro roster", 18, new Vector2(-300f, 430f), new Vector2(260f, 34f));
        CreateText(parent, "FarmLabel", "Farm", 18, new Vector2(0f, 430f), new Vector2(260f, 34f));
        CreateText(parent, "ReserveLabel", "Reserve", 18, new Vector2(300f, 430f), new Vector2(260f, 34f));

        Transform nhlContainer = CreateDraftScrollView(parent, "NhlRosterScrollView", new Vector2(280f, 760f), new Vector2(-300f, 30f));
        Transform farmContainer = CreateDraftScrollView(parent, "FarmRosterScrollView", new Vector2(280f, 760f), new Vector2(0f, 30f));
        Transform reserveContainer = CreateDraftScrollView(parent, "ReserveRosterScrollView", new Vector2(280f, 760f), new Vector2(300f, 30f));

        OrganizationPlayerRowView nhlRowTemplate = CreateOrganizationPlayerRowTemplate(nhlContainer, "NhlOrganizationPlayerRowTemplate");
        OrganizationPlayerRowView farmRowTemplate = CreateOrganizationPlayerRowTemplate(farmContainer, "FarmOrganizationPlayerRowTemplate");
        OrganizationPlayerRowView reserveRowTemplate = CreateOrganizationPlayerRowTemplate(reserveContainer, "ReserveOrganizationPlayerRowTemplate");

        organizationController.Configure(
            summaryText,
            selectedPlayerText,
            nhlContainer,
            farmContainer,
            reserveContainer,
            nhlRowTemplate,
            farmRowTemplate,
            reserveRowTemplate);

        Button sendToFarmButton = CreateButton(parent, "SendToFarmButton", "Отправить в фарм", new Vector2(-305f, -550f), new Vector2(270f, 46f));
        SetButtonFontSize(sendToFarmButton, 14);
        UnityEventTools.AddPersistentListener(sendToFarmButton.onClick, controller.SendSelectedPlayerToFarm);

        Button callUpButton = CreateButton(parent, "CallUpButton", "Вызвать в Pro", new Vector2(0f, -550f), new Vector2(270f, 46f));
        SetButtonFontSize(callUpButton, 14);
        UnityEventTools.AddPersistentListener(callUpButton.onClick, controller.CallUpSelectedPlayerToNhl);

        Button moveReserveButton = CreateButton(parent, "MoveReserveButton", "В резерв", new Vector2(305f, -550f), new Vector2(270f, 46f));
        SetButtonFontSize(moveReserveButton, 14);
        UnityEventTools.AddPersistentListener(moveReserveButton.onClick, controller.MoveSelectedPlayerToReserve);

        Button reserveToNhlButton = CreateButton(parent, "ReserveToNhlButton", "Из резерва в Pro", new Vector2(-155f, -610f), new Vector2(300f, 46f));
        SetButtonFontSize(reserveToNhlButton, 14);
        UnityEventTools.AddPersistentListener(reserveToNhlButton.onClick, controller.MoveSelectedReservePlayerToNhl);

        Button reserveToFarmButton = CreateButton(parent, "ReserveToFarmButton", "Из резерва в фарм", new Vector2(155f, -610f), new Vector2(300f, 46f));
        SetButtonFontSize(reserveToFarmButton, 14);
        UnityEventTools.AddPersistentListener(reserveToFarmButton.onClick, controller.MoveSelectedReservePlayerToFarm);

        Button autoFixButton = CreateButton(parent, "AutoFixRosterAndLineupButton", "Автозамена", new Vector2(-170f, -670f), new Vector2(280f, 48f));
        SetButtonFontSize(autoFixButton, 16);
        UnityEventTools.AddPersistentListener(autoFixButton.onClick, controller.AutoFixRosterAndLineup);

        Button lineupButton = CreateButton(parent, "OrganizationLineupButton", "Состав на матч", new Vector2(170f, -670f), new Vector2(280f, 48f));
        SetButtonFontSize(lineupButton, 16);
        UnityEventTools.AddPersistentListener(lineupButton.onClick, controller.ShowLineupFromPreGame);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -735f), new Vector2(320f, 52f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateWaiversPanel(
        Transform parent,
        GameScreenController controller,
        WaiversController waiversController)
    {
        CreateText(parent, "Title", "Waivers", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text summaryText = CreateText(parent, "SummaryText", "Active waivers: 0 | History: 0", 18, new Vector2(0f, 650f), new Vector2(860f, 64f));
        Text selectedWaiverText = CreateText(parent, "SelectedWaiverText", "Waiver entry не выбран", 16, new Vector2(0f, 535f), new Vector2(860f, 120f));

        CreateText(parent, "ActiveLabel", "Active waivers", 22, new Vector2(0f, 450f), new Vector2(860f, 42f));
        Transform activeContainer = CreateDraftScrollView(parent, "ActiveWaiversScrollView", new Vector2(860f, 420f), new Vector2(0f, 210f));
        WaiverRowView activeRowTemplate = CreateWaiverRowTemplate(activeContainer, "ActiveWaiverRowTemplate");

        CreateText(parent, "HistoryLabel", "History", 22, new Vector2(0f, -45f), new Vector2(860f, 42f));
        Transform historyContainer = CreateDraftScrollView(parent, "WaiverHistoryScrollView", new Vector2(860f, 420f), new Vector2(0f, -285f));
        WaiverRowView historyRowTemplate = CreateWaiverRowTemplate(historyContainer, "WaiverHistoryRowTemplate");

        waiversController.Configure(
            summaryText,
            selectedWaiverText,
            activeContainer,
            historyContainer,
            activeRowTemplate,
            historyRowTemplate);

        Button claimButton = CreateButton(parent, "ClaimWaiverButton", "Claim", new Vector2(-180f, -700f), new Vector2(320f, 52f));
        UnityEventTools.AddPersistentListener(claimButton.onClick, controller.ClaimSelectedWaiverPlayer);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(180f, -700f), new Vector2(320f, 52f));
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

    private static void CreateOwnerPanel(
        Transform parent,
        GameScreenController controller,
        OwnerController ownerController)
    {
        CreateText(parent, "Title", "Владелец", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text summaryText = CreateText(parent, "OwnerSummaryText", "GM trust: 65\nJob security: Safe\nPrimary goal: ...", 18, new Vector2(0f, 610f), new Vector2(860f, 150f));
        Text financeText = CreateText(parent, "OwnerFinanceText", "Payroll: 0 | Cap space: 104 000 000\nRevenue est.: 0 | Profit est.: 0\nFinancial health: Stable", 17, new Vector2(0f, 465f), new Vector2(860f, 120f));

        CreateText(parent, "GoalsLabel", "Цели сезона", 22, new Vector2(0f, 365f), new Vector2(860f, 42f));
        Transform goalsContainer = CreateDraftScrollView(parent, "OwnerGoalsScrollView", new Vector2(860f, 390f), new Vector2(0f, 135f));
        OwnerGoalRowView goalRowTemplate = CreateOwnerGoalRowTemplate(goalsContainer);

        CreateText(parent, "EvaluationsLabel", "История оценок", 22, new Vector2(0f, -105f), new Vector2(860f, 42f));
        Transform evaluationsContainer = CreateDraftScrollView(parent, "OwnerEvaluationsScrollView", new Vector2(860f, 420f), new Vector2(0f, -350f));
        OwnerEvaluationRowView evaluationRowTemplate = CreateOwnerEvaluationRowTemplate(evaluationsContainer);

        ownerController.Configure(
            summaryText,
            financeText,
            goalsContainer,
            evaluationsContainer,
            goalRowTemplate,
            evaluationRowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateGmCareerPanel(
        Transform parent,
        GameScreenController controller,
        GmCareerController gmCareerController)
    {
        CreateText(parent, "Title", "Карьера GM", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text summaryText = CreateText(parent, "GmCareerSummaryText", "GM Career: нет данных", 17, new Vector2(0f, 600f), new Vector2(860f, 210f));
        Text selectedOfferText = CreateText(parent, "SelectedOfferText", "Выберите предложение работы", 15, new Vector2(0f, 430f), new Vector2(860f, 130f));

        CreateText(parent, "JobOffersLabel", "Job offers", 18, new Vector2(-230f, 335f), new Vector2(410f, 34f));
        CreateText(parent, "CareerEventsLabel", "Career events", 18, new Vector2(230f, 335f), new Vector2(410f, 34f));

        Transform offersContainer = CreateDraftScrollView(parent, "GmJobOffersScrollView", new Vector2(420f, 680f), new Vector2(-230f, -25f));
        Transform eventsContainer = CreateDraftScrollView(parent, "GmCareerEventsScrollView", new Vector2(420f, 680f), new Vector2(230f, -25f));
        GmJobOfferRowView offerRowTemplate = CreateGmJobOfferRowTemplate(offersContainer);
        GmCareerEventRowView eventRowTemplate = CreateGmCareerEventRowTemplate(eventsContainer);

        gmCareerController.Configure(
            summaryText,
            selectedOfferText,
            offersContainer,
            eventsContainer,
            offerRowTemplate,
            eventRowTemplate,
            controller);

        Button generateButton = CreateButton(parent, "GenerateOffersButton", "Generate Offers", new Vector2(-330f, -660f), new Vector2(210f, 50f));
        SetButtonFontSize(generateButton, 15);
        UnityEventTools.AddPersistentListener(generateButton.onClick, controller.GenerateGmJobOffers);

        Button acceptButton = CreateButton(parent, "AcceptOfferButton", "Accept Offer", new Vector2(-110f, -660f), new Vector2(210f, 50f));
        SetButtonFontSize(acceptButton, 15);
        UnityEventTools.AddPersistentListener(acceptButton.onClick, controller.AcceptSelectedGmJobOffer);

        Button declineButton = CreateButton(parent, "DeclineOfferButton", "Decline Offer", new Vector2(110f, -660f), new Vector2(210f, 50f));
        SetButtonFontSize(declineButton, 15);
        UnityEventTools.AddPersistentListener(declineButton.onClick, controller.DeclineSelectedGmJobOffer);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(330f, -660f), new Vector2(210f, 50f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateDiagnosticsPanel(
        Transform parent,
        GameScreenController controller,
        DiagnosticsController diagnosticsController)
    {
        CreateText(parent, "Title", "Diagnostics", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text summaryText = CreateText(parent, "DiagnosticsSummaryText", "Diagnostics not run yet.", 14, new Vector2(0f, 620f), new Vector2(860f, 150f));
        Text migrationText = CreateText(parent, "MigrationReportText", "Migration: not run yet.", 13, new Vector2(-230f, 485f), new Vector2(410f, 110f));
        Text validationText = CreateText(parent, "ValidationReportText", "Validation: not run yet.", 13, new Vector2(230f, 485f), new Vector2(410f, 110f));
        Text balanceText = CreateText(parent, "BalanceReportText", "Balance: not run yet.", 11, new Vector2(0f, 190f), new Vector2(860f, 440f));
        balanceText.alignment = TextAnchor.UpperLeft;
        Text issuesText = CreateText(parent, "ValidationIssuesText", "Validation issues: none", 11, new Vector2(0f, -245f), new Vector2(860f, 380f));
        issuesText.alignment = TextAnchor.UpperLeft;

        diagnosticsController.Configure(summaryText, migrationText, validationText, balanceText, issuesText);

        Button validateButton = CreateButton(parent, "ValidateButton", "Validate", new Vector2(-350f, -570f), new Vector2(170f, 46f));
        SetButtonFontSize(validateButton, 14);
        UnityEventTools.AddPersistentListener(validateButton.onClick, controller.RunDiagnosticsValidation);

        Button repairButton = CreateButton(parent, "RepairSafeButton", "Repair Safe", new Vector2(-175f, -570f), new Vector2(170f, 46f));
        SetButtonFontSize(repairButton, 14);
        UnityEventTools.AddPersistentListener(repairButton.onClick, controller.RunDiagnosticsRepair);

        Button balanceButton = CreateButton(parent, "BalanceButton", "Balance", new Vector2(0f, -570f), new Vector2(170f, 46f));
        SetButtonFontSize(balanceButton, 14);
        UnityEventTools.AddPersistentListener(balanceButton.onClick, controller.RunDiagnosticsBalanceReport);

        Button alphaButton = CreateButton(parent, "AlphaReportButton", "Alpha Report", new Vector2(175f, -570f), new Vector2(170f, 46f));
        SetButtonFontSize(alphaButton, 13);
        UnityEventTools.AddPersistentListener(alphaButton.onClick, controller.RunAlphaBalanceReport);

        Button migrationButton = CreateButton(parent, "MigrationButton", "Migrate", new Vector2(350f, -570f), new Vector2(170f, 46f));
        SetButtonFontSize(migrationButton, 14);
        UnityEventTools.AddPersistentListener(migrationButton.onClick, controller.RunDiagnosticsMigration);

        Button alphaOneButton = CreateButton(parent, "AlphaOneSeasonButton", "Alpha 1 Season", new Vector2(-270f, -630f), new Vector2(210f, 44f));
        SetButtonFontSize(alphaOneButton, 12);
        UnityEventTools.AddPersistentListener(alphaOneButton.onClick, controller.RunAlphaBalanceReport1Season);

        Button alphaThreeButton = CreateButton(parent, "AlphaThreeSeasonsButton", "Alpha 3 Seasons", new Vector2(0f, -630f), new Vector2(210f, 44f));
        SetButtonFontSize(alphaThreeButton, 12);
        UnityEventTools.AddPersistentListener(alphaThreeButton.onClick, controller.RunAlphaBalanceReport3Seasons);

        Button alphaFiveButton = CreateButton(parent, "AlphaFiveSeasonsButton", "Alpha 5 Seasons", new Vector2(270f, -630f), new Vector2(210f, 44f));
        SetButtonFontSize(alphaFiveButton, 12);
        UnityEventTools.AddPersistentListener(alphaFiveButton.onClick, controller.RunAlphaBalanceReport5Seasons);

        Button androidReadinessButton = CreateButton(parent, "AndroidReadinessButton", "Android Readiness", new Vector2(-210f, -690f), new Vector2(260f, 44f));
        SetButtonFontSize(androidReadinessButton, 12);
        UnityEventTools.AddPersistentListener(androidReadinessButton.onClick, controller.RunAndroidReadinessCheck);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(210f, -690f), new Vector2(260f, 52f));
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

    private static void CreateHistoryPanel(
        Transform parent,
        GameScreenController controller,
        HistoryController historyController)
    {
        CreateText(parent, "Title", "История карьеры", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text summaryText = CreateText(parent, "HistorySummaryText", "История появится после завершения сезона.", 15, new Vector2(0f, 660f), new Vector2(860f, 110f));

        CreateText(parent, "LeagueHistoryLabel", "Сезоны лиги", 17, new Vector2(-230f, 580f), new Vector2(410f, 32f));
        Transform seasonHistoryContainer = CreateDraftScrollView(parent, "LeagueHistoryScrollView", new Vector2(420f, 295f), new Vector2(-230f, 415f));
        SeasonHistoryRowView seasonHistoryRowTemplate = CreateSeasonHistoryRowTemplate(seasonHistoryContainer);

        CreateText(parent, "AwardsLabel", "Награды", 17, new Vector2(230f, 580f), new Vector2(410f, 32f));
        Transform awardsContainer = CreateDraftScrollView(parent, "AwardsScrollView", new Vector2(420f, 295f), new Vector2(230f, 415f));
        AwardWinnerRowView awardRowTemplate = CreateAwardWinnerRowTemplate(awardsContainer);

        CreateText(parent, "RecordsLabel", "Рекорды карьеры", 17, new Vector2(-230f, 245f), new Vector2(410f, 32f));
        Transform recordsContainer = CreateDraftScrollView(parent, "RecordsScrollView", new Vector2(420f, 245f), new Vector2(-230f, 105f));
        LeagueRecordRowView recordRowTemplate = CreateLeagueRecordRowTemplate(recordsContainer);

        CreateText(parent, "UserTeamHistoryLabel", "История вашей команды", 17, new Vector2(230f, 245f), new Vector2(410f, 32f));
        Transform userTeamHistoryContainer = CreateDraftScrollView(parent, "UserTeamHistoryScrollView", new Vector2(420f, 245f), new Vector2(230f, 105f));
        UserTeamHistoryRowView userTeamHistoryRowTemplate = CreateUserTeamHistoryRowTemplate(userTeamHistoryContainer);

        CreateText(parent, "RetiredPlayersLabel", "Retired players", 16, new Vector2(-300f, -50f), new Vector2(270f, 30f));
        Transform retiredPlayersContainer = CreateDraftScrollView(parent, "RetiredPlayersScrollView", new Vector2(280f, 430f), new Vector2(-300f, -290f));
        RetiredPlayerRowView retiredPlayerRowTemplate = CreateRetiredPlayerRowTemplate(retiredPlayersContainer);

        CreateText(parent, "HallOfFameLabel", "Hall of Fame", 16, new Vector2(0f, -50f), new Vector2(270f, 30f));
        Transform hallOfFameContainer = CreateDraftScrollView(parent, "HallOfFameScrollView", new Vector2(280f, 430f), new Vector2(0f, -290f));
        HallOfFameRowView hallOfFameRowTemplate = CreateHallOfFameRowTemplate(hallOfFameContainer);

        CreateText(parent, "RetiredNumbersLabel", "Retired numbers", 16, new Vector2(300f, -50f), new Vector2(270f, 30f));
        Transform retiredNumbersContainer = CreateDraftScrollView(parent, "RetiredNumbersScrollView", new Vector2(280f, 430f), new Vector2(300f, -290f));
        RetiredNumberRowView retiredNumberRowTemplate = CreateRetiredNumberRowTemplate(retiredNumbersContainer);

        historyController.Configure(
            summaryText,
            seasonHistoryContainer,
            awardsContainer,
            recordsContainer,
            userTeamHistoryContainer,
            retiredPlayersContainer,
            hallOfFameContainer,
            retiredNumbersContainer,
            seasonHistoryRowTemplate,
            awardRowTemplate,
            recordRowTemplate,
            userTeamHistoryRowTemplate,
            retiredPlayerRowTemplate,
            hallOfFameRowTemplate,
            retiredNumberRowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateNewsPanel(
        Transform parent,
        GameScreenController controller,
        NewsController newsController)
    {
        CreateText(parent, "Title", "Новости", 40, new Vector2(0f, 735f), new Vector2(760f, 64f));
        Text summaryText = CreateText(parent, "SummaryText", "Новостей пока нет", 16, new Vector2(0f, 650f), new Vector2(860f, 88f));
        Text filterText = CreateText(parent, "FilterText", "Фильтр: Все", 16, new Vector2(0f, 590f), new Vector2(860f, 40f));

        float y = 535f;
        Button allButton = CreateNewsFilterButton(parent, "AllNewsButton", "Все", new Vector2(-330f, y));
        UnityEventTools.AddPersistentListener(allButton.onClick, controller.ShowAllNews);
        Button userButton = CreateNewsFilterButton(parent, "UserTeamNewsButton", "Моя команда", new Vector2(-110f, y));
        UnityEventTools.AddPersistentListener(userButton.onClick, controller.ShowUserTeamNews);
        Button recapButton = CreateNewsFilterButton(parent, "SeasonRecapNewsButton", "Recap", new Vector2(110f, y));
        UnityEventTools.AddPersistentListener(recapButton.onClick, controller.ShowSeasonRecapNews);
        Button awardsButton = CreateNewsFilterButton(parent, "AwardNewsButton", "Награды", new Vector2(330f, y));
        UnityEventTools.AddPersistentListener(awardsButton.onClick, controller.ShowAwardNews);

        y -= 54f;
        Button recordsButton = CreateNewsFilterButton(parent, "RecordNewsButton", "Рекорды", new Vector2(-330f, y));
        UnityEventTools.AddPersistentListener(recordsButton.onClick, controller.ShowRecordNews);
        Button tradeButton = CreateNewsFilterButton(parent, "TradeNewsButton", "Обмены", new Vector2(-110f, y));
        UnityEventTools.AddPersistentListener(tradeButton.onClick, controller.ShowTradeNews);
        Button injuryButton = CreateNewsFilterButton(parent, "InjuryNewsButton", "Травмы", new Vector2(110f, y));
        UnityEventTools.AddPersistentListener(injuryButton.onClick, controller.ShowInjuryNews);
        Button contractButton = CreateNewsFilterButton(parent, "ContractNewsButton", "Контракты", new Vector2(330f, y));
        UnityEventTools.AddPersistentListener(contractButton.onClick, controller.ShowContractNews);

        y -= 54f;
        Button freeAgencyButton = CreateNewsFilterButton(parent, "FreeAgencyNewsButton", "FA", new Vector2(-330f, y));
        UnityEventTools.AddPersistentListener(freeAgencyButton.onClick, controller.ShowFreeAgencyNews);
        Button draftButton = CreateNewsFilterButton(parent, "DraftNewsButton", "Драфт", new Vector2(-110f, y));
        UnityEventTools.AddPersistentListener(draftButton.onClick, controller.ShowDraftNews);
        Button ownerButton = CreateNewsFilterButton(parent, "OwnerNewsButton", "Владелец", new Vector2(110f, y));
        UnityEventTools.AddPersistentListener(ownerButton.onClick, controller.ShowOwnerNews);
        Button developmentButton = CreateNewsFilterButton(parent, "DevelopmentNewsButton", "Развитие", new Vector2(330f, y));
        UnityEventTools.AddPersistentListener(developmentButton.onClick, controller.ShowDevelopmentNews);

        Transform newsContainer = CreateDraftScrollView(parent, "NewsScrollView", new Vector2(860f, 900f), new Vector2(0f, -115f));
        NewsItemRowView newsRowTemplate = CreateNewsItemRowTemplate(newsContainer);
        newsController.Configure(summaryText, filterText, newsContainer, newsRowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 54f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateCalendarPanel(Transform parent, GameScreenController controller, CalendarController calendarController)
    {
        CreateText(parent, "Title", "Календарь", 40, new Vector2(0f, 720f), new Vector2(760f, 70f));
        Text statusText = CreateText(parent, "CalendarStatusText", "Текущий день: 1 | Выбран день: 1", 18, new Vector2(0f, 660f), new Vector2(840f, 36f));
        Button previousDayButton = CreateButton(parent, "CalendarPreviousDayButton", "<", new Vector2(-310f, 605f), new Vector2(90f, 46f));
        UnityEventTools.AddPersistentListener(previousDayButton.onClick, controller.CalendarPreviousDay);
        Button simulateToDayButton = CreateButton(parent, "CalendarSimulateToDayButton", "Симулировать до дня", new Vector2(0f, 605f), new Vector2(430f, 46f));
        UnityEventTools.AddPersistentListener(simulateToDayButton.onClick, controller.SimulateToSelectedCalendarDay);
        Button nextDayButton = CreateButton(parent, "CalendarNextDayButton", ">", new Vector2(310f, 605f), new Vector2(90f, 46f));
        UnityEventTools.AddPersistentListener(nextDayButton.onClick, controller.CalendarNextDay);

        GameObject calendarGrid = CreatePanel(parent, "CalendarGrid", new Vector2(0f, -45f), new Vector2(860f, 1060f));
        calendarGrid.GetComponent<Image>().color = new Color(0.018f, 0.045f, 0.095f, 0.94f);
        ScheduleGameRowView gameRowTemplate = CreateScheduleGameRowTemplate(calendarGrid.transform);
        calendarController.Configure(statusText, calendarGrid.transform, gameRowTemplate);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -720f), new Vector2(320f, 56f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateStandingsPanel(Transform parent, GameScreenController controller, StandingsController standingsController)
    {
        CreateText(parent, "Title", "Турнирная таблица", 40, new Vector2(0f, 720f), new Vector2(760f, 70f));
        Button divisionsButton = CreateButton(parent, "StandingsDivisionsButton", "Дивизионы", new Vector2(-220f, 660f), new Vector2(260f, 46f));
        UnityEventTools.AddPersistentListener(divisionsButton.onClick, controller.ShowStandingsDivisions);
        Button conferencesButton = CreateButton(parent, "StandingsConferencesButton", "Конференции", new Vector2(220f, 660f), new Vector2(260f, 46f));
        UnityEventTools.AddPersistentListener(conferencesButton.onClick, controller.ShowStandingsConferences);
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
        Button forwardsButton = CreateButton(parent, "StatsForwardsButton", "Нападающие", new Vector2(-315f, 660f), new Vector2(190f, 42f));
        UnityEventTools.AddPersistentListener(forwardsButton.onClick, controller.ShowStatsForwards);
        Button defenseButton = CreateButton(parent, "StatsDefenseButton", "Защитники", new Vector2(-105f, 660f), new Vector2(190f, 42f));
        UnityEventTools.AddPersistentListener(defenseButton.onClick, controller.ShowStatsDefensemen);
        Button goaliesButton = CreateButton(parent, "StatsGoaliesButton", "Вратари", new Vector2(105f, 660f), new Vector2(190f, 42f));
        UnityEventTools.AddPersistentListener(goaliesButton.onClick, controller.ShowStatsGoalies);
        Button under21Button = CreateButton(parent, "StatsUnder21Button", "До 21", new Vector2(315f, 660f), new Vector2(190f, 42f));
        UnityEventTools.AddPersistentListener(under21Button.onClick, controller.ShowStatsUnder21);
        Button previousTeamButton = CreateButton(parent, "StatsPreviousTeamButton", "-", new Vector2(-330f, 600f), new Vector2(80f, 42f));
        UnityEventTools.AddPersistentListener(previousTeamButton.onClick, controller.SelectPreviousPlayerStatsTeam);
        Dropdown teamDropdown = CreateDropdown(parent, "StatsTeamDropdown", new Vector2(0f, 600f), new Vector2(520f, 42f));
        controller.PlayerStatsTeamDropdown = teamDropdown;
        Button nextTeamButton = CreateButton(parent, "StatsNextTeamButton", "+", new Vector2(330f, 600f), new Vector2(80f, 42f));
        UnityEventTools.AddPersistentListener(nextTeamButton.onClick, controller.SelectNextPlayerStatsTeam);
        Button teamButton = CreateButton(parent, "StatsTeamButton", "Команда", new Vector2(0f, 548f), new Vector2(240f, 42f));
        UnityEventTools.AddPersistentListener(teamButton.onClick, controller.ShowStatsSelectedTeam);

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

    private static void CreatePreGamePanel(Transform parent, GameScreenController controller, PreGameController preGameController)
    {
        preGameController.TitleText = CreateText(parent, "Title", "Следующий матч", 36, new Vector2(0f, 700f), new Vector2(760f, 62f));
        preGameController.MatchupText = CreateText(parent, "MatchupText", "", 26, new Vector2(0f, 635f), new Vector2(820f, 56f));
        preGameController.DetailsText = CreateText(parent, "DetailsText", "", 20, new Vector2(0f, 580f), new Vector2(820f, 44f));
        preGameController.AwayLogoImage = CreateTeamLogoImage(parent, "AwayLogo", new Vector2(-285f, 455f), new Vector2(125f, 125f));
        preGameController.HomeLogoImage = CreateTeamLogoImage(parent, "HomeLogo", new Vector2(285f, 455f), new Vector2(125f, 125f));
        preGameController.AwayJerseyImage = CreateTeamLogoImage(parent, "AwayJersey", new Vector2(-285f, 300f), new Vector2(125f, 125f));
        preGameController.HomeJerseyImage = CreateTeamLogoImage(parent, "HomeJersey", new Vector2(285f, 300f), new Vector2(125f, 125f));
        preGameController.AwayTeamInfoText = CreateText(parent, "AwayTeamInfoText", "", 15, new Vector2(-285f, 160f), new Vector2(340f, 170f));
        preGameController.HomeTeamInfoText = CreateText(parent, "HomeTeamInfoText", "", 15, new Vector2(285f, 160f), new Vector2(340f, 170f));
        preGameController.LineupText = CreateText(parent, "LineupText", "", 15, new Vector2(0f, -65f), new Vector2(820f, 250f));
        preGameController.TacticsText = CreateText(parent, "TacticsText", "", 20, new Vector2(0f, -250f), new Vector2(820f, 44f));

        Button lineupButton = CreateButton(parent, "PreGameLineupButton", "Состав", new Vector2(-280f, -420f), new Vector2(240f, 56f));
        UnityEventTools.AddPersistentListener(lineupButton.onClick, controller.ShowLineup);

        Button tacticsButton = CreateButton(parent, "PreGameTacticsButton", "Тактика", new Vector2(0f, -420f), new Vector2(240f, 56f));
        UnityEventTools.AddPersistentListener(tacticsButton.onClick, controller.ShowTacticsFromPreGame);

        Button startButton = CreateButton(parent, "StartLiveMatchButton", "Начать матч", new Vector2(280f, -420f), new Vector2(240f, 56f));
        preGameController.StartButton = startButton;
        UnityEventTools.AddPersistentListener(startButton.onClick, controller.StartLiveMatch);

        Button backButton = CreateButton(parent, "BackButton", "Назад", new Vector2(0f, -500f), new Vector2(320f, 60f));
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowDashboard);
    }

    private static void CreateLiveMatchPanel(Transform parent, GameScreenController controller, LiveMatchController liveMatchController)
    {
        liveMatchController.ScoreboardText = CreateText(parent, "ScoreboardText", "0 - 0", 24, new Vector2(0f, 640f), new Vector2(820f, 58f));
        liveMatchController.ScoreboardText.resizeTextForBestFit = true;
        liveMatchController.ScoreboardText.resizeTextMinSize = 18;
        liveMatchController.ScoreboardText.resizeTextMaxSize = 26;
        liveMatchController.ClockText = CreateText(parent, "ClockText", "1 | 20:00", 20, new Vector2(0f, 594f), new Vector2(820f, 38f));
        liveMatchController.ClockText.resizeTextForBestFit = true;
        liveMatchController.ClockText.resizeTextMinSize = 16;
        liveMatchController.ClockText.resizeTextMaxSize = 21;
        liveMatchController.StatsText = CreateText(parent, "StatsText", "Броски: 0 - 0", 16, new Vector2(-220f, 530f), new Vector2(360f, 70f));
        liveMatchController.PowerPlayText = CreateText(parent, "PowerPlayText", "Равные составы", 16, new Vector2(220f, 530f), new Vector2(360f, 70f));

        GameObject rink = CreatePanel(parent, "RinkArea", new Vector2(0f, 150f), new Vector2(820f, 520f));
        Image rinkImage = rink.GetComponent<Image>();
        if (rinkImage != null)
        {
            rinkImage.color = new Color(0.055f, 0.115f, 0.145f, 0.88f);
        }

        liveMatchController.RinkArea = rink.GetComponent<RectTransform>();
        LiveMatchPlayerTokenView tokenPrefab = CreateLiveMatchTokenTemplate(rink.transform);
        liveMatchController.TokenPrefab = tokenPrefab;

        liveMatchController.EventFeedText = CreateText(parent, "EventFeedText", "", 13, new Vector2(-220f, -210f), new Vector2(400f, 290f));
        liveMatchController.EventFeedText.alignment = TextAnchor.UpperLeft;
        liveMatchController.TokenDetailsText = CreateText(parent, "TokenDetailsText", "Нажмите на номер игрока", 14, new Vector2(245f, -170f), new Vector2(360f, 120f));
        liveMatchController.TokenDetailsText.alignment = TextAnchor.UpperLeft;
        liveMatchController.TokenFullBodyImage = CreateTeamLogoImage(parent, "TokenFullBodyImage", new Vector2(250f, -340f), new Vector2(160f, 230f));

        Button pauseButton = CreateButton(parent, "PauseButton", "Пауза", new Vector2(-330f, -520f), new Vector2(160f, 48f));
        liveMatchController.PauseButton = pauseButton;
        UnityEventTools.AddPersistentListener(pauseButton.onClick, liveMatchController.TogglePause);
        Button speed1Button = CreateButton(parent, "Speed1Button", "x1", new Vector2(-170f, -550f), new Vector2(120f, 48f));
        liveMatchController.Speed1Button = speed1Button;
        UnityEventTools.AddPersistentListener(speed1Button.onClick, liveMatchController.SetSpeed1);
        speed1Button.gameObject.SetActive(false);
        Button speed2Button = CreateButton(parent, "Speed2Button", "x2", new Vector2(-160f, -520f), new Vector2(130f, 48f));
        liveMatchController.Speed2Button = speed2Button;
        UnityEventTools.AddPersistentListener(speed2Button.onClick, liveMatchController.SetSpeed2);
        Button speed4Button = CreateButton(parent, "Speed4Button", "x4", new Vector2(-20f, -520f), new Vector2(130f, 48f));
        liveMatchController.Speed4Button = speed4Button;
        UnityEventTools.AddPersistentListener(speed4Button.onClick, liveMatchController.SetSpeed4);
        Button skipPeriodButton = CreateButton(parent, "SkipPeriodButton", "До конца периода", new Vector2(185f, -520f), new Vector2(260f, 48f));
        UnityEventTools.AddPersistentListener(skipPeriodButton.onClick, liveMatchController.SkipPeriod);

        Button balancedButton = CreateButton(parent, "LiveBalancedButton", "Balanced", new Vector2(-315f, -610f), new Vector2(150f, 42f));
        UnityEventTools.AddPersistentListener(balancedButton.onClick, liveMatchController.SetBalancedTactic);
        balancedButton.gameObject.SetActive(false);
        Button offensiveButton = CreateButton(parent, "LiveOffensiveButton", "Offensive", new Vector2(-155f, -610f), new Vector2(150f, 42f));
        UnityEventTools.AddPersistentListener(offensiveButton.onClick, liveMatchController.SetOffensiveTactic);
        offensiveButton.gameObject.SetActive(false);
        Button defensiveButton = CreateButton(parent, "LiveDefensiveButton", "Defensive", new Vector2(5f, -610f), new Vector2(150f, 42f));
        UnityEventTools.AddPersistentListener(defensiveButton.onClick, liveMatchController.SetDefensiveTactic);
        defensiveButton.gameObject.SetActive(false);
        Button aggressiveButton = CreateButton(parent, "LiveAggressiveButton", "Aggressive", new Vector2(165f, -610f), new Vector2(150f, 42f));
        UnityEventTools.AddPersistentListener(aggressiveButton.onClick, liveMatchController.SetAggressiveTactic);
        aggressiveButton.gameObject.SetActive(false);
        Button changeGoalieButton = CreateButton(parent, "ChangeGoalieButton", "Вратарь", new Vector2(325f, -610f), new Vector2(150f, 42f));
        UnityEventTools.AddPersistentListener(changeGoalieButton.onClick, liveMatchController.ChangeUserGoalie);
        changeGoalieButton.gameObject.SetActive(false);

        Button pullGoalieButton = CreateButton(parent, "PullGoalieButton", "Снять G", new Vector2(-240f, -670f), new Vector2(180f, 42f));
        UnityEventTools.AddPersistentListener(pullGoalieButton.onClick, liveMatchController.PullUserGoalie);
        pullGoalieButton.gameObject.SetActive(false);
        Button returnGoalieButton = CreateButton(parent, "ReturnGoalieButton", "Вернуть G", new Vector2(-40f, -670f), new Vector2(180f, 42f));
        UnityEventTools.AddPersistentListener(returnGoalieButton.onClick, liveMatchController.ReturnUserGoalie);
        returnGoalieButton.gameObject.SetActive(false);
        Button skipMatchButton = CreateButton(parent, "SkipMatchButton", "Пропустить матч", new Vector2(-125f, -580f), new Vector2(260f, 48f));
        UnityEventTools.AddPersistentListener(skipMatchButton.onClick, liveMatchController.SkipMatch);
        Button finishButton = CreateButton(parent, "FinishLiveMatchButton", "Итог", new Vector2(165f, -580f), new Vector2(220f, 48f));
        UnityEventTools.AddPersistentListener(finishButton.onClick, controller.FinishLiveMatch);
        Button tacticToggleButton = CreateButton(parent, "LiveTacticsToggleButton", "Тактика", new Vector2(395f, -580f), new Vector2(170f, 48f));
        UnityEventTools.AddPersistentListener(tacticToggleButton.onClick, liveMatchController.ToggleTacticsMenu);
        Button exitButton = CreateButton(parent, "ExitLiveMatchButton", "Выйти и доиграть", new Vector2(0f, -735f), new Vector2(360f, 48f));
        UnityEventTools.AddPersistentListener(exitButton.onClick, controller.ExitLiveMatchAndFinish);
        exitButton.gameObject.SetActive(false);
    }

    private static void CreatePostGameSummaryPanel(Transform parent, GameScreenController controller, PostGameSummaryController postGameSummaryController)
    {
        postGameSummaryController.TitleText = CreateText(parent, "Title", "Итог матча", 32, new Vector2(0f, 675f), new Vector2(760f, 56f));
        postGameSummaryController.ScoreText = CreateText(parent, "ScoreText", "", 24, new Vector2(0f, 612f), new Vector2(820f, 64f));
        postGameSummaryController.ScoreText.resizeTextForBestFit = true;
        postGameSummaryController.ScoreText.resizeTextMinSize = 18;
        postGameSummaryController.ScoreText.resizeTextMaxSize = 28;
        postGameSummaryController.HomeLogoImage = CreateTeamLogoImage(parent, "HomeLogo", new Vector2(-360f, 600f), new Vector2(92f, 92f));
        postGameSummaryController.AwayLogoImage = CreateTeamLogoImage(parent, "AwayLogo", new Vector2(360f, 600f), new Vector2(92f, 92f));
        postGameSummaryController.SummaryText = CreateText(parent, "SummaryText", "", 18, new Vector2(0f, 470f), new Vector2(820f, 150f));
        postGameSummaryController.StarsText = CreateText(parent, "StarsText", "", 18, new Vector2(0f, 275f), new Vector2(820f, 135f));
        postGameSummaryController.EventsText = CreateText(parent, "EventsText", "", 13, new Vector2(0f, 45f), new Vector2(820f, 300f));
        postGameSummaryController.EventsText.alignment = TextAnchor.UpperLeft;

        Button dashboardButton = CreateButton(parent, "DashboardButton", "Вернуться в клуб", new Vector2(0f, -560f), new Vector2(360f, 60f));
        postGameSummaryController.DashboardButton = dashboardButton;
        UnityEventTools.AddPersistentListener(dashboardButton.onClick, controller.ShowDashboard);
    }

    private static LiveMatchPlayerTokenView CreateLiveMatchTokenTemplate(Transform parent)
    {
        GameObject tokenObject = new GameObject("LiveMatchTokenTemplate");
        tokenObject.transform.SetParent(parent, false);
        RectTransform rect = tokenObject.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(54f, 54f);

        Image jerseyImage = tokenObject.AddComponent<Image>();
        jerseyImage.color = Color.white;
        Button button = tokenObject.AddComponent<Button>();
        Text numberText = CreateText(tokenObject.transform, "NumberText", "0", 22, Vector2.zero, new Vector2(54f, 54f));
        numberText.color = Color.black;

        LiveMatchPlayerTokenView view = tokenObject.AddComponent<LiveMatchPlayerTokenView>();
        view.JerseyImage = jerseyImage;
        view.NumberText = numberText;
        view.Button = button;
        tokenObject.SetActive(false);
        return view;
    }

    private static GameObject CreateTutorialHintOverlay(
        Transform parent,
        GameScreenController controller,
        out TutorialHintView tutorialHintView)
    {
        GameObject hintObject = CreatePanel(parent, "TutorialHintOverlay", new Vector2(0f, -690f), new Vector2(860f, 170f));
        Text titleText = CreateText(hintObject.transform, "TitleText", "Подсказка", 18, new Vector2(-120f, 46f), new Vector2(580f, 30f));
        Text bodyText = CreateText(hintObject.transform, "BodyText", "", 14, new Vector2(-120f, 0f), new Vector2(580f, 78f));
        bodyText.alignment = TextAnchor.MiddleLeft;

        Button dismissButton = CreateButton(hintObject.transform, "DismissHintButton", "Скрыть", new Vector2(285f, 35f), new Vector2(170f, 42f));
        SetButtonFontSize(dismissButton, 14);
        UnityEventTools.AddPersistentListener(dismissButton.onClick, controller.DismissCurrentTutorialHint);

        Button helpButton = CreateButton(hintObject.transform, "HelpButton", "Помощь", new Vector2(285f, -35f), new Vector2(170f, 42f));
        SetButtonFontSize(helpButton, 14);
        UnityEventTools.AddPersistentListener(helpButton.onClick, controller.ShowTutorial);

        tutorialHintView = hintObject.AddComponent<TutorialHintView>();
        tutorialHintView.Configure(titleText, bodyText, dismissButton, helpButton);
        hintObject.SetActive(false);
        return hintObject;
    }

    private static void CreateTutorialPanel(
        Transform parent,
        GameScreenController controller,
        TutorialController tutorialController,
        out Text titleText,
        out Text summaryText,
        out Text checklistText,
        out Text hintText,
        out Button dismissHintButton,
        out Button disableButton,
        out Button resetButton)
    {
        titleText = CreateText(parent, "TitleText", "Обучение", 38, new Vector2(0f, 410f), new Vector2(760f, 58f));
        summaryText = CreateText(parent, "SummaryText", "Tutorial: 0/8 completed", 18, new Vector2(0f, 325f), new Vector2(800f, 92f));
        checklistText = CreateText(parent, "ChecklistText", "", 16, new Vector2(0f, 80f), new Vector2(800f, 360f));
        checklistText.alignment = TextAnchor.UpperLeft;
        hintText = CreateText(parent, "HintText", "", 16, new Vector2(0f, -175f), new Vector2(800f, 125f));
        hintText.alignment = TextAnchor.MiddleLeft;

        Button closeButton = CreateButton(parent, "CloseButton", "Закрыть", new Vector2(-300f, -390f), new Vector2(180f, 52f));
        UnityEventTools.AddPersistentListener(closeButton.onClick, controller.HideTutorial);

        dismissHintButton = CreateButton(parent, "DismissHintButton", "Скрыть подсказку", new Vector2(-95f, -390f), new Vector2(220f, 52f));
        SetButtonFontSize(dismissHintButton, 16);
        UnityEventTools.AddPersistentListener(dismissHintButton.onClick, controller.DismissCurrentTutorialHint);

        disableButton = CreateButton(parent, "DisableButton", "Выключить", new Vector2(140f, -390f), new Vector2(190f, 52f));
        SetButtonFontSize(disableButton, 16);
        UnityEventTools.AddPersistentListener(disableButton.onClick, controller.DisableTutorial);

        resetButton = CreateButton(parent, "ResetButton", "Сбросить", new Vector2(335f, -390f), new Vector2(170f, 52f));
        SetButtonFontSize(resetButton, 16);
        UnityEventTools.AddPersistentListener(resetButton.onClick, controller.ResetTutorial);

        tutorialController.Configure(
            titleText,
            summaryText,
            checklistText,
            hintText,
            dismissHintButton,
            disableButton,
            resetButton,
            closeButton);
    }

    private static GameObject CreateBusyOverlay(Transform parent, out Text busyText)
    {
        GameObject overlay = new GameObject("BusyOverlayPanel");
        overlay.transform.SetParent(parent, false);

        RectTransform rectTransform = overlay.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image background = overlay.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.62f);

        busyText = CreateText(overlay.transform, "BusyOverlayText", "Подождите...", 30, Vector2.zero, new Vector2(760f, 120f));
        busyText.alignment = TextAnchor.MiddleCenter;
        return overlay;
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(AndroidBuildConfig.PortraitReferenceWidth, AndroidBuildConfig.PortraitReferenceHeight);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        return canvas;
    }

    private static Camera CreateUiCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.02f, 0.03f, 0.045f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        return camera;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static void CreateTeamSelectBackground(Transform parent)
    {
        CreateStretchImage(parent, "TeamSelectIceBackground", new Color(0.025f, 0.045f, 0.065f, 1f), Vector2.zero, Vector2.zero);

        GameObject centerIce = CreatePanel(parent, "CenterIceBand", new Vector2(0f, 90f), new Vector2(960f, 360f));
        centerIce.GetComponent<Image>().color = new Color(0.62f, 0.82f, 0.95f, 0.08f);

        GameObject redLine = CreatePanel(parent, "CenterRedLine", new Vector2(0f, 90f), new Vector2(960f, 12f));
        redLine.GetComponent<Image>().color = new Color(0.95f, 0.16f, 0.18f, 0.42f);

        GameObject blueLineTop = CreatePanel(parent, "BlueLineTop", new Vector2(0f, 438f), new Vector2(960f, 10f));
        blueLineTop.GetComponent<Image>().color = new Color(0.15f, 0.42f, 0.95f, 0.35f);

        GameObject blueLineBottom = CreatePanel(parent, "BlueLineBottom", new Vector2(0f, -258f), new Vector2(960f, 10f));
        blueLineBottom.GetComponent<Image>().color = new Color(0.15f, 0.42f, 0.95f, 0.35f);

        GameObject diagonal = CreatePanel(parent, "DiagonalLightBand", new Vector2(230f, 0f), new Vector2(360f, 1900f));
        diagonal.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.045f);
        diagonal.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, -14f);

        GameObject sideShade = CreatePanel(parent, "RightSideShade", new Vector2(330f, 0f), new Vector2(380f, 1680f));
        sideShade.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.18f);
    }

    private static GameObject CreateTeamSelectLoadingPanel(
        Transform parent,
        out Slider loadingSlider,
        out Text titleText,
        out Text statusText,
        out Text percentText)
    {
        GameObject loadingPanel = CreatePanel(parent, "LoadingPanel", Vector2.zero, new Vector2(780f, 430f));
        loadingPanel.GetComponent<Image>().color = new Color(0.035f, 0.045f, 0.065f, 0.94f);

        titleText = CreateText(loadingPanel.transform, "LoadingTitleText", "Идёт создание игры", 34, new Vector2(0f, 130f), new Vector2(680f, 62f));
        statusText = CreateText(loadingPanel.transform, "LoadingStatusText", "Подготовка данных...", 20, new Vector2(0f, 55f), new Vector2(680f, 48f));
        loadingSlider = CreateLoadingSlider(loadingPanel.transform, "LoadingProgressSlider", new Vector2(0f, -35f), new Vector2(620f, 26f));
        percentText = CreateText(loadingPanel.transform, "LoadingPercentText", "0%", 24, new Vector2(0f, -100f), new Vector2(240f, 44f));
        CreateText(loadingPanel.transform, "LoadingHintText", "Пожалуйста, подождите. Команды, ассеты и лига готовятся заранее.", 15, new Vector2(0f, -162f), new Vector2(680f, 50f));

        return loadingPanel;
    }

    private static Slider CreateLoadingSlider(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject sliderObject = new GameObject(objectName);
        sliderObject.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.sizeDelta = size;
        sliderRect.anchoredPosition = anchoredPosition;

        Image background = sliderObject.AddComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.16f);

        GameObject fillAreaObject = new GameObject("Fill Area");
        fillAreaObject.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillAreaObject.AddComponent<RectTransform>();
        Stretch(fillAreaRect, new Vector2(4f, 4f), new Vector2(-4f, -4f));

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(fillAreaObject.transform, false);
        RectTransform fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = new Color(0.1f, 0.86f, 0.9f, 0.92f);

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        return slider;
    }

    private static Image CreateStretchImage(Transform parent, string objectName, Color color, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject imageObject = new GameObject(objectName);
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        Stretch(rectTransform, offsetMin, offsetMax);

        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static void StyleDarkButton(Button button, int fontSize)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.08f, 0.09f, 0.12f, 0.92f);
        }

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.08f, 0.09f, 0.12f, 0.92f);
        colors.highlightedColor = new Color(0.12f, 0.16f, 0.20f, 1f);
        colors.pressedColor = new Color(0.03f, 0.48f, 0.54f, 1f);
        colors.selectedColor = new Color(0.06f, 0.58f, 0.64f, 1f);
        button.colors = colors;

        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.color = Color.white;
            buttonText.fontSize = fontSize;
        }
    }

    private static void StylePrimaryButton(Button button, int fontSize)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.05f, 0.70f, 0.74f, 0.96f);
        }

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.05f, 0.70f, 0.74f, 0.96f);
        colors.highlightedColor = new Color(0.08f, 0.86f, 0.90f, 1f);
        colors.pressedColor = new Color(0.02f, 0.40f, 0.44f, 1f);
        colors.selectedColor = new Color(0.10f, 0.92f, 0.96f, 1f);
        button.colors = colors;

        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.color = Color.white;
            buttonText.fontSize = fontSize;
        }
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
        colors.pressedColor = new Color(0.05f, 0.70f, 0.74f, 1f);
        colors.selectedColor = new Color(0.08f, 0.86f, 0.90f, 1f);
        button.colors = colors;

        Text buttonText = CreateText(buttonObject.transform, "Text", label, 24, Vector2.zero, rectTransform.sizeDelta);
        buttonText.color = Color.black;

        return button;
    }

    private static Dropdown CreateDropdown(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject dropdownObject = new GameObject(objectName);
        dropdownObject.transform.SetParent(parent, false);

        RectTransform rectTransform = dropdownObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        Image image = dropdownObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Dropdown dropdown = dropdownObject.AddComponent<Dropdown>();
        dropdown.targetGraphic = image;

        Text labelText = CreateText(dropdownObject.transform, "Label", "Команда", 18, new Vector2(-12f, 0f), new Vector2(size.x - 64f, size.y));
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.black;

        Text arrowText = CreateText(dropdownObject.transform, "Arrow", "v", 18, new Vector2((size.x * 0.5f) - 24f, 0f), new Vector2(28f, size.y));
        arrowText.color = Color.black;

        RectTransform template = CreateDropdownTemplate(dropdownObject.transform, size);
        Text itemText = template.GetComponentInChildren<Toggle>(true).GetComponentInChildren<Text>(true);

        dropdown.template = template;
        dropdown.captionText = labelText;
        dropdown.itemText = itemText;
        dropdown.options.Clear();
        dropdown.options.Add(new Dropdown.OptionData("Команда"));
        dropdown.RefreshShownValue();

        return dropdown;
    }

    private static RectTransform CreateDropdownTemplate(Transform parent, Vector2 dropdownSize)
    {
        GameObject templateObject = new GameObject("Template");
        templateObject.transform.SetParent(parent, false);

        RectTransform templateRect = templateObject.AddComponent<RectTransform>();
        templateRect.sizeDelta = new Vector2(dropdownSize.x, 260f);
        templateRect.anchorMin = new Vector2(0f, 0f);
        templateRect.anchorMax = new Vector2(1f, 0f);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.anchoredPosition = new Vector2(0f, -dropdownSize.y * 0.5f);

        Image templateImage = templateObject.AddComponent<Image>();
        templateImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);

        ScrollRect scrollRect = templateObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        GameObject viewportObject = new GameObject("Viewport");
        viewportObject.transform.SetParent(templateObject.transform, false);
        RectTransform viewportRect = viewportObject.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = Color.white;
        Mask mask = viewportObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content");
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform contentRect = contentObject.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 260f);
        VerticalLayoutGroup layoutGroup = contentObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        ContentSizeFitter contentSizeFitter = contentObject.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Toggle itemToggle = CreateDropdownItem(contentObject.transform, dropdownSize.x);
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        templateObject.SetActive(false);
        return templateRect;
    }

    private static Toggle CreateDropdownItem(Transform parent, float width)
    {
        GameObject itemObject = new GameObject("Item");
        itemObject.transform.SetParent(parent, false);

        RectTransform itemRect = itemObject.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(width, 42f);
        LayoutElement layoutElement = itemObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 42f;
        layoutElement.minHeight = 42f;

        Image itemImage = itemObject.AddComponent<Image>();
        itemImage.color = new Color(0.92f, 0.94f, 0.98f, 1f);

        Toggle toggle = itemObject.AddComponent<Toggle>();
        toggle.targetGraphic = itemImage;

        GameObject checkmarkObject = new GameObject("Item Checkmark");
        checkmarkObject.transform.SetParent(itemObject.transform, false);
        RectTransform checkmarkRect = checkmarkObject.AddComponent<RectTransform>();
        checkmarkRect.sizeDelta = new Vector2(18f, 18f);
        checkmarkRect.anchoredPosition = new Vector2(-width * 0.5f + 18f, 0f);
        Image checkmarkImage = checkmarkObject.AddComponent<Image>();
        checkmarkImage.color = new Color(0.08f, 0.35f, 0.95f, 1f);
        toggle.graphic = checkmarkImage;

        Text itemText = CreateText(itemObject.transform, "Item Label", "Команда", 17, new Vector2(22f, 0f), new Vector2(width - 56f, 40f));
        itemText.alignment = TextAnchor.MiddleLeft;
        itemText.color = Color.black;

        return toggle;
    }

    private static Button CreateDashboardButton(Transform parent, string objectName, string label, ref float y)
    {
        Button button = CreateButton(parent, objectName, label, new Vector2(0f, y), new Vector2(560f, MobileUiConfig.ButtonHeight));
        SetButtonFontSize(button, MobileUiConfig.ButtonFontSize);

        y -= MobileUiConfig.ButtonHeight + MobileUiConfig.RowSpacing;
        return button;
    }

    private static Button CreateDashboardActionButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
    {
        return CreateDashboardActionButton(parent, objectName, label, anchoredPosition, new Vector2(260f, 50f));
    }

    private static Button CreateDashboardActionButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 size)
    {
        Button button = CreateButton(parent, objectName, label, anchoredPosition, size);
        SetButtonFontSize(button, Mathf.Max(16, MobileUiConfig.ButtonFontSize - 2));
        return button;
    }

    private static Transform CreateDashboardActionGroup(Transform parent, string objectName)
    {
        GameObject groupObject = new GameObject(objectName);
        groupObject.transform.SetParent(parent, false);

        RectTransform rectTransform = groupObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(860f, 620f);
        rectTransform.anchoredPosition = Vector2.zero;

        return groupObject.transform;
    }

    private static Button CreateNewsFilterButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
    {
        Button button = CreateButton(parent, objectName, label, anchoredPosition, new Vector2(200f, 42f));
        SetButtonFontSize(button, 14);
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
        return CreateVerticalScrollView(parent, "CalendarScrollView", new Vector2(840f, 980f), new Vector2(0f, -80f), 6f);
    }

    private static Transform CreateStandingsScrollView(Transform parent)
    {
        return CreateVerticalScrollView(parent, "StandingsScrollView", new Vector2(860f, 1000f), new Vector2(0f, -75f), 6f);
    }

    private static Transform CreatePlayerStatsScrollView(Transform parent)
    {
        return CreateVerticalScrollView(parent, "PlayerStatsScrollView", new Vector2(860f, 960f), new Vector2(0f, -120f), 6f);
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

    private static Image CreateTeamLogoImage(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject logoObject = new GameObject(name);
        logoObject.transform.SetParent(parent, false);

        RectTransform rectTransform = logoObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        Image image = logoObject.AddComponent<Image>();
        image.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);
        image.preserveAspect = true;
        return image;
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

        Image logoImage = CreateTeamLogoImage(buttonObject.transform, "LogoImage", new Vector2(-314f, 0f), new Vector2(54f, 54f));
        Text abbreviationText = CreateText(buttonObject.transform, "AbbreviationText", "MST", 22, new Vector2(-245f, 0f), new Vector2(80f, 52f));
        Text cityText = CreateText(buttonObject.transform, "CityText", "Moscow", 17, new Vector2(-15f, 17f), new Vector2(430f, 25f));
        Text nameText = CreateText(buttonObject.transform, "NameText", "Moscow Stars", 22, new Vector2(-15f, -7f), new Vector2(430f, 28f));
        Text divisionText = CreateText(buttonObject.transform, "DivisionText", "Western Conference / Capital Division", 12, new Vector2(80f, -29f), new Vector2(540f, 20f));

        abbreviationText.color = Color.black;
        cityText.color = Color.black;
        nameText.color = Color.black;
        divisionText.color = Color.black;

        TeamButtonView teamButtonView = buttonObject.AddComponent<TeamButtonView>();
        teamButtonView.Configure(button, logoImage, cityText, nameText, abbreviationText, divisionText);

        buttonObject.SetActive(false);
        return teamButtonView;
    }

    private static void CreateRosterHeader(Transform parent)
    {
        GameObject headerObject = new GameObject("RosterHeader");
        headerObject.transform.SetParent(parent, false);

        RectTransform rectTransform = headerObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(840f, 42f);
        rectTransform.anchoredPosition = new Vector2(0f, 590f);

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
        rectTransform.anchoredPosition = new Vector2(0f, 545f);

        CreateRosterHeaderText(headerObject.transform, "MatchupHeader", "Матчи лиги", Vector2.zero, new Vector2(800f, 40f));
    }

    private static void CreateStandingsHeader(Transform parent)
    {
        GameObject headerObject = new GameObject("StandingsHeader");
        headerObject.transform.SetParent(parent, false);

        RectTransform rectTransform = headerObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(840f, 42f);
        rectTransform.anchoredPosition = new Vector2(0f, 595f);

        CreateRosterHeaderText(headerObject.transform, "PlaceHeader", "#", new Vector2(-400f, 0f), new Vector2(35f, 40f));
        CreateRosterHeaderText(headerObject.transform, "TeamHeader", "Команда", new Vector2(-170f, 0f), new Vector2(390f, 40f));
        CreateRosterHeaderText(headerObject.transform, "GamesHeader", "И", new Vector2(60f, 0f), new Vector2(35f, 40f));
        CreateRosterHeaderText(headerObject.transform, "WinsHeader", "В", new Vector2(110f, 0f), new Vector2(35f, 40f));
        CreateRosterHeaderText(headerObject.transform, "LossesHeader", "П", new Vector2(160f, 0f), new Vector2(35f, 40f));
        CreateRosterHeaderText(headerObject.transform, "OvertimeLossesHeader", "ОТ", new Vector2(220f, 0f), new Vector2(45f, 40f));
        CreateRosterHeaderText(headerObject.transform, "PointsHeader", "О", new Vector2(280f, 0f), new Vector2(35f, 40f));
        CreateRosterHeaderText(headerObject.transform, "GoalsForHeader", "ЗШ", new Vector2(335f, 0f), new Vector2(45f, 40f));
        CreateRosterHeaderText(headerObject.transform, "GoalsAgainstHeader", "ПШ", new Vector2(390f, 0f), new Vector2(45f, 40f));
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
        rectTransform.sizeDelta = new Vector2(840f, 78f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 78f;
        layoutElement.minHeight = 78f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text nameText = CreatePlayerRowText(rowObject.transform, "NameText", "Player 1", new Vector2(-300f, 0f), new Vector2(230f, 66f));
        Text positionText = CreatePlayerRowText(rowObject.transform, "PositionText", "C", new Vector2(-160f, 0f), new Vector2(50f, 66f));
        Text ageText = CreatePlayerRowText(rowObject.transform, "AgeText", "19", new Vector2(-105f, 0f), new Vector2(60f, 66f));
        Text overallText = CreatePlayerRowText(rowObject.transform, "OverallText", "OVR 60 EFF 60", new Vector2(35f, 0f), new Vector2(210f, 66f));
        Text potentialText = CreatePlayerRowText(rowObject.transform, "PotentialText", "POT 70 | COND 100 | FAT 0", new Vector2(285f, 0f), new Vector2(280f, 66f));
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
        rectTransform.sizeDelta = new Vector2(840f, 84f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 84f;
        layoutElement.minHeight = 84f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player 1 | C | 19 лет | OVR 60 | $850 000 | 2 г. | Signed", new Vector2(-105f, 0f), new Vector2(600f, 72f));
        infoText.fontSize = 14;
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

    private static ExtensionCandidateRowView CreateExtensionCandidateRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ExtensionCandidateRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 78f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 78f;
        layoutElement.minHeight = 78f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player | EXT INT 70 | ask $5M x 4", Vector2.zero, new Vector2(360f, 68f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        ExtensionCandidateRowView rowView = rowObject.AddComponent<ExtensionCandidateRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static ExtensionOfferRowView CreateExtensionOfferRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ExtensionOfferRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 78f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 78f;
        layoutElement.minHeight = 78f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Offer history row", Vector2.zero, new Vector2(360f, 68f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        ExtensionOfferRowView rowView = rowObject.AddComponent<ExtensionOfferRowView>();
        rowView.Configure(infoText);

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

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Moscow Stars (MST) | Payroll $0 | Cap $104 000 000", Vector2.zero, new Vector2(360f, 50f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        TradeTeamRowView rowView = rowObject.AddComponent<TradeTeamRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static TradeBlockPlayerRowView CreateTradeBlockPlayerRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("TradeBlockPlayerRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 70f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 70f;
        layoutElement.minHeight = 70f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player 1 | C | 24 | OVR 76 / POT 82 | Avail 60", Vector2.zero, new Vector2(360f, 64f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        TradeBlockPlayerRowView rowView = rowObject.AddComponent<TradeBlockPlayerRowView>();
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
        rectTransform.sizeDelta = new Vector2(820f, 82f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 82f;
        layoutElement.minHeight = 82f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Free Agent 1 | C | 24 лет | OVR 70 | POT 80 | $1 000 000 | 1 г. | UFA", Vector2.zero, new Vector2(790f, 72f));
        infoText.fontSize = 12;
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

    private static FreeAgentOfferRowView CreateFreeAgentOfferRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("FreeAgentOfferRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 76f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 76f;
        layoutElement.minHeight = 76f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Free Agent | Team | $1 000 000 x 1 | score 60 | Accepted | User", Vector2.zero, new Vector2(790f, 54f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        FreeAgentOfferRowView rowView = rowObject.AddComponent<FreeAgentOfferRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static ProspectRowView CreateProspectRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ProspectRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 82f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 82f;
        layoutElement.minHeight = 82f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "#1 | Prospect 001 | C | Canada | 18 | OVR 70 | POT 90 | R1", Vector2.zero, new Vector2(360f, 74f));
        infoText.fontSize = 10;
        infoText.alignment = TextAnchor.MiddleLeft;

        ProspectRowView rowView = rowObject.AddComponent<ProspectRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static ScoutingProspectRowView CreateScoutingProspectRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ScoutingProspectRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 84f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 84f;
        layoutElement.minHeight = 84f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "#1 | Prospect 001 | C | OVR 58-66 | POT 78-88 | ACC 45%", Vector2.zero, new Vector2(360f, 76f));
        infoText.fontSize = 10;
        infoText.alignment = TextAnchor.MiddleLeft;

        ScoutingProspectRowView rowView = rowObject.AddComponent<ScoutingProspectRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static ScoutingReportRowView CreateScoutingReportRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("ScoutingReportRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380f, 70f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 70f;
        layoutElement.minHeight = 70f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Prospect 001 | ScoutPlayer | ACC 20 -> 45 | OVR 58-66 | POT 78-88", Vector2.zero, new Vector2(360f, 62f));
        infoText.fontSize = 10;
        infoText.alignment = TextAnchor.MiddleLeft;

        ScoutingReportRowView rowView = rowObject.AddComponent<ScoutingReportRowView>();
        rowView.Configure(infoText);

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

    private static GmJobOfferRowView CreateGmJobOfferRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("GmJobOfferRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(390f, 88f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 88f;
        layoutElement.minHeight = 88f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Team | Direction | Pts 0 | OVR 0", Vector2.zero, new Vector2(368f, 78f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        GmJobOfferRowView rowView = rowObject.AddComponent<GmJobOfferRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static GmCareerEventRowView CreateGmCareerEventRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("GmCareerEventRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(390f, 88f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 88f;
        layoutElement.minHeight = 88f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Season | Event | Summary", Vector2.zero, new Vector2(368f, 78f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        GmCareerEventRowView rowView = rowObject.AddComponent<GmCareerEventRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static OrganizationPlayerRowView CreateOrganizationPlayerRowTemplate(Transform parent, string objectName)
    {
        GameObject rowObject = new GameObject(objectName);
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(250f, 78f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 78f;
        layoutElement.minHeight = 78f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player | C | Pro | OVR 70 | $850 000", Vector2.zero, new Vector2(232f, 70f));
        infoText.fontSize = 10;
        infoText.alignment = TextAnchor.MiddleLeft;

        OrganizationPlayerRowView rowView = rowObject.AddComponent<OrganizationPlayerRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static WaiverRowView CreateWaiverRowTemplate(Transform parent, string objectName)
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

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player | C | OVR 74 | Age 25 | $1 000 000 | Team | Active", Vector2.zero, new Vector2(790f, 58f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        WaiverRowView rowView = rowObject.AddComponent<WaiverRowView>();
        rowView.Configure(infoText, button);

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

    private static AwardWinnerRowView CreateAwardWinnerRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("AwardWinnerRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(390f, 58f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 58f;
        layoutElement.minHeight = 58f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Award | Player | Team", Vector2.zero, new Vector2(365f, 52f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        AwardWinnerRowView rowView = rowObject.AddComponent<AwardWinnerRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static LeagueRecordRowView CreateLeagueRecordRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("LeagueRecordRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(390f, 58f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 58f;
        layoutElement.minHeight = 58f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Record | Player | Value", Vector2.zero, new Vector2(365f, 52f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        LeagueRecordRowView rowView = rowObject.AddComponent<LeagueRecordRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static UserTeamHistoryRowView CreateUserTeamHistoryRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("UserTeamHistoryRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(390f, 58f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 58f;
        layoutElement.minHeight = 58f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Season | User team | Result", Vector2.zero, new Vector2(365f, 52f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        UserTeamHistoryRowView rowView = rowObject.AddComponent<UserTeamHistoryRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static RetiredPlayerRowView CreateRetiredPlayerRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("RetiredPlayerRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(260f, 76f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 76f;
        layoutElement.minHeight = 76f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "#12 Player | HOF", Vector2.zero, new Vector2(246f, 68f));
        infoText.fontSize = 10;
        infoText.alignment = TextAnchor.MiddleLeft;

        RetiredPlayerRowView rowView = rowObject.AddComponent<RetiredPlayerRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static HallOfFameRowView CreateHallOfFameRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("HallOfFameRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(260f, 72f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 72f;
        layoutElement.minHeight = 72f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "2028 | #12 Player | Score 100", Vector2.zero, new Vector2(246f, 64f));
        infoText.fontSize = 10;
        infoText.alignment = TextAnchor.MiddleLeft;

        HallOfFameRowView rowView = rowObject.AddComponent<HallOfFameRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static RetiredNumberRowView CreateRetiredNumberRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("RetiredNumberRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(260f, 72f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 72f;
        layoutElement.minHeight = 72f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Team #12 | Player | 2028", Vector2.zero, new Vector2(246f, 64f));
        infoText.fontSize = 10;
        infoText.alignment = TextAnchor.MiddleLeft;

        RetiredNumberRowView rowView = rowObject.AddComponent<RetiredNumberRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static NewsItemRowView CreateNewsItemRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("NewsItemRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 122f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 122f;
        layoutElement.minHeight = 122f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "New | Date | Category | Title", Vector2.zero, new Vector2(790f, 112f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        NewsItemRowView rowView = rowObject.AddComponent<NewsItemRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static OwnerGoalRowView CreateOwnerGoalRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("OwnerGoalRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 60f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 60f;
        layoutElement.minHeight = 60f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Primary | Make Playoffs | 0 / Playoff berth | 0% | Active", Vector2.zero, new Vector2(790f, 54f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        OwnerGoalRowView rowView = rowObject.AddComponent<OwnerGoalRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static OwnerEvaluationRowView CreateOwnerEvaluationRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("OwnerEvaluationRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 68f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 68f;
        layoutElement.minHeight = 68f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "2026-27 | Trust 65 -> 70 (+5) | Satisfaction 70 | Safe", Vector2.zero, new Vector2(790f, 60f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        OwnerEvaluationRowView rowView = rowObject.AddComponent<OwnerEvaluationRowView>();
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

    private static MoralePlayerRowView CreateMoralePlayerRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("MoralePlayerRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 58f);

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

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player | MOR 70 | Content", Vector2.zero, new Vector2(790f, 52f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        MoralePlayerRowView rowView = rowObject.AddComponent<MoralePlayerRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static MoraleEventRowView CreateMoraleEventRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("MoraleEventRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 50f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 50f;
        layoutElement.minHeight = 50f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "Player | MoraleUpdate | 70 -> 65", Vector2.zero, new Vector2(790f, 46f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        MoraleEventRowView rowView = rowObject.AddComponent<MoraleEventRowView>();
        rowView.Configure(infoText);

        rowObject.SetActive(false);
        return rowView;
    }

    private static LeadershipCandidateRowView CreateLeadershipCandidateRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("LeadershipCandidateRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 56f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 56f;
        layoutElement.minHeight = 56f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button button = rowObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = new Color(0.82f, 0.88f, 1f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        button.colors = colors;

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "C | Player | LD 80 | Score 80", Vector2.zero, new Vector2(790f, 52f));
        infoText.fontSize = 11;
        infoText.alignment = TextAnchor.MiddleLeft;

        LeadershipCandidateRowView rowView = rowObject.AddComponent<LeadershipCandidateRowView>();
        rowView.Configure(infoText, button);

        rowObject.SetActive(false);
        return rowView;
    }

    private static StaffMemberRowView CreateStaffMemberRowTemplate(Transform parent)
    {
        GameObject rowObject = new GameObject("StaffMemberRowTemplate");
        rowObject.transform.SetParent(parent, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 76f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 76f;
        layoutElement.minHeight = 76f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text infoText = CreatePlayerRowText(rowObject.transform, "InfoText", "HeadCoach | Coach Name | Age 50 | Balanced | OVR 75 | Key ratings", Vector2.zero, new Vector2(790f, 68f));
        infoText.fontSize = 12;
        infoText.alignment = TextAnchor.MiddleLeft;

        StaffMemberRowView rowView = rowObject.AddComponent<StaffMemberRowView>();
        rowView.Configure(infoText);

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
        rectTransform.sizeDelta = new Vector2(840f, 104f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 104f;
        layoutElement.minHeight = 104f;

        Image image = rowObject.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Text placeText = CreatePlayerRowText(rowObject.transform, "PlaceText", "1", new Vector2(-400f, 0f), new Vector2(35f, 44f));
        Text teamNameText = CreatePlayerRowText(rowObject.transform, "TeamNameText", "Toronto Maple Leafs", new Vector2(-170f, 0f), new Vector2(390f, 44f));
        teamNameText.fontSize = 13;
        Text gamesPlayedText = CreatePlayerRowText(rowObject.transform, "GamesPlayedText", "0", new Vector2(60f, 0f), new Vector2(35f, 44f));
        Text winsText = CreatePlayerRowText(rowObject.transform, "WinsText", "0", new Vector2(110f, 0f), new Vector2(35f, 44f));
        Text lossesText = CreatePlayerRowText(rowObject.transform, "LossesText", "0", new Vector2(160f, 0f), new Vector2(35f, 44f));
        Text overtimeLossesText = CreatePlayerRowText(rowObject.transform, "OvertimeLossesText", "0", new Vector2(220f, 0f), new Vector2(45f, 44f));
        Text pointsText = CreatePlayerRowText(rowObject.transform, "PointsText", "0", new Vector2(280f, 0f), new Vector2(35f, 44f));
        Text goalsForText = CreatePlayerRowText(rowObject.transform, "GoalsForText", "0", new Vector2(335f, 0f), new Vector2(45f, 44f));
        Text goalsAgainstText = CreatePlayerRowText(rowObject.transform, "GoalsAgainstText", "0", new Vector2(390f, 0f), new Vector2(45f, 44f));

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

        Text text = CreatePlayerRowText(rowObject.transform, "Text", "Первый раунд | Team A 0 - 0 Team B\nИдёт\nG1: Team A 3 - 2 Team B", Vector2.zero, new Vector2(820f, 92f));
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleLeft;
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
