using System;
using System.Collections.Generic;

[Serializable]
public class LiveMatchStateData
{
    public string LiveMatchId;
    public string ScheduledGameId;
    public bool IsActive;
    public bool IsCompleted;
    public bool IsPlayoffGame;
    public int SeasonStartYear;
    public int SeasonEndYear;
    public int DayIndex;
    public string HomeTeamId;
    public string HomeTeamName;
    public string AwayTeamId;
    public string AwayTeamName;
    public int HomeScore;
    public int AwayScore;
    public int Period;
    public int PeriodSecondsRemaining;
    public int TotalGameSecondsElapsed;
    public bool IsPaused;
    public int SpeedMultiplier;
    public bool IsOvertime;
    public bool IsShootout;
    public bool IsSuddenDeath;
    public bool UserCanSave;
    public string UserTeamId;
    public string UserTeamName;
    public LiveMatchTeamStatsData HomeStats;
    public LiveMatchTeamStatsData AwayStats;
    public List<LiveMatchEventData> Events = new List<LiveMatchEventData>();
    public List<LiveMatchPlayerTokenData> Tokens = new List<LiveMatchPlayerTokenData>();
    public List<LiveMatchPlayerStatData> PlayerStats = new List<LiveMatchPlayerStatData>();
    public string WinnerTeamId;
    public string WinnerTeamName;
    public string CompletionReason;
    public string StartedAtUtc;
    public string CompletedAtUtc;

    public LiveMatchStateData()
    {
        LiveMatchId = Guid.NewGuid().ToString("N");
        EnsureCollections();
    }

    public void EnsureCollections()
    {
        if (Events == null) Events = new List<LiveMatchEventData>();
        if (Tokens == null) Tokens = new List<LiveMatchPlayerTokenData>();
        if (PlayerStats == null) PlayerStats = new List<LiveMatchPlayerStatData>();
        if (HomeStats == null) HomeStats = new LiveMatchTeamStatsData();
        if (AwayStats == null) AwayStats = new LiveMatchTeamStatsData();
    }
}
