using System;
using System.Collections.Generic;

public static class ContractExtensionService
{
    public static void EnsureExtensionHistory(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureContractExtensionHistory();
    }

    public static void EnsureExtensionDataForTeam(GameState state, TeamData team)
    {
        if (team == null)
        {
            return;
        }

        EnsureExtensionHistory(state);
        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        PlayerRoleService.EnsureRolesForTeam(team);
        IceTimeService.EnsureUsageForTeam(team);
        MoraleService.EnsureMoraleForTeam(state, team);
        LeadershipService.EnsureLeadershipForTeam(team);
        CoachingStaffService.EnsureStaffForTeam(team);

        foreach (PlayerData player in team.Players)
        {
            EnsureExtensionDataForPlayer(state, team, player);
        }
    }

    public static void EnsureExtensionDataForTeams(GameState state, List<TeamData> teams)
    {
        EnsureExtensionHistory(state);
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureExtensionDataForTeam(state, team);
        }
    }

    public static void EnsureExtensionDataForPlayer(GameState state, TeamData team, PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        ContractGenerator.NormalizeContract(player);
        if (string.IsNullOrEmpty(player.LastExtensionOfferStatus))
        {
            player.LastExtensionOfferStatus = "None";
        }

        bool eligible = IsPlayerExtensionEligible(state, team, player, out string reason);
        player.IsExtensionEligible = eligible;
        player.ExtensionInterest = ContractExtensionInterestService.CalculateExtensionInterest(state, team, player);
        player.ExtensionExpectedSalary = ContractExtensionValuationService.CalculateExpectedSalary(state, team, player);
        player.ExtensionMinimumSalary = ContractExtensionValuationService.CalculateMinimumSalary(state, team, player);
        player.ExtensionExpectedYears = ContractExtensionValuationService.CalculateExpectedYears(state, team, player);
        player.ExtensionPreferredYears = ContractExtensionValuationService.CalculatePreferredYears(state, team, player);
        player.ExtensionInterestSummary = eligible
            ? ContractExtensionInterestService.BuildInterestSummary(state, team, player, player.ExtensionInterest)
            : reason;
        player.ExtensionAskSummary = ContractExtensionValuationService.BuildAskSummary(player);
    }

    public static bool IsPlayerExtensionEligible(GameState state, TeamData team, PlayerData player, out string reason)
    {
        if (player == null)
        {
            reason = "Игрок не найден";
            return false;
        }

        if (player.IsRetired)
        {
            reason = "Игрок завершил карьеру";
            return false;
        }

        if (player.RefusesExtensionThisSeason)
        {
            reason = "Игрок отказывается вести переговоры в этом сезоне";
            return false;
        }

        if (player.IsOnWaivers)
        {
            reason = "Игрок находится на waivers";
            return false;
        }

        if (player.RosterStatus == RosterStatusConfig.FreeAgent)
        {
            reason = "Игрок является свободным агентом";
            return false;
        }

        ContractGenerator.NormalizeContract(player);
        if (player.ContractYearsRemaining > 1)
        {
            reason = "Контракт ещё не истекает";
            return false;
        }

        if (player.ContractYearsRemaining < 0)
        {
            reason = "Некорректный срок контракта";
            return false;
        }

        if (player.ContractStatus == "UFA"
            || player.ContractStatus == "RFA"
            || player.ContractStatus == "Expiring"
            || player.ContractStatus == "Signed"
            || player.IsEntryLevelContract
            || string.IsNullOrEmpty(player.ContractStatus))
        {
            reason = "Можно предложить продление";
            return true;
        }

        reason = "Статус контракта не поддерживает продление";
        return false;
    }

    public static List<ContractExtensionCandidateData> GetExtensionCandidates(GameState state, TeamData team)
    {
        List<ContractExtensionCandidateData> candidates = new List<ContractExtensionCandidateData>();
        EnsureExtensionDataForTeam(state, team);
        if (team == null || team.Players == null)
        {
            return candidates;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player == null || player.IsRetired)
            {
                continue;
            }

            if (player.IsExtensionEligible || player.ContractYearsRemaining <= 1)
            {
                candidates.Add(BuildCandidate(state, team, player));
            }
        }

        candidates.Sort(CompareCandidates);
        return candidates;
    }

    public static ContractExtensionCandidateData BuildCandidate(GameState state, TeamData team, PlayerData player)
    {
        EnsureExtensionDataForPlayer(state, team, player);
        string reason;
        bool eligible = IsPlayerExtensionEligible(state, team, player, out reason);
        string playerName = GetPlayerName(player);

        return new ContractExtensionCandidateData
        {
            PlayerId = player == null ? "" : player.Id,
            PlayerName = playerName,
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            Position = player == null ? "" : player.Position,
            Age = player == null ? 0 : player.Age,
            Overall = player == null ? 0 : player.Overall,
            Potential = player == null ? 0 : player.Potential,
            ContractStatus = player == null ? "" : NormalizeVisibleContractStatus(player),
            CurrentSalary = player == null ? 0 : player.Salary,
            ContractYearsRemaining = player == null ? 0 : player.ContractYearsRemaining,
            IsEntryLevelContract = player != null && player.IsEntryLevelContract,
            RosterStatus = player == null ? "" : player.RosterStatus,
            PlayerRole = player == null ? "" : player.PlayerRole,
            UsageCategory = player == null ? "" : player.UsageCategory,
            Morale = player == null ? 0 : player.Morale,
            WantsTrade = player != null && player.WantsTrade,
            ExtensionInterest = player == null ? 0 : player.ExtensionInterest,
            ExpectedSalary = player == null ? 0 : player.ExtensionExpectedSalary,
            MinimumSalary = player == null ? 0 : player.ExtensionMinimumSalary,
            ExpectedYears = player == null ? 0 : player.ExtensionExpectedYears,
            PreferredYears = player == null ? 0 : player.ExtensionPreferredYears,
            IsExtensionEligible = eligible,
            EligibilityReason = reason,
            InterestSummary = player == null ? "" : player.ExtensionInterestSummary,
            AskSummary = player == null ? "" : player.ExtensionAskSummary,
            Category = GetCategory(player, eligible),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    public static ContractExtensionSummaryData BuildSummary(GameState state, TeamData team)
    {
        List<ContractExtensionCandidateData> candidates = GetExtensionCandidates(state, team);
        ContractExtensionSummaryData summary = new ContractExtensionSummaryData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        ContractExtensionCandidateData important = null;
        foreach (ContractExtensionCandidateData candidate in candidates)
        {
            if (candidate == null)
            {
                continue;
            }

            if (candidate.IsExtensionEligible)
            {
                summary.EligiblePlayers++;
            }

            if (candidate.Category == "PendingUFA")
            {
                summary.PendingUfaCount++;
            }
            else if (candidate.Category == "PendingRFA")
            {
                summary.PendingRfaCount++;
            }
            else if (candidate.Category == "ELCExpiring")
            {
                summary.ElcExpiringCount++;
            }

            if (candidate.ExtensionInterest >= ContractExtensionConfig.HighInterestThreshold)
            {
                summary.HighInterestCount++;
            }

            if (candidate.ExtensionInterest < ContractExtensionConfig.LowInterestThreshold)
            {
                summary.LowInterestCount++;
            }

            PlayerData player = FindPlayer(team, candidate.PlayerId);
            if (player != null && player.RefusesExtensionThisSeason)
            {
                summary.RefusingCount++;
            }

            if (important == null || candidate.Overall > important.Overall || candidate.Potential > important.Potential && candidate.Overall >= important.Overall - 2)
            {
                important = candidate;
            }
        }

        summary.MostImportantPlayerId = important == null ? "" : important.PlayerId;
        summary.MostImportantPlayerName = important == null ? "" : important.PlayerName;
        summary.Summary = "Истекающие контракты: " + candidates.Count
            + " | UFA: " + summary.PendingUfaCount
            + " | RFA: " + summary.PendingRfaCount
            + " | ELC: " + summary.ElcExpiringCount
            + " | Низкий интерес: " + summary.LowInterestCount;
        return summary;
    }

    public static ContractExtensionOfferData MakeExtensionOffer(
        GameState state,
        TeamData team,
        string playerId,
        int offeredSalary,
        int offeredYears)
    {
        EnsureExtensionHistory(state);
        PlayerData player = FindPlayer(team, playerId);
        ContractExtensionOfferData offer = CreateOffer(team, player, offeredSalary, offeredYears);

        if (!ValidateExtensionOffer(state, team, player, offeredSalary, offeredYears, out string message))
        {
            offer.Decision = "Invalid";
            offer.DecisionReason = message;
            offer.Accepted = false;
            AddOfferToHistory(state, offer);
            return offer;
        }

        EnsureExtensionDataForPlayer(state, team, player);
        int score = CalculateAcceptanceScore(state, team, player, offeredSalary, offeredYears, out string reason);
        bool accepted = score >= ContractExtensionConfig.AcceptanceThreshold && offeredSalary >= player.ExtensionMinimumSalary;

        player.LastExtensionOfferSalary = offeredSalary;
        player.LastExtensionOfferYears = offeredYears;
        player.LastExtensionOfferAtUtc = offer.CreatedAtUtc;
        player.ExtensionAttemptsThisSeason++;
        player.HasExtensionOfferPending = false;

        offer.ExpectedSalary = player.ExtensionExpectedSalary;
        offer.MinimumSalary = player.ExtensionMinimumSalary;
        offer.ExpectedYears = player.ExtensionExpectedYears;
        offer.ExtensionInterest = player.ExtensionInterest;
        offer.AcceptanceScore = score;
        offer.Accepted = accepted;

        if (accepted)
        {
            offer.Decision = "Accepted";
            offer.DecisionReason = string.IsNullOrEmpty(reason)
                ? "Принято: зарплата и срок соответствуют ожиданиям"
                : "Принято: " + reason;
            ApplyAcceptedExtension(state, team, player, offer);
        }
        else
        {
            offer.Decision = "Rejected";
            offer.DecisionReason = string.IsNullOrEmpty(reason)
                ? "Отклонено: игрок не заинтересован остаться"
                : "Отклонено: " + reason;
            player.LastExtensionOfferStatus = "Rejected";
            if (offeredSalary < player.ExtensionMinimumSalary * 75 / 100
                || player.ExtensionAttemptsThisSeason >= 3 && player.ExtensionInterest < ContractExtensionConfig.LowInterestThreshold)
            {
                player.RefusesExtensionThisSeason = true;
            }

            ApplyRejectedOfferMorale(state, team, player, offeredSalary < player.ExtensionMinimumSalary);
        }

        AddOfferToHistory(state, offer);
        TrimExtensionHistory(state);
        return offer;
    }

    public static int CalculateAcceptanceScore(
        GameState state,
        TeamData team,
        PlayerData player,
        int offeredSalary,
        int offeredYears,
        out string reason)
    {
        reason = "";
        if (player == null)
        {
            reason = "игрок не найден";
            return -100;
        }

        EnsureExtensionDataForPlayer(state, team, player);
        int score = player.ExtensionInterest;
        int expectedSalary = Math.Max(1, player.ExtensionExpectedSalary);
        int ratio = offeredSalary * 100 / expectedSalary;

        if (ratio >= 105)
        {
            score += 20;
        }
        else if (ratio >= 95)
        {
            score += 10;
        }
        else if (ratio >= 85)
        {
            score += 0;
        }
        else if (ratio >= 75)
        {
            score -= 20;
        }
        else
        {
            score -= 40;
        }

        if (offeredSalary < player.ExtensionMinimumSalary)
        {
            score -= 35;
            reason = "зарплата ниже минимального ожидания";
        }

        int yearsDelta = Math.Abs(offeredYears - player.ExtensionExpectedYears);
        if (offeredYears == player.ExtensionExpectedYears || offeredYears == player.ExtensionPreferredYears)
        {
            score += 10;
        }
        else if (yearsDelta <= 1)
        {
            score += 5;
        }

        if (offeredYears <= 2 && IsYoungCore(player))
        {
            score -= 10;
            if (string.IsNullOrEmpty(reason))
            {
                reason = "срок слишком короткий";
            }
        }
        else if (offeredYears >= 4 && player.Age >= 32)
        {
            score += 5;
        }

        if (player.WantsTrade)
        {
            score -= 25;
            if (string.IsNullOrEmpty(reason))
            {
                reason = "игрок хочет обмен";
            }
        }

        if (player.Morale >= 80)
        {
            score += 8;
        }
        else if (player.Morale < 35)
        {
            score -= 15;
            if (string.IsNullOrEmpty(reason))
            {
                reason = "низкая morale";
            }
        }

        if (player.RoleSatisfaction >= 80)
        {
            score += 5;
        }

        if (player.RefusesExtensionThisSeason)
        {
            score -= 100;
            reason = "игрок отказывается вести переговоры в этом сезоне";
        }

        string capWarning = BuildProjectedCapWarning(team, player, offeredSalary, GetRules(state));
        if (string.IsNullOrEmpty(reason))
        {
            reason = score >= ContractExtensionConfig.AcceptanceThreshold
                ? "зарплата и срок соответствуют ожиданиям"
                : "условия ниже ожиданий игрока";
        }

        if (!string.IsNullOrEmpty(capWarning))
        {
            reason += ". " + capWarning;
        }

        return Clamp(score, -100, 120);
    }

    public static bool ValidateExtensionOffer(
        GameState state,
        TeamData team,
        PlayerData player,
        int offeredSalary,
        int offeredYears,
        out string message)
    {
        LeagueRulesData rules = GetRules(state);
        if (state == null)
        {
            message = "Недействительно: состояние игры не найдено";
            return false;
        }

        if (team == null)
        {
            message = "Недействительно: команда не найдена";
            return false;
        }

        if (player == null)
        {
            message = "Недействительно: игрок не найден";
            return false;
        }

        if (!IsPlayerExtensionEligible(state, team, player, out string reason))
        {
            message = "Недействительно: " + reason;
            return false;
        }

        if (offeredSalary < ContractExtensionConfig.GetLeagueMinimumSalary(rules))
        {
            message = "Недействительно: зарплата ниже минимума лиги";
            return false;
        }

        if (offeredSalary > ContractExtensionConfig.GetMaximumPlayerSalary(rules))
        {
            message = "Недействительно: зарплата превышает максимум игрока";
            return false;
        }

        if (offeredYears < ContractExtensionConfig.MinExtensionYears)
        {
            message = "Недействительно: срок меньше одного года";
            return false;
        }

        if (offeredYears > ContractExtensionConfig.GetMaxOwnTeamYears(rules))
        {
            message = "Недействительно: срок превышает максимум для своей команды";
            return false;
        }

        message = "Предложение возможно";
        return true;
    }

    public static void ApplyAcceptedExtension(
        GameState state,
        TeamData team,
        PlayerData player,
        ContractExtensionOfferData offer)
    {
        if (player == null || offer == null)
        {
            return;
        }

        ContractService.ApplyContractExtension(player, offer.OfferedSalary, offer.OfferedYears, out string message);
        player.HasExtensionOfferPending = false;
        player.LastExtensionOfferStatus = "Accepted";
        player.LastExtensionOfferSalary = offer.OfferedSalary;
        player.LastExtensionOfferYears = offer.OfferedYears;
        player.LastExtensionOfferAtUtc = offer.CreatedAtUtc;
        player.RefusesExtensionThisSeason = false;
        player.ExtensionAttemptsThisSeason = 0;
        player.IsExtensionEligible = false;

        if (player.WantsTrade && player.Morale >= 45)
        {
            player.WantsTrade = false;
        }

        ApplyAcceptedOfferMorale(state, team, player);
        EnsureExtensionDataForPlayer(state, team, player);
    }

    public static void AddOfferToHistory(GameState state, ContractExtensionOfferData offer)
    {
        if (state == null || offer == null)
        {
            return;
        }

        EnsureExtensionHistory(state);
        state.ContractExtensionHistory.Offers.Add(offer);
        state.ContractExtensionHistory.TotalOffers++;
        if (offer.Decision == "Accepted")
        {
            state.ContractExtensionHistory.AcceptedOffers++;
        }
        else if (offer.Decision == "Rejected")
        {
            state.ContractExtensionHistory.RejectedOffers++;
        }

        state.ContractExtensionHistory.LastOfferAtUtc = offer.CreatedAtUtc;
        TrimExtensionHistory(state);
    }

    public static void TrimExtensionHistory(GameState state)
    {
        if (state == null || state.ContractExtensionHistory == null)
        {
            return;
        }

        state.ContractExtensionHistory.EnsureOffers();
        while (state.ContractExtensionHistory.Offers.Count > ContractExtensionConfig.MaxOffersToKeep)
        {
            state.ContractExtensionHistory.Offers.RemoveAt(0);
        }
    }

    public static void ResetSeasonNegotiationFields(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsurePlayers();
            foreach (PlayerData player in team.Players)
            {
                if (player == null)
                {
                    continue;
                }

                player.ExtensionAttemptsThisSeason = 0;
                player.RefusesExtensionThisSeason = false;
                player.HasExtensionOfferPending = false;
                player.LastExtensionOfferStatus = "None";
            }
        }
    }

    public static void ResetPlayerExtensionFields(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        player.IsExtensionEligible = false;
        player.HasExtensionOfferPending = false;
        player.LastExtensionOfferStatus = "None";
        player.LastExtensionOfferAtUtc = "";
        player.ExtensionAttemptsThisSeason = 0;
        player.RefusesExtensionThisSeason = false;
        player.ExtensionInterestSummary = "";
        player.ExtensionAskSummary = "";
    }

    private static ContractExtensionOfferData CreateOffer(TeamData team, PlayerData player, int salary, int years)
    {
        return new ContractExtensionOfferData
        {
            OfferId = Guid.NewGuid().ToString("N"),
            PlayerId = player == null ? "" : player.Id,
            PlayerName = GetPlayerName(player),
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            OfferedSalary = salary,
            OfferedYears = years,
            ExpectedSalary = player == null ? 0 : player.ExtensionExpectedSalary,
            MinimumSalary = player == null ? 0 : player.ExtensionMinimumSalary,
            ExpectedYears = player == null ? 0 : player.ExtensionExpectedYears,
            ExtensionInterest = player == null ? 0 : player.ExtensionInterest,
            Decision = "",
            DecisionReason = "",
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    private static void ApplyAcceptedOfferMorale(GameState state, TeamData team, PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        MoraleService.InitializePlayerMorale(player);
        int before = player.Morale;
        player.Morale = ClampMorale(player.Morale + 3);
        player.ContractSatisfaction = ClampMorale(player.ContractSatisfaction + 10);
        player.MoraleSummary = "Extension accepted";
        MoraleService.AddMoraleEvent(state, team, player, "ExtensionAccepted", before, player.Morale, "Player signed a contract extension");
    }

    private static void ApplyRejectedOfferMorale(GameState state, TeamData team, PlayerData player, bool lowball)
    {
        if (player == null || !lowball)
        {
            return;
        }

        MoraleService.InitializePlayerMorale(player);
        int before = player.Morale;
        player.Morale = ClampMorale(player.Morale - 3);
        player.ContractSatisfaction = ClampMorale(player.ContractSatisfaction - 8);
        player.MoraleSummary = player.RefusesExtensionThisSeason ? "Contract concern" : player.MoraleSummary;
        MoraleService.AddMoraleEvent(state, team, player, "ContractConcern", before, player.Morale, "Lowball contract extension offer rejected");
    }

    private static string BuildProjectedCapWarning(TeamData team, PlayerData player, int offeredSalary, LeagueRulesData rules)
    {
        if (team == null || player == null || !RosterStatusConfig.IsNhlRoster(player))
        {
            return "";
        }

        int cap = rules != null && rules.SalaryCapUpperLimit > 0
            ? rules.SalaryCapUpperLimit
            : SalaryCapConfig.SalaryCapUpperLimit;
        int payrollAfter = SalaryCapService.CalculatePayroll(team) - player.Salary + offeredSalary;
        return payrollAfter > cap ? "Внимание: projected payroll выше потолка" : "";
    }

    private static string GetCategory(PlayerData player, bool eligible)
    {
        if (player == null || !eligible)
        {
            return "NotEligible";
        }

        if (player.IsEntryLevelContract && player.ContractYearsRemaining <= 1)
        {
            return "ELCExpiring";
        }

        string status = NormalizeVisibleContractStatus(player);
        if (status == "UFA")
        {
            return "PendingUFA";
        }

        if (status == "RFA")
        {
            return "PendingRFA";
        }

        if (player.Overall >= 82)
        {
            return "CorePlayer";
        }

        return player.Age >= 31 ? "Veteran" : "DepthPlayer";
    }

    private static string NormalizeVisibleContractStatus(PlayerData player)
    {
        if (player == null)
        {
            return "";
        }

        if (player.ContractStatus == "UFA" || player.ContractStatus == "RFA")
        {
            return player.ContractStatus;
        }

        if (player.ContractYearsRemaining <= 1)
        {
            return player.Age >= 27 ? "UFA" : "RFA";
        }

        return string.IsNullOrEmpty(player.ContractStatus) ? "Signed" : player.ContractStatus;
    }

    private static PlayerData FindPlayer(TeamData team, string playerId)
    {
        if (team == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static int CompareCandidates(ContractExtensionCandidateData left, ContractExtensionCandidateData right)
    {
        int yearsCompare = left.ContractYearsRemaining.CompareTo(right.ContractYearsRemaining);
        if (yearsCompare != 0)
        {
            return yearsCompare;
        }

        int overallCompare = right.Overall.CompareTo(left.Overall);
        if (overallCompare != 0)
        {
            return overallCompare;
        }

        int potentialCompare = right.Potential.CompareTo(left.Potential);
        if (potentialCompare != 0)
        {
            return potentialCompare;
        }

        return right.ExtensionInterest.CompareTo(left.ExtensionInterest);
    }

    private static bool IsYoungCore(PlayerData player)
    {
        return player != null && player.Age <= 25 && (player.Overall >= 80 || player.Potential >= 84);
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : player.FirstName + " " + player.LastName;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static LeagueRulesData GetRules(GameState state)
    {
        return state != null && state.LeagueRules != null ? state.LeagueRules : LeagueRulesConfig.CreateDefaultRules();
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }

    private static int ClampMorale(int value)
    {
        return Clamp(value, 0, 100);
    }
}
