using System;
using System.Collections.Generic;

[Serializable]
public class GameStateValidationReportData
{
    public string ReportId = Guid.NewGuid().ToString("N");
    public int SaveVersion;
    public int TeamsCount;
    public int PlayersCount;
    public int FreeAgentsCount;
    public int ProspectsCount;
    public int IssuesCount;
    public int WarningsCount;
    public int ErrorsCount;
    public int CriticalCount;
    public int AutoRepairableCount;
    public int RepairedCount;
    public List<ValidationIssueData> Issues = new List<ValidationIssueData>();
    public string Summary;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");

    public GameStateValidationReportData()
    {
        EnsureIssues();
    }

    public void EnsureIssues()
    {
        if (Issues == null)
        {
            Issues = new List<ValidationIssueData>();
        }
    }
}
