using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameSession
{
    public static GameState CurrentState { get; private set; }
    public static TeamData CurrentTeam { get; private set; }

    public static void StartNewGame(string selectedTeamId)
    {
        GameState gameState = new GameState
        {
            SaveVersion = 1,
            SelectedTeamId = selectedTeamId,
            TotalGamesSimulated = 0,
            Teams = TeamSeedData.CreateTeams()
        };

        CurrentState = gameState;
        CurrentTeam = FindTeam(gameState.Teams, selectedTeamId);
        EnsureLeagueRules();
        CurrentState.EnsureCareerProgress();
        EnsureTradeHistory();
        EnsureDraftPickOwnership();
        EnsureProspectSigningHistory();
        EnsureSeasonHistory();
        EnsureDevelopmentHistory();
        EnsureFreeAgents();

        if (CurrentTeam == null)
        {
            Debug.LogWarning("Выбранная команда не найдена: " + selectedTeamId);
            return;
        }

        EnsureTeamPlayers(CurrentTeam);
        EnsureContracts();
        EnsureLineups();
        EnsureFatigue();
        EnsureInjuries();
        EnsureSpecialTeamsAndTactics();
        EnsureRolesAndUsage();
        Debug.Log("Новая игра создана: " + GetTeamDisplayName(CurrentTeam));
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
        loadedState.EnsureInjuryHistory();
        loadedState.EnsureCareerProgress();
        loadedState.EnsureSeasonHistory();
        loadedState.EnsurePlayerDevelopmentHistory();

        CurrentState = loadedState;
        CurrentTeam = FindTeam(loadedState.Teams, loadedState.SelectedTeamId);
        EnsureLeagueRules();
        CurrentState.EnsureCareerProgress();
        EnsureTradeHistory();
        EnsureDraftPickOwnership();
        EnsureProspectSigningHistory();
        EnsureSeasonHistory();
        EnsureDevelopmentHistory();
        EnsureFreeAgents();

        if (CurrentTeam == null)
        {
            Debug.LogWarning("Команда из сохранения не найдена: " + loadedState.SelectedTeamId);
            return;
        }

        EnsureTeamPlayers(CurrentTeam);
        EnsureContracts();
        EnsureLineups();
        EnsureFatigue();
        EnsureInjuries();
        EnsureSpecialTeamsAndTactics();
        EnsureRolesAndUsage();
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

    public static void EnsureSeason()
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
        EnsureLineups();
        EnsureFatigue();
        EnsureInjuries();
        EnsureSpecialTeamsAndTactics();
        EnsureRolesAndUsage();

        if (recreateSeason)
        {
            SaveLoadService.Save(CurrentState);
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
        if (CurrentState != null)
        {
            SaveLoadService.Save(CurrentState);
        }
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

        EnsureInjuries();
        if (LineupService.HasInjuredActivePlayers(CurrentTeam, out message))
        {
            message = "Нельзя симулировать матч: " + message + ". Нажмите Автосостав.";
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

    public static void EnsureProspectSigningHistory()
    {
        if (CurrentState == null)
        {
            return;
        }

        ProspectSigningService.EnsureProspectSigningHistory(CurrentState);
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
            EnsureLineups();
            EnsureSpecialTeamsAndTactics();
            EnsureRolesAndUsage();
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

        bool result = TradeService.TryCreateTradeWithAssets(
            CurrentState,
            assetsFromUserTeam,
            otherTeamId,
            assetsFromOtherTeam,
            out proposal,
            out message);

        if (result)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            EnsureLineups();
            EnsureSpecialTeamsAndTactics();
            EnsureRolesAndUsage();
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
        EnsureFreeAgents();

        bool result = FreeAgentService.TrySignFreeAgent(CurrentState, playerId, out signing, out message);
        if (result)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            LineupService.SyncScratchPlayers(CurrentTeam);
            LineupService.ValidateLineup(CurrentTeam, out string validationMessage);
            SpecialTeamsService.ValidateSpecialTeams(CurrentTeam, out string specialTeamsMessage);
            EnsureRolesAndUsage();
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
        EnsureProspectSigningHistory();

        bool result = ProspectSigningService.TrySignProspectToElc(
            CurrentState,
            prospectId,
            out signing,
            out message);

        if (result)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            LineupService.SyncScratchPlayers(CurrentTeam);
            LineupService.ValidateLineup(CurrentTeam, out string validationMessage);
            SpecialTeamsService.ValidateSpecialTeams(CurrentTeam, out string specialTeamsMessage);
            EnsureRolesAndUsage();
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
        EnsureLineups();
        EnsureFatigue();
        EnsureInjuries();
        EnsureSpecialTeamsAndTactics();

        bool result = SeasonTransitionService.StartNextSeason(CurrentState, out message);
        if (result)
        {
            CurrentTeam = FindTeam(CurrentState.Teams, CurrentState.SelectedTeamId);
            EnsureLineups();
            EnsureFatigue();
            EnsureInjuries();
            EnsureSpecialTeamsAndTactics();
            EnsureRolesAndUsage();
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
        EnsureSeason();

        if (CurrentState == null || CurrentState.Season == null || CurrentTeam == null)
        {
            return null;
        }

        foreach (ScheduleGameData scheduleGame in CurrentState.Season.Schedule)
        {
            if (scheduleGame != null
                && !scheduleGame.IsPlayed
                && (scheduleGame.HomeTeamId == CurrentTeam.Id || scheduleGame.AwayTeamId == CurrentTeam.Id))
            {
                return scheduleGame;
            }
        }

        CurrentState.Season.IsSeasonFinished = true;
        return null;
    }

    public static List<ScheduleGameData> GetGamesForCurrentDay()
    {
        EnsureSeason();

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
        List<MatchResultData> results = new List<MatchResultData>();

        if (CurrentState == null)
        {
            Debug.LogWarning("Нельзя симулировать матч: активная игра не найдена");
            return results;
        }

        EnsureSeason();
        EnsureLineups();
        EnsureFatigue();
        EnsureInjuries();
        EnsureSpecialTeamsAndTactics();

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
            IceTimeService.EnsureUsageForTeam(homeTeam);
            IceTimeService.EnsureUsageForTeam(awayTeam);
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
        InjuryService.AdvanceInjuryRecovery(CurrentState);

        CurrentState.Season.CurrentDay = simulatedDay + 1;
        if (GetNextUnplayedGame(CurrentState.Season) == null)
        {
            CurrentState.Season.IsSeasonFinished = true;
        }

        SaveLoadService.Save(CurrentState);
        Debug.Log("Сыграно матчей игрового дня " + simulatedDay + ": " + results.Count);

        return results;
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
        EnsureLineups();
        EnsureFatigue();
        EnsureInjuries();
        EnsureSpecialTeamsAndTactics();
        EnsureRolesAndUsage();
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
        IceTimeService.EnsureUsageForTeam(homeTeam);
        IceTimeService.EnsureUsageForTeam(awayTeam);
        IceTimeService.ApplyLastGameIceTime(homeTeam);
        IceTimeService.ApplyLastGameIceTime(awayTeam);
        PlayerFatigueService.ApplyFatigueAfterMatch(homeTeam, awayTeam);
        InjuryService.ApplyInjuryChecksAfterMatch(CurrentState, homeTeam, awayTeam, "Playoffs");
        InjuryService.AdvanceInjuryRecovery(CurrentState);
        SaveLoadService.Save(CurrentState);
        return result;
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
    }

    private static TeamData FindTeam(List<TeamData> teams, string teamId)
    {
        if (teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
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
        InjuryService.EnsureInjuryFieldsForTeam(team);

        bool lineupNeedsRebuild = team.Lineup == null
            || !LineupService.ValidateLineup(team, out string lineupMessage)
            || LineupService.HasInjuredActivePlayers(team, out lineupMessage);

        if (lineupNeedsRebuild)
        {
            team.Lineup = LineupService.BuildAutoLineup(team);
        }

        bool specialTeamsNeedsRebuild = team.SpecialTeams == null
            || !SpecialTeamsService.ValidateSpecialTeams(team, out string specialTeamsMessage);

        if (specialTeamsNeedsRebuild)
        {
            team.SpecialTeams = SpecialTeamsService.BuildAutoSpecialTeams(team);
        }

        TacticsService.EnsureTactics(team);
    }

    private static void EnsureTeamPlayers(TeamData team)
    {
        team.EnsurePlayers();

        if (team.Players.Count == 0)
        {
            team.Players = PlayerSeedData.CreatePlayersForTeam(team.Id);
        }

        ContractGenerator.EnsureContractsForTeam(team);
        PlayerFatigueService.EnsureFatigueForTeam(team);
        InjuryService.EnsureInjuryFieldsForTeam(team);
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
        return team.City + " " + team.Name;
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
            rules.Ruleset = string.IsNullOrEmpty(rules.RulesetName) ? "NHL 2026-27" : rules.RulesetName;
        }

        if (string.IsNullOrEmpty(rules.RulesetName))
        {
            rules.RulesetName = rules.Ruleset;
        }

        if (string.IsNullOrEmpty(rules.Cba))
        {
            rules.Cba = string.IsNullOrEmpty(rules.CbaName) ? "NHL/NHLPA CBA 2026-2030" : rules.CbaName;
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
