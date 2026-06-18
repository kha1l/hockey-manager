using System;
using System.Collections.Generic;

[Serializable]
public class AlphaBalanceReportData
{
    public string ReportId = Guid.NewGuid().ToString("N");
    public int SeasonsRequested;
    public int SeasonsCompleted;
    public bool SimulatedDuringReport;
    public string StartedAtUtc = DateTime.UtcNow.ToString("o");
    public string CompletedAtUtc;
    public int MetricsCount;
    public int PassedCount;
    public int WarningCount;
    public int CriticalCount;
    public List<AlphaBalanceSeasonSnapshotData> SeasonSnapshots = new List<AlphaBalanceSeasonSnapshotData>();
    public List<AlphaBalanceMetricData> Metrics = new List<AlphaBalanceMetricData>();
    public string Summary;
    public string Recommendation;

    public AlphaBalanceReportData()
    {
        EnsureCollections();
    }

    public void EnsureCollections()
    {
        if (SeasonSnapshots == null)
        {
            SeasonSnapshots = new List<AlphaBalanceSeasonSnapshotData>();
        }

        if (Metrics == null)
        {
            Metrics = new List<AlphaBalanceMetricData>();
        }
    }
}
