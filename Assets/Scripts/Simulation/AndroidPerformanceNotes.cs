public static class AndroidPerformanceNotes
{
    public static string GetUiPerformanceNotes(AndroidPerformanceData data)
    {
        if (data == null)
        {
            return "UI: no panel timing yet.";
        }

        if (data.MaxObservedPanelRefreshMs > 250)
        {
            return "UI: heavy panel refresh detected; keep row limits and prefer filters.";
        }

        return "UI: panel refresh timings look acceptable for alpha.";
    }

    public static string GetSimulationPerformanceNotes(AndroidPerformanceData data)
    {
        if (data == null)
        {
            return "Simulation: no timing yet.";
        }

        if (data.LastSimulateDayMs > 1500)
        {
            return "Simulation: one day is slow; avoid extra refreshes during long runs.";
        }

        return "Simulation: current day timing is acceptable for alpha.";
    }

    public static string GetSaveLoadPerformanceNotes(AndroidPerformanceData data)
    {
        if (data == null)
        {
            return "Save/load: no timing yet.";
        }

        if (data.MaxObservedSaveMs > 1000 || data.MaxObservedLoadMs > 1000)
        {
            return "Save/load: timing is high; keep histories trimmed before alpha.";
        }

        return "Save/load: timings look acceptable for alpha.";
    }
}
