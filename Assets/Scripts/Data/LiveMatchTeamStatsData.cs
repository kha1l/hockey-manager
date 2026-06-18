using System;

[Serializable]
public class LiveMatchTeamStatsData
{
    public string TeamId;
    public string TeamName;
    public int Score;
    public int Shots;
    public int Saves;
    public int PenaltyMinutes;
    public int PowerPlayOpportunities;
    public int PowerPlayGoals;
    public int Hits;
    public int FaceoffWins;
    public bool IsGoaliePulled;
    public string CurrentGoaliePlayerId;
    public string CurrentGoalieName;
    public string TacticName;
    public int PowerPlaySecondsRemaining;
    public int PenaltyKillSecondsRemaining;
    public int ActivePowerPlayPenaltyMinutes;
    public int ActivePenaltyKillPenaltyMinutes;
}
