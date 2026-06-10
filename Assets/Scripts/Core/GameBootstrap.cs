using UnityEngine;
using UnityEngine.UI;

public class GameBootstrap : MonoBehaviour
{
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
                ? PlayerPrefs.GetString("SelectedTeamId", "")
                : GameSession.CurrentState.SelectedTeamId;
        }
    }

    public void Configure(Text selectedTeamText)
    {
        _selectedTeamText = selectedTeamText;
    }

    private void Awake()
    {
        EnsureGameSession();
    }

    private void Start()
    {
        UpdateSelectedTeamText();
    }

    private void EnsureGameSession()
    {
        if (GameSession.CurrentState == null)
        {
            GameState loadedState = SaveLoadService.Load();
            if (loadedState != null)
            {
                GameSession.LoadGame(loadedState);
            }
        }

        if (GameSession.CurrentState == null)
        {
            string selectedTeamId = PlayerPrefs.GetString("SelectedTeamId", "");
            if (!string.IsNullOrEmpty(selectedTeamId))
            {
                GameSession.StartNewGame(selectedTeamId);
            }
        }

        if (GameSession.CurrentTeam == null)
        {
            Debug.LogWarning("Команда не выбрана");
            return;
        }

        bool seasonNeedsSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.Season == null
                || GameSession.CurrentState.Season.ScheduleVersion < SeasonGenerator.CurrentScheduleVersion
                || GameSession.CurrentState.Season.TargetGamesPerTeam != SalaryCapConfig.TargetGamesPerTeam);
        bool freeAgentDataNeedsSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.FreeAgentPool == null
                || GameSession.CurrentState.FreeAgentHistory == null);
        bool draftDataNeedsSave = GameSession.CurrentState != null
            && (GameSession.CurrentState.Draft == null
                || GameSession.CurrentState.DraftHistory == null
                || GameSession.CurrentState.DraftPickOwnership == null
                || GameSession.CurrentState.DraftPickOwnership.Count == 0);
        bool prospectSigningDataNeedsSave = GameSession.CurrentState != null
            && GameSession.CurrentState.ProspectSigningHistory == null;
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

        GameSession.EnsureSeason();
        GameSession.EnsureLeagueRules();
        GameSession.EnsureTradeHistory();
        GameSession.EnsureDraftPickOwnership();
        GameSession.EnsureProspectSigningHistory();
        GameSession.EnsureSeasonHistory();
        GameSession.EnsureDevelopmentHistory();
        GameSession.EnsureContracts();
        GameSession.EnsureLineups();
        GameSession.EnsureFatigue();
        GameSession.EnsureInjuries();
        GameSession.EnsureSpecialTeamsAndTactics();
        GameSession.EnsureRolesAndUsage();
        GameSession.EnsureFreeAgents();

        if ((seasonNeedsSave || freeAgentDataNeedsSave || draftDataNeedsSave || prospectSigningDataNeedsSave || seasonHistoryNeedsSave || developmentHistoryNeedsSave || lineupNeedsSave || specialTeamsOrTacticsNeedsSave || fatigueNeedsSave || injuryNeedsSave || rolesOrUsageNeedsSave) && GameSession.CurrentState.Season != null)
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
            : "Выбранная команда: " + GetTeamDisplayName(GameSession.CurrentTeam) + " (" + GameSession.CurrentTeam.Abbreviation + ")";
    }

    private static string GetTeamDisplayName(TeamData team)
    {
        return team.City + " " + team.Name;
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
}
