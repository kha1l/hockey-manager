using System;
using System.Collections.Generic;

public static class ProspectRiskService
{
    public static void EnsureRiskProfilesForDraft(GameState state)
    {
        if (state == null || state.Draft == null || state.Draft.Prospects == null)
        {
            return;
        }

        for (int i = 0; i < state.Draft.Prospects.Count; i++)
        {
            ProspectData prospect = state.Draft.Prospects[i];
            int rank = GetProspectRank(prospect, i + 1);
            EnsureRiskProfile(prospect, rank);
        }
    }

    public static void EnsureRiskProfile(ProspectData prospect, int draftRank)
    {
        if (prospect == null)
        {
            return;
        }

        if (prospect.HasGeneratedRiskProfile)
        {
            ClampExistingProfile(prospect);
            RecalculateRiskHints(prospect);
            return;
        }

        string developmentType = DetermineDevelopmentType(prospect, draftRank);
        prospect.DevelopmentType = developmentType;
        prospect.HiddenCeiling = CalculateHiddenCeiling(prospect, developmentType, draftRank);
        prospect.HiddenFloor = CalculateHiddenFloor(prospect, developmentType, draftRank);
        prospect.DevelopmentRisk = CalculateDevelopmentRisk(prospect, developmentType, draftRank);
        prospect.BoomChance = CalculateBoomChance(prospect, developmentType, prospect.DevelopmentRisk);
        prospect.BustChance = CalculateBustChance(prospect, developmentType, prospect.DevelopmentRisk);
        prospect.HasGeneratedRiskProfile = true;
        ClampExistingProfile(prospect);
        RecalculateRiskHints(prospect);
    }

    public static void RecalculateRiskHints(ProspectData prospect)
    {
        if (prospect == null)
        {
            return;
        }

        if (!ProspectRiskConfig.IsValidDevelopmentType(prospect.DevelopmentType))
        {
            prospect.DevelopmentType = ProspectRiskConfig.DevelopmentTypeSafe;
        }

        prospect.CeilingHint = ProspectRiskConfig.GetCeilingHint(prospect.HiddenCeiling);
        prospect.FloorHint = ProspectRiskConfig.GetFloorHint(prospect.HiddenFloor);
        prospect.RiskHint = ProspectRiskConfig.GetRiskHint(prospect.DevelopmentRisk);
        prospect.DevelopmentTypeHint = ProspectRiskConfig.GetDevelopmentTypeHint(prospect.DevelopmentType);
    }

    public static ProspectRiskProfileData BuildRiskProfileData(ProspectData prospect)
    {
        if (prospect == null)
        {
            return new ProspectRiskProfileData();
        }

        EnsureRiskProfile(prospect, GetProspectRank(prospect, prospect.DraftPickOverall));
        return new ProspectRiskProfileData
        {
            ProspectId = prospect.Id,
            ProspectName = prospect.FirstName + " " + prospect.LastName,
            Position = prospect.Position,
            Age = prospect.Age,
            HiddenCeiling = prospect.HiddenCeiling,
            HiddenFloor = prospect.HiddenFloor,
            DevelopmentRisk = prospect.DevelopmentRisk,
            BoomChance = prospect.BoomChance,
            BustChance = prospect.BustChance,
            DevelopmentType = prospect.DevelopmentType,
            CeilingHint = prospect.CeilingHint,
            FloorHint = prospect.FloorHint,
            RiskHint = prospect.RiskHint,
            DevelopmentTypeHint = prospect.DevelopmentTypeHint,
            GeneratedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    public static string DetermineDevelopmentType(ProspectData prospect, int draftRank)
    {
        if (prospect == null)
        {
            return ProspectRiskConfig.DevelopmentTypeSafe;
        }

        int safeWeight = 30;
        int highFloorWeight = 20;
        int rawTalentWeight = 20;
        int lateBloomerWeight = 15;
        int boomBustWeight = 15;
        int growthRoom = prospect.Potential - prospect.Overall;

        if (prospect.Potential >= 88)
        {
            rawTalentWeight += 8;
            boomBustWeight += 6;
            safeWeight -= 6;
        }

        if (prospect.Overall >= 70 && prospect.Age <= 19)
        {
            safeWeight += 8;
            highFloorWeight += 8;
            boomBustWeight -= 5;
        }

        if (growthRoom >= 20)
        {
            rawTalentWeight += 8;
            boomBustWeight += 6;
            lateBloomerWeight += 4;
            highFloorWeight -= 5;
        }

        if (prospect.Position == "G")
        {
            lateBloomerWeight += 8;
            highFloorWeight -= 3;
        }

        ApplyArchetypeDevelopmentBias(
            prospect,
            ref safeWeight,
            ref highFloorWeight,
            ref rawTalentWeight,
            ref lateBloomerWeight,
            ref boomBustWeight);

        safeWeight = Math.Max(5, safeWeight);
        highFloorWeight = Math.Max(5, highFloorWeight);
        rawTalentWeight = Math.Max(5, rawTalentWeight);
        lateBloomerWeight = Math.Max(5, lateBloomerWeight);
        boomBustWeight = Math.Max(5, boomBustWeight);

        int totalWeight = safeWeight + highFloorWeight + rawTalentWeight + lateBloomerWeight + boomBustWeight;
        int roll = StableRange(GetSeed(prospect, draftRank) + ":type", 0, totalWeight - 1);
        if (roll < safeWeight)
        {
            return ProspectRiskConfig.DevelopmentTypeSafe;
        }

        roll -= safeWeight;
        if (roll < highFloorWeight)
        {
            return ProspectRiskConfig.DevelopmentTypeHighFloor;
        }

        roll -= highFloorWeight;
        if (roll < rawTalentWeight)
        {
            return ProspectRiskConfig.DevelopmentTypeRawTalent;
        }

        roll -= rawTalentWeight;
        return roll < lateBloomerWeight
            ? ProspectRiskConfig.DevelopmentTypeLateBloomer
            : ProspectRiskConfig.DevelopmentTypeBoomBust;
    }

    public static int CalculateHiddenCeiling(ProspectData prospect, string developmentType, int draftRank)
    {
        int bonus;
        string seed = GetSeed(prospect, draftRank) + ":ceiling";
        if (developmentType == ProspectRiskConfig.DevelopmentTypeBoomBust)
        {
            bonus = StableRange(seed, 3, 9);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeRawTalent)
        {
            bonus = StableRange(seed, 2, 7);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeLateBloomer)
        {
            bonus = StableRange(seed, 1, 5);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeHighFloor)
        {
            bonus = StableRange(seed, 0, 2);
        }
        else
        {
            bonus = StableRange(seed, 0, 3);
        }

        int minimum = prospect == null ? ProspectRiskConfig.MinCeiling : Math.Max(prospect.Potential, prospect.Overall);
        return ProspectRiskConfig.ClampCeiling(Math.Max(minimum, (prospect == null ? 0 : prospect.Potential) + bonus));
    }

    public static int CalculateHiddenFloor(ProspectData prospect, string developmentType, int draftRank)
    {
        int offset;
        string seed = GetSeed(prospect, draftRank) + ":floor";
        if (developmentType == ProspectRiskConfig.DevelopmentTypeHighFloor)
        {
            offset = StableRange(seed, 10, 18);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeSafe)
        {
            offset = StableRange(seed, 8, 15);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeLateBloomer)
        {
            offset = StableRange(seed, 4, 12);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeRawTalent)
        {
            offset = StableRange(seed, 0, 8);
        }
        else
        {
            offset = StableRange(seed, -2, 8);
        }

        int overall = prospect == null ? ProspectRiskConfig.MinFloor : prospect.Overall;
        int ceiling = prospect == null ? ProspectRiskConfig.MaxFloor : prospect.HiddenCeiling;
        int floor = ProspectRiskConfig.ClampFloor(overall + offset);
        return Math.Min(floor, Math.Min(ceiling, ProspectRiskConfig.MaxFloor));
    }

    public static int CalculateDevelopmentRisk(ProspectData prospect, string developmentType, int draftRank)
    {
        string seed = GetSeed(prospect, draftRank) + ":risk";
        int risk;
        if (developmentType == ProspectRiskConfig.DevelopmentTypeBoomBust)
        {
            risk = StableRange(seed, 60, 90);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeRawTalent)
        {
            risk = StableRange(seed, 45, 75);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeLateBloomer)
        {
            risk = StableRange(seed, 35, 60);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeHighFloor)
        {
            risk = StableRange(seed, 15, 35);
        }
        else
        {
            risk = StableRange(seed, 10, 30);
        }

        if (prospect != null && prospect.RiskLevel == "High")
        {
            risk += 5;
        }

        return ProspectRiskConfig.ClampRisk(risk);
    }

    public static int CalculateBoomChance(ProspectData prospect, string developmentType, int risk)
    {
        string seed = GetSeed(prospect, 0) + ":boom";
        int chance;
        if (developmentType == ProspectRiskConfig.DevelopmentTypeBoomBust)
        {
            chance = StableRange(seed, 18, 35);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeRawTalent)
        {
            chance = StableRange(seed, 12, 24);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeLateBloomer)
        {
            chance = StableRange(seed, 8, 16);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeHighFloor)
        {
            chance = StableRange(seed, 4, 10);
        }
        else
        {
            chance = StableRange(seed, 3, 8);
        }

        if (prospect != null && prospect.HiddenCeiling >= 90)
        {
            chance += 5;
        }

        return ProspectRiskConfig.ClampChance(chance);
    }

    public static int CalculateBustChance(ProspectData prospect, string developmentType, int risk)
    {
        string seed = GetSeed(prospect, 0) + ":bust";
        int chance;
        if (developmentType == ProspectRiskConfig.DevelopmentTypeBoomBust)
        {
            chance = StableRange(seed, 20, 40);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeRawTalent)
        {
            chance = StableRange(seed, 14, 28);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeLateBloomer)
        {
            chance = StableRange(seed, 8, 18);
        }
        else if (developmentType == ProspectRiskConfig.DevelopmentTypeHighFloor)
        {
            chance = StableRange(seed, 4, 10);
        }
        else
        {
            chance = StableRange(seed, 3, 8);
        }

        if (risk > 75)
        {
            chance += 5;
        }

        return ProspectRiskConfig.ClampChance(chance);
    }

    private static void ClampExistingProfile(ProspectData prospect)
    {
        prospect.HiddenCeiling = ProspectRiskConfig.ClampCeiling(Math.Max(prospect.HiddenCeiling, Math.Max(prospect.Potential, prospect.Overall)));
        prospect.HiddenFloor = ProspectRiskConfig.ClampFloor(prospect.HiddenFloor <= 0 ? Math.Max(ProspectRiskConfig.MinFloor, prospect.Overall - 3) : prospect.HiddenFloor);
        if (prospect.HiddenFloor > prospect.HiddenCeiling)
        {
            prospect.HiddenFloor = Math.Min(prospect.HiddenCeiling, ProspectRiskConfig.MaxFloor);
        }

        prospect.DevelopmentRisk = ProspectRiskConfig.ClampRisk(prospect.DevelopmentRisk);
        prospect.BoomChance = ProspectRiskConfig.ClampChance(prospect.BoomChance);
        prospect.BustChance = ProspectRiskConfig.ClampChance(prospect.BustChance);
    }

    private static string GetSeed(ProspectData prospect, int draftRank)
    {
        if (prospect == null)
        {
            return "missing-prospect:" + draftRank;
        }

        return prospect.Id + ":" + prospect.FirstName + ":" + prospect.LastName + ":" + prospect.Position + ":" + draftRank;
    }

    private static void ApplyArchetypeDevelopmentBias(
        ProspectData prospect,
        ref int safeWeight,
        ref int highFloorWeight,
        ref int rawTalentWeight,
        ref int lateBloomerWeight,
        ref int boomBustWeight)
    {
        if (prospect == null || string.IsNullOrEmpty(prospect.ProspectArchetype))
        {
            return;
        }

        string bias = ProspectArchetypeConfig.GetDevelopmentTypeBiasForArchetype(prospect.ProspectArchetype);
        if (bias == ProspectRiskConfig.DevelopmentTypeBoomBust)
        {
            boomBustWeight += 22;
            rawTalentWeight += 5;
            safeWeight -= 5;
            highFloorWeight -= 5;
        }
        else if (bias == ProspectRiskConfig.DevelopmentTypeRawTalent)
        {
            rawTalentWeight += 18;
            boomBustWeight += 6;
            safeWeight -= 4;
        }
        else if (bias == ProspectRiskConfig.DevelopmentTypeLateBloomer)
        {
            lateBloomerWeight += 18;
            rawTalentWeight += 4;
            highFloorWeight -= 4;
        }
        else if (bias == ProspectRiskConfig.DevelopmentTypeHighFloor)
        {
            highFloorWeight += 18;
            safeWeight += 8;
            rawTalentWeight -= 5;
            boomBustWeight -= 5;
        }
        else if (bias == ProspectRiskConfig.DevelopmentTypeSafe)
        {
            safeWeight += 18;
            boomBustWeight -= 5;
        }
    }

    private static int GetProspectRank(ProspectData prospect, int fallbackRank)
    {
        if (prospect == null)
        {
            return fallbackRank;
        }

        if (prospect.DraftRank > 0)
        {
            return prospect.DraftRank;
        }

        return prospect.ProjectedPick > 0 ? prospect.ProjectedPick : fallbackRank;
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
