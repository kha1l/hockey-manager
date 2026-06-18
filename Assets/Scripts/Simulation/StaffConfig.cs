using UnityEngine;

public static class StaffConfig
{
    public const string RoleHeadCoach = "HeadCoach";
    public const string RoleAssistantCoach = "AssistantCoach";
    public const string RoleDevelopmentCoach = "DevelopmentCoach";
    public const string RoleGoalieCoach = "GoalieCoach";

    public const string StyleBalanced = "Balanced";
    public const string StyleOffensive = "Offensive";
    public const string StyleDefensive = "Defensive";
    public const string StyleAggressive = "Aggressive";
    public const string StyleDevelopment = "Development";
    public const string StyleGoalieFocused = "GoalieFocused";

    public const int MinRating = 40;
    public const int MaxRating = 99;
    public const int AverageRating = 70;

    public const int MaxStaffRatingModifier = 3;
    public const int MinStaffRatingModifier = -3;

    public static bool IsValidStaffRole(string role)
    {
        return role == RoleHeadCoach
            || role == RoleAssistantCoach
            || role == RoleDevelopmentCoach
            || role == RoleGoalieCoach;
    }

    public static bool IsValidCoachingStyle(string style)
    {
        return style == StyleBalanced
            || style == StyleOffensive
            || style == StyleDefensive
            || style == StyleAggressive
            || style == StyleDevelopment
            || style == StyleGoalieFocused;
    }

    public static int ClampRating(int value)
    {
        return Mathf.Clamp(value, MinRating, MaxRating);
    }

    public static int RatingToModifier(int rating)
    {
        int safeRating = ClampRating(rating);
        if (safeRating >= 88)
        {
            return 3;
        }

        if (safeRating >= 80)
        {
            return 2;
        }

        if (safeRating >= 72)
        {
            return 1;
        }

        if (safeRating >= 60)
        {
            return 0;
        }

        if (safeRating >= 52)
        {
            return -1;
        }

        return -2;
    }

    public static string GetStaffQualityLabel(int overall)
    {
        int safeOverall = ClampRating(overall);
        if (safeOverall >= 88)
        {
            return "Elite";
        }

        if (safeOverall >= 80)
        {
            return "Great";
        }

        if (safeOverall >= 72)
        {
            return "Good";
        }

        if (safeOverall >= 60)
        {
            return "Average";
        }

        if (safeOverall >= 52)
        {
            return "Below Average";
        }

        return "Poor";
    }
}
