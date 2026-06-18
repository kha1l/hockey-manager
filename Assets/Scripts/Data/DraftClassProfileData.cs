using System;

[Serializable]
public class DraftClassProfileData
{
    public string ProfileId;
    public int DraftYear;
    public string StrengthType;
    public string DepthType;
    public string PositionalTheme;
    public string Summary;
    public int OverallQualityModifier;
    public int PotentialQualityModifier;
    public int TopProspectBonus;
    public int DepthBonus;
    public int ForwardQualityModifier;
    public int DefenseQualityModifier;
    public int GoalieQualityModifier;
    public int ExpectedEliteProspects;
    public int ExpectedFirstRoundTalent;
    public int ExpectedNhlDepthPlayers;
    public string GeneratedAtUtc;

    public DraftClassProfileData()
    {
        ProfileId = Guid.NewGuid().ToString("N");
        GeneratedAtUtc = DateTime.UtcNow.ToString("o");
    }
}
