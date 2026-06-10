using System;
using System.Collections.Generic;

[Serializable]
public class MatchResultData
{
    public string MatchId;
    public string HomeTeamId;
    public string AwayTeamId;
    public string HomeTeamName;
    public string AwayTeamName;
    public int HomeScore;
    public int AwayScore;
    public int HomeShots;
    public int AwayShots;
    public int HomePowerPlayOpportunities;
    public int AwayPowerPlayOpportunities;
    public int HomePowerPlayGoals;
    public int AwayPowerPlayGoals;
    public int HomePenaltyMinutes;
    public int AwayPenaltyMinutes;
    public string WinnerTeamId;
    public bool IsOvertime;
    public string PlayedAtUtc;
    public string Summary;
    public List<PlayerGameStatData> PlayerStats = new List<PlayerGameStatData>();

    public MatchResultData()
    {
        EnsurePlayerStats();
    }

    public void EnsurePlayerStats()
    {
        if (PlayerStats == null)
        {
            PlayerStats = new List<PlayerGameStatData>();
        }
    }
}
