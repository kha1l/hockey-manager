using System;
using System.Collections.Generic;

[Serializable]
public class DraftData
{
    public bool IsInitialized;
    public bool IsCompleted;
    public int DraftYear;
    public int CurrentPickIndex;
    public int TotalRounds;
    public int PicksPerRound;
    public string DraftStatus;
    public List<ProspectData> Prospects = new List<ProspectData>();
    public List<DraftPickData> DraftOrder = new List<DraftPickData>();

    public DraftData()
    {
        EnsureCollections();
    }

    public void EnsureCollections()
    {
        if (Prospects == null)
        {
            Prospects = new List<ProspectData>();
        }

        if (DraftOrder == null)
        {
            DraftOrder = new List<DraftPickData>();
        }
    }
}
