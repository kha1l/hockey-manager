using System;

[Serializable]
public class UserTeamSeasonHistoryData
{
    public int SeasonStartYear;
    public int SeasonEndYear;
    public string TeamId;
    public string TeamName;
    public int Wins;
    public int Losses;
    public int OvertimeLosses;
    public int Points;
    public int LeagueRank;
    public bool MadePlayoffs;
    public int PlayoffRoundsWon;
    public string PlayoffResult;
    public string OwnerEvaluationSummary;
    public int GmTrustAfterSeason;
    public string JobSecurityAfterSeason;
    public string Summary;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
}
