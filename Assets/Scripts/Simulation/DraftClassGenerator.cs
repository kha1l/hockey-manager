using System;
using System.Collections.Generic;

public static class DraftClassGenerator
{
    private static readonly string[] Nationalities =
    {
        "Canada",
        "USA",
        "Sweden",
        "Finland",
        "Czechia",
        "Slovakia",
        "Germany",
        "Switzerland"
    };

    public static List<ProspectData> CreateDraftClass(int draftYear)
    {
        return CreateDraftClass(draftYear, DraftClassProfileGenerator.GenerateProfile(draftYear));
    }

    public static List<ProspectData> CreateDraftClass(int draftYear, DraftClassProfileData profile)
    {
        if (profile == null)
        {
            profile = DraftClassProfileGenerator.GenerateProfile(draftYear);
        }

        DraftClassProfileGenerator.ApplyProfileModifiers(profile);
        profile.Summary = DraftClassConfig.BuildClassSummary(profile);

        List<string> positions = CreatePositionPool(profile);
        List<ProspectData> prospects = new List<ProspectData>();
        for (int i = 1; i <= DraftClassConfig.DraftClassSize; i++)
        {
            string position = positions[i - 1];
            prospects.Add(CreateProspect(draftYear, i, position, profile));
        }

        prospects.Sort(CompareProspectsForProjection);
        AssignRanksAndMetadata(prospects, profile);
        return prospects;
    }

    public static void EnsureRanksAndMetadata(List<ProspectData> prospects, DraftClassProfileData profile, int draftYear)
    {
        if (prospects == null || prospects.Count == 0)
        {
            return;
        }

        if (profile == null)
        {
            profile = DraftClassProfileGenerator.CreateFallbackProfile(draftYear);
        }

        DraftClassProfileGenerator.ApplyProfileModifiers(profile);
        if (string.IsNullOrEmpty(profile.Summary))
        {
            profile.Summary = DraftClassConfig.BuildClassSummary(profile);
        }

        prospects.Sort(CompareExistingProspectsForProjection);
        for (int i = 0; i < prospects.Count; i++)
        {
            ProspectData prospect = prospects[i];
            if (prospect == null)
            {
                continue;
            }

            int rank = i + 1;
            if (prospect.DraftRank <= 0)
            {
                prospect.DraftRank = rank;
            }

            if (prospect.ProjectedPick <= 0)
            {
                prospect.ProjectedPick = prospect.DraftRank;
            }

            if (string.IsNullOrEmpty(prospect.ProjectedRound))
            {
                prospect.ProjectedRound = DraftClassConfig.GetProjectedRoundByRank(prospect.DraftRank);
            }

            if (prospect.ProjectedRoundNumber <= 0)
            {
                prospect.ProjectedRoundNumber = GetRoundNumber(prospect.DraftRank);
            }

            if (string.IsNullOrEmpty(prospect.ProspectArchetype))
            {
                prospect.ProspectArchetype = ProspectArchetypeGenerator.DetermineArchetype(
                    prospect.Position,
                    prospect.DraftRank,
                    profile,
                    prospect.Id);
            }

            ApplyProfileMetadata(prospect, profile);
            if (prospect.ClassAdjustedOverall <= 0)
            {
                prospect.ClassAdjustedOverall = prospect.Overall;
            }

            if (prospect.ClassAdjustedPotential <= 0)
            {
                prospect.ClassAdjustedPotential = prospect.Potential;
            }
        }
    }

    private static ProspectData CreateProspect(
        int draftYear,
        int index,
        string position,
        DraftClassProfileData profile)
    {
        string id = "draft-" + draftYear + "-prospect-" + index.ToString("000");
        Random random = new Random(CreateStableSeed(draftYear, index));
        int provisionalRank = index;
        string archetype = ProspectArchetypeGenerator.DetermineArchetype(position, provisionalRank, profile, id);
        int overall = GenerateOverall(provisionalRank, random);
        int potential = GeneratePotential(provisionalRank, overall, random);

        ApplyClassAndArchetypeModifiers(position, provisionalRank, profile, archetype, ref overall, ref potential);
        overall = Clamp(overall, 40, 80);
        potential = Clamp(Math.Max(potential, overall), 55, 99);

        ProspectData prospect = new ProspectData
        {
            Id = id,
            FirstName = "Prospect",
            LastName = index.ToString("000"),
            Position = position,
            Nationality = Nationalities[random.Next(0, Nationalities.Length)],
            Age = GenerateAge(random),
            Overall = overall,
            Potential = potential,
            ProjectedRound = DraftClassConfig.GetProjectedRoundByRank(provisionalRank),
            ProjectedRoundNumber = GetRoundNumber(provisionalRank),
            ProjectedPick = provisionalRank,
            DraftRank = provisionalRank,
            ProspectArchetype = archetype,
            ClassAdjustedOverall = overall,
            ClassAdjustedPotential = potential,
            IsDrafted = false,
            DraftedByTeamId = "",
            DraftedByTeamName = "",
            DraftRound = 0,
            DraftPickOverall = 0
        };

        ApplyProfileMetadata(prospect, profile);
        return prospect;
    }

    private static void ApplyClassAndArchetypeModifiers(
        string position,
        int rank,
        DraftClassProfileData profile,
        string archetype,
        ref int overall,
        ref int potential)
    {
        overall += profile == null ? 0 : profile.OverallQualityModifier;
        potential += profile == null ? 0 : profile.PotentialQualityModifier;

        if (profile != null)
        {
            int depthModifier = GetDepthModifierByRank(profile, rank);
            overall += depthModifier / 2;
            potential += depthModifier;

            if (rank <= 10)
            {
                overall += profile.TopProspectBonus / 2;
                potential += profile.TopProspectBonus;
            }

            int themeModifier = GetThemeModifier(profile, position);
            overall += themeModifier / 2;
            potential += themeModifier;
        }

        overall += ProspectArchetypeConfig.GetOverallModifierForArchetype(archetype);
        potential += ProspectArchetypeConfig.GetPotentialModifierForArchetype(archetype);
    }

    private static int GetDepthModifierByRank(DraftClassProfileData profile, int rank)
    {
        if (profile == null)
        {
            return 0;
        }

        if (profile.DepthType == DraftClassConfig.DepthTopHeavy && rank >= 40)
        {
            return profile.DepthBonus - 1;
        }

        if (rank >= 33 && rank <= DraftClassConfig.DraftedPicksCount)
        {
            return profile.DepthBonus;
        }

        if (rank > DraftClassConfig.DraftedPicksCount)
        {
            return profile.DepthBonus / 2;
        }

        return 0;
    }

    private static int GetThemeModifier(DraftClassProfileData profile, string position)
    {
        if (position == "D")
        {
            return profile.DefenseQualityModifier;
        }

        if (position == "G")
        {
            return profile.GoalieQualityModifier;
        }

        return profile.ForwardQualityModifier;
    }

    private static List<string> CreatePositionPool(DraftClassProfileData profile)
    {
        GetPositionCounts(profile, out int forwardCount, out int defenseCount, out int goalieCount);
        List<string> positions = new List<string>();
        for (int i = 0; i < forwardCount; i++)
        {
            int forwardSlot = i % 3;
            positions.Add(forwardSlot == 0 ? "C" : forwardSlot == 1 ? "LW" : "RW");
        }

        for (int i = 0; i < defenseCount; i++)
        {
            positions.Add("D");
        }

        for (int i = 0; i < goalieCount; i++)
        {
            positions.Add("G");
        }

        DeterministicShuffle(positions, profile == null ? "draft-position-pool" : profile.ProfileId + ":positions");
        return positions;
    }

    private static void GetPositionCounts(DraftClassProfileData profile, out int forwards, out int defensemen, out int goalies)
    {
        string theme = profile == null ? DraftClassConfig.ThemeBalanced : profile.PositionalTheme;
        if (theme == DraftClassConfig.ThemeForwardHeavy)
        {
            forwards = 94;
            defensemen = 34;
            goalies = 12;
            return;
        }

        if (theme == DraftClassConfig.ThemeDefenseHeavy)
        {
            forwards = 76;
            defensemen = 52;
            goalies = 12;
            return;
        }

        if (theme == DraftClassConfig.ThemeGoalieHeavy)
        {
            forwards = 78;
            defensemen = 38;
            goalies = 24;
            return;
        }

        forwards = 84;
        defensemen = 42;
        goalies = 14;
    }

    private static int GenerateOverall(int rank, Random random)
    {
        if (rank <= 5)
        {
            return random.Next(67, 75);
        }

        if (rank <= 15)
        {
            return random.Next(63, 71);
        }

        if (rank <= 32)
        {
            return random.Next(60, 68);
        }

        if (rank <= 64)
        {
            return random.Next(56, 65);
        }

        if (rank <= 96)
        {
            return random.Next(52, 61);
        }

        return random.Next(48, 57);
    }

    private static int GeneratePotential(int rank, int overall, Random random)
    {
        int potential;
        if (rank <= 5)
        {
            potential = random.Next(85, 95);
        }
        else if (rank <= 15)
        {
            potential = random.Next(80, 91);
        }
        else if (rank <= 32)
        {
            potential = random.Next(76, 87);
        }
        else if (rank <= 64)
        {
            potential = random.Next(72, 83);
        }
        else if (rank <= 96)
        {
            potential = random.Next(68, 79);
        }
        else
        {
            potential = random.Next(62, 75);
        }

        return Math.Max(overall, potential);
    }

    private static int GenerateAge(Random random)
    {
        int roll = random.Next(0, 100);
        if (roll < 82)
        {
            return 18;
        }

        return roll < 94 ? 19 : 20;
    }

    private static void AssignRanksAndMetadata(List<ProspectData> prospects, DraftClassProfileData profile)
    {
        for (int i = 0; i < prospects.Count; i++)
        {
            ProspectData prospect = prospects[i];
            if (prospect == null)
            {
                continue;
            }

            int rank = i + 1;
            prospect.DraftRank = rank;
            prospect.ProjectedPick = rank;
            prospect.ProjectedRound = DraftClassConfig.GetProjectedRoundByRank(rank);
            prospect.ProjectedRoundNumber = GetRoundNumber(rank);
            ApplyProfileMetadata(prospect, profile);
        }
    }

    private static void ApplyProfileMetadata(ProspectData prospect, DraftClassProfileData profile)
    {
        if (prospect == null || profile == null)
        {
            return;
        }

        prospect.DraftClassStrengthType = profile.StrengthType;
        prospect.DraftClassDepthType = profile.DepthType;
        prospect.DraftClassPositionalTheme = profile.PositionalTheme;
    }

    private static int CompareProspectsForProjection(ProspectData left, ProspectData right)
    {
        int comparison = CalculateDraftQualityScore(right).CompareTo(CalculateDraftQualityScore(left));
        if (comparison != 0)
        {
            return comparison;
        }

        return string.Compare(left == null ? "" : left.Id, right == null ? "" : right.Id, StringComparison.Ordinal);
    }

    private static int CompareExistingProspectsForProjection(ProspectData left, ProspectData right)
    {
        int leftRank = left == null || left.DraftRank <= 0 ? 999 : left.DraftRank;
        int rightRank = right == null || right.DraftRank <= 0 ? 999 : right.DraftRank;
        if (leftRank != rightRank)
        {
            return leftRank.CompareTo(rightRank);
        }

        return CompareProspectsForProjection(left, right);
    }

    private static int CalculateDraftQualityScore(ProspectData prospect)
    {
        if (prospect == null)
        {
            return 0;
        }

        int ageBonus = prospect.Age <= 18 ? 6 : prospect.Age == 19 ? 3 : 0;
        int positionBonus = prospect.Position == "G" ? 2 : prospect.Position == "D" ? 1 : 0;
        return prospect.Potential * 2 + prospect.Overall + ageBonus + positionBonus;
    }

    private static int GetRoundNumber(int rank)
    {
        if (rank <= 0)
        {
            return 0;
        }

        if (rank <= DraftConfig.PicksPerRound)
        {
            return 1;
        }

        if (rank <= DraftConfig.PicksPerRound * 2)
        {
            return 2;
        }

        return rank <= DraftConfig.TotalDraftPicks ? 3 : 0;
    }

    private static void DeterministicShuffle(List<string> values, string seed)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int swapIndex = StableRange(seed + ":" + i, 0, i);
            string value = values[i];
            values[i] = values[swapIndex];
            values[swapIndex] = value;
        }
    }

    private static int CreateStableSeed(int draftYear, int index)
    {
        return StableHash("draft-prospect:" + draftYear + ":" + index);
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

    private static int Clamp(int value, int minValue, int maxValue)
    {
        if (value < minValue)
        {
            return minValue;
        }

        return value > maxValue ? maxValue : value;
    }
}
