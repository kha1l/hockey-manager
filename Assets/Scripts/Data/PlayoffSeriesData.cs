using System;
using System.Collections.Generic;

[Serializable]
public class PlayoffSeriesData
{
    public string SeriesId;
    public int RoundNumber;
    public string RoundName;
    public string Conference;
    public string TeamAId;
    public string TeamAName;
    public string TeamBId;
    public string TeamBName;
    public int TeamAWins;
    public int TeamBWins;
    public string WinnerTeamId;
    public string WinnerTeamName;
    public bool IsCompleted;
    public List<MatchResultData> Games = new List<MatchResultData>();

    public PlayoffSeriesData()
    {
        EnsureGames();
    }

    public void EnsureGames()
    {
        if (Games == null)
        {
            Games = new List<MatchResultData>();
        }
    }
}
