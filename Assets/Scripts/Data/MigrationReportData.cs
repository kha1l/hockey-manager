using System;
using System.Collections.Generic;

[Serializable]
public class MigrationReportData
{
    public string ReportId = Guid.NewGuid().ToString("N");
    public int FromSaveVersion;
    public int ToSaveVersion;
    public string Status = SaveMigrationConfig.MigrationStatusNotNeeded;
    public int WarningsCount;
    public int RepairsCount;
    public int ErrorsCount;
    public List<string> Warnings = new List<string>();
    public List<string> Repairs = new List<string>();
    public List<string> Errors = new List<string>();
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");

    public MigrationReportData()
    {
        EnsureCollections();
    }

    public void EnsureCollections()
    {
        if (Warnings == null)
        {
            Warnings = new List<string>();
        }

        if (Repairs == null)
        {
            Repairs = new List<string>();
        }

        if (Errors == null)
        {
            Errors = new List<string>();
        }

        WarningsCount = Warnings.Count;
        RepairsCount = Repairs.Count;
        ErrorsCount = Errors.Count;
    }
}
