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
    public string Reason;
    public string CreatedAtUtc;
}
