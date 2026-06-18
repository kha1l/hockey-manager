using System;
using System.Collections.Generic;

[Serializable]
public class GmCareerData
{
    public string GmId = Guid.NewGuid().ToString("N");
    public string GmName = "User GM";
    public string CurrentTeamId;
    public string CurrentTeamName;
    public int CareerStartYear;
    public int CurrentSeasonStartYear;
    public int SeasonsCompleted;
    public int TeamsManaged;
    public int CareerWins;
    public int CareerLosses;
    public int CareerOvertimeLosses;
    public int CareerPlayoffAppearances;
    public int CareerPlayoffRoundsWon;
    public int CareerChampionships;
    public int CareerAwards;
    public int CurrentJobSecurity;
    public int CurrentOwnerTrust;
    public bool IsFired;
    public bool IsUnemployed;
    public string FiredFromTeamId;
    public string FiredFromTeamName;
    public string FiredAtUtc;
    public string LastCareerEventSummary;
    public string CareerStatus = "Employed";
    public string UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    public int LastCareerSeasonUpdatedStartYear;
    public int LastJobSecurityEvaluationSeasonStartYear;
    public List<string> ManagedTeamIds = new List<string>();
    public List<string> ManagedTeamNames = new List<string>();

    public GmCareerData()
    {
        EnsureCollections();
    }

    public void EnsureCollections()
    {
        if (ManagedTeamIds == null)
        {
            ManagedTeamIds = new List<string>();
        }

        if (ManagedTeamNames == null)
        {
            ManagedTeamNames = new List<string>();
        }
    }
}
