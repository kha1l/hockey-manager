using System.Collections.Generic;

public static class TeamSeedData
{
    public static List<TeamData> CreateTeams()
    {
        return LeagueSeedService.CreateTeams();
    }
}
