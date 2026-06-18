using System;

[Serializable]
public class AndroidPerformanceData
{
    public int LastSaveMs;
    public int LastLoadMs;
    public int LastSimulateDayMs;
    public int LastSimulateSeasonMs;
    public int LastPanelRefreshMs;
    public string LastRefreshedPanel;
    public int LastDiagnosticsMs;
    public int LastAlphaReportMs;
    public int MaxObservedSaveMs;
    public int MaxObservedLoadMs;
    public int MaxObservedSimulateDayMs;
    public int MaxObservedPanelRefreshMs;
    public string LastUpdatedAtUtc;
}
