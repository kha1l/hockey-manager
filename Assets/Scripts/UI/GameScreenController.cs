using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScreenController : MonoBehaviour
{
    [SerializeField] private GameBootstrap _gameBootstrap;
    [SerializeField] private Text _teamText;
    [SerializeField] private Text _currentDayText;
    [SerializeField] private Text _gamesSimulatedText;
    [SerializeField] private Text _nextGameText;
    [SerializeField] private Text _lastMatchResultText;
    [SerializeField] private Text _seasonRulesText;
    [SerializeField] private Text _financeText;
    [SerializeField] private Text _leagueDateText;
    [SerializeField] private Text _tradeStatusText;
    [SerializeField] private Text _freeAgencyStatusText;
    [SerializeField] private Text _cpuRosterAiText;
    [SerializeField] private GameObject _dashboardPanel;
    [SerializeField] private GameObject _rosterPanel;
    [SerializeField] private GameObject _contractsPanel;
    [SerializeField] private GameObject _extensionsPanel;
    [SerializeField] private GameObject _tradesPanel;
    [SerializeField] private GameObject _scoutingPanel;
    [SerializeField] private GameObject _freeAgencyPanel;
    [SerializeField] private GameObject _draftPanel;
    [SerializeField] private GameObject _prospectRightsPanel;
    [SerializeField] private GameObject _organizationPanel;
    [SerializeField] private GameObject _waiversPanel;
    [SerializeField] private GameObject _offseasonPanel;
    [SerializeField] private GameObject _ownerPanel;
    [SerializeField] private GameObject _gmCareerPanel;
    [SerializeField] private GameObject _diagnosticsPanel;
    [SerializeField] private GameObject _historyPanel;
    [SerializeField] private GameObject _newsPanel;
    [SerializeField] private GameObject _developmentPanel;
    [SerializeField] private GameObject _lineupPanel;
    [SerializeField] private GameObject _rolesPanel;
    [SerializeField] private GameObject _moralePanel;
    [SerializeField] private GameObject _leadershipPanel;
    [SerializeField] private GameObject _staffPanel;
    [SerializeField] private GameObject _tacticsPanel;
    [SerializeField] private GameObject _injuriesPanel;
    [SerializeField] private GameObject _calendarPanel;
    [SerializeField] private GameObject _standingsPanel;
    [SerializeField] private GameObject _playerStatsPanel;
    [SerializeField] private GameObject _playoffsPanel;
    [SerializeField] private GameObject _preGamePanel;
    [SerializeField] private GameObject _liveMatchPanel;
    [SerializeField] private GameObject _postGameSummaryPanel;
    [SerializeField] private RosterController _rosterController;
    [SerializeField] private ContractsController _contractsController;
    [SerializeField] private ExtensionsController _extensionsController;
    [SerializeField] private TradesController _tradesController;
    [SerializeField] private ScoutingController _scoutingController;
    [SerializeField] private FreeAgencyController _freeAgencyController;
    [SerializeField] private DraftController _draftController;
    [SerializeField] private ProspectRightsController _prospectRightsController;
    [SerializeField] private OrganizationController _organizationController;
    [SerializeField] private WaiversController _waiversController;
    [SerializeField] private OffseasonController _offseasonController;
    [SerializeField] private OwnerController _ownerController;
    [SerializeField] private GmCareerController _gmCareerController;
    [SerializeField] private DiagnosticsController _diagnosticsController;
    [SerializeField] private HistoryController _historyController;
    [SerializeField] private NewsController _newsController;
    [SerializeField] private DevelopmentController _developmentController;
    [SerializeField] private LineupController _lineupController;
    [SerializeField] private RolesController _rolesController;
    [SerializeField] private MoraleController _moraleController;
    [SerializeField] private LeadershipController _leadershipController;
    [SerializeField] private StaffController _staffController;
    [SerializeField] private TacticsController _tacticsController;
    [SerializeField] private InjuriesController _injuriesController;
    [SerializeField] private CalendarController _calendarController;
    [SerializeField] private StandingsController _standingsController;
    [SerializeField] private PlayerStatsController _playerStatsController;
    [SerializeField] private PlayoffsController _playoffsController;
    [SerializeField] private PreGameController _preGameController;
    [SerializeField] private LiveMatchController _liveMatchController;
    [SerializeField] private PostGameSummaryController _postGameSummaryController;
    [SerializeField] private TutorialController _tutorialController;
    [SerializeField] private TutorialHintView _tutorialHintView;

    public GameObject TutorialPanel;
    public Text TutorialTitleText;
    public Text TutorialBodyText;
    public Text TutorialChecklistText;
    public Text TutorialHintText;
    public Button TutorialDismissHintButton;
    public Button TutorialDisableButton;
    public Button TutorialResetButton;
    public GameObject BusyOverlayPanel;
    public Text BusyOverlayText;
    public Image CurrentTeamLogoImage;
    public Text CurrentTeamIdentityText;
    public Dropdown PlayerStatsTeamDropdown;

    private Button _globalBackButton;
    private Button _globalAutoLineupButton;
    private Button _dashboardPlayoffsButton;
    private string _selectedUserTradePlayerId = "";
    private string _selectedUserTradePickId = "";
    private string _selectedOtherTradeTeamId = "";
    private string _selectedOtherTradePlayerId = "";
    private string _selectedOtherTradePickId = "";
    private string _selectedScoutingProspectId = "";
    private string _selectedFreeAgentId = "";
    private string _selectedFreeAgentPlayerId = "";
    private int _freeAgentOfferSalary;
    private int _freeAgentOfferYears;
    private string _selectedProspectId = "";
    private string _selectedProspectRightsId = "";
    private string _selectedOrganizationPlayerId = "";
    private string _selectedWaiverId = "";
    private string _selectedLineupSlotType = "";
    private int _selectedLineupLineOrPairNumber;
    private string _selectedLineupSlotPosition = "";
    private string _selectedLineupPlayerId = "";
    private string _selectedRolePlayerId = "";
    private string _selectedMoralePlayerId = "";
    private string _selectedLeadershipPlayerId = "";
    private string _selectedExtensionPlayerId = "";
    private string _selectedNewsFilter = NewsController.FilterAll;
    private string _selectedDashboardGroup = "Main";
    private string _selectedGmJobOfferId = "";
    private string _selectedStandingsMode = StandingsController.ModeDivisions;
    private string _selectedPlayerStatsMode = PlayerStatsController.ModeForwards;
    private string _selectedPlayerStatsTeamId = "";
    private int _selectedCalendarTargetDay = 1;
    private int _extensionOfferSalary;
    private int _extensionOfferYears;
    private string _currentTutorialHintId = "";
    private string _currentTutorialPanelId = TutorialConfig.PanelDashboard;
    private bool _isCompletingLiveMatchResult;
    private bool _returnToPreGameAfterTeamEdit;

    private void Start()
    {
        GameSession.EnsureCurrentTeam();
        EnsureGlobalBackButton();
        EnsureGlobalAutoLineupButton();
        EnsureDashboardPlayoffsButton();
        ShowDashboard();
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        BindCompactDashboardNavigation();
    }

    private void Update()
    {
        if (_liveMatchPanel != null
            && _liveMatchPanel.activeSelf
            && _liveMatchController != null
            && GameSession.IsLiveMatchActive())
        {
            _liveMatchController.TickUi(Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Escape) && ShouldShowGlobalBackButton())
        {
            ShowDashboard();
            return;
        }

        UpdateGlobalBackButtonVisibility();
        UpdateGlobalAutoLineupButtonVisibility();
        UpdateDashboardPlayoffsButtonVisibility();
    }

    private void BindCompactDashboardNavigation()
    {
        BindSceneButton("TopNavHomeButton", ShowDashboard);
        BindSceneButton("TopNavStandingsButton", ShowStandings);
        BindSceneButton("TopNavTeamButton", ShowOrganization);
        BindSceneButton("TopNavStatsButton", ShowPlayerStats);
        BindSceneButton("TopNavRosterButton", ShowLineup);
        BindSceneButton("CompactPlayoffsButton", ShowPlayoffs);
        BindSceneButton("PreGameLineupButton", ShowLineupFromPreGame);
        BindSceneButton("PreGameTacticsButton", ShowTacticsFromPreGame);
    }

    private static void BindSceneButton(string objectName, UnityAction action)
    {
        Button button = FindSceneButton(objectName);
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static Button FindSceneButton(string objectName)
    {
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button != null
                && button.gameObject.scene.IsValid()
                && button.gameObject.name == objectName)
            {
                return button;
            }
        }

        return null;
    }

    private static void BindPanelBackButton(GameObject panel, UnityAction action)
    {
        if (panel == null || action == null)
        {
            return;
        }

        Transform backTransform = panel.transform.Find("BackButton");
        Button button = backTransform == null ? null : backTransform.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static void MovePanelButton(GameObject panel, string objectName, Vector2 anchoredPosition)
    {
        Transform buttonTransform = panel == null ? null : panel.transform.Find(objectName);
        Button button = buttonTransform == null ? null : buttonTransform.GetComponent<Button>();
        RectTransform rect = button == null ? null : button.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = anchoredPosition;
        }
    }

    private void EnsureGlobalBackButton()
    {
        if (_globalBackButton != null)
        {
            return;
        }

        _globalBackButton = FindSceneButton("GlobalBackButton");
        if (_globalBackButton != null)
        {
            _globalBackButton.onClick.RemoveAllListeners();
            _globalBackButton.onClick.AddListener(ShowDashboard);
            return;
        }

        Canvas canvas = FindActiveSceneCanvas();
        if (canvas == null)
        {
            return;
        }

        GameObject buttonObject = new GameObject("GlobalBackButton");
        buttonObject.transform.SetParent(canvas.transform, false);
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(24f, -24f);
        rectTransform.sizeDelta = new Vector2(170f, 56f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.02f, 0.03f, 0.045f, 0.92f);

        _globalBackButton = buttonObject.AddComponent<Button>();
        _globalBackButton.onClick.AddListener(ShowDashboard);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObject.AddComponent<Text>();
        text.text = "Назад";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        buttonObject.transform.SetAsLastSibling();
        UpdateGlobalBackButtonVisibility();
    }

    private void EnsureGlobalAutoLineupButton()
    {
        if (_globalAutoLineupButton != null)
        {
            return;
        }

        _globalAutoLineupButton = FindSceneButton("GlobalAutoLineupButton");
        if (_globalAutoLineupButton != null)
        {
            _globalAutoLineupButton.onClick.RemoveAllListeners();
            _globalAutoLineupButton.onClick.AddListener(AutoFixRosterAndLineup);
            return;
        }

        Canvas canvas = FindActiveSceneCanvas();
        if (canvas == null)
        {
            return;
        }

        GameObject buttonObject = new GameObject("GlobalAutoLineupButton");
        buttonObject.transform.SetParent(canvas.transform, false);
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(-24f, -24f);
        rectTransform.sizeDelta = new Vector2(230f, 56f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.0f, 0.55f, 0.60f, 0.94f);

        _globalAutoLineupButton = buttonObject.AddComponent<Button>();
        _globalAutoLineupButton.onClick.AddListener(AutoFixRosterAndLineup);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObject.AddComponent<Text>();
        text.text = "Автозамена";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        buttonObject.transform.SetAsLastSibling();
        UpdateGlobalAutoLineupButtonVisibility();
    }

    private void EnsureDashboardPlayoffsButton()
    {
        if (_dashboardPlayoffsButton != null)
        {
            return;
        }

        _dashboardPlayoffsButton = FindSceneButton("CompactPlayoffsButton");
        if (_dashboardPlayoffsButton != null)
        {
            _dashboardPlayoffsButton.onClick.RemoveAllListeners();
            _dashboardPlayoffsButton.onClick.AddListener(ShowPlayoffs);
            return;
        }

        if (_dashboardPanel == null)
        {
            return;
        }

        GameObject buttonObject = new GameObject("CompactPlayoffsButton");
        buttonObject.transform.SetParent(_dashboardPanel.transform, false);
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, -525f);
        rectTransform.sizeDelta = new Vector2(300f, 52f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.02f, 0.025f, 0.035f, 0.96f);

        _dashboardPlayoffsButton = buttonObject.AddComponent<Button>();
        _dashboardPlayoffsButton.onClick.AddListener(ShowPlayoffs);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObject.AddComponent<Text>();
        text.text = "Плей-офф";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        UpdateDashboardPlayoffsButtonVisibility();
    }

    private static Canvas FindActiveSceneCanvas()
    {
        Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas != null && canvas.gameObject.scene.IsValid() && canvas.gameObject.activeInHierarchy)
            {
                return canvas;
            }
        }

        return null;
    }

    private void UpdateGlobalBackButtonVisibility()
    {
        if (_globalBackButton == null)
        {
            return;
        }

        _globalBackButton.gameObject.SetActive(ShouldShowGlobalBackButton());
        if (_globalBackButton.gameObject.activeSelf)
        {
            _globalBackButton.transform.SetAsLastSibling();
        }
    }

    private void UpdateGlobalAutoLineupButtonVisibility()
    {
        if (_globalAutoLineupButton == null)
        {
            return;
        }

        bool shouldShow = ((_lineupPanel != null && _lineupPanel.activeSelf)
                || (_organizationPanel != null && _organizationPanel.activeSelf))
            && (BusyOverlayPanel == null || !BusyOverlayPanel.activeSelf);
        _globalAutoLineupButton.gameObject.SetActive(shouldShow);
        if (_globalAutoLineupButton.gameObject.activeSelf)
        {
            _globalAutoLineupButton.transform.SetAsLastSibling();
        }
    }

    private void UpdateDashboardPlayoffsButtonVisibility()
    {
        if (_dashboardPlayoffsButton == null)
        {
            return;
        }

        bool shouldShow = _dashboardPanel != null
            && _dashboardPanel.activeSelf
            && IsPlayoffEntryAvailable()
            && (BusyOverlayPanel == null || !BusyOverlayPanel.activeSelf);
        _dashboardPlayoffsButton.gameObject.SetActive(shouldShow);
        if (_dashboardPlayoffsButton.gameObject.activeSelf)
        {
            _dashboardPlayoffsButton.transform.SetAsLastSibling();
        }
    }

    private static bool IsPlayoffEntryAvailable()
    {
        SeasonData season = GameSession.CurrentState == null ? null : GameSession.CurrentState.Season;
        return GameSession.CanStartPlayoffs()
            || (season != null && season.Playoffs != null && season.Playoffs.IsStarted);
    }

    private bool ShouldShowGlobalBackButton()
    {
        if (_globalBackButton == null)
        {
            return false;
        }

        if (BusyOverlayPanel != null && BusyOverlayPanel.activeSelf)
        {
            return false;
        }

        if (_liveMatchPanel != null && _liveMatchPanel.activeSelf && GameSession.IsLiveMatchActive())
        {
            return false;
        }

        return IsPanelActive(_rosterPanel)
            || IsPanelActive(_contractsPanel)
            || IsPanelActive(_extensionsPanel)
            || IsPanelActive(_tradesPanel)
            || IsPanelActive(_scoutingPanel)
            || IsPanelActive(_freeAgencyPanel)
            || IsPanelActive(_draftPanel)
            || IsPanelActive(_prospectRightsPanel)
            || IsPanelActive(_organizationPanel)
            || IsPanelActive(_waiversPanel)
            || IsPanelActive(_offseasonPanel)
            || IsPanelActive(_ownerPanel)
            || IsPanelActive(_gmCareerPanel)
            || IsPanelActive(_diagnosticsPanel)
            || IsPanelActive(_historyPanel)
            || IsPanelActive(_newsPanel)
            || IsPanelActive(_developmentPanel)
            || IsPanelActive(_lineupPanel)
            || IsPanelActive(_rolesPanel)
            || IsPanelActive(_moralePanel)
            || IsPanelActive(_leadershipPanel)
            || IsPanelActive(_staffPanel)
            || IsPanelActive(_tacticsPanel)
            || IsPanelActive(_injuriesPanel)
            || IsPanelActive(_calendarPanel)
            || IsPanelActive(_standingsPanel)
            || IsPanelActive(_playerStatsPanel)
            || IsPanelActive(_playoffsPanel)
            || IsPanelActive(_preGamePanel)
            || IsPanelActive(_postGameSummaryPanel);
    }

    private static bool IsPanelActive(GameObject panel)
    {
        return panel != null && panel.activeSelf;
    }

    private void OpenRosterFixScreen(string message)
    {
        if (NeedsOrganizationFix(message) && _organizationPanel != null)
        {
            ShowOrganization();
            return;
        }

        if (_lineupPanel != null)
        {
            ShowLineup();
            return;
        }

        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    private static bool NeedsOrganizationFix(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        return message.IndexOf("Pro", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("Farm", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("Reserve", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("12 F", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("6 D", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("2 G", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("доступных", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("фарм", StringComparison.OrdinalIgnoreCase) >= 0
            || message.IndexOf("резерв", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public void Configure(
        GameBootstrap gameBootstrap,
        Text teamText,
        Text currentDayText,
        Text gamesSimulatedText,
        Text nextGameText,
        Text lastMatchResultText,
        Text seasonRulesText,
        Text financeText,
        Text leagueDateText,
        Text tradeStatusText,
        Text freeAgencyStatusText,
        Text cpuRosterAiText,
        GameObject dashboardPanel,
        GameObject rosterPanel,
        GameObject lineupPanel,
        GameObject tacticsPanel,
        GameObject contractsPanel,
        GameObject extensionsPanel,
        GameObject tradesPanel,
        GameObject scoutingPanel,
        GameObject freeAgencyPanel,
        GameObject draftPanel,
        GameObject prospectRightsPanel,
        GameObject organizationPanel,
        GameObject waiversPanel,
        GameObject offseasonPanel,
        GameObject ownerPanel,
        GameObject gmCareerPanel,
        GameObject diagnosticsPanel,
        GameObject historyPanel,
        GameObject newsPanel,
        GameObject developmentPanel,
        GameObject rolesPanel,
        GameObject moralePanel,
        GameObject leadershipPanel,
        GameObject staffPanel,
        GameObject calendarPanel,
        GameObject injuriesPanel,
        GameObject standingsPanel,
        GameObject playerStatsPanel,
        GameObject playoffsPanel,
        RosterController rosterController,
        ContractsController contractsController,
        ExtensionsController extensionsController,
        TradesController tradesController,
        ScoutingController scoutingController,
        FreeAgencyController freeAgencyController,
        DraftController draftController,
        ProspectRightsController prospectRightsController,
        OrganizationController organizationController,
        WaiversController waiversController,
        OffseasonController offseasonController,
        OwnerController ownerController,
        GmCareerController gmCareerController,
        DiagnosticsController diagnosticsController,
        HistoryController historyController,
        NewsController newsController,
        DevelopmentController developmentController,
        RolesController rolesController,
        MoraleController moraleController,
        LeadershipController leadershipController,
        StaffController staffController,
        LineupController lineupController,
        TacticsController tacticsController,
        InjuriesController injuriesController,
        CalendarController calendarController,
        StandingsController standingsController,
        PlayerStatsController playerStatsController,
        PlayoffsController playoffsController)
    {
        _gameBootstrap = gameBootstrap;
        _teamText = teamText;
        _currentDayText = currentDayText;
        _gamesSimulatedText = gamesSimulatedText;
        _nextGameText = nextGameText;
        _lastMatchResultText = lastMatchResultText;
        _seasonRulesText = seasonRulesText;
        _financeText = financeText;
        _leagueDateText = leagueDateText;
        _tradeStatusText = tradeStatusText;
        _freeAgencyStatusText = freeAgencyStatusText;
        _cpuRosterAiText = cpuRosterAiText;
        _dashboardPanel = dashboardPanel;
        _rosterPanel = rosterPanel;
        _lineupPanel = lineupPanel;
        _tacticsPanel = tacticsPanel;
        _contractsPanel = contractsPanel;
        _extensionsPanel = extensionsPanel;
        _tradesPanel = tradesPanel;
        _scoutingPanel = scoutingPanel;
        _freeAgencyPanel = freeAgencyPanel;
        _draftPanel = draftPanel;
        _prospectRightsPanel = prospectRightsPanel;
        _organizationPanel = organizationPanel;
        _waiversPanel = waiversPanel;
        _offseasonPanel = offseasonPanel;
        _ownerPanel = ownerPanel;
        _gmCareerPanel = gmCareerPanel;
        _diagnosticsPanel = diagnosticsPanel;
        _historyPanel = historyPanel;
        _newsPanel = newsPanel;
        _developmentPanel = developmentPanel;
        _rolesPanel = rolesPanel;
        _moralePanel = moralePanel;
        _leadershipPanel = leadershipPanel;
        _staffPanel = staffPanel;
        _injuriesPanel = injuriesPanel;
        _calendarPanel = calendarPanel;
        _standingsPanel = standingsPanel;
        _playerStatsPanel = playerStatsPanel;
        _playoffsPanel = playoffsPanel;
        _rosterController = rosterController;
        _contractsController = contractsController;
        _extensionsController = extensionsController;
        _tradesController = tradesController;
        _scoutingController = scoutingController;
        _freeAgencyController = freeAgencyController;
        _draftController = draftController;
        _prospectRightsController = prospectRightsController;
        _organizationController = organizationController;
        _waiversController = waiversController;
        _offseasonController = offseasonController;
        _ownerController = ownerController;
        _gmCareerController = gmCareerController;
        _diagnosticsController = diagnosticsController;
        _historyController = historyController;
        _newsController = newsController;
        _developmentController = developmentController;
        _rolesController = rolesController;
        _moraleController = moraleController;
        _leadershipController = leadershipController;
        _staffController = staffController;
        _lineupController = lineupController;
        _tacticsController = tacticsController;
        _injuriesController = injuriesController;
        _calendarController = calendarController;
        _standingsController = standingsController;
        _playerStatsController = playerStatsController;
        _playoffsController = playoffsController;
    }

    public void ConfigureLiveMatch(
        GameObject preGamePanel,
        GameObject liveMatchPanel,
        GameObject postGameSummaryPanel,
        PreGameController preGameController,
        LiveMatchController liveMatchController,
        PostGameSummaryController postGameSummaryController)
    {
        _preGamePanel = preGamePanel;
        _liveMatchPanel = liveMatchPanel;
        _postGameSummaryPanel = postGameSummaryPanel;
        _preGameController = preGameController;
        _liveMatchController = liveMatchController;
        _postGameSummaryController = postGameSummaryController;
    }

    public void ConfigureTutorial(
        GameObject tutorialPanel,
        TutorialController tutorialController,
        TutorialHintView tutorialHintView,
        Text tutorialTitleText,
        Text tutorialBodyText,
        Text tutorialChecklistText,
        Text tutorialHintText,
        Button tutorialDismissHintButton,
        Button tutorialDisableButton,
        Button tutorialResetButton)
    {
        TutorialPanel = tutorialPanel;
        _tutorialController = tutorialController;
        _tutorialHintView = tutorialHintView;
        TutorialTitleText = tutorialTitleText;
        TutorialBodyText = tutorialBodyText;
        TutorialChecklistText = tutorialChecklistText;
        TutorialHintText = tutorialHintText;
        TutorialDismissHintButton = tutorialDismissHintButton;
        TutorialDisableButton = tutorialDisableButton;
        TutorialResetButton = tutorialResetButton;
    }

    public void ShowDashboard()
    {
        _returnToPreGameAfterTeamEdit = false;
        GameSession.EnsureCurrentTeam();
        HideAllPanels();
        _dashboardPanel.SetActive(true);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        OnTutorialPanelShown(TutorialConfig.PanelDashboard);
    }

    public void ShowTeamHub()
    {
        SelectDashboardTeam();
    }

    public void ShowSeasonHub()
    {
        SelectDashboardSeason();
    }

    public void ShowOfficeHub()
    {
        SelectDashboardOffice();
    }

    public void ShowMarketHub()
    {
        SelectDashboardMarket();
    }

    public void ShowHistoryHub()
    {
        SelectDashboardHistory();
    }

    public void SelectDashboardMain()
    {
        SelectDashboardGroup("Main");
    }

    public void SelectDashboardTeam()
    {
        SelectDashboardGroup("Team");
    }

    public void SelectDashboardSeason()
    {
        SelectDashboardGroup("Season");
    }

    public void SelectDashboardOffice()
    {
        SelectDashboardGroup("Office");
    }

    public void SelectDashboardMarket()
    {
        SelectDashboardGroup("Market");
    }

    public void SelectDashboardHistory()
    {
        SelectDashboardGroup("History");
    }

    private void SelectDashboardGroup(string group)
    {
        _selectedDashboardGroup = string.IsNullOrEmpty(group) ? "Main" : group;
        ShowDashboard();
    }

    public void ShowRoster()
    {
        GameSession.EnsureCurrentTeam();
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(true);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        if (GameSession.CurrentTeam == null)
        {
            Debug.LogWarning("Нельзя открыть состав: команда не выбрана");
            return;
        }

        MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
        OnTutorialPanelShown(TutorialConfig.PanelRoster);
    }

    public void ShowContracts()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(true);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        MeasurePanelRefresh("Contracts", RefreshContracts);
        OnTutorialPanelShown(TutorialConfig.PanelContracts);
    }

    public void ShowExtensions()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        SetPanelActive(_extensionsPanel, true);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureContractExtensions();
        MeasurePanelRefresh("Extensions", RefreshExtensions);
        OnTutorialPanelShown(TutorialConfig.PanelExtensions);
    }

    public void ShowTrades()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(true);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        GameSession.EnsureDraftPickOwnership();
        GameSession.EnsureTradeProfiles();
        MeasurePanelRefresh("Trades", RefreshTrades);
        OnTutorialPanelShown("Trades");
    }

    public void ShowFreeAgency()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(true);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        GameSession.EnsureFreeAgents();
        GameSession.EnsureBetterFreeAgency();
        MeasurePanelRefresh("FreeAgency", RefreshFreeAgency);
        OnTutorialPanelShown(TutorialConfig.PanelFreeAgency);
    }

    public void ShowDraft()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(true);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        GameSession.EnsureDraftPickOwnership();
        DraftService.EnsureDraft(GameSession.CurrentState);
        GameSession.EnsureScouting();
        MeasurePanelRefresh("Draft", RefreshDraft);
        OnTutorialPanelShown(TutorialConfig.PanelDraft);
    }

    public void ShowScouting()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        SetPanelActive(_scoutingPanel, true);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureScouting();
        MeasurePanelRefresh("Scouting", RefreshScouting);
        OnTutorialPanelShown(TutorialConfig.PanelScouting);
    }

    public void ShowProspectRights()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, true);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureProspectSigningHistory();
        MeasurePanelRefresh("ProspectRights", RefreshProspectRights);
        OnTutorialPanelShown("ProspectRights");
    }

    public void ShowOrganization()
    {
        GameSession.EnsureCurrentTeam();
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, true);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureRosterStatuses();
        MeasurePanelRefresh("Organization", RefreshOrganization);
        OnTutorialPanelShown("Organization");
    }

    public void ShowWaivers()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, true);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureWaivers();
        MeasurePanelRefresh("Waivers", RefreshWaivers);
        OnTutorialPanelShown("Waivers");
    }

    public void ShowOffseason()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, true);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureSeasonHistory();
        MeasurePanelRefresh("Offseason", RefreshOffseason);
        OnTutorialPanelShown("Offseason");
    }

    public void ShowOwner()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, true);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureOwnerGoals();
        MeasurePanelRefresh("Owner", RefreshOwner);
        OnTutorialPanelShown(TutorialConfig.PanelOwner);
    }

    public void ShowGmCareer()
    {
        HideAllPanels();
        SetPanelActive(_gmCareerPanel, true);
        GameSession.EnsureGmCareer();
        MeasurePanelRefresh("GmCareer", RefreshGmCareer);
        OnTutorialPanelShown(TutorialConfig.PanelGmCareer);
    }

    public void SelectGmJobOffer(string offerId)
    {
        _selectedGmJobOfferId = string.IsNullOrEmpty(offerId) ? "" : offerId;
        MeasurePanelRefresh("GmCareer", RefreshGmCareer);
    }

    public void AcceptSelectedGmJobOffer()
    {
        if (string.IsNullOrEmpty(_selectedGmJobOfferId))
        {
            Debug.LogWarning("Предложение работы не выбрано");
            return;
        }

        bool accepted = GameSession.AcceptGmJobOffer(_selectedGmJobOfferId, out string message);
        if (accepted)
        {
            Debug.Log(message);
            _selectedGmJobOfferId = "";
            ShowDashboard();
            MeasurePanelRefresh("Owner", RefreshOwner);
            if (_rosterController != null)
            {
                MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
            }
        }
        else
        {
            Debug.LogWarning(message);
            MeasurePanelRefresh("GmCareer", RefreshGmCareer);
        }
    }

    public void DeclineSelectedGmJobOffer()
    {
        if (string.IsNullOrEmpty(_selectedGmJobOfferId))
        {
            Debug.LogWarning("Предложение работы не выбрано");
            return;
        }

        bool declined = GameSession.DeclineGmJobOffer(_selectedGmJobOfferId, out string message);
        if (declined)
        {
            Debug.Log(message);
            _selectedGmJobOfferId = "";
        }
        else
        {
            Debug.LogWarning(message);
        }

        MeasurePanelRefresh("GmCareer", RefreshGmCareer);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void GenerateGmJobOffers()
    {
        List<GmJobOfferData> offers = GameSession.GenerateGmJobOffers();
        Debug.Log("GM job offers generated: " + (offers == null ? 0 : offers.Count));
        MeasurePanelRefresh("GmCareer", RefreshGmCareer);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void ShowDiagnostics()
    {
        HideAllPanels();
        SetPanelActive(_diagnosticsPanel, true);
        MeasurePanelRefresh("Diagnostics", RefreshDiagnostics);
        OnTutorialPanelShown(TutorialConfig.PanelDiagnostics);
    }

    public void RunDiagnosticsValidation()
    {
        GameStateValidationReportData report = GameSession.ValidateCurrentState();
        Debug.Log("Diagnostics validation: " + (report == null ? "no report" : report.Summary));
        MeasurePanelRefresh("Diagnostics", RefreshDiagnostics);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void RunDiagnosticsRepair()
    {
        RunWithBusy("Repair diagnostics...", () =>
        {
            GameStateValidationReportData report = GameSession.RepairCurrentStateSafeIssues();
            Debug.Log("Diagnostics repair: " + (report == null ? "no report" : report.Summary));
            MeasurePanelRefresh("Diagnostics", RefreshDiagnostics);
            MeasurePanelRefresh("Dashboard", RefreshDashboard);
        });
    }

    public void RunDiagnosticsBalanceReport()
    {
        BalanceReportData report = GameSession.GenerateBalanceReport();
        Debug.Log("Diagnostics balance: " + (report == null ? "no report" : report.Summary));
        MeasurePanelRefresh("Diagnostics", RefreshDiagnostics);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void RunAndroidReadinessCheck()
    {
        AndroidReadinessChecklistData checklist = GameSession.GenerateAndroidReadinessChecklist();
        Debug.Log(checklist == null ? "Android readiness: no checklist" : checklist.Summary);
        MeasurePanelRefresh("Diagnostics", RefreshDiagnostics);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void RunAlphaBalanceReport()
    {
        RunWithBusy("Alpha report...", () =>
        {
            AlphaBalanceReportData report = GameSession.GenerateAlphaBalanceReport();
            Debug.Log("Alpha balance: " + (report == null ? "no report" : report.Summary));
            MeasurePanelRefresh("Diagnostics", RefreshDiagnostics);
            MeasurePanelRefresh("Dashboard", RefreshDashboard);
        });
    }

    public void RunAlphaBalanceReport1Season()
    {
        RunWithBusy("Alpha report: 1 season...", () =>
        {
            LogAlphaMultiSeasonReport(GameSession.RunAlphaBalanceReportOneSeason());
            MeasurePanelRefresh("Diagnostics", RefreshDiagnostics);
            MeasurePanelRefresh("Dashboard", RefreshDashboard);
        });
    }

    public void RunAlphaBalanceReport3Seasons()
    {
        RunWithBusy("Alpha report: 3 seasons...", () =>
        {
            LogAlphaMultiSeasonReport(GameSession.RunAlphaBalanceReportThreeSeasons());
            MeasurePanelRefresh("Diagnostics", RefreshDiagnostics);
            MeasurePanelRefresh("Dashboard", RefreshDashboard);
        });
    }

    public void RunAlphaBalanceReport5Seasons()
    {
        RunWithBusy("Alpha report: 5 seasons...", () =>
        {
            LogAlphaMultiSeasonReport(GameSession.RunAlphaBalanceReportFiveSeasons());
            MeasurePanelRefresh("Diagnostics", RefreshDiagnostics);
            MeasurePanelRefresh("Dashboard", RefreshDashboard);
        });
    }

    public void RunDiagnosticsMigration()
    {
        MigrationReportData report = GameSession.RunSaveMigration();
        Debug.Log("Diagnostics migration: " + (report == null ? "no report" : report.Status));
        MeasurePanelRefresh("Diagnostics", RefreshDiagnostics);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void ShowHistory()
    {
        HideAllPanels();
        SetPanelActiveForHistory(_dashboardPanel, false);
        SetPanelActiveForHistory(_rosterPanel, false);
        SetPanelActiveForHistory(_contractsPanel, false);
        SetPanelActiveForHistory(_tradesPanel, false);
        SetPanelActiveForHistory(_freeAgencyPanel, false);
        SetPanelActiveForHistory(_draftPanel, false);
        SetPanelActiveForHistory(_prospectRightsPanel, false);
        SetPanelActiveForHistory(_scoutingPanel, false);
        SetPanelActiveForHistory(_organizationPanel, false);
        SetPanelActiveForHistory(_waiversPanel, false);
        SetPanelActiveForHistory(_offseasonPanel, false);
        SetPanelActiveForHistory(_ownerPanel, false);
        SetPanelActiveForHistory(_developmentPanel, false);
        SetPanelActiveForHistory(_lineupPanel, false);
        SetPanelActiveForHistory(_rolesPanel, false);
        SetPanelActiveForHistory(_tacticsPanel, false);
        SetPanelActiveForHistory(_injuriesPanel, false);
        SetPanelActiveForHistory(_moralePanel, false);
        SetPanelActiveForHistory(_leadershipPanel, false);
        SetPanelActiveForHistory(_staffPanel, false);
        SetPanelActiveForHistory(_extensionsPanel, false);
        SetPanelActiveForHistory(_calendarPanel, false);
        SetPanelActiveForHistory(_standingsPanel, false);
        SetPanelActiveForHistory(_playerStatsPanel, false);
        SetPanelActiveForHistory(_playoffsPanel, false);
        SetPanelActiveForHistory(_newsPanel, false);
        SetPanelActiveForHistory(_historyPanel, true);

        GameSession.EnsureLeagueHistory();
        GameSession.EnsureRetirementHistory();
        GameSession.GenerateSeasonRecapNewsIfNeeded();
        MeasurePanelRefresh("History", RefreshHistory);
        OnTutorialPanelShown(TutorialConfig.PanelHistory);
    }

    public void ShowNews()
    {
        HideAllPanels();
        SetPanelActiveForHistory(_dashboardPanel, false);
        SetPanelActiveForHistory(_rosterPanel, false);
        SetPanelActiveForHistory(_contractsPanel, false);
        SetPanelActiveForHistory(_tradesPanel, false);
        SetPanelActiveForHistory(_freeAgencyPanel, false);
        SetPanelActiveForHistory(_draftPanel, false);
        SetPanelActiveForHistory(_prospectRightsPanel, false);
        SetPanelActiveForHistory(_scoutingPanel, false);
        SetPanelActiveForHistory(_organizationPanel, false);
        SetPanelActiveForHistory(_waiversPanel, false);
        SetPanelActiveForHistory(_offseasonPanel, false);
        SetPanelActiveForHistory(_ownerPanel, false);
        SetPanelActiveForHistory(_developmentPanel, false);
        SetPanelActiveForHistory(_lineupPanel, false);
        SetPanelActiveForHistory(_rolesPanel, false);
        SetPanelActiveForHistory(_tacticsPanel, false);
        SetPanelActiveForHistory(_injuriesPanel, false);
        SetPanelActiveForHistory(_moralePanel, false);
        SetPanelActiveForHistory(_leadershipPanel, false);
        SetPanelActiveForHistory(_staffPanel, false);
        SetPanelActiveForHistory(_extensionsPanel, false);
        SetPanelActiveForHistory(_calendarPanel, false);
        SetPanelActiveForHistory(_standingsPanel, false);
        SetPanelActiveForHistory(_playerStatsPanel, false);
        SetPanelActiveForHistory(_playoffsPanel, false);
        SetPanelActiveForHistory(_historyPanel, false);
        SetPanelActiveForHistory(_newsPanel, true);

        GameSession.EnsureNewsFeed();
        MeasurePanelRefresh("News", RefreshNews);
        OnTutorialPanelShown(TutorialConfig.PanelNews);
    }

    public void ShowAllNews()
    {
        _selectedNewsFilter = NewsController.FilterAll;
        ShowNews();
    }

    public void ShowUserTeamNews()
    {
        _selectedNewsFilter = NewsController.FilterUserTeam;
        ShowNews();
    }

    public void ShowSeasonRecapNews()
    {
        _selectedNewsFilter = NewsConfig.CategorySeasonRecap;
        ShowNews();
    }

    public void ShowAwardNews()
    {
        _selectedNewsFilter = NewsConfig.CategoryAward;
        ShowNews();
    }

    public void ShowRecordNews()
    {
        _selectedNewsFilter = NewsConfig.CategoryRecord;
        ShowNews();
    }

    public void ShowTradeNews()
    {
        _selectedNewsFilter = NewsConfig.CategoryTrade;
        ShowNews();
    }

    public void ShowInjuryNews()
    {
        _selectedNewsFilter = NewsConfig.CategoryInjury;
        ShowNews();
    }

    public void ShowContractNews()
    {
        _selectedNewsFilter = NewsConfig.CategoryContract;
        ShowNews();
    }

    public void ShowFreeAgencyNews()
    {
        _selectedNewsFilter = NewsConfig.CategoryFreeAgency;
        ShowNews();
    }

    public void ShowDraftNews()
    {
        _selectedNewsFilter = NewsConfig.CategoryDraft;
        ShowNews();
    }

    public void ShowOwnerNews()
    {
        _selectedNewsFilter = NewsConfig.CategoryOwner;
        ShowNews();
    }

    public void ShowDevelopmentNews()
    {
        _selectedNewsFilter = NewsConfig.CategoryDevelopment;
        ShowNews();
    }

    public void MarkNewsAsRead(string newsId)
    {
        GameSession.MarkNewsAsRead(newsId);
        MeasurePanelRefresh("News", RefreshNews);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void ShowDevelopment()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, true);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureDevelopmentHistory();
        MeasurePanelRefresh("Development", RefreshDevelopment);
        OnTutorialPanelShown("Development");
    }

    public void ShowLineup()
    {
        GameSession.EnsureCurrentTeam();
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, true);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureLineups();
        MeasurePanelRefresh("Lineup", RefreshLineup);
        BindPanelBackButton(_lineupPanel, BackFromTeamEdit);
        MovePanelButton(_lineupPanel, "AssignPlayerButton", new Vector2(-330f, -660f));
        MovePanelButton(_lineupPanel, "SwapGoaliesButton", new Vector2(-110f, -660f));
        MovePanelButton(_lineupPanel, "ClearSelectionButton", new Vector2(110f, -660f));
        MovePanelButton(_lineupPanel, "AutoBuildLineupButton", new Vector2(330f, -660f));
        MovePanelButton(_lineupPanel, "BackButton", new Vector2(0f, -720f));
        OnTutorialPanelShown(TutorialConfig.PanelLineup);
    }

    public void ShowLineupFromPreGame()
    {
        _returnToPreGameAfterTeamEdit = true;
        ShowLineup();
    }

    public void ShowRoles()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, true);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureRolesAndUsage();
        MeasurePanelRefresh("Roles", RefreshRoles);
        OnTutorialPanelShown("Roles");
    }

    public void SelectRolePlayer(string playerId)
    {
        _selectedRolePlayerId = playerId;
        MeasurePanelRefresh("Roles", RefreshRoles);
    }

    public void ShowMorale()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_moralePanel, true);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureMorale();
        MeasurePanelRefresh("Morale", RefreshMorale);
        OnTutorialPanelShown("Morale");
    }

    public void SelectMoralePlayer(string playerId)
    {
        _selectedMoralePlayerId = playerId;
        MeasurePanelRefresh("Morale", RefreshMorale);
    }

    public void ShowLeadership()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, true);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureLeadership();
        MeasurePanelRefresh("Leadership", RefreshLeadership);
        OnTutorialPanelShown("Leadership");
    }

    public void SelectLeadershipPlayer(string playerId)
    {
        _selectedLeadershipPlayerId = playerId;
        MeasurePanelRefresh("Leadership", RefreshLeadership);
    }

    public void ShowStaff()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, true);
        SetPanelActive(_extensionsPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureCoachingStaff();
        MeasurePanelRefresh("Staff", RefreshStaff);
        OnTutorialPanelShown("Staff");
    }

    public void SelectExtensionPlayer(string playerId)
    {
        _selectedExtensionPlayerId = playerId;
        ContractExtensionCandidateData candidate = FindExtensionCandidate(playerId);
        if (candidate != null)
        {
            _extensionOfferSalary = candidate.ExpectedSalary;
            _extensionOfferYears = candidate.ExpectedYears;
        }

        MeasurePanelRefresh("Extensions", RefreshExtensions);
    }

    public void SetExtensionOfferSalary(string salaryText)
    {
        if (!TryParseSalary(salaryText, out int salary))
        {
            Debug.LogWarning("Не удалось прочитать зарплату: " + salaryText);
            return;
        }

        _extensionOfferSalary = ContractExtensionConfig.ClampSalary(salary, GameSession.CurrentState == null ? null : GameSession.CurrentState.LeagueRules);
        MeasurePanelRefresh("Extensions", RefreshExtensions);
    }

    public void SetExtensionOfferYears(string yearsText)
    {
        if (!int.TryParse(yearsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int years))
        {
            Debug.LogWarning("Не удалось прочитать срок: " + yearsText);
            return;
        }

        _extensionOfferYears = ContractExtensionConfig.ClampOwnTeamYears(years, GameSession.CurrentState == null ? null : GameSession.CurrentState.LeagueRules);
        MeasurePanelRefresh("Extensions", RefreshExtensions);
    }

    public void OfferSelectedPlayerExtension()
    {
        ContractExtensionCandidateData candidate = FindExtensionCandidate(_selectedExtensionPlayerId);
        if (candidate == null)
        {
            Debug.LogWarning("Игрок для продления не выбран");
            return;
        }

        int salary = _extensionOfferSalary > 0 ? _extensionOfferSalary : candidate.ExpectedSalary;
        int years = _extensionOfferYears > 0 ? _extensionOfferYears : candidate.ExpectedYears;
        MakeExtensionOffer(candidate.PlayerId, salary, years);
    }

    public void OfferSelectedPlayerExpectedExtension()
    {
        ContractExtensionCandidateData candidate = FindExtensionCandidate(_selectedExtensionPlayerId);
        if (candidate == null)
        {
            Debug.LogWarning("Игрок для продления не выбран");
            return;
        }

        MakeExtensionOffer(candidate.PlayerId, candidate.ExpectedSalary, candidate.ExpectedYears);
    }

    public void OfferSelectedPlayerMinimumExtension()
    {
        ContractExtensionCandidateData candidate = FindExtensionCandidate(_selectedExtensionPlayerId);
        if (candidate == null)
        {
            Debug.LogWarning("Игрок для продления не выбран");
            return;
        }

        MakeExtensionOffer(candidate.PlayerId, candidate.MinimumSalary, candidate.ExpectedYears);
    }

    public void OfferSelectedPlayerExpectedPlusTenExtension()
    {
        ContractExtensionCandidateData candidate = FindExtensionCandidate(_selectedExtensionPlayerId);
        if (candidate == null)
        {
            Debug.LogWarning("Игрок для продления не выбран");
            return;
        }

        int salary = candidate.ExpectedSalary + candidate.ExpectedSalary / 10;
        salary = ContractExtensionConfig.ClampSalary(salary, GameSession.CurrentState == null ? null : GameSession.CurrentState.LeagueRules);
        MakeExtensionOffer(candidate.PlayerId, salary, candidate.ExpectedYears);
    }

    public void OfferSelectedPlayerOneYearExtension()
    {
        ContractExtensionCandidateData candidate = FindExtensionCandidate(_selectedExtensionPlayerId);
        if (candidate == null)
        {
            Debug.LogWarning("Игрок для продления не выбран");
            return;
        }

        MakeExtensionOffer(candidate.PlayerId, candidate.ExpectedSalary, 1);
    }

    public void OfferSelectedPlayerLowballExtension()
    {
        ContractExtensionCandidateData candidate = FindExtensionCandidate(_selectedExtensionPlayerId);
        if (candidate == null)
        {
            Debug.LogWarning("Игрок для продления не выбран");
            return;
        }

        int salary = candidate.MinimumSalary * 70 / 100;
        MakeExtensionOffer(candidate.PlayerId, salary, candidate.ExpectedYears);
    }

    public void AutoAssignCaptains()
    {
        CaptaincyActionResultData result = GameSession.AutoAssignCurrentTeamCaptains();
        LogCaptaincyResult(result);
        RefreshLeadershipRelatedPanels();
    }

    public void AssignSelectedPlayerAsCaptain()
    {
        if (string.IsNullOrEmpty(_selectedLeadershipPlayerId))
        {
            Debug.LogWarning("Игрок не выбран");
            return;
        }

        CaptaincyActionResultData result = GameSession.AssignCurrentTeamCaptain(_selectedLeadershipPlayerId);
        LogCaptaincyResult(result);
        RefreshLeadershipRelatedPanels();
    }

    public void AssignSelectedPlayerAsAlternate()
    {
        if (string.IsNullOrEmpty(_selectedLeadershipPlayerId))
        {
            Debug.LogWarning("Игрок не выбран");
            return;
        }

        CaptaincyActionResultData result = GameSession.AssignCurrentTeamAlternateCaptain(_selectedLeadershipPlayerId);
        LogCaptaincyResult(result);
        RefreshLeadershipRelatedPanels();
    }

    public void ClearSelectedPlayerCaptaincy()
    {
        if (string.IsNullOrEmpty(_selectedLeadershipPlayerId))
        {
            Debug.LogWarning("Игрок не выбран");
            return;
        }

        CaptaincyActionResultData result = GameSession.ClearCurrentTeamCaptaincy(_selectedLeadershipPlayerId);
        LogCaptaincyResult(result);
        RefreshLeadershipRelatedPanels();
    }

    public void SetSelectedPlayerRoleSniper()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.Sniper);
    }

    public void SetSelectedPlayerRolePlaymaker()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.Playmaker);
    }

    public void SetSelectedPlayerRolePowerForward()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.PowerForward);
    }

    public void SetSelectedPlayerRoleTwoWayForward()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.TwoWayForward);
    }

    public void SetSelectedPlayerRoleGrinder()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.Grinder);
    }

    public void SetSelectedPlayerRoleDepthForward()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.DepthForward);
    }

    public void SetSelectedPlayerRoleOffensiveDefenseman()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.OffensiveDefenseman);
    }

    public void SetSelectedPlayerRoleDefensiveDefenseman()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.DefensiveDefenseman);
    }

    public void SetSelectedPlayerRoleTwoWayDefenseman()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.TwoWayDefenseman);
    }

    public void SetSelectedPlayerRoleStayAtHomeDefenseman()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.StayAtHomeDefenseman);
    }

    public void SetSelectedPlayerRoleStarterGoalie()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.StarterGoalie);
    }

    public void SetSelectedPlayerRoleBackupGoalie()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.BackupGoalie);
    }

    public void SetSelectedPlayerRoleDepthGoalie()
    {
        SetSelectedPlayerRole(PlayerRoleConfig.DepthGoalie);
    }

    public void AutoBuildLineup()
    {
        GameSession.EnsureCurrentTeam();
        GameSession.RebuildCurrentTeamLineup();
        ClearLineupSelectionFields();
        MeasurePanelRefresh("Lineup", RefreshLineup);
        MeasurePanelRefresh("Tactics", RefreshTactics);
        MeasurePanelRefresh("Injuries", RefreshInjuries);
        MeasurePanelRefresh("Organization", RefreshOrganization);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        Debug.Log("Автосостав создан");
    }

    public void AutoFixRosterAndLineup()
    {
        bool success = GameSession.AutoFixCurrentTeamRosterAndLineup(out string message);
        ClearLineupSelectionFields();
        if (success)
        {
            Debug.Log(message);
        }
        else
        {
            Debug.LogWarning(message);
        }

        MeasurePanelRefresh("Organization", RefreshOrganization);
        MeasurePanelRefresh("Lineup", RefreshLineup);
        MeasurePanelRefresh("Tactics", RefreshTactics);
        MeasurePanelRefresh("Injuries", RefreshInjuries);
        if (_rosterController != null)
        {
            MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
        }

        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void SelectLineupSlot(string slotType, int lineOrPairNumber, string slotPosition)
    {
        _selectedLineupSlotType = slotType;
        _selectedLineupLineOrPairNumber = lineOrPairNumber;
        _selectedLineupSlotPosition = slotPosition;
        _selectedLineupPlayerId = "";
        MeasurePanelRefresh("Lineup", RefreshLineup);
    }

    public void SelectLineupPlayer(string playerId)
    {
        _selectedLineupPlayerId = playerId;
        MeasurePanelRefresh("Lineup", RefreshLineup);
    }

    public void AssignSelectedPlayerToSelectedSlot()
    {
        if (string.IsNullOrEmpty(_selectedLineupSlotType))
        {
            Debug.LogWarning("Слот не выбран");
            return;
        }

        if (string.IsNullOrEmpty(_selectedLineupPlayerId))
        {
            Debug.LogWarning("Игрок не выбран");
            return;
        }

        bool success = GameSession.AssignCurrentTeamPlayerToLineupSlot(
            _selectedLineupSlotType,
            _selectedLineupLineOrPairNumber,
            _selectedLineupSlotPosition,
            _selectedLineupPlayerId,
            out string message);

        if (success)
        {
            Debug.Log(message);
            ClearLineupSelectionFields();
        }
        else
        {
            Debug.LogWarning(message);
        }

        MeasurePanelRefresh("Lineup", RefreshLineup);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        if (_rosterController != null)
        {
            MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
        }

        MeasurePanelRefresh("Tactics", RefreshTactics);
    }

    public void SwapGoalies()
    {
        bool success = GameSession.SwapCurrentTeamGoalies(out string message);
        if (success)
        {
            Debug.Log(message);
        }
        else
        {
            Debug.LogWarning(message);
        }

        MeasurePanelRefresh("Lineup", RefreshLineup);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void ClearLineupSelection()
    {
        ClearLineupSelectionFields();
        MeasurePanelRefresh("Lineup", RefreshLineup);
    }

    public void ShowTactics()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, true);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureSpecialTeamsAndTactics();
        MeasurePanelRefresh("Tactics", RefreshTactics);
        BindPanelBackButton(_tacticsPanel, BackFromTeamEdit);
        OnTutorialPanelShown("Tactics");
    }

    public void ShowTacticsFromPreGame()
    {
        _returnToPreGameAfterTeamEdit = true;
        ShowTactics();
    }

    public void BackFromTeamEdit()
    {
        if (_returnToPreGameAfterTeamEdit && _preGameController != null)
        {
            ShowPreparedPreGame();
            return;
        }

        ShowDashboard();
    }

    private void ShowPreparedPreGame()
    {
        bool prepared = GameSession.PrepareNextUserMatch(out PreGameSetupData setup, out string message);
        if (setup == null)
        {
            _returnToPreGameAfterTeamEdit = false;
            ShowDashboard();
            if (!string.IsNullOrEmpty(message))
            {
                Debug.LogWarning(message);
            }
            return;
        }

        HideAllPanels();
        SetPanelActive(_preGamePanel, true);
        _preGameController.ShowPreGame(setup);
        if (!prepared && !string.IsNullOrEmpty(message))
        {
            Debug.LogWarning(message);
        }
    }

    public void SetBalancedTactics()
    {
        SetTacticsPreset("Balanced");
    }

    public void SetOffensiveTactics()
    {
        SetTacticsPreset("Offensive");
    }

    public void SetDefensiveTactics()
    {
        SetTacticsPreset("Defensive");
    }

    public void SetAggressiveTactics()
    {
        SetTacticsPreset("Aggressive");
    }

    public void AutoBuildSpecialTeams()
    {
        GameSession.RebuildCurrentTeamSpecialTeams();
        MeasurePanelRefresh("Tactics", RefreshTactics);
        MeasurePanelRefresh("Injuries", RefreshInjuries);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        Debug.Log("Автоспецбригады созданы");
    }

    public void ShowInjuries()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, true);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureInjuries();
        MeasurePanelRefresh("Injuries", RefreshInjuries);
        OnTutorialPanelShown("Injuries");
    }

    public void StartNextSeason()
    {
        bool started = GameSession.StartNextSeason(out string message);
        if (started)
        {
            Debug.Log(message);
            MeasurePanelRefresh("Dashboard", RefreshDashboard);
            if (_rosterController != null)
            {
                MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
            }

            MeasurePanelRefresh("Contracts", RefreshContracts);
            MeasurePanelRefresh("Extensions", RefreshExtensions);
            MeasurePanelRefresh("Development", RefreshDevelopment);
            MeasurePanelRefresh("Lineup", RefreshLineup);
            MeasurePanelRefresh("Tactics", RefreshTactics);
            MeasurePanelRefresh("Injuries", RefreshInjuries);
            ShowDashboard();
            return;
        }

        Debug.LogWarning(message);
        MeasurePanelRefresh("Offseason", RefreshOffseason);
    }

    public void SelectProspectRights(string prospectId)
    {
        _selectedProspectRightsId = prospectId;
        MeasurePanelRefresh("ProspectRights", RefreshProspectRights);
    }

    public void SignSelectedProspectToElc()
    {
        if (string.IsNullOrEmpty(_selectedProspectRightsId))
        {
            Debug.LogWarning("Проспект не выбран");
            return;
        }

        bool accepted = GameSession.TrySignProspectToElc(
            _selectedProspectRightsId,
            out ProspectSigningData signing,
            out string message);

        if (accepted)
        {
            Debug.Log(message);
            _selectedProspectRightsId = "";
        }
        else
        {
            Debug.LogWarning(message);
        }

        MeasurePanelRefresh("ProspectRights", RefreshProspectRights);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        if (_rosterController != null)
        {
            MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
        }

        MeasurePanelRefresh("Contracts", RefreshContracts);
        MeasurePanelRefresh("Trades", RefreshTrades);
        MeasurePanelRefresh("Lineup", RefreshLineup);
        MeasurePanelRefresh("Tactics", RefreshTactics);
        MeasurePanelRefresh("Injuries", RefreshInjuries);
        MeasurePanelRefresh("Organization", RefreshOrganization);
    }

    public void SelectOrganizationPlayer(string playerId)
    {
        _selectedOrganizationPlayerId = playerId;
        MeasurePanelRefresh("Organization", RefreshOrganization);
    }

    public void SendSelectedPlayerToFarm()
    {
        MoveSelectedOrganizationPlayer(GameSession.SendCurrentTeamPlayerToFarm);
    }

    public void CallUpSelectedPlayerToNhl()
    {
        MoveSelectedOrganizationPlayer(GameSession.CallUpCurrentTeamPlayerToNhl);
    }

    public void MoveSelectedPlayerToReserve()
    {
        MoveSelectedOrganizationPlayer(GameSession.MoveCurrentTeamPlayerToReserve);
    }

    public void MoveSelectedReservePlayerToNhl()
    {
        MoveSelectedOrganizationPlayer(GameSession.MoveCurrentTeamReservePlayerToNhl);
    }

    public void MoveSelectedReservePlayerToFarm()
    {
        MoveSelectedOrganizationPlayer(GameSession.MoveCurrentTeamReservePlayerToFarm);
    }

    public void SelectWaiver(string waiverId)
    {
        _selectedWaiverId = waiverId;
        MeasurePanelRefresh("Waivers", RefreshWaivers);
    }

    public void ClaimSelectedWaiverPlayer()
    {
        if (string.IsNullOrEmpty(_selectedWaiverId))
        {
            Debug.LogWarning("Игрок на waivers не выбран");
            return;
        }

        bool success = GameSession.ClaimWaiverPlayer(_selectedWaiverId, out string message);
        if (success)
        {
            Debug.Log(message);
            _selectedWaiverId = "";
        }
        else
        {
            Debug.LogWarning(message);
        }

        MeasurePanelRefresh("Waivers", RefreshWaivers);
        MeasurePanelRefresh("Organization", RefreshOrganization);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        if (_rosterController != null)
        {
            MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
        }

        MeasurePanelRefresh("Contracts", RefreshContracts);
        MeasurePanelRefresh("Lineup", RefreshLineup);
        MeasurePanelRefresh("Tactics", RefreshTactics);
    }

    public void SelectProspect(string prospectId)
    {
        _selectedProspectId = prospectId;
        _selectedScoutingProspectId = prospectId;
        MeasurePanelRefresh("Draft", RefreshDraft);
    }

    public void SelectScoutingProspect(string prospectId)
    {
        _selectedScoutingProspectId = prospectId;
        _selectedProspectId = prospectId;
        MeasurePanelRefresh("Scouting", RefreshScouting);
    }

    public void ScoutSelectedProspect()
    {
        if (string.IsNullOrEmpty(_selectedScoutingProspectId))
        {
            Debug.LogWarning("Проспект не выбран");
            return;
        }

        ScoutingActionResultData result = GameSession.ScoutCurrentDraftProspect(_selectedScoutingProspectId);
        LogScoutingResult(result);
        MeasurePanelRefresh("Scouting", RefreshScouting);
        MeasurePanelRefresh("Draft", RefreshDraft);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void ScoutTopProspects()
    {
        ScoutingActionResultData result = GameSession.ScoutTopDraftProspects();
        LogScoutingResult(result);
        MeasurePanelRefresh("Scouting", RefreshScouting);
        MeasurePanelRefresh("Draft", RefreshDraft);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void ScoutForwards()
    {
        ScoutingActionResultData result = GameSession.ScoutDraftProspectsByPosition("Forward");
        LogScoutingResult(result);
        MeasurePanelRefresh("Scouting", RefreshScouting);
        MeasurePanelRefresh("Draft", RefreshDraft);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void ScoutDefensemen()
    {
        ScoutingActionResultData result = GameSession.ScoutDraftProspectsByPosition("D");
        LogScoutingResult(result);
        MeasurePanelRefresh("Scouting", RefreshScouting);
        MeasurePanelRefresh("Draft", RefreshDraft);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void ScoutGoalies()
    {
        ScoutingActionResultData result = GameSession.ScoutDraftProspectsByPosition("G");
        LogScoutingResult(result);
        MeasurePanelRefresh("Scouting", RefreshScouting);
        MeasurePanelRefresh("Draft", RefreshDraft);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void DraftSelectedProspect()
    {
        bool drafted = DraftService.SelectProspectForCurrentPick(
            GameSession.CurrentState,
            _selectedProspectId,
            out string message);

        if (drafted)
        {
            Debug.Log(message);
            _selectedProspectId = "";
            SaveLoadService.Save(GameSession.CurrentState);
        }
        else
        {
            Debug.LogWarning(message);
        }

        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        MeasurePanelRefresh("Draft", RefreshDraft);
    }

    public void AutoDraftUntilUserPick()
    {
        DraftService.AutoPickUntilUserPickOrDraftEnd(GameSession.CurrentState);
        if (GameSession.CurrentState != null)
        {
            SaveLoadService.Save(GameSession.CurrentState);
        }

        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        MeasurePanelRefresh("Draft", RefreshDraft);
    }

    public void SelectFreeAgent(string playerId)
    {
        _selectedFreeAgentId = playerId;
        _selectedFreeAgentPlayerId = playerId;
        PlayerData player = FindSelectedFreeAgent(playerId);
        if (player != null)
        {
            _freeAgentOfferSalary = player.FreeAgencyExpectedSalary;
            _freeAgentOfferYears = player.FreeAgencyExpectedYears;
        }

        MeasurePanelRefresh("FreeAgency", RefreshFreeAgency);
    }

    public void SelectFreeAgentPlayer(string playerId)
    {
        SelectFreeAgent(playerId);
    }

    public void SetFreeAgentOfferSalary(string salaryText)
    {
        if (!TryParseSalary(salaryText, out int salary))
        {
            Debug.LogWarning("Не удалось прочитать зарплату оффера FA");
            return;
        }

        _freeAgentOfferSalary = salary;
        MeasurePanelRefresh("FreeAgency", RefreshFreeAgency);
    }

    public void SetFreeAgentOfferYears(string yearsText)
    {
        if (!int.TryParse(yearsText, out int years))
        {
            Debug.LogWarning("Не удалось прочитать срок оффера FA");
            return;
        }

        _freeAgentOfferYears = BetterFreeAgencyConfig.ClampFreeAgentYears(
            years,
            GameSession.CurrentState == null ? null : GameSession.CurrentState.LeagueRules);
        MeasurePanelRefresh("FreeAgency", RefreshFreeAgency);
    }

    public void SignSelectedFreeAgent()
    {
        OfferSelectedFreeAgentExpectedContract();
    }

    public void OfferSelectedFreeAgentExpectedContract()
    {
        PlayerData player = FindSelectedFreeAgent(GetSelectedFreeAgentId());
        if (player == null)
        {
            Debug.LogWarning("Свободный агент не выбран");
            return;
        }

        MakeFreeAgentOffer(player.Id, player.FreeAgencyExpectedSalary, player.FreeAgencyExpectedYears);
    }

    public void OfferSelectedFreeAgentMinimumContract()
    {
        PlayerData player = FindSelectedFreeAgent(GetSelectedFreeAgentId());
        if (player == null)
        {
            Debug.LogWarning("Свободный агент не выбран");
            return;
        }

        MakeFreeAgentOffer(player.Id, player.FreeAgencyMinimumSalary, player.FreeAgencyPreferredYears);
    }

    public void OfferSelectedFreeAgentPlusTenPercent()
    {
        PlayerData player = FindSelectedFreeAgent(GetSelectedFreeAgentId());
        if (player == null)
        {
            Debug.LogWarning("Свободный агент не выбран");
            return;
        }

        int salary = BetterFreeAgencyConfig.ClampSalary(
            player.FreeAgencyExpectedSalary * 110 / 100,
            GameSession.CurrentState == null ? null : GameSession.CurrentState.LeagueRules);
        MakeFreeAgentOffer(player.Id, salary, player.FreeAgencyExpectedYears);
    }

    public void OfferSelectedFreeAgentCustomContract()
    {
        PlayerData player = FindSelectedFreeAgent(GetSelectedFreeAgentId());
        if (player == null)
        {
            Debug.LogWarning("Свободный агент не выбран");
            return;
        }

        int salary = _freeAgentOfferSalary > 0 ? _freeAgentOfferSalary : player.FreeAgencyExpectedSalary;
        int years = _freeAgentOfferYears > 0 ? _freeAgentOfferYears : player.FreeAgencyExpectedYears;
        MakeFreeAgentOffer(player.Id, salary, years);
    }

    public void RunCpuFreeAgencySignings()
    {
        List<FreeAgentOfferData> offers = GameSession.RunCpuFreeAgency(5);
        int accepted = 0;
        foreach (FreeAgentOfferData offer in offers)
        {
            if (offer != null && offer.Accepted)
            {
                accepted++;
            }
        }

        Debug.Log("CPU free agency: офферов " + offers.Count + ", подписаний " + accepted);
        MeasurePanelRefresh("FreeAgency", RefreshFreeAgency);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        MeasurePanelRefresh("Contracts", RefreshContracts);
        MeasurePanelRefresh("Organization", RefreshOrganization);
    }

    private string GetSelectedFreeAgentId()
    {
        return string.IsNullOrEmpty(_selectedFreeAgentPlayerId)
            ? _selectedFreeAgentId
            : _selectedFreeAgentPlayerId;
    }

    private PlayerData FindSelectedFreeAgent(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        GameSession.EnsureBetterFreeAgency();
        List<PlayerData> freeAgents = GameSession.GetEvaluatedFreeAgents();
        foreach (PlayerData player in freeAgents)
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private void MakeFreeAgentOffer(string playerId, int salary, int years)
    {
        FreeAgentOfferData offer = GameSession.MakeCurrentTeamFreeAgentOffer(playerId, salary, years);
        if (offer == null)
        {
            Debug.LogWarning("Оффер свободному агенту не создан");
            return;
        }

        if (offer.Decision == "Accepted")
        {
            Debug.Log(offer.DecisionReason);
            _selectedFreeAgentId = "";
            _selectedFreeAgentPlayerId = "";
            _freeAgentOfferSalary = 0;
            _freeAgentOfferYears = 0;
        }
        else
        {
            Debug.LogWarning(offer.DecisionReason);
        }

        MeasurePanelRefresh("FreeAgency", RefreshFreeAgency);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        if (_rosterController != null)
        {
            MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
        }

        MeasurePanelRefresh("Contracts", RefreshContracts);
        MeasurePanelRefresh("Trades", RefreshTrades);
        MeasurePanelRefresh("Lineup", RefreshLineup);
        MeasurePanelRefresh("Tactics", RefreshTactics);
        MeasurePanelRefresh("Injuries", RefreshInjuries);
        MeasurePanelRefresh("Organization", RefreshOrganization);
    }

    public void SimulateToFreeAgencyForTesting()
    {
        RunWithBusy("Симуляция до free agency...", () =>
        {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
        SimulateToDraftForTesting();

        int safetyCounter = 0;
        while (GameSession.CurrentState != null
            && GameSession.CurrentState.Draft != null
            && !GameSession.CurrentState.Draft.IsCompleted
            && safetyCounter < DraftConfig.TotalDraftPicks)
        {
            DraftService.AutoPickCurrentSelection(GameSession.CurrentState, out string message);
            safetyCounter++;
        }

        GameSession.EnsureFreeAgents();
        GameSession.EnsureBetterFreeAgency();
        if (GameSession.CurrentState != null)
        {
            SaveLoadService.Save(GameSession.CurrentState);
        }

        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        MeasurePanelRefresh("Draft", RefreshDraft);
        MeasurePanelRefresh("FreeAgency", RefreshFreeAgency);
        MeasurePanelRefresh("Injuries", RefreshInjuries);
        }
        finally
        {
            stopwatch.Stop();
            PerformanceTimerService.RecordSimulateSeason(GameSession.CurrentState, stopwatch.ElapsedMilliseconds);
        }
        });
    }

    public void SimulateToDraftForTesting()
    {
        RunWithBusy("Симуляция до драфта...", () =>
        {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
        GameSession.EnsureSeason();

        while (GameSession.CurrentState != null
            && GameSession.CurrentState.Season != null
            && !GameSession.CurrentState.Season.IsSeasonFinished)
        {
            int gamesBefore = GameSession.CurrentState.TotalGamesSimulated;
            GameSession.SimulateNextLeagueDay();
            if (GameSession.CurrentState != null && GameSession.CurrentState.TotalGamesSimulated == gamesBefore)
            {
                Debug.LogWarning("Симуляция остановлена: игровой день не был сыгран");
                break;
            }
        }

        GameSession.EnsurePlayoffs();

        int safetyCounter = 0;
        while (GameSession.CurrentState != null
            && !GameSession.IsPlayoffsCompleted()
            && safetyCounter < 120)
        {
            MatchResultData result = GameSession.SimulateNextPlayoffGame();
            if (result == null)
            {
                break;
            }

            safetyCounter++;
        }

        GameSession.EnsureDraftPickOwnership();
        DraftService.EnsureDraft(GameSession.CurrentState);
        if (GameSession.CurrentState != null)
        {
            SaveLoadService.Save(GameSession.CurrentState);
        }

        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        MeasurePanelRefresh("Draft", RefreshDraft);
        MeasurePanelRefresh("Injuries", RefreshInjuries);
        }
        finally
        {
            stopwatch.Stop();
            PerformanceTimerService.RecordSimulateSeason(GameSession.CurrentState, stopwatch.ElapsedMilliseconds);
        }
        });
    }

    public void SelectUserTradePlayer(string playerId)
    {
        _selectedUserTradePlayerId = playerId;
        MeasurePanelRefresh("Trades", RefreshTrades);
    }

    public void SelectUserTradePick(string pickId)
    {
        _selectedUserTradePickId = pickId;
        MeasurePanelRefresh("Trades", RefreshTrades);
    }

    public void SelectOtherTradeTeam(string teamId)
    {
        _selectedOtherTradeTeamId = teamId;
        _selectedOtherTradePlayerId = "";
        _selectedOtherTradePickId = "";
        MeasurePanelRefresh("Trades", RefreshTrades);
    }

    public void SelectOtherTradePlayer(string playerId)
    {
        _selectedOtherTradePlayerId = playerId;
        MeasurePanelRefresh("Trades", RefreshTrades);
    }

    public void SelectOtherTradePick(string pickId)
    {
        _selectedOtherTradePickId = pickId;
        MeasurePanelRefresh("Trades", RefreshTrades);
    }

    public void ExecuteSelectedTrade()
    {
        List<TradeAssetData> assetsFromUserTeam = BuildTradeAssets(
            GameSession.CurrentTeam,
            _selectedUserTradePlayerId,
            _selectedUserTradePickId);
        TeamData otherTeam = FindTeam(GameSession.CurrentState, _selectedOtherTradeTeamId);
        List<TradeAssetData> assetsFromOtherTeam = BuildTradeAssets(
            otherTeam,
            _selectedOtherTradePlayerId,
            _selectedOtherTradePickId);

        if (assetsFromUserTeam.Count == 0 || assetsFromOtherTeam.Count == 0)
        {
            Debug.LogWarning("Для обмена нужно выбрать активы обеих команд");
            MeasurePanelRefresh("Trades", RefreshTrades);
            return;
        }

        bool accepted = GameSession.TryTradeAssets(
            assetsFromUserTeam,
            _selectedOtherTradeTeamId,
            assetsFromOtherTeam,
            out TradeProposalData proposal,
            out string message);

        if (accepted)
        {
            Debug.Log(message);
            _selectedUserTradePlayerId = "";
            _selectedUserTradePickId = "";
            _selectedOtherTradeTeamId = "";
            _selectedOtherTradePlayerId = "";
            _selectedOtherTradePickId = "";
        }
        else
        {
            Debug.LogWarning(message);
        }

        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        MeasurePanelRefresh("Trades", RefreshTrades);
        MeasurePanelRefresh("Contracts", RefreshContracts);
        if (_rosterController != null)
        {
            MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
        }
        MeasurePanelRefresh("Lineup", RefreshLineup);
        MeasurePanelRefresh("Tactics", RefreshTactics);
        MeasurePanelRefresh("Injuries", RefreshInjuries);
        MeasurePanelRefresh("Organization", RefreshOrganization);
    }

    public void ShowCalendar()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(true);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        SeasonData season = GameSession.CurrentState == null ? null : GameSession.CurrentState.Season;
        if (season != null)
        {
            _selectedCalendarTargetDay = Mathf.Max(1, season.CurrentDay);
        }
        MeasurePanelRefresh("Calendar", RefreshCalendar);
        OnTutorialPanelShown(TutorialConfig.PanelCalendar);
    }

    public void ShowStandings()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(true);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        MeasurePanelRefresh("Standings", RefreshStandings);
        OnTutorialPanelShown(TutorialConfig.PanelStandings);
    }

    public void ShowPlayerStats()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(true);
        _playoffsPanel.SetActive(false);
        MeasurePanelRefresh("PlayerStats", RefreshPlayerStats);
        OnTutorialPanelShown("PlayerStats");
    }

    public void ShowStandingsDivisions()
    {
        _selectedStandingsMode = StandingsController.ModeDivisions;
        ShowStandings();
    }

    public void ShowStandingsConferences()
    {
        _selectedStandingsMode = StandingsController.ModeConferences;
        ShowStandings();
    }

    public void ShowStatsForwards()
    {
        _selectedPlayerStatsMode = PlayerStatsController.ModeForwards;
        ShowPlayerStats();
    }

    public void ShowStatsDefensemen()
    {
        _selectedPlayerStatsMode = PlayerStatsController.ModeDefensemen;
        ShowPlayerStats();
    }

    public void ShowStatsGoalies()
    {
        _selectedPlayerStatsMode = PlayerStatsController.ModeGoalies;
        ShowPlayerStats();
    }

    public void ShowStatsUnder21()
    {
        _selectedPlayerStatsMode = PlayerStatsController.ModeUnder21;
        ShowPlayerStats();
    }

    public void ShowStatsSelectedTeam()
    {
        _selectedPlayerStatsMode = PlayerStatsController.ModeTeam;
        EnsureSelectedPlayerStatsTeam();
        ShowPlayerStats();
    }

    public void SelectPreviousPlayerStatsTeam()
    {
        SelectPlayerStatsTeamOffset(-1);
    }

    public void SelectNextPlayerStatsTeam()
    {
        SelectPlayerStatsTeamOffset(1);
    }

    public void SelectPlayerStatsTeamByDropdown(int optionIndex)
    {
        List<TeamData> teams = GameSession.CurrentState == null ? null : GameSession.CurrentState.Teams;
        if (teams == null || optionIndex < 0 || optionIndex >= teams.Count)
        {
            return;
        }

        TeamData team = teams[optionIndex];
        if (team == null)
        {
            return;
        }

        _selectedPlayerStatsTeamId = team.Id;
        _selectedPlayerStatsMode = PlayerStatsController.ModeTeam;
        MeasurePanelRefresh("PlayerStats", RefreshPlayerStats);
    }

    public void CalendarPreviousDay()
    {
        _selectedCalendarTargetDay = Mathf.Max(1, _selectedCalendarTargetDay - 1);
        MeasurePanelRefresh("Calendar", RefreshCalendar);
    }

    public void CalendarNextDay()
    {
        _selectedCalendarTargetDay = Mathf.Min(GetMaxScheduleDay(), Mathf.Max(1, _selectedCalendarTargetDay + 1));
        MeasurePanelRefresh("Calendar", RefreshCalendar);
    }

    public void SimulateToSelectedCalendarDay()
    {
        RunWithBusy("Симуляция до выбранного дня...", () =>
        {
            GameSession.SimulateRegularSeasonToDay(_selectedCalendarTargetDay);
            MeasurePanelRefresh("Calendar", RefreshCalendar);
            MeasurePanelRefresh("Dashboard", RefreshDashboard);
            MeasurePanelRefresh("Standings", RefreshStandings);
            MeasurePanelRefresh("PlayerStats", RefreshPlayerStats);
            MeasurePanelRefresh("Injuries", RefreshInjuries);
            MeasurePanelRefresh("Waivers", RefreshWaivers);
            if (IsPlayoffEntryAvailable())
            {
                MeasurePanelRefresh("Playoffs", RefreshPlayoffs);
            }
        });
    }

    public void ShowPlayoffs()
    {
        HideAllPanels();
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_scoutingPanel, false);
        SetPanelActive(_organizationPanel, false);
        SetPanelActive(_waiversPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_ownerPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        SetPanelActive(_moralePanel, false);
        SetPanelActive(_leadershipPanel, false);
        SetPanelActive(_staffPanel, false);
        SetPanelActive(_extensionsPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(true);
        MeasurePanelRefresh("Playoffs", RefreshPlayoffs);
        OnTutorialPanelShown("Playoffs");
    }

    public void SimulateMatch()
    {
        StartCoroutine(SimulateMatchRoutine());
    }

    private IEnumerator SimulateMatchRoutine()
    {
        bool shouldShowPostGame = false;
        RunWithBusy("Симуляция матча...", () =>
        {
        MatchResultData result = GameSession.SimulateNextUserGameFast();
        if (result == null)
        {
            if (GameSession.AutoFixCurrentTeamRosterAndLineup(out string autoFixMessage))
            {
                Debug.Log(autoFixMessage);
                result = GameSession.SimulateNextUserGameFast();
            }
        }

        if (result == null)
        {
            if (IsPlayoffEntryAvailable())
            {
                ShowPlayoffs();
                Debug.Log("Регулярный сезон завершён. Открыт экран плей-офф.");
                return;
            }

            SetNoAvailableMatchText();
            string validationMessage = "";
            GameSession.ValidateCurrentTeamCanPlay(out validationMessage);
            if (string.IsNullOrEmpty(validationMessage))
            {
                validationMessage = "Нет доступных матчей";
            }

            Debug.LogWarning(validationMessage);
            OpenRosterFixScreen(validationMessage);
            return;
        }

        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        GameSession.PreparePostGameSummary(result);
        shouldShowPostGame = true;
        Debug.Log("Результат матча: " + result.Summary);
        });

        if (shouldShowPostGame)
        {
            ShowBusy("Симуляция матча...");
            yield return new WaitForSeconds(5f);
            HideBusy();
            ShowPostGameSummary();
        }
    }

    public void SimulatePlayoffGame()
    {
        RunWithBusy("Симуляция матча плей-офф...", () =>
        {
        if (!GameSession.CanStartPlayoffs())
        {
            MeasurePanelRefresh("Playoffs", RefreshPlayoffs);
            Debug.LogWarning("Плей-офф станет доступен после завершения регулярного сезона");
            return;
        }

        MatchResultData result = GameSession.SimulateNextPlayoffGame();
        if (result == null)
        {
            Debug.LogWarning("Нет доступного матча плей-офф");
            MeasurePanelRefresh("Playoffs", RefreshPlayoffs);
            return;
        }

        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        MeasurePanelRefresh("Playoffs", RefreshPlayoffs);
        MeasurePanelRefresh("PlayerStats", RefreshPlayerStats);
        MeasurePanelRefresh("Injuries", RefreshInjuries);
        MeasurePanelRefresh("Waivers", RefreshWaivers);
        Debug.Log("Матч плей-офф: " + result.Summary);
        });
    }

    public void SimulateRegularSeasonToEnd()
    {
        RunWithBusy("Симуляция сезона...", () =>
        {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
        while (GameSession.CurrentState != null
            && GameSession.CurrentState.Season != null
            && !GameSession.CurrentState.Season.IsSeasonFinished)
        {
            int gamesBefore = GameSession.CurrentState.TotalGamesSimulated;
            GameSession.SimulateNextLeagueDay();
            if (GameSession.CurrentState != null && GameSession.CurrentState.TotalGamesSimulated == gamesBefore)
            {
                Debug.LogWarning("Симуляция сезона остановлена: исправьте состав перед следующим матчем");
                break;
            }
            if (GameSession.CurrentState == null
                || GameSession.CurrentState.Season == null
                || GameSession.CurrentState.Season.IsSeasonFinished)
            {
                break;
            }
        }

        GameSession.EnsurePlayoffs();
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        MeasurePanelRefresh("Injuries", RefreshInjuries);
        }
        finally
        {
            stopwatch.Stop();
            PerformanceTimerService.RecordSimulateSeason(GameSession.CurrentState, stopwatch.ElapsedMilliseconds);
        }
        });
    }

    public void SaveGame()
    {
        if (GameSession.CurrentState == null)
        {
            Debug.LogWarning("Нет активной игры для сохранения");
            return;
        }

        if (GameSession.CurrentLiveMatch != null)
        {
            Debug.LogWarning("Нельзя сохранить игру во время live-матча. Завершите матч.");
            return;
        }

        ShowBusy("Сохранение...");
        try
        {
        GameSession.EnsureContracts();
        GameSession.EnsureRosterStatuses();
        GameSession.EnsureWaivers();
        GameSession.EnsureTradeHistory();
        GameSession.EnsureDraftPickOwnership();
        GameSession.EnsureFreeAgents();
        GameSession.EnsureBetterFreeAgency();
        GameSession.EnsureProspectSigningHistory();
        GameSession.EnsureScouting();
        GameSession.EnsureSeasonHistory();
        GameSession.EnsureDevelopmentHistory();
        GameSession.EnsureLineups();
        GameSession.EnsureFatigue();
        GameSession.EnsureInjuries();
        GameSession.EnsureSpecialTeamsAndTactics();
        GameSession.EnsureMorale();
        GameSession.EnsureCoachingStaff();
        GameSession.EnsureChemistry();
        GameSession.EnsureContractExtensions();
        GameSession.EnsureOwnerGoals();
        GameSession.EnsureGmCareer();
        GameSession.EnsureLeagueHistory();
        GameSession.EnsureNewsFeed();
        GameSession.EnsureTutorial();
        SaveLoadService.Save(GameSession.CurrentState);
        GameSession.MarkTutorialStepCompleted(TutorialConfig.StepSaveGame);
        UpdateTutorialChecklistIfVisible();
        Debug.Log("Игра сохранена");
        }
        finally
        {
            HideBusy();
        }
    }

    public void PlayNextUserMatch()
    {
        RunWithBusy("Подготовка матча...", () =>
        {
            bool prepared = GameSession.PrepareNextUserMatch(out PreGameSetupData setup, out string message);
            if (!prepared && GameSession.AutoFixCurrentTeamRosterAndLineup(out string autoFixMessage))
            {
                Debug.Log(autoFixMessage);
                prepared = GameSession.PrepareNextUserMatch(out setup, out message);
            }

            Debug.Log(message);
            if (!prepared)
            {
                Debug.LogWarning(message);
                OpenRosterFixScreen(message);
                return;
            }

            HideAllPanels();
            HideTutorialHintOverlay();
            SetPanelOnly(_preGamePanel, true);
            if (_preGameController != null)
            {
                _preGameController.ShowPreGame(setup);
            }
        });
    }

    public void StartLiveMatch()
    {
        bool started = GameSession.StartPreparedLiveMatch(out string message);
        if (!started)
        {
            Debug.LogWarning(message);
            return;
        }

        HideAllPanels();
        HideTutorialHintOverlay();
        SetPanelOnly(_liveMatchPanel, true);
        if (_liveMatchController != null)
        {
            _liveMatchController.ShowLiveMatch(GameSession.CurrentLiveMatch);
        }
    }

    public void FinishLiveMatch()
    {
        if (_isCompletingLiveMatchResult)
        {
            return;
        }

        if (!GameSession.PrepareCurrentLivePostGameSummary(out string message))
        {
            Debug.LogWarning(message);
            return;
        }

        ShowPostGameSummary();
        StartCoroutine(CompleteLiveMatchResultAfterSummary());
    }

    private IEnumerator CompleteLiveMatchResultAfterSummary()
    {
        _isCompletingLiveMatchResult = true;
        yield return null;

        bool completed = GameSession.CompleteCurrentLiveMatchAndApplyResult(
            out MatchResultData result,
            out string message,
            false);
        if (!completed)
        {
            Debug.LogWarning(message);
            _isCompletingLiveMatchResult = false;
            yield break;
        }

        Debug.Log(message);
        yield return new WaitForSeconds(0.25f);
        if (GameSession.CurrentState != null)
        {
            SaveLoadService.Save(GameSession.CurrentState);
        }

        _isCompletingLiveMatchResult = false;
    }

    public void ExitLiveMatchAndFinish()
    {
        RunWithBusy("Матч доигрывается...", () =>
        {
            bool completed = GameSession.ExitLiveMatchAndFinish(out MatchResultData result, out string message);
            if (!completed)
            {
                Debug.LogWarning(message);
                return;
            }

            ShowPostGameSummary();
        });
    }

    public void ShowPostGameSummary()
    {
        HideAllPanels();
        HideTutorialHintOverlay();
        SetPanelOnly(_postGameSummaryPanel, true);
        if (_postGameSummaryController != null)
        {
            _postGameSummaryController.ShowSummary(GameSession.LastPostGameSummary);
        }
    }

    private void HideTutorialHintOverlay()
    {
        if (_tutorialHintView != null)
        {
            _tutorialHintView.Clear();
        }
    }

    public void DeleteSave()
    {
        SaveLoadService.DeleteSave();
        GameSession.Clear();
        SceneManager.LoadScene("MainMenu");
    }

    public void BackToMainMenu()
    {
        if (UseCompactDashboardText() && _dashboardPanel != null && _dashboardPanel.activeSelf)
        {
            ShowDashboard();
            return;
        }

        SceneManager.LoadScene("MainMenu");
    }

    private void RefreshDashboard()
    {
        UpdateTeamText();
        UpdateSeasonRulesText();
        UpdateLeagueDateText();
        UpdateTradeStatusText();
        UpdateFreeAgencyStatusText();
        UpdateCurrentDayText();
        UpdateGamesSimulatedText();
        UpdateNextGameText();
        UpdateLastMatchResultText();
        UpdateFinanceText();
        UpdateCpuRosterAiText();
        UpdateDashboardNavigation();
    }

    private void RefreshContracts()
    {
        GameSession.EnsureContractExtensions();

        if (_contractsController != null)
        {
            _contractsController.ShowContracts();
        }
    }

    private void RefreshExtensions()
    {
        GameSession.EnsureContractExtensions();

        if (_extensionsController != null)
        {
            _extensionsController.ShowExtensions(
                GameSession.CurrentState,
                _selectedExtensionPlayerId,
                _extensionOfferSalary,
                _extensionOfferYears);
        }
    }

    private void RefreshFreeAgency()
    {
        GameSession.EnsureFreeAgents();
        GameSession.EnsureBetterFreeAgency();

        if (_freeAgencyController != null)
        {
            _freeAgencyController.ShowFreeAgency(
                GameSession.CurrentState,
                GetSelectedFreeAgentId(),
                _freeAgentOfferSalary,
                _freeAgentOfferYears);
        }
    }

    private void RefreshDraft()
    {
        GameSession.EnsureDraftPickOwnership();
        GameSession.EnsureScouting();

        if (_draftController != null)
        {
            _draftController.ShowDraft(GameSession.CurrentState, _selectedProspectId);
        }
    }

    private void RefreshScouting()
    {
        GameSession.EnsureScouting();

        if (_scoutingController != null)
        {
            _scoutingController.ShowScouting(GameSession.CurrentState, _selectedScoutingProspectId);
        }
    }

    private void RefreshProspectRights()
    {
        GameSession.EnsureProspectSigningHistory();

        if (_prospectRightsController != null)
        {
            _prospectRightsController.ShowProspectRights(GameSession.CurrentState, _selectedProspectRightsId);
        }
    }

    private void RefreshOrganization()
    {
        GameSession.EnsureRosterStatuses();

        if (_organizationController != null)
        {
            _organizationController.ShowOrganization(GameSession.CurrentState, _selectedOrganizationPlayerId, this);
        }
    }

    private void RefreshWaivers()
    {
        GameSession.EnsureWaivers();

        if (_waiversController != null)
        {
            _waiversController.ShowWaivers(GameSession.CurrentState, _selectedWaiverId, this);
        }
    }

    private void RefreshOffseason()
    {
        GameSession.EnsureSeasonHistory();
        GameSession.EnsureOwnerGoals();
        GameSession.GenerateSeasonRecapIfNeeded();
        GameSession.EnsureLeagueHistory();
        GameSession.GenerateSeasonRecapNewsIfNeeded();

        if (_offseasonController != null)
        {
            _offseasonController.ShowOffseason(GameSession.CurrentState);
        }
    }

    private void RefreshHistory()
    {
        GameSession.EnsureLeagueHistory();
        GameSession.EnsureRetirementHistory();
        GameSession.GenerateSeasonRecapNewsIfNeeded();

        if (_historyController != null)
        {
            _historyController.ShowHistory(GameSession.CurrentState);
        }
    }

    private void RefreshNews()
    {
        GameSession.EnsureNewsFeed();

        if (_newsController != null)
        {
            _newsController.ShowNews(GameSession.CurrentState, _selectedNewsFilter, this);
        }
    }

    public void ShowTutorial()
    {
        if (TutorialPanel != null)
        {
            TutorialPanel.SetActive(true);
        }

        UpdateTutorialUi();
    }

    public void HideTutorial()
    {
        if (TutorialPanel != null)
        {
            TutorialPanel.SetActive(false);
        }
    }

    public void DismissCurrentTutorialHint()
    {
        if (!string.IsNullOrEmpty(_currentTutorialHintId))
        {
            GameSession.DismissTutorialHint(_currentTutorialHintId);
        }

        _currentTutorialHintId = "";
        UpdateTutorialHint(_currentTutorialPanelId);
        UpdateTutorialUi();
    }

    public void DisableTutorial()
    {
        GameSession.DisableTutorial();
        _currentTutorialHintId = "";
        if (_tutorialHintView != null)
        {
            _tutorialHintView.Clear();
        }

        HideTutorial();
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void ResetTutorial()
    {
        GameSession.ResetTutorial();
        UpdateTutorialHint(_currentTutorialPanelId);
        UpdateTutorialUi();
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    public void UpdateTutorialUi()
    {
        GameSession.EnsureTutorial();
        if (TutorialTitleText != null)
        {
            TutorialTitleText.text = "Обучение";
        }

        if (TutorialBodyText != null)
        {
            TutorialBodyText.text = GameSession.GetTutorialSummary()
                + "\nПервые шаги помогут освоить Dashboard, состав, линии, таблицу, контракты и сохранение.";
        }

        if (TutorialChecklistText != null)
        {
            TutorialChecklistText.text = BuildTutorialChecklistText();
        }

        if (_tutorialController != null)
        {
            _tutorialController.ShowTutorial(GameSession.CurrentState, _currentTutorialPanelId, this);
        }
    }

    public void UpdateTutorialHint(string panelId)
    {
        _currentTutorialPanelId = TutorialConfig.NormalizePanelId(panelId);
        TutorialHintData hint = GameSession.GetCurrentPanelHint(_currentTutorialPanelId);
        _currentTutorialHintId = hint == null ? "" : hint.HintId;

        if (TutorialHintText != null)
        {
            TutorialHintText.text = hint == null ? "" : hint.Title + "\n" + hint.Body;
        }

        if (_tutorialHintView != null)
        {
            _tutorialHintView.Initialize(hint, this);
        }
    }

    private void OnTutorialPanelShown(string panelId)
    {
        _currentTutorialPanelId = TutorialConfig.NormalizePanelId(panelId);
        GameSession.MarkTutorialPanelVisited(_currentTutorialPanelId);
        UpdateTutorialHint(_currentTutorialPanelId);
        UpdateTutorialChecklistIfVisible();
    }

    private void UpdateTutorialChecklistIfVisible()
    {
        if (TutorialPanel != null && TutorialPanel.activeSelf)
        {
            UpdateTutorialUi();
        }
    }

    private static string BuildTutorialChecklistText()
    {
        List<TutorialStepData> steps = GameSession.GetTutorialSteps();
        string text = "";
        foreach (TutorialStepData step in steps)
        {
            if (step == null)
            {
                continue;
            }

            text += (step.IsCompleted ? "[x] " : "[ ] ")
                + step.Title
                + " - " + step.Description
                + "\n";
        }

        return text;
    }

    private void RefreshOwner()
    {
        GameSession.EnsureOwnerGoals();
        GameSession.EnsureGmCareer();

        if (_ownerController != null)
        {
            _ownerController.ShowOwner(GameSession.CurrentState);
        }
    }

    private void RefreshGmCareer()
    {
        GameSession.EnsureGmCareer();

        if (_gmCareerController != null)
        {
            _gmCareerController.ShowGmCareer(GameSession.CurrentState, _selectedGmJobOfferId);
        }
    }

    private void RefreshDiagnostics()
    {
        if (_diagnosticsController != null)
        {
            _diagnosticsController.ShowDiagnostics(GameSession.CurrentState);
        }
    }

    private void RefreshDevelopment()
    {
        GameSession.EnsureDevelopmentHistory();

        if (_developmentController != null)
        {
            _developmentController.ShowDevelopment(GameSession.CurrentState);
        }
    }

    private void RefreshRoles()
    {
        GameSession.EnsureRolesAndUsage();
        GameSession.EnsureMorale();

        if (_rolesController != null)
        {
            _rolesController.ShowRoles(GameSession.CurrentState, _selectedRolePlayerId);
        }
    }

    private void RefreshMorale()
    {
        GameSession.EnsureMorale();

        if (_moraleController != null)
        {
            _moraleController.ShowMorale(GameSession.CurrentState, _selectedMoralePlayerId);
        }
    }

    private void RefreshLeadership()
    {
        GameSession.EnsureLeadership();

        if (_leadershipController != null)
        {
            _leadershipController.ShowLeadership(GameSession.CurrentState, _selectedLeadershipPlayerId);
        }
    }

    private void RefreshStaff()
    {
        GameSession.EnsureCoachingStaff();

        if (_staffController != null)
        {
            _staffController.ShowStaff(GameSession.CurrentState);
        }
    }

    private void RefreshLeadershipRelatedPanels()
    {
        MeasurePanelRefresh("Leadership", RefreshLeadership);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        if (_rosterController != null)
        {
            MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
        }

        MeasurePanelRefresh("Morale", RefreshMorale);
        MeasurePanelRefresh("Lineup", RefreshLineup);
        MeasurePanelRefresh("Tactics", RefreshTactics);
    }

    private void MakeExtensionOffer(string playerId, int salary, int years)
    {
        ContractExtensionOfferData offer = GameSession.MakeCurrentTeamExtensionOffer(playerId, salary, years);
        if (offer == null)
        {
            Debug.LogWarning("Предложение продления не создано");
            return;
        }

        if (offer.Decision == "Accepted")
        {
            Debug.Log(offer.DecisionReason);
            _selectedExtensionPlayerId = "";
            _extensionOfferSalary = 0;
            _extensionOfferYears = 0;
        }
        else
        {
            Debug.LogWarning(offer.DecisionReason);
        }

        MeasurePanelRefresh("Extensions", RefreshExtensions);
        MeasurePanelRefresh("Contracts", RefreshContracts);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        MeasurePanelRefresh("Morale", RefreshMorale);
        MeasurePanelRefresh("Trades", RefreshTrades);
        if (_rosterController != null)
        {
            MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
        }
    }

    private ContractExtensionCandidateData FindExtensionCandidate(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        List<ContractExtensionCandidateData> candidates = GameSession.GetCurrentTeamExtensionCandidates();
        foreach (ContractExtensionCandidateData candidate in candidates)
        {
            if (candidate != null && candidate.PlayerId == playerId)
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool TryParseSalary(string salaryText, out int salary)
    {
        salary = 0;
        if (string.IsNullOrEmpty(salaryText))
        {
            return false;
        }

        string normalized = salaryText.Trim()
            .Replace("$", "")
            .Replace(" ", "")
            .Replace(",", "");
        bool millions = normalized.EndsWith("M", true, CultureInfo.InvariantCulture);
        if (millions)
        {
            normalized = normalized.Substring(0, normalized.Length - 1);
        }

        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
        {
            normalized = normalized.Replace(",", ".");
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                return false;
            }
        }

        if (millions)
        {
            value *= 1000000m;
        }

        salary = (int)value;
        return salary > 0;
    }

    private static void LogCaptaincyResult(CaptaincyActionResultData result)
    {
        if (result == null)
        {
            Debug.LogWarning("Captaincy action failed");
            return;
        }

        if (result.Success)
        {
            Debug.Log(result.Message);
        }
        else
        {
            Debug.LogWarning(result.Message);
        }
    }

    private void RefreshLineup()
    {
        GameSession.EnsureCurrentTeam();
        GameSession.EnsureLineups();
        GameSession.EnsureRolesAndUsage();
        GameSession.EnsureMorale();
        GameSession.EnsureChemistry();

        if (_lineupController != null)
        {
            _lineupController.ShowLineup(
                GameSession.CurrentTeam,
                _selectedLineupSlotType,
                _selectedLineupLineOrPairNumber,
                _selectedLineupSlotPosition,
                _selectedLineupPlayerId);
        }
    }

    private void RefreshTactics()
    {
        GameSession.EnsureSpecialTeamsAndTactics();
        GameSession.EnsureCoachingStaff();
        GameSession.EnsureChemistry();

        if (_tacticsController != null)
        {
            _tacticsController.ShowTactics(GameSession.CurrentTeam);
        }
    }

    private void RefreshInjuries()
    {
        GameSession.EnsureInjuries();

        if (_injuriesController != null)
        {
            _injuriesController.ShowInjuries(GameSession.CurrentState);
        }
    }

    private void SetTacticsPreset(string presetName)
    {
        GameSession.SetCurrentTeamTactics(presetName);
        MeasurePanelRefresh("Tactics", RefreshTactics);
        MeasurePanelRefresh("Roles", RefreshRoles);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    private void SetSelectedPlayerRole(string role)
    {
        if (string.IsNullOrEmpty(_selectedRolePlayerId))
        {
            Debug.LogWarning("Игрок не выбран");
            return;
        }

        bool success = GameSession.SetCurrentTeamPlayerRole(_selectedRolePlayerId, role, out string message);
        if (success)
        {
            Debug.Log(message);
        }
        else
        {
            Debug.LogWarning(message);
        }

        MeasurePanelRefresh("Roles", RefreshRoles);
        if (_rosterController != null)
        {
            MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
        }

        MeasurePanelRefresh("Lineup", RefreshLineup);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
    }

    private void MoveSelectedOrganizationPlayer(System.Func<string, RosterMoveResultData> moveAction)
    {
        if (string.IsNullOrEmpty(_selectedOrganizationPlayerId))
        {
            Debug.LogWarning("Игрок организации не выбран");
            return;
        }

        RosterMoveResultData result = moveAction(_selectedOrganizationPlayerId);
        if (result != null && result.Success)
        {
            Debug.Log(result.Message);
        }
        else
        {
            Debug.LogWarning(result == null ? "Перемещение не выполнено" : result.Message);
        }

        MeasurePanelRefresh("Organization", RefreshOrganization);
        MeasurePanelRefresh("Waivers", RefreshWaivers);
        MeasurePanelRefresh("Dashboard", RefreshDashboard);
        if (_rosterController != null)
        {
            MeasurePanelRefresh("Roster", () => _rosterController.ShowRoster());
        }

        MeasurePanelRefresh("Contracts", RefreshContracts);
        MeasurePanelRefresh("Lineup", RefreshLineup);
        MeasurePanelRefresh("Tactics", RefreshTactics);
        MeasurePanelRefresh("Injuries", RefreshInjuries);
    }

    private void RefreshTrades()
    {
        GameSession.EnsureLeagueRules();
        GameSession.EnsureTradeHistory();
        GameSession.EnsureContracts();
        GameSession.EnsureDraftPickOwnership();
        GameSession.EnsureTradeProfiles();

        if (_tradesController != null)
        {
            _tradesController.ShowTrades(
                GameSession.CurrentState,
                _selectedUserTradePlayerId,
                _selectedUserTradePickId,
                _selectedOtherTradeTeamId,
                _selectedOtherTradePlayerId,
                _selectedOtherTradePickId);
        }
    }

    private void RefreshCalendar()
    {
        GameSession.EnsureSeason();
        SeasonData season = GameSession.CurrentState == null ? null : GameSession.CurrentState.Season;
        if (season != null && _selectedCalendarTargetDay <= 0)
        {
            _selectedCalendarTargetDay = Mathf.Max(1, season.CurrentDay);
        }

        if (_calendarController != null)
        {
            _calendarController.ShowCalendar(season, _selectedCalendarTargetDay);
        }
    }

    private void RefreshStandings()
    {
        GameSession.EnsureSeason();
        SeasonData season = GameSession.CurrentState == null ? null : GameSession.CurrentState.Season;
        if (_standingsController != null)
        {
            List<TeamData> teams = GameSession.CurrentState == null ? null : GameSession.CurrentState.Teams;
            _standingsController.ShowStandings(season, teams, _selectedStandingsMode);
        }
    }

    private void RefreshPlayerStats()
    {
        GameSession.EnsureSeason();
        SeasonData season = GameSession.CurrentState == null ? null : GameSession.CurrentState.Season;
        EnsureSelectedPlayerStatsTeam();
        SyncPlayerStatsTeamDropdown();
        string teamId = string.IsNullOrEmpty(_selectedPlayerStatsTeamId)
            ? (GameSession.CurrentTeam == null ? "" : GameSession.CurrentTeam.Id)
            : _selectedPlayerStatsTeamId;

        if (_playerStatsController != null)
        {
            _playerStatsController.ShowStats(season, teamId, _selectedPlayerStatsMode);
        }
    }

    private void EnsureSelectedPlayerStatsTeam()
    {
        if (!string.IsNullOrEmpty(_selectedPlayerStatsTeamId))
        {
            return;
        }

        if (GameSession.CurrentTeam != null)
        {
            _selectedPlayerStatsTeamId = GameSession.CurrentTeam.Id;
            return;
        }

        List<TeamData> teams = GameSession.CurrentState == null ? null : GameSession.CurrentState.Teams;
        if (teams != null && teams.Count > 0 && teams[0] != null)
        {
            _selectedPlayerStatsTeamId = teams[0].Id;
        }
    }

    private void SelectPlayerStatsTeamOffset(int offset)
    {
        List<TeamData> teams = GameSession.CurrentState == null ? null : GameSession.CurrentState.Teams;
        if (teams == null || teams.Count == 0)
        {
            return;
        }

        EnsureSelectedPlayerStatsTeam();
        int selectedIndex = GetSelectedPlayerStatsTeamIndex(teams);
        if (selectedIndex < 0)
        {
            selectedIndex = 0;
        }

        int nextIndex = (selectedIndex + offset + teams.Count) % teams.Count;
        TeamData team = teams[nextIndex];
        if (team != null)
        {
            _selectedPlayerStatsTeamId = team.Id;
            _selectedPlayerStatsMode = PlayerStatsController.ModeTeam;
            ShowPlayerStats();
        }
    }

    private void SyncPlayerStatsTeamDropdown()
    {
        if (PlayerStatsTeamDropdown == null)
        {
            return;
        }

        List<TeamData> teams = GameSession.CurrentState == null ? null : GameSession.CurrentState.Teams;
        PlayerStatsTeamDropdown.onValueChanged.RemoveListener(SelectPlayerStatsTeamByDropdown);
        PlayerStatsTeamDropdown.ClearOptions();
        if (teams == null || teams.Count == 0)
        {
            PlayerStatsTeamDropdown.AddOptions(new List<string> { "No teams" });
            PlayerStatsTeamDropdown.onValueChanged.AddListener(SelectPlayerStatsTeamByDropdown);
            return;
        }

        List<string> options = new List<string>();
        for (int i = 0; i < teams.Count; i++)
        {
            options.Add(GetFullTeamName(teams[i]));
        }

        PlayerStatsTeamDropdown.AddOptions(options);
        int selectedIndex = GetSelectedPlayerStatsTeamIndex(teams);
        PlayerStatsTeamDropdown.SetValueWithoutNotify(selectedIndex < 0 ? 0 : selectedIndex);
        PlayerStatsTeamDropdown.RefreshShownValue();
        PlayerStatsTeamDropdown.onValueChanged.AddListener(SelectPlayerStatsTeamByDropdown);
    }

    private int GetSelectedPlayerStatsTeamIndex(List<TeamData> teams)
    {
        if (teams == null)
        {
            return -1;
        }

        for (int i = 0; i < teams.Count; i++)
        {
            TeamData team = teams[i];
            if (team != null && team.Id == _selectedPlayerStatsTeamId)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetMaxScheduleDay()
    {
        SeasonData season = GameSession.CurrentState == null ? null : GameSession.CurrentState.Season;
        if (season == null || season.Schedule == null)
        {
            return Mathf.Max(1, _selectedCalendarTargetDay);
        }

        int maxDay = 1;
        foreach (ScheduleGameData game in season.Schedule)
        {
            if (game != null && game.DayNumber > maxDay)
            {
                maxDay = game.DayNumber;
            }
        }

        return maxDay;
    }

    private static string GetFullTeamName(TeamData team)
    {
        if (team == null)
        {
            return "";
        }

        if (!string.IsNullOrEmpty(team.City) && !string.IsNullOrEmpty(team.Name))
        {
            return team.City + " " + team.Name;
        }

        return string.IsNullOrEmpty(team.Name) ? team.Id : team.Name;
    }

    private void RefreshPlayoffs()
    {
        SeasonData season = GameSession.CurrentState == null ? null : GameSession.CurrentState.Season;
        if (_playoffsController != null)
        {
            _playoffsController.ShowPlayoffs(season);
        }
    }

    private void UpdateTeamText()
    {
        if (_teamText == null)
        {
            return;
        }

        TeamData team = GameSession.CurrentTeam;
        if (team == null)
        {
            _teamText.text = "Команда не выбрана";
            if (CurrentTeamIdentityText != null)
            {
                CurrentTeamIdentityText.text = FictionalLeagueConfig.LeagueDisplayName;
            }

            if (CurrentTeamLogoImage != null)
            {
                CurrentTeamLogoImage.sprite = null;
                CurrentTeamLogoImage.color = Color.clear;
            }

            return;
        }

        TeamIdentityService.EnsureTeamIdentity(team);
        _teamText.text = TeamIdentityService.GetDisplayName(team);

        if (CurrentTeamIdentityText != null)
        {
            CurrentTeamIdentityText.text = TeamIdentityService.GetAbbreviation(team)
                + " | " + team.ConferenceName
                + " | " + team.DivisionName
                + "\nФорма: " + BuildLastFiveForm(GameSession.CurrentState, team.Id);
        }

        if (CurrentTeamLogoImage != null)
        {
            CurrentTeamLogoImage.sprite = TeamAssetService.LoadLogo(team);
            CurrentTeamLogoImage.color = CurrentTeamLogoImage.sprite == null
                ? TeamIdentityService.GetPrimaryColor(team)
                : Color.white;
            CurrentTeamLogoImage.preserveAspect = true;
        }
    }

    private void UpdateSeasonRulesText()
    {
        if (_seasonRulesText == null)
        {
            return;
        }

        if (UseCompactDashboardText())
        {
            TeamStandingData standing = FindCurrentTeamStanding(
                GameSession.CurrentState,
                GameSession.CurrentTeam == null ? "" : GameSession.CurrentTeam.Id);
            int wins = standing == null ? 0 : standing.Wins;
            int losses = standing == null ? 0 : standing.Losses;
            int overtimeLosses = standing == null ? 0 : standing.OvertimeLosses;
            int points = standing == null ? 0 : standing.Points;
            int gamesPlayed = standing == null ? 0 : standing.GamesPlayed;
            _seasonRulesText.text = "Победы: " + wins
                + "\nПоражения: " + losses
                + "\nОТ-поражения: " + overtimeLosses
                + "\nОчки: " + points
                + "\nИгр сыграно: " + gamesPlayed;
            return;
        }

        GameState state = GameSession.CurrentState;
        string seasonText = state == null
            ? "2026-27"
            : FormatSeason(state.CurrentSeasonStartYear, state.CurrentSeasonEndYear);
        int careerSeasonNumber = state == null ? 1 : state.CareerSeasonNumber;
        int historyCount = state == null || state.SeasonHistory == null ? 0 : state.SeasonHistory.Count;
        int developmentChangesCount = GetLastDevelopmentChangesCount(state);
        string lineupStatus = GetLineupStatusText();
        string tacticsStatus = GetTacticsStatusText(state);

        _seasonRulesText.text = "Сезон: " + seasonText
            + " | Сезон карьеры: " + careerSeasonNumber
            + " | Игр: " + SalaryCapConfig.TargetGamesPerTeam
            + "\nАрхивных сезонов: " + historyCount
            + "\nРазвитие игроков: " + developmentChangesCount + " изменений за последний сезон"
            + "\n" + lineupStatus
            + "\n" + tacticsStatus;
    }

    private void UpdateLeagueDateText()
    {
        if (_leagueDateText == null)
        {
            return;
        }

        if (UseCompactDashboardText())
        {
            _leagueDateText.text = "Дата: " + LeagueDateService.GetCurrentLeagueDate(GameSession.CurrentState).ToString("yyyy-MM-dd");
            return;
        }

        string deadline = GameSession.CurrentState == null || GameSession.CurrentState.LeagueCalendar == null
            ? ""
            : GameSession.CurrentState.LeagueCalendar.TradeDeadlineDate;
        _leagueDateText.text = "Дата лиги: " + LeagueDateService.GetCurrentLeagueDate(GameSession.CurrentState).ToString("yyyy-MM-dd")
            + " | Trade deadline: " + deadline;
    }

    private void UpdateTradeStatusText()
    {
        if (_tradeStatusText == null)
        {
            return;
        }

        if (UseCompactDashboardText())
        {
            _tradeStatusText.text = "";
            return;
        }

        _tradeStatusText.text = LeagueDateService.IsPastTradeDeadline(GameSession.CurrentState)
            ? "Обмены закрыты после дедлайна"
            : "Обмены доступны";
    }

    private void UpdateFreeAgencyStatusText()
    {
        if (_freeAgencyStatusText == null)
        {
            return;
        }

        if (UseCompactDashboardText())
        {
            _freeAgencyStatusText.text = "";
            return;
        }

        string freeAgencyStartDate = GameSession.CurrentState == null || GameSession.CurrentState.LeagueCalendar == null
            ? ""
            : GameSession.CurrentState.LeagueCalendar.FreeAgencyStartDate;
        string status = LeaguePhaseService.IsFreeAgencyOpen(GameSession.CurrentState)
            ? "Free agency: открыта"
            : "Free agency: закрыта";

        _freeAgencyStatusText.text = "FreeAgencyStartDate: " + freeAgencyStartDate
            + " | " + status;
    }

    private void UpdateFinanceText()
    {
        if (_financeText == null)
        {
            return;
        }

        if (UseCompactDashboardText())
        {
            _financeText.text = BuildDivisionStandingsSummary(GameSession.CurrentState, GameSession.CurrentTeam);
            return;
        }

        if (GameSession.CurrentTeam == null)
        {
            _financeText.text = "Финансы команды недоступны";
            return;
        }

        GameState state = GameSession.CurrentState;
        TeamData team = GameSession.CurrentTeam;
        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        OwnerProfileData ownerProfile = team.OwnerProfile;
        ClubFinanceData clubFinances = ownerProfile == null ? null : ownerProfile.Finances;
        TeamRosterSummaryData rosterSummary = TeamRosterService.GetRosterSummary(team);
        List<WaiverPlayerData> activeWaivers = state == null || state.WaiverWire == null || state.WaiverWire.ActiveWaivers == null
            ? new List<WaiverPlayerData>()
            : state.WaiverWire.ActiveWaivers;
        int currentTeamWaivers = CountCurrentTeamActiveWaivers(activeWaivers, team.Id);
        team.EnsureDraftRights();
        TeamMoraleSummaryData moraleSummary = BuildCachedTeamMoraleSummary(team);
        ContractExtensionSummaryData extensionSummary = GetCachedExtensionSummary(state, team);
        int freeAgentsCount = state == null || state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null
            ? 0
            : state.FreeAgentPool.FreeAgents.Count;
        int recentFreeAgencyOffers = state == null || state.FreeAgencyOfferHistory == null || state.FreeAgencyOfferHistory.Offers == null
            ? 0
            : Mathf.Min(5, state.FreeAgencyOfferHistory.Offers.Count);
        TeamLeadershipData leadership = team.LeadershipData;
        StaffEffectSummaryData staffSummary = team.Staff == null ? null : CoachingStaffService.BuildStaffEffectSummary(team);
        TeamChemistryData chemistry = team.Chemistry;
        int injuredPlayers = CountInjuredPlayers(team);
        LineupService.HasInjuredActivePlayers(team, out string injuredLineupMessage);
        TeamStandingData standing = FindCurrentTeamStanding(state, team.Id);
        string teamBadges = BuildFastDashboardBadges(finance, rosterSummary, injuredLineupMessage);
        string gmCareerSummary = GmCareerService.BuildCareerSummary(state);
        string text = "Зарплатная ведомость: " + FormatMoney(finance.Payroll)
            + " / " + FormatMoney(finance.SalaryCapUpperLimit) + "\n"
            + "Место под потолком: " + FormatMoney(finance.CapSpace) + "\n"
            + "Минимальный порог: " + FormatMoney(finance.SalaryCapLowerLimit) + "\n"
            + "Статусы: " + (string.IsNullOrEmpty(teamBadges) ? "нет предупреждений" : teamBadges) + "\n"
            + "Record: " + FormatStandingRecord(standing)
            + " | Место: " + FormatStandingRank(state, team.Id) + "\n"
            + gmCareerSummary + "\n"
            + FormatOwnerDashboard(ownerProfile, clubFinances) + "\n"
            + "Права на проспектов: " + team.DraftRights.Count + "\n"
            + "Pro roster: " + rosterSummary.NhlPlayers + " / " + RosterStatusConfig.MaxNhlRosterSize
            + " | Farm: " + rosterSummary.FarmPlayers
            + " | Reserve: " + rosterSummary.ReservePlayers + "\n"
            + "Waivers: " + activeWaivers.Count + " active"
            + " | Ваши игроки: " + currentTeamWaivers + "\n"
            + "Roster status: " + rosterSummary.ValidationMessage + "\n"
            + "Травмы: " + injuredPlayers + (string.IsNullOrEmpty(injuredLineupMessage) ? "" : " | " + injuredLineupMessage) + "\n"
            + "Средняя готовность состава: " + CalculateAverageActiveCondition(team) + "\n"
            + "Team morale: " + (moraleSummary == null ? 0 : moraleSummary.AverageMorale)
            + " | Unhappy players: " + (moraleSummary == null ? 0 : moraleSummary.UnhappyPlayers + moraleSummary.VeryUnhappyPlayers)
            + " | Trade requests: " + (moraleSummary == null ? 0 : moraleSummary.TradeRequests) + "\n"
            + "Lowest morale: " + FormatLowestMorale(moraleSummary) + "\n"
            + "Истекающие контракты: " + (extensionSummary == null ? 0 : extensionSummary.EligiblePlayers)
            + " | UFA: " + (extensionSummary == null ? 0 : extensionSummary.PendingUfaCount)
            + " | RFA: " + (extensionSummary == null ? 0 : extensionSummary.PendingRfaCount)
            + " | Низкий интерес: " + (extensionSummary == null ? 0 : extensionSummary.LowInterestCount) + "\n"
            + "Free agency: " + freeAgentsCount
            + " available | recent offers: " + recentFreeAgencyOffers + "\n"
            + FormatLeadershipDashboard(leadership) + "\n"
            + FormatStaffDashboard(staffSummary) + "\n"
            + FormatChemistryDashboard(chemistry) + "\n"
            + "Самый уставший: " + FormatMostFatiguedPlayer(FindMostFatiguedPlayer(team)) + "\n"
            + "Стартовый вратарь: " + GetStartingGoalieCondition(team);

        if (finance.IsOverCap)
        {
            text += "\nВнимание: команда выше потолка зарплат";
        }

        if (finance.IsBelowFloor)
        {
            text += "\nВнимание: команда ниже минимального порога зарплат";
        }

        string claimedWarning = FormatLatestClaimedPlayerWarning(team.Id);
        if (!string.IsNullOrEmpty(claimedWarning))
        {
            text += "\n" + claimedWarning;
        }

        _financeText.text = text;
    }

    private void UpdateCpuRosterAiText()
    {
        if (_cpuRosterAiText == null)
        {
            return;
        }

        if (UseCompactDashboardText())
        {
            _cpuRosterAiText.text = BuildTopPlayersSummary(GameSession.CurrentTeam);
            return;
        }

        string tradeProfileSummary = BuildTradeProfileSummary();
        string scoutingSummary = BuildScoutingDashboardSummary();
        string historySummary = BuildLeagueHistoryDashboardSummary();
        string diagnosticsSummary = BuildDiagnosticsDashboardSummary();
        string dashboardGroupSummary = BuildDashboardGroupSummary();
        string tutorialSummary = BuildCachedTutorialSummary();
        CpuRosterManagementReportData report = GameSession.CurrentState == null
            ? null
            : GameSession.CurrentState.LastCpuRosterManagementReport;
        if (report == null)
        {
            _cpuRosterAiText.text = dashboardGroupSummary + "\n" + tutorialSummary + "\nCPU roster AI: отчётов пока нет\n" + diagnosticsSummary + "\n" + tradeProfileSummary + "\n" + scoutingSummary + "\n" + historySummary;
            return;
        }

        report.EnsureActions();
        string text = "CPU roster AI: checked " + report.TeamsChecked
            + " teams, changed " + report.TeamsChanged
            + ", actions " + report.ActionsCount;

        if (report.Actions == null || report.Actions.Count == 0)
        {
            _cpuRosterAiText.text = dashboardGroupSummary + "\n" + tutorialSummary + "\n" + text + "\nПоследние действия: нет\n" + diagnosticsSummary + "\n" + tradeProfileSummary + "\n" + scoutingSummary + "\n" + historySummary;
            return;
        }

        int shown = 0;
        for (int i = report.Actions.Count - 1; i >= 0 && shown < 5; i--)
        {
            CpuRosterActionData action = report.Actions[i];
            if (action == null)
            {
                continue;
            }

            text += "\n" + FormatCpuRosterAction(action);
            shown++;
        }

        _cpuRosterAiText.text = dashboardGroupSummary + "\n" + tutorialSummary + "\n" + text + "\n" + diagnosticsSummary + "\n" + tradeProfileSummary + "\n" + scoutingSummary + "\n" + historySummary;
    }

    private static bool UseCompactDashboardText()
    {
        return true;
    }

    private static string BuildDivisionStandingsSummary(GameState state, TeamData currentTeam)
    {
        if (state == null || state.Season == null || currentTeam == null || state.Season.Standings == null)
        {
            return "Таблица пока недоступна";
        }

        string divisionName = currentTeam.DivisionName;
        List<TeamStandingData> divisionStandings = new List<TeamStandingData>();
        foreach (TeamStandingData standing in state.Season.Standings)
        {
            if (standing == null)
            {
                continue;
            }

            TeamData standingTeam = FindTeam(state, standing.TeamId);
            if (standingTeam != null && standingTeam.DivisionName == divisionName)
            {
                divisionStandings.Add(standing);
            }
        }

        if (divisionStandings.Count == 0)
        {
            return "В дивизионе пока нет данных";
        }

        divisionStandings.Sort(CompareStandingsForDashboard);
        string text = "";
        for (int i = 0; i < divisionStandings.Count && i < 8; i++)
        {
            TeamStandingData standing = divisionStandings[i];
            TeamData standingTeam = FindTeam(state, standing.TeamId);
            string prefix = standing.TeamId == currentTeam.Id ? "> " : "  ";
            string teamName = standingTeam == null ? standing.TeamName : TeamIdentityService.GetDisplayName(standingTeam);
            string line = prefix
                + (i + 1) + ". "
                + teamName + " "
                + FormatStandingRecord(standing)
                + "  " + standing.Points + " очк.";
            if (i < 3)
            {
                line = "<color=#7CFFB2>" + line + "</color>";
            }

            text += line;
            if (i < divisionStandings.Count - 1 && i < 7)
            {
                text += "\n";
            }
        }

        return text;
    }

    private static string BuildTopPlayersSummary(TeamData team)
    {
        if (team == null || team.Players == null || team.Players.Count == 0)
        {
            return "Игроки пока не найдены";
        }

        List<PlayerSeasonStatsData> stats = PlayerStatsService.GetTeamSkaterStats(
            GameSession.CurrentState == null ? null : GameSession.CurrentState.Season,
            team.Id);
        if (stats.Count > 0)
        {
            string statsText = "";
            int shownStats = 0;
            for (int i = 0; i < stats.Count && shownStats < 5; i++)
            {
                PlayerSeasonStatsData playerStats = stats[i];
                if (playerStats == null || playerStats.GamesPlayed <= 0)
                {
                    continue;
                }

                statsText += (shownStats + 1) + ". "
                    + SafeDashboardText(playerStats.PlayerName)
                    + "\n   " + playerStats.Position
                    + " | И " + playerStats.GamesPlayed
                    + " | " + playerStats.Goals + "G "
                    + playerStats.Assists + "A "
                    + playerStats.Points + "P"
                    + " | +/- " + playerStats.PlusMinus
                    + " | Бр " + playerStats.Shots;
                if (shownStats < 4)
                {
                    statsText += "\n";
                }

                shownStats++;
            }

            if (shownStats > 0)
            {
                return statsText.TrimEnd();
            }
        }

        List<PlayerData> players = new List<PlayerData>();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && !player.IsRetired)
            {
                players.Add(player);
            }
        }

        if (players.Count == 0)
        {
            return "Игроки пока не найдены";
        }

        players.Sort(ComparePlayersForDashboard);
        string text = "";
        for (int i = 0; i < players.Count && i < 5; i++)
        {
            PlayerData player = players[i];
            PlayerFatigueService.EnsureFatigueFields(player);
            text += (i + 1) + ". "
                + GetPlayerDisplayName(player)
                + "\n   " + player.Position
                + " | OVR " + player.Overall
                + " | POT " + player.Potential
                + " | Возр. " + player.Age
                + " | COND " + player.Condition;
            if (i < players.Count - 1 && i < 4)
            {
                text += "\n";
            }
        }

        return text;
    }

    private static string BuildLastFiveForm(GameState state, string teamId)
    {
        if (state == null || state.MatchHistory == null || string.IsNullOrEmpty(teamId))
        {
            return "-----";
        }

        List<string> results = new List<string>();
        for (int i = state.MatchHistory.Count - 1; i >= 0 && results.Count < 5; i--)
        {
            MatchResultData result = state.MatchHistory[i];
            if (result == null || (result.HomeTeamId != teamId && result.AwayTeamId != teamId))
            {
                continue;
            }

            bool won = result.WinnerTeamId == teamId;
            results.Insert(0, won ? "<color=#7CFFB2>W</color>" : "<color=#FF7C7C>L</color>");
        }

        while (results.Count < 5)
        {
            results.Insert(0, "<color=#AAB3C5>-</color>");
        }

        return string.Join("", results.ToArray());
    }

    private static int ComparePlayersForDashboard(PlayerData left, PlayerData right)
    {
        int overall = (right == null ? 0 : right.Overall).CompareTo(left == null ? 0 : left.Overall);
        if (overall != 0)
        {
            return overall;
        }

        int potential = (right == null ? 0 : right.Potential).CompareTo(left == null ? 0 : left.Potential);
        if (potential != 0)
        {
            return potential;
        }

        return (left == null ? 99 : left.Age).CompareTo(right == null ? 99 : right.Age);
    }

    private static string GetPlayerDisplayName(PlayerData player)
    {
        if (player == null)
        {
            return "Игрок";
        }

        string name = (player.FirstName + " " + player.LastName).Trim();
        return string.IsNullOrEmpty(name) ? player.Id : name;
    }

    private static string BuildDiagnosticsDashboardSummary()
    {
        GameState state = GameSession.CurrentState;
        GameStateValidationReportData validation = state == null ? null : state.LastValidationReport;
        BalanceReportData balance = state == null ? null : state.LastBalanceReport;
        AlphaBalanceReportData alpha = state == null ? null : state.LastAlphaBalanceReport;
        if (validation == null && balance == null && alpha == null)
        {
            return "Diagnostics: not run";
        }

        int critical = validation == null ? 0 : validation.CriticalCount;
        int warnings = validation == null ? 0 : validation.WarningsCount;
        int invalidLineups = balance == null ? 0 : balance.InvalidLineupTeams;
        int alphaWarnings = alpha == null ? 0 : alpha.WarningCount;
        int alphaCritical = alpha == null ? 0 : alpha.CriticalCount;
        return "Diagnostics: critical " + critical
            + " | warnings " + warnings
            + " | invalid lineups " + invalidLineups
            + " | alpha W/C " + alphaWarnings + "/" + alphaCritical;
    }

    private static void LogScoutingResult(ScoutingActionResultData result)
    {
        if (result == null)
        {
            Debug.LogWarning("Скаутинг не выполнен");
            return;
        }

        if (result.Success)
        {
            Debug.Log(result.Message);
        }
        else
        {
            Debug.LogWarning(result.Message);
        }
    }

    private static void LogAlphaMultiSeasonReport(AlphaBalanceReportData report)
    {
        if (report == null)
        {
            Debug.LogWarning("Alpha balance report not created");
            return;
        }

        if (!report.SimulatedDuringReport)
        {
            Debug.LogWarning(report.Recommendation);
            return;
        }

        Debug.Log(report.Summary);
    }

    private static string BuildTradeProfileSummary()
    {
        GameState state = GameSession.CurrentState;
        TeamData team = GameSession.CurrentTeam;
        TeamTradeProfileData profile = null;
        if (state != null && team != null && state.TeamTradeProfiles != null)
        {
            foreach (TeamTradeProfileData candidate in state.TeamTradeProfiles)
            {
                if (candidate != null && candidate.TeamId == team.Id)
                {
                    profile = candidate;
                    break;
                }
            }
        }

        if (profile == null || profile.Needs == null)
        {
            return "Trade AI profiles: нет данных";
        }

        string updatedAt = string.IsNullOrEmpty(profile.UpdatedAtUtc)
            ? "неизвестно"
            : profile.UpdatedAtUtc;

        return "Trade AI profiles: updated " + updatedAt
            + " | Direction: " + profile.Direction
            + " | Need: " + profile.Needs.PrimaryNeed;
    }

    private static string BuildScoutingDashboardSummary()
    {
        GameState state = GameSession.CurrentState;
        int reportCount = state == null || state.ScoutingHistory == null || state.ScoutingHistory.Reports == null
            ? 0
            : state.ScoutingHistory.Reports.Count;
        string lastAction = state == null || state.ScoutingHistory == null || string.IsNullOrEmpty(state.ScoutingHistory.LastScoutingActionAtUtc)
            ? "нет"
            : state.ScoutingHistory.LastScoutingActionAtUtc;
        int averageAccuracy = CalculateDraftClassAverageScoutingAccuracy(state);
        string draftClassSummary = state != null
            && state.Draft != null
            && state.Draft.Prospects != null
            && state.Draft.Prospects.Count > 0
                ? GameSession.GetCurrentDraftClassSummary()
                : "Draft class: not generated";
        return "Scouting reports: " + reportCount
            + " | Last scouting action: " + lastAction
            + " | Draft class scouted: average accuracy " + averageAccuracy + "%"
            + "\n" + draftClassSummary;
    }

    private static string BuildLeagueHistoryDashboardSummary()
    {
        GameState state = GameSession.CurrentState;
        LeagueSeasonHistoryData lastHistory = state == null ? null : state.LastLeagueSeasonHistory;
        string retirementSummary = BuildRetirementDashboardSummary();
        if (lastHistory == null)
        {
            return "History: сезоны пока не сохранены | " + retirementSummary;
        }

        SeasonAwardsData awards = state == null ? null : state.LastSeasonAwards;
        AwardWinnerData mvp = FindDashboardAward(awards, AwardsConfig.LeagueMvp);
        string mvpName = mvp == null ? lastHistory.MvpPlayerName : mvp.PlayerName;

        return "History: " + FormatSeasonLabel(lastHistory.SeasonStartYear, lastHistory.SeasonEndYear)
            + " champion " + SafeDashboardText(lastHistory.ChampionTeamName)
            + " | MVP " + SafeDashboardText(mvpName)
            + " | top scorer " + SafeDashboardText(lastHistory.TopScorerPlayerName)
            + " " + lastHistory.TopScorerPoints + "P"
            + " | user " + SafeDashboardText(lastHistory.UserTeamResult)
            + "\n" + retirementSummary;
    }

    private static string BuildRetirementDashboardSummary()
    {
        GameState state = GameSession.CurrentState;
        int retiredCount = state == null || state.RetiredPlayers == null || state.RetiredPlayers.Players == null
            ? 0
            : state.RetiredPlayers.Players.Count;
        int hallOfFameCount = state == null || state.HallOfFame == null || state.HallOfFame.Inductees == null
            ? 0
            : state.HallOfFame.Inductees.Count;
        int retiredNumberCount = state == null || state.LeagueRetiredNumbers == null
            ? 0
            : state.LeagueRetiredNumbers.Count;

        return "Retired " + retiredCount
            + " | HOF " + hallOfFameCount
            + " | retired numbers " + retiredNumberCount;
    }

    private static string BuildLatestNewsDashboardSummary()
    {
        GameState state = GameSession.CurrentState;
        List<NewsItemData> newsItems = state == null || state.NewsFeed == null
            ? null
            : state.NewsFeed.Items;
        if (newsItems == null || newsItems.Count == 0)
        {
            return "News: пока нет";
        }

        string text = "News:";
        int shown = 0;
        for (int i = newsItems.Count - 1; i >= 0 && shown < 3; i--)
        {
            NewsItemData item = newsItems[i];
            if (item == null)
            {
                continue;
            }

            text += "\n" + SafeDashboardText(item.Category)
                + ": " + SafeDashboardText(item.Title);
            shown++;
        }

        return text;
    }

    private static string BuildCachedTutorialSummary()
    {
        GameState state = GameSession.CurrentState;
        TutorialData tutorial = state == null ? null : state.Tutorial;
        if (tutorial == null)
        {
            return "Tutorial unavailable";
        }

        if (!tutorial.IsTutorialEnabled)
        {
            return "Tutorial disabled";
        }

        int completed = tutorial.CompletedStepIds == null ? 0 : tutorial.CompletedStepIds.Count;
        if (tutorial.HasCompletedChecklist)
        {
            return "Tutorial completed";
        }

        return "Tutorial: " + completed + " completed";
    }

    private string BuildDashboardGroupSummary()
    {
        return "Dashboard group: " + GetDashboardGroupLabel(_selectedDashboardGroup)
            + "\n" + GetDashboardGroupButtonsHint(_selectedDashboardGroup);
    }

    private void UpdateDashboardNavigation()
    {
        if (_dashboardPanel == null)
        {
            return;
        }

        SetDashboardGroupActive("DashboardMainActions", _selectedDashboardGroup == "Main");
        SetDashboardGroupActive("DashboardTeamActions", _selectedDashboardGroup == "Team");
        SetDashboardGroupActive("DashboardSeasonActions", _selectedDashboardGroup == "Season");
        SetDashboardGroupActive("DashboardOfficeActions", _selectedDashboardGroup == "Office");
        SetDashboardGroupActive("DashboardMarketActions", _selectedDashboardGroup == "Market");
        SetDashboardGroupActive("DashboardHistoryActions", _selectedDashboardGroup == "History");
        SetDashboardGroupActive("DashboardSystemActions", true);
        SetDashboardText("DashboardAlertsContainer", BuildDashboardAlertsSummary());
        SetDashboardText("DashboardLatestNewsText", BuildLatestNewsDashboardSummary());
    }

    private void SetDashboardGroupActive(string groupName, bool isActive)
    {
        Transform group = FindDashboardChild(groupName);
        if (group != null)
        {
            group.gameObject.SetActive(isActive);
        }
    }

    private void SetDashboardText(string objectName, string value)
    {
        Transform textTransform = FindDashboardChild(objectName);
        Text text = textTransform == null ? null : textTransform.GetComponent<Text>();
        if (text != null)
        {
            text.text = value;
        }
    }

    private Transform FindDashboardChild(string objectName)
    {
        if (_dashboardPanel == null || string.IsNullOrEmpty(objectName))
        {
            return null;
        }

        return _dashboardPanel.transform.Find(objectName);
    }

    private static string BuildDashboardAlertsSummary()
    {
        GameState state = GameSession.CurrentState;
        TeamData team = GameSession.CurrentTeam;
        if (state == null || team == null)
        {
            return "Alerts: нет критичных предупреждений";
        }

        List<string> alerts = new List<string>();
        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        if (finance != null && finance.IsOverCap)
        {
            alerts.Add("[Cap] Over cap - " + FormatMoney(finance.Payroll) + " / " + FormatMoney(finance.SalaryCapUpperLimit));
        }

        TeamRosterSummaryData rosterSummary = TeamRosterService.GetRosterSummary(team);
        if (rosterSummary != null && !rosterSummary.IsNhlRosterValid)
        {
            alerts.Add("[Roster] NHL roster - " + MobileUiConfig.FormatShortStatus(rosterSummary.ValidationMessage));
        }

        if (!LineupService.ValidateLineup(team, out string lineupMessage))
        {
            alerts.Add("[Lineup] Invalid lineup - " + MobileUiConfig.FormatShortStatus(lineupMessage));
        }

        if (LineupService.HasInjuredActivePlayers(team, out string injuredLineupMessage))
        {
            alerts.Add("[Injuries] Active injured player - " + MobileUiConfig.FormatShortStatus(injuredLineupMessage));
        }

        OwnerProfileData ownerProfile = team.OwnerProfile;
        if (ownerProfile != null && ownerProfile.GmTrust <= OwnerGoalConfig.DangerTrustThreshold)
        {
            alerts.Add("[Owner] Job security - " + SafeDashboardText(ownerProfile.JobSecurity));
        }

        if (alerts.Count == 0)
        {
            return "Alerts: нет критичных предупреждений";
        }

        string text = "Alerts:";
        for (int i = 0; i < alerts.Count && i < MobileUiConfig.MaxDashboardAlerts; i++)
        {
            text += "\n" + alerts[i];
        }

        return text;
    }

    private static string GetDashboardGroupLabel(string group)
    {
        if (group == "Team")
        {
            return "Команда";
        }

        if (group == "Season")
        {
            return "Сезон";
        }

        if (group == "Office")
        {
            return "Офис";
        }

        if (group == "Market")
        {
            return "Рынок";
        }

        if (group == "History")
        {
            return "История";
        }

        return "Main";
    }

    private static string GetDashboardGroupButtonsHint(string group)
    {
        if (group == "Team")
        {
            return "Команда: Состав | Организация | Линии | Роли | Мораль | Травмы | Капитаны | Тренеры";
        }

        if (group == "Season")
        {
            return "Сезон: Календарь | Таблица | Статистика | Плей-офф | Тактика";
        }

        if (group == "Office")
        {
            return "Офис: Контракты | Продления | Владелец | Развитие";
        }

        if (group == "Market")
        {
            return "Рынок: Обмены | FA | Waivers | Драфт | Скаутинг | Права";
        }

        if (group == "History")
        {
            return "История: Новости | История | Межсезонье";
        }

        return "Main: выберите группу или используйте системные действия ниже";
    }

    private static AwardWinnerData FindDashboardAward(SeasonAwardsData awards, string awardType)
    {
        if (awards == null || awards.Awards == null)
        {
            return null;
        }

        foreach (AwardWinnerData award in awards.Awards)
        {
            if (award != null && award.AwardType == awardType)
            {
                return award;
            }
        }

        return null;
    }

    private static string FormatSeasonLabel(int startYear, int endYear)
    {
        return startYear + "-" + (endYear % 100).ToString("D2");
    }

    private static string SafeDashboardText(string value)
    {
        return string.IsNullOrEmpty(value) ? "нет данных" : value;
    }

    private void UpdateGamesSimulatedText()
    {
        if (_gamesSimulatedText == null)
        {
            return;
        }

        _gamesSimulatedText.text = "";
    }

    private void UpdateCurrentDayText()
    {
        if (_currentDayText == null)
        {
            return;
        }

        _currentDayText.text = "";
    }

    private void UpdateNextGameText()
    {
        if (_nextGameText == null)
        {
            return;
        }

        ScheduleGameData nextGame = GameSession.GetNextGameForCurrentTeam();
        if (nextGame != null)
        {
            _nextGameText.text = "Следующий матч: " + nextGame.HomeTeamName + " vs " + nextGame.AwayTeamName;
            return;
        }

        PlayoffData playoffs = GameSession.CurrentState == null || GameSession.CurrentState.Season == null
            ? null
            : GameSession.CurrentState.Season.Playoffs;

        if (playoffs != null && playoffs.IsCompleted)
        {
            _nextGameText.text = "Чемпион: " + playoffs.ChampionTeamName;
            return;
        }

        _nextGameText.text = GameSession.CanStartPlayoffs()
            ? "Регулярный сезон завершён. Доступен плей-офф."
            : "Сезон завершён";
    }

    private void UpdateLastMatchResultText()
    {
        if (_lastMatchResultText == null)
        {
            return;
        }

        MatchResultData lastResult = GameSession.CurrentState == null ? null : GameSession.CurrentState.LastMatchResult;
        _lastMatchResultText.text = lastResult == null
            ? "Матчей ещё не было"
            : "Последний матч: " + lastResult.Summary;
    }

    private void SetNoAvailableMatchText()
    {
        if (_lastMatchResultText == null)
        {
            return;
        }

        bool seasonFinished = GameSession.CurrentState != null
            && GameSession.CurrentState.Season != null
            && GameSession.CurrentState.Season.IsSeasonFinished;

        if (seasonFinished)
        {
            _lastMatchResultText.text = "Сезон завершён";
            return;
        }

        _lastMatchResultText.text = GameSession.GetNextGameForCurrentTeam() == null
            ? "Нет доступных матчей"
            : "Матч не сыгран: проверьте доступных игроков";
    }

    private static List<TradeAssetData> BuildTradeAssets(TeamData team, string playerId, string pickId)
    {
        List<TradeAssetData> assets = new List<TradeAssetData>();
        if (team != null && !string.IsNullOrEmpty(playerId))
        {
            PlayerData player = FindPlayer(team, playerId);
            TradeAssetData playerAsset = TradeService.CreatePlayerAsset(player, team);
            if (playerAsset != null)
            {
                assets.Add(playerAsset);
            }
        }

        if (!string.IsNullOrEmpty(pickId))
        {
            DraftPickOwnershipData pick = DraftPickOwnershipService.FindPick(GameSession.CurrentState, pickId);
            TradeAssetData pickAsset = TradeService.CreateDraftPickAsset(pick);
            if (pickAsset != null)
            {
                assets.Add(pickAsset);
            }
        }

        return assets;
    }

    private static TeamData FindTeam(GameState state, string teamId)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
    }

    private static TeamStandingData FindCurrentTeamStanding(GameState state, string teamId)
    {
        if (state == null || state.Season == null || state.Season.Standings == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamStandingData standing in state.Season.Standings)
        {
            if (standing != null && standing.TeamId == teamId)
            {
                return standing;
            }
        }

        return null;
    }

    private static string FormatStandingRecord(TeamStandingData standing)
    {
        return standing == null
            ? "0-0-0"
            : MobileUiConfig.FormatRecord(standing.Wins, standing.Losses, standing.OvertimeLosses);
    }

    private static string FormatStandingRank(GameState state, string teamId)
    {
        if (state == null || state.Season == null || state.Season.Standings == null || string.IsNullOrEmpty(teamId))
        {
            return "n/a";
        }

        List<TeamStandingData> standings = new List<TeamStandingData>(state.Season.Standings);
        standings.Sort(CompareStandingsForDashboard);
        for (int i = 0; i < standings.Count; i++)
        {
            TeamStandingData standing = standings[i];
            if (standing != null && standing.TeamId == teamId)
            {
                return (i + 1).ToString();
            }
        }

        return "n/a";
    }

    private static int CompareStandingsForDashboard(TeamStandingData left, TeamStandingData right)
    {
        int rightPoints = right == null ? 0 : right.Points;
        int leftPoints = left == null ? 0 : left.Points;
        int points = rightPoints.CompareTo(leftPoints);
        if (points != 0)
        {
            return points;
        }

        int rightWins = right == null ? 0 : right.Wins;
        int leftWins = left == null ? 0 : left.Wins;
        return rightWins.CompareTo(leftWins);
    }

    private static PlayerData FindPlayer(TeamData team, string playerId)
    {
        if (team == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }

    private static string FormatLowestMorale(TeamMoraleSummaryData summary)
    {
        if (summary == null || string.IsNullOrEmpty(summary.LowestMoralePlayerName))
        {
            return "нет данных";
        }

        return summary.LowestMoralePlayerName + " - " + summary.LowestMorale;
    }

    private static string FormatOwnerDashboard(OwnerProfileData profile, ClubFinanceData finances)
    {
        if (profile == null)
        {
            return "Owner goals: нет данных";
        }

        string primaryGoal = "none";
        if (profile.CurrentGoals != null)
        {
            foreach (OwnerGoalData goal in profile.CurrentGoals)
            {
                if (goal != null && goal.GoalType == OwnerGoalConfig.GoalTypePrimary)
                {
                    primaryGoal = goal.Title + " " + goal.ProgressPercent + "%";
                    break;
                }
            }
        }

        string financialHealth = finances == null || string.IsNullOrEmpty(finances.FinancialHealthLabel)
            ? "нет данных"
            : finances.FinancialHealthLabel;
        return "Owner goal: " + primaryGoal
            + "\nGM trust: " + profile.GmTrust
            + " | Job security: " + profile.JobSecurity
            + " | Satisfaction: " + profile.OwnerSatisfaction
            + " | Finance: " + financialHealth;
    }

    private static string FormatLeadershipDashboard(TeamLeadershipData leadership)
    {
        if (leadership == null)
        {
            return "Captain: нет данных";
        }

        string captain = string.IsNullOrEmpty(leadership.CaptainName)
            ? "No captain assigned"
            : leadership.CaptainName;
        string alternates = FormatCaptainName(leadership.Alternate1Name)
            + " / " + FormatCaptainName(leadership.Alternate2Name);
        string warning = string.IsNullOrEmpty(leadership.CaptainName) ? " | WARNING: no captain" : "";
        return "Captain: " + captain
            + " | A: " + alternates
            + "\nLeadership: " + leadership.LeadershipLabel
            + " (" + FormatRatingModifier(leadership.MoraleImpact) + " morale, "
            + FormatRatingModifier(leadership.ChemistryImpact) + " chemistry)"
            + warning;
    }

    private static string FormatCaptainName(string value)
    {
        return string.IsNullOrEmpty(value) ? "none" : value;
    }

    private static string FormatStaffDashboard(StaffEffectSummaryData staff)
    {
        if (staff == null)
        {
            return "Head Coach: нет данных";
        }

        string coachName = string.IsNullOrEmpty(staff.HeadCoachName) ? "none" : staff.HeadCoachName;
        return "Head Coach: " + coachName + " - " + staff.CoachingStyle
            + "\nStaff: Off " + FormatRatingModifier(staff.OffenseModifier)
            + ", Def " + FormatRatingModifier(staff.DefenseModifier)
            + ", Dev " + FormatRatingModifier(staff.DevelopmentModifier)
            + ", Morale " + FormatRatingModifier(staff.MoraleModifier)
            + ", Chemistry " + FormatRatingModifier(staff.ChemistryModifier);
    }

    private static string FormatChemistryDashboard(TeamChemistryData chemistry)
    {
        if (chemistry == null)
        {
            return "Team chemistry: нет данных";
        }

        int modifier = ChemistryConfig.GetTeamRatingModifier(chemistry.TeamChemistryScore);
        return "Team chemistry: " + chemistry.TeamChemistryScore + " " + chemistry.TeamChemistryLabel
            + " | Mod " + FormatRatingModifier(modifier)
            + "\nBest unit: " + chemistry.BestUnitName + " " + chemistry.BestUnitScore
            + " | Worst unit: " + chemistry.WorstUnitName + " " + chemistry.WorstUnitScore;
    }

    private static string FormatRatingModifier(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }

    private static int CountCurrentTeamActiveWaivers(List<WaiverPlayerData> waivers, string teamId)
    {
        if (waivers == null || string.IsNullOrEmpty(teamId))
        {
            return 0;
        }

        int count = 0;
        foreach (WaiverPlayerData waiver in waivers)
        {
            if (waiver != null
                && waiver.OriginalTeamId == teamId
                && waiver.Status == WaiverConfig.WaiverWireStatusActive)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountInjuredPlayers(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return 0;
        }

        int count = 0;
        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.IsInjured)
            {
                count++;
            }
        }

        return count;
    }

    private static TeamMoraleSummaryData BuildCachedTeamMoraleSummary(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return null;
        }

        TeamMoraleSummaryData summary = new TeamMoraleSummaryData
        {
            TeamId = team.Id,
            TeamName = TeamIdentityService.GetDisplayName(team),
            LowestMorale = 100
        };

        int total = 0;
        int count = 0;
        foreach (PlayerData player in team.Players)
        {
            if (player == null || player.IsRetired)
            {
                continue;
            }

            int morale = player.Morale <= 0 ? MoraleConfig.DefaultMorale : player.Morale;
            total += morale;
            count++;

            if (morale < summary.LowestMorale)
            {
                summary.LowestMorale = morale;
                summary.LowestMoralePlayerId = player.Id;
                summary.LowestMoralePlayerName = player.FirstName + " " + player.LastName;
            }

            if (player.WantsTrade)
            {
                summary.TradeRequests++;
            }

            string status = string.IsNullOrEmpty(player.MoraleStatus)
                ? MoraleConfig.GetMoraleStatus(morale)
                : player.MoraleStatus;
            if (status == MoraleConfig.StatusHappy)
            {
                summary.HappyPlayers++;
            }
            else if (status == MoraleConfig.StatusConcerned)
            {
                summary.ConcernedPlayers++;
            }
            else if (status == MoraleConfig.StatusUnhappy)
            {
                summary.UnhappyPlayers++;
            }
            else if (status == MoraleConfig.StatusVeryUnhappy)
            {
                summary.VeryUnhappyPlayers++;
            }
            else
            {
                summary.ContentPlayers++;
            }
        }

        summary.AverageMorale = count == 0 ? 0 : total / count;
        if (count == 0)
        {
            summary.LowestMorale = 0;
        }

        return summary;
    }

    private static ContractExtensionSummaryData GetCachedExtensionSummary(GameState state, TeamData team)
    {
        return state == null || team == null
            ? null
            : ContractExtensionService.BuildSummary(state, team);
    }

    private static string BuildFastDashboardBadges(
        TeamFinanceData finance,
        TeamRosterSummaryData rosterSummary,
        string injuredLineupMessage)
    {
        List<string> badges = new List<string>();
        if (finance != null && finance.IsOverCap)
        {
            badges.Add("Over cap");
        }

        if (finance != null && finance.IsBelowFloor)
        {
            badges.Add("Below floor");
        }

        if (rosterSummary != null && !rosterSummary.IsNhlRosterValid)
        {
            badges.Add("Roster");
        }

        if (!string.IsNullOrEmpty(injuredLineupMessage))
        {
            badges.Add("Injured lineup");
        }

        return badges.Count == 0 ? "" : string.Join(" | ", badges.ToArray());
    }

    private static string FormatLatestClaimedPlayerWarning(string teamId)
    {
        if (GameSession.CurrentState == null
            || GameSession.CurrentState.WaiverWire == null
            || GameSession.CurrentState.WaiverWire.WaiverHistory == null
            || string.IsNullOrEmpty(teamId))
        {
            return "";
        }

        for (int i = GameSession.CurrentState.WaiverWire.WaiverHistory.Count - 1; i >= 0; i--)
        {
            WaiverPlayerData waiver = GameSession.CurrentState.WaiverWire.WaiverHistory[i];
            if (waiver != null
                && waiver.OriginalTeamId == teamId
                && waiver.Status == WaiverConfig.WaiverWireStatusClaimed)
            {
                return "Внимание: игрок с waivers был забран: " + waiver.PlayerName
                    + " -> " + waiver.ClaimedByTeamName;
            }
        }

        return "";
    }

    private static string FormatCpuRosterAction(CpuRosterActionData action)
    {
        string team = string.IsNullOrEmpty(action.TeamName) ? action.TeamId : action.TeamName;
        string player = string.IsNullOrEmpty(action.PlayerName) ? "" : " " + action.PlayerName;
        string message = string.IsNullOrEmpty(action.Message) ? action.Reason : action.Message;

        if (message != null && message.Length > 52)
        {
            message = message.Substring(0, 52) + "...";
        }

        return team + ": " + action.ActionType + player + " - " + message;
    }

    private static int CalculateDraftClassAverageScoutingAccuracy(GameState state)
    {
        List<ProspectData> prospects = ScoutingService.GetDraftClassProspects(state);
        if (prospects.Count == 0)
        {
            return 0;
        }

        int total = 0;
        int count = 0;
        foreach (ProspectData prospect in prospects)
        {
            if (prospect == null)
            {
                continue;
            }

            total += prospect.ScoutingAccuracy;
            count++;
        }

        return count == 0 ? 0 : Mathf.RoundToInt((float)total / count);
    }

    private static string FormatSeason(int startYear, int endYear)
    {
        return startYear + "-" + (endYear % 100).ToString("D2");
    }

    private static int GetLastDevelopmentChangesCount(GameState state)
    {
        if (state == null || state.PlayerDevelopmentHistory == null || state.PlayerDevelopmentHistory.Changes == null)
        {
            return 0;
        }

        int processedSeason = state.PlayerDevelopmentHistory.LastProcessedSeasonStartYear;
        if (processedSeason <= 0)
        {
            return 0;
        }

        int count = 0;
        foreach (PlayerDevelopmentChangeData change in state.PlayerDevelopmentHistory.Changes)
        {
            if (change != null && change.SeasonStartYear == processedSeason)
            {
                count++;
            }
        }

        return count;
    }

    private static string GetLineupStatusText()
    {
        TeamData team = GameSession.CurrentTeam;
        if (team == null)
        {
            return "Состав на матч: команда не выбрана";
        }

        GameSession.ValidateCurrentTeamLineup(out string message);
        bool isValid = team.Lineup != null && team.Lineup.IsValid;
        PlayerData starter = LineupService.GetStartingGoalie(team);
        if (starter != null)
        {
            PlayerFatigueService.EnsureFatigueFields(starter);
        }

        string starterText = starter == null
            ? "нет"
            : starter.FirstName + " " + starter.LastName + " (" + starter.Overall + ", COND " + starter.Condition + ")";
        string manualText = team.Lineup != null && team.Lineup.IsManual ? "да" : "нет";
        string manualDate = team.Lineup != null && team.Lineup.IsManual && !string.IsNullOrEmpty(team.Lineup.LastManualUpdateUtc)
            ? " | Обновлен: " + team.Lineup.LastManualUpdateUtc
            : "";

        return "Состав на матч: " + (isValid ? "валиден" : "требует исправления")
            + " | Ручной состав: " + manualText + manualDate
            + " | O:" + TeamRatingCalculator.CalculateOffenseRating(team)
            + " D:" + TeamRatingCalculator.CalculateDefenseRating(team)
            + " G:" + TeamRatingCalculator.CalculateGoalieRating(team)
            + " Total:" + TeamRatingCalculator.CalculateLineupOverall(team)
            + " | Стартовый вратарь: " + starterText
            + (isValid ? "" : " | " + message);
    }

    private static string GetTacticsStatusText(GameState state)
    {
        TeamData team = GameSession.CurrentTeam;
        if (team == null)
        {
            return "Тактика: команда не выбрана";
        }

        TacticsService.EnsureTactics(team);
        SpecialTeamsService.EnsureSpecialTeams(team);
        string preset = team.Tactics == null ? "Balanced" : team.Tactics.PresetName;
        string text = "Тактика: " + preset
            + " | PP " + SpecialTeamsService.CalculatePowerPlayRating(team)
            + " | PK " + SpecialTeamsService.CalculatePenaltyKillRating(team);

        MatchResultData last = state == null ? null : state.LastMatchResult;
        if (last != null)
        {
            bool userWasHome = last.HomeTeamId == team.Id;
            int ppGoals = userWasHome ? last.HomePowerPlayGoals : last.AwayPowerPlayGoals;
            int ppOpp = userWasHome ? last.HomePowerPlayOpportunities : last.AwayPowerPlayOpportunities;
            int pkGoalsAgainst = userWasHome ? last.AwayPowerPlayGoals : last.HomePowerPlayGoals;
            int pkOpp = userWasHome ? last.AwayPowerPlayOpportunities : last.HomePowerPlayOpportunities;
            text += " | PP: " + ppGoals + "/" + ppOpp
                + ", PK: " + Mathf.Max(0, pkOpp - pkGoalsAgainst) + "/" + pkOpp;
        }

        return text;
    }

    private static int CalculateAverageActiveCondition(TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        List<PlayerData> activePlayers = LineupService.GetActivePlayers(team);
        if (activePlayers.Count == 0)
        {
            return 0;
        }

        int total = 0;
        int count = 0;
        foreach (PlayerData player in activePlayers)
        {
            if (player == null)
            {
                continue;
            }

            PlayerFatigueService.EnsureFatigueFields(player);
            total += player.Condition;
            count++;
        }

        return count == 0 ? 0 : Mathf.RoundToInt((float)total / count);
    }

    private static PlayerData FindMostFatiguedPlayer(TeamData team)
    {
        if (team == null)
        {
            return null;
        }

        team.EnsurePlayers();
        PlayerData mostFatiguedPlayer = null;
        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            PlayerFatigueService.EnsureFatigueFields(player);
            if (mostFatiguedPlayer == null || player.Fatigue > mostFatiguedPlayer.Fatigue)
            {
                mostFatiguedPlayer = player;
            }
        }

        return mostFatiguedPlayer;
    }

    private static string FormatMostFatiguedPlayer(PlayerData player)
    {
        if (player == null)
        {
            return "нет данных";
        }

        return player.FirstName + " " + player.LastName
            + " - COND " + player.Condition
            + " / FAT " + player.Fatigue;
    }

    private static string GetStartingGoalieCondition(TeamData team)
    {
        PlayerData starter = LineupService.GetStartingGoalie(team);
        if (starter == null)
        {
            return "нет данных";
        }

        PlayerFatigueService.EnsureFatigueFields(starter);
        return starter.FirstName + " " + starter.LastName
            + " - COND " + starter.Condition
            + " / FAT " + starter.Fatigue;
    }

    private static PlayerData FindTopToiPlayer(TeamData team, string category)
    {
        if (team == null)
        {
            return null;
        }

        team.EnsurePlayers();
        PlayerData topPlayer = null;
        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            bool matchesCategory = category == "D"
                ? player.Position == "D"
                : player.Position == "C" || player.Position == "LW" || player.Position == "RW";
            if (!matchesCategory)
            {
                continue;
            }

            if (topPlayer == null || player.EstimatedTimeOnIceSeconds > topPlayer.EstimatedTimeOnIceSeconds)
            {
                topPlayer = player;
            }
        }

        return topPlayer;
    }

    private static string FormatToiPlayer(PlayerData player)
    {
        if (player == null)
        {
            return "нет данных";
        }

        return player.FirstName + " " + player.LastName
            + " " + IceTimeConfig.FormatSeconds(player.EstimatedTimeOnIceSeconds);
    }

    public void ShowBusy(string message)
    {
        if (BusyOverlayText != null)
        {
            BusyOverlayText.text = string.IsNullOrEmpty(message) ? "Подождите..." : message;
        }

        if (BusyOverlayPanel != null)
        {
            BusyOverlayPanel.SetActive(true);
        }
    }

    public void HideBusy()
    {
        if (BusyOverlayPanel != null)
        {
            BusyOverlayPanel.SetActive(false);
        }
    }

    private void RunWithBusy(string message, Action action)
    {
        ShowBusy(message);
        try
        {
            action?.Invoke();
        }
        finally
        {
            HideBusy();
        }
    }

    private void MeasurePanelRefresh(string panelName, Action refreshAction)
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            refreshAction?.Invoke();
        }
        finally
        {
            stopwatch.Stop();
            PerformanceTimerService.RecordPanelRefresh(GameSession.CurrentState, panelName, stopwatch.ElapsedMilliseconds);
        }
    }

    private void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }

        if (_historyPanel != null && panel != _historyPanel)
        {
            _historyPanel.SetActive(false);
        }

        if (_newsPanel != null && panel != _newsPanel)
        {
            _newsPanel.SetActive(false);
        }
    }

    private void HideAllPanels()
    {
        SetPanelOnly(_dashboardPanel, false);
        SetPanelOnly(_newsPanel, false);
        SetPanelOnly(_ownerPanel, false);
        SetPanelOnly(_gmCareerPanel, false);
        SetPanelOnly(_diagnosticsPanel, false);
        SetPanelOnly(_historyPanel, false);
        SetPanelOnly(_rosterPanel, false);
        SetPanelOnly(_organizationPanel, false);
        SetPanelOnly(_waiversPanel, false);
        SetPanelOnly(_lineupPanel, false);
        SetPanelOnly(_rolesPanel, false);
        SetPanelOnly(_moralePanel, false);
        SetPanelOnly(_leadershipPanel, false);
        SetPanelOnly(_staffPanel, false);
        SetPanelOnly(_tacticsPanel, false);
        SetPanelOnly(_injuriesPanel, false);
        SetPanelOnly(_contractsPanel, false);
        SetPanelOnly(_extensionsPanel, false);
        SetPanelOnly(_tradesPanel, false);
        SetPanelOnly(_scoutingPanel, false);
        SetPanelOnly(_freeAgencyPanel, false);
        SetPanelOnly(_draftPanel, false);
        SetPanelOnly(_prospectRightsPanel, false);
        SetPanelOnly(_offseasonPanel, false);
        SetPanelOnly(_developmentPanel, false);
        SetPanelOnly(_calendarPanel, false);
        SetPanelOnly(_standingsPanel, false);
        SetPanelOnly(_playerStatsPanel, false);
        SetPanelOnly(_playoffsPanel, false);
        SetPanelOnly(_preGamePanel, false);
        SetPanelOnly(_liveMatchPanel, false);
        SetPanelOnly(_postGameSummaryPanel, false);
    }

    private static void SetPanelOnly(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }

    private static void SetPanelActiveForHistory(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }

    private void ClearLineupSelectionFields()
    {
        _selectedLineupSlotType = "";
        _selectedLineupLineOrPairNumber = 0;
        _selectedLineupSlotPosition = "";
        _selectedLineupPlayerId = "";
    }
}
