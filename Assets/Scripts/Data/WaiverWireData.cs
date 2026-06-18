using System;
using System.Collections.Generic;

[Serializable]
public class WaiverWireData
{
    public List<WaiverPlayerData> ActiveWaivers = new List<WaiverPlayerData>();
    public List<WaiverPlayerData> WaiverHistory = new List<WaiverPlayerData>();
    public List<WaiverClaimData> Claims = new List<WaiverClaimData>();

    public void EnsureCollections()
    {
        if (ActiveWaivers == null)
        {
            ActiveWaivers = new List<WaiverPlayerData>();
        }

        if (WaiverHistory == null)
        {
            WaiverHistory = new List<WaiverPlayerData>();
        }

        if (Claims == null)
        {
            Claims = new List<WaiverClaimData>();
        }
    }
}
