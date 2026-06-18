public static class DraftClassConfig
{
    public const string StrengthWeak = "Weak";
    public const string StrengthAverage = "Average";
    public const string StrengthStrong = "Strong";

    public const string DepthShallow = "Shallow";
    public const string DepthAverage = "Average";
    public const string DepthDeep = "Deep";
    public const string DepthTopHeavy = "TopHeavy";

    public const string ThemeBalanced = "Balanced";
    public const string ThemeForwardHeavy = "ForwardHeavy";
    public const string ThemeDefenseHeavy = "DefenseHeavy";
    public const string ThemeGoalieHeavy = "GoalieHeavy";

    public const int DraftClassSize = 140;
    public const int DraftedPicksCount = 96;

    public static bool IsValidStrengthType(string value)
    {
        return value == StrengthWeak || value == StrengthAverage || value == StrengthStrong;
    }

    public static bool IsValidDepthType(string value)
    {
        return value == DepthShallow || value == DepthAverage || value == DepthDeep || value == DepthTopHeavy;
    }

    public static bool IsValidPositionalTheme(string value)
    {
        return value == ThemeBalanced || value == ThemeForwardHeavy || value == ThemeDefenseHeavy || value == ThemeGoalieHeavy;
    }

    public static string GetProjectedRoundByRank(int rank)
    {
        if (rank >= 1 && rank <= 32)
        {
            return "1st Round";
        }

        if (rank <= 64)
        {
            return "2nd Round";
        }

        if (rank <= DraftedPicksCount)
        {
            return "3rd Round";
        }

        return "Undrafted";
    }

    public static int GetProjectedRoundNumberByRank(int rank)
    {
        if (rank >= 1 && rank <= 32)
        {
            return 1;
        }

        if (rank <= 64)
        {
            return 2;
        }

        return rank <= DraftedPicksCount ? 3 : 0;
    }

    public static string BuildClassSummary(DraftClassProfileData profile)
    {
        if (profile == null)
        {
            return "Average, average, balanced draft class";
        }

        return profile.StrengthType + ", "
            + FormatDepth(profile.DepthType) + ", "
            + FormatTheme(profile.PositionalTheme)
            + " draft class";
    }

    private static string FormatDepth(string depthType)
    {
        return depthType == DepthTopHeavy ? "top-heavy" : (depthType ?? DepthAverage).ToLowerInvariant();
    }

    private static string FormatTheme(string theme)
    {
        if (theme == ThemeForwardHeavy)
        {
            return "forward-heavy";
        }

        if (theme == ThemeDefenseHeavy)
        {
            return "defense-heavy";
        }

        if (theme == ThemeGoalieHeavy)
        {
            return "goalie-heavy";
        }

        return "balanced";
    }
}
