using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class OffseasonController : MonoBehaviour
{
    [SerializeField] private Text _statusText;
    [SerializeField] private Text _summaryText;
    [SerializeField] private Transform _historyContainer;
    [SerializeField] private SeasonHistoryRowView _historyRowPrefab;

    public void Configure(
        Text statusText,
        Text summaryText,
        Transform historyContainer,
        SeasonHistoryRowView historyRowPrefab)
    {
        _statusText = statusText;
        _summaryText = summaryText;
        _historyContainer = historyContainer;
        _historyRowPrefab = historyRowPrefab;
    }

    public void ShowOffseason(GameState state)
    {
        if (_statusText == null || _summaryText == null || _historyContainer == null || _historyRowPrefab == null)
        {
            Debug.LogError("OffseasonController: UI references are not configured.");
            return;
        }

        SeasonHistoryService.EnsureSeasonHistory(state);
        RenderStatus(state);
        RenderSummary(state);
        RenderHistory(state);
    }

    private void RenderStatus(GameState state)
    {
        string phase = LeaguePhaseService.GetCurrentPhase(state);
        string currentSeason = state == null
            ? "2026-27"
            : FormatSeason(state.CurrentSeasonStartYear, state.CurrentSeasonEndYear);
        string nextSeason = state == null
            ? "2027-28"
            : FormatSeason(state.CurrentSeasonStartYear + 1, state.CurrentSeasonEndYear + 1);
        bool canStartNextSeason = SeasonTransitionService.CanStartNextSeason(state, out string message);
        string status = canStartNextSeason ? "Следующий сезон доступен" : "Завершите плей-офф и драфт";
        string champion = GetChampionText(state);

        _statusText.text = "Фаза: " + phase
            + "\nТекущий сезон: " + currentSeason
            + "\nСледующий сезон: " + nextSeason
            + "\nСтатус: " + status
            + "\n" + message
            + "\nЧемпион прошлого сезона: " + champion;
    }

    private void RenderSummary(GameState state)
    {
        TeamData team = GetUserTeam(state);
        if (team == null)
        {
            _summaryText.text = "Команда не найдена";
            return;
        }

        team.EnsurePlayers();
        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        CountContractStatuses(team.Players, out int expiringCount, out int rfaCount, out int ufaCount);

        _summaryText.text = "Expiring: " + expiringCount
            + " | RFA: " + rfaCount
            + " | UFA: " + ufaCount
            + "\nRoster: " + finance.PlayerCount + " / " + SalaryCapConfig.MaxRosterSize
            + "\nPayroll: " + FormatMoney(finance.Payroll)
            + " | Cap space: " + FormatMoney(finance.CapSpace);
    }

    private void RenderHistory(GameState state)
    {
        ClearRows();
        _historyRowPrefab.gameObject.SetActive(false);

        if (state == null || state.SeasonHistory == null || state.SeasonHistory.Count == 0)
        {
            CreateInfoRow("История сезонов пока пуста");
            return;
        }

        int firstIndex = Mathf.Max(0, state.SeasonHistory.Count - 10);
        for (int i = state.SeasonHistory.Count - 1; i >= firstIndex; i--)
        {
            SeasonHistoryData history = state.SeasonHistory[i];
            if (history == null)
            {
                continue;
            }

            history.EnsureCollections();
            SeasonHistoryRowView row = Instantiate(_historyRowPrefab, _historyContainer);
            row.name = "season-history-" + history.SeasonStartYear;
            row.gameObject.SetActive(true);
            row.Initialize(history);
        }
    }

    private void ClearRows()
    {
        Transform prefabTransform = _historyRowPrefab.transform;
        for (int i = _historyContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _historyContainer.GetChild(i);
            if (child == prefabTransform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private void CreateInfoRow(string value)
    {
        GameObject rowObject = new GameObject("InfoRow");
        rowObject.transform.SetParent(_historyContainer, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 44f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 44f;
        layoutElement.minHeight = 44f;

        Text text = rowObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
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

    private static string GetChampionText(GameState state)
    {
        if (state != null
            && state.Season != null
            && state.Season.Playoffs != null
            && !string.IsNullOrEmpty(state.Season.Playoffs.ChampionTeamName))
        {
            return state.Season.Playoffs.ChampionTeamName;
        }

        SeasonHistoryData lastHistory = GetLastHistory(state);
        return lastHistory == null || string.IsNullOrEmpty(lastHistory.ChampionTeamName)
            ? "нет данных"
            : lastHistory.ChampionTeamName;
    }

    private static SeasonHistoryData GetLastHistory(GameState state)
    {
        if (state == null || state.SeasonHistory == null || state.SeasonHistory.Count == 0)
        {
            return null;
        }

        for (int i = state.SeasonHistory.Count - 1; i >= 0; i--)
        {
            if (state.SeasonHistory[i] != null)
            {
                return state.SeasonHistory[i];
            }
        }

        return null;
    }

    private static void CountContractStatuses(List<PlayerData> players, out int expiringCount, out int rfaCount, out int ufaCount)
    {
        expiringCount = 0;
        rfaCount = 0;
        ufaCount = 0;

        if (players == null)
        {
            return;
        }

        foreach (PlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            if (player.ContractStatus == "Expiring")
            {
                expiringCount++;
            }
            else if (player.ContractStatus == "RFA")
            {
                rfaCount++;
            }
            else if (player.ContractStatus == "UFA")
            {
                ufaCount++;
            }
        }
    }

    private static string FormatSeason(int startYear, int endYear)
    {
        return startYear + "-" + (endYear % 100).ToString("D2");
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
