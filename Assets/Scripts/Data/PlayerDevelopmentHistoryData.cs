using System;
using System.Collections.Generic;

[Serializable]
public class PlayerDevelopmentHistoryData
{
    public int LastProcessedSeasonStartYear;
    public List<PlayerDevelopmentChangeData> Changes = new List<PlayerDevelopmentChangeData>();

    public PlayerDevelopmentHistoryData()
    {
        EnsureChanges();
    }

    public void EnsureChanges()
    {
        if (Changes == null)
        {
            Changes = new List<PlayerDevelopmentChangeData>();
        }
    }
}
