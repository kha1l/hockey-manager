using System;
using System.Collections.Generic;

[Serializable]
public class TeamData
{
    public string Id;
    public string Name;
    public string City;
    public string Abbreviation;
    public TeamIdentityData Identity;
    public string ConferenceName;
    public string DivisionName;
    public string PrimaryColorHex;
    public string SecondaryColorHex;
    public string TertiaryColorHex;
    public List<PlayerData> Players = new List<PlayerData>();
    public List<ProspectData> DraftRights = new List<ProspectData>();
    public TeamLineupData Lineup;
    public SpecialTeamsData SpecialTeams;
    public TeamTacticsData Tactics;
    public TeamChemistryData Chemistry;
    public TeamLeadershipData LeadershipData;
    public TeamStaffData Staff;
    public OwnerProfileData OwnerProfile;
    public TeamRetiredNumbersData RetiredNumbersData;

    public TeamData()
    {
        EnsurePlayers();
        EnsureDraftRights();
        EnsureLineupData();
        EnsureSpecialTeamsData();
        EnsureTacticsData();
        EnsureRetiredNumbersData();
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

    public void EnsureRetiredNumbersData()
    {
        if (RetiredNumbersData == null)
        {
            RetiredNumbersData = new TeamRetiredNumbersData
            {
                TeamId = Id,
                TeamName = TeamIdentityService.GetDisplayName(this)
            };
        }

        RetiredNumbersData.EnsureRetiredNumbers();
        if (string.IsNullOrEmpty(RetiredNumbersData.TeamId))
        {
            RetiredNumbersData.TeamId = Id;
        }

        if (string.IsNullOrEmpty(RetiredNumbersData.TeamName))
        {
            RetiredNumbersData.TeamName = TeamIdentityService.GetDisplayName(this);
        }
    }
}
