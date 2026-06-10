using System;
using System.Collections.Generic;

[Serializable]
public class FreeAgentPoolData
{
    public bool IsInitialized;
    public string GeneratedAtUtc;
    public List<PlayerData> FreeAgents = new List<PlayerData>();

    public FreeAgentPoolData()
    {
        EnsureFreeAgents();
    }

    public void EnsureFreeAgents()
    {
        if (FreeAgents == null)
        {
            FreeAgents = new List<PlayerData>();
        }
    }
}
