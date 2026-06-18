using System;
using System.Collections.Generic;

public static class FreeAgentFitService
{
    public static FreeAgentFitEvaluationData EvaluateFit(
        GameState state,
        TeamData team,
        PlayerData player,
        int offeredSalary,
        int offeredYears)
    {
        EnsureEvaluationInputs(state, team, player);

        int teamFit = CalculateTeamFitScore(state, team, player);
        int contractFit = CalculateContractFitScore(state, player, offeredSalary, offeredYears);
        int roleFit = CalculateRoleFitScore(team, player);
        int rosterOpportunity = CalculateRosterOpportunityScore(team, player);
        int contender = CalculateContenderScore(state, team, player);
        int coaching = CalculateCoachingFitScore(team, player);
        int capFit = CalculateCapFitScore(state, team, offeredSalary);

        int weighted = teamFit * 20
            + contractFit * 30
            + roleFit * 20
            + rosterOpportunity * 15
            + contender * 10
            + coaching * 5;
        int finalScore = weighted / 100;

        if (capFit < 40)
        {
            finalScore -= 30;
        }
        else if (capFit < 60)
        {
            finalScore -= 12;
        }
        else if (capFit >= 85)
        {
            finalScore += 4;
        }

        finalScore = BetterFreeAgencyConfig.ClampInterest(finalScore);

        return new FreeAgentFitEvaluationData
        {
            PlayerId = player == null ? "" : player.Id,
            PlayerName = GetPlayerName(player),
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            TeamFitScore = teamFit,
            ContractFitScore = contractFit,
            RoleFitScore = roleFit,
            RosterOpportunityScore = rosterOpportunity,
            ContenderScore = contender,
            CoachingFitScore = coaching,
            CapFitScore = capFit,
            FinalInterestScore = finalScore,
            BestProjectedRole = DetermineProjectedRole(team, player),
            Summary = BuildFitSummary(team, player, finalScore),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    public static int CalculateTeamFitScore(GameState state, TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return 0;
        }

        TeamNeedData needs = GetTeamNeeds(state, team);
        if (needs == null)
        {
            return 50;
        }

        int score = 45;
        if (TradeAiConfig.IsGoalie(player))
        {
            score += needs.NeedGoalie / 2;
        }
        else if (TradeAiConfig.IsDefenseman(player))
        {
            score += needs.NeedDefenseman / 2;
        }
        else if (TradeAiConfig.IsForward(player))
        {
            int forwardNeed = player.Overall >= 78 ? needs.NeedTop6Forward : needs.NeedBottom6Forward;
            score += forwardNeed / 2;
        }

        if (player.Age <= 24)
        {
            score += needs.NeedYoungPlayers / 5;
        }
        else if (player.Age >= 30)
        {
            score += needs.NeedVeteranHelp / 5;
        }

        TeamTradeProfileData profile = GetTeamTradeProfile(state, team);
        if (profile != null && profile.RosterPressureScore >= TradeAiConfig.RosterPressureHighThreshold)
        {
            score -= 15;
        }

        return BetterFreeAgencyConfig.ClampInterest(score);
    }

    public static int CalculateContractFitScore(GameState state, PlayerData player, int offeredSalary, int offeredYears)
    {
        if (player == null)
        {
            return 0;
        }

        EnsurePlayerAsk(state, player);
        int expectedSalary = Math.Max(BetterFreeAgencyConfig.GetLeagueMinimumSalary(GetRules(state)), player.FreeAgencyExpectedSalary);
        int minimumSalary = Math.Max(BetterFreeAgencyConfig.GetLeagueMinimumSalary(GetRules(state)), player.FreeAgencyMinimumSalary);
        int expectedYears = Math.Max(1, player.FreeAgencyExpectedYears);

        int score;
        if (offeredSalary >= expectedSalary)
        {
            score = 82 + Math.Min(13, (offeredSalary - expectedSalary) / 250000);
        }
        else if (offeredSalary >= minimumSalary)
        {
            int range = Math.Max(1, expectedSalary - minimumSalary);
            score = 55 + (offeredSalary - minimumSalary) * 25 / range;
        }
        else
        {
            score = 15 + Math.Max(0, offeredSalary - BetterFreeAgencyConfig.GetLeagueMinimumSalary(GetRules(state))) * 30 / Math.Max(1, minimumSalary);
        }

        int yearDelta = Math.Abs(offeredYears - expectedYears);
        if (yearDelta == 0)
        {
            score += 8;
        }
        else if (yearDelta == 1)
        {
            score += 2;
        }
        else
        {
            score -= yearDelta * 5;
        }

        return BetterFreeAgencyConfig.ClampInterest(score);
    }

    public static int CalculateRoleFitScore(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return 0;
        }

        string role = DetermineProjectedRole(team, player);
        if (role == "Starter" || role == "Top6" || role == "TopPair")
        {
            return player.Overall >= 78 ? 88 : 72;
        }

        if (role == "Backup" || role == "Bottom6" || role == "ThirdPair")
        {
            return player.Overall >= 84 ? 58 : 72;
        }

        return player.Overall >= 80 ? 35 : 55;
    }

    public static int CalculateRosterOpportunityScore(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return 0;
        }

        TeamRosterService.EnsureRosterStatusesForTeam(team);
        int nhlCount = TeamRosterService.GetNhlPlayers(team).Count;
        int score = nhlCount < RosterStatusConfig.MaxNhlRosterSize ? 65 : 35;

        int forwards = 0;
        int defense = 0;
        int goalies = 0;
        foreach (PlayerData rosterPlayer in TeamRosterService.GetNhlPlayers(team))
        {
            if (TradeAiConfig.IsForward(rosterPlayer))
            {
                forwards++;
            }
            else if (TradeAiConfig.IsDefenseman(rosterPlayer))
            {
                defense++;
            }
            else if (TradeAiConfig.IsGoalie(rosterPlayer))
            {
                goalies++;
            }
        }

        if (TradeAiConfig.IsForward(player) && forwards < CpuRosterManagementConfig.PreferredForwards)
        {
            score += 20;
        }
        else if (TradeAiConfig.IsDefenseman(player) && defense < CpuRosterManagementConfig.PreferredDefensemen)
        {
            score += 20;
        }
        else if (TradeAiConfig.IsGoalie(player) && goalies < CpuRosterManagementConfig.PreferredGoalies)
        {
            score += 25;
        }

        if (nhlCount >= RosterStatusConfig.MaxNhlRosterSize && player.Overall < 76)
        {
            score -= 15;
        }

        return BetterFreeAgencyConfig.ClampInterest(score);
    }

    public static int CalculateContenderScore(GameState state, TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return 50;
        }

        string direction = TeamDirectionService.DetermineTeamDirection(state, team);
        if (direction == TradeAiConfig.DirectionContender)
        {
            return player.Age >= 29 || player.Overall >= 82 ? 85 : 65;
        }

        if (direction == TradeAiConfig.DirectionPlayoffTeam)
        {
            return 72;
        }

        if (direction == TradeAiConfig.DirectionBubbleTeam)
        {
            return 58;
        }

        if (direction == TradeAiConfig.DirectionRetool)
        {
            return player.Age <= 26 ? 64 : 42;
        }

        return player.Age <= 25 ? 62 : 35;
    }

    public static int CalculateCoachingFitScore(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return 50;
        }

        CoachingStaffService.EnsureStaffForTeam(team);
        int score = 55;
        score += CoachingStaffService.GetMoraleModifier(team) * 6;
        score += CoachingStaffService.GetChemistryModifier(team) * 4;

        if (player.Age <= 24)
        {
            score += CoachingStaffService.GetDevelopmentModifier(team, player) * 6;
        }

        if (player.Position == "G")
        {
            score += CoachingStaffService.GetGoalieDevelopmentModifier(team, player) * 5;
        }

        return BetterFreeAgencyConfig.ClampInterest(score);
    }

    public static int CalculateCapFitScore(GameState state, TeamData team, int offeredSalary)
    {
        if (team == null)
        {
            return 0;
        }

        LeagueRulesData rules = GetRules(state);
        int payroll = SalaryCapService.CalculatePayroll(team);
        int cap = rules.SalaryCapUpperLimit > 0 ? rules.SalaryCapUpperLimit : SalaryCapConfig.SalaryCapUpperLimit;
        int capSpace = cap - payroll;
        if (offeredSalary <= 0)
        {
            return 50;
        }

        if (capSpace < offeredSalary)
        {
            return capSpace <= 0 ? 0 : 25;
        }

        int remainingAfter = capSpace - offeredSalary;
        if (remainingAfter >= cap / 10)
        {
            return 95;
        }

        if (remainingAfter >= cap / 20)
        {
            return 78;
        }

        return 62;
    }

    public static string DetermineProjectedRole(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return "Depth";
        }

        TeamRosterService.EnsureRosterStatusesForTeam(team);
        if (player.Position == "G")
        {
            int goalieRank = GetProjectedRank(team, player, "G");
            if (goalieRank <= 1)
            {
                return "Starter";
            }

            return goalieRank <= 2 ? "Backup" : "Depth";
        }

        if (player.Position == "D")
        {
            int defenseRank = GetProjectedRank(team, player, "D");
            if (defenseRank <= 2)
            {
                return "TopPair";
            }

            return defenseRank <= 6 ? "ThirdPair" : "Depth";
        }

        int forwardRank = GetProjectedRank(team, player, "F");
        if (forwardRank <= 6)
        {
            return "Top6";
        }

        return forwardRank <= 12 ? "Bottom6" : "Depth";
    }

    public static string BuildFitSummary(TeamData team, PlayerData player, int score)
    {
        if (team == null || player == null)
        {
            return "Fit недоступен";
        }

        return GetTeamName(team)
            + ": " + BetterFreeAgencyConfig.GetInterestLabel(score)
            + " interest (" + score + ")"
            + " | роль " + DetermineProjectedRole(team, player);
    }

    private static void EnsureEvaluationInputs(GameState state, TeamData team, PlayerData player)
    {
        if (state != null && state.LeagueRules == null)
        {
            state.LeagueRules = LeagueRulesConfig.CreateDefaultRules();
        }

        TeamRosterService.EnsureRosterStatusesForTeam(team);
        PlayerRoleService.EnsureRole(player);
        EnsurePlayerAsk(state, player);
    }

    private static void EnsurePlayerAsk(GameState state, PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        if (player.FreeAgencyExpectedSalary <= 0)
        {
            player.FreeAgencyExpectedSalary = FreeAgentValuationService.CalculateExpectedSalary(state, player);
        }

        if (player.FreeAgencyMinimumSalary <= 0)
        {
            player.FreeAgencyMinimumSalary = FreeAgentValuationService.CalculateMinimumSalary(state, player);
        }

        if (player.FreeAgencyExpectedYears <= 0)
        {
            player.FreeAgencyExpectedYears = FreeAgentValuationService.CalculateExpectedYears(state, player);
        }

        if (player.FreeAgencyPreferredYears <= 0)
        {
            player.FreeAgencyPreferredYears = FreeAgentValuationService.CalculatePreferredYears(state, player);
        }
    }

    private static int GetProjectedRank(TeamData team, PlayerData player, string group)
    {
        List<PlayerData> players = new List<PlayerData>();
        foreach (PlayerData rosterPlayer in TeamRosterService.GetNhlPlayers(team))
        {
            if (IsInGroup(rosterPlayer, group))
            {
                players.Add(rosterPlayer);
            }
        }

        players.Add(player);
        players.Sort(ComparePlayersForRole);
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == player || players[i] != null && players[i].Id == player.Id)
            {
                return i + 1;
            }
        }

        return players.Count;
    }

    private static bool IsInGroup(PlayerData player, string group)
    {
        if (group == "G")
        {
            return TradeAiConfig.IsGoalie(player);
        }

        if (group == "D")
        {
            return TradeAiConfig.IsDefenseman(player);
        }

        return TradeAiConfig.IsForward(player);
    }

    private static int ComparePlayersForRole(PlayerData left, PlayerData right)
    {
        int overallComparison = (right == null ? 0 : right.Overall).CompareTo(left == null ? 0 : left.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        int potentialComparison = (right == null ? 0 : right.Potential).CompareTo(left == null ? 0 : left.Potential);
        if (potentialComparison != 0)
        {
            return potentialComparison;
        }

        return (left == null ? 99 : left.Age).CompareTo(right == null ? 99 : right.Age);
    }

    private static TeamNeedData GetTeamNeeds(GameState state, TeamData team)
    {
        return TeamNeedService.GetTeamNeeds(state, team);
    }

    private static TeamTradeProfileData GetTeamTradeProfile(GameState state, TeamData team)
    {
        return TeamTradeProfileService.GetTradeProfile(state, team == null ? "" : team.Id);
    }

    private static LeagueRulesData GetRules(GameState state)
    {
        return state != null && state.LeagueRules != null ? state.LeagueRules : LeagueRulesConfig.CreateDefaultRules();
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : player.FirstName + " " + player.LastName;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
