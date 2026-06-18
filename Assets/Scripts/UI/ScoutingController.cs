using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoutingController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _selectedProspectText;
    [SerializeField] private Transform _prospectsContainer;
    [SerializeField] private Transform _reportsContainer;
    [SerializeField] private ScoutingProspectRowView _prospectRowPrefab;
    [SerializeField] private ScoutingReportRowView _reportRowPrefab;
    [SerializeField] private GameScreenController _screenController;

    public void Configure(
        Text summaryText,
        Text selectedProspectText,
        Transform prospectsContainer,
        Transform reportsContainer,
        ScoutingProspectRowView prospectRowPrefab,
        ScoutingReportRowView reportRowPrefab,
        GameScreenController screenController)
    {
        _summaryText = summaryText;
        _selectedProspectText = selectedProspectText;
        _prospectsContainer = prospectsContainer;
        _reportsContainer = reportsContainer;
        _prospectRowPrefab = prospectRowPrefab;
        _reportRowPrefab = reportRowPrefab;
        _screenController = screenController;
    }

    public void ShowScouting(GameState state, string selectedProspectId)
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("ScoutingController: UI references are not configured.");
            return;
        }

        ScoutingService.EnsureScouting(state);
        RenderSummary(state);
        RenderSelectedProspect(state, selectedProspectId);
        RenderProspects(state);
        RenderReports(state);
    }

    private void RenderSummary(GameState state)
    {
        int reportCount = state == null || state.ScoutingHistory == null || state.ScoutingHistory.Reports == null
            ? 0
            : state.ScoutingHistory.Reports.Count;
        int totalActions = state == null || state.ScoutingHistory == null
            ? 0
            : state.ScoutingHistory.TotalScoutingActions;
        string lastAction = state == null || state.ScoutingHistory == null || string.IsNullOrEmpty(state.ScoutingHistory.LastScoutingActionAtUtc)
            ? "нет"
            : state.ScoutingHistory.LastScoutingActionAtUtc;
        int averageAccuracy = CalculateAverageAccuracy(state);
        string draftClassSummary = state != null
            && state.Draft != null
            && state.Draft.Prospects != null
            && state.Draft.Prospects.Count > 0
                ? GameSession.GetCurrentDraftClassSummary()
                : "Draft class: not generated";

        _summaryText.text = "Scouting actions: " + totalActions
            + " | Reports: " + reportCount
            + " | Last: " + lastAction
            + "\nDraft class average accuracy: " + averageAccuracy + "%"
            + "\n" + draftClassSummary
            + "\nScout Player повышает accuracy выбранного игрока сильнее"
            + "\nScout Top Prospects повышает accuracy топ-10"
            + "\nScout by Position повышает accuracy выбранной группы";
    }

    private void RenderSelectedProspect(GameState state, string selectedProspectId)
    {
        ProspectData prospect = ScoutingService.FindProspect(state, selectedProspectId);
        if (prospect == null)
        {
            _selectedProspectText.text = "Выберите проспекта";
            return;
        }

        int rank = ScoutingService.GetProspectRank(prospect, prospect.ProjectedPick);
        ScoutingService.EnsureProspectScouting(prospect, rank);
        _selectedProspectText.text = prospect.FirstName + " " + prospect.LastName
            + " | #" + rank
            + " | " + prospect.ProjectedRound
            + " | " + prospect.ProspectArchetype
            + " | " + prospect.Position
            + " | " + prospect.Age
            + " | OVR " + FormatOverall(prospect)
            + " | POT " + FormatPotential(prospect)
            + " | ACC " + prospect.ScoutingAccuracy + "%"
            + " | " + prospect.ScoutingGrade
            + " | " + prospect.ProjectedRole
            + " | " + prospect.RiskHint
            + " | " + prospect.DraftProjection
            + "\nCeiling: " + prospect.CeilingHint
            + " | Floor: " + prospect.FloorHint
            + " | Profile: " + prospect.DevelopmentTypeHint
            + "\n" + prospect.ScoutingSummary;
    }

    private void RenderProspects(GameState state)
    {
        ClearRows(_prospectsContainer, _prospectRowPrefab.transform);
        _prospectRowPrefab.gameObject.SetActive(false);

        List<ProspectData> prospects = ScoutingService.GetDraftClassProspects(state);
        prospects.Sort(CompareProspectsByRank);
        if (prospects.Count == 0)
        {
            CreateInfoRow(_prospectsContainer, "Драфт-класс пока не создан");
            return;
        }

        int totalEligible = 0;
        foreach (ProspectData prospect in prospects)
        {
            if (prospect != null && !prospect.IsDrafted)
            {
                totalEligible++;
            }
        }

        int shownLimit = UiDisplayLimitConfig.ClampRowCount(totalEligible, UiDisplayLimitConfig.MaxScoutingRows);
        int shown = 0;
        foreach (ProspectData prospect in prospects)
        {
            if (prospect == null || prospect.IsDrafted)
            {
                continue;
            }

            if (shown >= shownLimit)
            {
                break;
            }

            ScoutingProspectRowView row = Instantiate(_prospectRowPrefab, _prospectsContainer);
            row.name = prospect.Id + "-scouting-prospect-row";
            row.gameObject.SetActive(true);
            row.Initialize(prospect, _screenController);
            shown++;
        }

        string limitMessage = UiDisplayLimitConfig.BuildLimitMessage(shown, totalEligible);
        if (!string.IsNullOrEmpty(limitMessage))
        {
            CreateInfoRow(_prospectsContainer, limitMessage);
        }
    }

    private void RenderReports(GameState state)
    {
        ClearRows(_reportsContainer, _reportRowPrefab.transform);
        _reportRowPrefab.gameObject.SetActive(false);

        List<ScoutingReportData> reports = GameSession.GetRecentScoutingReports(12);
        if (reports.Count == 0)
        {
            CreateInfoRow(_reportsContainer, "Scouting reports пока нет");
            return;
        }

        foreach (ScoutingReportData report in reports)
        {
            ScoutingReportRowView row = Instantiate(_reportRowPrefab, _reportsContainer);
            row.name = report.ReportId + "-scouting-report-row";
            row.gameObject.SetActive(true);
            row.Initialize(report);
        }
    }

    private bool HasRequiredReferences()
    {
        return _summaryText != null
            && _selectedProspectText != null
            && _prospectsContainer != null
            && _reportsContainer != null
            && _prospectRowPrefab != null
            && _reportRowPrefab != null;
    }

    private static int CalculateAverageAccuracy(GameState state)
    {
        List<ProspectData> prospects = ScoutingService.GetDraftClassProspects(state);
        if (prospects.Count == 0)
        {
            return 0;
        }

        int total = 0;
        int count = 0;
        foreach (ProspectData prospect in prospects)
        {
            if (prospect == null)
            {
                continue;
            }

            total += prospect.ScoutingAccuracy;
            count++;
        }

        return count == 0 ? 0 : total / count;
    }

    private static int CompareProspectsByRank(ProspectData left, ProspectData right)
    {
        int leftRank = ScoutingService.GetProspectRank(left, 999);
        int rightRank = ScoutingService.GetProspectRank(right, 999);
        return leftRank.CompareTo(rightRank);
    }

    private static string FormatOverall(ProspectData prospect)
    {
        return prospect.IsFullyScouted
            ? prospect.Overall.ToString()
            : prospect.EstimatedOverallMin + "-" + prospect.EstimatedOverallMax;
    }

    private static string FormatPotential(ProspectData prospect)
    {
        return prospect.IsFullyScouted
            ? prospect.Potential.ToString()
            : prospect.EstimatedPotentialMin + "-" + prospect.EstimatedPotentialMax;
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
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
    }
}
