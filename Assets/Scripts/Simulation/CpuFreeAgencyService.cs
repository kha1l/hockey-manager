using System.Collections.Generic;

public static class CpuFreeAgencyService
{
    public static List<FreeAgentOfferData> RunCpuFreeAgency(GameState state, string userTeamId, int maxSignings)
    {
        List<FreeAgentOfferData> offers = new List<FreeAgentOfferData>();
        if (state == null || state.Teams == null || maxSignings <= 0)
        {
            return offers;
        }

        if (!LeaguePhaseService.IsFreeAgencyOpen(state))
        {
            return offers;
        }

        BetterFreeAgencyService.EnsureFreeAgentEvaluations(state);
        int acceptedCount = 0;
        foreach (TeamData team in state.Teams)
        {
            if (team == null || IsUserTeam(team, userTeamId))
            {
                continue;
            }

            FreeAgentOfferData offer = TrySignBestFreeAgentForTeam(state, team);
            if (offer == null)
            {
                continue;
            }

            offers.Add(offer);
            if (offer.Accepted)
            {
                acceptedCount++;
                CpuRosterManagementService.RunForTeams(
                    state,
                    new List<TeamData> { team },
                    userTeamId,
                    "AfterCpuFreeAgentSigning");
            }

            if (acceptedCount >= maxSignings)
            {
                break;
            }
        }

        if (acceptedCount > 0)
        {
            TeamTradeProfileService.EnsureTradeProfiles(state);
        }

        return offers;
    }

    public static FreeAgentOfferData TrySignBestFreeAgentForTeam(GameState state, TeamData team)
    {
        PlayerData target = FindBestFreeAgentTarget(state, team);
        if (target == null)
        {
            return null;
        }

        int salary = CalculateCpuOfferSalary(state, team, target);
        int years = CalculateCpuOfferYears(state, team, target);
        FreeAgentFitEvaluationData evaluation = FreeAgentFitService.EvaluateFit(state, team, target, salary, years);
        if (!ShouldCpuTeamSignFreeAgent(state, team, target, evaluation, salary))
        {
            return null;
        }

        return BetterFreeAgencyService.MakeCpuFreeAgentOffer(state, team, target, salary, years);
    }

    public static PlayerData FindBestFreeAgentTarget(GameState state, TeamData team)
    {
        if (state == null || team == null)
        {
            return null;
        }

        List<PlayerData> freeAgents = BetterFreeAgencyService.GetFreeAgents(state);
        PlayerData bestPlayer = null;
        int bestScore = 0;

        foreach (PlayerData player in freeAgents)
        {
            if (player == null || player.ContractStatus != "UFA")
            {
                continue;
            }

            int salary = CalculateCpuOfferSalary(state, team, player);
            int years = CalculateCpuOfferYears(state, team, player);
            FreeAgentFitEvaluationData evaluation = FreeAgentFitService.EvaluateFit(state, team, player, salary, years);
            int acceptanceScore = BetterFreeAgencyService.CalculateAcceptanceScore(state, team, player, salary, years);
            int score = evaluation.FinalInterestScore + acceptanceScore + player.Overall / 2;
            if (!ShouldCpuTeamSignFreeAgent(state, team, player, evaluation, salary))
            {
                continue;
            }

            if (bestPlayer == null || score > bestScore)
            {
                bestPlayer = player;
                bestScore = score;
            }
        }

        return bestPlayer;
    }

    public static bool ShouldCpuTeamSignFreeAgent(
        GameState state,
        TeamData team,
        PlayerData player,
        FreeAgentFitEvaluationData evaluation,
        int salary)
    {
        if (state == null || team == null || player == null || evaluation == null)
        {
            return false;
        }

        if (!LeaguePhaseService.IsFreeAgencyOpen(state))
        {
            return false;
        }

        if (evaluation.FinalInterestScore < BetterFreeAgencyConfig.CpuSigningThreshold)
        {
            return false;
        }

        if (TeamRosterService.GetNhlPlayers(team).Count < RosterStatusConfig.MaxNhlRosterSize
            && !SalaryCapService.CanAddSalary(team, salary))
        {
            return false;
        }

        TeamNeedData needs = TeamNeedService.GetTeamNeeds(state, team);
        bool positionNeed = HasPositionNeed(needs, player);
        bool rosterSpot = TeamRosterService.GetNhlPlayers(team).Count < RosterStatusConfig.MaxNhlRosterSize;
        bool highImpact = player.Overall >= 80 && evaluation.FinalInterestScore >= 78;
        return positionNeed || rosterSpot && player.Overall >= 74 || highImpact;
    }

    public static int CalculateCpuOfferSalary(GameState state, TeamData team, PlayerData player)
    {
        if (player == null)
        {
            return BetterFreeAgencyConfig.GetLeagueMinimumSalary(GetRules(state));
        }

        BetterFreeAgencyService.EnsureFreeAgentEvaluation(state, FindUserTeam(state), player);
        int salary = player.FreeAgencyExpectedSalary;
        TeamNeedData needs = TeamNeedService.GetTeamNeeds(state, team);
        if (HasPositionNeed(needs, player))
        {
            salary = salary * 105 / 100;
        }

        if (SalaryCapService.CalculateCapSpace(team) < salary)
        {
            salary = player.FreeAgencyMinimumSalary;
        }

        return BetterFreeAgencyConfig.ClampSalary(salary, GetRules(state));
    }

    public static int CalculateCpuOfferYears(GameState state, TeamData team, PlayerData player)
    {
        if (player == null)
        {
            return BetterFreeAgencyConfig.MinFreeAgentYears;
        }

        BetterFreeAgencyService.EnsureFreeAgentEvaluation(state, FindUserTeam(state), player);
        int years = player.FreeAgencyExpectedYears;
        string direction = TeamDirectionService.DetermineTeamDirection(state, team);
        if (direction == TradeAiConfig.DirectionRebuild && player.Age >= 31)
        {
            years--;
        }
        else if ((direction == TradeAiConfig.DirectionContender || direction == TradeAiConfig.DirectionPlayoffTeam)
            && player.Overall >= 82
            && player.Age <= 30)
        {
            years++;
        }

        return BetterFreeAgencyConfig.ClampFreeAgentYears(years, GetRules(state));
    }

    private static bool IsUserTeam(TeamData team, string userTeamId)
    {
        return team != null && !string.IsNullOrEmpty(userTeamId) && team.Id == userTeamId;
    }

    private static bool HasPositionNeed(TeamNeedData needs, PlayerData player)
    {
        if (needs == null || player == null)
        {
            return false;
        }

        if (TradeAiConfig.IsGoalie(player))
        {
            return needs.NeedGoalie >= TradeAiConfig.NeedMedium;
        }

        if (TradeAiConfig.IsDefenseman(player))
        {
            return needs.NeedDefenseman >= TradeAiConfig.NeedMedium;
        }

        if (TradeAiConfig.IsForward(player))
        {
            return player.Overall >= 78
                ? needs.NeedTop6Forward >= TradeAiConfig.NeedMedium
                : needs.NeedBottom6Forward >= TradeAiConfig.NeedMedium;
        }

        return false;
    }

    private static TeamData FindUserTeam(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == state.SelectedTeamId)
            {
                return team;
            }
        }

        return null;
    }

    private static LeagueRulesData GetRules(GameState state)
    {
        return state != null && state.LeagueRules != null ? state.LeagueRules : LeagueRulesConfig.CreateDefaultRules();
    }
}
