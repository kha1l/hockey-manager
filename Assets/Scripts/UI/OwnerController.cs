using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class OwnerController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _financeText;
    [SerializeField] private Transform _goalsContainer;
    [SerializeField] private Transform _evaluationHistoryContainer;
    [SerializeField] private OwnerGoalRowView _goalRowPrefab;
    [SerializeField] private OwnerEvaluationRowView _evaluationRowPrefab;

    public void Configure(
        Text summaryText,
        Text financeText,
        Transform goalsContainer,
        Transform evaluationHistoryContainer,
        OwnerGoalRowView goalRowPrefab,
        OwnerEvaluationRowView evaluationRowPrefab)
    {
        _summaryText = summaryText;
        _financeText = financeText;
        _goalsContainer = goalsContainer;
        _evaluationHistoryContainer = evaluationHistoryContainer;
        _goalRowPrefab = goalRowPrefab;
        _evaluationRowPrefab = evaluationRowPrefab;
    }

    public void ShowOwner(GameState state)
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("OwnerController: UI references are not configured.");
            return;
        }

        GameSession.EnsureOwnerGoals();
        OwnerProfileData profile = GameSession.GetCurrentTeamOwnerProfile();
        if (profile == null)
        {
            _summaryText.text = "Профиль владельца недоступен";
            _financeText.text = "Финансы клуба недоступны";
            ClearRows(_goalsContainer, _goalRowPrefab.transform);
            ClearRows(_evaluationHistoryContainer, _evaluationRowPrefab.transform);
            return;
        }

        profile.EnsureCollections();
        RenderSummary(profile);
        RenderFinance(profile.Finances);
        RenderGoals(profile.CurrentGoals);
        RenderEvaluationHistory(profile);
    }

    private void RenderSummary(OwnerProfileData profile)
    {
        string satisfactionLabel = OwnerGoalConfig.GetOwnerSatisfactionLabel(profile.OwnerSatisfaction);
        GmCareerData career = GameSession.GetGmCareer();
        string careerSummary = career == null
            ? "GM career: нет данных"
            : "GM Career: " + career.CareerStatus
                + " | Security " + GmJobSecurityConfig.GetJobSecurityLabel(career.CurrentJobSecurity) + " " + career.CurrentJobSecurity
                + " | Trust " + career.CurrentOwnerTrust
                + "\nLast GM event: " + (string.IsNullOrEmpty(career.LastCareerEventSummary) ? "none" : career.LastCareerEventSummary);
        _summaryText.text = profile.TeamName
            + "\nDirection: " + profile.TeamDirection
            + "\nGM trust: " + profile.GmTrust
            + " | Job security: " + profile.JobSecurity
            + "\nOwner satisfaction: " + profile.OwnerSatisfaction + " (" + satisfactionLabel + ")"
            + "\n" + careerSummary
            + "\n" + profile.ExpectationsSummary;
    }

    private void RenderFinance(ClubFinanceData finances)
    {
        if (finances == null)
        {
            _financeText.text = "Финансы клуба недоступны";
            return;
        }

        _financeText.text = "Payroll: " + FormatMoney(finances.Payroll)
            + " | Cap space: " + FormatMoney(finances.SalaryCapSpace)
            + " | Budget: " + FormatMoney(finances.Budget)
            + "\nRevenue est.: " + FormatMoney(finances.RevenueEstimate)
            + " | Expenses est.: " + FormatMoney(finances.ExpensesEstimate)
            + " | Profit est.: " + FormatMoney(finances.ProfitEstimate)
            + "\nFan interest: " + finances.FanInterestScore
            + " | Star power: " + finances.StarPowerScore
            + " | Health: " + finances.FinancialHealthLabel
            + "\n" + finances.FinanceSummary;
    }

    private void RenderGoals(List<OwnerGoalData> goals)
    {
        ClearRows(_goalsContainer, _goalRowPrefab.transform);
        _goalRowPrefab.gameObject.SetActive(false);

        if (goals == null || goals.Count == 0)
        {
            CreateInfoRow(_goalsContainer, "Цели владельца пока не созданы");
            return;
        }

        foreach (OwnerGoalData goal in goals)
        {
            if (goal == null)
            {
                continue;
            }

            OwnerGoalRowView row = Instantiate(_goalRowPrefab, _goalsContainer);
            row.name = "owner-goal-" + goal.GoalType;
            row.gameObject.SetActive(true);
            row.Initialize(goal);
        }
    }

    private void RenderEvaluationHistory(OwnerProfileData profile)
    {
        ClearRows(_evaluationHistoryContainer, _evaluationRowPrefab.transform);
        _evaluationRowPrefab.gameObject.SetActive(false);

        if (profile.EvaluationHistory == null || profile.EvaluationHistory.Count == 0)
        {
            CreateInfoRow(_evaluationHistoryContainer, "История оценок владельца пока пуста");
            return;
        }

        int firstIndex = Mathf.Max(0, profile.EvaluationHistory.Count - 10);
        for (int i = profile.EvaluationHistory.Count - 1; i >= firstIndex; i--)
        {
            OwnerSeasonEvaluationData evaluation = profile.EvaluationHistory[i];
            if (evaluation == null)
            {
                continue;
            }

            OwnerEvaluationRowView row = Instantiate(_evaluationRowPrefab, _evaluationHistoryContainer);
            row.name = "owner-evaluation-" + evaluation.SeasonStartYear;
            row.gameObject.SetActive(true);
            row.Initialize(evaluation);
        }
    }

    private bool HasRequiredReferences()
    {
        return _summaryText != null
            && _financeText != null
            && _goalsContainer != null
            && _evaluationHistoryContainer != null
            && _goalRowPrefab != null
            && _evaluationRowPrefab != null;
    }

    private static void ClearRows(Transform container, Transform prefabTransform)
    {
        if (container == null || prefabTransform == null)
        {
            return;
        }

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
