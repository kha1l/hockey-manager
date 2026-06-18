using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSession
{
    public static GameState CurrentState { get; private set; }
    public static TeamData CurrentTeam { get; private set; }
    public static LiveMatchStateData CurrentLiveMatch { get; private set; }
    public static PreGameSetupData CurrentPreGameSetup { get; private set; }
    public static PostGameSummaryData LastPostGameSummary { get; private set; }
    private static List<TeamData> _teamLookupSource;
    private static Dictionary<string, TeamData> _teamLookup;
    private static GameState _preparedNewGameState;
    private static bool _isPreparingNewGameState;

    public static bool IsNewGameTemplateReady
    {
        get { return _preparedNewGameState != null; }
    }

    public static TeamData EnsureCurrentTeam()
    {
        if (CurrentTeam != null)
        {
            return CurrentTeam;
        }

        if (CurrentState == null || CurrentState.Teams == null || string.IsNullOrEmpty(CurrentState.SelectedTeamId))
        {
            return null;
        }

        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        return CurrentTeam;
    }

    public static void PrepareNewGameTemplate()
    {
        if (_preparedNewGameState != null || _isPreparingNewGameState)
        {
            return;
        }

        _isPreparingNewGameState = true;
        GameState previousState = CurrentState;
        TeamData previousTeam = CurrentTeam;
        List<TeamData> previousTeamLookupSource = _teamLookupSource;
        Dictionary<string, TeamData> previousTeamLookup = _teamLookup;

        try
        {
            GameState templateState = CreateNewGameState(GetDefaultNewGameTeamId());
            CurrentState = templateState;
            InvalidateTeamLookup();
            CurrentTeam = FindTeam(templateState.Teams, templateState.SelectedTeamId);
            EnsureAllCoreSystemsAfterLoad(false);
            EnsurePreparedNewGameSystems();
            _preparedNewGameState = CloneGameState(templateState);
            Debug.Log("Шаблон новой игры подготовлен");
        }
        finally
        {
            CurrentState = previousState;
            CurrentTeam = previousTeam;
            _teamLookupSource = previousTeamLookupSource;
            _teamLookup = previousTeamLookup;
            _isPreparingNewGameState = false;
        }
    }

    public static IEnumerator PrepareNewGameTemplateAsync(Action<string, float> onProgress = null)
    {
        if (_preparedNewGameState != null)
        {
            yield break;
        }

        while (_isPreparingNewGameState)
        {
            yield return null;
        }

        if (_preparedNewGameState != null)
        {
            yield break;
        }

        _isPreparingNewGameState = true;
        GameState previousState = CurrentState;
        TeamData previousTeam = CurrentTeam;
        List<TeamData> previousTeamLookupSource = _teamLookupSource;
        Dictionary<string, TeamData> previousTeamLookup = _teamLookup;

        try
        {
            SetPreparationStatus(onProgress, "Подготовка команд...", 0.24f);
            GameState templateState = CreateNewGameState(GetDefaultNewGameTeamId());
            CurrentState = templateState;
            TeamIdentityService.EnsureGameStateIdentity(CurrentState);
            InvalidateTeamLookup();
            CurrentTeam = FindTeam(templateState.Teams, templateState.SelectedTeamId);
            yield return null;

            yield return EnsureNewGameTemplateCoreSystemsAsync(onProgress);

            SetPreparationStatus(onProgress, "Фиксация шаблона...", 0.98f);
            _preparedNewGameState = CloneGameState(templateState);
            Debug.Log("Шаблон новой игры подготовлен");
            yield return null;
        }
        finally
        {
            CurrentState = previousState;
            CurrentTeam = previousTeam;
            _teamLookupSource = previousTeamLookupSource;
            _teamLookup = previousTeamLookup;
            _isPreparingNewGameState = false;
        }
    }

    public static void StartNewGame(string selectedTeamId)
    {
        GameState gameState = ConsumePreparedNewGameState(selectedTeamId);
        bool usedPreparedTemplate = gameState != null;
        if (gameState == null)
        {
            gameState = CreateNewGameState(selectedTeamId);
        }

        CurrentState = gameState;
        TeamIdentityService.EnsureGameStateIdentity(CurrentState);
        InvalidateTeamLookup();
        CurrentTeam = FindTeam(gameState.Teams, selectedTeamId);
        if (CurrentTeam == null)
        {
            Debug.LogWarning("Выбранная команда не найдена: " + selectedTeamId);
            return;
        }

        if (usedPreparedTemplate)
        {
            EnsureSelectedTeamSystemsAfterNewGame();
        }
        else
        {
            EnsureAllCoreSystemsAfterLoad(false);
            EnsureSelectedTeamSystemsAfterNewGame();
        }

        EnsureInitialCurrentTeamCaptaincy();
        SaveLoadService.Save(CurrentState);
        Debug.Log("Новая игра создана: " + GetTeamDisplayName(CurrentTeam));
    }

    private static GameState CreateNewGameState(string selectedTeamId)
    {
        return new GameState
        {
            SaveVersion = 1,
            SelectedTeamId = selectedTeamId,
            TotalGamesSimulated = 0,
            Teams = TeamSeedData.CreateTeams()
        };
    }

    private static string GetDefaultNewGameTeamId()
    {
        List<TeamData> teams = LeagueSeedService.CreateTeamSummaries();
        return teams != null && teams.Count > 0 && teams[0] != null ? teams[0].Id : "";
    }

    private static GameState ConsumePreparedNewGameState(string selectedTeamId)
    {
        if (_preparedNewGameState == null)
        {
            return null;
        }

        GameState preparedState = CloneGameState(_preparedNewGameState);
        _preparedNewGameState = null;
        if (preparedState == null)
        {
            return null;
        }

        preparedState.SelectedTeamId = selectedTeamId;
        return preparedState;
    }

    private static GameState CloneGameState(GameState state)
    {
        if (state == null)
        {
            return null;
        }

        string json = JsonUtility.ToJson(state, false);
        return JsonUtility.FromJson<GameState>(json);
    }

    private static void EnsurePreparedNewGameSystems()
    {
        EnsureProspectSigningHistory();
        EnsureCpuRosterManagementHistory();
        EnsureTutorial();
    }

    private static IEnumerator EnsureNewGameTemplateCoreSystemsAsync(Action<string, float> onProgress)
    {
        SetPreparationStatus(onProgress, "Правила лиги...", 0.28f);
        EnsureLeagueRules();
        yield return null;

        SetPreparationStatus(onProgress, "Календарь сезона...", 0.34f);
        EnsureTradeHistory();
        if (CurrentState.Teams == null || CurrentState.Teams.Count == 0)
        {
            CurrentState.Teams = TeamSeedData.CreateTeams();
        }

        if (CurrentTeam == null)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        }

        bool recreateSeason = ShouldRecreateSeason();
        if (recreateSeason)
        {
            CurrentState.Season = SeasonGenerator.CreateSimpleSeason(
                CurrentState.SelectedTeamId,
                CurrentState.Teams,
                CurrentState.CurrentSeasonStartYear);
            CurrentState.TotalGamesSimulated = 0;
            CurrentState.LastMatchResult = null;
            CurrentState.MatchHistory = new List<MatchResultData>();
        }
        yield return null;

        SetPreparationStatus(onProgress, "Таблица и статистика...", 0.42f);
        CurrentState.Season.EnsureCollections();
        PlayerStatsService.EnsurePlayerStats(CurrentState.Season);
        if (CurrentState.Season.Standings.Count == 0)
        {
            StandingsService.EnsureStandings(CurrentState.Season, CurrentState.Teams);
        }

        if (CurrentState.Season.CurrentDay <= 0)
        {
            CurrentState.Season.CurrentDay = 1;
        }

        if (CurrentTeam != null)
        {
            EnsureTeamPlayers(CurrentTeam);
        }
        yield return null;

        SetPreparationStatus(onProgress, "Контракты и составы...", 0.50f);
        EnsureContracts();
        EnsureRosterStatuses();
        EnsureWaivers();
        yield return null;

        SetPreparationStatus(onProgress, "Линии и спецбригады...", 0.58f);
        EnsureLineups();
        EnsureSpecialTeamsAndTactics();
        yield return null;

        SetPreparationStatus(onProgress, "Форма, травмы и роли...", 0.66f);
        EnsureFatigue();
        EnsureInjuries();
        EnsureRolesAndUsage();
        yield return null;

        SetPreparationStatus(onProgress, "Базовые данные клуба...", 0.78f);
        EnsureProspectSigningHistory();
        EnsureCpuRosterManagementHistory();
        EnsureTutorial();
        yield return null;

        SetPreparationStatus(onProgress, "Подготовка главной...", 0.90f);
        CurrentState.EnsureMatchHistory();
        yield return null;

        SetPreparationStatus(onProgress, "Готово...", 0.96f);
        yield return null;
    }

    private static void SetPreparationStatus(Action<string, float> onProgress, string status, float progress)
    {
        if (onProgress != null)
        {
            onProgress(status, progress);
        }
    }

    private static void EnsureSelectedTeamSystemsAfterNewGame()
    {
        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        EnsureProspectSigningHistory();
        EnsureCpuRosterManagementHistory();
        EnsureTutorial();
    }

    public static void LoadGame(GameState loadedState)
    {
        if (loadedState == null)
        {
            Debug.LogWarning("GameSession: loadedState is null.");
            return;
        }

        if (loadedState.Teams == null)
        {
            loadedState.Teams = new List<TeamData>();
        }

        loadedState.EnsureMatchHistory();
        loadedState.EnsureTradeHistory();
        loadedState.EnsureFreeAgentHistory();
        loadedState.EnsureDraftData();
        loadedState.EnsureProspectSigningHistory();
        loadedState.EnsureCpuRosterManagementHistory();
        loadedState.EnsureAlphaBalanceReports();
        loadedState.EnsureAndroidPerformanceData();
        loadedState.EnsureWaiverWire();
        loadedState.EnsureInjuryHistory();
        loadedState.EnsureCareerProgress();
        loadedState.EnsureSeasonHistory();
        loadedState.EnsurePlayerDevelopmentHistory();
        loadedState.EnsureTeamTradeProfiles();
        loadedState.EnsureScoutingHistory();
        loadedState.EnsureMoraleHistory();
        loadedState.EnsureContractExtensionHistory();
        loadedState.EnsureFreeAgencyOfferHistory();
        loadedState.EnsureOwnerEvaluationHistory();
        loadedState.EnsureLeagueHistory();
        loadedState.EnsureNewsFeed();
        loadedState.EnsureTutorialData();
        loadedState.EnsureGmCareerData();
        loadedState.EnsureDiagnosticsData();

        CurrentState = loadedState;
        TeamIdentityService.EnsureGameStateIdentity(CurrentState);
        InvalidateTeamLookup();
        CurrentTeam = FindTeam(loadedState.Teams, loadedState.SelectedTeamId);
        bool shouldAutoAssignLoadedUserCaptains = CurrentTeam != null
            && CurrentTeam.LeadershipData == null
            && !HasAnyCaptaincy(CurrentTeam);

        if (CurrentTeam == null)
        {
            Debug.LogWarning("Команда из сохранения не найдена: " + loadedState.SelectedTeamId);
            return;
        }

        EnsureAllCoreSystemsAfterLoad();
        if (shouldAutoAssignLoadedUserCaptains)
        {
            LeadershipService.AutoAssignCaptains(CurrentTeam);
            EnsureLeadership();
            EnsureMorale();
            EnsureChemistry();
        }

        Debug.Log("Игра загружена: " + GetTeamDisplayName(CurrentTeam));
    }

    public static void EnsureLeagueRules()
    {
        if (CurrentState == null)
        {
            return;
        }

        if (CurrentState.LeagueRules == null)
        {
            CurrentState.LeagueRules = LeagueRulesConfig.CreateDefaultRules();
        }

        if (CurrentState.LeagueCalendar == null)
        {
            CurrentState.LeagueCalendar = LeagueCalendarConfig.CreateDefaultCalendar();
        }

        NormalizeLeagueRules(CurrentState.LeagueRules);
        NormalizeLeagueCalendar(CurrentState);
        CurrentState.EnsureCareerProgress();
    }

    public static void EnsureSeason(bool saveIfRecreated = true)
    {
        if (CurrentState == null)
        {
            Debug.LogWarning("Нельзя подготовить сезон: активная игра не найдена");
            return;
        }

        EnsureLeagueRules();
        EnsureTradeHistory();

        if (CurrentState.Teams == null || CurrentState.Teams.Count == 0)
        {
            CurrentState.Teams = TeamSeedData.CreateTeams();
        }

        if (CurrentTeam == null)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        }

        bool recreateSeason = ShouldRecreateSeason();
        if (recreateSeason)
        {
            CurrentState.Season = SeasonGenerator.CreateSimpleSeason(
                CurrentState.SelectedTeamId,
                CurrentState.Teams,
                CurrentState.CurrentSeasonStartYear);
            CurrentState.TotalGamesSimulated = 0;
            CurrentState.LastMatchResult = null;
            CurrentState.MatchHistory = new List<MatchResultData>();
        }

        CurrentState.Season.EnsureCollections();
        PlayerStatsService.EnsurePlayerStats(CurrentState.Season);

        if (CurrentState.Season.Standings.Count == 0)
        {
            StandingsService.EnsureStandings(CurrentState.Season, CurrentState.Teams);
        }

        if (CurrentState.Season.CurrentDay <= 0)
        {
            CurrentState.Season.CurrentDay = 1;
        }

        if (CurrentTeam != null)
        {
            EnsureTeamPlayers(CurrentTeam);
        }

        EnsureContracts();
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureLineups();
        EnsureFatigue();
        EnsureInjuries();
        EnsureSpecialTeamsAndTactics();
        EnsureRolesAndUsage();
        EnsureMorale();
        EnsureLeadership();
        EnsureCoachingStaff();
        EnsureChemistry();
        EnsureContractExtensions();
        EnsureBetterFreeAgency();
        EnsureOwnerGoals();
        EnsureGmCareer();
        EnsureLeagueHistory();
        EnsureNewsFeed();
        EnsureRetirementHistory();

        if (recreateSeason && saveIfRecreated)
        {
            SaveLoadService.Save(CurrentState);
        }
    }

    private static void EnsureSeasonScheduleAndStandingsOnly()
    {
        if (CurrentState == null)
        {
            return;
        }

        EnsureLeagueRules();
        if (CurrentState.Teams == null)
        {
            CurrentState.Teams = new List<TeamData>();
        }

        bool recreateSeason = ShouldRecreateSeason();
        if (recreateSeason)
        {
            CurrentState.Season = SeasonGenerator.CreateSimpleSeason(
                CurrentState.SelectedTeamId,
                CurrentState.Teams,
                CurrentState.CurrentSeasonStartYear);
            CurrentState.TotalGamesSimulated = 0;
            CurrentState.LastMatchResult = null;
            CurrentState.MatchHistory = new List<MatchResultData>();
        }

        if (CurrentState.Season == null)
        {
            return;
        }

        CurrentState.Season.EnsureCollections();
        if (CurrentState.Season.Standings.Count == 0)
        {
            StandingsService.EnsureStandings(CurrentState.Season, CurrentState.Teams);
        }

        if (CurrentState.Season.CurrentDay <= 0)
        {
            CurrentState.Season.CurrentDay = 1;
        }
    }

    public static void EnsureContracts()
    {
        if (CurrentState == null || CurrentState.Teams == null)
        {
            return;
        }

        ContractGenerator.EnsureContractsForTeams(CurrentState.Teams);

        if (CurrentTeam != null)
        {
            ContractGenerator.EnsureContractsForTeam(CurrentTeam);
        }
    }

    public static void EnsureContractExtensions()
    {
        if (CurrentState == null)
        {
            return;
        }

        CurrentState.EnsureContractExtensionHistory();
        EnsureLeagueRules();
        if (CurrentState.Teams == null)
        {
            return;
        }

        ContractGenerator.EnsureContractsForTeams(CurrentState.Teams);
        TeamRosterService.EnsureRosterStatusesForTeams(CurrentState.Teams);
        ContractExtensionService.EnsureExtensionDataForTeams(CurrentState, CurrentState.Teams);
    }

    public static void EnsureRosterStatuses()
    {
        if (CurrentState == null)
        {
            return;
        }

        if (CurrentState.Teams != null)
        {
            TeamRosterService.EnsureRosterStatusesForTeams(CurrentState.Teams);
        }

        if (CurrentState.FreeAgentPool != null)
        {
            CurrentState.FreeAgentPool.EnsureFreeAgents();
            TeamRosterService.EnsureFreeAgentRosterStatuses(CurrentState.FreeAgentPool.FreeAgents);
        }
    }

    public static void EnsureWaivers()
    {
        if (CurrentState == null)
        {
            return;
        }

        WaiverService.EnsureWaiverWire(CurrentState);
        if (CurrentState.Teams != null)
        {
            WaiverEligibilityService.EnsureWaiverEligibilityForTeams(CurrentState.Teams);
        }
    }

    public static List<WaiverPlayerData> GetActiveWaivers()
    {
        EnsureWaivers();
        return CurrentState == null || CurrentState.WaiverWire == null
            ? new List<WaiverPlayerData>()
            : CurrentState.WaiverWire.ActiveWaivers;
    }

    public static List<WaiverPlayerData> GetWaiverHistory()
    {
        EnsureWaivers();
        return CurrentState == null || CurrentState.WaiverWire == null
            ? new List<WaiverPlayerData>()
            : CurrentState.WaiverWire.WaiverHistory;
    }

    public static bool ClaimWaiverPlayer(string waiverId, out string message)
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            message = "Команда не выбрана";
            return false;
        }

        EnsureWaivers();
        bool result = WaiverService.TryUserClaimWaiverPlayer(CurrentState, CurrentTeam, waiverId, out message);
        if (result)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            EnsureRosterStatuses();
            EnsureWaivers();
            EnsureLineups();
            EnsureSpecialTeamsAndTactics();
            EnsureRolesAndUsage();
            EnsureMorale();
            EnsureChemistry();
            RunCpuRosterManagement("AfterWaiverClaim", false);
        }

        SaveLoadService.Save(CurrentState);
        return result;
    }

    public static void EnsureLineups()
    {
        if (CurrentState == null || CurrentState.Teams == null)
        {
            return;
        }

        foreach (TeamData team in CurrentState.Teams)
        {
            if (team != null)
            {
                EnsureTeamPlayers(team);
            }
        }

        EnsureRosterStatuses();
        LineupService.EnsureLineupsForTeams(CurrentState.Teams);
        if (CurrentTeam != null)
        {
            LineupService.EnsureLineup(CurrentTeam);
        }
    }

    public static void EnsureFatigue()
    {
        if (CurrentState == null || CurrentState.Teams == null)
        {
            return;
        }

        PlayerFatigueService.EnsureFatigueForTeams(CurrentState.Teams);
    }

    public static void EnsureInjuries()
    {
        if (CurrentState == null)
        {
            return;
        }

        InjuryService.EnsureInjuryHistory(CurrentState);
        if (CurrentState.Teams == null)
        {
            return;
        }

        InjuryService.EnsureInjuryFieldsForTeams(CurrentState.Teams);
    }

    public static void ResetFatigueForNewSeason()
    {
        if (CurrentState == null || CurrentState.Teams == null)
        {
            return;
        }

        PlayerFatigueService.ResetFatigueForNewSeason(CurrentState.Teams);
    }

    public static void RebuildCurrentTeamLineup()
    {
        EnsureCurrentTeam();
        if (CurrentTeam == null)
        {
            return;
        }

        CurrentTeam.Lineup = LineupService.BuildAutoLineup(CurrentTeam);
        CurrentTeam.SpecialTeams = SpecialTeamsService.BuildAutoSpecialTeams(CurrentTeam);
        TacticsService.EnsureTactics(CurrentTeam);
        PlayerFatigueService.EnsureFatigueForTeam(CurrentTeam);
        InjuryService.EnsureInjuryFieldsForTeam(CurrentTeam);
        IceTimeService.EnsureUsageForTeam(CurrentTeam);
        EnsureMorale();
        EnsureChemistry();
        if (CurrentState != null)
        {
            SaveLoadService.Save(CurrentState);
        }
    }

    public static bool AutoFixCurrentTeamRosterAndLineup(out string message)
    {
        CpuRosterManagementReportData report = new CpuRosterManagementReportData();
        bool result = TryAutoFixCurrentTeamForGame(report, out message);
        if (CurrentState != null)
        {
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    private static bool TryAutoFixCurrentTeamForGame(out string message)
    {
        return TryAutoFixCurrentTeamForGame(null, out message);
    }

    private static bool TryAutoFixCurrentTeamForGame(CpuRosterManagementReportData report, out string message)
    {
        message = "";
        EnsureCurrentTeam();
        if (CurrentTeam == null)
        {
            message = "Команда не выбрана";
            return false;
        }

        int actionsBefore = report == null || report.Actions == null ? 0 : report.Actions.Count;
        TeamRosterService.EnsureRosterStatusesForTeam(CurrentTeam);
        InjuryService.EnsureInjuryFieldsForTeam(CurrentTeam);
        PlayerFatigueService.EnsureFatigueForTeam(CurrentTeam);
        CreateImmediateCurrentTeamRosterRoomForAutoFix(report, "UserAutoFixBeforeGame");
        CpuRosterManagementService.FixHealthyPlayerShortage(
            CurrentState,
            CurrentTeam,
            report,
            "UserAutoFixBeforeGame");
        CpuRosterManagementService.FixRosterSizeAndPositions(
            CurrentState,
            CurrentTeam,
            report,
            "UserAutoFixBeforeGame");
        CpuRosterManagementService.FixExcessNhlPlayers(
            CurrentState,
            CurrentTeam,
            report,
            "UserAutoFixBeforeGame");

        if (!TeamRosterService.ValidateNhlRoster(CurrentTeam, out message))
        {
            message = "Автозамена не смогла собрать Pro roster: " + message
                + BuildAutoFixActionSummary(report, actionsBefore);
            return false;
        }

        bool needsRebuild = CurrentTeam.Lineup == null || CurrentTeam.SpecialTeams == null;
        if (!needsRebuild)
        {
            needsRebuild = LineupService.HasInjuredActivePlayers(CurrentTeam, out message)
                || LineupService.HasNonNhlActivePlayers(CurrentTeam, out message)
                || !LineupService.ValidateLineup(CurrentTeam, out message)
                || !SpecialTeamsService.ValidateSpecialTeams(CurrentTeam, out message);
        }

        if (!needsRebuild)
        {
            message = "Состав готов" + BuildAutoFixActionSummary(report, actionsBefore);
            return true;
        }

        CurrentTeam.Lineup = LineupService.BuildAutoLineup(CurrentTeam);
        CurrentTeam.SpecialTeams = SpecialTeamsService.BuildAutoSpecialTeams(CurrentTeam);
        TacticsService.EnsureTactics(CurrentTeam);
        PlayerRoleService.EnsureRolesForTeam(CurrentTeam);
        IceTimeService.EnsureUsageForTeam(CurrentTeam);

        if (!LineupService.ValidateLineup(CurrentTeam, out message))
        {
            message = "Автозамена не смогла исправить линии: " + message
                + BuildAutoFixActionSummary(report, actionsBefore);
            return false;
        }

        if (!SpecialTeamsService.ValidateSpecialTeams(CurrentTeam, out message))
        {
            message = "Автозамена не смогла исправить спецбригады: " + message
                + BuildAutoFixActionSummary(report, actionsBefore);
            return false;
        }

        if (CurrentState != null)
        {
            SaveLoadService.Save(CurrentState);
        }

        message = "Автозамена выполнена: Pro roster и линии готовы"
            + BuildAutoFixActionSummary(report, actionsBefore);
        return true;
    }

    private static void CreateImmediateCurrentTeamRosterRoomForAutoFix(
        CpuRosterManagementReportData report,
        string reason)
    {
        if (CurrentTeam == null)
        {
            return;
        }

        int attempts = 0;
        while (TeamRosterService.GetNhlPlayers(CurrentTeam).Count >= RosterStatusConfig.MaxNhlRosterSize
            && HasCurrentTeamAvailablePlayerShortage()
            && attempts < 6)
        {
            PlayerData player = FindBestImmediateRosterRoomPlayer(CurrentTeam);
            if (player == null)
            {
                break;
            }

            string fromStatus = player.RosterStatus;
            RosterMoveResultData result = player.IsInjured
                ? TeamRosterService.MovePlayerToReserve(CurrentTeam, player.Id)
                : TeamRosterService.SendPlayerToFarm(CurrentTeam, player.Id);
            bool success = result != null && result.Success;
            CpuRosterManagementService.AddAction(
                report,
                CurrentTeam,
                player.IsInjured ? "MoveToReserve" : "SendDown",
                player,
                fromStatus,
                success ? player.RosterStatus : fromStatus,
                reason,
                success,
                result == null ? "Auto roster room failed" : result.Message);

            if (!success)
            {
                break;
            }

            attempts++;
        }
    }

    private static bool HasCurrentTeamAvailablePlayerShortage()
    {
        if (CurrentTeam == null)
        {
            return false;
        }

        int availableForwards = 0;
        int availableDefensemen = 0;
        int availableGoalies = 0;
        foreach (PlayerData player in TeamRosterService.GetNhlPlayers(CurrentTeam))
        {
            if (player == null || player.IsOnWaivers || !InjuryService.IsPlayerAvailable(player))
            {
                continue;
            }

            if (CpuRosterManagementConfig.IsForward(player))
            {
                availableForwards++;
            }
            else if (CpuRosterManagementConfig.IsDefenseman(player))
            {
                availableDefensemen++;
            }
            else if (CpuRosterManagementConfig.IsGoalie(player))
            {
                availableGoalies++;
            }
        }

        return availableForwards < CpuRosterManagementConfig.MinHealthyForwardsForGame
            || availableDefensemen < CpuRosterManagementConfig.MinHealthyDefensemenForGame
            || availableGoalies < CpuRosterManagementConfig.MinHealthyGoaliesForGame;
    }

    private static PlayerData FindBestImmediateRosterRoomPlayer(TeamData team)
    {
        if (team == null)
        {
            return null;
        }

        PlayerData injured = null;
        foreach (PlayerData player in TeamRosterService.GetNhlPlayers(team))
        {
            if (player == null || !player.IsInjured)
            {
                continue;
            }

            if (injured == null
                || player.InjuryDaysRemaining > injured.InjuryDaysRemaining
                || (player.InjuryDaysRemaining == injured.InjuryDaysRemaining && player.Overall < injured.Overall))
            {
                injured = player;
            }
        }

        if (injured != null)
        {
            return injured;
        }

        PlayerData noWaiverCandidate = null;
        foreach (PlayerData player in TeamRosterService.GetNhlPlayers(team))
        {
            if (player == null
                || player.IsOnWaivers
                || player.RequiresWaivers
                || !InjuryService.IsPlayerAvailable(player)
                || !WouldImmediateSendDownPreserveAvailableMinimum(team, player))
            {
                continue;
            }

            if (noWaiverCandidate == null
                || player.Overall < noWaiverCandidate.Overall
                || (player.Overall == noWaiverCandidate.Overall && player.Potential < noWaiverCandidate.Potential))
            {
                noWaiverCandidate = player;
            }
        }

        return noWaiverCandidate;
    }

    private static bool WouldImmediateSendDownPreserveAvailableMinimum(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return false;
        }

        int availableForwards = 0;
        int availableDefensemen = 0;
        int availableGoalies = 0;
        foreach (PlayerData current in TeamRosterService.GetNhlPlayers(team))
        {
            if (current == null || current.IsOnWaivers || !InjuryService.IsPlayerAvailable(current))
            {
                continue;
            }

            if (CpuRosterManagementConfig.IsForward(current))
            {
                availableForwards++;
            }
            else if (CpuRosterManagementConfig.IsDefenseman(current))
            {
                availableDefensemen++;
            }
            else if (CpuRosterManagementConfig.IsGoalie(current))
            {
                availableGoalies++;
            }
        }

        if (CpuRosterManagementConfig.IsForward(player))
        {
            availableForwards--;
        }
        else if (CpuRosterManagementConfig.IsDefenseman(player))
        {
            availableDefensemen--;
        }
        else if (CpuRosterManagementConfig.IsGoalie(player))
        {
            availableGoalies--;
        }

        return availableForwards >= CpuRosterManagementConfig.MinHealthyForwardsForGame
            && availableDefensemen >= CpuRosterManagementConfig.MinHealthyDefensemenForGame
            && availableGoalies >= CpuRosterManagementConfig.MinHealthyGoaliesForGame;
    }

    private static string BuildAutoFixActionSummary(CpuRosterManagementReportData report, int actionsBefore)
    {
        if (report == null || report.Actions == null || report.Actions.Count <= actionsBefore)
        {
            return "";
        }

        string summary = "";
        int shown = 0;
        for (int i = actionsBefore; i < report.Actions.Count && shown < 4; i++)
        {
            CpuRosterActionData action = report.Actions[i];
            if (action == null || !action.Success)
            {
                continue;
            }

            string player = string.IsNullOrEmpty(action.PlayerName) ? "" : ": " + action.PlayerName;
            string move = string.IsNullOrEmpty(action.FromStatus) && string.IsNullOrEmpty(action.ToStatus)
                ? ""
                : " (" + action.FromStatus + " -> " + action.ToStatus + ")";
            summary += "\n" + action.ActionType + player + move;
            shown++;
        }

        return summary;
    }

    public static bool ValidateCurrentTeamLineup(out string message)
    {
        return LineupService.ValidateLineup(CurrentTeam, out message);
    }

    public static List<LineupSlotData> GetCurrentTeamLineupSlots()
    {
        if (CurrentTeam == null)
        {
            return new List<LineupSlotData>();
        }

        EnsureLineups();
        return LineupService.GetLineupSlots(CurrentTeam);
    }

    public static List<PlayerData> GetEligiblePlayersForCurrentTeamSlot(string slotType, string slotPosition)
    {
        if (CurrentTeam == null)
        {
            return new List<PlayerData>();
        }

        EnsureLineups();
        return LineupService.GetEligiblePlayersForSlot(CurrentTeam, slotType, slotPosition);
    }

    public static bool AssignCurrentTeamPlayerToLineupSlot(
        string slotType,
        int lineOrPairNumber,
        string slotPosition,
        string playerId,
        out string message)
    {
        if (CurrentTeam == null)
        {
            message = "Команда не выбрана";
            return false;
        }

        EnsureLineups();
        bool result = LineupService.TryAssignPlayerToSlot(
            CurrentTeam,
            slotType,
            lineOrPairNumber,
            slotPosition,
            playerId,
            out message);

        if (result && CurrentState != null)
        {
            IceTimeService.EnsureUsageForTeam(CurrentTeam);
            EnsureMorale();
            EnsureChemistry();
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static bool SwapCurrentTeamGoalies(out string message)
    {
        if (CurrentTeam == null)
        {
            message = "Команда не выбрана";
            return false;
        }

        EnsureLineups();
        bool result = LineupService.TrySwapGoalies(CurrentTeam, out message);
        if (result && CurrentState != null)
        {
            IceTimeService.EnsureUsageForTeam(CurrentTeam);
            EnsureMorale();
            EnsureChemistry();
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static bool ValidateCurrentTeamCanPlay(out string message)
    {
        message = "";
        if (CurrentTeam == null)
        {
            message = "Команда не выбрана";
            return false;
        }

        TeamRosterService.EnsureRosterStatusesForTeam(CurrentTeam);
        InjuryService.EnsureInjuryFieldsForTeam(CurrentTeam);
        TryAutoFixCurrentTeamForGame(out string autoFixMessage);

        if (!TeamRosterService.ValidateNhlRoster(CurrentTeam, out message))
        {
            message = "Нельзя симулировать матч: исправьте основной состав. " + message;
            return false;
        }

        if (LineupService.HasInjuredActivePlayers(CurrentTeam, out message))
        {
            message = "Нельзя симулировать матч: " + message + ". Автосостав не нашёл замену.";
            return false;
        }

        if (LineupService.HasNonNhlActivePlayers(CurrentTeam, out message))
        {
            message = "Нельзя симулировать матч: " + message + ". Проверьте организацию и линии.";
            return false;
        }

        if (!LineupService.ValidateLineup(CurrentTeam, out message))
        {
            message = "Нельзя симулировать матч: исправьте состав. " + message;
            return false;
        }

        if (!SpecialTeamsService.ValidateSpecialTeams(CurrentTeam, out message))
        {
            message = "Нельзя симулировать матч: исправьте спецбригады. " + message;
            return false;
        }

        message = "Команда готова к матчу";
        return true;
    }

    public static List<PlayerData> GetCurrentTeamInjuredPlayers()
    {
        EnsureInjuries();
        return InjuryService.GetInjuredPlayers(CurrentTeam);
    }

    public static void EnsureSpecialTeamsAndTactics()
    {
        if (CurrentState == null || CurrentState.Teams == null)
        {
            return;
        }

        foreach (TeamData team in CurrentState.Teams)
        {
            if (team != null)
            {
                EnsureTeamPlayers(team);
            }
        }

        LineupService.EnsureLineupsForTeams(CurrentState.Teams);
        SpecialTeamsService.EnsureSpecialTeamsForTeams(CurrentState.Teams);
        TacticsService.EnsureTacticsForTeams(CurrentState.Teams);

        if (CurrentTeam != null)
        {
            SpecialTeamsService.EnsureSpecialTeams(CurrentTeam);
            TacticsService.EnsureTactics(CurrentTeam);
        }
    }

    public static void RebuildCurrentTeamSpecialTeams()
    {
        if (CurrentTeam == null)
        {
            return;
        }

        CurrentTeam.SpecialTeams = SpecialTeamsService.BuildAutoSpecialTeams(CurrentTeam);
        IceTimeService.EnsureUsageForTeam(CurrentTeam);
        EnsureMorale();
        EnsureChemistry();
        if (CurrentState != null)
        {
            SaveLoadService.Save(CurrentState);
        }
    }

    public static void SetCurrentTeamTactics(string presetName)
    {
        if (CurrentTeam == null)
        {
            return;
        }

        TacticsService.SetTacticsPreset(CurrentTeam, presetName);
        IceTimeService.EnsureUsageForTeam(CurrentTeam);
        EnsureMorale();
        EnsureChemistry();
        if (CurrentState != null)
        {
            SaveLoadService.Save(CurrentState);
        }
    }

    public static void EnsureRolesAndUsage()
    {
        if (CurrentState == null || CurrentState.Teams == null)
        {
            return;
        }

        PlayerRoleService.EnsureRolesForTeams(CurrentState.Teams);
        IceTimeService.EnsureUsageForTeams(CurrentState.Teams);
    }

    public static List<PlayerUsageData> GetCurrentTeamUsage()
    {
        if (CurrentTeam == null)
        {
            return new List<PlayerUsageData>();
        }

        EnsureRolesAndUsage();
        return IceTimeService.CalculateTeamUsage(CurrentTeam);
    }

    public static bool SetCurrentTeamPlayerRole(string playerId, string role, out string message)
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            message = "Команда не выбрана";
            return false;
        }

        CurrentTeam.EnsurePlayers();
        PlayerData player = FindPlayer(CurrentTeam.Players, playerId);
        bool result = PlayerRoleService.SetPlayerRole(player, role, true, out message);
        if (result)
        {
            IceTimeService.EnsureUsageForTeam(CurrentTeam);
            EnsureMorale();
            EnsureChemistry();
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static bool ValidateCurrentTeamSpecialTeams(out string message)
    {
        return SpecialTeamsService.ValidateSpecialTeams(CurrentTeam, out message);
    }

    public static void EnsureTradeHistory()
    {
        if (CurrentState == null)
        {
            return;
        }

        TradeService.EnsureTradeHistory(CurrentState);
    }

    public static void EnsureDraftPickOwnership()
    {
        if (CurrentState == null)
        {
            return;
        }

        EnsureLeagueRules();
        DraftPickOwnershipService.EnsureDraftPickOwnership(CurrentState);

        if (CurrentState.Teams != null)
        {
            foreach (TeamData team in CurrentState.Teams)
            {
                if (team != null)
                {
                    team.EnsureDraftRights();
                }
            }
        }
    }

    public static void EnsureDraftClassProfile()
    {
        if (CurrentState == null)
        {
            return;
        }

        DraftService.EnsureDraftClassProfile(CurrentState);
    }

    public static DraftClassProfileData GetCurrentDraftClassProfile()
    {
        EnsureDraftClassProfile();
        return CurrentState == null || CurrentState.Draft == null ? null : CurrentState.Draft.ClassProfile;
    }

    public static string GetCurrentDraftClassSummary()
    {
        return DraftService.GetDraftClassSummary(CurrentState);
    }

    public static void EnsureProspectSigningHistory()
    {
        if (CurrentState == null)
        {
            return;
        }

        ProspectSigningService.EnsureProspectSigningHistory(CurrentState);
    }

    public static void EnsureCpuRosterManagementHistory()
    {
        if (CurrentState == null)
        {
            return;
        }

        CurrentState.EnsureCpuRosterManagementHistory();
    }

    public static void EnsureMorale()
    {
        if (CurrentState == null)
        {
            return;
        }

        CurrentState.EnsureMoraleHistory();
        MoraleService.EnsureMorale(CurrentState);
    }

    public static void EnsureLeadership()
    {
        if (CurrentState == null || CurrentState.Teams == null)
        {
            return;
        }

        LeadershipService.EnsureLeadershipForTeams(CurrentState.Teams);
        string userTeamId = CurrentTeam != null ? CurrentTeam.Id : CurrentState.SelectedTeamId;
        foreach (TeamData team in CurrentState.Teams)
        {
            if (team == null || team.Id == userTeamId)
            {
                continue;
            }

            PlayerData captain = FindCaptain(team);
            if (captain == null || !LeadershipService.IsEligibleForCaptaincy(captain))
            {
                LeadershipService.AutoAssignCaptains(team);
            }
        }
    }

    public static void EnsureCoachingStaff()
    {
        if (CurrentState == null || CurrentState.Teams == null)
        {
            return;
        }

        CoachingStaffService.EnsureStaffForTeams(CurrentState.Teams);
    }

    public static void UpdateMoraleAfterGameDay()
    {
        if (CurrentState == null)
        {
            return;
        }

        MoraleService.UpdateMoraleAfterGameDay(CurrentState);
        EnsureLeadership();
        SaveLoadService.Save(CurrentState);
    }

    public static TeamLeadershipData GetCurrentTeamLeadership()
    {
        EnsureLeadership();
        return CurrentTeam == null ? null : CurrentTeam.LeadershipData;
    }

    public static TeamStaffData GetCurrentTeamStaff()
    {
        EnsureCoachingStaff();
        return CurrentTeam == null ? null : CurrentTeam.Staff;
    }

    public static StaffEffectSummaryData GetCurrentTeamStaffEffectSummary()
    {
        EnsureCoachingStaff();
        return CurrentTeam == null ? null : CoachingStaffService.BuildStaffEffectSummary(CurrentTeam);
    }

    public static List<LeadershipCandidateData> GetCurrentTeamLeadershipCandidates()
    {
        EnsureLeadership();
        return CurrentTeam == null
            ? new List<LeadershipCandidateData>()
            : LeadershipService.BuildLeadershipCandidates(CurrentTeam);
    }

    public static CaptaincyActionResultData AutoAssignCurrentTeamCaptains()
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            return new CaptaincyActionResultData
            {
                Success = false,
                Message = "Команда не выбрана",
                ActionType = "AutoAssign",
                AssignedRole = LeadershipConfig.RoleNone
            };
        }

        CaptaincyActionResultData result = LeadershipService.AutoAssignCaptains(CurrentTeam);
        RefreshLeadershipDependentSystems(result);
        return result;
    }

    public static CaptaincyActionResultData AssignCurrentTeamCaptain(string playerId)
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            return CreateMissingCaptaincyResult("AssignCaptain");
        }

        CaptaincyActionResultData result = LeadershipService.AssignCaptain(CurrentTeam, playerId);
        RefreshLeadershipDependentSystems(result);
        return result;
    }

    public static CaptaincyActionResultData AssignCurrentTeamAlternateCaptain(string playerId)
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            return CreateMissingCaptaincyResult("AssignAlternate");
        }

        CaptaincyActionResultData result = LeadershipService.AssignAlternateCaptain(CurrentTeam, playerId);
        RefreshLeadershipDependentSystems(result);
        return result;
    }

    public static CaptaincyActionResultData ClearCurrentTeamCaptaincy(string playerId)
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            return CreateMissingCaptaincyResult("ClearCaptaincy");
        }

        CaptaincyActionResultData result = LeadershipService.ClearCaptaincy(CurrentTeam, playerId);
        RefreshLeadershipDependentSystems(result);
        return result;
    }

    public static TeamMoraleSummaryData GetCurrentTeamMoraleSummary()
    {
        EnsureMorale();
        return CurrentTeam == null ? null : MoraleService.BuildTeamMoraleSummary(CurrentState, CurrentTeam);
    }

    public static List<PlayerMoraleSnapshotData> GetCurrentTeamMoraleSnapshots()
    {
        EnsureMorale();
        return CurrentTeam == null
            ? new List<PlayerMoraleSnapshotData>()
            : MoraleService.BuildTeamMoraleSnapshots(CurrentState, CurrentTeam);
    }

    public static List<MoraleEventData> GetRecentMoraleEvents(int maxCount)
    {
        EnsureMorale();
        List<MoraleEventData> events = new List<MoraleEventData>();
        if (CurrentState == null || CurrentState.MoraleHistory == null || CurrentState.MoraleHistory.Events == null)
        {
            return events;
        }

        for (int i = CurrentState.MoraleHistory.Events.Count - 1; i >= 0; i--)
        {
            MoraleEventData moraleEvent = CurrentState.MoraleHistory.Events[i];
            if (moraleEvent == null)
            {
                continue;
            }

            events.Add(moraleEvent);
            if (maxCount > 0 && events.Count >= maxCount)
            {
                break;
            }
        }

        return events;
    }

    public static void EnsureChemistry()
    {
        if (CurrentState == null || CurrentState.Teams == null)
        {
            return;
        }

        EnsureMorale();
        EnsureLeadership();
        EnsureCoachingStaff();
        ChemistryService.EnsureChemistryForTeams(CurrentState.Teams);
    }

    public static TeamChemistryData GetCurrentTeamChemistry()
    {
        EnsureChemistry();
        return CurrentTeam == null ? null : CurrentTeam.Chemistry;
    }

    public static List<LineChemistryData> GetCurrentTeamForwardChemistry()
    {
        TeamChemistryData chemistry = GetCurrentTeamChemistry();
        return chemistry == null || chemistry.ForwardLines == null
            ? new List<LineChemistryData>()
            : new List<LineChemistryData>(chemistry.ForwardLines);
    }

    public static List<LineChemistryData> GetCurrentTeamDefenseChemistry()
    {
        TeamChemistryData chemistry = GetCurrentTeamChemistry();
        return chemistry == null || chemistry.DefensePairs == null
            ? new List<LineChemistryData>()
            : new List<LineChemistryData>(chemistry.DefensePairs);
    }

    public static List<LineChemistryData> GetCurrentTeamSpecialTeamsChemistry()
    {
        TeamChemistryData chemistry = GetCurrentTeamChemistry();
        List<LineChemistryData> units = new List<LineChemistryData>();
        if (chemistry == null)
        {
            return units;
        }

        if (chemistry.PowerPlayUnits != null)
        {
            units.AddRange(chemistry.PowerPlayUnits);
        }

        if (chemistry.PenaltyKillUnits != null)
        {
            units.AddRange(chemistry.PenaltyKillUnits);
        }

        return units;
    }

    public static CpuRosterManagementReportData RunCpuRosterManagement(string reason, bool saveAfterRun = true)
    {
        if (CurrentState == null)
        {
            return new CpuRosterManagementReportData();
        }

        EnsureLeagueRules();
        EnsureContracts();
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureFatigue();
        EnsureInjuries();
        EnsureCpuRosterManagementHistory();
        EnsureCoachingStaff();
        EnsureMorale();
        EnsureChemistry();

        string userTeamId = CurrentTeam != null ? CurrentTeam.Id : CurrentState.SelectedTeamId;
        CpuRosterManagementReportData report = CpuRosterManagementService.RunForAllCpuTeams(
            CurrentState,
            userTeamId,
            reason);

        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        EnsureCoachingStaff();
        EnsureMorale();
        EnsureChemistry();
        TeamTradeProfileService.EnsureTradeProfiles(CurrentState);
        EnsureOwnerGoals();
        if (saveAfterRun)
        {
            SaveLoadService.Save(CurrentState);
        }

        return report;
    }

    public static CpuRosterManagementReportData GetLastCpuRosterManagementReport()
    {
        EnsureCpuRosterManagementHistory();
        return CurrentState == null ? null : CurrentState.LastCpuRosterManagementReport;
    }

    public static void EnsureAlphaBalanceReports()
    {
        if (CurrentState != null)
        {
            CurrentState.EnsureAlphaBalanceReports();
        }
    }

    public static void EnsureAndroidPerformance()
    {
        if (CurrentState != null)
        {
            CurrentState.EnsureAndroidPerformanceData();
        }
    }

    public static void EnsureAllCoreSystemsAfterLoad(bool saveSeasonIfRecreated = true)
    {
        if (CurrentState == null)
        {
            return;
        }

        // Stabilization order after load/migration:
        // 1. League rules/calendar; 2. Teams/players; 3. Roster statuses;
        // 4. Contracts/cap; 5. Season/schedule/standings; 6. Lineups;
        // 7. Special teams/tactics; 8. Fatigue/injuries; 9. Roles/usage;
        // 10. Morale; 11. Leadership; 12. Coaching staff; 13. Chemistry;
        // 14. Lazy systems such as draft/scouting/free agency/history are initialized by their screens.
        EnsureLeagueRules();
        if (CurrentState.Teams == null)
        {
            CurrentState.Teams = new List<TeamData>();
        }

        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        EnsureSeason(saveSeasonIfRecreated);
        EnsureProspectSigningHistory();
        EnsureCpuRosterManagementHistory();
        EnsureTutorial();
    }

    public static MigrationReportData RunSaveMigration()
    {
        if (CurrentState == null)
        {
            return null;
        }

        MigrationReportData report = SaveMigrationService.Migrate(CurrentState);
        SaveLoadService.Save(CurrentState);
        return report;
    }

    public static GameStateValidationReportData ValidateCurrentState()
    {
        if (CurrentState == null)
        {
            return null;
        }

        GameStateValidationReportData report = GameStateValidationService.Validate(CurrentState);
        SaveLoadService.Save(CurrentState);
        return report;
    }

    public static GameStateValidationReportData RepairCurrentStateSafeIssues()
    {
        if (CurrentState == null)
        {
            return null;
        }

        GameStateValidationReportData report = GameStateRepairService.RepairSafeIssues(CurrentState);
        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        EnsureAllCoreSystemsAfterLoad();
        SaveLoadService.Save(CurrentState);
        return report;
    }

    public static BalanceReportData GenerateBalanceReport()
    {
        if (CurrentState == null)
        {
            return null;
        }

        BalanceReportData report = BalanceReportService.Generate(CurrentState);
        SaveLoadService.Save(CurrentState);
        return report;
    }

    public static AlphaBalanceReportData GenerateAlphaBalanceReport()
    {
        if (CurrentState == null)
        {
            return null;
        }

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        EnsureAlphaBalanceReports();
        AlphaBalanceReportData report = AlphaBalanceReportService.GenerateCurrentStateReport(CurrentState);
        stopwatch.Stop();
        PerformanceTimerService.RecordAlphaReport(CurrentState, stopwatch.ElapsedMilliseconds);
        SaveLoadService.Save(CurrentState);
        return report;
    }

    public static AlphaBalanceReportData RunAlphaBalanceReportOneSeason()
    {
        return RunAlphaBalanceReportSeasons(AlphaBalanceConfig.AlphaShortSimSeasons);
    }

    public static AlphaBalanceReportData RunAlphaBalanceReportThreeSeasons()
    {
        return RunAlphaBalanceReportSeasons(AlphaBalanceConfig.AlphaMediumSimSeasons);
    }

    public static AlphaBalanceReportData RunAlphaBalanceReportFiveSeasons()
    {
        return RunAlphaBalanceReportSeasons(AlphaBalanceConfig.AlphaLongSimSeasons);
    }

    private static AlphaBalanceReportData RunAlphaBalanceReportSeasons(int seasons)
    {
        if (CurrentState == null)
        {
            return null;
        }

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        EnsureAlphaBalanceReports();
        AlphaBalanceReportData report = AlphaBalanceReportService.RunMultiSeasonReport(CurrentState, seasons);
        stopwatch.Stop();
        PerformanceTimerService.RecordAlphaReport(CurrentState, stopwatch.ElapsedMilliseconds);
        SaveLoadService.Save(CurrentState);
        return report;
    }

    public static AndroidReadinessChecklistData GenerateAndroidReadinessChecklist()
    {
        if (CurrentState == null)
        {
            return AndroidReadinessService.Generate(null);
        }

        AndroidReadinessChecklistData checklist = AndroidReadinessService.Generate(CurrentState);
        SaveLoadService.Save(CurrentState);
        return checklist;
    }

    public static AndroidPerformanceData GetAndroidPerformanceData()
    {
        EnsureAndroidPerformance();
        return CurrentState == null ? null : CurrentState.AndroidPerformance;
    }

    public static string GetDiagnosticsSummary()
    {
        if (CurrentState == null)
        {
            return "Diagnostics: no active game";
        }

        MigrationReportData migration = CurrentState.LastMigrationReport;
        GameStateValidationReportData validation = CurrentState.LastValidationReport;
        BalanceReportData balance = CurrentState.LastBalanceReport;
        AlphaBalanceReportData alpha = CurrentState.LastAlphaBalanceReport;
        string migrationText = migration == null
            ? "Migration: not run"
            : "Migration: " + migration.Status + " " + migration.FromSaveVersion + "->" + migration.ToSaveVersion
                + " | W/R/E " + migration.WarningsCount + "/" + migration.RepairsCount + "/" + migration.ErrorsCount;
        string validationText = validation == null
            ? "Validation: not run"
            : "Validation: issues " + validation.IssuesCount
                + " | W/E/C " + validation.WarningsCount + "/" + validation.ErrorsCount + "/" + validation.CriticalCount;
        string balanceText = balance == null
            ? "Balance: not run"
            : "Balance: invalid rosters " + balance.InvalidRosterTeams
                + " | invalid lineups " + balance.InvalidLineupTeams
                + " | cap " + balance.CapViolationTeams;
        string alphaText = alpha == null
            ? "Alpha: not run"
            : "Alpha: warnings " + alpha.WarningCount
                + " | critical " + alpha.CriticalCount
                + " | " + alpha.Recommendation;

        return migrationText + "\n" + validationText + "\n" + balanceText + "\n" + alphaText
            + "\n" + PerformanceTimerService.BuildPerformanceSummary(CurrentState);
    }

    public static void EnsureOwnerGoals()
    {
        if (CurrentState == null)
        {
            return;
        }

        EnsureLeagueRules();
        EnsureContracts();
        EnsureRosterStatuses();
        EnsureSeasonHistory();
        EnsureDevelopmentHistory();
        EnsureMorale();
        CurrentState.EnsureOwnerEvaluationHistory();
        OwnerGoalService.EnsureOwnerProfiles(CurrentState);
        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        GmCareerService.EnsureGmCareer(CurrentState);
    }

    public static void EnsureGmCareer()
    {
        if (CurrentState == null)
        {
            return;
        }

        EnsureLeagueRules();
        if (CurrentState.Teams == null)
        {
            CurrentState.Teams = new List<TeamData>();
        }

        OwnerGoalService.EnsureOwnerProfiles(CurrentState);
        GmCareerService.EnsureGmCareer(CurrentState);
        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
    }

    public static GmCareerData GetGmCareer()
    {
        EnsureGmCareer();
        return CurrentState == null ? null : CurrentState.GmCareer;
    }

    public static List<GmJobOfferData> GetActiveGmJobOffers()
    {
        EnsureGmCareer();
        return CurrentState == null || CurrentState.ActiveGmJobOffers == null
            ? new List<GmJobOfferData>()
            : new List<GmJobOfferData>(CurrentState.ActiveGmJobOffers);
    }

    public static List<GmCareerEventData> GetGmCareerEvents(int maxCount)
    {
        EnsureGmCareer();
        List<GmCareerEventData> events = CurrentState == null || CurrentState.GmCareerEvents == null
            ? new List<GmCareerEventData>()
            : new List<GmCareerEventData>(CurrentState.GmCareerEvents);
        events.Sort((left, right) => string.Compare(
            right == null ? "" : right.CreatedAtUtc,
            left == null ? "" : left.CreatedAtUtc,
            StringComparison.Ordinal));

        if (maxCount > 0)
        {
            while (events.Count > maxCount)
            {
                events.RemoveAt(events.Count - 1);
            }
        }

        return events;
    }

    public static bool AcceptGmJobOffer(string offerId, out string message)
    {
        EnsureGmCareer();
        bool result = GmJobMarketService.AcceptJobOffer(CurrentState, offerId, out message);
        if (result)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            EnsureRosterStatuses();
            EnsureLineups();
            EnsureMorale();
            EnsureChemistry();
            EnsureLeadership();
            EnsureCoachingStaff();
            EnsureOwnerGoals();
            EnsureNewsFeed();
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static bool DeclineGmJobOffer(string offerId, out string message)
    {
        EnsureGmCareer();
        bool result = GmJobMarketService.DeclineJobOffer(CurrentState, offerId, out message);
        if (CurrentState != null)
        {
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static List<GmJobOfferData> GenerateGmJobOffers()
    {
        EnsureGmCareer();
        List<GmJobOfferData> offers = GmJobMarketService.GenerateJobOffers(CurrentState);
        if (CurrentState != null)
        {
            SaveLoadService.Save(CurrentState);
        }

        return offers;
    }

    public static bool IsGmUnemployed()
    {
        EnsureGmCareer();
        return CurrentState != null && CurrentState.GmCareer != null && CurrentState.GmCareer.IsUnemployed;
    }

    public static OwnerProfileData GetCurrentTeamOwnerProfile()
    {
        EnsureOwnerGoals();
        return CurrentState == null || CurrentTeam == null
            ? null
            : OwnerGoalService.GetOwnerProfile(CurrentState, CurrentTeam);
    }

    public static ClubFinanceData GetCurrentTeamClubFinances()
    {
        EnsureOwnerGoals();
        return CurrentState == null || CurrentTeam == null
            ? null
            : OwnerGoalService.GetClubFinances(CurrentState, CurrentTeam);
    }

    public static List<OwnerGoalData> GetCurrentTeamOwnerGoals()
    {
        OwnerProfileData profile = GetCurrentTeamOwnerProfile();
        return profile == null || profile.CurrentGoals == null
            ? new List<OwnerGoalData>()
            : new List<OwnerGoalData>(profile.CurrentGoals);
    }

    public static OwnerSeasonEvaluationData EvaluateCurrentTeamOwnerSeason()
    {
        if (CurrentState == null)
        {
            return null;
        }

        EnsureOwnerGoals();
        OwnerSeasonEvaluationData evaluation = OwnerGoalService.EvaluateCurrentTeamSeason(CurrentState);
        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        SaveLoadService.Save(CurrentState);
        return evaluation;
    }

    public static OwnerSeasonEvaluationData GetLastOwnerSeasonEvaluation()
    {
        OwnerProfileData profile = GetCurrentTeamOwnerProfile();
        return profile == null ? null : profile.LastSeasonEvaluation;
    }

    public static void EnsureLeagueHistory()
    {
        if (CurrentState == null)
        {
            return;
        }

        CurrentState.EnsureLeagueHistory();
        LeagueHistoryService.EnsureLeagueHistory(CurrentState);
        foreach (PlayerData player in CareerStatsService.GetAllPlayersIncludingFreeAgents(CurrentState))
        {
            CareerStatsService.EnsureCareerStats(player);
        }
    }

    public static void EnsureRetirementHistory()
    {
        if (CurrentState == null)
        {
            return;
        }

        RetirementService.EnsureRetirementData(CurrentState);
        JerseyNumberService.EnsureJerseyNumbersForTeams(CurrentState.Teams);
    }

    public static void EnsureTutorial()
    {
        if (CurrentState == null)
        {
            return;
        }

        TutorialService.EnsureTutorial(CurrentState);
    }

    public static List<TutorialStepData> GetTutorialSteps()
    {
        EnsureTutorial();
        return TutorialService.GetTutorialSteps(CurrentState);
    }

    public static TutorialHintData GetCurrentPanelHint(string panelId)
    {
        EnsureTutorial();
        return TutorialService.GetPanelHint(CurrentState, panelId);
    }

    public static void MarkTutorialPanelVisited(string panelId)
    {
        if (CurrentState == null)
        {
            return;
        }

        TutorialService.MarkPanelVisited(CurrentState, panelId);
        SaveLoadService.Save(CurrentState);
    }

    public static void MarkTutorialStepCompleted(string stepId)
    {
        if (CurrentState == null)
        {
            return;
        }

        TutorialService.MarkStepCompleted(CurrentState, stepId);
        SaveLoadService.Save(CurrentState);
    }

    public static void DismissTutorialHint(string hintId)
    {
        if (CurrentState == null)
        {
            return;
        }

        TutorialService.DismissHint(CurrentState, hintId);
        SaveLoadService.Save(CurrentState);
    }

    public static void DisableTutorial()
    {
        if (CurrentState == null)
        {
            return;
        }

        TutorialService.DisableTutorial(CurrentState);
        SaveLoadService.Save(CurrentState);
    }

    public static void EnableTutorial()
    {
        if (CurrentState == null)
        {
            return;
        }

        TutorialService.EnableTutorial(CurrentState);
        SaveLoadService.Save(CurrentState);
    }

    public static void ResetTutorial()
    {
        if (CurrentState == null)
        {
            return;
        }

        TutorialService.ResetTutorial(CurrentState);
        SaveLoadService.Save(CurrentState);
    }

    public static string GetTutorialSummary()
    {
        EnsureTutorial();
        return TutorialService.BuildTutorialSummary(CurrentState);
    }

    public static RetiredPlayersData GetRetiredPlayers()
    {
        EnsureRetirementHistory();
        return CurrentState == null ? null : CurrentState.RetiredPlayers;
    }

    public static HallOfFameData GetHallOfFame()
    {
        EnsureRetirementHistory();
        return CurrentState == null ? null : CurrentState.HallOfFame;
    }

    public static List<RetiredNumberData> GetLeagueRetiredNumbers()
    {
        EnsureRetirementHistory();
        return CurrentState == null || CurrentState.LeagueRetiredNumbers == null
            ? new List<RetiredNumberData>()
            : new List<RetiredNumberData>(CurrentState.LeagueRetiredNumbers);
    }

    public static void ProcessRetirementsIfNeeded()
    {
        if (CurrentState == null)
        {
            return;
        }

        EnsureRetirementHistory();
        if (CurrentState.LastRetirementProcessedSeasonStartYear == CurrentState.CurrentSeasonStartYear)
        {
            return;
        }

        RetirementService.ProcessRetirementsAfterSeason(CurrentState);
        CurrentState.LastRetirementProcessedSeasonStartYear = CurrentState.CurrentSeasonStartYear;
        SaveLoadService.Save(CurrentState);
    }

    public static LeagueSeasonHistoryData GetLastLeagueSeasonHistory()
    {
        EnsureLeagueHistory();
        return CurrentState == null ? null : CurrentState.LastLeagueSeasonHistory;
    }

    public static List<LeagueSeasonHistoryData> GetLeagueHistory()
    {
        EnsureLeagueHistory();
        List<LeagueSeasonHistoryData> history = CurrentState == null || CurrentState.LeagueHistory == null
            ? new List<LeagueSeasonHistoryData>()
            : new List<LeagueSeasonHistoryData>(CurrentState.LeagueHistory);
        history.Sort(CompareLeagueHistoryDescending);
        return history;
    }

    public static List<UserTeamSeasonHistoryData> GetUserTeamHistory()
    {
        EnsureLeagueHistory();
        List<UserTeamSeasonHistoryData> history = CurrentState == null || CurrentState.UserTeamHistory == null
            ? new List<UserTeamSeasonHistoryData>()
            : new List<UserTeamSeasonHistoryData>(CurrentState.UserTeamHistory);
        history.Sort(CompareUserTeamHistoryDescending);
        return history;
    }

    public static SeasonAwardsData GetLastSeasonAwards()
    {
        EnsureLeagueHistory();
        return CurrentState == null ? null : CurrentState.LastSeasonAwards;
    }

    public static List<AwardWinnerData> GetAllAwardsHistory()
    {
        EnsureLeagueHistory();
        List<AwardWinnerData> awards = new List<AwardWinnerData>();
        if (CurrentState == null)
        {
            return awards;
        }

        if (CurrentState.LeagueHistory != null)
        {
            foreach (LeagueSeasonHistoryData history in CurrentState.LeagueHistory)
            {
                if (history == null || history.Awards == null || history.Awards.Awards == null)
                {
                    continue;
                }

                awards.AddRange(history.Awards.Awards);
            }
        }

        if (CurrentState.LastSeasonAwards != null
            && !HasAwardsForSeason(awards, CurrentState.LastSeasonAwards.SeasonStartYear)
            && CurrentState.LastSeasonAwards.Awards != null)
        {
            awards.AddRange(CurrentState.LastSeasonAwards.Awards);
        }

        awards.Sort(CompareAwardsDescending);
        return awards;
    }

    public static LeagueRecordsData GetLeagueRecords()
    {
        EnsureLeagueHistory();
        return CurrentState == null ? null : CurrentState.LeagueRecords;
    }

    public static void GenerateSeasonRecapIfNeeded()
    {
        if (CurrentState == null)
        {
            return;
        }

        EnsureLeagueHistory();
        if (!LeaguePhaseService.CanStartNextSeason(CurrentState))
        {
            return;
        }

        SeasonRecapService.GenerateAndStoreSeasonRecap(CurrentState);
        SaveLoadService.Save(CurrentState);
    }

    public static void EnsureNewsFeed()
    {
        if (CurrentState == null)
        {
            return;
        }

        CurrentState.EnsureNewsFeed();
        NewsFeedService.EnsureNewsFeed(CurrentState);
    }

    public static List<NewsItemData> GetLatestNews(int maxCount)
    {
        EnsureNewsFeed();
        return NewsFeedService.GetLatestNews(CurrentState, maxCount);
    }

    public static List<NewsItemData> GetUserTeamNews(int maxCount)
    {
        EnsureNewsFeed();
        return NewsFeedService.GetUserTeamNews(CurrentState, maxCount);
    }

    public static List<NewsItemData> GetNewsByCategory(string category, int maxCount)
    {
        EnsureNewsFeed();
        return NewsFeedService.GetNewsByCategory(CurrentState, category, maxCount);
    }

    public static void MarkNewsAsRead(string newsId)
    {
        EnsureNewsFeed();
        NewsFeedService.MarkNewsAsRead(CurrentState, newsId);
        if (CurrentState != null)
        {
            SaveLoadService.Save(CurrentState);
        }
    }

    public static void GenerateSeasonRecapNewsIfNeeded()
    {
        if (CurrentState == null)
        {
            return;
        }

        EnsureNewsFeed();
        EnsureLeagueHistory();
        if (CurrentState.LastLeagueSeasonHistory == null)
        {
            return;
        }

        SeasonRecapNewsService.GenerateNewsForSeasonRecap(CurrentState, CurrentState.LastLeagueSeasonHistory);
        SaveLoadService.Save(CurrentState);
    }

    public static void EnsureSeasonHistory()
    {
        if (CurrentState == null)
        {
            return;
        }

        CurrentState.EnsureCareerProgress();
        SeasonHistoryService.EnsureSeasonHistory(CurrentState);
    }

    public static void EnsureTradeProfiles()
    {
        if (CurrentState == null)
        {
            return;
        }

        EnsureLeagueRules();
        EnsureContracts();
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureMorale();
        EnsureChemistry();
        CurrentState.EnsureTeamTradeProfiles();
        TeamTradeProfileService.EnsureTradeProfiles(CurrentState);
    }

    public static TeamTradeProfileData GetCurrentTeamTradeProfile()
    {
        EnsureTradeProfiles();
        return CurrentTeam == null ? null : TeamTradeProfileService.GetTradeProfile(CurrentState, CurrentTeam.Id);
    }

    public static List<TeamTradeProfileData> GetTeamTradeProfiles()
    {
        EnsureTradeProfiles();
        return CurrentState == null
            ? new List<TeamTradeProfileData>()
            : TeamTradeProfileService.GetAllTradeProfiles(CurrentState);
    }

    public static List<TradeBlockPlayerData> GetCurrentTradePartnerTradeBlock(string teamId)
    {
        EnsureTradeProfiles();
        TeamTradeProfileData profile = TeamTradeProfileService.GetTradeProfile(CurrentState, teamId);
        if (profile == null || profile.TradeBlock == null)
        {
            return new List<TradeBlockPlayerData>();
        }

        return new List<TradeBlockPlayerData>(profile.TradeBlock);
    }

    public static List<TradeProposalData> GenerateTradeIdeasForCurrentTeam(int maxIdeas)
    {
        EnsureTradeProfiles();
        return TradeAiService.GenerateCpuTradeIdeas(CurrentState, CurrentTeam, maxIdeas);
    }

    public static void EnsureScouting()
    {
        if (CurrentState == null)
        {
            return;
        }

        CurrentState.EnsureScoutingHistory();
        EnsureDraftClassProfile();
        ScoutingService.EnsureScouting(CurrentState);
    }

    public static ScoutingActionResultData ScoutCurrentDraftProspect(string prospectId)
    {
        EnsureScouting();
        ScoutingActionResultData result = ScoutingService.ScoutProspect(CurrentState, prospectId);
        if (result != null && result.Success)
        {
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static ScoutingActionResultData ScoutTopDraftProspects()
    {
        EnsureScouting();
        ScoutingActionResultData result = ScoutingService.ScoutTopProspects(CurrentState);
        if (result != null && result.Success)
        {
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static ScoutingActionResultData ScoutDraftProspectsByPosition(string position)
    {
        EnsureScouting();
        ScoutingActionResultData result = ScoutingService.ScoutByPosition(CurrentState, position);
        if (result != null && result.Success)
        {
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static List<ScoutingReportData> GetRecentScoutingReports(int maxCount)
    {
        EnsureScouting();
        List<ScoutingReportData> reports = new List<ScoutingReportData>();
        if (CurrentState == null || CurrentState.ScoutingHistory == null || CurrentState.ScoutingHistory.Reports == null)
        {
            return reports;
        }

        int count = 0;
        for (int i = CurrentState.ScoutingHistory.Reports.Count - 1; i >= 0; i--)
        {
            ScoutingReportData report = CurrentState.ScoutingHistory.Reports[i];
            if (report == null)
            {
                continue;
            }

            reports.Add(report);
            count++;
            if (maxCount > 0 && count >= maxCount)
            {
                break;
            }
        }

        return reports;
    }

    public static void EnsureDevelopmentHistory()
    {
        if (CurrentState == null)
        {
            return;
        }

        PlayerDevelopmentService.EnsureDevelopmentHistory(CurrentState);
    }

    public static List<PlayerDevelopmentChangeData> GetRecentDevelopmentChanges(int limit)
    {
        List<PlayerDevelopmentChangeData> changes = new List<PlayerDevelopmentChangeData>();
        if (CurrentState == null || CurrentState.PlayerDevelopmentHistory == null)
        {
            return changes;
        }

        CurrentState.PlayerDevelopmentHistory.EnsureChanges();
        changes.AddRange(CurrentState.PlayerDevelopmentHistory.Changes);
        changes.Sort(CompareDevelopmentChangesDescending);

        if (limit > 0 && changes.Count > limit)
        {
            changes.RemoveRange(limit, changes.Count - limit);
        }

        return changes;
    }

    public static List<ProspectData> GetCurrentTeamDraftRights()
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            return new List<ProspectData>();
        }

        return ProspectSigningService.GetTeamDraftRights(CurrentState, CurrentTeam.Id);
    }

    public static List<ContractExtensionCandidateData> GetCurrentTeamExtensionCandidates()
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            return new List<ContractExtensionCandidateData>();
        }

        EnsureContractExtensions();
        return ContractExtensionService.GetExtensionCandidates(CurrentState, CurrentTeam);
    }

    public static ContractExtensionSummaryData GetCurrentTeamExtensionSummary()
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            return new ContractExtensionSummaryData
            {
                TeamId = "",
                TeamName = "",
                Summary = "Команда не выбрана",
                UpdatedAtUtc = DateTime.UtcNow.ToString("o")
            };
        }

        EnsureContractExtensions();
        return ContractExtensionService.BuildSummary(CurrentState, CurrentTeam);
    }

    public static ContractExtensionOfferData MakeCurrentTeamExtensionOffer(string playerId, int salary, int years)
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            return new ContractExtensionOfferData
            {
                OfferId = Guid.NewGuid().ToString("N"),
                PlayerId = playerId,
                OfferedSalary = salary,
                OfferedYears = years,
                Decision = "Invalid",
                DecisionReason = "Команда не выбрана",
                CreatedAtUtc = DateTime.UtcNow.ToString("o")
            };
        }

        EnsureLeagueRules();
        EnsureContracts();
        EnsureRosterStatuses();
        EnsureRolesAndUsage();
        EnsureMorale();
        EnsureCoachingStaff();
        EnsureContractExtensions();

        ContractExtensionOfferData offer = ContractExtensionService.MakeExtensionOffer(
            CurrentState,
            CurrentTeam,
            playerId,
            salary,
            years);
        EventNewsService.CreateContractExtensionNews(CurrentState, offer);

        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        EnsureContracts();
        EnsureMorale();
        EnsureTradeProfiles();
        EnsureContractExtensions();
        EnsureOwnerGoals();
        SaveLoadService.Save(CurrentState);
        return offer;
    }

    public static List<ContractExtensionOfferData> GetRecentExtensionOffers(int maxCount)
    {
        EnsureContractExtensions();
        List<ContractExtensionOfferData> offers = new List<ContractExtensionOfferData>();
        if (CurrentState == null
            || CurrentState.ContractExtensionHistory == null
            || CurrentState.ContractExtensionHistory.Offers == null)
        {
            return offers;
        }

        for (int i = CurrentState.ContractExtensionHistory.Offers.Count - 1; i >= 0; i--)
        {
            ContractExtensionOfferData offer = CurrentState.ContractExtensionHistory.Offers[i];
            if (offer == null)
            {
                continue;
            }

            offers.Add(offer);
            if (maxCount > 0 && offers.Count >= maxCount)
            {
                break;
            }
        }

        return offers;
    }

    public static TeamRosterSummaryData GetCurrentTeamRosterSummary()
    {
        EnsureRosterStatuses();
        return TeamRosterService.GetRosterSummary(CurrentTeam);
    }

    public static List<PlayerData> GetCurrentTeamNhlPlayers()
    {
        EnsureRosterStatuses();
        return TeamRosterService.GetNhlPlayers(CurrentTeam);
    }

    public static List<PlayerData> GetCurrentTeamFarmPlayers()
    {
        EnsureRosterStatuses();
        return TeamRosterService.GetFarmPlayers(CurrentTeam);
    }

    public static List<PlayerData> GetCurrentTeamReservePlayers()
    {
        EnsureRosterStatuses();
        return TeamRosterService.GetReservePlayers(CurrentTeam);
    }

    public static RosterMoveResultData SendCurrentTeamPlayerToFarm(string playerId)
    {
        return MoveCurrentTeamPlayerWithState(playerId, TeamRosterService.SendPlayerToFarm);
    }

    public static RosterMoveResultData CallUpCurrentTeamPlayerToNhl(string playerId)
    {
        return MoveCurrentTeamPlayer(playerId, TeamRosterService.CallUpPlayerToNhl);
    }

    public static RosterMoveResultData CallUpCurrentTeamPlayer(string playerId)
    {
        return CallUpCurrentTeamPlayerToNhl(playerId);
    }

    public static RosterMoveResultData MoveCurrentTeamPlayerToReserve(string playerId)
    {
        return MoveCurrentTeamPlayerWithState(playerId, TeamRosterService.MovePlayerToReserve);
    }

    public static RosterMoveResultData MoveCurrentTeamReservePlayerToNhl(string playerId)
    {
        return MoveCurrentTeamPlayer(playerId, TeamRosterService.MoveReservePlayerToNhl);
    }

    public static RosterMoveResultData MoveCurrentTeamReservePlayerToFarm(string playerId)
    {
        return MoveCurrentTeamPlayer(playerId, TeamRosterService.MoveReservePlayerToFarm);
    }

    private static RosterMoveResultData MoveCurrentTeamPlayer(
        string playerId,
        Func<TeamData, string, RosterMoveResultData> moveAction)
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            return new RosterMoveResultData
            {
                Success = false,
                Message = "Команда не выбрана",
                PlayerId = playerId,
                PlayerName = "",
                FromStatus = "",
                ToStatus = "",
                UpdatedAtUtc = DateTime.UtcNow.ToString("o")
            };
        }

        EnsureContracts();
        EnsureRosterStatuses();
        EnsureWaivers();
        RosterMoveResultData result = moveAction(CurrentTeam, playerId);
        EnsureWaivers();
        LineupService.SyncScratchPlayers(CurrentTeam);
        LineupService.ValidateLineup(CurrentTeam, out string lineupMessage);
        SpecialTeamsService.ValidateSpecialTeams(CurrentTeam, out string specialTeamsMessage);
        EnsureRolesAndUsage();
        EnsureMorale();
        EnsureChemistry();
        SaveLoadService.Save(CurrentState);
        return result;
    }

    private static RosterMoveResultData MoveCurrentTeamPlayerWithState(
        string playerId,
        Func<GameState, TeamData, string, RosterMoveResultData> moveAction)
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            return new RosterMoveResultData
            {
                Success = false,
                Message = "Команда не выбрана",
                PlayerId = playerId,
                PlayerName = "",
                FromStatus = "",
                ToStatus = "",
                UpdatedAtUtc = DateTime.UtcNow.ToString("o")
            };
        }

        EnsureContracts();
        EnsureRosterStatuses();
        EnsureWaivers();
        RosterMoveResultData result = moveAction(CurrentState, CurrentTeam, playerId);
        EnsureWaivers();
        LineupService.SyncScratchPlayers(CurrentTeam);
        LineupService.ValidateLineup(CurrentTeam, out string lineupMessage);
        SpecialTeamsService.ValidateSpecialTeams(CurrentTeam, out string specialTeamsMessage);
        EnsureRolesAndUsage();
        EnsureMorale();
        EnsureChemistry();
        SaveLoadService.Save(CurrentState);
        return result;
    }

    public static List<DraftPickOwnershipData> GetOwnedDraftPicks(string teamId)
    {
        EnsureDraftPickOwnership();
        return DraftPickOwnershipService.GetOwnedPicks(CurrentState, teamId);
    }

    public static void EnsureFreeAgents()
    {
        if (CurrentState == null)
        {
            return;
        }

        EnsureLeagueRules();
        EnsureContracts();
        FreeAgentService.EnsureFreeAgentData(CurrentState);
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureContractExtensions();
    }

    public static void EnsureBetterFreeAgency()
    {
        if (CurrentState == null)
        {
            return;
        }

        EnsureLeagueRules();
        CurrentState.EnsureFreeAgencyOfferHistory();
        if (CurrentState.FreeAgentPool == null)
        {
            FreeAgentService.EnsureFreeAgentData(CurrentState);
        }
        else
        {
            BetterFreeAgencyService.EnsureFreeAgentEvaluations(CurrentState);
        }
    }

    public static FreeAgencyMarketSummaryData GetFreeAgencyMarketSummary()
    {
        EnsureBetterFreeAgency();
        return BetterFreeAgencyService.BuildMarketSummary(CurrentState);
    }

    public static List<PlayerData> GetEvaluatedFreeAgents()
    {
        EnsureBetterFreeAgency();
        return BetterFreeAgencyService.GetFreeAgents(CurrentState);
    }

    public static FreeAgentOfferData MakeCurrentTeamFreeAgentOffer(string playerId, int salary, int years)
    {
        if (CurrentState == null || CurrentTeam == null)
        {
            return new FreeAgentOfferData
            {
                OfferId = Guid.NewGuid().ToString("N"),
                PlayerId = playerId,
                OfferedSalary = salary,
                OfferedYears = years,
                Decision = "Invalid",
                DecisionReason = "Команда не выбрана",
                CreatedAtUtc = DateTime.UtcNow.ToString("o"),
                Source = "User"
            };
        }

        EnsureLeagueRules();
        EnsureContracts();
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureFreeAgents();
        EnsureBetterFreeAgency();

        FreeAgentOfferData offer = BetterFreeAgencyService.MakeUserFreeAgentOffer(
            CurrentState,
            playerId,
            salary,
            years);

        if (offer != null && offer.Accepted)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            EnsureRosterStatuses();
            EnsureWaivers();
            LineupService.SyncScratchPlayers(CurrentTeam);
            LineupService.ValidateLineup(CurrentTeam, out string validationMessage);
            SpecialTeamsService.ValidateSpecialTeams(CurrentTeam, out string specialTeamsMessage);
            EnsureRolesAndUsage();
            EnsureMorale();
            EnsureChemistry();
            EnsureContractExtensions();
            RunCpuRosterManagement("AfterFreeAgentSigning", false);
            EnsureOwnerGoals();
        }

        EnsureBetterFreeAgency();
        SaveLoadService.Save(CurrentState);
        return offer;
    }

    public static List<FreeAgentOfferData> GetRecentFreeAgencyOffers(int maxCount)
    {
        EnsureBetterFreeAgency();
        List<FreeAgentOfferData> offers = new List<FreeAgentOfferData>();
        if (CurrentState == null
            || CurrentState.FreeAgencyOfferHistory == null
            || CurrentState.FreeAgencyOfferHistory.Offers == null)
        {
            return offers;
        }

        for (int i = CurrentState.FreeAgencyOfferHistory.Offers.Count - 1; i >= 0; i--)
        {
            FreeAgentOfferData offer = CurrentState.FreeAgencyOfferHistory.Offers[i];
            if (offer == null)
            {
                continue;
            }

            offers.Add(offer);
            if (maxCount > 0 && offers.Count >= maxCount)
            {
                break;
            }
        }

        return offers;
    }

    public static List<FreeAgentOfferData> RunCpuFreeAgency(int maxSignings)
    {
        if (CurrentState == null)
        {
            return new List<FreeAgentOfferData>();
        }

        EnsureLeagueRules();
        EnsureContracts();
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureFreeAgents();
        EnsureBetterFreeAgency();

        string userTeamId = CurrentTeam != null ? CurrentTeam.Id : CurrentState.SelectedTeamId;
        List<FreeAgentOfferData> offers = CpuFreeAgencyService.RunCpuFreeAgency(
            CurrentState,
            userTeamId,
            maxSignings <= 0 ? 5 : maxSignings);

        bool hasAccepted = false;
        foreach (FreeAgentOfferData offer in offers)
        {
            if (offer != null && offer.Accepted)
            {
                hasAccepted = true;
                break;
            }
        }

        if (hasAccepted)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            EnsureRosterStatuses();
            EnsureWaivers();
            EnsureLineups();
            EnsureSpecialTeamsAndTactics();
            EnsureRolesAndUsage();
            EnsureMorale();
            EnsureChemistry();
            EnsureContractExtensions();
            RunCpuRosterManagement("AfterCpuFreeAgency", false);
            EnsureOwnerGoals();
        }

        EnsureBetterFreeAgency();
        SaveLoadService.Save(CurrentState);
        return offers;
    }

    public static bool TryTradePlayer(
        string userTeamPlayerId,
        string otherTeamId,
        string otherTeamPlayerId,
        out TradeProposalData proposal,
        out string message)
    {
        proposal = null;

        if (CurrentState == null)
        {
            message = "Состояние игры не найдено";
            return false;
        }

        EnsureLeagueRules();
        EnsureContracts();
        EnsureTradeHistory();
        EnsureDraftPickOwnership();
        EnsureWaivers();

        bool result = TradeService.TryCreateOneForOneTrade(
            CurrentState,
            userTeamPlayerId,
            otherTeamId,
            otherTeamPlayerId,
            out proposal,
            out message);

        if (result)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            EnsureRosterStatuses();
            EnsureWaivers();
            EnsureLineups();
            EnsureSpecialTeamsAndTactics();
            EnsureRolesAndUsage();
            EnsureMorale();
            EnsureChemistry();
            EnsureContractExtensions();
            RunCpuRosterManagement("AfterTrade", false);
            EnsureOwnerGoals();
        }

        if (proposal != null)
        {
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static bool TryTradeAssets(
        List<TradeAssetData> assetsFromUserTeam,
        string otherTeamId,
        List<TradeAssetData> assetsFromOtherTeam,
        out TradeProposalData proposal,
        out string message)
    {
        proposal = null;

        if (CurrentState == null)
        {
            message = "Состояние игры не найдено";
            return false;
        }

        EnsureLeagueRules();
        EnsureContracts();
        EnsureTradeHistory();
        EnsureDraftPickOwnership();
        EnsureWaivers();

        bool result = TradeService.TryCreateTradeWithAssets(
            CurrentState,
            assetsFromUserTeam,
            otherTeamId,
            assetsFromOtherTeam,
            out proposal,
            out message);

        if (result)
        {
            EventNewsService.CreateTradeNews(CurrentState, proposal);
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            EnsureRosterStatuses();
            EnsureWaivers();
            EnsureLineups();
            EnsureSpecialTeamsAndTactics();
            EnsureRolesAndUsage();
            EnsureMorale();
            EnsureChemistry();
            EnsureContractExtensions();
            RunCpuRosterManagement("AfterTrade", false);
            EnsureOwnerGoals();
        }

        if (proposal != null)
        {
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static bool TrySignFreeAgent(
        string playerId,
        out FreeAgentSigningData signing,
        out string message)
    {
        signing = null;

        if (CurrentState == null)
        {
            message = "Состояние игры не найдено";
            return false;
        }

        EnsureLeagueRules();
        EnsureContracts();
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureFreeAgents();

        bool result = FreeAgentService.TrySignFreeAgent(CurrentState, playerId, out signing, out message);
        if (result)
        {
            EventNewsService.CreateFreeAgentSigningNews(CurrentState, signing);
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            EnsureRosterStatuses();
            EnsureWaivers();
            LineupService.SyncScratchPlayers(CurrentTeam);
            LineupService.ValidateLineup(CurrentTeam, out string validationMessage);
            SpecialTeamsService.ValidateSpecialTeams(CurrentTeam, out string specialTeamsMessage);
            EnsureRolesAndUsage();
            EnsureMorale();
            EnsureChemistry();
            EnsureContractExtensions();
            RunCpuRosterManagement("AfterFreeAgentSigning", false);
            EnsureOwnerGoals();
        }

        if (signing != null)
        {
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static bool TrySignProspectToElc(
        string prospectId,
        out ProspectSigningData signing,
        out string message)
    {
        signing = null;

        if (CurrentState == null)
        {
            message = "Состояние игры не найдено";
            return false;
        }

        EnsureLeagueRules();
        EnsureContracts();
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureProspectSigningHistory();
        EnsureCoachingStaff();

        bool result = ProspectSigningService.TrySignProspectToElc(
            CurrentState,
            prospectId,
            out signing,
            out message);

        if (result)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            EnsureRosterStatuses();
            EnsureWaivers();
            LineupService.SyncScratchPlayers(CurrentTeam);
            LineupService.ValidateLineup(CurrentTeam, out string validationMessage);
            SpecialTeamsService.ValidateSpecialTeams(CurrentTeam, out string specialTeamsMessage);
            EnsureRolesAndUsage();
            EnsureMorale();
            EnsureCoachingStaff();
            EnsureChemistry();
            EnsureContractExtensions();
            RunCpuRosterManagement("AfterProspectSigning", false);
            EnsureOwnerGoals();
        }

        if (signing != null)
        {
            SaveLoadService.Save(CurrentState);
        }

        return result;
    }

    public static bool CanStartNextSeason(out string message)
    {
        return SeasonTransitionService.CanStartNextSeason(CurrentState, out message);
    }

    public static bool StartNextSeason(out string message)
    {
        if (CurrentState == null)
        {
            message = "Состояние игры не найдено";
            return false;
        }

        EnsureLeagueRules();
        EnsureContracts();
        EnsureSeasonHistory();
        EnsureDevelopmentHistory();
        EnsureWaivers();
        EnsureLineups();
        EnsureFatigue();
        EnsureInjuries();
        EnsureSpecialTeamsAndTactics();
        EnsureCoachingStaff();

        bool result = SeasonTransitionService.StartNextSeason(CurrentState, out message);
        if (result)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            EnsureRosterStatuses();
            EnsureWaivers();
            EnsureLineups();
            EnsureFatigue();
            EnsureInjuries();
            EnsureSpecialTeamsAndTactics();
            EnsureRolesAndUsage();
            EnsureMorale();
            EnsureCoachingStaff();
            EnsureChemistry();
            EnsureContractExtensions();
            RunCpuRosterManagement("AfterSeasonTransition", false);
            EnsureOwnerGoals();
            EnsureGmCareer();
            GameStateValidationService.Validate(CurrentState);
            BalanceReportService.Generate(CurrentState);
            SaveLoadService.Save(CurrentState);
        }
        else
        {
            Debug.LogWarning(message);
        }

        return result;
    }

    public static ScheduleGameData GetNextGameForCurrentTeam()
    {
        EnsureSeasonScheduleAndStandingsOnly();

        if (CurrentState == null || CurrentState.Season == null || CurrentTeam == null)
        {
            return null;
        }

        ScheduleGameData nextGame = FindNextRegularSeasonUserGame();
        if (nextGame != null)
        {
            return nextGame;
        }

        CurrentState.Season.IsSeasonFinished = GetNextUnplayedGame(CurrentState.Season) == null;
        return null;
    }

    public static List<ScheduleGameData> GetGamesForCurrentDay()
    {
        EnsureSeasonScheduleAndStandingsOnly();

        if (CurrentState == null || CurrentState.Season == null)
        {
            return new List<ScheduleGameData>();
        }

        List<ScheduleGameData> gamesForDay = new List<ScheduleGameData>();
        foreach (ScheduleGameData game in CurrentState.Season.Schedule)
        {
            if (game != null && game.DayNumber == CurrentState.Season.CurrentDay)
            {
                gamesForDay.Add(game);
            }
        }

        return gamesForDay;
    }

    public static List<MatchResultData> SimulateNextLeagueDay()
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        List<MatchResultData> results = new List<MatchResultData>();

        try
        {
        if (CurrentState == null)
        {
            Debug.LogWarning("Нельзя симулировать матч: активная игра не найдена");
            return results;
        }

        EnsureSeason();
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureLineups();
        EnsureFatigue();
        EnsureInjuries();
        EnsureSpecialTeamsAndTactics();
        EnsureChemistry();
        RunCpuRosterManagement("BeforeLeagueDay", false);

        if (CurrentState.Season == null || CurrentState.Season.IsSeasonFinished)
        {
            Debug.Log("Сезон завершён");
            return results;
        }

        int simulatedDay = GetNextUnplayedDay(CurrentState.Season);
        if (simulatedDay <= 0)
        {
            CurrentState.Season.IsSeasonFinished = true;
            SaveLoadService.Save(CurrentState);
            return results;
        }

        List<ScheduleGameData> gamesForDay = GetGamesForDay(CurrentState.Season, simulatedDay);
        if (CurrentTeamPlaysOnDay(gamesForDay) && !ValidateCurrentTeamCanPlay(out string validationMessage))
        {
            Debug.LogWarning(validationMessage);
            return results;
        }

        HashSet<string> playedTeamIds = new HashSet<string>();
        foreach (ScheduleGameData game in gamesForDay)
        {
            TeamData homeTeam = FindTeam(CurrentState.Teams, game.HomeTeamId);
            TeamData awayTeam = FindTeam(CurrentState.Teams, game.AwayTeamId);

            if (homeTeam == null || awayTeam == null)
            {
                Debug.LogWarning("Нельзя симулировать матч: команда из календаря не найдена");
                continue;
            }

            EnsureTeamPlayers(homeTeam);
            EnsureTeamPlayers(awayTeam);
            TeamRosterService.EnsureRosterStatusesForTeam(homeTeam);
            TeamRosterService.EnsureRosterStatusesForTeam(awayTeam);
            InjuryService.EnsureInjuryFieldsForTeam(homeTeam);
            InjuryService.EnsureInjuryFieldsForTeam(awayTeam);
            RepairCpuTeamForGame(homeTeam);
            RepairCpuTeamForGame(awayTeam);
            LineupService.EnsureLineup(homeTeam);
            LineupService.EnsureLineup(awayTeam);
            PlayerFatigueService.EnsureFatigueForTeam(homeTeam);
            PlayerFatigueService.EnsureFatigueForTeam(awayTeam);
            SpecialTeamsService.EnsureSpecialTeams(homeTeam);
            SpecialTeamsService.EnsureSpecialTeams(awayTeam);
            TacticsService.EnsureTactics(homeTeam);
            TacticsService.EnsureTactics(awayTeam);
            CoachingStaffService.EnsureStaffForTeam(homeTeam);
            CoachingStaffService.EnsureStaffForTeam(awayTeam);
            IceTimeService.EnsureUsageForTeam(homeTeam);
            IceTimeService.EnsureUsageForTeam(awayTeam);
            ChemistryService.EnsureChemistryForTeam(homeTeam);
            ChemistryService.EnsureChemistryForTeam(awayTeam);
            CurrentState.EnsureMatchHistory();

            MatchResultData result = MatchSimulator.Simulate(homeTeam, awayTeam);
            IceTimeService.EnsureUsageForTeam(homeTeam);
            IceTimeService.EnsureUsageForTeam(awayTeam);
            IceTimeService.ApplyLastGameIceTime(homeTeam);
            IceTimeService.ApplyLastGameIceTime(awayTeam);
            result.PlayerStats = PlayerGameStatsGenerator.Generate(result, homeTeam, awayTeam);
            game.Result = result;
            game.IsPlayed = true;
            CurrentState.MatchHistory.Add(result);
            CurrentState.TotalGamesSimulated++;
            CurrentState.Season.CurrentGameIndex = game.GameNumber;
            results.Add(result);

            StandingsService.ApplyMatchResult(CurrentState.Season, result);
            PlayerStatsService.ApplyGameStats(CurrentState.Season, result.PlayerStats);
            PlayerFatigueService.ApplyFatigueAfterMatch(homeTeam, awayTeam);
            InjuryService.ApplyInjuryChecksAfterMatch(CurrentState, homeTeam, awayTeam, "RegularSeason");
            playedTeamIds.Add(homeTeam.Id);
            playedTeamIds.Add(awayTeam.Id);

            if (CurrentTeam != null && (game.HomeTeamId == CurrentTeam.Id || game.AwayTeamId == CurrentTeam.Id))
            {
                CurrentState.LastMatchResult = result;
            }
        }

        RecoverNonPlayingTeamsForDay(playedTeamIds);
        AdvanceRosterDaysForTeams();
        InjuryService.AdvanceInjuryRecovery(CurrentState);
        WaiverService.AdvanceWaiverDay(CurrentState);
        RunCpuRosterManagement("AfterLeagueDay", false);
        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureLineups();
        EnsureSpecialTeamsAndTactics();
        EnsureRolesAndUsage();
        EnsureChemistry();

        CurrentState.Season.CurrentDay = simulatedDay + 1;
        if (GetNextUnplayedGame(CurrentState.Season) == null)
        {
            CurrentState.Season.IsSeasonFinished = true;
        }

        UpdateMoraleAfterGameDay();
        EnsureChemistry();
        EnsureOwnerGoals();
        if (results.Count > 0)
        {
            TutorialService.MarkStepCompleted(CurrentState, TutorialConfig.StepSimulateFirstDay);
        }

        SaveLoadService.Save(CurrentState);
        Debug.Log("Сыграно матчей игрового дня " + simulatedDay + ": " + results.Count);

        return results;
        }
        finally
        {
            stopwatch.Stop();
            PerformanceTimerService.RecordSimulateDay(CurrentState, stopwatch.ElapsedMilliseconds);
        }
    }

    public static MatchResultData SimulateMatchForCurrentTeam()
    {
        return SimulateNextScheduledGame();
    }

    public static MatchResultData SimulateNextScheduledGame()
    {
        List<MatchResultData> results = SimulateNextLeagueDay();

        if (results.Count == 0)
        {
            return null;
        }

        if (CurrentTeam != null)
        {
            foreach (MatchResultData result in results)
            {
                if (result.HomeTeamId == CurrentTeam.Id || result.AwayTeamId == CurrentTeam.Id)
                {
                    return result;
                }
            }
        }

        return results[0];
    }

    public static MatchResultData SimulateNextUserGameFast()
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (CurrentState == null)
            {
                Debug.LogWarning("Нельзя симулировать матч: активная игра не найдена");
                return null;
            }

            EnsureSeasonScheduleAndStandingsOnly();
            if (CurrentTeam == null)
            {
                CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            }

            if (CurrentTeam == null)
            {
                Debug.LogWarning("Нельзя симулировать матч: команда пользователя не найдена");
                return null;
            }

            EnsureTeamPlayers(CurrentTeam);
            if (!ValidateCurrentTeamCanPlay(out string validationMessage))
            {
                Debug.LogWarning(validationMessage);
                return null;
            }

            ScheduleGameData userGame = FindNextRegularSeasonUserGame();
            if (userGame == null)
            {
                CurrentState.Season.IsSeasonFinished = GetNextUnplayedGame(CurrentState.Season) == null;
                Debug.LogWarning("Ближайший матч пользователя не найден");
                return null;
            }

            CurrentState.Season.IsSeasonFinished = false;
            int targetDay = userGame.DayNumber;
            MatchResultData userResult = null;
            int safetyCounter = 0;

            while (safetyCounter < 500)
            {
                safetyCounter++;
                int simulatedDay = GetNextUnplayedDay(CurrentState.Season);
                if (simulatedDay <= 0 || simulatedDay > targetDay)
                {
                    break;
                }

                List<ScheduleGameData> gamesForDay = GetGamesForDay(CurrentState.Season, simulatedDay);
                if (CurrentTeamPlaysOnDay(gamesForDay) && !ValidateCurrentTeamCanPlay(out validationMessage))
                {
                    Debug.LogWarning(validationMessage);
                    return null;
                }

                bool simulatedAnyGame = false;
                HashSet<string> playedTeamIds = new HashSet<string>();
                foreach (ScheduleGameData game in gamesForDay)
                {
                    MatchResultData result = SimulateScheduledRegularSeasonGameFast(game, playedTeamIds);
                    if (result == null)
                    {
                        continue;
                    }

                    simulatedAnyGame = true;
                    if (CurrentTeam != null && (game.HomeTeamId == CurrentTeam.Id || game.AwayTeamId == CurrentTeam.Id))
                    {
                        userResult = result;
                        CurrentState.LastMatchResult = result;
                    }
                }

                if (!simulatedAnyGame)
                {
                    Debug.LogWarning("Не удалось симулировать игровой день " + simulatedDay);
                    break;
                }

                CompleteFastRegularSeasonDay(simulatedDay, playedTeamIds);
            }

            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            if (GetNextUnplayedGame(CurrentState.Season) == null)
            {
                CurrentState.Season.IsSeasonFinished = true;
            }

            SaveLoadService.Save(CurrentState);
            if (userResult != null)
            {
                Debug.Log("Симулирован игровой день до матча пользователя: " + userResult.Summary);
            }

            return userResult;
        }
        finally
        {
            stopwatch.Stop();
            PerformanceTimerService.RecordSimulateDay(CurrentState, stopwatch.ElapsedMilliseconds);
        }
    }

    private static MatchResultData SimulateScheduledRegularSeasonGameFast(ScheduleGameData game, HashSet<string> playedTeamIds)
    {
        if (CurrentState == null || CurrentState.Season == null || game == null || game.IsPlayed)
        {
            return null;
        }

        TeamData homeTeam = FindTeam(CurrentState.Teams, game.HomeTeamId);
        TeamData awayTeam = FindTeam(CurrentState.Teams, game.AwayTeamId);
        if (homeTeam == null || awayTeam == null)
        {
            Debug.LogWarning("Нельзя симулировать матч: команда из календаря не найдена");
            return null;
        }

        bool homeIsUserTeam = CurrentTeam != null && homeTeam.Id == CurrentTeam.Id;
        bool awayIsUserTeam = CurrentTeam != null && awayTeam.Id == CurrentTeam.Id;
        bool isUserGame = homeIsUserTeam || awayIsUserTeam;
        if (!PrepareTeamForFastSimulation(homeTeam, homeIsUserTeam, out string homeMessage))
        {
            Debug.LogWarning(homeMessage);
            return null;
        }

        if (!PrepareTeamForFastSimulation(awayTeam, awayIsUserTeam, out string awayMessage))
        {
            Debug.LogWarning(awayMessage);
            return null;
        }

        CurrentState.EnsureMatchHistory();
        MatchResultData result = MatchSimulator.SimulateFast(homeTeam, awayTeam);
        if (isUserGame)
        {
            result.PlayerStats = PlayerGameStatsGenerator.Generate(result, homeTeam, awayTeam);
            IceTimeService.ApplyLastGameIceTime(homeTeam);
            IceTimeService.ApplyLastGameIceTime(awayTeam);
        }
        else
        {
            result.PlayerStats = new List<PlayerGameStatData>();
        }

        game.Result = result;
        game.IsPlayed = true;
        CurrentState.MatchHistory.Add(result);
        CurrentState.TotalGamesSimulated++;
        CurrentState.Season.CurrentGameIndex = game.GameNumber;

        StandingsService.ApplyMatchResult(CurrentState.Season, result);
        if (isUserGame)
        {
            PlayerStatsService.ApplyGameStats(CurrentState.Season, result.PlayerStats);
            PlayerFatigueService.ApplyFatigueAfterMatch(homeTeam, awayTeam);
            InjuryService.ApplyInjuryChecksAfterMatch(CurrentState, homeTeam, awayTeam, "RegularSeasonFast");
        }

        if (playedTeamIds != null)
        {
            playedTeamIds.Add(homeTeam.Id);
            playedTeamIds.Add(awayTeam.Id);
        }

        return result;
    }

    private static bool PrepareTeamForFastSimulation(TeamData team, bool isUserTeam, out string message)
    {
        message = "";
        if (team == null)
        {
            message = "Команда матча не найдена";
            return false;
        }

        if (isUserTeam)
        {
            EnsureTeamPlayers(team);
            TeamRosterService.EnsureRosterStatusesForTeam(team);
            InjuryService.EnsureInjuryFieldsForTeam(team);
            PlayerFatigueService.EnsureFatigueForTeam(team);
            TryAutoFixCurrentTeamForGame(out message);
        }
        else
        {
            if (team.Players == null || team.Players.Count == 0)
            {
                EnsureTeamPlayers(team);
            }

            TeamRosterService.EnsureRosterStatusesForTeam(team);
            bool needsRepair = team.Lineup == null
                || !LineupService.ValidateLineup(team, out message)
                || LineupService.HasInjuredActivePlayers(team, out message)
                || SpecialTeamsService.ValidateSpecialTeams(team, out message) == false;

            if (needsRepair)
            {
                RepairCpuTeamForGame(team);
            }
        }

        LineupService.EnsureLineup(team);
        SpecialTeamsService.EnsureSpecialTeams(team);
        TacticsService.EnsureTactics(team);
        return true;
    }

    private static void CompleteFastRegularSeasonDay(int simulatedDay, HashSet<string> playedTeamIds)
    {
        if (CurrentState == null || CurrentState.Season == null)
        {
            return;
        }

        RecoverNonPlayingTeamsForDay(playedTeamIds);
        AdvanceRosterDaysForTeams();
        InjuryService.AdvanceInjuryRecovery(CurrentState);
        WaiverService.AdvanceWaiverDay(CurrentState);
        CurrentState.Season.CurrentDay = Mathf.Max(CurrentState.Season.CurrentDay, simulatedDay + 1);
        CurrentState.Season.IsSeasonFinished = GetNextUnplayedGame(CurrentState.Season) == null;
        TutorialService.MarkStepCompleted(CurrentState, TutorialConfig.StepSimulateFirstDay);
    }

    public static bool PrepareNextUserMatch(out PreGameSetupData setup, out string message)
    {
        setup = null;
        message = "";
        LastPostGameSummary = null;

        if (CurrentState == null)
        {
            message = "Активная игра не найдена";
            return false;
        }

        if (IsLiveMatchActive())
        {
            setup = CurrentPreGameSetup;
            message = "Live-матч уже идёт";
            return false;
        }

        if (CurrentTeam == null)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        }

        if (CurrentTeam == null)
        {
            message = "Команда пользователя не найдена";
            return false;
        }

        EnsureSeasonScheduleAndStandingsOnly();
        EnsureTeamPlayers(CurrentTeam);
        TeamRosterService.EnsureRosterStatusesForTeam(CurrentTeam);
        InjuryService.EnsureInjuryFieldsForTeam(CurrentTeam);
        PlayerFatigueService.EnsureFatigueForTeam(CurrentTeam);
        TryAutoFixCurrentTeamForGame(out string autoFixMessage);

        ScheduleGameData nextGame = FindNextRegularSeasonUserGame();
        if (nextGame != null)
        {
            setup = CreatePreGameSetup(nextGame, false);
            CurrentPreGameSetup = setup;
            message = setup.AvailabilityMessage;
            return setup.CanStartMatch;
        }

        setup = TryCreatePlayoffPreGameSetup();
        if (setup != null)
        {
            CurrentPreGameSetup = setup;
            message = setup.AvailabilityMessage;
            return setup.CanStartMatch;
        }

        message = "Ближайший матч пользователя не найден";
        return false;
    }

    public static bool StartPreparedLiveMatch(out string message)
    {
        message = "";
        if (CurrentPreGameSetup == null)
        {
            if (!PrepareNextUserMatch(out PreGameSetupData setup, out message))
            {
                CurrentPreGameSetup = setup;
                return false;
            }
        }

        if (CurrentPreGameSetup == null || !CurrentPreGameSetup.CanStartMatch)
        {
            message = CurrentPreGameSetup == null ? "Матч не подготовлен" : CurrentPreGameSetup.AvailabilityMessage;
            return false;
        }

        TeamData homeTeam = FindTeam(CurrentState.Teams, CurrentPreGameSetup.HomeTeamId);
        TeamData awayTeam = FindTeam(CurrentState.Teams, CurrentPreGameSetup.AwayTeamId);
        if (homeTeam == null || awayTeam == null)
        {
            message = "Команда матча не найдена";
            return false;
        }

        ScheduleGameData scheduledGame = FindScheduledGame(CurrentPreGameSetup.ScheduledGameId);
        if (scheduledGame == null && CurrentPreGameSetup.IsPlayoffGame)
        {
            scheduledGame = new ScheduleGameData
            {
                GameId = CurrentPreGameSetup.ScheduledGameId,
                DayNumber = CurrentState.Season == null ? 0 : CurrentState.Season.CurrentDay,
                HomeTeamId = homeTeam.Id,
                HomeTeamName = TeamIdentityService.GetDisplayName(homeTeam),
                AwayTeamId = awayTeam.Id,
                AwayTeamName = TeamIdentityService.GetDisplayName(awayTeam)
            };
        }

        CurrentLiveMatch = LiveMatchSimulator.CreateLiveMatch(
            scheduledGame,
            homeTeam,
            awayTeam,
            CurrentTeam == null ? CurrentState.SelectedTeamId : CurrentTeam.Id,
            CurrentPreGameSetup.IsPlayoffGame);
        message = "Матч начат";
        return true;
    }

    public static void TickLiveMatch(float realDeltaSeconds)
    {
        if (CurrentLiveMatch == null || CurrentLiveMatch.IsCompleted)
        {
            return;
        }

        TeamData homeTeam = FindTeam(CurrentState.Teams, CurrentLiveMatch.HomeTeamId);
        TeamData awayTeam = FindTeam(CurrentState.Teams, CurrentLiveMatch.AwayTeamId);
        int ticks = Mathf.Max(1, CurrentLiveMatch.SpeedMultiplier);
        for (int i = 0; i < ticks; i++)
        {
            LiveMatchSimulator.AdvanceTick(CurrentLiveMatch, homeTeam, awayTeam);
            if (CurrentLiveMatch.IsCompleted)
            {
                LastPostGameSummary = LiveMatchResultAdapter.ToPostGameSummary(CurrentLiveMatch);
                break;
            }
        }
    }

    public static void SetLiveMatchPaused(bool isPaused)
    {
        if (CurrentLiveMatch != null)
        {
            CurrentLiveMatch.IsPaused = isPaused;
        }
    }

    public static void SetLiveMatchSpeed(int speedMultiplier)
    {
        if (CurrentLiveMatch != null)
        {
            CurrentLiveMatch.SpeedMultiplier = Mathf.Clamp(speedMultiplier, 1, 4);
        }
    }

    public static void SkipLiveMatchPeriod()
    {
        if (CurrentLiveMatch == null || CurrentState == null)
        {
            return;
        }

        TeamData homeTeam = FindTeam(CurrentState.Teams, CurrentLiveMatch.HomeTeamId);
        TeamData awayTeam = FindTeam(CurrentState.Teams, CurrentLiveMatch.AwayTeamId);
        LiveMatchSimulator.AdvanceUntilPeriodEnd(CurrentLiveMatch, homeTeam, awayTeam);
        if (CurrentLiveMatch.IsCompleted)
        {
            LastPostGameSummary = LiveMatchResultAdapter.ToPostGameSummary(CurrentLiveMatch);
        }
    }

    public static void SkipLiveMatchToEnd()
    {
        if (CurrentLiveMatch == null || CurrentState == null)
        {
            return;
        }

        TeamData homeTeam = FindTeam(CurrentState.Teams, CurrentLiveMatch.HomeTeamId);
        TeamData awayTeam = FindTeam(CurrentState.Teams, CurrentLiveMatch.AwayTeamId);
        LiveMatchSimulator.AdvanceUntilMatchEnd(CurrentLiveMatch, homeTeam, awayTeam);
        LastPostGameSummary = LiveMatchResultAdapter.ToPostGameSummary(CurrentLiveMatch);
    }

    public static bool ChangeLiveMatchGoalie(string teamId, out string message)
    {
        message = "";
        TeamData team = FindTeam(CurrentState == null ? null : CurrentState.Teams, teamId);
        return LiveMatchGoalieService.ChangeGoalie(CurrentLiveMatch, team, out message);
    }

    public static bool PullLiveMatchGoalie(string teamId, out string message)
    {
        message = "";
        TeamData team = FindTeam(CurrentState == null ? null : CurrentState.Teams, teamId);
        return LiveMatchGoalieService.PullGoalie(CurrentLiveMatch, team, out message);
    }

    public static bool ReturnLiveMatchGoalie(string teamId, out string message)
    {
        message = "";
        TeamData team = FindTeam(CurrentState == null ? null : CurrentState.Teams, teamId);
        return LiveMatchGoalieService.ReturnGoalie(CurrentLiveMatch, team, out message);
    }

    public static bool SetLiveMatchTactic(string teamId, string tacticName, out string message)
    {
        message = "";
        TeamData team = FindTeam(CurrentState == null ? null : CurrentState.Teams, teamId);
        if (team == null)
        {
            message = "Команда не найдена";
            return false;
        }

        LiveMatchTacticsService.SetTeamTactic(CurrentLiveMatch, team, tacticName);
        message = "Тактика обновлена";
        return true;
    }

    public static bool PrepareCurrentLivePostGameSummary(out string message)
    {
        message = "";
        if (CurrentLiveMatch == null)
        {
            message = "Live-матч не найден";
            return false;
        }

        if (!CurrentLiveMatch.IsCompleted)
        {
            SkipLiveMatchToEnd();
        }

        LastPostGameSummary = LiveMatchResultAdapter.ToPostGameSummary(CurrentLiveMatch);
        return true;
    }

    public static bool CompleteCurrentLiveMatchAndApplyResult(out MatchResultData result, out string message)
    {
        return CompleteCurrentLiveMatchAndApplyResult(out result, out message, true);
    }

    public static bool CompleteCurrentLiveMatchAndApplyResult(
        out MatchResultData result,
        out string message,
        bool saveAfterApply)
    {
        result = null;
        message = "";
        if (CurrentLiveMatch == null)
        {
            message = "Live-матч не найден";
            return false;
        }

        if (!CurrentLiveMatch.IsCompleted)
        {
            SkipLiveMatchToEnd();
        }

        TeamData homeTeam = FindTeam(CurrentState.Teams, CurrentLiveMatch.HomeTeamId);
        TeamData awayTeam = FindTeam(CurrentState.Teams, CurrentLiveMatch.AwayTeamId);
        if (homeTeam == null || awayTeam == null)
        {
            message = "Команда матча не найдена";
            return false;
        }

        if (CurrentLiveMatch.IsPlayoffGame)
        {
            result = ApplyLivePlayoffResult(CurrentLiveMatch, homeTeam, awayTeam);
        }
        else
        {
            result = LiveMatchResultAdapter.ApplyLiveMatchResultToSeason(CurrentState, CurrentLiveMatch, homeTeam, awayTeam);
            SimulateCpuGamesForLiveMatchDay(CurrentLiveMatch.DayIndex);
            CompleteRegularSeasonDayAfterLiveMatch(CurrentLiveMatch.DayIndex);
        }

        LastPostGameSummary = LiveMatchResultAdapter.ToPostGameSummary(CurrentLiveMatch);
        CurrentLiveMatch = null;
        CurrentPreGameSetup = null;
        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        if (CurrentTeam != null)
        {
            TeamRosterService.EnsureRosterStatusesForTeam(CurrentTeam);
            InjuryService.EnsureInjuryFieldsForTeam(CurrentTeam);
            PlayerFatigueService.EnsureFatigueForTeam(CurrentTeam);
            LineupService.EnsureLineup(CurrentTeam);
            SpecialTeamsService.EnsureSpecialTeams(CurrentTeam);
        }

        if (saveAfterApply)
        {
            SaveLoadService.Save(CurrentState);
        }

        message = "Live-матч завершён";
        return result != null;
    }

    public static bool ExitLiveMatchAndFinish(out MatchResultData result, out string message)
    {
        return CompleteCurrentLiveMatchAndApplyResult(out result, out message);
    }

    public static bool IsLiveMatchActive()
    {
        return CurrentLiveMatch != null && CurrentLiveMatch.IsActive && !CurrentLiveMatch.IsCompleted;
    }

    private static ScheduleGameData FindNextRegularSeasonUserGame()
    {
        if (CurrentState == null || CurrentState.Season == null || CurrentState.Season.Schedule == null || CurrentTeam == null)
        {
            return null;
        }

        ScheduleGameData nextGame = null;
        foreach (ScheduleGameData game in CurrentState.Season.Schedule)
        {
            if (game == null
                || game.IsPlayed
                || (game.HomeTeamId != CurrentTeam.Id && game.AwayTeamId != CurrentTeam.Id))
            {
                continue;
            }

            if (nextGame == null
                || game.DayNumber < nextGame.DayNumber
                || (game.DayNumber == nextGame.DayNumber && game.GameNumber < nextGame.GameNumber))
            {
                nextGame = game;
            }
        }

        return nextGame;
    }

    private static bool SimulateCpuOnlyDaysBefore(int targetDayNumber, out string message)
    {
        message = "";
        if (CurrentState == null || CurrentState.Season == null)
        {
            message = "Сезон не найден";
            return false;
        }

        while (true)
        {
            int nextDay = GetNextUnplayedDay(CurrentState.Season);
            if (nextDay <= 0 || nextDay >= targetDayNumber)
            {
                return true;
            }

            List<ScheduleGameData> games = GetGamesForDay(CurrentState.Season, nextDay);
            if (CurrentTeamPlaysOnDay(games))
            {
                message = "Найден более ранний матч пользователя";
                return false;
            }

            SimulateCpuGamesForDay(nextDay, "");
            CompleteRegularSeasonDayAfterLiveMatch(nextDay);
        }
    }

    private static void SimulateCpuGamesForLiveMatchDay(int dayNumber)
    {
        if (dayNumber <= 0)
        {
            return;
        }

        SimulateCpuGamesForDay(dayNumber, CurrentLiveMatch == null ? "" : CurrentLiveMatch.ScheduledGameId);
    }

    private static List<MatchResultData> SimulateCpuGamesForDay(int dayNumber, string excludedGameId)
    {
        List<MatchResultData> results = new List<MatchResultData>();
        if (CurrentState == null || CurrentState.Season == null)
        {
            return results;
        }

        List<ScheduleGameData> gamesForDay = GetGamesForDay(CurrentState.Season, dayNumber);
        HashSet<string> playedTeamIds = new HashSet<string>();
        foreach (ScheduleGameData game in gamesForDay)
        {
            if (game == null || game.IsPlayed || game.GameId == excludedGameId)
            {
                continue;
            }

            if (CurrentTeam != null && (game.HomeTeamId == CurrentTeam.Id || game.AwayTeamId == CurrentTeam.Id))
            {
                continue;
            }

            MatchResultData result = SimulateScheduledRegularSeasonGameFast(game, playedTeamIds);
            if (result != null)
            {
                results.Add(result);
            }
        }

        RecoverNonPlayingTeamsForDay(playedTeamIds);
        return results;
    }

    private static void CompleteRegularSeasonDayAfterLiveMatch(int simulatedDay)
    {
        if (CurrentState == null || CurrentState.Season == null)
        {
            return;
        }

        AdvanceRosterDaysForTeams();
        InjuryService.AdvanceInjuryRecovery(CurrentState);
        WaiverService.AdvanceWaiverDay(CurrentState);
        RunCpuRosterManagement("AfterLiveMatchDay", false);
        CurrentState.Season.CurrentDay = simulatedDay + 1;
        if (GetNextUnplayedGame(CurrentState.Season) == null)
        {
            CurrentState.Season.IsSeasonFinished = true;
        }

        UpdateMoraleAfterGameDay();
        EnsureChemistry();
        EnsureOwnerGoals();
        TutorialService.MarkStepCompleted(CurrentState, TutorialConfig.StepSimulateFirstDay);
    }

    private static PreGameSetupData CreatePreGameSetup(ScheduleGameData game, bool isPlayoffGame)
    {
        TeamData homeTeam = FindTeam(CurrentState.Teams, game == null ? "" : game.HomeTeamId);
        TeamData awayTeam = FindTeam(CurrentState.Teams, game == null ? "" : game.AwayTeamId);
        TeamData opponent = CurrentTeam != null && homeTeam != null && homeTeam.Id == CurrentTeam.Id ? awayTeam : homeTeam;
        bool canStart = ValidateCurrentTeamCanPlay(out string lineupMessage);
        if (homeTeam != null && CurrentTeam != null && homeTeam.Id != CurrentTeam.Id)
        {
            RepairCpuTeamForGame(homeTeam);
        }

        if (awayTeam != null && CurrentTeam != null && awayTeam.Id != CurrentTeam.Id)
        {
            RepairCpuTeamForGame(awayTeam);
        }

        PlayerData starter = LineupService.GetStartingGoalie(CurrentTeam);
        PlayerData backup = LineupService.GetBackupGoalie(CurrentTeam);
        TeamIdentityData homeIdentity = homeTeam == null ? null : homeTeam.Identity;
        TeamIdentityData awayIdentity = awayTeam == null ? null : awayTeam.Identity;

        return new PreGameSetupData
        {
            ScheduledGameId = game == null ? "" : game.GameId,
            IsAvailable = homeTeam != null && awayTeam != null,
            AvailabilityMessage = canStart ? "Матч готов" : lineupMessage,
            IsPlayoffGame = isPlayoffGame,
            HomeTeamId = homeTeam == null ? "" : homeTeam.Id,
            HomeTeamName = TeamIdentityService.GetDisplayName(homeTeam),
            HomeLogoResourcePath = homeIdentity == null ? "" : homeIdentity.LogoResourcePath,
            HomeJerseyResourcePath = homeIdentity == null ? "" : homeIdentity.HomeJerseyResourcePath,
            HomeFullBodyResourcePath = homeIdentity == null ? "" : homeIdentity.FullBodyResourcePath,
            AwayTeamId = awayTeam == null ? "" : awayTeam.Id,
            AwayTeamName = TeamIdentityService.GetDisplayName(awayTeam),
            AwayLogoResourcePath = awayIdentity == null ? "" : awayIdentity.LogoResourcePath,
            AwayJerseyResourcePath = awayIdentity == null ? "" : awayIdentity.AwayJerseyResourcePath,
            AwayFullBodyResourcePath = awayIdentity == null ? "" : awayIdentity.FullBodyResourcePath,
            UserTeamId = CurrentTeam == null ? "" : CurrentTeam.Id,
            UserTeamName = TeamIdentityService.GetDisplayName(CurrentTeam),
            IsUserHomeTeam = CurrentTeam != null && homeTeam != null && CurrentTeam.Id == homeTeam.Id,
            OpponentTeamId = opponent == null ? "" : opponent.Id,
            OpponentTeamName = TeamIdentityService.GetDisplayName(opponent),
            CurrentTacticName = CurrentTeam == null || CurrentTeam.Tactics == null ? "Balanced" : CurrentTeam.Tactics.PresetName,
            StartingGoaliePlayerId = starter == null ? "" : starter.Id,
            StartingGoalieName = GetPlayerName(starter),
            BackupGoaliePlayerId = backup == null ? "" : backup.Id,
            BackupGoalieName = GetPlayerName(backup),
            IsLineupValid = canStart,
            LineupValidationMessage = lineupMessage,
            CanStartMatch = canStart && homeTeam != null && awayTeam != null,
            Summary = TeamIdentityService.GetDisplayName(awayTeam) + " @ " + TeamIdentityService.GetDisplayName(homeTeam)
        };
    }

    private static PreGameSetupData TryCreatePlayoffPreGameSetup()
    {
        if (CurrentState == null || CurrentState.Season == null || CurrentTeam == null)
        {
            return null;
        }

        PlayoffService.EnsurePlayoffs(CurrentState.Season);
        PlayoffData playoffs = CurrentState.Season.Playoffs;
        PlayoffSeriesData series = playoffs == null ? null : PlayoffService.GetNextActiveSeries(playoffs);
        if (series == null || (series.TeamAId != CurrentTeam.Id && series.TeamBId != CurrentTeam.Id))
        {
            return null;
        }

        int gameNumber = series.Games == null ? 1 : series.Games.Count + 1;
        bool teamAHome = gameNumber == 1 || gameNumber == 2 || gameNumber == 5 || gameNumber == 7;
        ScheduleGameData fakeGame = new ScheduleGameData
        {
            GameId = "playoff-" + series.SeriesId + "-" + gameNumber,
            GameNumber = gameNumber,
            DayNumber = CurrentState.Season.CurrentDay,
            HomeTeamId = teamAHome ? series.TeamAId : series.TeamBId,
            HomeTeamName = teamAHome ? series.TeamAName : series.TeamBName,
            AwayTeamId = teamAHome ? series.TeamBId : series.TeamAId,
            AwayTeamName = teamAHome ? series.TeamBName : series.TeamAName
        };
        return CreatePreGameSetup(fakeGame, true);
    }

    private static MatchResultData ApplyLivePlayoffResult(LiveMatchStateData match, TeamData homeTeam, TeamData awayTeam)
    {
        MatchResultData result = LiveMatchResultAdapter.ToMatchResult(match);
        PlayoffData playoffs = CurrentState == null || CurrentState.Season == null ? null : CurrentState.Season.Playoffs;
        PlayoffSeriesData series = playoffs == null ? null : PlayoffService.GetNextActiveSeries(playoffs);
        if (series != null)
        {
            series.EnsureGames();
            series.Games.Add(result);
            if (result.WinnerTeamId == series.TeamAId)
            {
                series.TeamAWins++;
            }
            else if (result.WinnerTeamId == series.TeamBId)
            {
                series.TeamBWins++;
            }

            if (series.TeamAWins >= 4 || series.TeamBWins >= 4)
            {
                series.IsCompleted = true;
                series.WinnerTeamId = series.TeamAWins >= 4 ? series.TeamAId : series.TeamBId;
                series.WinnerTeamName = series.TeamAWins >= 4 ? series.TeamAName : series.TeamBName;
            }
        }

        CurrentState.EnsureMatchHistory();
        CurrentState.MatchHistory.Add(result);
        CurrentState.LastMatchResult = result;
        CurrentState.TotalGamesSimulated++;
        PlayerStatsService.ApplyGameStats(CurrentState.Season, result.PlayerStats);
        PlayerFatigueService.ApplyFatigueAfterMatch(homeTeam, awayTeam);
        InjuryService.ApplyInjuryChecksAfterMatch(CurrentState, homeTeam, awayTeam, "LivePlayoffs");
        TeamRosterService.AdvanceRosterDaysForTeam(homeTeam);
        TeamRosterService.AdvanceRosterDaysForTeam(awayTeam);
        InjuryService.AdvanceInjuryRecovery(CurrentState);
        WaiverService.AdvanceWaiverDay(CurrentState);
        return result;
    }

    private static ScheduleGameData FindScheduledGame(string gameId)
    {
        if (CurrentState == null || CurrentState.Season == null || CurrentState.Season.Schedule == null || string.IsNullOrEmpty(gameId))
        {
            return null;
        }

        foreach (ScheduleGameData game in CurrentState.Season.Schedule)
        {
            if (game != null && game.GameId == gameId)
            {
                return game;
            }
        }

        return null;
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : player.FirstName + " " + player.LastName;
    }

    public static void EnsurePlayoffs()
    {
        if (CurrentState == null || CurrentState.Season == null)
        {
            return;
        }

        bool hadPlayoffs = CurrentState.Season.Playoffs != null && CurrentState.Season.Playoffs.IsStarted;
        PlayoffService.EnsurePlayoffs(CurrentState.Season);

        if (!hadPlayoffs && CurrentState.Season.Playoffs != null && CurrentState.Season.Playoffs.IsStarted)
        {
            SaveLoadService.Save(CurrentState);
        }
    }

    public static MatchResultData SimulateNextPlayoffGame()
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureLineups();
        EnsureFatigue();
        EnsureInjuries();
        EnsureSpecialTeamsAndTactics();
        EnsureRolesAndUsage();
        EnsureChemistry();
        RunCpuRosterManagement("BeforePlayoffGame", false);
        if (CurrentState == null || CurrentState.Season == null)
        {
            return null;
        }

        PlayoffService.EnsurePlayoffs(CurrentState.Season);
        PlayoffData playoffs = CurrentState.Season.Playoffs;
        PlayoffSeriesData nextSeries = playoffs == null ? null : PlayoffService.GetNextActiveSeries(playoffs);
        if (nextSeries != null)
        {
            TeamData teamA = FindTeam(CurrentState.Teams, nextSeries.TeamAId);
            TeamData teamB = FindTeam(CurrentState.Teams, nextSeries.TeamBId);

            if (CurrentTeam != null
                && (nextSeries.TeamAId == CurrentTeam.Id || nextSeries.TeamBId == CurrentTeam.Id)
                && !ValidateCurrentTeamCanPlay(out string validationMessage))
            {
                Debug.LogWarning(validationMessage);
                return null;
            }

            RepairCpuTeamForGame(teamA);
            RepairCpuTeamForGame(teamB);
        }

        MatchResultData result = PlayoffService.SimulateNextPlayoffGame(CurrentState);
        if (result == null)
        {
            return null;
        }

        TeamData homeTeam = FindTeam(CurrentState.Teams, result.HomeTeamId);
        TeamData awayTeam = FindTeam(CurrentState.Teams, result.AwayTeamId);
        CoachingStaffService.EnsureStaffForTeam(homeTeam);
        CoachingStaffService.EnsureStaffForTeam(awayTeam);
        IceTimeService.EnsureUsageForTeam(homeTeam);
        IceTimeService.EnsureUsageForTeam(awayTeam);
        IceTimeService.ApplyLastGameIceTime(homeTeam);
        IceTimeService.ApplyLastGameIceTime(awayTeam);
        PlayerFatigueService.ApplyFatigueAfterMatch(homeTeam, awayTeam);
        InjuryService.ApplyInjuryChecksAfterMatch(CurrentState, homeTeam, awayTeam, "Playoffs");
        TeamRosterService.AdvanceRosterDaysForTeam(homeTeam);
        TeamRosterService.AdvanceRosterDaysForTeam(awayTeam);
        InjuryService.AdvanceInjuryRecovery(CurrentState);
        WaiverService.AdvanceWaiverDay(CurrentState);
        RunCpuRosterManagement("AfterPlayoffGame", false);
        CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
        EnsureRosterStatuses();
        EnsureWaivers();
        EnsureLineups();
        EnsureSpecialTeamsAndTactics();
        EnsureRolesAndUsage();
        UpdateMoraleAfterGameDay();
        EnsureChemistry();
        EnsureOwnerGoals();
        SaveLoadService.Save(CurrentState);
        return result;
        }
        finally
        {
            stopwatch.Stop();
            PerformanceTimerService.RecordSimulateDay(CurrentState, stopwatch.ElapsedMilliseconds);
        }
    }

    public static bool CanStartPlayoffs()
    {
        return CurrentState != null
            && CurrentState.Season != null
            && PlayoffService.IsPlayoffAvailable(CurrentState.Season);
    }

    public static bool IsPlayoffsCompleted()
    {
        return CurrentState != null
            && CurrentState.Season != null
            && CurrentState.Season.Playoffs != null
            && CurrentState.Season.Playoffs.IsCompleted;
    }

    public static void Clear()
    {
        CurrentState = null;
        CurrentTeam = null;
        CurrentLiveMatch = null;
        CurrentPreGameSetup = null;
        LastPostGameSummary = null;
        InvalidateTeamLookup();
    }

    private static void EnsureInitialCurrentTeamCaptaincy()
    {
        if (CurrentTeam == null || HasAnyCaptaincy(CurrentTeam))
        {
            return;
        }

        LeadershipService.AutoAssignCaptains(CurrentTeam);
    }

    private static bool HasAnyCaptaincy(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return false;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null && (player.IsCaptain || player.IsAlternateCaptain))
            {
                return true;
            }
        }

        return false;
    }

    private static PlayerData FindCaptain(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return null;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.IsCaptain)
            {
                return player;
            }
        }

        return null;
    }

    private static CaptaincyActionResultData CreateMissingCaptaincyResult(string actionType)
    {
        return new CaptaincyActionResultData
        {
            Success = false,
            Message = "Команда не выбрана",
            ActionType = actionType,
            AssignedRole = LeadershipConfig.RoleNone
        };
    }

    private static void RefreshLeadershipDependentSystems(CaptaincyActionResultData result)
    {
        if (CurrentState == null || result == null || !result.Success)
        {
            return;
        }

        EnsureMorale();
        EnsureLeadership();
        EnsureChemistry();
        SaveLoadService.Save(CurrentState);
    }

    private static TeamData FindTeam(List<TeamData> teams, string teamId)
    {
        if (teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        EnsureTeamLookup(teams);
        return _teamLookup != null && _teamLookup.TryGetValue(teamId, out TeamData team) ? team : null;
    }

    private static void InvalidateTeamLookup()
    {
        _teamLookupSource = null;
        _teamLookup = null;
    }

    private static void EnsureTeamLookup(List<TeamData> teams)
    {
        if (teams == null)
        {
            _teamLookupSource = null;
            _teamLookup = null;
            return;
        }

        if (_teamLookupSource == teams && _teamLookup != null && _teamLookup.Count == teams.Count)
        {
            return;
        }

        _teamLookupSource = teams;
        _teamLookup = new Dictionary<string, TeamData>();
        foreach (TeamData team in teams)
        {
            if (team != null && !string.IsNullOrEmpty(team.Id) && !_teamLookup.ContainsKey(team.Id))
            {
                _teamLookup.Add(team.Id, team);
            }
        }
    }

    private static PlayerData FindPlayer(List<PlayerData> players, string playerId)
    {
        if (players == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (PlayerData player in players)
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static bool ShouldRecreateSeason()
    {
        if (CurrentState.Season == null)
        {
            return true;
        }

        CurrentState.Season.EnsureCollections();
        if (CurrentState.Season.ScheduleVersion < SeasonGenerator.CurrentScheduleVersion)
        {
            return true;
        }

        if (CurrentState.Season.TargetGamesPerTeam != SalaryCapConfig.TargetGamesPerTeam)
        {
            return true;
        }

        if (!HasExpectedGamesPerTeam(CurrentState.Season))
        {
            return true;
        }

        foreach (ScheduleGameData game in CurrentState.Season.Schedule)
        {
            if (game != null && game.DayNumber <= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasExpectedGamesPerTeam(SeasonData season)
    {
        if (season == null || season.Schedule == null || season.Schedule.Count == 0)
        {
            return false;
        }

        Dictionary<string, int> gamesByTeam = new Dictionary<string, int>();

        if (CurrentState.Teams != null)
        {
            foreach (TeamData team in CurrentState.Teams)
            {
                if (team != null && !gamesByTeam.ContainsKey(team.Id))
                {
                    gamesByTeam[team.Id] = 0;
                }
            }
        }

        foreach (ScheduleGameData game in season.Schedule)
        {
            if (game == null)
            {
                continue;
            }

            if (!gamesByTeam.ContainsKey(game.HomeTeamId))
            {
                gamesByTeam[game.HomeTeamId] = 0;
            }

            if (!gamesByTeam.ContainsKey(game.AwayTeamId))
            {
                gamesByTeam[game.AwayTeamId] = 0;
            }

            gamesByTeam[game.HomeTeamId]++;
            gamesByTeam[game.AwayTeamId]++;
        }

        foreach (KeyValuePair<string, int> entry in gamesByTeam)
        {
            if (entry.Value != SalaryCapConfig.TargetGamesPerTeam)
            {
                return false;
            }
        }

        return gamesByTeam.Count > 0;
    }

    private static int GetNextUnplayedDay(SeasonData season)
    {
        if (season == null || season.Schedule == null)
        {
            return 0;
        }

        int nextDay = int.MaxValue;

        foreach (ScheduleGameData game in season.Schedule)
        {
            if (game != null && !game.IsPlayed && game.DayNumber < nextDay)
            {
                nextDay = game.DayNumber;
            }
        }

        return nextDay == int.MaxValue ? 0 : nextDay;
    }

    private static List<ScheduleGameData> GetGamesForDay(SeasonData season, int dayNumber)
    {
        List<ScheduleGameData> games = new List<ScheduleGameData>();
        if (season == null || season.Schedule == null)
        {
            return games;
        }

        foreach (ScheduleGameData game in season.Schedule)
        {
            if (game != null && !game.IsPlayed && game.DayNumber == dayNumber)
            {
                games.Add(game);
            }
        }

        return games;
    }

    private static ScheduleGameData GetNextUnplayedGame(SeasonData season)
    {
        if (season == null || season.Schedule == null)
        {
            return null;
        }

        foreach (ScheduleGameData scheduleGame in season.Schedule)
        {
            if (scheduleGame != null && !scheduleGame.IsPlayed)
            {
                return scheduleGame;
            }
        }

        return null;
    }

    private static bool CurrentTeamPlaysOnDay(List<ScheduleGameData> gamesForDay)
    {
        if (CurrentTeam == null || gamesForDay == null)
        {
            return false;
        }

        foreach (ScheduleGameData game in gamesForDay)
        {
            if (game != null && (game.HomeTeamId == CurrentTeam.Id || game.AwayTeamId == CurrentTeam.Id))
            {
                return true;
            }
        }

        return false;
    }

    private static void RepairCpuTeamForGame(TeamData team)
    {
        if (team == null || (CurrentTeam != null && team.Id == CurrentTeam.Id))
        {
            return;
        }

        EnsureTeamPlayers(team);
        CpuRosterManagementService.EnsureCpuTeamReadyForGame(
            CurrentState,
            team,
            CurrentState == null ? "" : CurrentState.SelectedTeamId,
            null);
    }

    private static void EnsureTeamPlayers(TeamData team)
    {
        team.EnsurePlayers();

        if (team.Players.Count == 0)
        {
            team.Players = PlayerSeedData.CreatePlayersForTeam(team.Id);
        }

        ContractGenerator.EnsureContractsForTeam(team);
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        PlayerFatigueService.EnsureFatigueForTeam(team);
        InjuryService.EnsureInjuryFieldsForTeam(team);
    }

    private static void AdvanceRosterDaysForTeams()
    {
        if (CurrentState == null || CurrentState.Teams == null)
        {
            return;
        }

        foreach (TeamData team in CurrentState.Teams)
        {
            TeamRosterService.AdvanceRosterDaysForTeam(team);
        }
    }

    private static void RecoverNonPlayingTeamsForDay(HashSet<string> playedTeamIds)
    {
        if (CurrentState == null || CurrentState.Teams == null || playedTeamIds == null)
        {
            return;
        }

        foreach (TeamData team in CurrentState.Teams)
        {
            if (team != null && !playedTeamIds.Contains(team.Id))
            {
                PlayerFatigueService.RecoverNonPlayingTeam(team);
            }
        }
    }

    private static string GetTeamDisplayName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static int CompareLeagueHistoryDescending(LeagueSeasonHistoryData left, LeagueSeasonHistoryData right)
    {
        int startComparison = right.SeasonStartYear.CompareTo(left.SeasonStartYear);
        if (startComparison != 0)
        {
            return startComparison;
        }

        return right.SeasonEndYear.CompareTo(left.SeasonEndYear);
    }

    private static int CompareUserTeamHistoryDescending(UserTeamSeasonHistoryData left, UserTeamSeasonHistoryData right)
    {
        int startComparison = right.SeasonStartYear.CompareTo(left.SeasonStartYear);
        if (startComparison != 0)
        {
            return startComparison;
        }

        return right.SeasonEndYear.CompareTo(left.SeasonEndYear);
    }

    private static int CompareAwardsDescending(AwardWinnerData left, AwardWinnerData right)
    {
        int startComparison = right.SeasonStartYear.CompareTo(left.SeasonStartYear);
        if (startComparison != 0)
        {
            return startComparison;
        }

        return string.Compare(left.AwardType, right.AwardType, StringComparison.Ordinal);
    }

    private static bool HasAwardsForSeason(List<AwardWinnerData> awards, int seasonStartYear)
    {
        if (awards == null)
        {
            return false;
        }

        foreach (AwardWinnerData award in awards)
        {
            if (award != null && award.SeasonStartYear == seasonStartYear)
            {
                return true;
            }
        }

        return false;
    }

    private static int CompareDevelopmentChangesDescending(PlayerDevelopmentChangeData left, PlayerDevelopmentChangeData right)
    {
        DateTime leftDate = DateTime.MinValue;
        DateTime rightDate = DateTime.MinValue;
        bool hasLeftDate = left != null && DateTime.TryParse(left.CreatedAtUtc, out leftDate);
        bool hasRightDate = right != null && DateTime.TryParse(right.CreatedAtUtc, out rightDate);

        if (hasLeftDate && hasRightDate)
        {
            return rightDate.CompareTo(leftDate);
        }

        if (hasLeftDate)
        {
            return -1;
        }

        if (hasRightDate)
        {
            return 1;
        }

        return 0;
    }

    private static void NormalizeLeagueRules(LeagueRulesData rules)
    {
        if (rules == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(rules.Ruleset))
        {
            rules.Ruleset = string.IsNullOrEmpty(rules.RulesetName) ? "Continental League 2026-27" : rules.RulesetName;
        }

        if (string.IsNullOrEmpty(rules.RulesetName))
        {
            rules.RulesetName = rules.Ruleset;
        }

        if (string.IsNullOrEmpty(rules.Cba))
        {
            rules.Cba = string.IsNullOrEmpty(rules.CbaName) ? "NHL-style CBA 2026-2030" : rules.CbaName;
        }

        if (string.IsNullOrEmpty(rules.CbaName))
        {
            rules.CbaName = rules.Cba;
        }

        if (rules.RulesSeasonStartYear <= 0)
        {
            rules.RulesSeasonStartYear = SalaryCapConfig.RulesSeasonStartYear;
        }

        if (rules.RegularSeasonGamesPerTeam <= 0)
        {
            rules.RegularSeasonGamesPerTeam = SalaryCapConfig.TargetGamesPerTeam;
        }

        if (rules.PreseasonGamesPerTeam <= 0)
        {
            rules.PreseasonGamesPerTeam = 4;
        }

        if (rules.SalaryCapUpperLimit <= 0)
        {
            rules.SalaryCapUpperLimit = SalaryCapConfig.SalaryCapUpperLimit;
        }

        if (rules.SalaryCapLowerLimit <= 0)
        {
            rules.SalaryCapLowerLimit = SalaryCapConfig.SalaryCapLowerLimit;
        }

        if (rules.LeagueMinimumSalary <= 0)
        {
            rules.LeagueMinimumSalary = SalaryCapConfig.LeagueMinimumSalary;
        }

        if (rules.MaximumPlayerSalary <= 0)
        {
            rules.MaximumPlayerSalary = SalaryCapConfig.MaximumPlayerSalary;
        }

        if (rules.MaxContractYearsWithOwnTeam <= 0)
        {
            rules.MaxContractYearsWithOwnTeam = SalaryCapConfig.MaxContractYearsWithOwnTeam;
        }

        if (rules.MaxContractYearsFreeAgent <= 0)
        {
            rules.MaxContractYearsFreeAgent = SalaryCapConfig.MaxContractYearsFreeAgent;
        }

        if (rules.MinRosterSize <= 0)
        {
            rules.MinRosterSize = SalaryCapConfig.MinRosterSize;
        }

        if (rules.MaxRosterSize <= 0)
        {
            rules.MaxRosterSize = SalaryCapConfig.MaxRosterSize;
        }
    }

    private static void NormalizeLeagueCalendar(GameState state)
    {
        if (state == null || state.LeagueCalendar == null)
        {
            return;
        }

        if (state.LeagueCalendar.SeasonStartYear <= 0)
        {
            state.LeagueCalendar.SeasonStartYear = state.CurrentSeasonStartYear > 0
                ? state.CurrentSeasonStartYear
                : SalaryCapConfig.RulesSeasonStartYear;
        }

        if (state.LeagueCalendar.SeasonEndYear <= 0)
        {
            state.LeagueCalendar.SeasonEndYear = state.LeagueCalendar.SeasonStartYear + 1;
        }

        if (string.IsNullOrEmpty(state.LeagueCalendar.CalendarStatus))
        {
            state.LeagueCalendar.CalendarStatus = "Provisional";
        }
    }
}
