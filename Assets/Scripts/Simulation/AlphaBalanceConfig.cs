public static class AlphaBalanceConfig
{
    public const int MaxReportsToKeep = 10;

    public const int TargetMinGoalsPerGame = 4;
    public const int TargetMaxGoalsPerGame = 8;

    public const int TargetMinAverageTeamPoints = 70;
    public const int TargetMaxAverageTeamPoints = 115;

    public const int TargetMaxInvalidRosterTeams = 0;
    public const int TargetMaxInvalidLineupTeams = 0;

    public const int TargetMaxCapViolationTeams = 4;

    public const int TargetMinAverageMorale = 45;
    public const int TargetMaxAverageMorale = 85;

    public const int TargetMinAverageChemistry = 40;
    public const int TargetMaxAverageChemistry = 85;

    public const int TargetMaxMajorInjuriesPerTeam = 8;
    public const int TargetMaxTotalInjuriesPerTeam = 25;

    public const int TargetMinFreeAgentsRemainingAfterOffseason = 20;
    public const int TargetMaxFreeAgentsRemainingAfterOffseason = 250;

    public const int TargetMaxPlayersOverall90Plus = 40;
    public const int TargetMaxPlayersOverall95Plus = 10;

    public const int TargetMinDraftClassSize = 140;
    public const int TargetMaxDraftClassSize = 140;

    public const int TargetMaxNewsItems = 300;

    public const int AlphaShortSimSeasons = 1;
    public const int AlphaMediumSimSeasons = 3;
    public const int AlphaLongSimSeasons = 5;

    public static string GetPassFailLabel(bool passed)
    {
        return passed ? "PASS" : "WARN";
    }

    public static string GetRangeStatus(int value, int min, int max)
    {
        if (value < min)
        {
            return "LOW";
        }

        if (value > max)
        {
            return "HIGH";
        }

        return "OK";
    }
}
