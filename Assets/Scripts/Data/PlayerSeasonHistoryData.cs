using System;

[Serializable]
public class PlayerSeasonHistoryData
{
    public string PlayerId;
    public string TeamId;
    public string PlayerName;
    public string Position;
    public bool IsGoalie;
    public int GamesPlayed;
    public int Goals;
    public int Assists;
    public int Points;
    public int Shots;
    public int PlusMinus;
    public int GoalieGamesPlayed;
    public int GoalieWins;
    public int GoalieLosses;
    public int GoalieOvertimeLosses;
    public int Saves;
    public int ShotsAgainst;
    public int GoalsAgainst;
    public int Shutouts;
}
