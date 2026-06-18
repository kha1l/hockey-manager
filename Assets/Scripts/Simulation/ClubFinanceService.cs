using System;
using System.Collections.Generic;

public static class ClubFinanceService
{
    public static ClubFinanceData CalculateClubFinances(GameState state, TeamData team)
    {
        LeagueRulesData rules = state == null || state.LeagueRules == null
            ? LeagueRulesConfig.CreateDefaultRules()
            : state.LeagueRules;
        string direction = TeamDirectionService.DetermineTeamDirection(state, team);
        int payroll = CalculatePayroll(team);
        int starPower = CalculateStarPowerScore(team);
        int fanInterest = CalculateFanInterestScore(state, team, starPower, direction);
        int playoffRevenue = CalculatePlayoffRevenueEstimate(state, team);
        int budget = CalculateBudget(rules, direction, fanInterest);
        int revenue = CalculateRevenueEstimate(state, team, starPower, fanInterest, playoffRevenue);
        int expenses = CalculateExpensesEstimate(payroll);
        int profit = revenue - expenses;
        int healthScore = CalculateFinancialHealthScore(profit, payroll, fanInterest);

        return new ClubFinanceData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            Payroll = payroll,
            SalaryCapUpperLimit = rules.SalaryCapUpperLimit,
            SalaryCapSpace = rules.SalaryCapUpperLimit - payroll,
            Budget = budget,
            RevenueEstimate = revenue,
            ExpensesEstimate = expenses,
            ProfitEstimate = profit,
            PlayoffRevenueEstimate = playoffRevenue,
            StarPowerScore = starPower,
            FanInterestScore = fanInterest,
            FinancialHealthScore = healthScore,
            FinancialHealthLabel = GetFinancialHealthLabel(healthScore),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    public static ClubFinanceData EnsureClubFinances(GameState state, TeamData team)
    {
        ClubFinanceData finances = CalculateClubFinances(state, team);
        finances.FinanceSummary = BuildFinanceSummary(finances);

        if (team != null)
        {
            if (team.OwnerProfile == null)
            {
                team.OwnerProfile = new OwnerProfileData();
            }

            team.OwnerProfile.Finances = finances;
            team.OwnerProfile.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
        }

        return finances;
    }

    public static int CalculatePayroll(TeamData team)
    {
        return SalaryCapService.CalculatePayroll(team);
    }

    public static int CalculateBudget(LeagueRulesData rules, string teamDirection, int fanInterestScore)
    {
        int cap = rules == null || rules.SalaryCapUpperLimit <= 0
            ? SalaryCapConfig.SalaryCapUpperLimit
            : rules.SalaryCapUpperLimit;

        int budget = cap;
        if (teamDirection == TradeAiConfig.DirectionRebuild)
        {
            budget = cap * 92 / 100;
        }
        else if (teamDirection == TradeAiConfig.DirectionRetool)
        {
            budget = cap * 95 / 100;
        }
        else if (teamDirection == TradeAiConfig.DirectionBubbleTeam)
        {
            budget = cap * 98 / 100;
        }

        if (fanInterestScore >= 75)
        {
            budget += cap * 2 / 100;
        }
        else if (fanInterestScore < 45)
        {
            budget -= cap * 3 / 100;
        }

        return Clamp(budget, cap * 85 / 100, cap);
    }

    public static int CalculateRevenueEstimate(
        GameState state,
        TeamData team,
        int starPowerScore,
        int fanInterestScore,
        int playoffRevenueEstimate)
    {
        int revenue = 120000000;
        revenue += (fanInterestScore - 50) * 700000;
        revenue += (starPowerScore - 50) * 350000;
        revenue += playoffRevenueEstimate;

        TeamStandingData standing = FindStanding(state, team);
        if (standing != null && standing.GamesPlayed > 0)
        {
            int pointsPercentage = standing.Points * 100 / Math.Max(1, standing.GamesPlayed * 2);
            revenue += (pointsPercentage - 50) * 450000;
        }

        return Clamp(revenue, 80000000, 180000000);
    }

    public static int CalculateExpensesEstimate(int payroll)
    {
        return payroll + 45000000;
    }

    public static int CalculatePlayoffRevenueEstimate(GameState state, TeamData team)
    {
        if (state == null || team == null)
        {
            return 0;
        }

        int roundsWon = OwnerGoalProgressService.GetPlayoffRoundsWon(state, team);
        if (roundsWon > 0)
        {
            return roundsWon * 8000000;
        }

        if (OwnerGoalProgressService.DidTeamMakePlayoffs(state, team))
        {
            return 5000000;
        }

        TeamStandingData standing = FindStanding(state, team);
        if (standing != null && standing.GamesPlayed > 0)
        {
            int rank = OwnerGoalProgressService.GetLeagueRank(state, team);
            return rank > 0 && rank <= 16 ? 5000000 : 0;
        }

        return 0;
    }

    public static int CalculateStarPowerScore(TeamData team)
    {
        if (team == null)
        {
            return 40;
        }

        team.EnsurePlayers();
        int score = 35;
        foreach (PlayerData player in team.Players)
        {
            if (player == null || !RosterStatusConfig.IsNhlRoster(player))
            {
                continue;
            }

            if (player.Overall >= 90)
            {
                score += 16;
            }
            else if (player.Overall >= 85)
            {
                score += 10;
            }
            else if (player.Overall >= 80)
            {
                score += 4;
            }

            if (player.IsCaptain || player.IsAlternateCaptain)
            {
                score += 2;
            }
        }

        return Clamp(score, 0, 100);
    }

    public static int CalculateFanInterestScore(GameState state, TeamData team, int starPowerScore, string teamDirection)
    {
        int score = 45 + starPowerScore / 4;
        TeamStandingData standing = FindStanding(state, team);
        if (standing != null && standing.GamesPlayed > 0)
        {
            int pointsPercentage = standing.Points * 100 / Math.Max(1, standing.GamesPlayed * 2);
            score += (pointsPercentage - 50) / 2;
        }

        TeamMoraleSummaryData morale = MoraleService.BuildTeamMoraleSummary(state, team);
        if (morale != null)
        {
            score += (morale.AverageMorale - 50) / 6;
        }

        if (teamDirection == TradeAiConfig.DirectionRebuild)
        {
            score += CountYoungHighPotentialPlayers(team) * 3;
        }

        return Clamp(score, 0, 100);
    }

    public static int CalculateFinancialHealthScore(int profitEstimate, int payroll, int fanInterestScore)
    {
        int score = 55;
        score += profitEstimate / 2000000;
        score += (fanInterestScore - 50) / 2;

        if (payroll > SalaryCapConfig.SalaryCapUpperLimit)
        {
            score -= 20;
        }

        return Clamp(score, 0, 100);
    }

    public static string GetFinancialHealthLabel(int score)
    {
        if (score >= 85)
        {
            return "Excellent";
        }

        if (score >= 70)
        {
            return "Good";
        }

        if (score >= 50)
        {
            return "Stable";
        }

        if (score >= 35)
        {
            return "Warning";
        }

        return "Poor";
    }

    public static string BuildFinanceSummary(ClubFinanceData finances)
    {
        if (finances == null)
        {
            return "Финансы клуба недоступны";
        }

        return "Payroll " + FormatMoney(finances.Payroll)
            + " | Cap space " + FormatMoney(finances.SalaryCapSpace)
            + " | Budget " + FormatMoney(finances.Budget)
            + " | Profit est. " + FormatMoney(finances.ProfitEstimate)
            + " | Health " + finances.FinancialHealthLabel;
    }

    private static TeamStandingData FindStanding(GameState state, TeamData team)
    {
        if (state == null || state.Season == null || state.Season.Standings == null || team == null)
        {
            return null;
        }

        foreach (TeamStandingData standing in state.Season.Standings)
        {
            if (standing != null && standing.TeamId == team.Id)
            {
                return standing;
            }
        }

        return null;
    }

    private static int CountYoungHighPotentialPlayers(TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        int count = 0;
        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null
                && RosterStatusConfig.IsInOrganization(player)
                && player.Age <= 23
                && player.Potential >= 82)
            {
                count++;
            }
        }

        return count;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", " ");
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}
