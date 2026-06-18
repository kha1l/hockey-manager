using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InjuriesController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Transform _activeInjuriesContainer;
    [SerializeField] private Transform _historyContainer;
    [SerializeField] private InjuryRowView _activeInjuryRowPrefab;
    [SerializeField] private InjuryRowView _historyRowPrefab;

    public void Configure(
        Text summaryText,
        Transform activeInjuriesContainer,
        Transform historyContainer,
        InjuryRowView activeInjuryRowPrefab,
        InjuryRowView historyRowPrefab)
    {
        _summaryText = summaryText;
        _activeInjuriesContainer = activeInjuriesContainer;
        _historyContainer = historyContainer;
        _activeInjuryRowPrefab = activeInjuryRowPrefab;
        _historyRowPrefab = historyRowPrefab;
    }

    public void ShowInjuries(GameState state)
    {
        if (_activeInjuriesContainer == null || _historyContainer == null || _activeInjuryRowPrefab == null || _historyRowPrefab == null)
        {
            Debug.LogError("InjuriesController: UI references are not configured.");
            return;
        }

        ClearRows(_activeInjuriesContainer, _activeInjuryRowPrefab.transform);
        ClearRows(_historyContainer, _historyRowPrefab.transform);
        _activeInjuryRowPrefab.gameObject.SetActive(false);
        _historyRowPrefab.gameObject.SetActive(false);

        InjuryService.EnsureInjuryHistory(state);
        TeamData team = GameSession.CurrentTeam;
        if (team == null)
        {
            SetSummary("Команда не выбрана");
            return;
        }

        InjuryService.EnsureInjuryFieldsForTeam(team);
        List<PlayerData> injuredPlayers = InjuryService.GetInjuredPlayers(team);
        SetSummary(BuildSummaryText(team, injuredPlayers));

        if (injuredPlayers.Count == 0)
        {
            AddEmptyRow(_activeInjuriesContainer, _activeInjuryRowPrefab, "Активных травм нет");
        }
        else
        {
            int shown = UiDisplayLimitConfig.ClampRowCount(injuredPlayers.Count, UiDisplayLimitConfig.MaxInjuryRows);
            for (int i = 0; i < shown; i++)
            {
                PlayerData player = injuredPlayers[i];
                InjuryRowView row = Instantiate(_activeInjuryRowPrefab, _activeInjuriesContainer);
                row.name = player.Id + "-injury-row";
                row.gameObject.SetActive(true);
                row.Initialize(player, team);
            }

            string limitMessage = UiDisplayLimitConfig.BuildLimitMessage(shown, injuredPlayers.Count);
            if (!string.IsNullOrEmpty(limitMessage))
            {
                AddEmptyRow(_activeInjuriesContainer, _activeInjuryRowPrefab, limitMessage);
            }
        }

        List<InjuryRecordData> history = GetRecentHistory(state, team.Id, UiDisplayLimitConfig.MaxInjuryRows);
        if (history.Count == 0)
        {
            AddEmptyRow(_historyContainer, _historyRowPrefab, "История травм пока пуста");
            return;
        }

        foreach (InjuryRecordData injury in history)
        {
            InjuryRowView row = Instantiate(_historyRowPrefab, _historyContainer);
            row.name = injury.InjuryId + "-injury-history-row";
            row.gameObject.SetActive(true);
            row.Initialize(injury);
        }
    }

    private void SetSummary(string text)
    {
        if (_summaryText != null)
        {
            _summaryText.text = text;
        }
    }

    private static string BuildSummaryText(TeamData team, List<PlayerData> injuredPlayers)
    {
        string teamName = TeamIdentityService.GetDisplayName(team);
        int injuredCount = injuredPlayers == null ? 0 : injuredPlayers.Count;
        string lineupMessage;
        bool hasInjuredActivePlayers = LineupService.HasInjuredActivePlayers(team, out lineupMessage);

        return "Команда: " + teamName
            + "\nАктивных травм: " + injuredCount
            + "\nСтатус состава: " + (hasInjuredActivePlayers ? lineupMessage + " | Нажмите Автосостав" : "травмированных в активном составе нет");
    }

    private static List<InjuryRecordData> GetRecentHistory(GameState state, string teamId, int limit)
    {
        List<InjuryRecordData> history = new List<InjuryRecordData>();
        if (state == null || state.InjuryHistory == null || state.InjuryHistory.Injuries == null)
        {
            return history;
        }

        foreach (InjuryRecordData injury in state.InjuryHistory.Injuries)
        {
            if (injury != null && injury.TeamId == teamId)
            {
                history.Add(injury);
            }
        }

        history.Sort(CompareInjuriesDescending);
        if (limit > 0 && history.Count > limit)
        {
            history.RemoveRange(limit, history.Count - limit);
        }

        return history;
    }

    private static int CompareInjuriesDescending(InjuryRecordData left, InjuryRecordData right)
    {
        return string.Compare(right == null ? "" : right.InjuredAtUtc, left == null ? "" : left.InjuredAtUtc, System.StringComparison.Ordinal);
    }

    private static void AddEmptyRow(Transform container, InjuryRowView prefab, string text)
    {
        InjuryRowView row = Instantiate(prefab, container);
        row.name = "empty-injury-row";
        row.gameObject.SetActive(true);
        Text infoText = row.GetComponentInChildren<Text>();
        if (infoText != null)
        {
            infoText.text = text;
        }
    }

    private static void ClearRows(Transform container, Transform template)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child == template)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }
}
