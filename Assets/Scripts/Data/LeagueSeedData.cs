using System;
using System.Collections.Generic;

[Serializable]
public class LeagueSeedData
{
    public string SeedVersion;
    public string CreatedAtUtc;
    public List<TeamData> Teams = new List<TeamData>();

    public void EnsureCollections()
    {
        if (Teams == null)
        {
            Teams = new List<TeamData>();
        }

        foreach (TeamData team in Teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsurePlayers();
            team.EnsureDraftRights();
            foreach (PlayerData player in team.Players)
            {
                if (player == null)
                {
                    continue;
                }

                if (player.CareerAwardIds == null)
                {
                    player.CareerAwardIds = new List<string>();
                }
            }
        }
    }
}
