using UnityEngine;

public static class LeadershipConfig
{
    public const int MinLeadership = 0;
    public const int MaxLeadership = 100;
    public const int DefaultLeadership = 50;

    public const int ExcellentLeadershipThreshold = 85;
    public const int GoodLeadershipThreshold = 70;
    public const int AverageLeadershipThreshold = 50;
    public const int PoorLeadershipThreshold = 35;

    public const int CaptainMoraleBonusMax = 5;
    public const int CaptainChemistryBonusMax = 3;
    public const int NoCaptainPenalty = -3;
    public const int UnhappyCaptainPenalty = -4;
    public const int TradeRequestCaptainPenalty = -6;

    public const string RoleNone = "None";
    public const string RoleCaptain = "Captain";
    public const string RoleAlternate = "Alternate";

    public static int ClampLeadership(int value)
    {
        return Mathf.Clamp(value, MinLeadership, MaxLeadership);
    }

    public static string GetLeadershipLabel(int score)
    {
        score = ClampLeadership(score);
        if (score >= ExcellentLeadershipThreshold)
        {
            return "Excellent";
        }

        if (score >= GoodLeadershipThreshold)
        {
            return "Good";
        }

        if (score >= AverageLeadershipThreshold)
        {
            return "Average";
        }

        return score >= PoorLeadershipThreshold ? "Poor" : "Bad";
    }

    public static int GetMoraleImpactByLeadership(int leadershipScore)
    {
        leadershipScore = ClampLeadership(leadershipScore);
        if (leadershipScore >= ExcellentLeadershipThreshold)
        {
            return 5;
        }

        if (leadershipScore >= GoodLeadershipThreshold)
        {
            return 3;
        }

        if (leadershipScore >= AverageLeadershipThreshold)
        {
            return 1;
        }

        return leadershipScore >= PoorLeadershipThreshold ? -1 : -3;
    }

    public static int GetChemistryImpactByLeadership(int leadershipScore)
    {
        leadershipScore = ClampLeadership(leadershipScore);
        if (leadershipScore >= ExcellentLeadershipThreshold)
        {
            return 3;
        }

        if (leadershipScore >= GoodLeadershipThreshold)
        {
            return 2;
        }

        if (leadershipScore >= AverageLeadershipThreshold)
        {
            return 1;
        }

        return leadershipScore >= PoorLeadershipThreshold ? -1 : -2;
    }
}
