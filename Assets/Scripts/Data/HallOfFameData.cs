using System;
using System.Collections.Generic;

[Serializable]
public class HallOfFameData
{
    public List<HallOfFameInducteeData> Inductees = new List<HallOfFameInducteeData>();
    public int TotalInductees;
    public string LastInductionAtUtc;

    public HallOfFameData()
    {
        EnsureInductees();
    }

    public void EnsureInductees()
    {
        if (Inductees == null)
        {
            Inductees = new List<HallOfFameInducteeData>();
        }

        if (LastInductionAtUtc == null)
        {
            LastInductionAtUtc = "";
        }

        TotalInductees = Inductees.Count;
    }
}
