using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class FreeAgencyController : MonoBehaviour
{
    [SerializeField] private Text _statusText;
    [SerializeField] private Text _financeText;
    [SerializeField] private Text _selectedFreeAgentText;
    [SerializeField] private Transform _freeAgentsContainer;
    [SerializeField] private Transform _historyContainer;
    [SerializeField] private FreeAgentRowView _freeAgentRowPrefab;
    [SerializeField] private FreeAgentSigningHistoryRowView _historyRowPrefab;
    [SerializeField] private GameScreenController _screenController;

    public void Configure(
        Text statusText,
        Text financeText,
        Text selectedFreeAgentText,
        Transform freeAgentsContainer,
        Transform historyContainer,
        FreeAgentRowView freeAgentRowPrefab,
        FreeAgentSigningHistoryRowView historyRowPrefab,
        GameScreenController screenController)
    {
        _statusText = statusText;
        _financeText = financeText;
        _selectedFreeAgentText = selectedFreeAgentText;
        _freeAgentsContainer = freeAgentsContainer;
        _historyContainer = historyContainer;
        _freeAgentRowPrefab = freeAgentRowPrefab;
        _historyRowPrefab = historyRowPrefab;
        _screenController = screenController;
    }

    public void ShowFreeAgency(GameState state, string selectedFreeAgentId)
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("FreeAgencyController: UI references are not configured.");
            return;
        }

        FreeAgentService.EnsureFreeAgentData(state);
        RenderStatus(state);
        RenderFinance(state);
        RenderSelectedFreeAgent(state, selectedFreeAgentId);
        RenderFreeAgents(state);
        RenderHistory(state);
    }

    private void RenderStatus(GameState state)
    {
        string freeAgencyStartDate = state == null || state.LeagueCalendar == null
            ? ""
            : state.LeagueCalendar.FreeAgencyStartDate;
        string calendarStatus = state == null || state.LeagueCalendar == null
            ? ""
            : state.LeagueCalendar.CalendarStatus;
        string phase = LeaguePhaseService.GetCurrentPhase(state);
        string marketStatus = LeaguePhaseService.IsFreeAgencyOpen(state)
            ? "Рынок свободных агентов открыт"
            : "Рынок откроется после завершения драфта";

        _statusText.text = "Фаза сезона: " + phase
            + "\nFreeAgencyStartDate: " + freeAgencyStartDate
            + "\nCalendarStatus: " + calendarStatus
            + "\n" + marketStatus;
    }

    private void RenderFinance(GameState state)
    {
        TeamData team = GetUserTeam(state);
        if (team == null)
        {
            _financeText.text = "Команда не найдена";
            return;
        }

        team.EnsurePlayers();
        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        _financeText.text = "Salary cap: " + FormatMoney(finance.SalaryCapUpperLimit)
            + "\nPayroll: " + FormatMoney(finance.Payroll)
            + "\nCap space: " + FormatMoney(finance.CapSpace)
            + "\nRoster size: " + finance.PlayerCount + " / " + SalaryCapConfig.MaxRosterSize;
    }

    private void RenderSelectedFreeAgent(GameState state, string selectedFreeAgentId)
    {
        PlayerData selectedPlayer = FreeAgentService.FindFreeAgent(state, selectedFreeAgentId);
        _selectedFreeAgentText.text = selectedPlayer == null
            ? "Выбранный свободный агент: не выбран"
            : "Выбранный свободный агент: " + selectedPlayer.FirstName + " " + selectedPlayer.LastName
                + " | " + selectedPlayer.Position
                + " | OVR " + selectedPlayer.Overall
                + " | $" + FormatMoney(selectedPlayer.Salary)
                + " | " + selectedPlayer.ContractYearsRemaining + " г.";
    }

    private void RenderFreeAgents(GameState state)
    {
        ClearRows(_freeAgentsContainer, _freeAgentRowPrefab.transform);
        _freeAgentRowPrefab.gameObject.SetActive(false);

        List<PlayerData> freeAgents = FreeAgentService.GetAvailableFreeAgents(state);
        if (freeAgents.Count == 0)
        {
            CreateInfoRow(_freeAgentsContainer, "Свободных агентов нет");
            return;
        }

        foreach (PlayerData player in freeAgents)
        {
            FreeAgentRowView row = Instantiate(_freeAgentRowPrefab, _freeAgentsContainer);
            row.name = player.Id + "-free-agent-row";
            row.gameObject.SetActive(true);
            row.Initialize(player, _screenController);
        }
    }

    private void RenderHistory(GameState state)
    {
        ClearRows(_historyContainer, _historyRowPrefab.transform);
        _historyRowPrefab.gameObject.SetActive(false);

        if (state == null || state.FreeAgentHistory == null || state.FreeAgentHistory.Signings == null || state.FreeAgentHistory.Signings.Count == 0)
        {
            CreateInfoRow(_historyContainer, "История подписаний пуста");
            return;
        }

        int firstIndex = Mathf.Max(0, state.FreeAgentHistory.Signings.Count - 10);
        for (int i = state.FreeAgentHistory.Signings.Count - 1; i >= firstIndex; i--)
        {
            FreeAgentSigningHistoryRowView row = Instantiate(_historyRowPrefab, _historyContainer);
            row.name = "free-agent-signing-" + i;
            row.gameObject.SetActive(true);
            row.Initialize(state.FreeAgentHistory.Signings[i]);
        }
    }

    private bool HasRequiredReferences()
    {
        return _statusText != null
            && _financeText != null
            && _selectedFreeAgentText != null
            && _freeAgentsContainer != null
            && _historyContainer != null
            && _freeAgentRowPrefab != null
            && _historyRowPrefab != null;
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
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
