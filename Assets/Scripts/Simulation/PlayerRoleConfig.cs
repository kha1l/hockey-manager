using System.Collections.Generic;

public static class PlayerRoleConfig
{
    public const string Sniper = "Sniper";
    public const string Playmaker = "Playmaker";
    public const string PowerForward = "PowerForward";
    public const string TwoWayForward = "TwoWayForward";
    public const string Grinder = "Grinder";
    public const string DepthForward = "DepthForward";

    public const string OffensiveDefenseman = "OffensiveDefenseman";
    public const string DefensiveDefenseman = "DefensiveDefenseman";
    public const string TwoWayDefenseman = "TwoWayDefenseman";
    public const string StayAtHomeDefenseman = "StayAtHomeDefenseman";

    public const string StarterGoalie = "StarterGoalie";
    public const string BackupGoalie = "BackupGoalie";
    public const string DepthGoalie = "DepthGoalie";

    public static bool IsForwardRole(string role)
    {
        return role == Sniper
            || role == Playmaker
            || role == PowerForward
            || role == TwoWayForward
            || role == Grinder
            || role == DepthForward;
    }

    public static bool IsDefenseRole(string role)
    {
        return role == OffensiveDefenseman
            || role == DefensiveDefenseman
            || role == TwoWayDefenseman
            || role == StayAtHomeDefenseman;
    }

    public static bool IsGoalieRole(string role)
    {
        return role == StarterGoalie
            || role == BackupGoalie
            || role == DepthGoalie;
    }

    public static List<string> GetRolesForPosition(string position)
    {
        if (position == "C" || position == "LW" || position == "RW")
        {
            return new List<string>
            {
                Sniper,
                Playmaker,
                PowerForward,
                TwoWayForward,
                Grinder,
                DepthForward
            };
        }

        if (position == "D")
        {
            return new List<string>
            {
                OffensiveDefenseman,
                DefensiveDefenseman,
                TwoWayDefenseman,
                StayAtHomeDefenseman
            };
        }

        if (position == "G")
        {
            return new List<string>
            {
                StarterGoalie,
                BackupGoalie,
                DepthGoalie
            };
        }

        return new List<string>();
    }
}
