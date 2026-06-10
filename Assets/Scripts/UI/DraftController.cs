using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftController : MonoBehaviour
{
    [SerializeField] private Text _statusText;
    [SerializeField] private Text _currentPickText;
    [SerializeField] private Text _selectedProspectText;
    [SerializeField] private Transform _prospectsContainer;
    [SerializeField] private Transform _recentPicksContainer;
    [SerializeField] private Transform _draftRightsContainer;
    [SerializeField] private Transform _ownedPicksContainer;
    [SerializeField] private ProspectRowView _prospectRowPrefab;
    [SerializeField] private DraftPickRowView _draftPickRowPrefab;
    [SerializeField] private DraftRightsRowView _draftRightsRowPrefab;
    [SerializeField] private GameScreenController _screenController;

    public void Configure(
        Text statusText,
        Text currentPickText,
        Text selectedProspectText,
        Transform prospectsContainer,
        Transform recentPicksContainer,
        Transform draftRightsContainer,
        Transform ownedPicksContainer,
        ProspectRowView prospectRowPrefab,
        DraftPickRowView draftPickRowPrefab,
        DraftRightsRowView draftRightsRowPrefab,
        GameScreenController screenController)
    {
        _statusText = statusText;
        _currentPickText = currentPickText;
        _selectedProspectText = selectedProspectText;
        _prospectsContainer = prospectsContainer;
        _recentPicksContainer = recentPicksContainer;
        _draftRightsContainer = draftRightsContainer;
        _ownedPicksContainer = ownedPicksContainer;
        _prospectRowPrefab = prospectRowPrefab;
        _draftPickRowPrefab = draftPickRowPrefab;
        _draftRightsRowPrefab = draftRightsRowPrefab;
        _screenController = screenController;
    }

    public void ShowDraft(GameState state, string selectedProspectId)
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("DraftController: UI references are not configured.");
            return;
        }

        DraftService.EnsureDraft(state);
        RenderStatus(state);
        RenderCurrentPick(state);
        RenderSelectedProspect(state, selectedProspectId);
        RenderProspects(state);
        RenderRecentPicks(state);
        RenderDraftRights(state);
        RenderOwnedPicks(state);
    }

    private void RenderStatus(GameState state)
    {
        string draftStartDate = state == null || state.LeagueCalendar == null ? "" : state.LeagueCalendar.DraftStartDate;
        string draftEndDate = state == null || state.LeagueCalendar == null ? "" : state.LeagueCalendar.DraftEndDate;
        string calendarStatus = state == null || state.LeagueCalendar == null ? "" : state.LeagueCalendar.CalendarStatus;
        string phase = LeaguePhaseService.GetCurrentPhase(state);
        string draftStatus = state == null || state.Draft == null || string.IsNullOrEmpty(state.Draft.DraftStatus)
            ? "NotStarted"
            : state.Draft.DraftStatus;
        int draftYear = DraftPickOwnershipService.GetDraftYear(state);

        string availability = "Драфт станет доступен после завершения плей-офф";
        if (DraftService.IsDraftAvailable(state))
        {
            if (state.Draft != null && state.Draft.IsCompleted)
            {
                availability = "Драфт завершён";
            }
            else if (DraftService.IsUserOnClock(state))
            {
                availability = "Вы на часах";
            }
            else
            {
                DraftPickData pick = DraftService.GetCurrentPick(state);
                availability = pick == null ? "Драфт завершён" : "Сейчас выбирает: " + pick.TeamName;
            }
        }

        _statusText.text = "Фаза сезона: " + phase
            + "\nDraftStartDate: " + draftStartDate
            + " | DraftEndDate: " + draftEndDate
            + "\nCalendarStatus: " + calendarStatus
            + "\nDraftYear: " + draftYear
            + " | DraftStatus: " + draftStatus
            + " | TotalRounds: " + DraftConfig.DraftRounds
            + "\n" + availability;
    }

    private void RenderCurrentPick(GameState state)
    {
        DraftPickData pick = DraftService.GetCurrentPick(state);
        if (!DraftService.IsDraftAvailable(state))
        {
            _currentPickText.text = "Текущий выбор: драфт пока недоступен";
            return;
        }

        _currentPickText.text = pick == null
            ? "Текущий выбор: драфт завершён"
            : "Текущий выбор: #" + pick.OverallPick
                + " | Round " + pick.Round
                + " | Pick " + pick.PickInRound
                + " | " + pick.TeamName
                + " | original " + pick.OriginalTeamName;
    }

    private void RenderSelectedProspect(GameState state, string selectedProspectId)
    {
        ProspectData prospect = FindProspect(state, selectedProspectId);
        _selectedProspectText.text = prospect == null
            ? "Выбранный проспект: не выбран"
            : "Выбранный проспект: " + prospect.FirstName + " " + prospect.LastName
                + " | " + prospect.Position
                + " | OVR " + prospect.Overall
                + " | POT " + prospect.Potential;
    }

    private void RenderProspects(GameState state)
    {
        ClearRows(_prospectsContainer, _prospectRowPrefab.transform);
        _prospectRowPrefab.gameObject.SetActive(false);

        if (!DraftService.IsDraftAvailable(state))
        {
            CreateInfoRow(_prospectsContainer, "Драфт станет доступен после завершения плей-офф");
            return;
        }

        List<ProspectData> prospects = DraftService.GetAvailableProspects(state);
        if (prospects.Count == 0)
        {
            CreateInfoRow(_prospectsContainer, "Доступных проспектов нет");
            return;
        }

        foreach (ProspectData prospect in prospects)
        {
            ProspectRowView row = Instantiate(_prospectRowPrefab, _prospectsContainer);
            row.name = prospect.Id + "-prospect-row";
            row.gameObject.SetActive(true);
            row.Initialize(prospect, _screenController);
        }
    }

    private void RenderRecentPicks(GameState state)
    {
        ClearRows(_recentPicksContainer, _draftPickRowPrefab.transform);
        _draftPickRowPrefab.gameObject.SetActive(false);

        if (state == null || state.DraftHistory == null || state.DraftHistory.CompletedPicks == null || state.DraftHistory.CompletedPicks.Count == 0)
        {
            CreateInfoRow(_recentPicksContainer, "Выборов пока нет");
            return;
        }

        int firstIndex = Mathf.Max(0, state.DraftHistory.CompletedPicks.Count - 10);
        for (int i = state.DraftHistory.CompletedPicks.Count - 1; i >= firstIndex; i--)
        {
            DraftPickRowView row = Instantiate(_draftPickRowPrefab, _recentPicksContainer);
            row.name = "draft-pick-history-" + i;
            row.gameObject.SetActive(true);
            row.Initialize(state.DraftHistory.CompletedPicks[i]);
        }
    }

    private void RenderDraftRights(GameState state)
    {
        ClearRows(_draftRightsContainer, _draftRightsRowPrefab.transform);
        _draftRightsRowPrefab.gameObject.SetActive(false);

        TeamData team = GetUserTeam(state);
        if (team == null)
        {
            CreateInfoRow(_draftRightsContainer, "Команда не найдена");
            return;
        }

        team.EnsureDraftRights();
        if (team.DraftRights.Count == 0)
        {
            CreateInfoRow(_draftRightsContainer, "Прав на проспектов пока нет");
            return;
        }

        foreach (ProspectData prospect in team.DraftRights)
        {
            DraftRightsRowView row = Instantiate(_draftRightsRowPrefab, _draftRightsContainer);
            row.name = prospect.Id + "-draft-rights-row";
            row.gameObject.SetActive(true);
            row.Initialize(prospect);
        }
    }

    private void RenderOwnedPicks(GameState state)
    {
        ClearRows(_ownedPicksContainer, null);

        TeamData team = GetUserTeam(state);
        if (team == null)
        {
            CreateInfoRow(_ownedPicksContainer, "Команда не найдена");
            return;
        }

        if (state != null && state.Draft != null && state.Draft.IsCompleted)
        {
            CreateInfoRow(_ownedPicksContainer, "Драфт завершён");
            return;
        }

        List<DraftPickOwnershipData> picks = DraftPickOwnershipService.GetOwnedPicks(state, team.Id);
        if (picks.Count == 0)
        {
            CreateInfoRow(_ownedPicksContainer, "Доступных пиков нет");
            return;
        }

        foreach (DraftPickOwnershipData pick in picks)
        {
            CreateInfoRow(_ownedPicksContainer, pick.DraftYear
                + " | Round " + pick.Round
                + " | from " + pick.OriginalTeamName
                + " | owner " + pick.CurrentOwnerTeamName);
        }
    }

    private bool HasRequiredReferences()
    {
        return _statusText != null
            && _currentPickText != null
            && _selectedProspectText != null
            && _prospectsContainer != null
            && _recentPicksContainer != null
            && _draftRightsContainer != null
            && _ownedPicksContainer != null
            && _prospectRowPrefab != null
            && _draftPickRowPrefab != null
            && _draftRightsRowPrefab != null;
    }

    private static ProspectData FindProspect(GameState state, string prospectId)
    {
        if (state == null || state.Draft == null || state.Draft.Prospects == null || string.IsNullOrEmpty(prospectId))
        {
            return null;
        }

        foreach (ProspectData prospect in state.Draft.Prospects)
        {
            if (prospect != null && prospect.Id == prospectId)
            {
                return prospect;
            }
        }

        return null;
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
            if (prefabTransform != null && child == prefabTransform)
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
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
    }
}
