using System;
using System.Collections.Generic;

[Serializable]
public class SeasonData
{
    public int SeasonYear;
    public int ScheduleVersion;
    public int TargetGamesPerTeam;
    public int CurrentDay;
    public int CurrentGameIndex;
    public List<ScheduleGameData> Schedule = new List<ScheduleGameData>();
    public List<TeamStandingData> Standings = new List<TeamStandingData>();
    public List<PlayerSeasonStatsData> PlayerStats = new List<PlayerSeasonStatsData>();
    public PlayoffData Playoffs;
    public bool IsSeasonFinished;

    public SeasonData()
    {
        EnsureCollections();
    }

    public void EnsureCollections()
    {
        if (Schedule == null)
        {
            Schedule = new List<ScheduleGameData>();
        }

        if (Standings == null)
        {
            Standings = new List<TeamStandingData>();
        }

        if (PlayerStats == null)
        {
            PlayerStats = new List<PlayerSeasonStatsData>();
        }

        if (Playoffs != null)
        {
            Playoffs.EnsureRounds();

            foreach (PlayoffRoundData round in Playoffs.Rounds)
            {
                if (round == null)
                {
                    continue;
                }

                round.EnsureSeries();

                foreach (PlayoffSeriesData series in round.Series)
                {
                    if (series != null)
                    {
                        series.EnsureGames();
                    }
                }
            }
        }
    }
}
