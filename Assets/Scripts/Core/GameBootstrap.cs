using UnityEngine;
using UnityEngine.UI;

public class GameBootstrap : MonoBehaviour
{
    private const string SelectedTeamIdKey = "SelectedTeamId";
    private const string StartNewGamePendingKey = "StartNewGamePending";

    [SerializeField] private Text _selectedTeamText;

    public TeamData CurrentTeam
    {
        get { return GameSession.CurrentTeam; }
    }

    public string SelectedTeamId
    {
        get
        {
            return GameSession.CurrentState == null
                ? PlayerPrefs.GetString(SelectedTeamIdKey, "")
                : GameSession.CurrentState.SelectedTeamId;
        }
    }

    public void Configure(Text selectedTeamText)
    {
        _selectedTeamText = selectedTeamText;
    }

    private void Awake()
    {
        Application.targetFrameRate = AndroidBuildConfig.TargetFrameRate;
        EnsureGameSession();
    }

    private void Start()
    {
        UpdateSelectedTeamText();
    }

    private void EnsureGameSession()
    {
        bool startNewGamePending = PlayerPrefs.GetInt(StartNewGamePendingKey, 0) == 1;
        bool createdNewGameThisBoot = false;
        bool loadedGameThisBoot = false;
        string selectedTeamId = PlayerPrefs.GetString(SelectedTeamIdKey, "");

        if (startNewGamePending)
        {
            if (string.IsNullOrEmpty(selectedTeamId))
            {
                Debug.LogWarning("Новая игра ожидается, но команда не выбрана");
            }
            else
            {
                GameSession.Clear();
                GameSession.StartNewGame(selectedTeamId);
                createdNewGameThisBoot = GameSession.CurrentState != null;
            }

            PlayerPrefs.DeleteKey(StartNewGamePendingKey);
            PlayerPrefs.Save();
        }

        if (!startNewGamePending && GameSession.CurrentState == null)
        {
            GameState loadedState = SaveLoadService.Load();
            if (loadedState != null)
            {
                GameSession.LoadGame(loadedState);
                loadedGameThisBoot = GameSession.CurrentState != null;
            }
        }

        if (GameSession.CurrentState == null)
        {
            if (!string.IsNullOrEmpty(selectedTeamId))
            {
                GameSession.StartNewGame(selectedTeamId);
                createdNewGameThisBoot = GameSession.CurrentState != null;
            }
        }

        if (GameSession.CurrentTeam == null)
        {
            Debug.LogWarning("Команда не выбрана");
            return;
        }

        if (createdNewGameThisBoot || loadedGameThisBoot)
        {
            if (loadedGameThisBoot)
            {
                SaveLoadService.Save(GameSession.CurrentState);
            }

            int bootScheduleCount = GameSession.CurrentState == null || GameSession.CurrentState.Season == null
                ? 0
                : GameSession.CurrentState.Season.Schedule.Count;
            Debug.Log("Календарь матчей: " + bootScheduleCount);
            Debug.Log("Выбранная команда: " + GetTeamDisplayName(GameSession.CurrentTeam));
            return;
        }

        bool seasonNeedsSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.Season == null
                || GameSession.CurrentState.Season.ScheduleVersion < SeasonGenerator.CurrentScheduleVersion
                || GameSession.CurrentState.Season.TargetGamesPerTeam != SalaryCapConfig.TargetGamesPerTeam);
        bool freeAgentDataNeedsSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.FreeAgentPool == null
                || GameSession.CurrentState.FreeAgentHistory == null);
        bool freeAgencyOfferHistoryNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.FreeAgencyOfferHistory == null;
        bool ownerGoalsNeedSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.OwnerEvaluationHistory == null || HasMissingOwnerProfileData(GameSession.CurrentState));
        bool leagueHistoryNeedsSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.LeagueHistory == null
                || GameSession.CurrentState.UserTeamHistory == null
                || GameSession.CurrentState.LeagueRecords == null
                || HasMissingCareerStatsData(GameSession.CurrentState));
        bool newsFeedNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.NewsFeed == null;
        bool retirementHistoryNeedsSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.RetiredPlayers == null
                || GameSession.CurrentState.HallOfFame == null
                || GameSession.CurrentState.LeagueRetiredNumbers == null);
        bool tutorialNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.Tutorial == null;
        bool gmCareerNeedsSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.GmCareer == null
                || GameSession.CurrentState.ActiveGmJobOffers == null
                || GameSession.CurrentState.GmCareerEvents == null);
        bool draftDataNeedsSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.Draft == null
                || GameSession.CurrentState.DraftHistory == null
                || GameSession.CurrentState.DraftPickOwnership == null
                || GameSession.CurrentState.DraftPickOwnership.Count == 0);
        bool prospectSigningDataNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.ProspectSigningHistory == null;
        bool cpuRosterManagementNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.CpuRosterManagementHistory == null;
        bool alphaBalanceNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.AlphaBalanceReportHistory == null;
        bool androidPerformanceNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.AndroidPerformance == null;
        bool tradeProfilesNeedSave = GameSession.CurrentState != null
            && GameSession.CurrentState.TeamTradeProfiles == null;
        bool scoutingNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.ScoutingHistory == null;
        bool moraleNeedsSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.MoraleHistory == null || HasMissingMoraleData(GameSession.CurrentState));
        bool leadershipNeedsSave = GameSession.CurrentState != null
            && HasMissingLeadershipData(GameSession.CurrentState);
        bool staffNeedsSave = GameSession.CurrentState != null
            && HasMissingStaffData(GameSession.CurrentState);
        bool chemistryNeedsSave = GameSession.CurrentState != null
            && HasMissingChemistryData(GameSession.CurrentState);
        bool contractExtensionsNeedSave = GameSession.CurrentState != null
            && GameSession.CurrentState.ContractExtensionHistory == null;
        bool seasonHistoryNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.SeasonHistory == null;
        bool developmentHistoryNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.PlayerDevelopmentHistory == null;
        bool lineupNeedsSave = GameSession.CurrentState != null
            && HasMissingLineup(GameSession.CurrentState);
        bool specialTeamsOrTacticsNeedsSave = GameSession.CurrentState != null
            && HasMissingSpecialTeamsOrTactics(GameSession.CurrentState);
        bool fatigueNeedsSave = GameSession.CurrentState != null
            && HasMissingFatigueData(GameSession.CurrentState);
        bool injuryNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.InjuryHistory == null;
        bool rolesOrUsageNeedsSave = GameSession.CurrentState != null
            && HasMissingRolesOrUsage(GameSession.CurrentState);
        bool rosterStatusNeedsSave = GameSession.CurrentState != null
            && HasMissingRosterStatus(GameSession.CurrentState);
        bool waiversNeedSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.WaiverWire == null || HasMissingWaiverStatus(GameSession.CurrentState));

        GameSession.EnsureSeason();
        GameSession.EnsureLeagueRules();
        GameSession.EnsureRosterStatuses();
        GameSession.EnsureWaivers();
        GameSession.EnsureTradeHistory();
        GameSession.EnsureDraftPickOwnership();
        GameSession.EnsureProspectSigningHistory();
        GameSession.EnsureCpuRosterManagementHistory();
        GameSession.EnsureAlphaBalanceReports();
        GameSession.EnsureAndroidPerformance();
        GameSession.EnsureTradeProfiles();
        GameSession.EnsureScouting();
        GameSession.EnsureMorale();
        GameSession.EnsureSeasonHistory();
        GameSession.EnsureDevelopmentHistory();
        GameSession.EnsureContracts();
        GameSession.EnsureLineups();
        GameSession.EnsureFatigue();
        GameSession.EnsureInjuries();
        GameSession.EnsureSpecialTeamsAndTactics();
        GameSession.EnsureRolesAndUsage();
        GameSession.EnsureFreeAgents();
        GameSession.EnsureBetterFreeAgency();
        GameSession.EnsureLeadership();
        GameSession.EnsureCoachingStaff();
        GameSession.EnsureChemistry();
        GameSession.EnsureContractExtensions();
        GameSession.EnsureOwnerGoals();
        GameSession.EnsureGmCareer();
        GameSession.EnsureLeagueHistory();
        GameSession.EnsureNewsFeed();
        GameSession.EnsureRetirementHistory();
        GameSession.EnsureTutorial();

        if ((seasonNeedsSave || freeAgentDataNeedsSave || freeAgencyOfferHistoryNeedsSave || ownerGoalsNeedSave || leagueHistoryNeedsSave || newsFeedNeedsSave || retirementHistoryNeedsSave || tutorialNeedsSave || gmCareerNeedsSave || draftDataNeedsSave || prospectSigningDataNeedsSave || cpuRosterManagementNeedsSave || alphaBalanceNeedsSave || androidPerformanceNeedsSave || tradeProfilesNeedSave || scoutingNeedsSave || moraleNeedsSave || leadershipNeedsSave || staffNeedsSave || chemistryNeedsSave || contractExtensionsNeedSave || seasonHistoryNeedsSave || developmentHistoryNeedsSave || lineupNeedsSave || specialTeamsOrTacticsNeedsSave || fatigueNeedsSave || injuryNeedsSave || rolesOrUsageNeedsSave || rosterStatusNeedsSave || waiversNeedSave) && GameSession.CurrentState.Season != null)
        {
            SaveLoadService.Save(GameSession.CurrentState);
        }

        int scheduleGamesCount = GameSession.CurrentState.Season == null ? 0 : GameSession.CurrentState.Season.Schedule.Count;
        Debug.Log("Календарь матчей: " + scheduleGamesCount);
        Debug.Log("Выбранная команда: " + GetTeamDisplayName(GameSession.CurrentTeam));
    }

    private void UpdateSelectedTeamText()
    {
        if (_selectedTeamText == null)
        {
            return;
        }

        _selectedTeamText.text = GameSession.CurrentTeam == null
            ? "Команда не выбрана"
            : "Выбранная команда: " + GetTeamDisplayName(GameSession.CurrentTeam) + " (" + TeamIdentityService.GetAbbreviation(GameSession.CurrentTeam) + ")";
    }

    private static string GetTeamDisplayName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static bool HasMissingLineup(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Lineup == null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasMissingSpecialTeamsOrTactics(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && (team.SpecialTeams == null || team.Tactics == null))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasMissingFatigueData(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && player.Condition <= 0 && player.Fatigue <= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasMissingRolesOrUsage(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null
                    && (string.IsNullOrEmpty(player.PlayerRole)
                        || string.IsNullOrEmpty(player.UsageCategory)
                        || player.EstimatedTimeOnIceSeconds <= 0 && LineupService.IsPlayerActive(team, player.Id)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasMissingRosterStatus(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && string.IsNullOrEmpty(player.RosterStatus))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasMissingWaiverStatus(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && string.IsNullOrEmpty(player.WaiverStatus))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasMissingMoraleData(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && !player.HasMoraleInitialized)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasMissingOwnerProfileData(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null
                && (team.OwnerProfile == null
                    || team.OwnerProfile.CurrentGoals == null
                    || team.OwnerProfile.Finances == null))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasMissingCareerStatsData(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && player.CareerAwardIds == null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasMissingLeadershipData(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            if (team.LeadershipData == null)
            {
                return true;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && (!player.HasLeadershipProfile || string.IsNullOrEmpty(player.CaptaincyRole)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasMissingStaffData(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            if (team.Staff == null
                || team.Staff.HeadCoach == null
                || team.Staff.AssistantCoach == null
                || team.Staff.DevelopmentCoach == null
                || team.Staff.GoalieCoach == null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasMissingChemistryData(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return false;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            if (team.Chemistry == null)
            {
                return true;
            }

            if (team.Lineup != null
                && (string.IsNullOrEmpty(team.Lineup.TeamChemistryLabel)
                    || string.IsNullOrEmpty(team.Lineup.LastChemistryUpdateUtc)))
            {
                return true;
            }
        }

        return false;
    }
}
