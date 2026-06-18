public static class CpuRosterManagementConfig
{
    public const int MaxReportsToKeep = 20;

    public const int MinNhlRosterSize = 20;
    public const int MaxNhlRosterSize = 23;

    public const int RequiredForwards = 12;
    public const int RequiredDefensemen = 6;
    public const int RequiredGoalies = 2;

    public const int PreferredForwards = 13;
    public const int PreferredDefensemen = 7;
    public const int PreferredGoalies = 2;

    public const int MaxNhlGoalies = 3;

    public const int MinHealthyForwardsForGame = 12;
    public const int MinHealthyDefensemenForGame = 6;
    public const int MinHealthyGoaliesForGame = 2;

    public const int LowOverallSendDownThreshold = 72;
    public const int ProspectAgeMax = 23;

    public static bool IsForward(PlayerData player)
    {
        return player != null && (player.Position == "C" || player.Position == "LW" || player.Position == "RW");
    }

    public static bool IsDefenseman(PlayerData player)
    {
        return player != null && player.Position == "D";
    }

    public static bool IsGoalie(PlayerData player)
    {
        return player != null && player.Position == "G";
    }

    public static int GetPositionNeedScore(TeamData team, string position)
    {
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        if (team == null)
        {
            return 0;
        }

        string group = NormalizePositionGroup(position);
        int forwards = 0;
        int defensemen = 0;
        int goalies = 0;

        foreach (PlayerData player in TeamRosterService.GetNhlPlayers(team))
        {
            if (IsForward(player))
            {
                forwards++;
            }
            else if (IsDefenseman(player))
            {
                defensemen++;
            }
            else if (IsGoalie(player))
            {
                goalies++;
            }
        }

        if (group == "Forward")
        {
            return RequiredForwards - forwards;
        }

        if (group == "Defense")
        {
            return RequiredDefensemen - defensemen;
        }

        if (group == "Goalie")
        {
            return RequiredGoalies - goalies;
        }

        int totalNeed = 0;
        if (forwards < RequiredForwards)
        {
            totalNeed += RequiredForwards - forwards;
        }

        if (defensemen < RequiredDefensemen)
        {
            totalNeed += RequiredDefensemen - defensemen;
        }

        if (goalies < RequiredGoalies)
        {
            totalNeed += RequiredGoalies - goalies;
        }

        return totalNeed;
    }

    private static string NormalizePositionGroup(string position)
    {
        if (position == "Forward" || position == "F" || position == "C" || position == "LW" || position == "RW")
        {
            return "Forward";
        }

        if (position == "Defense" || position == "D")
        {
            return "Defense";
        }

        if (position == "Goalie" || position == "G")
        {
            return "Goalie";
        }

        return "Any";
    }
}
