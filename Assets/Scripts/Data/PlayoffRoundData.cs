using System;
using System.Collections.Generic;

[Serializable]
public class PlayoffRoundData
{
    public int RoundNumber;
    public string RoundName;
    public bool IsCompleted;
    public List<PlayoffSeriesData> Series = new List<PlayoffSeriesData>();

    public PlayoffRoundData()
    {
        EnsureSeries();
    }

    public void EnsureSeries()
    {
        if (Series == null)
        {
            Series = new List<PlayoffSeriesData>();
        }
    }
}
