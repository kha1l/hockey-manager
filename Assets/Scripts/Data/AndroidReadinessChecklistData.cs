using System;

[Serializable]
public class AndroidReadinessChecklistData
{
    public bool HasNoCriticalDiagnostics;
    public bool HasValidSaveVersion;
    public bool HasCanvasScaler;
    public bool HasPortraitLayout;
    public bool CanSaveAndLoad;
    public bool CanSimulateDay;
    public bool HasAlphaBalanceReport;
    public bool HasNewsLimit;
    public bool HasHistoryLimit;
    public bool HasUiDisplayLimits;
    public bool HasNoInvalidUserLineup;
    public int PassedCount;
    public int TotalCount;
    public string Summary;
    public string UpdatedAtUtc;
}
