using System;
using System.Collections.Generic;

[Serializable]
public class PlayoffData
{
    public bool IsStarted;
    public bool IsCompleted;
    public int CurrentRoundNumber = 1;
    public string ChampionTeamId;
    public string ChampionTeamName;
    public List<PlayoffRoundData> Rounds = new List<PlayoffRoundData>();

    public PlayoffData()
    {
        EnsureRounds();
    }

    public void EnsureRounds()
    {
        if (Rounds == null)
        {
            Rounds = new List<PlayoffRoundData>();
        }
    }
}
