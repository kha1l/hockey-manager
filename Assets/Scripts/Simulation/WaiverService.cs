using System;
using System.Collections.Generic;

public static class WaiverService
{
    private const int CpuClaimMinimumScore = 120;

    public static void EnsureWaiverWire(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureWaiverWire();
        NormalizeWaiverPlayers(state);
        CancelRetiredPlayerWaivers(state);
        CancelMissingPlayerWaivers(state);
        ResetOrphanPlayerWaiverFlags(state);
    }

    public static bool IsPlayerOnActiveWaivers(GameState state, string playerId)
    {
        return GetActiveWaiverForPlayer(state, playerId) != null;
    }

    public static WaiverPlayerData GetActiveWaiverForPlayer(GameState state, string playerId)
    {
        EnsureWaiverWire(state);
        if (state == null || state.WaiverWire == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (WaiverPlayerData waiver in state.WaiverWire.ActiveWaivers)
        {
            if (waiver != null
                && waiver.PlayerId == playerId
                && waiver.Status == WaiverConfig.WaiverWireStatusActive)
            {
                return waiver;
            }
        }

        return null;
    }

    public static RosterMoveResultData PlacePlayerOnWaivers(
        GameState state,
        TeamData originalTeam,
        string playerId,
        string intendedDestination)
    {
        EnsureWaiverWire(state);
        if (state == null)
        {
            return CreateMoveResult(false, "Состояние игры не найдено", playerId, "", "", intendedDestination);
        }

        if (originalTeam == null)
        {
            return CreateMoveResult(false, "Команда не найдена", playerId, "", "", intendedDestination);
        }

        if (!WaiverConfig.IsValidDestination(intendedDestination))
        {
            return CreateMoveResult(false, "Некорректное назначение waivers", playerId, "", "", intendedDestination);
        }

        PlayerData player = FindPlayer(originalTeam, playerId);
        if (player == null)
        {
            return CreateMoveResult(false, "Игрок не найден", playerId, "", "", intendedDestination);
        }

        if (player.IsRetired)
        {
            return CreateMoveResult(false, "Игрок завершил карьеру", player, intendedDestination);
        }

        TeamRosterService.EnsureRosterStatusesForTeam(originalTeam);
        WaiverEligibilityService.EnsureWaiverEligibility(player);

        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            return CreateMoveResult(false, "Игрок не в Pro составе", player, intendedDestination);
        }

        if (IsPlayerOnActiveWaivers(state, player.Id))
        {
            return CreateMoveResult(false, "Игрок уже на waivers", player, intendedDestination);
        }

        if (!player.RequiresWaivers)
        {
            return CreateMoveResult(false, "Waivers не нужны", player, intendedDestination);
        }

        DateTime now = DateTime.UtcNow;
        WaiverPlayerData waiver = new WaiverPlayerData
        {
            WaiverId = Guid.NewGuid().ToString("N"),
            PlayerId = player.Id,
            PlayerName = player.FirstName + " " + player.LastName,
            Position = player.Position,
            Age = player.Age,
            Overall = player.Overall,
            Potential = player.Potential,
            Salary = player.Salary,
            ContractYearsRemaining = player.ContractYearsRemaining,
            OriginalTeamId = originalTeam.Id,
            OriginalTeamName = GetTeamName(originalTeam),
            IntendedDestination = intendedDestination,
            Status = WaiverConfig.WaiverWireStatusActive,
            PlacedAtUtc = now.ToString("o"),
            ExpiresAtUtc = now.AddDays(WaiverConfig.WaiverDurationGameDays).ToString("o"),
            DaysRemaining = WaiverConfig.WaiverDurationGameDays,
            ClaimedByUser = false,
            ClaimedByTeamId = "",
            ClaimedByTeamName = "",
            ResolvedAtUtc = "",
            Resolution = ""
        };

        state.WaiverWire.ActiveWaivers.Add(waiver);
        player.IsOnWaivers = true;
        player.WaiverStatus = WaiverConfig.WaiverStatusOnWaivers;
        player.WaiverPlacedAtUtc = waiver.PlacedAtUtc;
        player.WaiverExpiresAtUtc = waiver.ExpiresAtUtc;
        player.WaiverOriginalTeamId = waiver.OriginalTeamId;
        player.WaiverOriginalTeamName = waiver.OriginalTeamName;
        player.WaiverIntendedDestination = intendedDestination;

        return CreateMoveResult(
            true,
            "Игрок выставлен на waivers. Если его никто не заберёт, он перейдёт в "
                + FormatDestination(intendedDestination) + ".",
            player,
            WaiverConfig.WaiverStatusOnWaivers);
    }

    public static void AdvanceWaiverDay(GameState state)
    {
        EnsureWaiverWire(state);
        if (state == null || state.WaiverWire == null)
        {
            return;
        }

        foreach (WaiverPlayerData waiver in state.WaiverWire.ActiveWaivers)
        {
            if (waiver != null && waiver.Status == WaiverConfig.WaiverWireStatusActive)
            {
                waiver.DaysRemaining--;
            }
        }

        ResolveExpiredWaivers(state);
    }

    public static void ResolveExpiredWaivers(GameState state)
    {
        EnsureWaiverWire(state);
        if (state == null || state.WaiverWire == null)
        {
            return;
        }

        List<WaiverPlayerData> activeWaivers = new List<WaiverPlayerData>(state.WaiverWire.ActiveWaivers);
        foreach (WaiverPlayerData waiver in activeWaivers)
        {
            if (waiver == null
                || waiver.Status != WaiverConfig.WaiverWireStatusActive
                || waiver.DaysRemaining > 0)
            {
                continue;
            }

            if (TryCpuClaimsForWaiver(state, waiver, out WaiverClaimData acceptedClaim) && acceptedClaim != null)
            {
                TeamData fromTeam;
                FindPlayerById(state, waiver.PlayerId, out fromTeam);
                TeamData toTeam = FindTeamById(state, acceptedClaim.ClaimingTeamId);
                TransferPlayerToClaimingTeam(state, waiver, fromTeam, toTeam, "ClaimedByCpu");
            }
            else
            {
                ClearWaiverToIntendedDestination(state, waiver);
            }
        }
    }

    public static bool TryUserClaimWaiverPlayer(
        GameState state,
        TeamData userTeam,
        string waiverId,
        out string message)
    {
        EnsureWaiverWire(state);
        WaiverPlayerData waiver = FindActiveWaiverById(state, waiverId);
        if (waiver == null)
        {
            message = "Игрок на waivers не найден";
            return false;
        }

        if (userTeam == null)
        {
            message = "Команда пользователя не найдена";
            return false;
        }

        if (userTeam.Id == waiver.OriginalTeamId)
        {
            message = "Нельзя забрать своего игрока с waivers";
            return false;
        }

        if (!CanTeamClaimPlayer(userTeam, waiver, state, out message))
        {
            return false;
        }

        int claimScore = CalculateCpuClaimScore(userTeam, waiver, state);
        TeamData fromTeam;
        FindPlayerById(state, waiver.PlayerId, out fromTeam);
        TransferPlayerToClaimingTeam(state, waiver, fromTeam, userTeam, "ClaimedByUser");
        AddClaim(state, waiver, userTeam, "Accepted", claimScore);
        message = "Игрок забран с waivers";
        return true;
    }

    public static bool TryCpuClaimsForWaiver(
        GameState state,
        WaiverPlayerData waiver,
        out WaiverClaimData acceptedClaim)
    {
        acceptedClaim = null;
        EnsureWaiverWire(state);
        if (state == null || state.Teams == null || waiver == null)
        {
            return false;
        }

        TeamData bestTeam = null;
        int bestScore = 0;
        foreach (TeamData team in state.Teams)
        {
            if (team == null
                || team.Id == waiver.OriginalTeamId
                || team.Id == state.SelectedTeamId
                || !CanTeamClaimPlayer(team, waiver, state, out string reason))
            {
                continue;
            }

            int score = CalculateCpuClaimScore(team, waiver, state);
            if (score > bestScore)
            {
                bestScore = score;
                bestTeam = team;
            }
        }

        if (bestTeam == null || bestScore < CpuClaimMinimumScore)
        {
            return false;
        }

        acceptedClaim = AddClaim(state, waiver, bestTeam, "Accepted", bestScore);
        return true;
    }

    public static int CalculateCpuClaimScore(
        TeamData claimingTeam,
        WaiverPlayerData waiver,
        GameState state)
    {
        if (!CanTeamClaimPlayer(claimingTeam, waiver, state, out string reason))
        {
            return 0;
        }

        int score = waiver.Overall * 5;
        if (HasPositionNeed(claimingTeam, waiver.Position))
        {
            score += 80;
        }

        if (waiver.Age <= 25)
        {
            score += 40;
        }

        if (waiver.Potential >= waiver.Overall + 5)
        {
            score += 40;
        }

        score -= (waiver.Salary / 1000000) * 10;
        if (score < 0)
        {
            return 0;
        }

        return score > WaiverConfig.MaxCpuClaimScore ? WaiverConfig.MaxCpuClaimScore : score;
    }

    public static bool CanTeamClaimPlayer(
        TeamData team,
        WaiverPlayerData waiver,
        GameState state,
        out string reason)
    {
        if (team == null)
        {
            reason = "Команда не найдена";
            return false;
        }

        if (waiver == null)
        {
            reason = "Waiver entry не найден";
            return false;
        }

        if (team.Id == waiver.OriginalTeamId)
        {
            reason = "Нельзя забрать своего игрока";
            return false;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        if (FindPlayer(team, waiver.PlayerId) != null)
        {
            reason = "Игрок уже есть в команде";
            return false;
        }

        if (TeamRosterService.GetNhlPlayers(team).Count >= RosterStatusConfig.MaxNhlRosterSize)
        {
            reason = "В Pro roster нет места";
            return false;
        }

        if (!SalaryCapService.CanAddSalary(team, waiver.Salary))
        {
            reason = "Недостаточно места под потолком зарплат";
            return false;
        }

        reason = "Claim возможен";
        return true;
    }

    private static PlayerData FindPlayerById(GameState state, string playerId, out TeamData team)
    {
        team = null;
        if (state == null || state.Teams == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (TeamData candidateTeam in state.Teams)
        {
            PlayerData player = FindPlayer(candidateTeam, playerId);
            if (player != null)
            {
                team = candidateTeam;
                return player;
            }
        }

        return null;
    }

    private static TeamData FindTeamById(GameState state, string teamId)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
    }

    private static void TransferPlayerToClaimingTeam(
        GameState state,
        WaiverPlayerData waiver,
        TeamData fromTeam,
        TeamData toTeam,
        string resolution)
    {
        if (state == null || waiver == null || toTeam == null)
        {
            MarkWaiverResolved(state, waiver, WaiverConfig.WaiverWireStatusCancelled, "Cancelled", "", "");
            return;
        }

        TeamData actualFromTeam;
        PlayerData player = FindPlayerById(state, waiver.PlayerId, out actualFromTeam);
        fromTeam = actualFromTeam ?? fromTeam;
        if (player == null || player.IsRetired || fromTeam == null)
        {
            MarkWaiverResolved(state, waiver, WaiverConfig.WaiverWireStatusCancelled, "Cancelled", "", "");
            return;
        }

        fromTeam.EnsurePlayers();
        toTeam.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(fromTeam);
        TeamRosterService.EnsureRosterStatusesForTeam(toTeam);
        if (player.IsCaptain || player.IsAlternateCaptain)
        {
            LeadershipService.ClearCaptaincy(fromTeam, player.Id);
        }

        fromTeam.Players.Remove(player);
        if (FindPlayer(toTeam, player.Id) == null)
        {
            toTeam.Players.Add(player);
        }

        LeadershipService.ClearPlayerCaptaincy(player);
        string oldStatus = string.IsNullOrEmpty(player.RosterStatus) ? RosterStatusConfig.NHL : player.RosterStatus;
        player.TeamId = toTeam.Id;
        player.PreviousRosterStatus = oldStatus;
        player.RosterStatus = RosterStatusConfig.NHL;
        player.RosterStatusUpdatedAtUtc = DateTime.UtcNow.ToString("o");
        player.IsOnWaivers = false;
        player.WaiverStatus = WaiverConfig.WaiverStatusClaimed;
        player.WaiverIntendedDestination = "";
        player.WaiverOriginalTeamId = waiver.OriginalTeamId;
        player.WaiverOriginalTeamName = waiver.OriginalTeamName;

        RemoveFromScratches(fromTeam, player.Id);
        AddToScratchesIfNeeded(toTeam, player.Id);
        LineupService.SyncScratchPlayers(fromTeam);
        LineupService.SyncScratchPlayers(toTeam);
        LineupService.ValidateLineup(fromTeam, out string fromLineupMessage);
        LineupService.ValidateLineup(toTeam, out string toLineupMessage);
        SpecialTeamsService.ValidateSpecialTeams(fromTeam, out string fromSpecialTeamsMessage);
        SpecialTeamsService.ValidateSpecialTeams(toTeam, out string toSpecialTeamsMessage);
        MarkWaiverResolved(
            state,
            waiver,
            WaiverConfig.WaiverWireStatusClaimed,
            resolution,
            toTeam.Id,
            GetTeamName(toTeam));
    }

    private static void ClearWaiverToIntendedDestination(GameState state, WaiverPlayerData waiver)
    {
        if (state == null || waiver == null)
        {
            return;
        }

        TeamData team;
        PlayerData player = FindPlayerById(state, waiver.PlayerId, out team);
        if (player == null || team == null)
        {
            MarkWaiverResolved(state, waiver, WaiverConfig.WaiverWireStatusCancelled, "Cancelled", "", "");
            return;
        }

        string destination = WaiverConfig.IsValidDestination(waiver.IntendedDestination)
            ? waiver.IntendedDestination
            : WaiverConfig.DestinationFarm;
        string oldStatus = string.IsNullOrEmpty(player.RosterStatus) ? RosterStatusConfig.NHL : player.RosterStatus;
        player.PreviousRosterStatus = oldStatus;
        player.RosterStatus = destination == WaiverConfig.DestinationReserve
            ? RosterStatusConfig.Reserve
            : RosterStatusConfig.Farm;
        player.RosterStatusUpdatedAtUtc = DateTime.UtcNow.ToString("o");
        player.IsOnWaivers = false;
        player.WaiverStatus = WaiverConfig.WaiverStatusCleared;
        player.WaiverIntendedDestination = "";
        if (player.IsCaptain || player.IsAlternateCaptain)
        {
            LeadershipService.ClearCaptaincy(team, player.Id);
        }

        RemoveFromScratches(team, player.Id);
        LineupService.SyncScratchPlayers(team);
        LineupService.ValidateLineup(team, out string lineupMessage);
        SpecialTeamsService.ValidateSpecialTeams(team, out string specialTeamsMessage);

        MarkWaiverResolved(
            state,
            waiver,
            WaiverConfig.WaiverWireStatusCleared,
            destination == WaiverConfig.DestinationReserve ? "ClearedToReserve" : "ClearedToFarm",
            "",
            "");
    }

    private static void MarkWaiverResolved(
        GameState state,
        WaiverPlayerData waiver,
        string status,
        string resolution,
        string claimedByTeamId,
        string claimedByTeamName)
    {
        if (state == null || waiver == null || state.WaiverWire == null)
        {
            return;
        }

        state.WaiverWire.EnsureCollections();
        waiver.Status = status;
        waiver.Resolution = resolution;
        waiver.ClaimedByUser = resolution == "ClaimedByUser";
        waiver.ClaimedByTeamId = claimedByTeamId ?? "";
        waiver.ClaimedByTeamName = claimedByTeamName ?? "";
        waiver.ResolvedAtUtc = DateTime.UtcNow.ToString("o");
        state.WaiverWire.ActiveWaivers.Remove(waiver);
        if (!state.WaiverWire.WaiverHistory.Contains(waiver))
        {
            state.WaiverWire.WaiverHistory.Add(waiver);
        }
    }

    private static WaiverClaimData AddClaim(GameState state, WaiverPlayerData waiver, TeamData team, string status, int score)
    {
        if (state == null || state.WaiverWire == null || waiver == null || team == null)
        {
            return null;
        }

        state.WaiverWire.EnsureCollections();
        WaiverClaimData claim = new WaiverClaimData
        {
            ClaimId = Guid.NewGuid().ToString("N"),
            WaiverId = waiver.WaiverId,
            PlayerId = waiver.PlayerId,
            PlayerName = waiver.PlayerName,
            ClaimingTeamId = team.Id,
            ClaimingTeamName = GetTeamName(team),
            OriginalTeamId = waiver.OriginalTeamId,
            OriginalTeamName = waiver.OriginalTeamName,
            CreatedAtUtc = DateTime.UtcNow.ToString("o"),
            Status = status,
            ClaimScore = score
        };

        state.WaiverWire.Claims.Add(claim);
        return claim;
    }

    private static WaiverPlayerData FindActiveWaiverById(GameState state, string waiverId)
    {
        if (state == null || state.WaiverWire == null || string.IsNullOrEmpty(waiverId))
        {
            return null;
        }

        foreach (WaiverPlayerData waiver in state.WaiverWire.ActiveWaivers)
        {
            if (waiver != null
                && waiver.WaiverId == waiverId
                && waiver.Status == WaiverConfig.WaiverWireStatusActive)
            {
                return waiver;
            }
        }

        return null;
    }

    private static void NormalizeWaiverPlayers(GameState state)
    {
        if (state == null || state.WaiverWire == null)
        {
            return;
        }

        NormalizeWaiverList(state.WaiverWire.ActiveWaivers);
        NormalizeWaiverList(state.WaiverWire.WaiverHistory);
        if (state.WaiverWire.Claims == null)
        {
            state.WaiverWire.Claims = new List<WaiverClaimData>();
        }
    }

    private static void NormalizeWaiverList(List<WaiverPlayerData> waivers)
    {
        if (waivers == null)
        {
            return;
        }

        foreach (WaiverPlayerData waiver in waivers)
        {
            if (waiver == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(waiver.Status))
            {
                waiver.Status = WaiverConfig.WaiverWireStatusActive;
            }

            if (waiver.DaysRemaining < 0)
            {
                waiver.DaysRemaining = 0;
            }

            if (waiver.Resolution == null)
            {
                waiver.Resolution = "";
            }
        }
    }

    private static void CancelMissingPlayerWaivers(GameState state)
    {
        if (state == null || state.WaiverWire == null)
        {
            return;
        }

        List<WaiverPlayerData> activeWaivers = new List<WaiverPlayerData>(state.WaiverWire.ActiveWaivers);
        foreach (WaiverPlayerData waiver in activeWaivers)
        {
            if (waiver == null || waiver.Status != WaiverConfig.WaiverWireStatusActive)
            {
                continue;
            }

            TeamData team;
            if (FindPlayerById(state, waiver.PlayerId, out team) == null)
            {
                MarkWaiverResolved(state, waiver, WaiverConfig.WaiverWireStatusCancelled, "Cancelled", "", "");
            }
        }
    }

    private static void CancelRetiredPlayerWaivers(GameState state)
    {
        if (state == null || state.WaiverWire == null)
        {
            return;
        }

        List<WaiverPlayerData> activeWaivers = new List<WaiverPlayerData>(state.WaiverWire.ActiveWaivers);
        foreach (WaiverPlayerData waiver in activeWaivers)
        {
            if (waiver == null || waiver.Status != WaiverConfig.WaiverWireStatusActive)
            {
                continue;
            }

            TeamData team;
            PlayerData player = FindPlayerById(state, waiver.PlayerId, out team);
            if (player != null && player.IsRetired)
            {
                MarkWaiverResolved(state, waiver, WaiverConfig.WaiverWireStatusCancelled, "Retired", "", "");
            }
        }
    }

    private static void ResetOrphanPlayerWaiverFlags(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return;
        }

        HashSet<string> activePlayerIds = new HashSet<string>();
        if (state.WaiverWire != null && state.WaiverWire.ActiveWaivers != null)
        {
            foreach (WaiverPlayerData waiver in state.WaiverWire.ActiveWaivers)
            {
                if (waiver != null && waiver.Status == WaiverConfig.WaiverWireStatusActive)
                {
                    activePlayerIds.Add(waiver.PlayerId);
                }
            }
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                WaiverEligibilityService.EnsureWaiverFields(player);
                if (player != null && player.IsOnWaivers && !activePlayerIds.Contains(player.Id))
                {
                    player.IsOnWaivers = false;
                    player.WaiverStatus = WaiverConfig.WaiverStatusNone;
                    player.WaiverIntendedDestination = "";
                }
            }
        }
    }

    private static bool HasPositionNeed(TeamData team, string position)
    {
        TeamRosterSummaryData summary = TeamRosterService.GetRosterSummary(team);
        if (position == "G")
        {
            return summary.NhlGoalies < 2;
        }

        if (position == "D")
        {
            return summary.NhlDefensemen < 6;
        }

        return summary.NhlForwards < 12;
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

    private static void AddToScratchesIfNeeded(TeamData team, string playerId)
    {
        if (team == null || team.Lineup == null || string.IsNullOrEmpty(playerId))
        {
            return;
        }

        team.Lineup.EnsureCollections();
        if (!LineupService.IsPlayerInLineup(team.Lineup, playerId) && !team.Lineup.ScratchPlayerIds.Contains(playerId))
        {
            team.Lineup.ScratchPlayerIds.Add(playerId);
            team.Lineup.Touch();
        }
    }

    private static void RemoveFromScratches(TeamData team, string playerId)
    {
        if (team == null || team.Lineup == null || string.IsNullOrEmpty(playerId))
        {
            return;
        }

        team.Lineup.EnsureCollections();
        team.Lineup.ScratchPlayerIds.Remove(playerId);
        team.Lineup.Touch();
    }

    private static RosterMoveResultData CreateMoveResult(bool success, string message, PlayerData player, string toStatus)
    {
        return CreateMoveResult(
            success,
            message,
            player == null ? "" : player.Id,
            player == null ? "" : player.FirstName + " " + player.LastName,
            player == null ? "" : player.RosterStatus,
            toStatus);
    }

    private static RosterMoveResultData CreateMoveResult(
        bool success,
        string message,
        string playerId,
        string playerName,
        string fromStatus,
        string toStatus)
    {
        return new RosterMoveResultData
        {
            Success = success,
            Message = message,
            PlayerId = playerId,
            PlayerName = playerName,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static string FormatDestination(string destination)
    {
        return destination == WaiverConfig.DestinationReserve ? "резерв" : "фарм";
    }
}
