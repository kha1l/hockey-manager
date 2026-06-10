using System;
using System.Collections.Generic;

[Serializable]
public class SpecialTeamsData
{
    public string TeamId;
    public List<PowerPlayUnitData> PowerPlayUnits = new List<PowerPlayUnitData>();
    public List<PenaltyKillUnitData> PenaltyKillUnits = new List<PenaltyKillUnitData>();
    public bool IsValid;
    public string ValidationMessage;
    public string UpdatedAtUtc;

    public SpecialTeamsData()
    {
        EnsureCollections();
        Touch();
    }

    public void EnsureCollections()
    {
        if (PowerPlayUnits == null)
        {
            PowerPlayUnits = new List<PowerPlayUnitData>();
        }

        if (PenaltyKillUnits == null)
        {
            PenaltyKillUnits = new List<PenaltyKillUnitData>();
        }
    }

    public void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }
}
