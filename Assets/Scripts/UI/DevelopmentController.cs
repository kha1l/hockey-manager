using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevelopmentController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Transform _changesContainer;
    [SerializeField] private DevelopmentRowView _rowPrefab;

    public void Configure(
        Text summaryText,
        Transform changesContainer,
        DevelopmentRowView rowPrefab)
    {
        _summaryText = summaryText;
        _changesContainer = changesContainer;
        _rowPrefab = rowPrefab;
    }

    public void ShowDevelopment(GameState state)
    {
        if (_summaryText == null || _changesContainer == null || _rowPrefab == null)
        {
            Debug.LogError("DevelopmentController: UI references are not configured.");
            return;
        }

        PlayerDevelopmentService.EnsureDevelopmentHistory(state);
        RenderSummary(state);
        RenderChanges(state);
    }

    private void RenderSummary(GameState state)
    {
        if (state == null || state.PlayerDevelopmentHistory == null)
        {
            _summaryText.text = "История развития пока пуста. Завершите сезон и начните следующий.";
            return;
        }

        int totalChanges = state.PlayerDevelopmentHistory.Changes == null
            ? 0
            : state.PlayerDevelopmentHistory.Changes.Count;
        int lastProcessedSeason = state.PlayerDevelopmentHistory.LastProcessedSeasonStartYear;
        int userTeamChanges = CountUserTeamChanges(state);
        string seasonText = state.CurrentSeasonStartYear + "-" + (state.CurrentSeasonEndYear % 100).ToString("D2");

        _summaryText.text = "Текущий сезон: " + seasonText
            + "\nВсего записей развития: " + totalChanges
            + "\nПоследний обработанный сезон: " + (lastProcessedSeason <= 0 ? "нет" : lastProcessedSeason.ToString())
            + "\nИзменения команды пользователя за последний переход: " + userTeamChanges;
    }

    private void RenderChanges(GameState state)
    {
        ClearRows();
        _rowPrefab.gameObject.SetActive(false);

        if (state == null
            || state.PlayerDevelopmentHistory == null
            || state.PlayerDevelopmentHistory.Changes == null
            || state.PlayerDevelopmentHistory.Changes.Count == 0)
        {
            CreateInfoRow("История развития пока пуста. Завершите сезон и начните следующий.");
            return;
        }

        List<PlayerDevelopmentChangeData> changes = new List<PlayerDevelopmentChangeData>();
        AddUserTeamChangesFirst(state, changes);
        AddRecentChanges(state, changes, UiDisplayLimitConfig.MaxDevelopmentRows);

        foreach (PlayerDevelopmentChangeData change in changes)
        {
            DevelopmentRowView row = Instantiate(_rowPrefab, _changesContainer);
            row.name = "development-" + change.ChangeId;
            row.gameObject.SetActive(true);
            row.Initialize(change);
        }

        int totalChanges = state.PlayerDevelopmentHistory.Changes.Count;
        string limitMessage = UiDisplayLimitConfig.BuildLimitMessage(changes.Count, totalChanges);
        if (!string.IsNullOrEmpty(limitMessage))
        {
            CreateInfoRow(limitMessage);
        }
    }

    private void AddUserTeamChangesFirst(GameState state, List<PlayerDevelopmentChangeData> output)
    {
        int processedSeason = state.PlayerDevelopmentHistory.LastProcessedSeasonStartYear;
        if (processedSeason <= 0)
        {
            return;
        }

        foreach (PlayerDevelopmentChangeData change in state.PlayerDevelopmentHistory.Changes)
        {
            if (change != null
                && change.SeasonStartYear == processedSeason
                && change.TeamId == state.SelectedTeamId
                && output.Count < 12)
            {
                output.Add(change);
            }
        }
    }

    private void AddRecentChanges(GameState state, List<PlayerDevelopmentChangeData> output, int limit)
    {
        List<PlayerDevelopmentChangeData> recentChanges = GameSession.GetRecentDevelopmentChanges(limit);
        foreach (PlayerDevelopmentChangeData change in recentChanges)
        {
            if (change != null && !ContainsChange(output, change.ChangeId))
            {
                output.Add(change);
            }
        }
    }

    private int CountUserTeamChanges(GameState state)
    {
        if (state == null
            || state.PlayerDevelopmentHistory == null
            || state.PlayerDevelopmentHistory.Changes == null)
        {
            return 0;
        }

        int processedSeason = state.PlayerDevelopmentHistory.LastProcessedSeasonStartYear;
        int count = 0;
        foreach (PlayerDevelopmentChangeData change in state.PlayerDevelopmentHistory.Changes)
        {
            if (change != null
                && change.SeasonStartYear == processedSeason
                && change.TeamId == state.SelectedTeamId)
            {
                count++;
            }
        }

        return count;
    }

    private void ClearRows()
    {
        Transform prefabTransform = _rowPrefab.transform;
        for (int i = _changesContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _changesContainer.GetChild(i);
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
        rowObject.transform.SetParent(_changesContainer, false);

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

    private static bool ContainsChange(List<PlayerDevelopmentChangeData> changes, string changeId)
    {
        foreach (PlayerDevelopmentChangeData change in changes)
        {
            if (change != null && change.ChangeId == changeId)
            {
                return true;
            }
        }

        return false;
    }
}
