public static class ProspectArchetypeGenerator
{
    public static string DetermineArchetype(string position, int draftRank, DraftClassProfileData profile, string seed)
    {
        if (position == "D")
        {
            return DetermineDefenseArchetype(draftRank, profile, seed);
        }

        if (position == "G")
        {
            return DetermineGoalieArchetype(draftRank, profile, seed);
        }

        return DetermineForwardArchetype(draftRank, profile, seed);
    }

    public static string DetermineForwardArchetype(int draftRank, DraftClassProfileData profile, string seed)
    {
        int roll = StableRange(seed + ":forward-archetype", 0, 99);
        bool forwardHeavy = profile != null && profile.PositionalTheme == DraftClassConfig.ThemeForwardHeavy;
        if (draftRank <= 10)
        {
            if (roll < (forwardHeavy ? 28 : 22))
            {
                return ProspectArchetypeConfig.EliteSniper;
            }

            if (roll < 48)
            {
                return ProspectArchetypeConfig.TwoWayCenter;
            }

            if (roll < 68)
            {
                return ProspectArchetypeConfig.PlaymakingWinger;
            }
        }

        if (draftRank > 80)
        {
            if (roll < 35)
            {
                return ProspectArchetypeConfig.DepthForward;
            }

            if (roll < 62)
            {
                return ProspectArchetypeConfig.CheckingForward;
            }

            if (roll < 82)
            {
                return ProspectArchetypeConfig.RawScoringForward;
            }

            return ProspectArchetypeConfig.BoomBustWinger;
        }

        if (roll < 18)
        {
            return ProspectArchetypeConfig.PowerForward;
        }

        if (roll < 36)
        {
            return ProspectArchetypeConfig.PlaymakingWinger;
        }

        if (roll < 54)
        {
            return ProspectArchetypeConfig.TwoWayCenter;
        }

        if (roll < 72)
        {
            return ProspectArchetypeConfig.CheckingForward;
        }

        return roll < 88 ? ProspectArchetypeConfig.RawScoringForward : ProspectArchetypeConfig.BoomBustWinger;
    }

    public static string DetermineDefenseArchetype(int draftRank, DraftClassProfileData profile, string seed)
    {
        int roll = StableRange(seed + ":defense-archetype", 0, 99);
        bool defenseHeavy = profile != null && profile.PositionalTheme == DraftClassConfig.ThemeDefenseHeavy;
        if (draftRank <= 15)
        {
            if (roll < (defenseHeavy ? 30 : 22))
            {
                return ProspectArchetypeConfig.OffensiveDefenseman;
            }

            if (roll < 48)
            {
                return ProspectArchetypeConfig.TwoWayDefenseman;
            }

            if (roll < 68)
            {
                return ProspectArchetypeConfig.ShutdownDefenseman;
            }
        }

        if (draftRank > 80)
        {
            if (roll < 32)
            {
                return ProspectArchetypeConfig.StayAtHomeDefenseman;
            }

            if (roll < 62)
            {
                return ProspectArchetypeConfig.RawDefenseman;
            }

            if (roll < 82)
            {
                return ProspectArchetypeConfig.LateBloomingDefenseman;
            }

            return ProspectArchetypeConfig.PuckMovingDefenseman;
        }

        if (roll < 22)
        {
            return ProspectArchetypeConfig.PuckMovingDefenseman;
        }

        if (roll < 42)
        {
            return ProspectArchetypeConfig.TwoWayDefenseman;
        }

        if (roll < 62)
        {
            return ProspectArchetypeConfig.ShutdownDefenseman;
        }

        if (roll < 80)
        {
            return ProspectArchetypeConfig.StayAtHomeDefenseman;
        }

        return roll < 90 ? ProspectArchetypeConfig.RawDefenseman : ProspectArchetypeConfig.LateBloomingDefenseman;
    }

    public static string DetermineGoalieArchetype(int draftRank, DraftClassProfileData profile, string seed)
    {
        int roll = StableRange(seed + ":goalie-archetype", 0, 99);
        bool goalieHeavy = profile != null && profile.PositionalTheme == DraftClassConfig.ThemeGoalieHeavy;
        if (goalieHeavy && draftRank <= 40)
        {
            if (roll < 24)
            {
                return ProspectArchetypeConfig.FranchiseGoalie;
            }

            if (roll < 58)
            {
                return ProspectArchetypeConfig.StarterGoalie;
            }

            if (roll < 80)
            {
                return ProspectArchetypeConfig.RawGoalie;
            }
        }

        if (draftRank <= 20 && roll < 12)
        {
            return ProspectArchetypeConfig.FranchiseGoalie;
        }

        if (draftRank > 80)
        {
            if (roll < 42)
            {
                return ProspectArchetypeConfig.SafeBackupGoalie;
            }

            if (roll < 74)
            {
                return ProspectArchetypeConfig.RawGoalie;
            }

            return ProspectArchetypeConfig.BoomBustGoalie;
        }

        if (roll < 30)
        {
            return ProspectArchetypeConfig.StarterGoalie;
        }

        if (roll < 62)
        {
            return ProspectArchetypeConfig.SafeBackupGoalie;
        }

        return roll < 84 ? ProspectArchetypeConfig.RawGoalie : ProspectArchetypeConfig.BoomBustGoalie;
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
