using System;
using System.Collections.Generic;

[Serializable]
public class ScoutingActionResultData
{
    public bool Success;
    public string Message;
    public string ActionType;
    public int ProspectsScouted;
    public string CreatedAtUtc;
    public List<ScoutingReportData> CreatedReports = new List<ScoutingReportData>();

    public ScoutingActionResultData()
    {
        CreatedAtUtc = DateTime.UtcNow.ToString("o");
        EnsureReports();
    }

    public void EnsureReports()
    {
        if (CreatedReports == null)
        {
            CreatedReports = new List<ScoutingReportData>();
        }
    }
}
