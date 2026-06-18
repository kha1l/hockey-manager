using System;

[Serializable]
public class PlayerMoraleSnapshotData
{
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public string Position;
    public int Age;
    public int Overall;
    public int Potential;
    public string RosterStatus;
    public string PlayerRole;
    public string UsageCategory;
    public string ExpectedRole;
    public string ExpectedUsageCategory;
    public int EstimatedTimeOnIceSeconds;
    public int AverageTimeOnIceSeconds;
    public int ExpectedTimeOnIceSeconds;
    public string CaptaincyRole;
    public int Leadership;
    public int Morale;
    public int RoleSatisfaction;
    public int IceTimeSatisfaction;
    public int TeamPerformanceSatisfaction;
    public int ContractSatisfaction;
    public int RosterStatusSatisfaction;
    public int OverallSatisfaction;
    public bool WantsTrade;
    public string MoraleStatus;
    public string MoraleTrend;
    public string MoraleSummary;
    public string UpdatedAtUtc;

    public PlayerMoraleSnapshotData()
    {
        UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }
}
