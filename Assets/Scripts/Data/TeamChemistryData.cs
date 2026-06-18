using System;
using System.Collections.Generic;

[Serializable]
public class TeamChemistryData
{
    public string TeamId;
    public string TeamName;
    public int TeamChemistryScore;
    public string TeamChemistryLabel;
    public string TeamChemistrySummary;
    public int ForwardChemistryAverage;
    public int DefenseChemistryAverage;
    public int SpecialTeamsChemistryAverage;
    public int MoraleChemistryImpact;
    public int BestUnitScore;
    public string BestUnitName;
    public int WorstUnitScore;
    public string WorstUnitName;
    public List<LineChemistryData> ForwardLines = new List<LineChemistryData>();
    public List<LineChemistryData> DefensePairs = new List<LineChemistryData>();
    public List<LineChemistryData> PowerPlayUnits = new List<LineChemistryData>();
    public List<LineChemistryData> PenaltyKillUnits = new List<LineChemistryData>();
    public string UpdatedAtUtc;

    public TeamChemistryData()
    {
        EnsureCollections();
        UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    public void EnsureCollections()
    {
        if (ForwardLines == null)
        {
            ForwardLines = new List<LineChemistryData>();
        }

        if (DefensePairs == null)
        {
            DefensePairs = new List<LineChemistryData>();
        }

        if (PowerPlayUnits == null)
        {
            PowerPlayUnits = new List<LineChemistryData>();
        }

        if (PenaltyKillUnits == null)
        {
            PenaltyKillUnits = new List<LineChemistryData>();
        }
    }
}
