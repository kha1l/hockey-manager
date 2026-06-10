using System;
using System.Collections.Generic;

public static class TradeService
{
    public static bool TryCreateOneForOneTrade(
        GameState state,
        string userTeamPlayerId,
        string otherTeamId,
        string otherTeamPlayerId,
        out TradeProposalData proposal,
        out string message)
    {
        TeamData userTeam = state == null ? null : FindTeam(state.Teams, state.SelectedTeamId);
        TeamData otherTeam = state == null ? null : FindTeam(state.Teams, otherTeamId);
        EnsureTeamPlayers(userTeam);
        EnsureTeamPlayers(otherTeam);

        List<TradeAssetData> assetsFromUserTeam = new List<TradeAssetData>();
        List<TradeAssetData> assetsFromOtherTeam = new List<TradeAssetData>();

        PlayerData userPlayer = FindPlayer(userTeam, userTeamPlayerId);
        PlayerData otherPlayer = FindPlayer(otherTeam, otherTeamPlayerId);
        if (userPlayer != null)
        {
            assetsFromUserTeam.Add(CreatePlayerAsset(userPlayer, userTeam));
        }

        if (otherPlayer != null)
        {
            assetsFromOtherTeam.Add(CreatePlayerAsset(otherPlayer, otherTeam));
        }

        return TryCreateTradeWithAssets(
            state,
            assetsFromUserTeam,
            otherTeamId,
            assetsFromOtherTeam,
            out proposal,
            out message);
    }

    public static bool TryCreateTradeWithAssets(
        GameState state,
        List<TradeAssetData> assetsFromUserTeam,
        string otherTeamId,
        List<TradeAssetData> assetsFromOtherTeam,
        out TradeProposalData proposal,
        out string message)
    {
        EnsureTradeHistory(state);
        DraftPickOwnershipService.EnsureDraftPickOwnership(state);

        TeamData userTeam = state == null ? null : FindTeam(state.Teams, state.SelectedTeamId);
        TeamData otherTeam = state == null ? null : FindTeam(state.Teams, otherTeamId);
        EnsureTeamPlayers(userTeam);
        EnsureTeamPlayers(otherTeam);

        assetsFromUserTeam = NormalizeAssets(assetsFromUserTeam);
        assetsFromOtherTeam = NormalizeAssets(assetsFromOtherTeam);
        proposal = CreateProposal(userTeam, otherTeam, assetsFromUserTeam, assetsFromOtherTeam);

        if (!ValidateTradeWithAssets(state, userTeam, assetsFromUserTeam, otherTeam, assetsFromOtherTeam, out message))
        {
            proposal.Status = "Rejected";
            proposal.RejectionReason = message;
            AddProposalToHistory(state, proposal);
            return false;
        }

        string validationMessage = message;
        ApplyPlayerTransfers(userTeam, otherTeam, assetsFromUserTeam);
        ApplyPlayerTransfers(otherTeam, userTeam, assetsFromOtherTeam);
        ApplyPickTransfers(state, assetsFromUserTeam, otherTeam, proposal.TradeId);
        ApplyPickTransfers(state, assetsFromOtherTeam, userTeam, proposal.TradeId);

        ContractGenerator.EnsureContractsForTeam(userTeam);
        ContractGenerator.EnsureContractsForTeam(otherTeam);

        proposal.Status = "Accepted";
        proposal.RejectionReason = "";
        AddProposalToHistory(state, proposal);

        message = "Обмен принят";
        if (!string.IsNullOrEmpty(validationMessage) && validationMessage != "Обмен возможен")
        {
            message += ". " + validationMessage;
        }

        AppendFloorWarnings(userTeam, otherTeam, ref message);
        return true;
    }

    public static void EnsureTradeHistory(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.TradeHistory == null)
        {
            state.TradeHistory = new TradeHistoryData();
        }

        state.TradeHistory.EnsureTrades();
    }

    public static TradeAssetData CreatePlayerAsset(PlayerData player, TeamData team)
    {
        if (player == null)
        {
            return null;
        }

        ContractGenerator.NormalizeContract(player);

        TradeAssetData asset = new TradeAssetData
        {
            AssetType = "Player",
            PlayerId = player.Id,
            PlayerName = player.FirstName + " " + player.LastName,
            PickId = "",
            DraftYear = 0,
            DraftRound = 0,
            OriginalTeamId = "",
            OriginalTeamName = "",
            TeamId = team == null ? player.TeamId : team.Id,
            TeamName = team == null ? "" : GetTeamName(team),
            Position = player.Position,
            Age = player.Age,
            Overall = player.Overall,
            Salary = player.Salary,
            ContractYearsRemaining = player.ContractYearsRemaining,
            HasNoTradeClause = player.HasNoTradeClause,
            RetainedSalaryPercent = 0
        };

        asset.EstimatedTradeValue = TradeValueCalculator.CalculateAssetValue(asset);
        return asset;
    }

    public static TradeAssetData CreateDraftPickAsset(DraftPickOwnershipData pick)
    {
        if (pick == null)
        {
            return null;
        }

        TradeAssetData asset = new TradeAssetData
        {
            AssetType = "DraftPick",
            PlayerId = "",
            PlayerName = "",
            PickId = pick.PickId,
            DraftYear = pick.DraftYear,
            DraftRound = pick.Round,
            OriginalTeamId = pick.OriginalTeamId,
            OriginalTeamName = pick.OriginalTeamName,
            TeamId = pick.CurrentOwnerTeamId,
            TeamName = pick.CurrentOwnerTeamName,
            Position = "",
            Age = 0,
            Overall = 0,
            Salary = 0,
            ContractYearsRemaining = 0,
            HasNoTradeClause = false,
            RetainedSalaryPercent = 0
        };

        asset.EstimatedTradeValue = TradeValueCalculator.CalculateAssetValue(asset);
        return asset;
    }

    private static bool ValidateTradeWithAssets(
        GameState state,
        TeamData userTeam,
        List<TradeAssetData> assetsFromUserTeam,
        TeamData otherTeam,
        List<TradeAssetData> assetsFromOtherTeam,
        out string message)
    {
        if (state == null)
        {
            message = "Состояние игры не найдено";
            return false;
        }

        if (userTeam == null || otherTeam == null)
        {
            message = "Команда для обмена не найдена";
            return false;
        }

        if (userTeam.Id == otherTeam.Id)
        {
            message = "Нельзя обменивать активы внутри одной команды";
            return false;
        }

        if (assetsFromUserTeam.Count == 0 || assetsFromOtherTeam.Count == 0)
        {
            message = "Обе стороны должны отдавать хотя бы один актив";
            return false;
        }

        if (!IsAssetLimitValid(assetsFromUserTeam) || !IsAssetLimitValid(assetsFromOtherTeam))
        {
            message = "В MVP можно обменивать максимум 1 игрока и 1 пик с каждой стороны";
            return false;
        }

        if (LeagueDateService.IsPastTradeDeadline(state))
        {
            message = "Обмены после дедлайна недоступны";
            return false;
        }

        if (!ValidateAssetsBelongToTeam(state, userTeam, assetsFromUserTeam, out message)
            || !ValidateAssetsBelongToTeam(state, otherTeam, assetsFromOtherTeam, out message))
        {
            return false;
        }

        if (HasNoTradeClauseAsset(assetsFromUserTeam))
        {
            message = "Игрок вашей команды имеет no-trade clause";
            return false;
        }

        if (HasNoTradeClauseAsset(assetsFromOtherTeam))
        {
            message = "Игрок другой команды имеет no-trade clause";
            return false;
        }

        if (!ValidateRosterSize(state, userTeam, assetsFromUserTeam, assetsFromOtherTeam)
            || !ValidateRosterSize(state, otherTeam, assetsFromOtherTeam, assetsFromUserTeam))
        {
            message = "Размер состава после обмена вне допустимых лимитов";
            return false;
        }

        int salaryCapUpperLimit = GetSalaryCapUpperLimit(state);
        int userPayrollAfter = CalculatePayrollAfterAssetTrade(userTeam, assetsFromUserTeam, assetsFromOtherTeam);
        int otherPayrollAfter = CalculatePayrollAfterAssetTrade(otherTeam, assetsFromOtherTeam, assetsFromUserTeam);

        if (userPayrollAfter > salaryCapUpperLimit)
        {
            message = "Ваша команда будет выше потолка зарплат";
            return false;
        }

        if (otherPayrollAfter > salaryCapUpperLimit)
        {
            message = "Команда-соперник будет выше потолка зарплат";
            return false;
        }

        int userValue = TradeValueCalculator.CalculateAssetsValue(assetsFromUserTeam);
        int otherValue = TradeValueCalculator.CalculateAssetsValue(assetsFromOtherTeam);
        int valueDifference = Math.Abs(userValue - otherValue);
        message = valueDifference > 500
            ? "Обмен возможен, но ценность активов сильно отличается"
            : "Обмен возможен";
        return true;
    }

    private static bool ValidateAssetsBelongToTeam(GameState state, TeamData team, List<TradeAssetData> assets, out string message)
    {
        foreach (TradeAssetData asset in assets)
        {
            if (asset.AssetType == "Player")
            {
                if (FindPlayer(team, asset.PlayerId) == null)
                {
                    message = "Игрок не принадлежит команде";
                    return false;
                }
            }
            else if (asset.AssetType == "DraftPick")
            {
                DraftPickOwnershipData pick = DraftPickOwnershipService.FindPick(state, asset.PickId);
                if (pick == null)
                {
                    message = "Драфт-пик не найден";
                    return false;
                }

                if (pick.IsUsed)
                {
                    message = "Нельзя обменять уже использованный драфт-пик";
                    return false;
                }

                if (pick.CurrentOwnerTeamId != team.Id)
                {
                    message = "Нельзя обменять чужой драфт-пик";
                    return false;
                }

                if (IsCompletedDraftOrderPick(state, pick.PickId))
                {
                    message = "Нельзя обменять уже сделанный выбор драфта";
                    return false;
                }
            }
        }

        message = "";
        return true;
    }

    private static bool IsCompletedDraftOrderPick(GameState state, string pickId)
    {
        if (state == null || state.Draft == null || state.Draft.DraftOrder == null)
        {
            return false;
        }

        foreach (DraftPickData pick in state.Draft.DraftOrder)
        {
            if (pick != null && pick.PickId == pickId)
            {
                return pick.IsCompleted;
            }
        }

        return false;
    }

    private static bool IsAssetLimitValid(List<TradeAssetData> assets)
    {
        int players = 0;
        int picks = 0;
        foreach (TradeAssetData asset in assets)
        {
            if (asset.AssetType == "Player")
            {
                players++;
            }
            else if (asset.AssetType == "DraftPick")
            {
                picks++;
            }
        }

        return players <= 1 && picks <= 1;
    }

    private static bool HasNoTradeClauseAsset(List<TradeAssetData> assets)
    {
        foreach (TradeAssetData asset in assets)
        {
            if (asset.AssetType == "Player" && asset.HasNoTradeClause)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ValidateRosterSize(
        GameState state,
        TeamData team,
        List<TradeAssetData> outgoingAssets,
        List<TradeAssetData> incomingAssets)
    {
        int rosterAfter = team.Players.Count
            - CountPlayerAssets(outgoingAssets)
            + CountPlayerAssets(incomingAssets);

        return rosterAfter >= GetMinRosterSize(state) && rosterAfter <= GetMaxRosterSize(state);
    }

    private static int CalculatePayrollAfterAssetTrade(
        TeamData team,
        List<TradeAssetData> outgoingAssets,
        List<TradeAssetData> incomingAssets)
    {
        return SalaryCapService.CalculatePayroll(team)
            - SumPlayerSalary(outgoingAssets)
            + SumPlayerSalary(incomingAssets);
    }

    private static void ApplyPlayerTransfers(TeamData fromTeam, TeamData toTeam, List<TradeAssetData> assets)
    {
        foreach (TradeAssetData asset in assets)
        {
            if (asset.AssetType != "Player")
            {
                continue;
            }

            PlayerData player = FindPlayer(fromTeam, asset.PlayerId);
            if (player == null)
            {
                continue;
            }

            fromTeam.Players.Remove(player);
            player.TeamId = toTeam.Id;
            toTeam.Players.Add(player);
        }
    }

    private static void ApplyPickTransfers(GameState state, List<TradeAssetData> assets, TeamData newOwnerTeam, string tradeId)
    {
        foreach (TradeAssetData asset in assets)
        {
            if (asset.AssetType != "DraftPick")
            {
                continue;
            }

            DraftPickOwnershipService.TransferPick(
                state,
                asset.PickId,
                newOwnerTeam.Id,
                GetTeamName(newOwnerTeam),
                tradeId);
        }
    }

    private static List<TradeAssetData> NormalizeAssets(List<TradeAssetData> assets)
    {
        List<TradeAssetData> normalized = new List<TradeAssetData>();
        if (assets == null)
        {
            return normalized;
        }

        foreach (TradeAssetData asset in assets)
        {
            if (asset != null && !string.IsNullOrEmpty(asset.AssetType))
            {
                asset.EstimatedTradeValue = TradeValueCalculator.CalculateAssetValue(asset);
                normalized.Add(asset);
            }
        }

        return normalized;
    }

    private static TradeProposalData CreateProposal(
        TeamData userTeam,
        TeamData otherTeam,
        List<TradeAssetData> assetsFromUserTeam,
        List<TradeAssetData> assetsFromOtherTeam)
    {
        TradeProposalData proposal = new TradeProposalData
        {
            TradeId = Guid.NewGuid().ToString("N"),
            FromTeamId = userTeam == null ? "" : userTeam.Id,
            FromTeamName = GetTeamName(userTeam),
            ToTeamId = otherTeam == null ? "" : otherTeam.Id,
            ToTeamName = GetTeamName(otherTeam),
            AssetsFromUserTeam = new List<TradeAssetData>(assetsFromUserTeam),
            AssetsFromOtherTeam = new List<TradeAssetData>(assetsFromOtherTeam),
            CreatedAtUtc = DateTime.UtcNow.ToString("o"),
            Status = "Draft",
            RejectionReason = ""
        };

        proposal.PlayerFromUserTeam = FirstPlayerAsset(assetsFromUserTeam);
        proposal.PlayerFromOtherTeam = FirstPlayerAsset(assetsFromOtherTeam);
        proposal.EnsureAssets();
        return proposal;
    }

    private static TradeAssetData FirstPlayerAsset(List<TradeAssetData> assets)
    {
        foreach (TradeAssetData asset in assets)
        {
            if (asset.AssetType == "Player")
            {
                return asset;
            }
        }

        return null;
    }

    private static int CountPlayerAssets(List<TradeAssetData> assets)
    {
        int count = 0;
        foreach (TradeAssetData asset in assets)
        {
            if (asset.AssetType == "Player")
            {
                count++;
            }
        }

        return count;
    }

    private static int SumPlayerSalary(List<TradeAssetData> assets)
    {
        int salary = 0;
        foreach (TradeAssetData asset in assets)
        {
            if (asset.AssetType == "Player")
            {
                salary += asset.Salary;
            }
        }

        return salary;
    }

    private static void AppendFloorWarnings(TeamData userTeam, TeamData otherTeam, ref string message)
    {
        TeamFinanceData userFinance = SalaryCapService.CalculateTeamFinance(userTeam);
        TeamFinanceData otherFinance = SalaryCapService.CalculateTeamFinance(otherTeam);
        if (userFinance.IsBelowFloor)
        {
            message += ". Ваша команда ниже минимального порога зарплат";
        }

        if (otherFinance.IsBelowFloor)
        {
            message += ". Команда-соперник ниже минимального порога зарплат";
        }
    }

    private static void AddProposalToHistory(GameState state, TradeProposalData proposal)
    {
        EnsureTradeHistory(state);
        if (state != null && state.TradeHistory != null && proposal != null)
        {
            proposal.EnsureAssets();
            state.TradeHistory.Trades.Add(proposal);
        }
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

    private static void EnsureTeamPlayers(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        team.EnsureDraftRights();
        if (team.Players.Count == 0)
        {
            team.Players = PlayerSeedData.CreatePlayersForTeam(team.Id);
        }

        ContractGenerator.EnsureContractsForTeam(team);
    }

    private static int GetSalaryCapUpperLimit(GameState state)
    {
        return state.LeagueRules == null || state.LeagueRules.SalaryCapUpperLimit <= 0
            ? SalaryCapConfig.SalaryCapUpperLimit
            : state.LeagueRules.SalaryCapUpperLimit;
    }

    private static int GetMinRosterSize(GameState state)
    {
        return state.LeagueRules == null || state.LeagueRules.MinRosterSize <= 0
            ? SalaryCapConfig.MinRosterSize
            : state.LeagueRules.MinRosterSize;
    }

    private static int GetMaxRosterSize(GameState state)
    {
        return state.LeagueRules == null || state.LeagueRules.MaxRosterSize <= 0
            ? SalaryCapConfig.MaxRosterSize
            : state.LeagueRules.MaxRosterSize;
    }

    private static string GetTeamName(TeamData team)
    {
        return team == null ? "" : team.City + " " + team.Name;
    }
}
