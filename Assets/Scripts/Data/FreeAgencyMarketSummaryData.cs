using System;

[Serializable]
public class FreeAgencyMarketSummaryData
{
    public int TotalFreeAgents;
    public int TopPlayersAvailable;
    public int GoaliesAvailable;
    public int DefensemenAvailable;
    public int ForwardsAvailable;
    public int UserTeamCapSpace;
    public int UserTeamNhlRosterSpots;
    public string BestAvailablePlayerId;
    public string BestAvailablePlayerName;
    public string Summary;
    public string UpdatedAtUtc = DateTime.UtcNow.ToString("o");
}
