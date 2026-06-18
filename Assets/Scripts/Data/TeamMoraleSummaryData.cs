using System;

[Serializable]
public class TeamMoraleSummaryData
{
    public string TeamId;
    public string TeamName;
    public int AverageMorale;
    public int HappyPlayers;
    public int ContentPlayers;
    public int ConcernedPlayers;
    public int UnhappyPlayers;
    public int VeryUnhappyPlayers;
    public int TradeRequests;
    public string LowestMoralePlayerId;
    public string LowestMoralePlayerName;
    public int LowestMorale;
    public string Summary;
    public string UpdatedAtUtc;

    public TeamMoraleSummaryData()
    {
        UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }
}
