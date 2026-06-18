public static class OwnerGoalConfig
{
    public const int DefaultGmTrust = 65;
    public const int MinTrust = 0;
    public const int MaxTrust = 100;

    public const int ExcellentTrustThreshold = 85;
    public const int SafeTrustThreshold = 65;
    public const int PressureTrustThreshold = 45;
    public const int DangerTrustThreshold = 25;

    public const int MaxEvaluationHistoryToKeep = 20;
    public const int MaxGlobalEvaluationHistoryToKeep = 50;

    public const string GoalMakePlayoffs = "Make Playoffs";
    public const string GoalWinPlayoffRound = "Win Playoff Round";
    public const string GoalReachConferenceFinal = "Reach Conference Final";
    public const string GoalReachStanleyCupFinal = "Reach Stanley Cup Final";
    public const string GoalWinCup = "Win Stanley Cup";
    public const string GoalImproveTeam = "Improve Team";
    public const string GoalDevelopYoungPlayers = "Develop Young Players";
    public const string GoalAcquireDraftPicks = "Acquire Draft Picks";
    public const string GoalReducePayroll = "Reduce Payroll";
    public const string GoalStayUnderCap = "Stay Under Cap";
    public const string GoalReSignCorePlayer = "Re-sign Core Player";
    public const string GoalImproveMorale = "Improve Morale";
    public const string GoalBuildForFuture = "Build For Future";

    public const string GoalTypePrimary = "Primary";
    public const string GoalTypeSecondary = "Secondary";
    public const string GoalTypeFinancial = "Financial";
    public const string GoalTypeDevelopment = "Development";
    public const string GoalTypeRoster = "Roster";

    public const string StatusActive = "Active";
    public const string StatusCompleted = "Completed";
    public const string StatusFailed = "Failed";
    public const string StatusNotEvaluated = "NotEvaluated";

    public static int ClampTrust(int value)
    {
        if (value < MinTrust)
        {
            return MinTrust;
        }

        return value > MaxTrust ? MaxTrust : value;
    }

    public static string GetJobSecurityLabel(int trust)
    {
        if (trust >= ExcellentTrustThreshold)
        {
            return "Excellent";
        }

        if (trust >= SafeTrustThreshold)
        {
            return "Safe";
        }

        if (trust >= PressureTrustThreshold)
        {
            return "Pressure";
        }

        if (trust >= DangerTrustThreshold)
        {
            return "Danger";
        }

        return "Critical";
    }

    public static string GetOwnerSatisfactionLabel(int satisfaction)
    {
        if (satisfaction >= 85)
        {
            return "Excellent";
        }

        if (satisfaction >= 70)
        {
            return "Good";
        }

        if (satisfaction >= 50)
        {
            return "Stable";
        }

        if (satisfaction >= 35)
        {
            return "Warning";
        }

        return "Poor";
    }
}
