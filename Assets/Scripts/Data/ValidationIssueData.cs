using System;

[Serializable]
public class ValidationIssueData
{
    public string IssueId = Guid.NewGuid().ToString("N");
    public string Severity;
    public string Category;
    public string Message;
    public string TeamId;
    public string TeamName;
    public string PlayerId;
    public string PlayerName;
    public string SuggestedRepair;
    public bool CanAutoRepair;
    public bool WasRepaired;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
}
