using System;
using System.Collections.Generic;

[Serializable]
public class SeasonHistoryData
{
    public int SeasonStartYear;
    public int SeasonEndYear;
    public string RulesetName;
    public string CbaName;
    public string ChampionTeamId;
    public string ChampionTeamName;
    public string UserTeamId;
    public string UserTeamName;
    public int UserTeamPoints;
    public int UserTeamRank;
    public bool UserTeamMadePlayoffs;
    public string ArchivedAtUtc;
    public List<TeamSeasonHistoryData> TeamStandings = new List<TeamSeasonHistoryData>();
    public List<PlayerSeasonHistoryData> PlayerStats = new List<PlayerSeasonHistoryData>();
    public List<DraftPickData> DraftPicks = new List<DraftPickData>();

    public SeasonHistoryData()
    {
        EnsureCollections();
    }

    public void EnsureCollections()
    {
        if (TeamStandings == null)
        {
            TeamStandings = new List<TeamSeasonHistoryData>();
        }

        if (PlayerStats == null)
        {
            PlayerStats = new List<PlayerSeasonHistoryData>();
        }

        if (DraftPicks == null)
        {
            DraftPicks = new List<DraftPickData>();
        }
    }
}
