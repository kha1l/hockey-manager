using System;

[Serializable]
public class LeagueSeasonHistoryData
{
    public string HistoryId = Guid.NewGuid().ToString("N");
    public int SeasonStartYear;
    public int SeasonEndYear;
    public string ChampionTeamId;
    public string ChampionTeamName;
    public string FinalistTeamId;
    public string FinalistTeamName;
    public string BestRegularSeasonTeamId;
    public string BestRegularSeasonTeamName;
    public int BestRegularSeasonPoints;
    public string TopScorerPlayerId;
    public string TopScorerPlayerName;
    public string TopScorerTeamName;
    public int TopScorerPoints;
    public string MvpPlayerId;
    public string MvpPlayerName;
    public string BestGoaliePlayerId;
    public string BestGoaliePlayerName;
    public string UserTeamId;
    public string UserTeamName;
    public int UserTeamPoints;
    public bool UserTeamMadePlayoffs;
    public int UserTeamPlayoffRoundsWon;
    public string UserTeamResult;
    public SeasonAwardsData Awards = new SeasonAwardsData();
    public string Summary;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");

    public void EnsureAwards()
    {
        if (Awards == null)
        {
            Awards = new SeasonAwardsData();
        }

        Awards.EnsureAwards();
    }
}
