using System;

[Serializable]
public class ScoutingReportData
{
    public string ReportId;
    public string ProspectId;
    public string ProspectName;
    public string Position;
    public int Age;
    public int AccuracyBefore;
    public int AccuracyAfter;
    public int EstimatedOverallMin;
    public int EstimatedOverallMax;
    public int EstimatedPotentialMin;
    public int EstimatedPotentialMax;
    public string ScoutingGrade;
    public string ProjectedRole;
    public string RiskLevel;
    public string DraftProjection;
    public string ProspectArchetype;
    public string CeilingHint;
    public string FloorHint;
    public string DevelopmentTypeHint;
    public string RiskHint;
    public string Strengths;
    public string Weaknesses;
    public string Summary;
    public string CreatedAtUtc;
    public string Source;

    public ScoutingReportData()
    {
        ReportId = Guid.NewGuid().ToString("N");
        CreatedAtUtc = DateTime.UtcNow.ToString("o");
    }
}
