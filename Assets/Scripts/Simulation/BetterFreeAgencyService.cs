using System;
using System.Collections.Generic;

public static class BetterFreeAgencyService
{
    public static void EnsureFreeAgencyOfferHistory(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureFreeAgencyOfferHistory();
    }

    public static void EnsureFreeAgentEvaluations(GameState state)
    {
        if (state == null)
        {
            return;
        }

        EnsureFreeAgencyOfferHistory(state);
        EnsureFreeAgentPool(state);
        if (state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null)
        {
            return;
        }

        TeamData userTeam = FindTeam(state.Teams, state.SelectedTeamId);
        foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
        {
            if (player != null && player.IsRetired)
            {
                continue;
            }

            EnsureFreeAgentEvaluation(state, userTeam, player);
        }
    }

    public static void EnsureFreeAgentEvaluation(GameState state, TeamData userTeam, PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        LeagueRulesData rules = GetRules(state);
        player.FreeAgencyExpectedSalary = FreeAgentValuationService.CalculateExpectedSalary(state, player);
        player.FreeAgencyMinimumSalary = FreeAgentValuationService.CalculateMinimumSalary(state, player);
        player.FreeAgencyExpectedYears = FreeAgentValuationService.CalculateExpectedYears(state, player);
        player.FreeAgencyPreferredYears = FreeAgentValuationService.CalculatePreferredYears(state, player);
        player.FreeAgencyAskSummary = FreeAgentValuationService.BuildAskSummary(player);

        FreeAgentFitEvaluationData userEvaluation = userTeam == null
            ? null
            : FreeAgentFitService.EvaluateFit(
                state,
                userTeam,
                player,
                player.FreeAgencyExpectedSalary,
                player.FreeAgencyExpectedYears);

        if (userEvaluation != null)
        {
            player.FreeAgencyInterestInUserTeam = userEvaluation.FinalInterestScore;
            player.FreeAgencyInterestSummary = userEvaluation.Summary;
        }
        else
        {
            player.FreeAgencyInterestInUserTeam = 0;
            player.FreeAgencyInterestSummary = "Команда пользователя не выбрана";
        }

        FreeAgentFitEvaluationData bestFit = FindBestFit(state, player);
        if (bestFit != null)
        {
            player.FreeAgencyBestFitRole = bestFit.BestProjectedRole;
            player.FreeAgencyBestFitTeamId = bestFit.TeamId;
            player.FreeAgencyBestFitTeamName = bestFit.TeamName;
        }

        player.HasFreeAgencyEvaluation = true;
        if (string.IsNullOrEmpty(player.LastFreeAgencyOfferStatus))
        {
            player.LastFreeAgencyOfferStatus = "None";
        }

        player.Salary = BetterFreeAgencyConfig.ClampSalary(
            player.Salary <= 0 ? player.FreeAgencyExpectedSalary : player.Salary,
            rules);
        player.ContractYearsRemaining = BetterFreeAgencyConfig.ClampFreeAgentYears(
            player.ContractYearsRemaining <= 0 ? player.FreeAgencyExpectedYears : player.ContractYearsRemaining,
            rules);
    }

    public static List<PlayerData> GetFreeAgents(GameState state)
    {
        EnsureFreeAgentEvaluations(state);
        List<PlayerData> freeAgents = state == null || state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null
            ? new List<PlayerData>()
            : new List<PlayerData>(state.FreeAgentPool.FreeAgents);

        freeAgents.RemoveAll(player => player == null || player.IsRetired);
        freeAgents.Sort(CompareFreeAgents);
        return freeAgents;
    }

    public static FreeAgencyMarketSummaryData BuildMarketSummary(GameState state)
    {
        EnsureFreeAgentEvaluations(state);
        TeamData userTeam = FindTeam(state == null ? null : state.Teams, state == null ? "" : state.SelectedTeamId);
        TeamFinanceData finance = userTeam == null ? null : SalaryCapService.CalculateTeamFinance(userTeam);
        List<PlayerData> freeAgents = GetFreeAgents(state);

        FreeAgencyMarketSummaryData summary = new FreeAgencyMarketSummaryData
        {
            UserTeamCapSpace = finance == null ? 0 : finance.CapSpace,
            UserTeamNhlRosterSpots = userTeam == null ? 0 : Math.Max(0, RosterStatusConfig.MaxNhlRosterSize - TeamRosterService.GetNhlPlayers(userTeam).Count),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        foreach (PlayerData player in freeAgents)
        {
            if (player == null)
            {
                continue;
            }

            summary.TotalFreeAgents++;
            if (player.Overall >= 82)
            {
                summary.TopPlayersAvailable++;
            }

            if (TradeAiConfig.IsGoalie(player))
            {
                summary.GoaliesAvailable++;
            }
            else if (TradeAiConfig.IsDefenseman(player))
            {
                summary.DefensemenAvailable++;
            }
            else if (TradeAiConfig.IsForward(player))
            {
                summary.ForwardsAvailable++;
            }

            if (string.IsNullOrEmpty(summary.BestAvailablePlayerId))
            {
                summary.BestAvailablePlayerId = player.Id;
                summary.BestAvailablePlayerName = GetPlayerName(player);
            }
        }

        summary.Summary = "FA: " + summary.TotalFreeAgents
            + " | Top: " + summary.TopPlayersAvailable
            + " | F/D/G: " + summary.ForwardsAvailable + "/" + summary.DefensemenAvailable + "/" + summary.GoaliesAvailable
            + " | Best: " + (string.IsNullOrEmpty(summary.BestAvailablePlayerName) ? "none" : summary.BestAvailablePlayerName)
            + " | Cap: " + BetterFreeAgencyConfig.FormatMoney(summary.UserTeamCapSpace);
        return summary;
    }

    public static FreeAgentOfferData MakeUserFreeAgentOffer(GameState state, string playerId, int salary, int years)
    {
        TeamData team = FindTeam(state == null ? null : state.Teams, state == null ? "" : state.SelectedTeamId);
        PlayerData player = FindFreeAgent(state, playerId);
        return MakeFreeAgentOffer(state, team, player, salary, years, "User");
    }

    public static FreeAgentOfferData MakeCpuFreeAgentOffer(
        GameState state,
        TeamData team,
        PlayerData player,
        int salary,
        int years)
    {
        return MakeFreeAgentOffer(state, team, player, salary, years, "Cpu");
    }

    public static int CalculateAcceptanceScore(
        GameState state,
        TeamData team,
        PlayerData player,
        int offeredSalary,
        int offeredYears)
    {
        if (player == null || team == null)
        {
            return 0;
        }

        EnsureFreeAgentEvaluation(state, FindTeam(state == null ? null : state.Teams, state == null ? "" : state.SelectedTeamId), player);
        FreeAgentFitEvaluationData evaluation = FreeAgentFitService.EvaluateFit(state, team, player, offeredSalary, offeredYears);
        int score = evaluation.FinalInterestScore;

        if (offeredSalary < player.FreeAgencyMinimumSalary)
        {
            score -= 25;
        }
        else if (offeredSalary >= player.FreeAgencyExpectedSalary)
        {
            score += 8;
        }

        if (offeredYears == player.FreeAgencyPreferredYears)
        {
            score += 4;
        }

        if (player.FreeAgencyOfferAttempts > 2 && offeredSalary < player.FreeAgencyExpectedSalary)
        {
            score -= 5;
        }

        return BetterFreeAgencyConfig.ClampInterest(score);
    }

    public static bool ValidateFreeAgentOffer(
        GameState state,
        TeamData team,
        PlayerData player,
        int salary,
        int years,
        out string message)
    {
        if (state == null)
        {
            message = "Состояние игры не найдено";
            return false;
        }

        if (team == null)
        {
            message = "Команда не найдена";
            return false;
        }

        if (player == null)
        {
            message = "Свободный агент не найден";
            return false;
        }

        if (player.IsRetired)
        {
            message = "Игрок завершил карьеру";
            return false;
        }

        if (!LeaguePhaseService.IsFreeAgencyOpen(state))
        {
            message = "Рынок свободных агентов откроется после завершения драфта";
            return false;
        }

        if (player.ContractStatus == "RFA")
        {
            message = "Offer sheets/RFA пока не реализованы";
            return false;
        }

        if (player.ContractStatus != "UFA")
        {
            message = "Игрок не является UFA";
            return false;
        }

        LeagueRulesData rules = GetRules(state);
        if (salary < BetterFreeAgencyConfig.GetLeagueMinimumSalary(rules))
        {
            message = "Зарплата ниже минимума лиги";
            return false;
        }

        if (salary > BetterFreeAgencyConfig.GetMaximumPlayerSalary(rules))
        {
            message = "Зарплата выше максимума игрока";
            return false;
        }

        if (years < BetterFreeAgencyConfig.MinFreeAgentYears || years > BetterFreeAgencyConfig.GetMaxFreeAgentYears(rules))
        {
            message = "Недопустимый срок контракта";
            return false;
        }

        bool willAssignToNhl = TeamRosterService.GetNhlPlayers(team).Count < RosterStatusConfig.MaxNhlRosterSize;
        if (willAssignToNhl && !SalaryCapService.CanAddSalary(team, salary))
        {
            message = "Недостаточно места под потолком зарплат";
            return false;
        }

        message = "Оффер можно отправить";
        return true;
    }

    public static void ApplyAcceptedFreeAgentSigning(
        GameState state,
        TeamData team,
        PlayerData freeAgent,
        FreeAgentOfferData offer)
    {
        if (state == null || team == null || freeAgent == null || offer == null || freeAgent.IsRetired)
        {
            return;
        }

        EnsureFreeAgentPool(state);
        if (ContainsPlayer(team.Players, freeAgent.Id))
        {
            RemoveFreeAgentFromPool(state, freeAgent);
            return;
        }

        RemoveFreeAgentFromPool(state, freeAgent);
        team.EnsurePlayers();

        freeAgent.TeamId = team.Id;
        freeAgent.Salary = BetterFreeAgencyConfig.ClampSalary(offer.OfferedSalary, GetRules(state));
        freeAgent.ContractYearsRemaining = BetterFreeAgencyConfig.ClampFreeAgentYears(offer.OfferedYears, GetRules(state));
        freeAgent.ContractStatus = "Signed";
        freeAgent.HasNoTradeClause = false;
        freeAgent.IsGeneratedContract = true;
        ContractExtensionService.ResetPlayerExtensionFields(freeAgent);

        bool canFitNhl = TeamRosterService.GetNhlPlayers(team).Count < RosterStatusConfig.MaxNhlRosterSize
            && SalaryCapService.CanAddSalary(team, freeAgent.Salary)
            && offer.RoleFitScore >= 35;
        freeAgent.PreviousRosterStatus = RosterStatusConfig.FreeAgent;
        freeAgent.RosterStatus = canFitNhl ? RosterStatusConfig.NHL : RosterStatusConfig.Farm;
        freeAgent.RosterStatusUpdatedAtUtc = DateTime.UtcNow.ToString("o");
        freeAgent.IsOnWaivers = false;
        freeAgent.WaiverStatus = WaiverConfig.WaiverStatusNone;
        freeAgent.WaiverPlacedAtUtc = "";
        freeAgent.WaiverExpiresAtUtc = "";
        freeAgent.WaiverOriginalTeamId = "";
        freeAgent.WaiverOriginalTeamName = "";
        freeAgent.WaiverIntendedDestination = "";
        freeAgent.WantsTrade = false;
        freeAgent.Morale = MoraleConfig.DefaultMorale;
        freeAgent.RoleSatisfaction = MoraleConfig.DefaultMorale;
        freeAgent.IceTimeSatisfaction = MoraleConfig.DefaultMorale;
        freeAgent.TeamPerformanceSatisfaction = MoraleConfig.DefaultMorale;
        freeAgent.ContractSatisfaction = MoraleConfig.DefaultMorale;
        freeAgent.RosterStatusSatisfaction = MoraleConfig.DefaultMorale;
        freeAgent.OverallSatisfaction = MoraleConfig.DefaultMorale;
        freeAgent.MoraleStatus = MoraleConfig.StatusContent;
        freeAgent.MoraleTrend = MoraleConfig.TrendStable;
        freeAgent.MoraleSummary = "Signed as free agent";
        freeAgent.LastMoraleUpdateUtc = DateTime.UtcNow.ToString("o");
        freeAgent.HasMoraleInitialized = true;
        LeadershipService.ClearPlayerCaptaincy(freeAgent);
        LeadershipService.EnsurePlayerLeadershipProfile(freeAgent);
        WaiverEligibilityService.EnsureWaiverEligibility(freeAgent);
        PlayerFatigueService.EnsureFatigueFields(freeAgent);
        InjuryService.EnsureInjuryFields(freeAgent);
        PlayerRoleService.EnsureRole(freeAgent);
        team.Players.Add(freeAgent);

        LineupService.SyncScratchPlayers(team);
        ContractGenerator.EnsureContractsForTeam(team);
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        IceTimeService.EnsureUsageForTeam(team);
        MoraleService.EnsureMoraleForTeam(state, team);
        TeamTradeProfileService.EnsureTradeProfiles(state);
        AddOldSigningToHistory(state, offer);
    }

    public static void AddOfferToHistory(GameState state, FreeAgentOfferData offer)
    {
        if (state == null || offer == null)
        {
            return;
        }

        EnsureFreeAgencyOfferHistory(state);
        state.FreeAgencyOfferHistory.Offers.Add(offer);
        TrimOfferHistory(state);
        state.FreeAgencyOfferHistory.EnsureOffers();
    }

    public static void TrimOfferHistory(GameState state)
    {
        if (state == null || state.FreeAgencyOfferHistory == null)
        {
            return;
        }

        state.FreeAgencyOfferHistory.EnsureOffers();
        while (state.FreeAgencyOfferHistory.Offers.Count > BetterFreeAgencyConfig.MaxOffersToKeep)
        {
            state.FreeAgencyOfferHistory.Offers.RemoveAt(0);
        }
    }

    public static PlayerData FindFreeAgent(GameState state, string playerId)
    {
        if (state == null || state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
        {
            if (player != null && !player.IsRetired && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static FreeAgentOfferData MakeFreeAgentOffer(
        GameState state,
        TeamData team,
        PlayerData player,
        int salary,
        int years,
        string source)
    {
        EnsureFreeAgencyOfferHistory(state);
        EnsureFreeAgentEvaluation(state, FindTeam(state == null ? null : state.Teams, state == null ? "" : state.SelectedTeamId), player);

        FreeAgentFitEvaluationData evaluation = team == null || player == null
            ? null
            : FreeAgentFitService.EvaluateFit(state, team, player, salary, years);

        FreeAgentOfferData offer = CreateOffer(team, player, salary, years, evaluation, source);
        if (!ValidateFreeAgentOffer(state, team, player, salary, years, out string validationMessage))
        {
            offer.Decision = "Invalid";
            offer.DecisionReason = validationMessage;
            offer.Accepted = false;
            UpdatePlayerOfferFields(player, offer);
            AddOfferToHistory(state, offer);
            return offer;
        }

        int acceptanceScore = CalculateAcceptanceScore(state, team, player, salary, years);
        offer.AcceptanceScore = acceptanceScore;
        bool accepted = acceptanceScore >= BetterFreeAgencyConfig.AcceptanceThreshold;
        offer.Accepted = accepted;
        offer.Decision = accepted ? "Accepted" : "Rejected";
        offer.DecisionReason = accepted
            ? "Оффер принят"
            : "Оффер отклонен: interest " + acceptanceScore
                + " / " + BetterFreeAgencyConfig.AcceptanceThreshold
                + ", ask " + BetterFreeAgencyConfig.FormatMoney(player.FreeAgencyExpectedSalary)
                + " x " + player.FreeAgencyExpectedYears;

        if (accepted)
        {
            ApplyAcceptedFreeAgentSigning(state, team, player, offer);
            EventNewsService.CreateFreeAgentSigningNews(state, offer);
        }
        else
        {
            AddOldSigningToHistory(state, offer);
        }

        UpdatePlayerOfferFields(player, offer);
        AddOfferToHistory(state, offer);
        return offer;
    }

    private static FreeAgentOfferData CreateOffer(
        TeamData team,
        PlayerData player,
        int salary,
        int years,
        FreeAgentFitEvaluationData evaluation,
        string source)
    {
        return new FreeAgentOfferData
        {
            OfferId = Guid.NewGuid().ToString("N"),
            PlayerId = player == null ? "" : player.Id,
            PlayerName = GetPlayerName(player),
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            OfferedSalary = salary,
            OfferedYears = years,
            ExpectedSalary = player == null ? 0 : player.FreeAgencyExpectedSalary,
            MinimumSalary = player == null ? 0 : player.FreeAgencyMinimumSalary,
            ExpectedYears = player == null ? 0 : player.FreeAgencyExpectedYears,
            TeamFitScore = evaluation == null ? 0 : evaluation.TeamFitScore,
            ContractFitScore = evaluation == null ? 0 : evaluation.ContractFitScore,
            RoleFitScore = evaluation == null ? 0 : evaluation.RoleFitScore,
            RosterOpportunityScore = evaluation == null ? 0 : evaluation.RosterOpportunityScore,
            ContenderScore = evaluation == null ? 0 : evaluation.ContenderScore,
            CoachingFitScore = evaluation == null ? 0 : evaluation.CoachingFitScore,
            CapFitScore = evaluation == null ? 0 : evaluation.CapFitScore,
            FinalInterestScore = evaluation == null ? 0 : evaluation.FinalInterestScore,
            AcceptanceScore = evaluation == null ? 0 : evaluation.FinalInterestScore,
            Accepted = false,
            Decision = "",
            DecisionReason = "",
            CreatedAtUtc = DateTime.UtcNow.ToString("o"),
            Source = source
        };
    }

    private static void UpdatePlayerOfferFields(PlayerData player, FreeAgentOfferData offer)
    {
        if (player == null || offer == null)
        {
            return;
        }

        player.FreeAgencyOfferAttempts++;
        player.LastFreeAgencyOfferStatus = offer.Decision;
        player.LastFreeAgencyOfferSalary = offer.OfferedSalary;
        player.LastFreeAgencyOfferYears = offer.OfferedYears;
        player.LastFreeAgencyOfferTeamId = offer.TeamId;
        player.LastFreeAgencyOfferTeamName = offer.TeamName;
        player.LastFreeAgencyOfferAtUtc = offer.CreatedAtUtc;
    }

    private static FreeAgentFitEvaluationData FindBestFit(GameState state, PlayerData player)
    {
        if (state == null || state.Teams == null || player == null)
        {
            return null;
        }

        FreeAgentFitEvaluationData best = null;
        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            FreeAgentFitEvaluationData evaluation = FreeAgentFitService.EvaluateFit(
                state,
                team,
                player,
                player.FreeAgencyExpectedSalary,
                player.FreeAgencyExpectedYears);
            if (best == null || evaluation.FinalInterestScore > best.FinalInterestScore)
            {
                best = evaluation;
            }
        }

        return best;
    }

    private static void AddOldSigningToHistory(GameState state, FreeAgentOfferData offer)
    {
        if (state == null || offer == null)
        {
            return;
        }

        state.EnsureFreeAgentHistory();
        state.FreeAgentHistory.Signings.Add(new FreeAgentSigningData
        {
            SigningId = Guid.NewGuid().ToString("N"),
            PlayerId = offer.PlayerId,
            PlayerName = offer.PlayerName,
            TeamId = offer.TeamId,
            TeamName = offer.TeamName,
            SignedAtUtc = offer.CreatedAtUtc,
            Salary = offer.OfferedSalary,
            ContractYears = offer.OfferedYears,
            Status = offer.Decision,
            RejectionReason = offer.Decision == "Accepted" ? "" : offer.DecisionReason
        });
    }

    private static void EnsureFreeAgentPool(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.LeagueRules == null)
        {
            state.LeagueRules = LeagueRulesConfig.CreateDefaultRules();
        }

        if (state.FreeAgentPool == null)
        {
            state.FreeAgentPool = FreeAgentGenerator.CreateFreeAgentPool();
        }

        state.FreeAgentPool.EnsureFreeAgents();
        TeamRosterService.EnsureFreeAgentRosterStatuses(state.FreeAgentPool.FreeAgents);
        if (state.FreeAgentHistory == null)
        {
            state.FreeAgentHistory = new FreeAgentHistoryData();
        }

        state.FreeAgentHistory.EnsureSignings();
    }

    private static void RemoveFreeAgentFromPool(GameState state, PlayerData freeAgent)
    {
        if (state == null || state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null || freeAgent == null)
        {
            return;
        }

        state.FreeAgentPool.FreeAgents.Remove(freeAgent);
    }

    private static bool ContainsPlayer(List<PlayerData> players, string playerId)
    {
        if (players == null || string.IsNullOrEmpty(playerId))
        {
            return false;
        }

        foreach (PlayerData player in players)
        {
            if (player != null && player.Id == playerId)
            {
                return true;
            }
        }

        return false;
    }

    private static int CompareFreeAgents(PlayerData left, PlayerData right)
    {
        int overallComparison = (right == null ? 0 : right.Overall).CompareTo(left == null ? 0 : left.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        int interestComparison = (right == null ? 0 : right.FreeAgencyInterestInUserTeam)
            .CompareTo(left == null ? 0 : left.FreeAgencyInterestInUserTeam);
        if (interestComparison != 0)
        {
            return interestComparison;
        }

        return (left == null ? int.MaxValue : left.FreeAgencyExpectedSalary)
            .CompareTo(right == null ? int.MaxValue : right.FreeAgencyExpectedSalary);
    }

    private static TeamData FindTeam(List<TeamData> teams, string teamId)
    {
        if (teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in teams)
        {
            if (team != null && team.Id == teamId)
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

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : player.FirstName + " " + player.LastName;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
