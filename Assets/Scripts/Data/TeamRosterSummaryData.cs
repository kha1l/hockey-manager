using System;

[Serializable]
public class TeamRosterSummaryData
{
    public string TeamId;
    public string TeamName;
    public int TotalPlayers;
    public int NhlPlayers;
    public int FarmPlayers;
    public int ReservePlayers;
    public int InjuredPlayers;
    public int AvailableNhlPlayers;
    public int NhlForwards;
    public int NhlDefensemen;
    public int NhlGoalies;
    public bool IsNhlRosterValid;
    public string ValidationMessage;
    public string UpdatedAtUtc;
}
