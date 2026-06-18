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
    public string ProjectedRound;
    public int ProjectedRoundNumber;
    public int ProjectedPick;
    public int DraftRank;
    public string ProspectArchetype;
    public string DraftClassStrengthType;
    public string DraftClassDepthType;
    public string DraftClassPositionalTheme;
    public int ClassAdjustedOverall;
    public int ClassAdjustedPotential;
    public bool IsDrafted;
    public string DraftedByTeamId;
    public string DraftedByTeamName;
    public int DraftRound;
    public int DraftPickOverall;
    public int LastSeasonOverall;
    public int LastSeasonPotential;
    public int LastDevelopmentDelta;
    public string LastDevelopmentType;
    public int ScoutingAccuracy;
    public int EstimatedOverallMin;
    public int EstimatedOverallMax;
    public int EstimatedPotentialMin;
    public int EstimatedPotentialMax;
    public string ScoutingGrade;
    public string ProjectedRole;
    public string RiskLevel;
    public string DraftProjection;
    public string LastScoutedAtUtc;
    public int TimesScouted;
    public bool IsFullyScouted;
    public bool IsUserPinned;
    public string ScoutingSummary;
    public int HiddenCeiling;
    public int HiddenFloor;
    public int DevelopmentRisk;
    public int BoomChance;
    public int BustChance;
    public string DevelopmentType;
    public string CeilingHint;
    public string FloorHint;
    public string DevelopmentTypeHint;
    public string RiskHint;
    public bool HasGeneratedRiskProfile;
}
