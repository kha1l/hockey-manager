using System;
using System.Collections.Generic;

[Serializable]
public class CpuRosterManagementReportData
{
    public string ReportId = Guid.NewGuid().ToString("N");
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
    public int TeamsChecked;
    public int TeamsChanged;
    public int ActionsCount;
    public List<CpuRosterActionData> Actions = new List<CpuRosterActionData>();

    public CpuRosterManagementReportData()
    {
        if (string.IsNullOrEmpty(ReportId))
        {
            ReportId = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrEmpty(CreatedAtUtc))
        {
            CreatedAtUtc = DateTime.UtcNow.ToString("o");
        }

        EnsureActions();
    }

    public void EnsureActions()
    {
        if (Actions == null)
        {
            Actions = new List<CpuRosterActionData>();
        }

        ActionsCount = Actions.Count;
    }
}
