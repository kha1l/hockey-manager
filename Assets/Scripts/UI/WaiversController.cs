using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class WaiversController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _selectedWaiverText;
    [SerializeField] private Transform _activeContainer;
    [SerializeField] private Transform _historyContainer;
    [SerializeField] private WaiverRowView _activeRowPrefab;
    [SerializeField] private WaiverRowView _historyRowPrefab;

    private GameScreenController _screenController;

    public void Configure(
        Text summaryText,
        Text selectedWaiverText,
        Transform activeContainer,
        Transform historyContainer,
        WaiverRowView activeRowPrefab,
        WaiverRowView historyRowPrefab)
    {
        _summaryText = summaryText;
        _selectedWaiverText = selectedWaiverText;
        _activeContainer = activeContainer;
        _historyContainer = historyContainer;
        _activeRowPrefab = activeRowPrefab;
        _historyRowPrefab = historyRowPrefab;
    }

    public void ShowWaivers(GameState state, string selectedWaiverId, GameScreenController screenController)
    {
        _screenController = screenController;
        if (_activeContainer == null || _historyContainer == null || _activeRowPrefab == null || _historyRowPrefab == null)
        {
            Debug.LogError("WaiversController: UI references are not configured.");
            return;
        }

        WaiverService.EnsureWaiverWire(state);
        ClearRows(_activeContainer, _activeRowPrefab);
        ClearRows(_historyContainer, _historyRowPrefab);
        _activeRowPrefab.gameObject.SetActive(false);
        _historyRowPrefab.gameObject.SetActive(false);

        List<WaiverPlayerData> activeWaivers = state == null || state.WaiverWire == null
            ? new List<WaiverPlayerData>()
            : state.WaiverWire.ActiveWaivers;
        List<WaiverPlayerData> history = state == null || state.WaiverWire == null
            ? new List<WaiverPlayerData>()
            : state.WaiverWire.WaiverHistory;

        SetText(_summaryText, BuildSummaryText(activeWaivers, history));
        SetText(_selectedWaiverText, BuildSelectedWaiverText(state, selectedWaiverId));

        if (activeWaivers.Count == 0)
        {
            SetText(_summaryText, BuildSummaryText(activeWaivers, history) + "\nСписок waivers пуст");
        }

        int shownActive = UiDisplayLimitConfig.ClampRowCount(activeWaivers.Count, UiDisplayLimitConfig.MaxWaiverRows);
        for (int i = 0; i < shownActive; i++)
        {
            CreateRow(_activeContainer, _activeRowPrefab, activeWaivers[i], screenController);
        }

        string activeLimitMessage = UiDisplayLimitConfig.BuildLimitMessage(shownActive, activeWaivers.Count);
        if (!string.IsNullOrEmpty(activeLimitMessage))
        {
            CreateInfoRow(_activeContainer, activeLimitMessage);
        }

        int shownHistory = UiDisplayLimitConfig.ClampRowCount(history.Count, UiDisplayLimitConfig.MaxHistoryRows);
        for (int i = 0; i < shownHistory; i++)
        {
            CreateRow(_historyContainer, _historyRowPrefab, history[i], screenController);
        }

        string historyLimitMessage = UiDisplayLimitConfig.BuildLimitMessage(shownHistory, history.Count);
        if (!string.IsNullOrEmpty(historyLimitMessage))
        {
            CreateInfoRow(_historyContainer, historyLimitMessage);
        }
    }

    public void ShowWaivers(GameState state, string selectedWaiverId)
    {
        ShowWaivers(state, selectedWaiverId, _screenController);
    }

    private static string BuildSummaryText(List<WaiverPlayerData> activeWaivers, List<WaiverPlayerData> history)
    {
        return "Active waivers: " + (activeWaivers == null ? 0 : activeWaivers.Count)
            + " | History: " + (history == null ? 0 : history.Count);
    }

    private static string BuildSelectedWaiverText(GameState state, string selectedWaiverId)
    {
        WaiverPlayerData waiver = FindWaiver(state, selectedWaiverId);
        if (waiver == null)
        {
            return "Waiver entry не выбран";
        }

        string reason = "Команда пользователя не выбрана";
        bool canClaim = GameSession.CurrentTeam != null
            && WaiverService.CanTeamClaimPlayer(GameSession.CurrentTeam, waiver, state, out reason);

        return "Выбран: " + waiver.PlayerName
            + " | " + waiver.OriginalTeamName
            + " | " + waiver.Position
            + " | Age " + waiver.Age
            + " | OVR " + waiver.Overall
            + " | POT " + waiver.Potential
            + " | $" + FormatMoney(waiver.Salary)
            + " | " + waiver.ContractYearsRemaining + " г."
            + "\nНазначение: " + waiver.IntendedDestination
            + " | Days: " + waiver.DaysRemaining
            + " | Status: " + waiver.Status
            + "\nClaim пользователем: " + (canClaim ? "да" : "нет") + " | " + reason;
    }

    private static WaiverPlayerData FindWaiver(GameState state, string waiverId)
    {
        if (state == null || state.WaiverWire == null || string.IsNullOrEmpty(waiverId))
        {
            return null;
        }

        foreach (WaiverPlayerData waiver in state.WaiverWire.ActiveWaivers)
        {
            if (waiver != null && waiver.WaiverId == waiverId)
            {
                return waiver;
            }
        }

        foreach (WaiverPlayerData waiver in state.WaiverWire.WaiverHistory)
        {
            if (waiver != null && waiver.WaiverId == waiverId)
            {
                return waiver;
            }
        }

        return null;
    }

    private static void CreateRow(
        Transform container,
        WaiverRowView prefab,
        WaiverPlayerData waiver,
        GameScreenController screenController)
    {
        WaiverRowView row = Instantiate(prefab, container);
        row.name = waiver.WaiverId + "-waiver-row";
        row.gameObject.SetActive(true);
        row.Initialize(waiver, screenController);
    }

    private static void ClearRows(Transform container, WaiverRowView prefab)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child == prefab.transform)
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
        text.fontSize = 15;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    private static void SetText(Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
