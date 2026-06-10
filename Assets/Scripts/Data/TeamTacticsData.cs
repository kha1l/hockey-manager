using System;

[Serializable]
public class TeamTacticsData
{
    public string TeamId;
    public string PresetName;
    public int OffensiveFocus;
    public int DefensiveFocus;
    public int Aggressiveness;
    public int Tempo;
    public int ShootingFrequency;
    public int RiskLevel;
    public string UpdatedAtUtc;

    public void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }
}
