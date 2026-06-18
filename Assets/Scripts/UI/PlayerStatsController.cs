using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsController : MonoBehaviour
{
    [SerializeField] private Transform _statsContainer;
    [SerializeField] private PlayerStatsRowView _statsRowPrefab;

    public void Configure(Transform statsContainer, PlayerStatsRowView statsRowPrefab)
    {
        _statsContainer = statsContainer;
        _statsRowPrefab = statsRowPrefab;
    }

    public void ShowStats(SeasonData season, string teamId)
    {
        if (_statsContainer == null || _statsRowPrefab == null)
        {
            Debug.LogError("PlayerStatsController: UI references are not configured.");
            return;
        }

        ClearRows();
        _statsRowPrefab.gameObject.SetActive(false);

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
        skaterHeader.InitializeMessage("Полевые: Игрок | Поз | И | Г | П | О | ATOI | PPP | PIM | Бр | +/-");

        foreach (PlayerSeasonStatsData stats in skaters)
        {
            PlayerStatsRowView row = CreateRow(stats.PlayerId + "-skater-row");
            row.InitializeSkater(stats, FindPlayer(playersById, stats.PlayerId));
        }

        PlayerStatsRowView goalieHeader = CreateRow("goalie-header-row");
        goalieHeader.InitializeMessage("Вратари: Игрок | И | В | П | ОТП | ATOI | Сэйвы | ПШ | SO");

        foreach (PlayerSeasonStatsData stats in goalies)
        {
            PlayerStatsRowView row = CreateRow(stats.PlayerId + "-goalie-row");
            row.InitializeGoalie(stats, FindPlayer(playersById, stats.PlayerId));
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
}
