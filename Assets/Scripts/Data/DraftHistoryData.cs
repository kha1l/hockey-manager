using System;
using System.Collections.Generic;

[Serializable]
public class DraftHistoryData
{
    public List<DraftPickData> CompletedPicks = new List<DraftPickData>();

    public DraftHistoryData()
    {
        EnsureCompletedPicks();
    }

    public void EnsureCompletedPicks()
    {
        if (CompletedPicks == null)
        {
            CompletedPicks = new List<DraftPickData>();
        }
    }
}
