using System;
using System.Collections.Generic;

[Serializable]
public class TeamData
{
    public string Id;
    public string Name;
    public string City;
    public string Abbreviation;
    public List<PlayerData> Players = new List<PlayerData>();
    public List<ProspectData> DraftRights = new List<ProspectData>();
    public TeamLineupData Lineup;
    public SpecialTeamsData SpecialTeams;
    public TeamTacticsData Tactics;

    public TeamData()
    {
        EnsurePlayers();
        EnsureDraftRights();
        EnsureLineupData();
        EnsureSpecialTeamsData();
        EnsureTacticsData();
    }

    public void EnsurePlayers()
    {
        if (Players == null)
        {
            Players = new List<PlayerData>();
        }
    }

    public void EnsureDraftRights()
    {
        if (DraftRights == null)
        {
            DraftRights = new List<ProspectData>();
        }
    }

    public void EnsureLineupData()
    {
        if (Lineup != null)
        {
            Lineup.EnsureCollections();
        }
    }

    public void EnsureSpecialTeamsData()
    {
        if (SpecialTeams != null)
        {
            SpecialTeams.EnsureCollections();
        }
    }

    public void EnsureTacticsData()
    {
        if (Tactics != null && string.IsNullOrEmpty(Tactics.PresetName))
        {
            Tactics.PresetName = "Balanced";
        }
    }
}
