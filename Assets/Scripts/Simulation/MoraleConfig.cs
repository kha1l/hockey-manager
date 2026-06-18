using UnityEngine;

public static class MoraleConfig
{
    public const int MinMorale = 0;
    public const int MaxMorale = 100;
    public const int DefaultMorale = 70;

    public const int HappyThreshold = 80;
    public const int ContentThreshold = 60;
    public const int ConcernedThreshold = 40;
    public const int UnhappyThreshold = 20;

    public const int TradeRequestMoraleThreshold = 25;
    public const int TradeRequestLowMoraleGamesRequired = 5;

    public const int MaxMoraleEventsToKeep = 300;

    public const string StatusHappy = "Happy";
    public const string StatusContent = "Content";
    public const string StatusConcerned = "Concerned";
    public const string StatusUnhappy = "Unhappy";
    public const string StatusVeryUnhappy = "VeryUnhappy";

    public const string TrendUp = "Up";
    public const string TrendStable = "Stable";
    public const string TrendDown = "Down";

    public static int ClampMorale(int value)
    {
        return Mathf.Clamp(value, MinMorale, MaxMorale);
    }

    public static string GetMoraleStatus(int morale)
    {
        morale = ClampMorale(morale);
        if (morale >= HappyThreshold)
        {
            return StatusHappy;
        }

        if (morale >= ContentThreshold)
        {
            return StatusContent;
        }

        if (morale >= ConcernedThreshold)
        {
            return StatusConcerned;
        }

        return morale >= UnhappyThreshold ? StatusUnhappy : StatusVeryUnhappy;
    }

    public static string GetMoraleTrend(int before, int after)
    {
        if (after >= before + 3)
        {
            return TrendUp;
        }

        if (after <= before - 3)
        {
            return TrendDown;
        }

        return TrendStable;
    }

    public static int GetEffectiveOverallPenalty(int morale)
    {
        morale = ClampMorale(morale);
        if (morale >= ContentThreshold)
        {
            return 0;
        }

        if (morale >= ConcernedThreshold)
        {
            return 1;
        }

        return morale >= UnhappyThreshold ? 2 : 3;
    }
}
