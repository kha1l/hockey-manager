using System;
using System.Collections.Generic;

public static class ScoutingService
{
    public static void EnsureScouting(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureScoutingHistory();
        EnsureScoutingForDraft(state);
        TrimScoutingHistory(state);
    }

    public static void EnsureScoutingForDraft(GameState state)
    {
        DraftService.EnsureDraftClassProfile(state);
        ProspectRiskService.EnsureRiskProfilesForDraft(state);
        List<ProspectData> prospects = GetDraftClassProspects(state);
        for (int i = 0; i < prospects.Count; i++)
        {
            ProspectData prospect = prospects[i];
            int rank = GetProspectRank(prospect, i + 1);
            EnsureProspectScouting(prospect, rank);
        }
    }

    public static void EnsureProspectScouting(ProspectData prospect, int rank)
    {
        if (prospect == null)
        {
            return;
        }

        rank = GetProspectRank(prospect, rank);
        ProspectRiskService.EnsureRiskProfile(prospect, rank);

        if (prospect.ScoutingAccuracy <= 0)
        {
            prospect.ScoutingAccuracy = StableRange(
                prospect.Id + "-initial-scouting",
                ScoutingConfig.InitialAccuracyMin,
                ScoutingConfig.InitialAccuracyMax);
        }

        if (prospect.EstimatedOverallMin <= 0
            || prospect.EstimatedOverallMax <= 0
            || prospect.EstimatedPotentialMin <= 0
            || prospect.EstimatedPotentialMax <= 0)
        {
            RecalculateProspectEstimates(prospect, rank);
        }

        if (string.IsNullOrEmpty(prospect.ScoutingGrade)
            || string.IsNullOrEmpty(prospect.ProjectedRole)
            || string.IsNullOrEmpty(prospect.RiskLevel)
            || string.IsNullOrEmpty(prospect.DraftProjection)
            || string.IsNullOrEmpty(prospect.ScoutingSummary))
        {
            RecalculateProspectEstimates(prospect, rank);
        }

        prospect.IsFullyScouted = prospect.ScoutingAccuracy >= ScoutingConfig.FullyScoutedAccuracy;
    }

    public static void RecalculateProspectEstimates(ProspectData prospect, int rank)
    {
        if (prospect == null)
        {
            return;
        }

        prospect.ScoutingAccuracy = ScoutingConfig.ClampAccuracy(prospect.ScoutingAccuracy);
        prospect.IsFullyScouted = prospect.ScoutingAccuracy >= ScoutingConfig.FullyScoutedAccuracy;

        if (prospect.IsFullyScouted)
        {
            prospect.EstimatedOverallMin = ClampRating(prospect.Overall);
            prospect.EstimatedOverallMax = ClampRating(prospect.Overall);
            prospect.EstimatedPotentialMin = ClampRating(prospect.Potential);
            prospect.EstimatedPotentialMax = ClampRating(prospect.Potential);
        }
        else
        {
            int range = ScoutingConfig.GetEstimateRangeByAccuracy(prospect.ScoutingAccuracy);
            int noiseRange = Math.Max(1, range);
            int overallNoise = StableRange(prospect.Id + "-overall-" + prospect.ScoutingAccuracy, -noiseRange, noiseRange);
            int potentialNoise = StableRange(prospect.Id + "-potential-" + prospect.ScoutingAccuracy, -noiseRange, noiseRange);

            prospect.EstimatedOverallMin = ClampRating(prospect.Overall + overallNoise - range);
            prospect.EstimatedOverallMax = ClampRating(prospect.Overall + overallNoise + range);
            prospect.EstimatedPotentialMin = ClampRating(prospect.Potential + potentialNoise - range);
            prospect.EstimatedPotentialMax = ClampRating(prospect.Potential + potentialNoise + range);
        }

        if (prospect.EstimatedOverallMin > prospect.EstimatedOverallMax)
        {
            int value = prospect.EstimatedOverallMin;
            prospect.EstimatedOverallMin = prospect.EstimatedOverallMax;
            prospect.EstimatedOverallMax = value;
        }

        if (prospect.EstimatedPotentialMin > prospect.EstimatedPotentialMax)
        {
            int value = prospect.EstimatedPotentialMin;
            prospect.EstimatedPotentialMin = prospect.EstimatedPotentialMax;
            prospect.EstimatedPotentialMax = value;
        }

        int estimatedPotential = (prospect.EstimatedPotentialMin + prospect.EstimatedPotentialMax) / 2;
        prospect.ScoutingGrade = ScoutingConfig.GetGradeByPotential(estimatedPotential);
        prospect.RiskLevel = ScoutingConfig.GetRiskLevelByAccuracyAndPotential(prospect.ScoutingAccuracy, estimatedPotential);
        rank = GetProspectRank(prospect, rank);
        prospect.ProjectedRole = DetermineProjectedRole(prospect);
        prospect.ProjectedRound = string.IsNullOrEmpty(prospect.ProjectedRound)
            ? DraftClassConfig.GetProjectedRoundByRank(rank)
            : prospect.ProjectedRound;
        prospect.ProjectedRoundNumber = prospect.ProjectedRoundNumber > 0
            ? prospect.ProjectedRoundNumber
            : DraftClassConfig.GetProjectedRoundNumberByRank(rank);
        prospect.DraftProjection = DraftClassConfig.GetProjectedRoundByRank(rank);
        prospect.ScoutingSummary = BuildScoutingSummary(prospect);
    }

    public static ScoutingActionResultData ScoutProspect(GameState state, string prospectId)
    {
        EnsureScouting(state);
        ScoutingActionResultData result = CreateResult("ScoutPlayer");
        ProspectData prospect = FindProspect(state, prospectId);
        if (prospect == null)
        {
            result.Success = false;
            result.Message = "Проспект не найден";
            return result;
        }

        int rank = GetProspectRank(state, prospect);
        int accuracyBefore = prospect.ScoutingAccuracy;
        ApplyScoutingGain(prospect, ScoutingConfig.ScoutPlayerAccuracyGain, rank);
        ScoutingReportData report = CreateScoutingReport(prospect, accuracyBefore, "ScoutPlayer", rank);
        AddReport(state, report);
        result.CreatedReports.Add(report);
        result.Success = true;
        result.ProspectsScouted = 1;
        result.Message = "Проспект изучен: " + GetProspectName(prospect)
            + " (" + accuracyBefore + "% -> " + prospect.ScoutingAccuracy + "%)";
        RecordScoutingAction(state);
        return result;
    }

    public static ScoutingActionResultData ScoutTopProspects(GameState state)
    {
        EnsureScouting(state);
        ScoutingActionResultData result = CreateResult("ScoutTopProspects");
        List<ProspectData> prospects = GetDraftClassProspects(state);
        prospects.Sort(CompareProspectsByRank);
        int count = Math.Min(ScoutingConfig.TopProspectsScoutCount, prospects.Count);
        for (int i = 0; i < count; i++)
        {
            ScoutProspectForBatch(state, prospects[i], ScoutingConfig.ScoutTopProspectsAccuracyGain, "ScoutTopProspects", result);
        }

        result.Success = result.ProspectsScouted > 0;
        result.Message = result.Success
            ? "Изучены топ-проспекты: " + result.ProspectsScouted
            : "Драфт-класс пока не создан";
        if (result.Success)
        {
            RecordScoutingAction(state);
        }

        return result;
    }

    public static ScoutingActionResultData ScoutByPosition(GameState state, string position)
    {
        EnsureScouting(state);
        ScoutingActionResultData result = CreateResult("ScoutByPosition");
        List<ProspectData> prospects = GetDraftClassProspects(state);
        prospects.Sort(CompareProspectsByRank);

        foreach (ProspectData prospect in prospects)
        {
            if (prospect == null || !MatchesPosition(prospect, position))
            {
                continue;
            }

            ScoutProspectForBatch(state, prospect, ScoutingConfig.ScoutByPositionAccuracyGain, "ScoutByPosition", result);
            if (result.ProspectsScouted >= 12)
            {
                break;
            }
        }

        result.Success = result.ProspectsScouted > 0;
        result.Message = result.Success
            ? "Изучены проспекты по позиции " + position + ": " + result.ProspectsScouted
            : "Проспекты по позиции " + position + " не найдены";
        if (result.Success)
        {
            RecordScoutingAction(state);
        }

        return result;
    }

    public static ScoutingReportData CreateScoutingReport(
        ProspectData prospect,
        int accuracyBefore,
        string source,
        int rank)
    {
        EnsureProspectScouting(prospect, rank);
        return new ScoutingReportData
        {
            ReportId = Guid.NewGuid().ToString("N"),
            ProspectId = prospect == null ? "" : prospect.Id,
            ProspectName = prospect == null ? "" : GetProspectName(prospect),
            Position = prospect == null ? "" : prospect.Position,
            Age = prospect == null ? 0 : prospect.Age,
            AccuracyBefore = accuracyBefore,
            AccuracyAfter = prospect == null ? 0 : prospect.ScoutingAccuracy,
            EstimatedOverallMin = prospect == null ? 0 : prospect.EstimatedOverallMin,
            EstimatedOverallMax = prospect == null ? 0 : prospect.EstimatedOverallMax,
            EstimatedPotentialMin = prospect == null ? 0 : prospect.EstimatedPotentialMin,
            EstimatedPotentialMax = prospect == null ? 0 : prospect.EstimatedPotentialMax,
            ScoutingGrade = prospect == null ? "" : prospect.ScoutingGrade,
            ProjectedRole = prospect == null ? "" : prospect.ProjectedRole,
            RiskLevel = prospect == null ? "" : prospect.RiskLevel,
            DraftProjection = prospect == null ? "" : prospect.DraftProjection,
            ProspectArchetype = prospect == null ? "" : prospect.ProspectArchetype,
            CeilingHint = prospect == null ? "" : prospect.CeilingHint,
            FloorHint = prospect == null ? "" : prospect.FloorHint,
            DevelopmentTypeHint = prospect == null ? "" : prospect.DevelopmentTypeHint,
            RiskHint = prospect == null ? "" : prospect.RiskHint,
            Strengths = BuildStrengths(prospect),
            Weaknesses = BuildWeaknesses(prospect),
            Summary = BuildScoutingSummary(prospect),
            CreatedAtUtc = DateTime.UtcNow.ToString("o"),
            Source = source
        };
    }

    public static List<ProspectData> GetDraftClassProspects(GameState state)
    {
        if (state == null || state.Draft == null || state.Draft.Prospects == null)
        {
            return new List<ProspectData>();
        }

        return new List<ProspectData>(state.Draft.Prospects);
    }

    public static int GetProspectRank(ProspectData prospect, int fallbackRank)
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

    public static ProspectData FindProspect(GameState state, string prospectId)
    {
        if (string.IsNullOrEmpty(prospectId))
        {
            return null;
        }

        foreach (ProspectData prospect in GetDraftClassProspects(state))
        {
            if (prospect != null && prospect.Id == prospectId)
            {
                return prospect;
            }
        }

        return null;
    }

    public static string BuildScoutingSummary(ProspectData prospect)
    {
        if (prospect == null)
        {
            return "";
        }

        string overall = FormatRange(prospect.EstimatedOverallMin, prospect.EstimatedOverallMax);
        string potential = FormatRange(prospect.EstimatedPotentialMin, prospect.EstimatedPotentialMax);
        int rank = GetProspectRank(prospect, 999);
        string projectedRound = string.IsNullOrEmpty(prospect.ProjectedRound)
            ? DraftClassConfig.GetProjectedRoundByRank(rank)
            : prospect.ProjectedRound;
        string archetype = string.IsNullOrEmpty(prospect.ProspectArchetype)
            ? "Unknown Archetype"
            : prospect.ProspectArchetype;
        return "#" + rank
            + " | " + projectedRound
            + " | " + archetype
            + " | " + prospect.Position
            + " | OVR " + overall
            + " | POT " + potential
            + " | " + prospect.ProjectedRole
            + " | " + prospect.DevelopmentTypeHint
            + " | " + prospect.RiskHint;
    }

    public static string BuildStrengths(ProspectData prospect)
    {
        if (prospect == null)
        {
            return "";
        }

        if (prospect.CeilingHint == "Star upside" || prospect.CeilingHint == "Top-line upside")
        {
            return "High-end upside, strong projection";
        }

        if (prospect.FloorHint == "High floor" || prospect.DevelopmentType == ProspectRiskConfig.DevelopmentTypeHighFloor)
        {
            return "Reliable floor, safer projection";
        }

        if (prospect.EstimatedPotentialMax >= 88)
        {
            return "High-end tools, strong projection";
        }

        if (prospect.EstimatedOverallMin >= 70)
        {
            return "Closer to pro-ready, reliable baseline";
        }

        if (IsGoalie(prospect))
        {
            return "Goalie depth with development runway";
        }

        return "Useful tools for projected role";
    }

    public static string BuildWeaknesses(ProspectData prospect)
    {
        if (prospect == null)
        {
            return "";
        }

        if (prospect.ScoutingAccuracy < 45)
        {
            return "Limited viewings, wide projection range";
        }

        if (prospect.RiskHint == "High Risk" || prospect.RiskHint == "Very High Risk")
        {
            return "Development uncertainty, high variance projection";
        }

        if (prospect.FloorHint == "Bust risk")
        {
            return "Low floor, real bust concern";
        }

        if (prospect.RiskLevel == "High")
        {
            return "High variance scouting projection";
        }

        if (prospect.EstimatedOverallMax < 62)
        {
            return "Needs significant development time";
        }

        return "No major concern from current report";
    }

    public static string DetermineProjectedRole(ProspectData prospect)
    {
        if (prospect == null)
        {
            return "";
        }

        int potential = (prospect.EstimatedPotentialMin + prospect.EstimatedPotentialMax) / 2;
        if (IsGoalie(prospect))
        {
            if (potential >= 86)
            {
                return "Starting Goalie";
            }

            return potential >= 78 ? "Backup Goalie" : "Depth Goalie";
        }

        if (IsDefenseman(prospect))
        {
            if (potential >= 86)
            {
                return "Top Pair Defenseman";
            }

            if (potential >= 80)
            {
                return "Top 4 Defenseman";
            }

            return potential >= 74 ? "Third Pair Defenseman" : "Depth Defenseman";
        }

        if (potential >= 86)
        {
            return "Top 6 Forward";
        }

        if (potential >= 80)
        {
            return "Middle 6 Forward";
        }

        return potential >= 74 ? "Bottom 6 Forward" : "Depth Forward";
    }

    public static void AddReport(GameState state, ScoutingReportData report)
    {
        if (state == null || report == null)
        {
            return;
        }

        state.EnsureScoutingHistory();
        state.ScoutingHistory.Reports.Add(report);
        TrimScoutingHistory(state);
    }

    public static void TrimScoutingHistory(GameState state)
    {
        if (state == null || state.ScoutingHistory == null)
        {
            return;
        }

        state.ScoutingHistory.EnsureReports();
        while (state.ScoutingHistory.Reports.Count > ScoutingConfig.MaxReportsToKeep)
        {
            state.ScoutingHistory.Reports.RemoveAt(0);
        }
    }

    private static void ScoutProspectForBatch(
        GameState state,
        ProspectData prospect,
        int gain,
        string source,
        ScoutingActionResultData result)
    {
        if (prospect == null || result == null)
        {
            return;
        }

        int rank = GetProspectRank(state, prospect);
        int accuracyBefore = prospect.ScoutingAccuracy;
        ApplyScoutingGain(prospect, gain, rank);
        ScoutingReportData report = CreateScoutingReport(prospect, accuracyBefore, source, rank);
        AddReport(state, report);
        result.CreatedReports.Add(report);
        result.ProspectsScouted++;
    }

    private static void ApplyScoutingGain(ProspectData prospect, int gain, int rank)
    {
        prospect.ScoutingAccuracy = ScoutingConfig.ClampAccuracy(prospect.ScoutingAccuracy + gain);
        prospect.TimesScouted++;
        prospect.LastScoutedAtUtc = DateTime.UtcNow.ToString("o");
        RecalculateProspectEstimates(prospect, rank);
    }

    private static ScoutingActionResultData CreateResult(string actionType)
    {
        return new ScoutingActionResultData
        {
            ActionType = actionType,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    private static void RecordScoutingAction(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureScoutingHistory();
        state.ScoutingHistory.TotalScoutingActions++;
        state.ScoutingHistory.LastScoutingActionAtUtc = DateTime.UtcNow.ToString("o");
    }

    private static int GetProspectRank(GameState state, ProspectData prospect)
    {
        if (prospect == null)
        {
            return 999;
        }

        int rank = GetProspectRank(prospect, 0);
        if (rank > 0)
        {
            return rank;
        }

        List<ProspectData> prospects = GetDraftClassProspects(state);
        prospects.Sort(CompareProspectsByRank);
        int index = prospects.IndexOf(prospect);
        return index >= 0 ? index + 1 : 999;
    }

    private static int CompareProspectsByRank(ProspectData left, ProspectData right)
    {
        int leftRank = GetProspectRank(left, 999);
        int rightRank = GetProspectRank(right, 999);
        int comparison = leftRank.CompareTo(rightRank);
        if (comparison != 0)
        {
            return comparison;
        }

        int potentialComparison = (right == null ? 0 : right.Potential).CompareTo(left == null ? 0 : left.Potential);
        return potentialComparison != 0 ? potentialComparison : (right == null ? 0 : right.Overall).CompareTo(left == null ? 0 : left.Overall);
    }

    private static bool MatchesPosition(ProspectData prospect, string position)
    {
        if (prospect == null || string.IsNullOrEmpty(position))
        {
            return false;
        }

        if (position == "Forward")
        {
            return IsForward(prospect);
        }

        return prospect.Position == position;
    }

    private static bool IsForward(ProspectData prospect)
    {
        return prospect.Position == "C" || prospect.Position == "LW" || prospect.Position == "RW";
    }

    private static bool IsDefenseman(ProspectData prospect)
    {
        return prospect.Position == "D";
    }

    private static bool IsGoalie(ProspectData prospect)
    {
        return prospect.Position == "G";
    }

    private static int ClampRating(int value)
    {
        if (value < 40)
        {
            return 40;
        }

        return value > 99 ? 99 : value;
    }

    private static string GetProspectName(ProspectData prospect)
    {
        return prospect.FirstName + " " + prospect.LastName;
    }

    private static string FormatRange(int min, int max)
    {
        return min == max ? min.ToString() : min + "-" + max;
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
