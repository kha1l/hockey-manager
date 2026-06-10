using System;

[Serializable]
public class ProspectData
{
    public string Id;
    public string FirstName;
    public string LastName;
    public string Position;
    public string Nationality;
    public int Age;
    public int Overall;
    public int Potential;
    public int ProjectedRound;
    public int ProjectedPick;
    public bool IsDrafted;
    public string DraftedByTeamId;
    public string DraftedByTeamName;
    public int DraftRound;
    public int DraftPickOverall;
    public int LastSeasonOverall;
    public int LastSeasonPotential;
    public int LastDevelopmentDelta;
    public string LastDevelopmentType;
}
