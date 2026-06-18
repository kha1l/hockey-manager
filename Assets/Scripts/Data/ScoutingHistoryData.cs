using System;
using System.Collections.Generic;

[Serializable]
public class ScoutingHistoryData
{
    public List<ScoutingReportData> Reports = new List<ScoutingReportData>();
    public int TotalScoutingActions;
    public string LastScoutingActionAtUtc;

    public ScoutingHistoryData()
    {
        EnsureReports();
    }

    public void EnsureReports()
    {
        if (Reports == null)
        {
            Reports = new List<ScoutingReportData>();
        }

        if (LastScoutingActionAtUtc == null)
        {
            LastScoutingActionAtUtc = "";
        }
    }
}
