using System;
using System.Collections.Generic;

[Serializable]
public class PostGameSummaryData
{
    public string LiveMatchId;
    public string HomeTeamId;
    public string HomeTeamName;
    public string AwayTeamId;
    public string AwayTeamName;
    public int HomeScore;
    public int AwayScore;
    public string WinnerTeamId;
    public string WinnerTeamName;
    public bool WentToOvertime;
    public bool WentToShootout;
    public int HomeShots;
    public int AwayShots;
    public int HomePowerPlayGoals;
    public int HomePowerPlayOpportunities;
    public int AwayPowerPlayGoals;
    public int AwayPowerPlayOpportunities;
    public string FirstStarPlayerName;
    public string SecondStarPlayerName;
    public string ThirdStarPlayerName;
    public List<LiveMatchEventData> ScoringEvents = new List<LiveMatchEventData>();
    public List<LiveMatchEventData> InjuryEvents = new List<LiveMatchEventData>();
    public string Summary;

    public void EnsureCollections()
    {
        if (ScoringEvents == null) ScoringEvents = new List<LiveMatchEventData>();
        if (InjuryEvents == null) InjuryEvents = new List<LiveMatchEventData>();
    }
}
