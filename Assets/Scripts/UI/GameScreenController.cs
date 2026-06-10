using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
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
    [SerializeField] private GameObject _dashboardPanel;
    [SerializeField] private GameObject _rosterPanel;
    [SerializeField] private GameObject _contractsPanel;
    [SerializeField] private GameObject _tradesPanel;
    [SerializeField] private GameObject _freeAgencyPanel;
    [SerializeField] private GameObject _draftPanel;
    [SerializeField] private GameObject _prospectRightsPanel;
    [SerializeField] private GameObject _offseasonPanel;
    [SerializeField] private GameObject _developmentPanel;
    [SerializeField] private GameObject _lineupPanel;
    [SerializeField] private GameObject _rolesPanel;
    [SerializeField] private GameObject _tacticsPanel;
    [SerializeField] private GameObject _injuriesPanel;
    [SerializeField] private GameObject _calendarPanel;
    [SerializeField] private GameObject _standingsPanel;
    [SerializeField] private GameObject _playerStatsPanel;
    [SerializeField] private GameObject _playoffsPanel;
    [SerializeField] private RosterController _rosterController;
    [SerializeField] private ContractsController _contractsController;
    [SerializeField] private TradesController _tradesController;
    [SerializeField] private FreeAgencyController _freeAgencyController;
    [SerializeField] private DraftController _draftController;
    [SerializeField] private ProspectRightsController _prospectRightsController;
    [SerializeField] private OffseasonController _offseasonController;
    [SerializeField] private DevelopmentController _developmentController;
    [SerializeField] private LineupController _lineupController;
    [SerializeField] private RolesController _rolesController;
    [SerializeField] private TacticsController _tacticsController;
    [SerializeField] private InjuriesController _injuriesController;
    [SerializeField] private CalendarController _calendarController;
    [SerializeField] private StandingsController _standingsController;
    [SerializeField] private PlayerStatsController _playerStatsController;
    [SerializeField] private PlayoffsController _playoffsController;

    private string _selectedUserTradePlayerId = "";
    private string _selectedUserTradePickId = "";
    private string _selectedOtherTradeTeamId = "";
    private string _selectedOtherTradePlayerId = "";
    private string _selectedOtherTradePickId = "";
    private string _selectedFreeAgentId = "";
    private string _selectedProspectId = "";
    private string _selectedProspectRightsId = "";
    private string _selectedLineupSlotType = "";
    private int _selectedLineupLineOrPairNumber;
    private string _selectedLineupSlotPosition = "";
    private string _selectedLineupPlayerId = "";
    private string _selectedRolePlayerId = "";

    private void Start()
    {
        ShowDashboard();
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
        GameObject dashboardPanel,
        GameObject rosterPanel,
        GameObject lineupPanel,
        GameObject tacticsPanel,
        GameObject contractsPanel,
        GameObject tradesPanel,
        GameObject freeAgencyPanel,
        GameObject draftPanel,
        GameObject prospectRightsPanel,
        GameObject offseasonPanel,
        GameObject developmentPanel,
        GameObject rolesPanel,
        GameObject calendarPanel,
        GameObject injuriesPanel,
        GameObject standingsPanel,
        GameObject playerStatsPanel,
        GameObject playoffsPanel,
        RosterController rosterController,
        ContractsController contractsController,
        TradesController tradesController,
        FreeAgencyController freeAgencyController,
        DraftController draftController,
        ProspectRightsController prospectRightsController,
        OffseasonController offseasonController,
        DevelopmentController developmentController,
        RolesController rolesController,
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
        _dashboardPanel = dashboardPanel;
        _rosterPanel = rosterPanel;
        _lineupPanel = lineupPanel;
        _tacticsPanel = tacticsPanel;
        _contractsPanel = contractsPanel;
        _tradesPanel = tradesPanel;
        _freeAgencyPanel = freeAgencyPanel;
        _draftPanel = draftPanel;
        _prospectRightsPanel = prospectRightsPanel;
        _offseasonPanel = offseasonPanel;
        _developmentPanel = developmentPanel;
        _rolesPanel = rolesPanel;
        _injuriesPanel = injuriesPanel;
        _calendarPanel = calendarPanel;
        _standingsPanel = standingsPanel;
        _playerStatsPanel = playerStatsPanel;
        _playoffsPanel = playoffsPanel;
        _rosterController = rosterController;
        _contractsController = contractsController;
        _tradesController = tradesController;
        _freeAgencyController = freeAgencyController;
        _draftController = draftController;
        _prospectRightsController = prospectRightsController;
        _offseasonController = offseasonController;
        _developmentController = developmentController;
        _rolesController = rolesController;
        _lineupController = lineupController;
        _tacticsController = tacticsController;
        _injuriesController = injuriesController;
        _calendarController = calendarController;
        _standingsController = standingsController;
        _playerStatsController = playerStatsController;
        _playoffsController = playoffsController;
    }

    public void ShowDashboard()
    {
        _dashboardPanel.SetActive(true);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        RefreshDashboard();
    }

    public void ShowRoster()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(true);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        if (GameSession.CurrentTeam == null)
        {
            Debug.LogWarning("Нельзя открыть состав: команда не выбрана");
            return;
        }

        _rosterController.ShowRoster();
    }

    public void ShowContracts()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(true);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        RefreshContracts();
    }

    public void ShowTrades()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(true);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        GameSession.EnsureDraftPickOwnership();
        RefreshTrades();
    }

    public void ShowFreeAgency()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(true);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        GameSession.EnsureFreeAgents();
        RefreshFreeAgency();
    }

    public void ShowDraft()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(true);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        GameSession.EnsureDraftPickOwnership();
        DraftService.EnsureDraft(GameSession.CurrentState);
        RefreshDraft();
    }

    public void ShowProspectRights()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, true);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureProspectSigningHistory();
        RefreshProspectRights();
    }

    public void ShowOffseason()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, true);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureSeasonHistory();
        RefreshOffseason();
    }

    public void ShowDevelopment()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, true);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureDevelopmentHistory();
        RefreshDevelopment();
    }

    public void ShowLineup()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, true);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureLineups();
        RefreshLineup();
    }

    public void ShowRoles()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, true);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureRolesAndUsage();
        RefreshRoles();
    }

    public void SelectRolePlayer(string playerId)
    {
        _selectedRolePlayerId = playerId;
        RefreshRoles();
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
        GameSession.RebuildCurrentTeamLineup();
        ClearLineupSelectionFields();
        RefreshLineup();
        RefreshTactics();
        RefreshInjuries();
        RefreshDashboard();
        Debug.Log("Автосостав создан");
    }

    public void SelectLineupSlot(string slotType, int lineOrPairNumber, string slotPosition)
    {
        _selectedLineupSlotType = slotType;
        _selectedLineupLineOrPairNumber = lineOrPairNumber;
        _selectedLineupSlotPosition = slotPosition;
        _selectedLineupPlayerId = "";
        RefreshLineup();
    }

    public void SelectLineupPlayer(string playerId)
    {
        _selectedLineupPlayerId = playerId;
        RefreshLineup();
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

        RefreshLineup();
        RefreshDashboard();
        if (_rosterController != null)
        {
            _rosterController.ShowRoster();
        }

        RefreshTactics();
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

        RefreshLineup();
        RefreshDashboard();
    }

    public void ClearLineupSelection()
    {
        ClearLineupSelectionFields();
        RefreshLineup();
    }

    public void ShowTactics()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, true);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureSpecialTeamsAndTactics();
        RefreshTactics();
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
        RefreshTactics();
        RefreshInjuries();
        RefreshDashboard();
        Debug.Log("Автоспецбригады созданы");
    }

    public void ShowInjuries()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, true);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);

        GameSession.EnsureInjuries();
        RefreshInjuries();
    }

    public void StartNextSeason()
    {
        bool started = GameSession.StartNextSeason(out string message);
        if (started)
        {
            Debug.Log(message);
            RefreshDashboard();
            if (_rosterController != null)
            {
                _rosterController.ShowRoster();
            }

            RefreshContracts();
            RefreshDevelopment();
            RefreshLineup();
            RefreshTactics();
            RefreshInjuries();
            ShowDashboard();
            return;
        }

        Debug.LogWarning(message);
        RefreshOffseason();
    }

    public void SelectProspectRights(string prospectId)
    {
        _selectedProspectRightsId = prospectId;
        RefreshProspectRights();
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

        RefreshProspectRights();
        RefreshDashboard();
        if (_rosterController != null)
        {
            _rosterController.ShowRoster();
        }

        RefreshContracts();
        RefreshTrades();
        RefreshLineup();
        RefreshTactics();
        RefreshInjuries();
    }

    public void SelectProspect(string prospectId)
    {
        _selectedProspectId = prospectId;
        RefreshDraft();
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

        RefreshDashboard();
        RefreshDraft();
    }

    public void AutoDraftUntilUserPick()
    {
        DraftService.AutoPickUntilUserPickOrDraftEnd(GameSession.CurrentState);
        if (GameSession.CurrentState != null)
        {
            SaveLoadService.Save(GameSession.CurrentState);
        }

        RefreshDashboard();
        RefreshDraft();
    }

    public void SelectFreeAgent(string playerId)
    {
        _selectedFreeAgentId = playerId;
        RefreshFreeAgency();
    }

    public void SignSelectedFreeAgent()
    {
        if (string.IsNullOrEmpty(_selectedFreeAgentId))
        {
            Debug.LogWarning("Свободный агент не выбран");
            return;
        }

        bool signed = GameSession.TrySignFreeAgent(
            _selectedFreeAgentId,
            out FreeAgentSigningData signing,
            out string message);

        if (signed)
        {
            Debug.Log(message);
            _selectedFreeAgentId = "";
        }
        else
        {
            Debug.LogWarning(message);
        }

        RefreshFreeAgency();
        RefreshDashboard();
        if (_rosterController != null)
        {
            _rosterController.ShowRoster();
        }

        RefreshContracts();
        RefreshTrades();
        RefreshLineup();
        RefreshTactics();
        RefreshInjuries();
    }

    public void SimulateToFreeAgencyForTesting()
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
        if (GameSession.CurrentState != null)
        {
            SaveLoadService.Save(GameSession.CurrentState);
        }

        RefreshDashboard();
        RefreshDraft();
        RefreshFreeAgency();
        RefreshInjuries();
    }

    public void SimulateToDraftForTesting()
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

        RefreshDashboard();
        RefreshDraft();
        RefreshInjuries();
    }

    public void SelectUserTradePlayer(string playerId)
    {
        _selectedUserTradePlayerId = playerId;
        RefreshTrades();
    }

    public void SelectUserTradePick(string pickId)
    {
        _selectedUserTradePickId = pickId;
        RefreshTrades();
    }

    public void SelectOtherTradeTeam(string teamId)
    {
        _selectedOtherTradeTeamId = teamId;
        _selectedOtherTradePlayerId = "";
        _selectedOtherTradePickId = "";
        RefreshTrades();
    }

    public void SelectOtherTradePlayer(string playerId)
    {
        _selectedOtherTradePlayerId = playerId;
        RefreshTrades();
    }

    public void SelectOtherTradePick(string pickId)
    {
        _selectedOtherTradePickId = pickId;
        RefreshTrades();
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
            RefreshTrades();
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

        RefreshDashboard();
        RefreshTrades();
        RefreshContracts();
        if (_rosterController != null)
        {
            _rosterController.ShowRoster();
        }
        RefreshLineup();
        RefreshTactics();
        RefreshInjuries();
    }

    public void ShowCalendar()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(true);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        RefreshCalendar();
    }

    public void ShowStandings()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(true);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(false);
        RefreshStandings();
    }

    public void ShowPlayerStats()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(true);
        _playoffsPanel.SetActive(false);
        RefreshPlayerStats();
    }

    public void ShowPlayoffs()
    {
        _dashboardPanel.SetActive(false);
        _rosterPanel.SetActive(false);
        _contractsPanel.SetActive(false);
        _tradesPanel.SetActive(false);
        _freeAgencyPanel.SetActive(false);
        _draftPanel.SetActive(false);
        SetPanelActive(_prospectRightsPanel, false);
        SetPanelActive(_offseasonPanel, false);
        SetPanelActive(_developmentPanel, false);
        SetPanelActive(_lineupPanel, false);
        SetPanelActive(_rolesPanel, false);
        SetPanelActive(_tacticsPanel, false);
        SetPanelActive(_injuriesPanel, false);
        _calendarPanel.SetActive(false);
        _standingsPanel.SetActive(false);
        _playerStatsPanel.SetActive(false);
        _playoffsPanel.SetActive(true);
        RefreshPlayoffs();
    }

    public void SimulateMatch()
    {
        MatchResultData result = GameSession.SimulateNextScheduledGame();
        if (result == null)
        {
            SetNoAvailableMatchText();
            Debug.LogWarning("Нет доступных матчей");
            return;
        }

        RefreshDashboard();
        RefreshCalendar();
        RefreshStandings();
        RefreshPlayerStats();
        RefreshInjuries();
        Debug.Log("Результат матча: " + result.Summary);
    }

    public void SimulatePlayoffGame()
    {
        if (!GameSession.CanStartPlayoffs())
        {
            RefreshPlayoffs();
            Debug.LogWarning("Плей-офф станет доступен после завершения регулярного сезона");
            return;
        }

        MatchResultData result = GameSession.SimulateNextPlayoffGame();
        if (result == null)
        {
            Debug.LogWarning("Нет доступного матча плей-офф");
            RefreshPlayoffs();
            return;
        }

        RefreshDashboard();
        RefreshPlayoffs();
        RefreshPlayerStats();
        RefreshInjuries();
        Debug.Log("Матч плей-офф: " + result.Summary);
    }

    public void SimulateRegularSeasonToEnd()
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
        RefreshDashboard();
        RefreshInjuries();
    }

    public void SaveGame()
    {
        if (GameSession.CurrentState == null)
        {
            Debug.LogWarning("Нет активной игры для сохранения");
            return;
        }

        GameSession.EnsureContracts();
        GameSession.EnsureTradeHistory();
        GameSession.EnsureDraftPickOwnership();
        GameSession.EnsureFreeAgents();
        GameSession.EnsureProspectSigningHistory();
        GameSession.EnsureSeasonHistory();
        GameSession.EnsureDevelopmentHistory();
        GameSession.EnsureLineups();
        GameSession.EnsureFatigue();
        GameSession.EnsureInjuries();
        GameSession.EnsureSpecialTeamsAndTactics();
        SaveLoadService.Save(GameSession.CurrentState);
        Debug.Log("Игра сохранена");
    }

    public void DeleteSave()
    {
        SaveLoadService.DeleteSave();
        GameSession.Clear();
        SceneManager.LoadScene("MainMenu");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void RefreshDashboard()
    {
        GameSession.EnsureLeagueRules();
        GameSession.EnsureTradeHistory();
        GameSession.EnsureDraftPickOwnership();
        GameSession.EnsureContracts();
        GameSession.EnsureFreeAgents();
        GameSession.EnsureProspectSigningHistory();
        GameSession.EnsureSeasonHistory();
        GameSession.EnsureDevelopmentHistory();
        GameSession.EnsureLineups();
        GameSession.EnsureFatigue();
        GameSession.EnsureInjuries();
        GameSession.EnsureSpecialTeamsAndTactics();
        GameSession.EnsureRolesAndUsage();
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
    }

    private void RefreshContracts()
    {
        if (_contractsController != null)
        {
            _contractsController.ShowContracts();
        }
    }

    private void RefreshFreeAgency()
    {
        GameSession.EnsureFreeAgents();

        if (_freeAgencyController != null)
        {
            _freeAgencyController.ShowFreeAgency(GameSession.CurrentState, _selectedFreeAgentId);
        }
    }

    private void RefreshDraft()
    {
        GameSession.EnsureDraftPickOwnership();

        if (_draftController != null)
        {
            _draftController.ShowDraft(GameSession.CurrentState, _selectedProspectId);
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

    private void RefreshOffseason()
    {
        GameSession.EnsureSeasonHistory();

        if (_offseasonController != null)
        {
            _offseasonController.ShowOffseason(GameSession.CurrentState);
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

        if (_rolesController != null)
        {
            _rolesController.ShowRoles(GameSession.CurrentState, _selectedRolePlayerId);
        }
    }

    private void RefreshLineup()
    {
        GameSession.EnsureLineups();
        GameSession.EnsureRolesAndUsage();

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
        RefreshTactics();
        RefreshRoles();
        RefreshDashboard();
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

        RefreshRoles();
        if (_rosterController != null)
        {
            _rosterController.ShowRoster();
        }

        RefreshLineup();
        RefreshDashboard();
    }

    private void RefreshTrades()
    {
        GameSession.EnsureLeagueRules();
        GameSession.EnsureTradeHistory();
        GameSession.EnsureContracts();
        GameSession.EnsureDraftPickOwnership();

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
        if (_calendarController != null)
        {
            _calendarController.ShowCalendar(season);
        }
    }

    private void RefreshStandings()
    {
        GameSession.EnsureSeason();
        SeasonData season = GameSession.CurrentState == null ? null : GameSession.CurrentState.Season;
        if (_standingsController != null)
        {
            _standingsController.ShowStandings(season);
        }
    }

    private void RefreshPlayerStats()
    {
        GameSession.EnsureSeason();
        SeasonData season = GameSession.CurrentState == null ? null : GameSession.CurrentState.Season;
        string teamId = GameSession.CurrentTeam == null ? "" : GameSession.CurrentTeam.Id;

        if (_playerStatsController != null)
        {
            _playerStatsController.ShowStats(season, teamId);
        }
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
        _teamText.text = team == null
            ? "Команда не выбрана"
            : "Выбранная команда: " + team.City + " " + team.Name + " (" + team.Abbreviation + ")";
    }

    private void UpdateSeasonRulesText()
    {
        if (_seasonRulesText == null)
        {
            return;
        }

        GameState state = GameSession.CurrentState;
        string seasonText = state == null
            ? "2026-27"
            : FormatSeason(state.CurrentSeasonStartYear, state.CurrentSeasonEndYear);
        int careerSeasonNumber = state == null ? 1 : state.CareerSeasonNumber;
        string phase = LeaguePhaseService.GetCurrentPhase(state);
        bool canStartNextSeason = GameSession.CanStartNextSeason(out string message);
        string transitionStatus = canStartNextSeason ? "Можно начать следующий сезон" : "Следующий сезон пока недоступен";
        int historyCount = state == null || state.SeasonHistory == null ? 0 : state.SeasonHistory.Count;
        int developmentChangesCount = GetLastDevelopmentChangesCount(state);
        string lineupStatus = GetLineupStatusText();
        string tacticsStatus = GetTacticsStatusText(state);

        _seasonRulesText.text = "Сезон: " + seasonText
            + " | Сезон карьеры: " + careerSeasonNumber
            + " | Игр: " + SalaryCapConfig.TargetGamesPerTeam
            + "\nФаза: " + phase
            + " | " + transitionStatus
            + " | Архивных сезонов: " + historyCount
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

        string freeAgencyStartDate = GameSession.CurrentState == null || GameSession.CurrentState.LeagueCalendar == null
            ? ""
            : GameSession.CurrentState.LeagueCalendar.FreeAgencyStartDate;
        string phase = LeaguePhaseService.GetCurrentPhase(GameSession.CurrentState);
        string status = LeaguePhaseService.IsFreeAgencyOpen(GameSession.CurrentState)
            ? "Free agency: открыта"
            : "Free agency: закрыта";

        _freeAgencyStatusText.text = "Фаза сезона: " + phase
            + " | FreeAgencyStartDate: " + freeAgencyStartDate
            + " | " + status;
    }

    private void UpdateFinanceText()
    {
        if (_financeText == null)
        {
            return;
        }

        if (GameSession.CurrentTeam == null)
        {
            _financeText.text = "Финансы команды недоступны";
            return;
        }

        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(GameSession.CurrentTeam);
        GameSession.CurrentTeam.EnsureDraftRights();
        PlayerFatigueService.EnsureFatigueForTeam(GameSession.CurrentTeam);
        InjuryService.EnsureInjuryFieldsForTeam(GameSession.CurrentTeam);
        IceTimeService.EnsureUsageForTeam(GameSession.CurrentTeam);
        TeamUsageSummaryData usageSummary = IceTimeService.CalculateTeamUsageSummary(GameSession.CurrentTeam);
        List<PlayerData> injuredPlayers = GameSession.GetCurrentTeamInjuredPlayers();
        LineupService.HasInjuredActivePlayers(GameSession.CurrentTeam, out string injuredLineupMessage);
        string text = "Зарплатная ведомость: " + FormatMoney(finance.Payroll)
            + " / " + FormatMoney(finance.SalaryCapUpperLimit) + "\n"
            + "Место под потолком: " + FormatMoney(finance.CapSpace) + "\n"
            + "Минимальный порог: " + FormatMoney(finance.SalaryCapLowerLimit) + "\n"
            + "Права на проспектов: " + GameSession.CurrentTeam.DraftRights.Count + "\n"
            + "Состав: " + finance.PlayerCount + " / " + SalaryCapConfig.MaxRosterSize + "\n"
            + "Травмы: " + injuredPlayers.Count + (string.IsNullOrEmpty(injuredLineupMessage) ? "" : " | " + injuredLineupMessage) + "\n"
            + "Средняя готовность состава: " + CalculateAverageActiveCondition(GameSession.CurrentTeam) + "\n"
            + "Самый уставший: " + FormatMostFatiguedPlayer(FindMostFatiguedPlayer(GameSession.CurrentTeam)) + "\n"
            + "Стартовый вратарь: " + GetStartingGoalieCondition(GameSession.CurrentTeam) + "\n"
            + "Средний TOI: " + IceTimeConfig.FormatSeconds(usageSummary.AverageActiveTimeOnIceSeconds)
            + " | Топ F: " + FormatToiPlayer(FindTopToiPlayer(GameSession.CurrentTeam, "F"))
            + " | Топ D: " + FormatToiPlayer(FindTopToiPlayer(GameSession.CurrentTeam, "D"));

        if (finance.IsOverCap)
        {
            text += "\nВнимание: команда выше потолка зарплат";
        }

        if (finance.IsBelowFloor)
        {
            text += "\nВнимание: команда ниже минимального порога зарплат";
        }

        _financeText.text = text;
    }

    private void UpdateGamesSimulatedText()
    {
        if (_gamesSimulatedText == null)
        {
            return;
        }

        int gamesSimulated = GameSession.CurrentState == null ? 0 : GameSession.CurrentState.TotalGamesSimulated;
        int totalGames = GameSession.CurrentState == null || GameSession.CurrentState.Season == null
            ? 0
            : GameSession.CurrentState.Season.Schedule.Count;

        _gamesSimulatedText.text = "Матчей сыграно в лиге: " + gamesSimulated + " / " + totalGames;
    }

    private void UpdateCurrentDayText()
    {
        if (_currentDayText == null)
        {
            return;
        }

        int currentDay = GameSession.CurrentState == null || GameSession.CurrentState.Season == null
            ? 1
            : GameSession.CurrentState.Season.CurrentDay;

        _currentDayText.text = "Игровой день: " + currentDay;
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

        _lastMatchResultText.text = seasonFinished ? "Сезон завершён" : "Нет доступных матчей";
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

    private static void SetPanelActive(GameObject panel, bool isActive)
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
