using System;

[Serializable]
public class LiveMatchPlayerStatData
{
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public string Position;
    public int Goals;
    public int Assists;
    public int Points;
    public int Shots;
    public int PenaltyMinutes;
    public int Saves;
    public int GoalsAgainst;
    public int TimeOnIceSeconds;
    public bool WasInjured;
    public bool IsGoalie;
    public bool StartedGame;
    public bool FinishedGame;
}
