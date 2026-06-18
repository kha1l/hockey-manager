using System;

[Serializable]
public class PlayerDevelopmentChangeData
{
    public string ChangeId;
    public int SeasonStartYear;
    public int SeasonEndYear;
    public string EntityType;
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public string Position;
    public int Age;
    public int OldOverall;
    public int NewOverall;
    public int OverallDelta;
    public int OldPotential;
    public int NewPotential;
    public int PotentialDelta;
    public string DevelopmentType;
    public string DevelopmentEvent;
    public int HiddenCeilingAtTime;
    public int HiddenFloorAtTime;
    public int DevelopmentRiskAtTime;
    public int MoraleAtTime;
    public int MoraleDevelopmentModifier;
    public int LeadershipSupportAtTime;
    public int LeadershipDevelopmentModifier;
    public int StaffDevelopmentModifier;
    public string StaffDevelopmentSummary;
    public string Reason;
    public string CreatedAtUtc;
}
