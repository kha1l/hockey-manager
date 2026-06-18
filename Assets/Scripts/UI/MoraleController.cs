using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoraleController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _selectedPlayerText;
    [SerializeField] private Transform _playersContainer;
    [SerializeField] private Transform _eventsContainer;
    [SerializeField] private MoralePlayerRowView _playerRowPrefab;
    [SerializeField] private MoraleEventRowView _eventRowPrefab;
    [SerializeField] private GameScreenController _screenController;

    public void Configure(
        Text summaryText,
        Text selectedPlayerText,
        Transform playersContainer,
        Transform eventsContainer,
        MoralePlayerRowView playerRowPrefab,
        MoraleEventRowView eventRowPrefab,
        GameScreenController screenController)
    {
        _summaryText = summaryText;
        _selectedPlayerText = selectedPlayerText;
        _playersContainer = playersContainer;
        _eventsContainer = eventsContainer;
        _playerRowPrefab = playerRowPrefab;
        _eventRowPrefab = eventRowPrefab;
        _screenController = screenController;
    }

    public void ShowMorale(GameState state, string selectedPlayerId)
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("MoraleController: UI references are not configured.");
            return;
        }

        MoraleService.EnsureMorale(state);
        TeamData team = GetUserTeam(state);
        RenderSummary(state, team);
        RenderSelectedPlayer(state, team, selectedPlayerId);
        RenderPlayers(state, team);
        RenderEvents(state, team);
    }

    private void RenderSummary(GameState state, TeamData team)
    {
        TeamMoraleSummaryData summary = MoraleService.BuildTeamMoraleSummary(state, team);
        if (team == null || summary == null)
        {
            _summaryText.text = "Команда не выбрана";
            return;
        }

        _summaryText.text = TeamIdentityService.GetDisplayName(team)
            + "\nAverage morale: " + summary.AverageMorale
            + " | Happy: " + summary.HappyPlayers
            + " | Content: " + summary.ContentPlayers
            + " | Concerned: " + summary.ConcernedPlayers
            + "\nUnhappy: " + summary.UnhappyPlayers
            + " | Very unhappy: " + summary.VeryUnhappyPlayers
            + " | Trade requests: " + summary.TradeRequests
            + "\nLowest morale: " + FormatLowestMorale(summary);
    }

    private void RenderSelectedPlayer(GameState state, TeamData team, string selectedPlayerId)
    {
        PlayerData player = FindPlayer(team, selectedPlayerId);
        if (player == null)
        {
            _selectedPlayerText.text = "Выберите игрока";
            return;
        }

        PlayerMoraleSnapshotData snapshot = MoraleService.BuildMoraleSnapshot(state, team, player);
        _selectedPlayerText.text = snapshot.PlayerName
            + " | " + snapshot.Position
            + " | OVR " + snapshot.Overall
            + " | Age " + snapshot.Age
            + " | " + snapshot.RosterStatus
            + "\nRole: " + snapshot.PlayerRole
            + " | Usage: " + snapshot.UsageCategory
            + " | Expected: " + snapshot.ExpectedRole + " / " + snapshot.ExpectedUsageCategory
            + "\nTOI est/avg/expected: " + IceTimeConfig.FormatSeconds(snapshot.EstimatedTimeOnIceSeconds)
            + " / " + IceTimeConfig.FormatSeconds(snapshot.AverageTimeOnIceSeconds)
            + " / " + IceTimeConfig.FormatSeconds(snapshot.ExpectedTimeOnIceSeconds)
            + "\nMorale: " + snapshot.Morale
            + " | " + snapshot.MoraleStatus
            + " | Trend: " + snapshot.MoraleTrend
            + " | Overall satisfaction: " + snapshot.OverallSatisfaction
            + "\nRole " + snapshot.RoleSatisfaction
            + " | Ice " + snapshot.IceTimeSatisfaction
            + " | Team " + snapshot.TeamPerformanceSatisfaction
            + " | Contract " + snapshot.ContractSatisfaction
            + " | Roster " + snapshot.RosterStatusSatisfaction
            + "\nTrade request: " + (snapshot.WantsTrade ? "YES" : "no")
            + " | " + snapshot.MoraleSummary;
    }

    private void RenderPlayers(GameState state, TeamData team)
    {
        ClearRows(_playersContainer, _playerRowPrefab.transform);
        _playerRowPrefab.gameObject.SetActive(false);

        if (team == null)
        {
            CreateInfoRow(_playersContainer, "Команда не выбрана");
            return;
        }

        List<PlayerMoraleSnapshotData> snapshots = MoraleService.BuildTeamMoraleSnapshots(state, team);
        if (snapshots.Count == 0)
        {
            CreateInfoRow(_playersContainer, "Игроки не найдены");
            return;
        }

        foreach (PlayerMoraleSnapshotData snapshot in snapshots)
        {
            MoralePlayerRowView row = Instantiate(_playerRowPrefab, _playersContainer);
            row.name = snapshot.PlayerId + "-morale-row";
            row.gameObject.SetActive(true);
            row.Initialize(snapshot, _screenController);
        }
    }

    private void RenderEvents(GameState state, TeamData team)
    {
        ClearRows(_eventsContainer, _eventRowPrefab.transform);
        _eventRowPrefab.gameObject.SetActive(false);

        List<MoraleEventData> events = GetRecentEvents(state, team == null ? "" : team.Id, 10);
        if (events.Count == 0)
        {
            CreateInfoRow(_eventsContainer, "История морали пуста");
            return;
        }

        foreach (MoraleEventData moraleEvent in events)
        {
            MoraleEventRowView row = Instantiate(_eventRowPrefab, _eventsContainer);
            row.name = moraleEvent.EventId + "-morale-event-row";
            row.gameObject.SetActive(true);
            row.Initialize(moraleEvent);
        }
    }

    private bool HasRequiredReferences()
    {
        return _summaryText != null
            && _selectedPlayerText != null
            && _playersContainer != null
            && _eventsContainer != null
            && _playerRowPrefab != null
            && _eventRowPrefab != null;
    }

    private static TeamData GetUserTeam(GameState state)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(state.SelectedTeamId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == state.SelectedTeamId)
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

    private static List<MoraleEventData> GetRecentEvents(GameState state, string teamId, int maxCount)
    {
        List<MoraleEventData> events = new List<MoraleEventData>();
        if (state == null || state.MoraleHistory == null || state.MoraleHistory.Events == null)
        {
            return events;
        }

        for (int i = state.MoraleHistory.Events.Count - 1; i >= 0; i--)
        {
            MoraleEventData moraleEvent = state.MoraleHistory.Events[i];
            if (moraleEvent == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(teamId) && moraleEvent.TeamId != teamId)
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

    private static void ClearRows(Transform container, Transform prefabTransform)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child == prefabTransform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private static void CreateInfoRow(Transform container, string value)
    {
        GameObject rowObject = new GameObject("InfoRow");
        rowObject.transform.SetParent(container, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 44f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 44f;
        layoutElement.minHeight = 44f;

        Text text = rowObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 20;
        text.color = Color.white;
    }

    private static string FormatLowestMorale(TeamMoraleSummaryData summary)
    {
        if (summary == null || string.IsNullOrEmpty(summary.LowestMoralePlayerName))
        {
            return "нет данных";
        }

        return summary.LowestMoralePlayerName + " - " + summary.LowestMorale;
    }
}
