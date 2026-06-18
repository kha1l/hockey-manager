using System;

public static class DraftClassProfileGenerator
{
    public static DraftClassProfileData GenerateProfile(int draftYear)
    {
        DraftClassProfileData profile = new DraftClassProfileData
        {
            ProfileId = "draft-class-profile-" + draftYear,
            DraftYear = draftYear,
            StrengthType = DetermineStrengthType(draftYear),
            DepthType = DetermineDepthType(draftYear),
            PositionalTheme = DeterminePositionalTheme(draftYear),
            GeneratedAtUtc = DateTime.UtcNow.ToString("o")
        };

        ApplyProfileModifiers(profile);
        profile.Summary = DraftClassConfig.BuildClassSummary(profile);
        return profile;
    }

    public static DraftClassProfileData CreateFallbackProfile(int draftYear)
    {
        DraftClassProfileData profile = new DraftClassProfileData
        {
            ProfileId = "draft-class-profile-fallback-" + draftYear,
            DraftYear = draftYear,
            StrengthType = DraftClassConfig.StrengthAverage,
            DepthType = DraftClassConfig.DepthAverage,
            PositionalTheme = DraftClassConfig.ThemeBalanced,
            GeneratedAtUtc = DateTime.UtcNow.ToString("o")
        };

        ApplyProfileModifiers(profile);
        profile.Summary = "Average balanced draft class";
        return profile;
    }

    public static string DetermineStrengthType(int draftYear)
    {
        int roll = StableRange("draft-strength:" + draftYear, 0, 99);
        if (roll < 25)
        {
            return DraftClassConfig.StrengthWeak;
        }

        return roll < 75 ? DraftClassConfig.StrengthAverage : DraftClassConfig.StrengthStrong;
    }

    public static string DetermineDepthType(int draftYear)
    {
        int roll = StableRange("draft-depth:" + draftYear, 0, 99);
        if (roll < 25)
        {
            return DraftClassConfig.DepthShallow;
        }

        if (roll < 65)
        {
            return DraftClassConfig.DepthAverage;
        }

        return roll < 85 ? DraftClassConfig.DepthDeep : DraftClassConfig.DepthTopHeavy;
    }

    public static string DeterminePositionalTheme(int draftYear)
    {
        int roll = StableRange("draft-theme:" + draftYear, 0, 99);
        if (roll < 55)
        {
            return DraftClassConfig.ThemeBalanced;
        }

        if (roll < 75)
        {
            return DraftClassConfig.ThemeForwardHeavy;
        }

        return roll < 92 ? DraftClassConfig.ThemeDefenseHeavy : DraftClassConfig.ThemeGoalieHeavy;
    }

    public static void ApplyProfileModifiers(DraftClassProfileData profile)
    {
        if (profile == null)
        {
            return;
        }

        profile.OverallQualityModifier = 0;
        profile.PotentialQualityModifier = 0;
        profile.TopProspectBonus = 0;
        profile.DepthBonus = 0;
        profile.ForwardQualityModifier = 0;
        profile.DefenseQualityModifier = 0;
        profile.GoalieQualityModifier = 0;

        if (profile.StrengthType == DraftClassConfig.StrengthWeak)
        {
            profile.OverallQualityModifier = -2;
            profile.PotentialQualityModifier = -3;
            profile.ExpectedEliteProspects = 1;
            profile.ExpectedFirstRoundTalent = 24;
            profile.ExpectedNhlDepthPlayers = 55;
        }
        else if (profile.StrengthType == DraftClassConfig.StrengthStrong)
        {
            profile.OverallQualityModifier = 2;
            profile.PotentialQualityModifier = 3;
            profile.ExpectedEliteProspects = 4;
            profile.ExpectedFirstRoundTalent = 40;
            profile.ExpectedNhlDepthPlayers = 85;
        }
        else
        {
            profile.StrengthType = DraftClassConfig.StrengthAverage;
            profile.ExpectedEliteProspects = 2;
            profile.ExpectedFirstRoundTalent = 32;
            profile.ExpectedNhlDepthPlayers = 70;
        }

        if (profile.DepthType == DraftClassConfig.DepthShallow)
        {
            profile.DepthBonus = -3;
        }
        else if (profile.DepthType == DraftClassConfig.DepthDeep)
        {
            profile.DepthBonus = 3;
        }
        else if (profile.DepthType == DraftClassConfig.DepthTopHeavy)
        {
            profile.TopProspectBonus = 4;
            profile.DepthBonus = -1;
        }
        else
        {
            profile.DepthType = DraftClassConfig.DepthAverage;
        }

        if (profile.PositionalTheme == DraftClassConfig.ThemeForwardHeavy)
        {
            profile.ForwardQualityModifier = 2;
        }
        else if (profile.PositionalTheme == DraftClassConfig.ThemeDefenseHeavy)
        {
            profile.DefenseQualityModifier = 2;
        }
        else if (profile.PositionalTheme == DraftClassConfig.ThemeGoalieHeavy)
        {
            profile.GoalieQualityModifier = 3;
        }
        else
        {
            profile.PositionalTheme = DraftClassConfig.ThemeBalanced;
        }
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 23;
            string text = value ?? "";
            for (int i = 0; i < text.Length; i++)
            {
                hash = hash * 31 + text[i];
            }

            long positiveHash = hash;
            if (positiveHash < 0)
            {
                positiveHash = -positiveHash;
            }

            return (int)(positiveHash % int.MaxValue);
        }
    }

    private static int StableRange(string seed, int minInclusive, int maxInclusive)
    {
        if (maxInclusive <= minInclusive)
        {
            return minInclusive;
        }

        int span = maxInclusive - minInclusive + 1;
        return minInclusive + StableHash(seed) % span;
    }
}
