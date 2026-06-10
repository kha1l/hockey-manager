using System;
using System.Collections.Generic;

[Serializable]
public class InjuryHistoryData
{
    public List<InjuryRecordData> Injuries = new List<InjuryRecordData>();

    public InjuryHistoryData()
    {
        EnsureInjuries();
    }

    public void EnsureInjuries()
    {
        if (Injuries == null)
        {
            Injuries = new List<InjuryRecordData>();
        }
    }
}
