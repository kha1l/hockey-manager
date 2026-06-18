using System;
using System.Collections.Generic;

[Serializable]
public class LineChemistryData
{
    public string TeamId;
    public string TeamName;
    public string UnitType;
    public int UnitNumber;
    public string UnitName;
    public List<string> PlayerIds = new List<string>();
    public List<string> PlayerNames = new List<string>();
    public int ChemistryScore;
    public string ChemistryLabel;
    public string ChemistrySummary;
    public int RoleBalanceScore;
    public int MoraleScore;
    public int ConditionScore;
    public int StabilityScore;
    public int PositionFitScore;
    public int SpecialTeamsFitScore;
    public string UpdatedAtUtc;

    public LineChemistryData()
    {
        EnsureCollections();
        UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    public void EnsureCollections()
    {
        if (PlayerIds == null)
        {
            PlayerIds = new List<string>();
        }

        if (PlayerNames == null)
        {
            PlayerNames = new List<string>();
        }
    }
}
