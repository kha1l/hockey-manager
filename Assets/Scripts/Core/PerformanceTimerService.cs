using System;

public static class PerformanceTimerService
{
    public static long MeasureMs(Action action)
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        action?.Invoke();
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    public static T MeasureMs<T>(Func<T> func, out long elapsedMs)
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        T result = func == null ? default(T) : func();
        stopwatch.Stop();
        elapsedMs = stopwatch.ElapsedMilliseconds;
        return result;
    }

    public static void EnsurePerformanceData(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureAndroidPerformanceData();
    }

    public static void RecordSave(GameState state, long elapsedMs)
    {
        AndroidPerformanceData data = GetData(state);
        if (data == null)
        {
            return;
        }

        data.LastSaveMs = ClampMs(elapsedMs);
        data.MaxObservedSaveMs = Math.Max(data.MaxObservedSaveMs, data.LastSaveMs);
        Touch(data);
    }

    public static void RecordLoad(GameState state, long elapsedMs)
    {
        AndroidPerformanceData data = GetData(state);
        if (data == null)
        {
            return;
        }

        data.LastLoadMs = ClampMs(elapsedMs);
        data.MaxObservedLoadMs = Math.Max(data.MaxObservedLoadMs, data.LastLoadMs);
        Touch(data);
    }

    public static void RecordSimulateDay(GameState state, long elapsedMs)
    {
        AndroidPerformanceData data = GetData(state);
        if (data == null)
        {
            return;
        }

        data.LastSimulateDayMs = ClampMs(elapsedMs);
        data.MaxObservedSimulateDayMs = Math.Max(data.MaxObservedSimulateDayMs, data.LastSimulateDayMs);
        Touch(data);
    }

    public static void RecordSimulateSeason(GameState state, long elapsedMs)
    {
        AndroidPerformanceData data = GetData(state);
        if (data == null)
        {
            return;
        }

        data.LastSimulateSeasonMs = ClampMs(elapsedMs);
        Touch(data);
    }

    public static void RecordPanelRefresh(GameState state, string panelName, long elapsedMs)
    {
        AndroidPerformanceData data = GetData(state);
        if (data == null)
        {
            return;
        }

        data.LastPanelRefreshMs = ClampMs(elapsedMs);
        data.LastRefreshedPanel = string.IsNullOrEmpty(panelName) ? "Unknown" : panelName;
        data.MaxObservedPanelRefreshMs = Math.Max(data.MaxObservedPanelRefreshMs, data.LastPanelRefreshMs);
        Touch(data);
    }

    public static void RecordDiagnostics(GameState state, long elapsedMs)
    {
        AndroidPerformanceData data = GetData(state);
        if (data == null)
        {
            return;
        }

        data.LastDiagnosticsMs = ClampMs(elapsedMs);
        Touch(data);
    }

    public static void RecordAlphaReport(GameState state, long elapsedMs)
    {
        AndroidPerformanceData data = GetData(state);
        if (data == null)
        {
            return;
        }

        data.LastAlphaReportMs = ClampMs(elapsedMs);
        Touch(data);
    }

    public static string BuildPerformanceSummary(GameState state)
    {
        AndroidPerformanceData data = state == null ? null : state.AndroidPerformance;
        if (data == null)
        {
            return "Android performance: no data yet";
        }

        return "Android performance"
            + "\nSave/load: " + data.LastSaveMs + " ms / " + data.LastLoadMs + " ms"
            + "\nSim day/season: " + data.LastSimulateDayMs + " ms / " + data.LastSimulateSeasonMs + " ms"
            + "\nPanel: " + Safe(data.LastRefreshedPanel, "none") + " " + data.LastPanelRefreshMs + " ms"
            + "\nDiagnostics/alpha: " + data.LastDiagnosticsMs + " ms / " + data.LastAlphaReportMs + " ms";
    }

    private static AndroidPerformanceData GetData(GameState state)
    {
        EnsurePerformanceData(state);
        return state == null ? null : state.AndroidPerformance;
    }

    private static int ClampMs(long elapsedMs)
    {
        if (elapsedMs < 0)
        {
            return 0;
        }

        return elapsedMs > int.MaxValue ? int.MaxValue : (int)elapsedMs;
    }

    private static void Touch(AndroidPerformanceData data)
    {
        data.LastUpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    private static string Safe(string value, string fallback)
    {
        return string.IsNullOrEmpty(value) ? fallback : value;
    }
}
