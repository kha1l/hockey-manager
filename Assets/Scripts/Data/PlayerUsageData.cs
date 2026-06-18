using System;

[Serializable]
public class PlayerUsageData
{
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public string Position;
    public string PlayerRole;
    public string UsageCategory;
    public int EstimatedTimeOnIceSeconds;
    public int EffectiveOverall;
    public int Condition;
    public int Fatigue;
    public int Morale;
    public int RoleSatisfaction;
    public int IceTimeSatisfaction;
    public string MoraleStatus;
    public bool WantsTrade;
    public bool IsInjured;
    public bool IsActive;
    public bool IsOnPowerPlay;
    public bool IsOnPenaltyKill;
}
