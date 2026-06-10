public static class PlayerDevelopmentConfig
{
    public const int MinOverall = 40;
    public const int MaxOverall = 99;
    public const int MinPotential = 40;
    public const int MaxPotential = 99;

    public const int ProspectFastGrowthMaxAge = 20;
    public const int YoungGrowthMaxAge = 23;
    public const int SlowGrowthMaxAge = 26;
    public const int PrimeAgeMax = 30;
    public const int VeteranRegressionStartAge = 31;
    public const int HeavyRegressionStartAge = 35;

    public const int MaxYearlyGrowth = 5;
    public const int MaxProspectYearlyGrowth = 6;
    public const int MaxYearlyRegression = -5;

    public static int ClampOverall(int value)
    {
        return Clamp(value, MinOverall, MaxOverall);
    }

    public static int ClampPotential(int value)
    {
        return Clamp(value, MinPotential, MaxPotential);
    }

    private static int Clamp(int value, int minValue, int maxValue)
    {
        if (value < minValue)
        {
            return minValue;
        }

        if (value > maxValue)
        {
            return maxValue;
        }

        return value;
    }
}
