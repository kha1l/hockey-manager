using System;

[Serializable]
public class ScheduleGameData
{
    public string GameId;
    public int GameNumber;
    public int DayNumber;
    public string HomeTeamId;
    public string AwayTeamId;
    public string HomeTeamName;
    public string AwayTeamName;
    public bool IsPlayed;
    public MatchResultData Result;
}
