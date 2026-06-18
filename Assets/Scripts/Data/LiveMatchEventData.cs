using System;

[Serializable]
public class LiveMatchEventData
{
    public string EventId;
    public string EventType;
    public int Period;
    public string PeriodLabel;
    public int GameSecondsElapsed;
    public int PeriodSecondsRemaining;
    public string ClockLabel;
    public string TeamId;
    public string TeamName;
    public string PlayerId;
    public string PlayerName;
    public string Assist1PlayerId;
    public string Assist1PlayerName;
    public string Assist2PlayerId;
    public string Assist2PlayerName;
    public string GoaliePlayerId;
    public string GoaliePlayerName;
    public string Description;
    public int HomeScoreAfter;
    public int AwayScoreAfter;
    public int Importance;

    public LiveMatchEventData()
    {
        EventId = Guid.NewGuid().ToString("N");
    }
}
