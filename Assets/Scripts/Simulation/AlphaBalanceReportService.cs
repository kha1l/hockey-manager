using System;
using System.Collections.Generic;
using UnityEngine;

public static class AlphaBalanceReportService
{
    public static AlphaBalanceReportData GenerateCurrentStateReport(GameState state)
    {
        AlphaBalanceReportData report = CreateBaseReport(0);
        report.SeasonsCompleted = 0;
        report.SimulatedDuringReport = false;
        AddSnapshotAndMetrics(state, report);
        FinalizeReport(report);
        StoreReport(state, report);
        return report;
    }

    public static AlphaBalanceReportData RunMultiSeasonReport(GameState state, int seasonsToSimulate)
    {
        AlphaBalanceReportData report = CreateBaseReport(seasonsToSimulate);
        report.SeasonsCompleted = 0;
        report.SimulatedDuringReport = false;

        if (state == null)
        {
            report.Metrics.Add(AlphaBalanceMetricService.CreateMinMetric("Simulation", "StateAvailable", 0, 1, "No active game state for alpha balance simulation."));
        }
        else
        {
            AddSnapshotAndMetrics(state, report);
            if (!CanCreateSafeCopy(state, out string copyMessage))
            {
                report.Metrics.Add(AlphaBalanceMetricService.CreateMinMetric("Simulation", "SafeCopyAvailable", 0, 1, copyMessage));
            }
            else
            {
                report.Metrics.Add(AlphaBalanceMetricService.CreateMinMetric("Simulation", "NonDestructiveMultiSeason", 0, 1, "Non-destructive multi-season simulation is not available yet. Current-state report generated instead."));
            }
        }

        FinalizeReport(report);
        StoreReport(state, report);
        return report;
    }

    public static void StoreReport(GameState state, AlphaBalanceReportData report)
    {
        if (state == null || report == null)
        {
            return;
        }

        state.EnsureAlphaBalanceReports();
        report.EnsureCollections();
        state.LastAlphaBalanceReport = report;
        state.LastAlphaBalanceReportAtUtc = DateTime.UtcNow.ToString("o");
        state.AlphaBalanceReportHistory.Add(report);
        TrimHistory(state.AlphaBalanceReportHistory);
    }

    public static string BuildSummary(AlphaBalanceReportData report)
    {
        if (report == null)
        {
            return "Alpha report unavailable.";
        }

        string simulated = report.SimulatedDuringReport
            ? "simulated " + report.SeasonsCompleted + "/" + report.SeasonsRequested + " seasons"
            : "current state only";
        return "Alpha report: " + report.MetricsCount
            + " metrics, " + report.WarningCount
            + " warnings, " + report.CriticalCount
            + " critical (" + simulated + ").";
    }

    public static string BuildRecommendation(AlphaBalanceReportData report)
    {
        if (report == null || report.Metrics == null)
        {
            return "Run alpha balance report again.";
        }

        if (HasMetricProblem(report, "InvalidLineupTeams"))
        {
            return "Run diagnostics repair or review lineup generation.";
        }

        if (HasMetricProblem(report, "AverageGoalsPerGame"))
        {
            AlphaBalanceMetricData metric = FindMetric(report, "AverageGoalsPerGame");
            if (metric != null && metric.Status == "HIGH")
            {
                return "Tune MatchSimulator scoring down.";
            }

            return "Review MatchSimulator scoring if games feel too low event.";
        }

        if (HasMetricProblem(report, "MajorInjuriesCount") || HasMetricProblem(report, "InjuredPlayersCount"))
        {
            return "Reduce InjuryConfig risk or fatigue injury bonuses.";
        }

        if (HasMetricProblem(report, "CapViolationTeams"))
        {
            return "Review CPU roster/free agency cap logic.";
        }

        if (HasMetricProblem(report, "AverageMorale"))
        {
            return "Review MoraleService penalties and owner pressure.";
        }

        if (HasMetricProblem(report, "PlayersOverall90Plus") || HasMetricProblem(report, "PlayersOverall95Plus"))
        {
            return "Tune PlayerDevelopment and draft class potential.";
        }

        if (HasMetricProblem(report, "NonDestructiveMultiSeason"))
        {
            return "Non-destructive multi-season simulation is not available yet. Current-state report generated instead.";
        }

        return "Ready for manual alpha playtest.";
    }

    private static void AddSnapshotAndMetrics(GameState state, AlphaBalanceReportData report)
    {
        if (report == null)
        {
            return;
        }

        report.EnsureCollections();
        AlphaBalanceSeasonSnapshotData snapshot = AlphaBalanceSnapshotService.CreateSnapshot(state);
        report.SeasonSnapshots.Add(snapshot);
        report.Metrics.AddRange(AlphaBalanceMetricService.EvaluateSnapshot(snapshot));
    }

    private static AlphaBalanceReportData CreateBaseReport(int seasonsRequested)
    {
        return new AlphaBalanceReportData
        {
            ReportId = Guid.NewGuid().ToString("N"),
            SeasonsRequested = seasonsRequested,
            StartedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    private static void FinalizeReport(AlphaBalanceReportData report)
    {
        if (report == null)
        {
            return;
        }

        report.EnsureCollections();
        report.CompletedAtUtc = DateTime.UtcNow.ToString("o");
        report.MetricsCount = report.Metrics.Count;
        report.PassedCount = 0;
        report.WarningCount = 0;
        report.CriticalCount = 0;

        foreach (AlphaBalanceMetricData metric in report.Metrics)
        {
            if (metric == null)
            {
                continue;
            }

            if (metric.Passed)
            {
                report.PassedCount++;
            }
            else if (IsCriticalMetric(metric))
            {
                report.CriticalCount++;
            }
            else
            {
                report.WarningCount++;
            }
        }

        report.Summary = BuildSummary(report);
        report.Recommendation = BuildRecommendation(report);
    }

    private static bool CanCreateSafeCopy(GameState state, out string message)
    {
        message = "";
        if (state == null)
        {
            message = "No active GameState.";
            return false;
        }

        try
        {
            string json = JsonUtility.ToJson(state);
            GameState copy = JsonUtility.FromJson<GameState>(json);
            if (copy == null)
            {
                message = "JsonUtility deep copy returned null.";
                return false;
            }

            message = "Safe copy available, but isolated simulation runner is not implemented.";
            return true;
        }
        catch (Exception exception)
        {
            message = "JsonUtility deep copy failed: " + exception.Message;
            return false;
        }
    }

    private static bool HasMetricProblem(AlphaBalanceReportData report, string metricName)
    {
        AlphaBalanceMetricData metric = FindMetric(report, metricName);
        return metric != null && !metric.Passed;
    }

    private static AlphaBalanceMetricData FindMetric(AlphaBalanceReportData report, string metricName)
    {
        if (report == null || report.Metrics == null)
        {
            return null;
        }

        foreach (AlphaBalanceMetricData metric in report.Metrics)
        {
            if (metric != null && metric.Name == metricName)
            {
                return metric;
            }
        }

        return null;
    }

    private static bool IsCriticalMetric(AlphaBalanceMetricData metric)
    {
        return metric != null
            && !metric.Passed
            && (metric.Name == "InvalidRosterTeams" || metric.Name == "InvalidLineupTeams");
    }

    private static void TrimHistory(List<AlphaBalanceReportData> history)
    {
        if (history == null)
        {
            return;
        }

        for (int i = history.Count - 1; i >= 0; i--)
        {
            if (history[i] == null)
            {
                history.RemoveAt(i);
            }
        }

        while (history.Count > AlphaBalanceConfig.MaxReportsToKeep)
        {
            history.RemoveAt(0);
        }
    }
}
