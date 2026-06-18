public static class RetirementConfig
{
    public const int MinimumRetirementAge = 34;
    public const int CommonRetirementAge = 36;
    public const int HighRetirementAge = 39;
    public const int MaximumPlayerAge = 45;

    public const int RetirementScoreThreshold = 70;
    public const int HallOfFameScoreThreshold = 100;
    public const int RetiredNumberScoreThreshold = 85;

    public const int MaxJerseyNumber = 98;
    public const int MinJerseyNumber = 1;

    public static int ClampJerseyNumber(int number)
    {
        if (number < MinJerseyNumber)
        {
            return MinJerseyNumber;
        }

        return number > MaxJerseyNumber ? MaxJerseyNumber : number;
    }

    public static string GetRetirementReasonByScore(int score)
    {
        if (score >= 90)
        {
            return "Age and declining performance";
        }

        if (score >= 80)
        {
            return "Veteran retirement";
        }

        return score >= 70 ? "Career transition" : "Retired";
    }

    public static string GetHallOfFameLabel(int score)
    {
        if (score >= 150)
        {
            return "Legend";
        }

        if (score >= 120)
        {
            return "Star";
        }

        return score >= 100 ? "Honored Career" : "Not inducted";
    }
}
