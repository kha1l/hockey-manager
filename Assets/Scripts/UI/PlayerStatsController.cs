using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsController : MonoBehaviour
{
    public const string ModeForwards = "Forwards";
    public const string ModeDefensemen = "Defensemen";
    public const string ModeGoalies = "Goalies";
    public const string ModeUnder21 = "Under21";
    public const string ModeTeam = "Team";

    [SerializeField] private Transform _statsContainer;
    [SerializeField] private PlayerStatsRowView _statsRowPrefab;

    public void Configure(Transform statsContainer, PlayerStatsRowView statsRowPrefab)
    {
        _statsContainer = statsContainer;
        _statsRowPrefab = statsRowPrefab;
    }

    public void ShowStats(SeasonData season, string teamId)
    {
        ShowStats(season, teamId, ModeForwards);
    }

    public void ShowStats(SeasonData season, string teamId, string mode)
    {
        if (_statsContainer == null || _statsRowPrefab == null)
        {
            Debug.LogError("PlayerStatsController: UI references are not configured.");
            return;
        }

        ClearRows();
        _statsRowPrefab.gameObject.SetActive(false);

        if (season == null)
        {
            PlayerStatsRowView emptyRow = CreateRow("empty-stats-row");
            emptyRow.InitializeMessage("Stats are not available yet.");
            return;
        }

        if (mode == ModeTeam)
        {
            ShowTeamStats(season, teamId);
            return;
        }

        Dictionary<string, PlayerData> playersById = BuildPlayerLookup();
        if (mode == ModeGoalies)
        {
            ShowLeagueGoalies(season, playersById);
            return;
        }

        ShowLeagueSkaters(season, playersById, mode);
    }

    private void ShowTeamStats(SeasonData season, string teamId)
    {
        List<PlayerSeasonStatsData> skaters = PlayerStatsService.GetTeamSkaterStats(season, teamId);
        List<PlayerSeasonStatsData> goalies = PlayerStatsService.GetTeamGoalieStats(season, teamId);
        Dictionary<string, PlayerData> playersById = BuildPlayerLookup();

        if (skaters.Count == 0 && goalies.Count == 0)
        {
            PlayerStatsRowView emptyRow = CreateRow("empty-stats-row");
            emptyRow.InitializeMessage("Статистики пока нет. Симулируйте игровой день.");
            return;
        }

        PlayerStatsRowView skaterHeader = CreateRow("skater-header-row");
        skaterHeader.InitializeMessage("Полевые: # Игрок [Ком] | Поз | И | Г | П | О | ATOI | PPP | PIM | +/-");

        foreach (PlayerSeasonStatsData stats in skaters)
        {
            PlayerStatsRowView row = CreateRow(stats.PlayerId + "-skater-row");
            row.InitializeSkater(stats, FindPlayer(playersById, stats.PlayerId));
        }

        PlayerStatsRowView goalieHeader = CreateRow("goalie-header-row");
        goalieHeader.InitializeMessage("Вратари: # Игрок [Ком] | И | В | П | ОТП | ATOI | SV% | GAA | SO");

        foreach (PlayerSeasonStatsData stats in goalies)
        {
            PlayerStatsRowView row = CreateRow(stats.PlayerId + "-goalie-row");
            row.InitializeGoalie(stats, FindPlayer(playersById, stats.PlayerId));
        }
    }

    private void ShowLeagueSkaters(
        SeasonData season,
        Dictionary<string, PlayerData> playersById,
        string mode)
    {
        List<PlayerSeasonStatsData> skaters = new List<PlayerSeasonStatsData>();
        if (season.PlayerStats != null)
        {
            foreach (PlayerSeasonStatsData stats in season.PlayerStats)
            {
                if (stats == null || stats.IsGoalie)
                {
                    continue;
                }

                PlayerData player = FindPlayer(playersById, stats.PlayerId);
                if (!MatchesSkaterMode(stats, player, mode))
                {
                    continue;
                }

                skaters.Add(stats);
            }
        }

        skaters.Sort(CompareSkaters);
        string title = mode == ModeDefensemen
            ? "Top 10 defensemen: # Player [Team] | Pos | Age | GP | G | A | P | ATOI | PPP | PIM | +/-"
            : mode == ModeUnder21
                ? "Top 10 U21 players: # Player [Team] | Age | Pos | GP | G | A | P | ATOI | PPP | PIM | +/-"
                : "Top 10 forwards: # Player [Team] | Pos | Age | GP | G | A | P | ATOI | PPP | PIM | +/-";
        PlayerStatsRowView header = CreateRow("league-skater-header-row");
        header.InitializeMessage(title);

        int count = Mathf.Min(10, skaters.Count);
        for (int i = 0; i < count; i++)
        {
            PlayerSeasonStatsData stats = skaters[i];
            PlayerData player = FindPlayer(playersById, stats.PlayerId);
            PlayerStatsRowView row = CreateRow(stats.PlayerId + "-league-skater-row");
            row.InitializeSkater(stats, player);
        }

        if (count == 0)
        {
            PlayerStatsRowView emptyRow = CreateRow("league-skater-empty-row");
            emptyRow.InitializeMessage("No players match this filter yet.");
        }
    }

    private void ShowLeagueGoalies(SeasonData season, Dictionary<string, PlayerData> playersById)
    {
        List<PlayerSeasonStatsData> goalies = new List<PlayerSeasonStatsData>();
        if (season.PlayerStats != null)
        {
            foreach (PlayerSeasonStatsData stats in season.PlayerStats)
            {
                if (stats != null && stats.IsGoalie)
                {
                    goalies.Add(stats);
                }
            }
        }

        goalies.Sort(CompareGoalies);
        PlayerStatsRowView header = CreateRow("league-goalie-header-row");
        header.InitializeMessage("Top 10 goalies: # Player [Team] | Age | GP | W | L | OTL | ATOI | SV% | GAA | SO");

        int count = Mathf.Min(10, goalies.Count);
        for (int i = 0; i < count; i++)
        {
            PlayerSeasonStatsData stats = goalies[i];
            PlayerStatsRowView row = CreateRow(stats.PlayerId + "-league-goalie-row");
            row.InitializeGoalie(stats, FindPlayer(playersById, stats.PlayerId));
        }

        if (count == 0)
        {
            PlayerStatsRowView emptyRow = CreateRow("league-goalie-empty-row");
            emptyRow.InitializeMessage("No goalie stats yet.");
        }
    }

    private PlayerStatsRowView CreateRow(string rowName)
    {
        PlayerStatsRowView row = Instantiate(_statsRowPrefab, _statsContainer);
        row.name = rowName;
        row.gameObject.SetActive(true);
        return row;
    }

    private void ClearRows()
    {
        for (int i = _statsContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _statsContainer.GetChild(i);
            if (child == _statsRowPrefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private static Dictionary<string, PlayerData> BuildPlayerLookup()
    {
        Dictionary<string, PlayerData> lookup = new Dictionary<string, PlayerData>();
        if (GameSession.CurrentState == null)
        {
            return lookup;
        }

        List<PlayerData> players = CareerStatsService.GetAllPlayersIncludingFreeAgents(GameSession.CurrentState);
        foreach (PlayerData player in players)
        {
            if (player != null && !string.IsNullOrEmpty(player.Id) && !lookup.ContainsKey(player.Id))
            {
                lookup.Add(player.Id, player);
            }
        }

        return lookup;
    }

    private static PlayerData FindPlayer(Dictionary<string, PlayerData> playersById, string playerId)
    {
        if (playersById == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        return playersById.TryGetValue(playerId, out PlayerData player) ? player : null;
    }

    private static bool MatchesSkaterMode(PlayerSeasonStatsData stats, PlayerData player, string mode)
    {
        string position = string.IsNullOrEmpty(stats.Position)
            ? (player == null ? "" : player.Position)
            : stats.Position;
        if (mode == ModeDefensemen)
        {
            return position == "D";
        }

        if (mode == ModeUnder21)
        {
            return player != null && player.Age <= 21;
        }

        return position != "D";
    }

    private static int CompareSkaters(PlayerSeasonStatsData left, PlayerSeasonStatsData right)
    {
        int pointsComparison = right.Points.CompareTo(left.Points);
        if (pointsComparison != 0)
        {
            return pointsComparison;
        }

        int goalsComparison = right.Goals.CompareTo(left.Goals);
        if (goalsComparison != 0)
        {
            return goalsComparison;
        }

        int shotsComparison = right.Shots.CompareTo(left.Shots);
        if (shotsComparison != 0)
        {
            return shotsComparison;
        }

        return string.Compare(left.PlayerName, right.PlayerName, System.StringComparison.Ordinal);
    }

    private static int CompareGoalies(PlayerSeasonStatsData left, PlayerSeasonStatsData right)
    {
        int winsComparison = right.GoalieWins.CompareTo(left.GoalieWins);
        if (winsComparison != 0)
        {
            return winsComparison;
        }

        int savesComparison = right.Saves.CompareTo(left.Saves);
        if (savesComparison != 0)
        {
            return savesComparison;
        }

        int goalsAgainstComparison = left.GoalsAgainst.CompareTo(right.GoalsAgainst);
        if (goalsAgainstComparison != 0)
        {
            return goalsAgainstComparison;
        }

        return string.Compare(left.PlayerName, right.PlayerName, System.StringComparison.Ordinal);
    }
}
