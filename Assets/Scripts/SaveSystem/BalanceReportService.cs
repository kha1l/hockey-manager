using System;
using System.Collections.Generic;

public static class BalanceReportService
{
    public static BalanceReportData Generate(GameState state)
    {
        BalanceReportData report = new BalanceReportData
        {
            ReportId = Guid.NewGuid().ToString("N"),
            TeamsCount = state == null || state.Teams == null ? 0 : state.Teams.Count,
            PlayersCount = CountPlayers(state),
            AverageTeamOverall = CalculateAverageTeamOverall(state),
            AverageNhlRosterSize = CalculateAverageNhlRosterSize(state),
            InvalidRosterTeams = CountInvalidRosterTeams(state),
            InvalidLineupTeams = CountInvalidLineupTeams(state),
            CapViolationTeams = CountCapViolationTeams(state),
            AveragePayroll = CalculateAveragePayroll(state),
            AverageCapSpace = CalculateAverageCapSpace(state),
            FreeAgentsCount = CountFreeAgents(state),
            WaiverPlayersCount = CountWaiverPlayers(state),
            InjuredPlayersCount = CountInjuredPlayers(state),
            AverageMorale = CalculateAverageMorale(state),
            AverageChemistry = CalculateAverageChemistry(state),
            DraftClassSize = state == null || state.Draft == null || state.Draft.Prospects == null ? 0 : state.Draft.Prospects.Count,
            DraftClassSummary = BuildDraftClassSummary(state),
            NewsCount = state == null || state.NewsFeed == null || state.NewsFeed.Items == null ? 0 : state.NewsFeed.Items.Count,
            HistorySeasonsCount = state == null || state.LeagueHistory == null ? 0 : state.LeagueHistory.Count,
            RetiredPlayersCount = state == null || state.RetiredPlayers == null || state.RetiredPlayers.Players == null ? 0 : state.RetiredPlayers.Players.Count,
            HallOfFameInducteesCount = state == null || state.HallOfFame == null || state.HallOfFame.Inductees == null ? 0 : state.HallOfFame.Inductees.Count,
            RetiredNumbersCount = state == null || state.LeagueRetiredNumbers == null ? 0 : state.LeagueRetiredNumbers.Count,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        report.Summary = BuildSummary(report);
        if (state != null)
        {
            state.LastBalanceReport = report;
            state.LastStabilityCheckAtUtc = DateTime.UtcNow.ToString("o");
        }

        return report;
    }

    public static int CalculateAverageTeamOverall(GameState state)
    {
        int total = 0;
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            total += TeamRatingCalculator.CalculateOverall(team);
            count++;
        }

        return count == 0 ? 0 : total / count;
    }

    public static int CalculateAverageNhlRosterSize(GameState state)
    {
        int total = 0;
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            total += CountRoster(team, RosterStatusConfig.NHL);
            count++;
        }

        return count == 0 ? 0 : total / count;
    }

    public static int CountInvalidRosterTeams(GameState state)
    {
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            int nhl = CountRoster(team, RosterStatusConfig.NHL);
            if (nhl < RosterStatusConfig.MinNhlRosterSize || nhl > RosterStatusConfig.MaxNhlRosterSize)
            {
                count++;
            }
        }

        return count;
    }

    public static int CountInvalidLineupTeams(GameState state)
    {
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            if (!LineupService.ValidateLineup(team, out string _))
            {
                count++;
            }
        }

        return count;
    }

    public static int CountCapViolationTeams(GameState state)
    {
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
            if (finance != null && (finance.IsOverCap || finance.IsBelowFloor))
            {
                count++;
            }
        }

        return count;
    }

    public static int CalculateAveragePayroll(GameState state)
    {
        int total = 0;
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
            total += finance == null ? 0 : finance.Payroll;
            count++;
        }

        return count == 0 ? 0 : total / count;
    }

    public static int CalculateAverageCapSpace(GameState state)
    {
        int total = 0;
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
            total += finance == null ? 0 : finance.CapSpace;
            count++;
        }

        return count == 0 ? 0 : total / count;
    }

    public static int CountFreeAgents(GameState state)
    {
        return state == null || state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null
            ? 0
            : state.FreeAgentPool.FreeAgents.Count;
    }

    public static int CountWaiverPlayers(GameState state)
    {
        return state == null || state.WaiverWire == null || state.WaiverWire.ActiveWaivers == null
            ? 0
            : state.WaiverWire.ActiveWaivers.Count;
    }

    public static int CountInjuredPlayers(GameState state)
    {
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && player.IsInjured)
                {
                    count++;
                }
            }
        }

        return count;
    }

    public static int CalculateAverageMorale(GameState state)
    {
        int total = 0;
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && player.Morale > 0)
                {
                    total += player.Morale;
                    count++;
                }
            }
        }

        return count == 0 ? 0 : total / count;
    }

    public static int CalculateAverageChemistry(GameState state)
    {
        int total = 0;
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            if (team != null && team.Chemistry != null)
            {
                total += team.Chemistry.TeamChemistryScore;
                count++;
            }
        }

        return count == 0 ? 0 : total / count;
    }

    public static string BuildSummary(BalanceReportData report)
    {
        if (report == null)
        {
            return "Balance report unavailable";
        }

        if (report.InvalidRosterTeams == 0 && report.InvalidLineupTeams == 0 && report.CapViolationTeams == 0)
        {
            return "Balance looks stable: " + report.TeamsCount + " teams, 0 invalid lineups, average morale " + report.AverageMorale + ".";
        }

        return "Warnings: " + report.CapViolationTeams + " cap violations, "
            + report.InvalidRosterTeams + " invalid rosters, "
            + report.InvalidLineupTeams + " invalid lineups."
            + " History: retired " + report.RetiredPlayersCount
            + ", HOF " + report.HallOfFameInducteesCount
            + ", numbers " + report.RetiredNumbersCount + ".";
    }

    private static int CountPlayers(GameState state)
    {
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            count += team == null || team.Players == null ? 0 : team.Players.Count;
        }

        return count;
    }

    private static string BuildDraftClassSummary(GameState state)
    {
        if (state == null || state.Draft == null)
        {
            return "Draft not initialized";
        }

        int count = state.Draft.Prospects == null ? 0 : state.Draft.Prospects.Count;
        return count + " prospects"
            + " | rounds " + (state.Draft.TotalRounds == 0 ? DraftConfig.DraftRounds : state.Draft.TotalRounds)
            + " | status " + (string.IsNullOrEmpty(state.Draft.DraftStatus) ? "Unknown" : state.Draft.DraftStatus);
    }

    private static IEnumerable<TeamData> SafeTeams(GameState state)
    {
        return state == null || state.Teams == null ? new List<TeamData>() : state.Teams;
    }

    private static int CountRoster(TeamData team, string rosterStatus)
    {
        int count = 0;
        if (team == null || team.Players == null)
        {
            return count;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.RosterStatus == rosterStatus)
            {
                count++;
            }
        }

        return count;
    }
}
