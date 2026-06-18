using System;
using System.Collections.Generic;

[Serializable]
public class RetiredPlayersData
{
    public List<RetiredPlayerData> Players = new List<RetiredPlayerData>();
    public int TotalRetiredPlayers;
    public string LastRetirementAtUtc;

    public RetiredPlayersData()
    {
        EnsurePlayers();
    }

    public void EnsurePlayers()
    {
        if (Players == null)
        {
            Players = new List<RetiredPlayerData>();
        }

        if (LastRetirementAtUtc == null)
        {
            LastRetirementAtUtc = "";
        }

        TotalRetiredPlayers = Players.Count;
    }
}
