using System;

[Serializable]
public class PlayerGameStatData
{
    public string PlayerId;
    public string TeamId;
    public string PlayerName;
    public string Position;
    public bool IsGoalie;
    public int Goals;
    public int Assists;
    public int Points;
    public int PowerPlayGoals;
    public int PowerPlayAssists;
    public int PowerPlayPoints;
    public int ShortHandedGoals;
    public int ShortHandedAssists;
    public int ShortHandedPoints;
    public int Shots;
    public int PenaltyMinutes;
    public int PlusMinus;
    public int Saves;
    public int ShotsAgainst;
    public int GoalsAgainst;
    public int TimeOnIceSeconds;
    public bool GoalieWin;
    public bool GoalieLoss;
    public bool GoalieOvertimeLoss;
    public bool Shutout;
}
