using System;
using System.Collections.Generic;

[Serializable]
public class TeamLineupData
{
    public string TeamId;
    public List<ForwardLineData> ForwardLines = new List<ForwardLineData>();
    public List<DefensePairData> DefensePairs = new List<DefensePairData>();
    public GoalieLineupData Goalies = new GoalieLineupData();
    public List<string> ScratchPlayerIds = new List<string>();
    public bool IsValid;
    public string ValidationMessage;
    public string UpdatedAtUtc;
    public bool IsManual;
    public string LastManualUpdateUtc;
    public string LastSelectedSlotType;
    public int LastSelectedLineOrPairNumber;
    public string LastSelectedSlotPosition;
    public int TeamChemistryScore;
    public string TeamChemistryLabel;
    public string LastChemistryUpdateUtc;

    public TeamLineupData()
    {
        EnsureCollections();
        Touch();
    }

    public void EnsureCollections()
    {
        if (ForwardLines == null)
        {
            ForwardLines = new List<ForwardLineData>();
        }

        if (DefensePairs == null)
        {
            DefensePairs = new List<DefensePairData>();
        }

        if (Goalies == null)
        {
            Goalies = new GoalieLineupData();
        }

        if (ScratchPlayerIds == null)
        {
            ScratchPlayerIds = new List<string>();
        }
    }

    public void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }
}
