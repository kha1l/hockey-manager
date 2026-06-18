using System;
using System.Collections.Generic;

[Serializable]
public class TeamLeadershipData
{
    public string TeamId;
    public string TeamName;
    public string CaptainPlayerId;
    public string CaptainName;
    public string Alternate1PlayerId;
    public string Alternate1Name;
    public string Alternate2PlayerId;
    public string Alternate2Name;
    public int LeadershipScore;
    public int LockerRoomImpact;
    public int MoraleImpact;
    public int ChemistryImpact;
    public string LeadershipLabel;
    public string LeadershipSummary;
    public List<LeadershipCandidateData> Candidates = new List<LeadershipCandidateData>();
    public string UpdatedAtUtc;

    public TeamLeadershipData()
    {
        EnsureCandidates();
        UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    public void EnsureCandidates()
    {
        if (Candidates == null)
        {
            Candidates = new List<LeadershipCandidateData>();
        }
    }
}
