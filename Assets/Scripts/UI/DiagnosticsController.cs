using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DiagnosticsController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _migrationText;
    [SerializeField] private Text _validationText;
    [SerializeField] private Text _balanceText;
    [SerializeField] private Text _issuesText;

    public void Configure(
        Text summaryText,
        Text migrationText,
        Text validationText,
        Text balanceText,
        Text issuesText)
    {
        _summaryText = summaryText;
        _migrationText = migrationText;
        _validationText = validationText;
        _balanceText = balanceText;
        _issuesText = issuesText;
    }

    public void ShowDiagnostics(GameState state)
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (_summaryText != null)
            {
                _summaryText.text = state == null
                    ? "Diagnostics: no active game"
                        : "SaveVersion: " + state.SaveVersion
                        + "\nLeague: " + SafeText(state.LeagueDisplayName, "unknown")
                        + " | Identity: " + SafeText(state.LeagueIdentityId, "missing")
                        + " v" + state.LeagueIdentityVersion
                        + " | Teams: " + (state.Teams == null ? 0 : state.Teams.Count)
                        + "\nLast stability check: " + SafeText(state.LastStabilityCheckAtUtc, "not run")
                        + "\nAndroid readiness: " + SafeText(state.LastAndroidReadinessCheckAtUtc, "not run")
                        + "\n" + BuildTeamAssetDiagnosticsText()
                        + "\n" + BuildTutorialDiagnostics(state)
                        + "\n" + GameSession.GetDiagnosticsSummary();
            }

            RenderMigration(state == null ? null : state.LastMigrationReport);
            RenderValidation(state == null ? null : state.LastValidationReport);
            RenderBalance(state == null ? null : state.LastBalanceReport);
            RenderIssues(state == null ? null : state.LastValidationReport);
        }
        finally
        {
            stopwatch.Stop();
            PerformanceTimerService.RecordDiagnostics(state, stopwatch.ElapsedMilliseconds);
        }
    }

    private void RenderMigration(MigrationReportData report)
    {
        if (_migrationText == null)
        {
            return;
        }

        if (report == null)
        {
            _migrationText.text = "Migration: not run yet.";
            return;
        }

        _migrationText.text = "Migration: " + report.Status
            + "\nVersion: " + report.FromSaveVersion + " -> " + report.ToSaveVersion
            + "\nWarnings: " + report.WarningsCount
            + " | Repairs: " + report.RepairsCount
            + " | Errors: " + report.ErrorsCount;
    }

    private void RenderValidation(GameStateValidationReportData report)
    {
        if (_validationText == null)
        {
            return;
        }

        if (report == null)
        {
            _validationText.text = "Validation: not run yet. Validate проверяет сейв, Repair Safe исправляет безопасные проблемы.";
            return;
        }

        _validationText.text = "Validation"
            + "\nIssues: " + report.IssuesCount
            + " | W/E/C: " + report.WarningsCount + "/" + report.ErrorsCount + "/" + report.CriticalCount
            + "\nAuto repairable: " + report.AutoRepairableCount
            + " | Repaired: " + report.RepairedCount
            + "\n" + SafeText(report.Summary, "No summary");
    }

    private void RenderBalance(BalanceReportData report)
    {
        if (_balanceText == null)
        {
            return;
        }

        AlphaBalanceReportData alphaReport = GameSession.CurrentState == null
            ? null
            : GameSession.CurrentState.LastAlphaBalanceReport;
        AlphaPlaytestChecklistData checklist = AlphaPlaytestChecklistService.BuildChecklist();

        if (report == null && alphaReport == null)
        {
            _balanceText.text = "Balance: not run yet.\nAlpha balance report has not been run yet.\n" + FormatChecklist(checklist);
            return;
        }

        string text = "";
        if (report == null)
        {
            text = "Balance: not run yet.";
        }
        else
        {
            text = "Balance"
                + "\nTeams: " + report.TeamsCount
                + " | Players: " + report.PlayersCount
                + " | Free agents: " + report.FreeAgentsCount
                + "\nInvalid roster/lineup/cap: " + report.InvalidRosterTeams + "/" + report.InvalidLineupTeams + "/" + report.CapViolationTeams
                + "\nAvg OVR: " + report.AverageTeamOverall
                + " | Avg Pro roster: " + report.AverageNhlRosterSize
                + " | Avg morale/chem: " + report.AverageMorale + "/" + report.AverageChemistry
                + "\nDraft: " + SafeText(report.DraftClassSummary, "none")
                + "\n" + SafeText(report.Summary, "No summary");
        }

        text += "\n\n" + BuildAlphaBalanceText(alphaReport)
            + "\n\n" + BalanceTuningNotes.Notes
            + "\n" + BalanceTuningNotes.GetMatchBalanceNotes()
            + "\n" + BalanceTuningNotes.GetInjuryBalanceNotes()
            + "\n" + BalanceTuningNotes.GetContractBalanceNotes()
            + "\n" + BalanceTuningNotes.GetDevelopmentBalanceNotes()
            + "\n" + BalanceTuningNotes.GetOwnerBalanceNotes()
            + "\n\n" + FormatChecklist(checklist)
            + "\n\n" + BuildAndroidDiagnosticsText(GameSession.CurrentState);
        _balanceText.text = text;
    }

    private static string BuildTeamAssetDiagnosticsText()
    {
        List<string> warnings = TeamAssetValidationService.ValidateTeamAssets();
        if (warnings.Count == 0)
        {
            return "Team assets: OK";
        }

        return "Team assets warnings: " + warnings.Count;
    }

    private void RenderIssues(GameStateValidationReportData report)
    {
        if (_issuesText == null)
        {
            return;
        }

        if (report == null || report.Issues == null || report.Issues.Count == 0)
        {
            _issuesText.text = "Validation issues: none";
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Top validation issues");
        int limit = UiDisplayLimitConfig.ClampRowCount(report.Issues.Count, UiDisplayLimitConfig.MaxDiagnosticsIssues);
        List<ValidationIssueData> issues = new List<ValidationIssueData>(report.Issues);
        issues.Sort(CompareIssues);
        for (int i = 0; i < limit; i++)
        {
            ValidationIssueData issue = issues[i];
            if (issue == null)
            {
                continue;
            }

            builder.Append(issue.Severity)
                .Append(" | ")
                .Append(issue.Category)
                .Append(" | ")
                .Append(issue.Message);

            if (!string.IsNullOrEmpty(issue.TeamName))
            {
                builder.Append(" | ").Append(issue.TeamName);
            }

            if (!string.IsNullOrEmpty(issue.SuggestedRepair))
            {
                builder.Append(" | Fix: ").Append(issue.SuggestedRepair);
            }

            if (issue.WasRepaired)
            {
                builder.Append(" | repaired");
            }

            builder.AppendLine();
        }

        string limitMessage = UiDisplayLimitConfig.BuildLimitMessage(limit, report.Issues.Count);
        if (!string.IsNullOrEmpty(limitMessage))
        {
            builder.AppendLine(limitMessage);
        }

        _issuesText.text = builder.ToString();
    }

    private static int CompareIssues(ValidationIssueData left, ValidationIssueData right)
    {
        return GetSeverityRank(right == null ? "" : right.Severity)
            .CompareTo(GetSeverityRank(left == null ? "" : left.Severity));
    }

    private static int GetSeverityRank(string severity)
    {
        if (severity == GameStateValidationService.SeverityCritical)
        {
            return 4;
        }

        if (severity == GameStateValidationService.SeverityError)
        {
            return 3;
        }

        if (severity == GameStateValidationService.SeverityWarning)
        {
            return 2;
        }

        return 1;
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrEmpty(value) ? fallback : value;
    }

    private static string BuildTutorialDiagnostics(GameState state)
    {
        TutorialService.EnsureTutorial(state);
        if (state == null || state.Tutorial == null)
        {
            return "Tutorial: unavailable";
        }

        int completed = state.Tutorial.CompletedStepIds == null ? 0 : state.Tutorial.CompletedStepIds.Count;
        return "Tutorial: " + (state.Tutorial.IsTutorialEnabled ? "enabled" : "disabled")
            + " | completed " + completed
            + " | version " + state.Tutorial.TutorialVersion;
    }

    private static string BuildAlphaBalanceText(AlphaBalanceReportData report)
    {
        if (report == null)
        {
            return "Alpha balance report has not been run yet.";
        }

        report.EnsureCollections();
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Alpha Balance")
            .Append("Seasons requested: ").Append(report.SeasonsRequested)
            .Append(" | completed: ").Append(report.SeasonsCompleted)
            .Append(" | simulated live: ").Append(report.SimulatedDuringReport ? "yes" : "no")
            .AppendLine()
            .Append("Metrics: ").Append(report.MetricsCount)
            .Append(" | pass/warn/critical: ").Append(report.PassedCount)
            .Append("/").Append(report.WarningCount)
            .Append("/").Append(report.CriticalCount)
            .AppendLine()
            .AppendLine(SafeText(report.Summary, "No alpha summary"))
            .AppendLine(SafeText(report.Recommendation, "No recommendation"));

        if (report.Metrics == null || report.Metrics.Count == 0)
        {
            builder.AppendLine("No metrics available.");
            return builder.ToString();
        }

        builder.AppendLine("Top alpha metrics");
        int shown = 0;
        foreach (AlphaBalanceMetricData metric in report.Metrics)
        {
            if (metric == null)
            {
                continue;
            }

            if (metric.Passed && shown >= 8)
            {
                continue;
            }

            builder.Append(metric.Category)
                .Append(" | ")
                .Append(metric.Name)
                .Append(" | value ")
                .Append(metric.Value)
                .Append(" | target ")
                .Append(FormatMetricTarget(metric))
                .Append(" | ")
                .Append(metric.Status)
                .Append(" | ")
                .Append(metric.Message)
                .AppendLine();

            shown++;
            if (shown >= UiDisplayLimitConfig.MaxAlphaMetrics)
            {
                break;
            }
        }

        return builder.ToString();
    }

    private static string FormatMetricTarget(AlphaBalanceMetricData metric)
    {
        if (metric == null)
        {
            return "";
        }

        return metric.MaxTarget <= 0
            ? "min " + metric.MinTarget
            : metric.MinTarget + ".." + metric.MaxTarget;
    }

    private static string FormatChecklist(AlphaPlaytestChecklistData checklist)
    {
        if (checklist == null || checklist.ChecklistItems == null || checklist.ChecklistItems.Count == 0)
        {
            return "Alpha checklist unavailable.";
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(SafeText(checklist.Summary, "Alpha playtest checklist"));
        int limit = checklist.ChecklistItems.Count < 8 ? checklist.ChecklistItems.Count : 8;
        for (int i = 0; i < limit; i++)
        {
            builder.Append("- ").AppendLine(checklist.ChecklistItems[i]);
        }

        if (checklist.ChecklistItems.Count > limit)
        {
            builder.Append("... ").Append(checklist.ChecklistItems.Count - limit).AppendLine(" more");
        }

        return builder.ToString();
    }

    private static string BuildAndroidDiagnosticsText(GameState state)
    {
        AndroidPerformanceData performance = state == null ? null : state.AndroidPerformance;
        return PerformanceTimerService.BuildPerformanceSummary(state)
            + "\n" + AndroidPerformanceNotes.GetSaveLoadPerformanceNotes(performance)
            + "\n" + AndroidPerformanceNotes.GetSimulationPerformanceNotes(performance)
            + "\n" + AndroidPerformanceNotes.GetUiPerformanceNotes(performance)
            + "\n\n" + AndroidReadinessService.BuildChecklistText(state == null ? null : state.AndroidReadinessChecklist)
            + "\n\n" + AndroidBuildChecklistService.BuildManualChecklist();
    }
}
