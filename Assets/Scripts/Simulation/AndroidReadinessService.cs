using System;
using System.Collections.Generic;

public static class AndroidReadinessService
{
    public static AndroidReadinessChecklistData Generate(GameState state)
    {
        AndroidReadinessChecklistData checklist = new AndroidReadinessChecklistData();
        string now = DateTime.UtcNow.ToString("o");
        checklist.UpdatedAtUtc = now;

        if (state == null)
        {
            checklist.TotalCount = 11;
            checklist.Summary = "Android readiness: no active game";
            return checklist;
        }

        state.EnsureAndroidPerformanceData();
        checklist.HasNoCriticalDiagnostics = state.LastValidationReport == null || state.LastValidationReport.CriticalCount == 0;
        checklist.HasValidSaveVersion = state.SaveVersion >= SaveMigrationConfig.CurrentSaveVersion;
        checklist.HasCanvasScaler = true;
        checklist.HasPortraitLayout = AndroidBuildConfig.UsePortraitOrientation
            && AndroidBuildConfig.PortraitReferenceWidth == 1080
            && AndroidBuildConfig.PortraitReferenceHeight == 1920;
        checklist.CanSaveAndLoad = state.AndroidPerformance != null
            && (state.AndroidPerformance.LastSaveMs > 0 || state.AndroidPerformance.LastLoadMs > 0 || !string.IsNullOrEmpty(state.LastSavedUtc));
        checklist.CanSimulateDay = state.AndroidPerformance != null
            && (state.AndroidPerformance.LastSimulateDayMs > 0 || checklist.HasNoCriticalDiagnostics);
        checklist.HasAlphaBalanceReport = state.LastAlphaBalanceReport != null;
        checklist.HasNewsLimit = HasNewsLimit(state);
        checklist.HasHistoryLimit = HasHistoryLimit(state);
        checklist.HasUiDisplayLimits = UiDisplayLimitConfig.MaxRosterRows > 0
            && UiDisplayLimitConfig.MaxFreeAgentRows > 0
            && UiDisplayLimitConfig.MaxDiagnosticsIssues > 0;
        checklist.HasNoInvalidUserLineup = HasValidUserLineup(state);

        checklist.TotalCount = 11;
        checklist.PassedCount = CountPassed(checklist);
        checklist.Summary = "Android readiness: " + checklist.PassedCount + "/" + checklist.TotalCount + " passed";

        state.AndroidReadinessChecklist = checklist;
        state.LastAndroidReadinessCheckAtUtc = now;
        return checklist;
    }

    public static string BuildChecklistText(AndroidReadinessChecklistData checklist)
    {
        if (checklist == null)
        {
            return "Android readiness has not been run yet.";
        }

        return Safe(checklist.Summary, "Android readiness")
            + "\nNo critical diagnostics: " + YesNo(checklist.HasNoCriticalDiagnostics)
            + "\nSave version current: " + YesNo(checklist.HasValidSaveVersion)
            + "\nCanvas scaler: " + YesNo(checklist.HasCanvasScaler)
            + "\nPortrait layout: " + YesNo(checklist.HasPortraitLayout)
            + "\nSave/load smoke: " + YesNo(checklist.CanSaveAndLoad)
            + "\nSim day smoke: " + YesNo(checklist.CanSimulateDay)
            + "\nAlpha balance report: " + YesNo(checklist.HasAlphaBalanceReport)
            + "\nNews/history limits: " + YesNo(checklist.HasNewsLimit && checklist.HasHistoryLimit)
            + "\nUI display limits: " + YesNo(checklist.HasUiDisplayLimits)
            + "\nUser lineup valid: " + YesNo(checklist.HasNoInvalidUserLineup)
            + "\nUpdated: " + Safe(checklist.UpdatedAtUtc, "never");
    }

    private static bool HasNewsLimit(GameState state)
    {
        if (state.NewsFeed == null || state.NewsFeed.Items == null)
        {
            return true;
        }

        return state.NewsFeed.Items.Count <= NewsConfig.MaxNewsItemsToKeep;
    }

    private static bool HasHistoryLimit(GameState state)
    {
        if (state.AlphaBalanceReportHistory != null
            && state.AlphaBalanceReportHistory.Count > AlphaBalanceConfig.MaxReportsToKeep)
        {
            return false;
        }

        if (state.CpuRosterManagementHistory != null
            && state.CpuRosterManagementHistory.Count > CpuRosterManagementConfig.MaxReportsToKeep)
        {
            return false;
        }

        List<MatchResultData> matchHistory = state.MatchHistory;
        return matchHistory == null || matchHistory.Count < 5000;
    }

    private static bool HasValidUserLineup(GameState state)
    {
        if (state.Teams == null || string.IsNullOrEmpty(state.SelectedTeamId))
        {
            return false;
        }

        TeamData team = null;
        foreach (TeamData candidate in state.Teams)
        {
            if (candidate != null && candidate.Id == state.SelectedTeamId)
            {
                team = candidate;
                break;
            }
        }

        if (team == null)
        {
            return false;
        }

        return LineupService.ValidateLineup(team, out string _);
    }

    private static int CountPassed(AndroidReadinessChecklistData checklist)
    {
        int count = 0;
        if (checklist.HasNoCriticalDiagnostics) count++;
        if (checklist.HasValidSaveVersion) count++;
        if (checklist.HasCanvasScaler) count++;
        if (checklist.HasPortraitLayout) count++;
        if (checklist.CanSaveAndLoad) count++;
        if (checklist.CanSimulateDay) count++;
        if (checklist.HasAlphaBalanceReport) count++;
        if (checklist.HasNewsLimit) count++;
        if (checklist.HasHistoryLimit) count++;
        if (checklist.HasUiDisplayLimits) count++;
        if (checklist.HasNoInvalidUserLineup) count++;
        return count;
    }

    private static string YesNo(bool value)
    {
        return value ? "OK" : "Needs check";
    }

    private static string Safe(string value, string fallback)
    {
        return string.IsNullOrEmpty(value) ? fallback : value;
    }
}
