using System.Collections.Generic;

public static class PlayerSeedData
{
    public static List<PlayerData> CreatePlayersForTeam(string teamId)
    {
        return LeagueSeedGenerator.CreatePlayersForTeam(teamId);
    }
}
