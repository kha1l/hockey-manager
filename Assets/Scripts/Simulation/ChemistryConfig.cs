using UnityEngine;

public static class ChemistryConfig
{
    public const int MinChemistry = 0;
    public const int MaxChemistry = 100;
    public const int DefaultChemistry = 60;

    public const int ExcellentThreshold = 85;
    public const int GoodThreshold = 70;
    public const int AverageThreshold = 50;
    public const int PoorThreshold = 35;

    public const int MaxTeamRatingBonus = 2;
    public const int MaxTeamRatingPenalty = -2;

    public const string LabelExcellent = "Excellent";
    public const string LabelGood = "Good";
    public const string LabelAverage = "Average";
    public const string LabelPoor = "Poor";
    public const string LabelBad = "Bad";

    public static int ClampChemistry(int value)
    {
        return Mathf.Clamp(value, MinChemistry, MaxChemistry);
    }

    public static string GetChemistryLabel(int score)
    {
        score = ClampChemistry(score);
        if (score >= ExcellentThreshold)
        {
            return LabelExcellent;
        }

        if (score >= GoodThreshold)
        {
            return LabelGood;
        }

        if (score >= AverageThreshold)
        {
            return LabelAverage;
        }

        return score >= PoorThreshold ? LabelPoor : LabelBad;
    }

    public static int GetTeamRatingModifier(int teamChemistryScore)
    {
        teamChemistryScore = ClampChemistry(teamChemistryScore);
        if (teamChemistryScore >= ExcellentThreshold)
        {
            return MaxTeamRatingBonus;
        }

        if (teamChemistryScore >= 75)
        {
            return 1;
        }

        if (teamChemistryScore >= AverageThreshold)
        {
            return 0;
        }

        return teamChemistryScore >= PoorThreshold ? -1 : MaxTeamRatingPenalty;
    }

    public static string BuildChemistrySummary(int score, int roleBalance, int morale, int condition, int stability)
    {
        string summary;
        score = ClampChemistry(score);
        if (score >= ExcellentThreshold)
        {
            summary = "Excellent fit";
        }
        else if (score >= GoodThreshold)
        {
            summary = "Good fit";
        }
        else if (score >= AverageThreshold)
        {
            summary = "Average fit";
        }
        else if (score >= PoorThreshold)
        {
            summary = "Poor fit";
        }
        else
        {
            summary = "Bad fit";
        }

        if (roleBalance < 45)
        {
            summary += " | role balance issue";
        }

        if (morale < 45)
        {
            summary += " | morale issue";
        }

        if (condition < 55)
        {
            summary += " | fatigue issue";
        }

        if (stability < 65)
        {
            summary += " | new unit";
        }

        return summary;
    }
}
