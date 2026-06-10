using System;
using System.Collections.Generic;

[Serializable]
public class FreeAgentHistoryData
{
    public List<FreeAgentSigningData> Signings = new List<FreeAgentSigningData>();

    public FreeAgentHistoryData()
    {
        EnsureSignings();
    }

    public void EnsureSignings()
    {
        if (Signings == null)
        {
            Signings = new List<FreeAgentSigningData>();
        }
    }
}
