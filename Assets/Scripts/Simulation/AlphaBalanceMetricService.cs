using System.Collections.Generic;

public static class AlphaBalanceMetricService
{
    public static List<AlphaBalanceMetricData> EvaluateSnapshot(AlphaBalanceSeasonSnapshotData snapshot)
    {
        List<AlphaBalanceMetricData> metrics = new List<AlphaBalanceMetricData>();
        if (snapshot == null)
        {
            return metrics;
        }

        metrics.Add(CreateRangeMetric("Match", "AverageGoalsPerGame", snapshot.AverageGoalsPerGameTimes100, AlphaBalanceConfig.TargetMinGoalsPerGame * 100, AlphaBalanceConfig.TargetMaxGoalsPerGame * 100, "Average total goals per game."));
        metrics.Add(CreateRangeMetric("Match", "AverageTeamPoints", snapshot.AverageTeamPoints, AlphaBalanceConfig.TargetMinAverageTeamPoints, AlphaBalanceConfig.TargetMaxAverageTeamPoints, "Average standings points across teams."));

        metrics.Add(CreateMaxMetric("Roster", "InvalidRosterTeams", snapshot.InvalidRosterTeams, AlphaBalanceConfig.TargetMaxInvalidRosterTeams, "Teams with invalid Pro roster size/positions/health."));
        metrics.Add(CreateMaxMetric("Roster", "InvalidLineupTeams", snapshot.InvalidLineupTeams, AlphaBalanceConfig.TargetMaxInvalidLineupTeams, "Teams with invalid lineups."));
        metrics.Add(CreateMaxMetric("Roster", "CapViolationTeams", snapshot.CapViolationTeams, AlphaBalanceConfig.TargetMaxCapViolationTeams, "Teams over cap or below floor."));

        metrics.Add(CreateRangeMetric("Market", "FreeAgentsCount", snapshot.FreeAgentsCount, AlphaBalanceConfig.TargetMinFreeAgentsRemainingAfterOffseason, AlphaBalanceConfig.TargetMaxFreeAgentsRemainingAfterOffseason, "Remaining free agents should stay usable but not bloated."));

        metrics.Add(CreateMaxMetric("Health", "InjuredPlayersCount", snapshot.InjuredPlayersCount, 32 * AlphaBalanceConfig.TargetMaxTotalInjuriesPerTeam, "Current injured players across the league."));
        metrics.Add(CreateMaxMetric("Health", "MajorInjuriesCount", snapshot.MajorInjuriesCount, 32 * AlphaBalanceConfig.TargetMaxMajorInjuriesPerTeam, "Current major or long-term injuries."));

        metrics.Add(CreateRangeMetric("Morale", "AverageMorale", snapshot.AverageMorale, AlphaBalanceConfig.TargetMinAverageMorale, AlphaBalanceConfig.TargetMaxAverageMorale, "League-wide average player morale."));
        metrics.Add(CreateRangeMetric("Chemistry", "AverageChemistry", snapshot.AverageChemistry, AlphaBalanceConfig.TargetMinAverageChemistry, AlphaBalanceConfig.TargetMaxAverageChemistry, "League-wide average team chemistry."));

        metrics.Add(CreateMaxMetric("Development", "PlayersOverall90Plus", snapshot.PlayersOverall90Plus, AlphaBalanceConfig.TargetMaxPlayersOverall90Plus, "Elite players should remain rare."));
        metrics.Add(CreateMaxMetric("Development", "PlayersOverall95Plus", snapshot.PlayersOverall95Plus, AlphaBalanceConfig.TargetMaxPlayersOverall95Plus, "Franchise-level players should remain very rare."));

        metrics.Add(CreateRangeMetric("Draft", "DraftClassSize", snapshot.DraftClassSize, AlphaBalanceConfig.TargetMinDraftClassSize, AlphaBalanceConfig.TargetMaxDraftClassSize, "Draft class size for the prototype."));

        metrics.Add(CreateMaxMetric("News", "NewsCount", snapshot.NewsCount, AlphaBalanceConfig.TargetMaxNewsItems, "News feed should not grow without bounds."));
        metrics.Add(CreateMinMetric("History", "LeagueHistorySeasonsCount", snapshot.LeagueHistorySeasonsCount, 0, "Informational history count."));
        metrics.Add(CreateMinMetric("GM", "GmJobSecurity", snapshot.GmJobSecurity, 15, "Warn if GM job security is too harsh."));
        metrics.Add(CreateMinMetric("Owner", "OwnerJobSecurity", string.IsNullOrEmpty(snapshot.OwnerJobSecurity) ? 0 : 1, 0, "Informational owner job security label: " + (string.IsNullOrEmpty(snapshot.OwnerJobSecurity) ? "none" : snapshot.OwnerJobSecurity)));

        return metrics;
    }

    public static AlphaBalanceMetricData CreateRangeMetric(
        string category,
        string name,
        int value,
        int minTarget,
        int maxTarget,
        string message)
    {
        string status = AlphaBalanceConfig.GetRangeStatus(value, minTarget, maxTarget);
        return new AlphaBalanceMetricData
        {
            Category = category,
            Name = name,
            Value = value,
            MinTarget = minTarget,
            MaxTarget = maxTarget,
            Status = status,
            Message = message,
            Passed = status == "OK"
        };
    }

    public static AlphaBalanceMetricData CreateMaxMetric(
        string category,
        string name,
        int value,
        int maxTarget,
        string message)
    {
        bool passed = value <= maxTarget;
        return new AlphaBalanceMetricData
        {
            Category = category,
            Name = name,
            Value = value,
            MinTarget = 0,
            MaxTarget = maxTarget,
            Status = passed ? "OK" : "HIGH",
            Message = message,
            Passed = passed
        };
    }

    public static AlphaBalanceMetricData CreateMinMetric(
        string category,
        string name,
        int value,
        int minTarget,
        string message)
    {
        bool passed = value >= minTarget;
        return new AlphaBalanceMetricData
        {
            Category = category,
            Name = name,
            Value = value,
            MinTarget = minTarget,
            MaxTarget = 0,
            Status = passed ? "OK" : "LOW",
            Message = message,
            Passed = passed
        };
    }
}
