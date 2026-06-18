using System;

[Serializable]
public class ProspectRiskProfileData
{
    public string ProspectId;
    public string ProspectName;
    public string Position;
    public int Age;
    public int HiddenCeiling;
    public int HiddenFloor;
    public int DevelopmentRisk;
    public int BoomChance;
    public int BustChance;
    public string DevelopmentType;
    public string CeilingHint;
    public string FloorHint;
    public string RiskHint;
    public string DevelopmentTypeHint;
    public string GeneratedAtUtc;

    public ProspectRiskProfileData()
    {
        GeneratedAtUtc = DateTime.UtcNow.ToString("o");
    }
}
