using System;
using System.Collections.Generic;

[Serializable]
public class ProspectSigningHistoryData
{
    public List<ProspectSigningData> Signings = new List<ProspectSigningData>();

    public ProspectSigningHistoryData()
    {
        EnsureSignings();
    }

    public void EnsureSignings()
    {
        if (Signings == null)
        {
            Signings = new List<ProspectSigningData>();
        }
    }
}
