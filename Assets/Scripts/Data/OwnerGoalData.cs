using System;

[Serializable]
public class OwnerGoalData
{
    public string GoalId = Guid.NewGuid().ToString("N");
    public string GoalType;
    public string Title;
    public string Description;
    public string TargetValueLabel;
    public int TargetValue;
    public int CurrentValue;
    public int ProgressPercent;
    public bool IsCompleted;
    public bool IsFailed;
    public int TrustImpactOnSuccess;
    public int TrustImpactOnFailure;
    public string Status = "Active";
    public string ResultSummary;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
    public string UpdatedAtUtc = DateTime.UtcNow.ToString("o");
}
