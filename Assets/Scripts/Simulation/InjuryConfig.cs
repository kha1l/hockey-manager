using System;

public static class InjuryConfig
{
    public const int MinInjuryDays = 1;
    public const int MinorInjuryMinDays = 1;
    public const int MinorInjuryMaxDays = 7;
    public const int MediumInjuryMinDays = 8;
    public const int MediumInjuryMaxDays = 21;
    public const int MajorInjuryMinDays = 22;
    public const int MajorInjuryMaxDays = 60;
    public const int LongTermInjuryMinDays = 61;
    public const int LongTermInjuryMaxDays = 120;

    public const int BaseInjuryRiskPerGameBasisPoints = 45;
    public const int HighFatigueRiskBonusBasisPoints = 35;
    public const int VeryHighFatigueRiskBonusBasisPoints = 75;
    public const int LowConditionRiskBonusBasisPoints = 40;
    public const int VeteranRiskBonusBasisPoints = 25;
    public const int AggressiveTacticsRiskBonusBasisPoints = 35;
    public const int ConsecutiveGamesRiskBonusBasisPoints = 25;

    public static int ClampInjuryDays(int days)
    {
        if (days < MinInjuryDays)
        {
            return MinInjuryDays;
        }

        if (days > LongTermInjuryMaxDays)
        {
            return LongTermInjuryMaxDays;
        }

        return days;
    }

    public static string GetRandomInjuryType(string position, int seed)
    {
        string[] skaterInjuries =
        {
            "Upper Body Injury",
            "Lower Body Injury",
            "Shoulder Injury",
            "Knee Injury",
            "Hand Injury",
            "Illness",
            "Concussion"
        };
        string[] goalieInjuries =
        {
            "Goalie Strain",
            "Lower Body Injury",
            "Hip Injury",
            "Illness",
            "Goalie Strain",
            "Lower Body Injury"
        };

        string[] injuries = position == "G" ? goalieInjuries : skaterInjuries;
        Random random = new Random(seed);
        return injuries[random.Next(0, injuries.Length)];
    }

    public static string GetSeverityByDays(int days)
    {
        if (days <= MinorInjuryMaxDays)
        {
            return "Minor";
        }

        if (days <= MediumInjuryMaxDays)
        {
            return "Medium";
        }

        if (days <= MajorInjuryMaxDays)
        {
            return "Major";
        }

        return "LongTerm";
    }
}
