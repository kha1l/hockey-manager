public static class ScoutingConfig
{
    public const int MinAccuracy = 0;
    public const int MaxAccuracy = 100;
    public const int InitialAccuracyMin = 15;
    public const int InitialAccuracyMax = 35;

    public const int ScoutPlayerAccuracyGain = 25;
    public const int ScoutTopProspectsAccuracyGain = 12;
    public const int ScoutByPositionAccuracyGain = 15;

    public const int FullyScoutedAccuracy = 95;

    public const int MaxReportsToKeep = 200;

    public const int TopProspectsScoutCount = 10;

    public static int ClampAccuracy(int value)
    {
        if (value < MinAccuracy)
        {
            return MinAccuracy;
        }

        return value > MaxAccuracy ? MaxAccuracy : value;
    }

    public static int GetEstimateRangeByAccuracy(int accuracy)
    {
        if (accuracy < 25)
        {
            return 14;
        }

        if (accuracy < 50)
        {
            return 10;
        }

        if (accuracy < 75)
        {
            return 7;
        }

        return accuracy < FullyScoutedAccuracy ? 4 : 1;
    }

    public static string GetGradeByPotential(int estimatedPotential)
    {
        if (estimatedPotential >= 90)
        {
            return "A+";
        }

        if (estimatedPotential >= 86)
        {
            return "A";
        }

        if (estimatedPotential >= 82)
        {
            return "B+";
        }

        if (estimatedPotential >= 78)
        {
            return "B";
        }

        if (estimatedPotential >= 74)
        {
            return "C+";
        }

        return estimatedPotential >= 70 ? "C" : "D";
    }

    public static string GetRiskLevelByAccuracyAndPotential(int accuracy, int potential)
    {
        if (accuracy >= 80)
        {
            return "Low";
        }

        if (potential >= 86 && accuracy < 50)
        {
            return "High";
        }

        return "Medium";
    }

    public static string GetDraftProjectionByRank(int rank)
    {
        return DraftClassConfig.GetProjectedRoundByRank(rank);
    }
}
