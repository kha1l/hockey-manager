using System;

[Serializable]
public class LiveMatchPlayerTokenData
{
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public string Position;
    public int JerseyNumber;
    public bool IsGoalie;
    public bool IsPulledGoalieReplacement;
    public bool IsHomeTeam;
    public float NormalizedX;
    public float NormalizedY;
    public string JerseyResourcePath;
    public string FullBodyResourcePath;
    public bool IsOnIce;
    public bool IsInjured;
    public int Condition;
    public int Morale;
}
