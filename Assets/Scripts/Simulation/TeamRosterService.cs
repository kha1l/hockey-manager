using System;
using System.Collections.Generic;

public static class TeamRosterService
{
    public static void EnsureRosterStatusesForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.IsRetired)
            {
                continue;
            }

            EnsureRosterStatusForPlayer(player, RosterStatusConfig.NHL);
            WaiverEligibilityService.EnsureWaiverEligibility(player);
        }
    }

    public static void EnsureRosterStatusesForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureRosterStatusesForTeam(team);
        }
    }

    public static void EnsureFreeAgentRosterStatuses(List<PlayerData> players)
    {
        if (players == null)
        {
            return;
        }

        foreach (PlayerData player in players)
        {
            if (player != null && player.IsRetired)
            {
                continue;
            }

            EnsureRosterStatusForPlayer(player, RosterStatusConfig.FreeAgent);
            WaiverEligibilityService.EnsureWaiverEligibility(player);
        }
    }

    public static List<PlayerData> GetNhlPlayers(TeamData team)
    {
        return GetPlayersByRosterStatus(team, RosterStatusConfig.NHL);
    }

    public static List<PlayerData> GetFarmPlayers(TeamData team)
    {
        return GetPlayersByRosterStatus(team, RosterStatusConfig.Farm);
    }

    public static List<PlayerData> GetReservePlayers(TeamData team)
    {
        return GetPlayersByRosterStatus(team, RosterStatusConfig.Reserve);
    }

    public static List<PlayerData> GetAvailableNhlPlayers(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        foreach (PlayerData player in GetNhlPlayers(team))
        {
            if (InjuryService.IsPlayerAvailable(player))
            {
                players.Add(player);
            }
        }

        SortPlayers(players);
        return players;
    }

    public static TeamRosterSummaryData GetRosterSummary(TeamData team)
    {
        EnsureRosterStatusesForTeam(team);
        TeamRosterSummaryData summary = new TeamRosterSummaryData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = TeamIdentityService.GetDisplayName(team),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        if (team == null || team.Players == null)
        {
            summary.ValidationMessage = "Команда не найдена";
            return summary;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player == null || player.IsRetired)
            {
                continue;
            }

            summary.TotalPlayers++;
            InjuryService.EnsureInjuryFields(player);
            if (player.IsInjured)
            {
                summary.InjuredPlayers++;
            }

            if (RosterStatusConfig.IsNhlRoster(player))
            {
                summary.NhlPlayers++;
                if (IsForward(player))
                {
                    summary.NhlForwards++;
                }
                else if (player.Position == "D")
                {
                    summary.NhlDefensemen++;
                }
                else if (player.Position == "G")
                {
                    summary.NhlGoalies++;
                }

                if (InjuryService.IsPlayerAvailable(player))
                {
                    summary.AvailableNhlPlayers++;
                }
            }
            else if (RosterStatusConfig.IsFarmRoster(player))
            {
                summary.FarmPlayers++;
            }
            else if (RosterStatusConfig.IsReserve(player))
            {
                summary.ReservePlayers++;
            }
        }

        summary.IsNhlRosterValid = ValidateNhlRoster(team, out string message);
        summary.ValidationMessage = HasPlayersOnWaivers(team)
            ? message + ". Есть игроки на waivers"
            : message;
        return summary;
    }

    public static bool ValidateNhlRoster(TeamData team, out string message)
    {
        EnsureRosterStatusesForTeam(team);
        if (team == null)
        {
            message = "Команда не найдена";
            return false;
        }

        List<PlayerData> nhlPlayers = GetNhlPlayers(team);
        if (nhlPlayers.Count < RosterStatusConfig.MinNhlRosterSize)
        {
            message = "В Pro roster меньше 20 игроков";
            return false;
        }

        if (nhlPlayers.Count > RosterStatusConfig.MaxNhlRosterSize)
        {
            message = "В Pro roster больше 23 игроков";
            return false;
        }

        int forwards = 0;
        int defensemen = 0;
        int goalies = 0;
        int availableForwards = 0;
        int availableDefensemen = 0;
        int availableGoalies = 0;

        foreach (PlayerData player in nhlPlayers)
        {
            bool available = InjuryService.IsPlayerAvailable(player);
            if (IsForward(player))
            {
                forwards++;
                if (available)
                {
                    availableForwards++;
                }
            }
            else if (player.Position == "D")
            {
                defensemen++;
                if (available)
                {
                    availableDefensemen++;
                }
            }
            else if (player.Position == "G")
            {
                goalies++;
                if (available)
                {
                    availableGoalies++;
                }
            }
        }

        if (forwards < 12 || defensemen < 6 || goalies < 2)
        {
            message = "Pro roster должен иметь минимум 12 нападающих, 6 защитников и 2 вратарей";
            return false;
        }

        if (availableForwards < 12 || availableDefensemen < 6 || availableGoalies < 2)
        {
            message = "Недостаточно доступных Pro игроков: нужно 12 F, 6 D и 2 G без травм";
            return false;
        }

        message = "Pro roster валиден";
        return true;
    }

    public static RosterMoveResultData SendPlayerToFarm(TeamData team, string playerId)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player == null)
        {
            return CreateResult(false, "Игрок не найден", playerId, "", "", RosterStatusConfig.Farm);
        }

        if (player.IsRetired)
        {
            return CreateResult(false, "Игрок завершил карьеру", player, RosterStatusConfig.Farm);
        }

        EnsureRosterStatusForPlayer(player, RosterStatusConfig.NHL);
        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            return CreateResult(false, "В фарм можно отправить только игрока из Pro roster", player, RosterStatusConfig.Farm);
        }

        if (GetNhlPlayers(team).Count <= RosterStatusConfig.MinNhlRosterSize)
        {
            return CreateResult(false, "Нельзя отправить игрока: Pro roster станет меньше 20 игроков", player, RosterStatusConfig.Farm);
        }

        SetRosterStatus(player, RosterStatusConfig.Farm);
        ResetWaiverState(player, WaiverConfig.WaiverStatusNone);
        string captaincyMessage = ClearCaptaincyForNhlExit(team, player);
        RemoveFromScratches(team, player.Id);
        ValidateLineupAfterNhlMove(team);
        return CreateResult(true, "Игрок отправлен в фарм. Проверьте линии." + captaincyMessage, player, RosterStatusConfig.Farm);
    }

    public static RosterMoveResultData SendPlayerToFarm(GameState state, TeamData team, string playerId)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player == null)
        {
            return CreateResult(false, "Игрок не найден", playerId, "", "", RosterStatusConfig.Farm);
        }

        if (player.IsRetired)
        {
            return CreateResult(false, "Игрок завершил карьеру", player, RosterStatusConfig.Farm);
        }

        EnsureRosterStatusForPlayer(player, RosterStatusConfig.NHL);
        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            return CreateResult(false, "В фарм можно отправить только игрока из Pro roster", player, RosterStatusConfig.Farm);
        }

        if (GetNhlPlayers(team).Count <= RosterStatusConfig.MinNhlRosterSize)
        {
            return CreateResult(false, "Нельзя отправить игрока: Pro roster станет меньше 20 игроков", player, RosterStatusConfig.Farm);
        }

        WaiverEligibilityService.EnsureWaiverEligibility(player);
        if (player.RequiresWaivers)
        {
            return WaiverService.PlacePlayerOnWaivers(state, team, playerId, WaiverConfig.DestinationFarm);
        }

        return SendPlayerToFarm(team, playerId);
    }

    public static RosterMoveResultData CallUpPlayerToNhl(TeamData team, string playerId)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player == null)
        {
            return CreateResult(false, "Игрок не найден", playerId, "", "", RosterStatusConfig.NHL);
        }

        if (player.IsRetired)
        {
            return CreateResult(false, "Игрок завершил карьеру", player, RosterStatusConfig.NHL);
        }

        EnsureRosterStatusForPlayer(player, RosterStatusConfig.NHL);
        if (player.IsOnWaivers)
        {
            return CreateResult(false, "Нельзя вызвать игрока, пока он на waivers", player, RosterStatusConfig.NHL);
        }

        if (!RosterStatusConfig.IsFarmRoster(player))
        {
            return CreateResult(false, "В Pro можно вызвать только игрока из фарма", player, RosterStatusConfig.NHL);
        }

        if (GetNhlPlayers(team).Count >= RosterStatusConfig.MaxNhlRosterSize)
        {
            return CreateResult(false, "Нельзя вызвать игрока: Pro roster уже содержит 23 игрока", player, RosterStatusConfig.NHL);
        }

        SetRosterStatus(player, RosterStatusConfig.NHL);
        ResetWaiverState(player, WaiverConfig.WaiverStatusNone);
        AddToScratchesIfNeeded(team, player.Id);
        ValidateLineupAfterNhlMove(team);
        return CreateResult(true, "Игрок вызван в Pro roster", player, RosterStatusConfig.NHL);
    }

    public static RosterMoveResultData MovePlayerToReserve(TeamData team, string playerId)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player == null)
        {
            return CreateResult(false, "Игрок не найден", playerId, "", "", RosterStatusConfig.Reserve);
        }

        if (player.IsRetired)
        {
            return CreateResult(false, "Игрок завершил карьеру", player, RosterStatusConfig.Reserve);
        }

        EnsureRosterStatusForPlayer(player, RosterStatusConfig.NHL);
        if (!RosterStatusConfig.IsNhlRoster(player) && !RosterStatusConfig.IsFarmRoster(player))
        {
            return CreateResult(false, "В резерв можно перевести только игрока Pro или фарма", player, RosterStatusConfig.Reserve);
        }

        if (RosterStatusConfig.IsNhlRoster(player) && GetNhlPlayers(team).Count <= RosterStatusConfig.MinNhlRosterSize)
        {
            return CreateResult(false, "Нельзя перевести игрока в резерв: Pro roster станет меньше 20 игроков", player, RosterStatusConfig.Reserve);
        }

        SetRosterStatus(player, RosterStatusConfig.Reserve);
        ResetWaiverState(player, WaiverConfig.WaiverStatusNone);
        string captaincyMessage = ClearCaptaincyForNhlExit(team, player);
        RemoveFromScratches(team, player.Id);
        ValidateLineupAfterNhlMove(team);
        return CreateResult(true, "Игрок переведён в резерв. Проверьте линии." + captaincyMessage, player, RosterStatusConfig.Reserve);
    }

    public static RosterMoveResultData MovePlayerToReserve(GameState state, TeamData team, string playerId)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player == null)
        {
            return CreateResult(false, "Игрок не найден", playerId, "", "", RosterStatusConfig.Reserve);
        }

        if (player.IsRetired)
        {
            return CreateResult(false, "Игрок завершил карьеру", player, RosterStatusConfig.Reserve);
        }

        EnsureRosterStatusForPlayer(player, RosterStatusConfig.NHL);
        if (!RosterStatusConfig.IsNhlRoster(player) && !RosterStatusConfig.IsFarmRoster(player))
        {
            return CreateResult(false, "В резерв можно перевести только игрока Pro или фарма", player, RosterStatusConfig.Reserve);
        }

        if (RosterStatusConfig.IsNhlRoster(player) && GetNhlPlayers(team).Count <= RosterStatusConfig.MinNhlRosterSize)
        {
            return CreateResult(false, "Нельзя перевести игрока в резерв: Pro roster станет меньше 20 игроков", player, RosterStatusConfig.Reserve);
        }

        WaiverEligibilityService.EnsureWaiverEligibility(player);
        if (RosterStatusConfig.IsNhlRoster(player) && player.RequiresWaivers)
        {
            return WaiverService.PlacePlayerOnWaivers(state, team, playerId, WaiverConfig.DestinationReserve);
        }

        return MovePlayerToReserve(team, playerId);
    }

    public static RosterMoveResultData MoveReservePlayerToNhl(TeamData team, string playerId)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player == null)
        {
            return CreateResult(false, "Игрок не найден", playerId, "", "", RosterStatusConfig.NHL);
        }

        if (player.IsRetired)
        {
            return CreateResult(false, "Игрок завершил карьеру", player, RosterStatusConfig.NHL);
        }

        EnsureRosterStatusForPlayer(player, RosterStatusConfig.NHL);
        if (!RosterStatusConfig.IsReserve(player))
        {
            return CreateResult(false, "В Pro можно вернуть только игрока из резерва", player, RosterStatusConfig.NHL);
        }

        if (GetNhlPlayers(team).Count >= RosterStatusConfig.MaxNhlRosterSize)
        {
            return CreateResult(false, "Нельзя вернуть игрока: Pro roster уже содержит 23 игрока", player, RosterStatusConfig.NHL);
        }

        SetRosterStatus(player, RosterStatusConfig.NHL);
        ResetWaiverState(player, WaiverConfig.WaiverStatusNone);
        AddToScratchesIfNeeded(team, player.Id);
        ValidateLineupAfterNhlMove(team);
        return CreateResult(true, "Игрок возвращён из резерва в Pro roster", player, RosterStatusConfig.NHL);
    }

    public static RosterMoveResultData MoveReservePlayerToFarm(TeamData team, string playerId)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player == null)
        {
            return CreateResult(false, "Игрок не найден", playerId, "", "", RosterStatusConfig.Farm);
        }

        if (player.IsRetired)
        {
            return CreateResult(false, "Игрок завершил карьеру", player, RosterStatusConfig.Farm);
        }

        EnsureRosterStatusForPlayer(player, RosterStatusConfig.NHL);
        if (!RosterStatusConfig.IsReserve(player))
        {
            return CreateResult(false, "В фарм можно вернуть только игрока из резерва", player, RosterStatusConfig.Farm);
        }

        SetRosterStatus(player, RosterStatusConfig.Farm);
        ResetWaiverState(player, WaiverConfig.WaiverStatusNone);
        RemoveFromScratches(team, player.Id);
        return CreateResult(true, "Игрок возвращён из резерва в фарм", player, RosterStatusConfig.Farm);
    }

    public static void AdvanceRosterDaysForTeam(TeamData team)
    {
        EnsureRosterStatusesForTeam(team);
        if (team == null || team.Players == null)
        {
            return;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player == null || player.IsRetired)
            {
                continue;
            }

            if (player.IsOnWaivers)
            {
                continue;
            }

            if (RosterStatusConfig.IsFarmRoster(player))
            {
                player.FarmDaysThisSeason++;
            }
            else if (RosterStatusConfig.IsReserve(player))
            {
                player.ReserveDaysThisSeason++;
            }
        }
    }

    public static void SetInitialRosterStatus(PlayerData player, string rosterStatus)
    {
        if (player == null)
        {
            return;
        }

        if (!RosterStatusConfig.IsValidRosterStatus(rosterStatus))
        {
            rosterStatus = RosterStatusConfig.NHL;
        }

        if (string.IsNullOrEmpty(player.RosterStatus) || !RosterStatusConfig.IsValidRosterStatus(player.RosterStatus))
        {
            player.RosterStatus = rosterStatus;
        }

        if (string.IsNullOrEmpty(player.PreviousRosterStatus))
        {
            player.PreviousRosterStatus = player.RosterStatus;
        }

        if (string.IsNullOrEmpty(player.RosterStatusUpdatedAtUtc))
        {
            player.RosterStatusUpdatedAtUtc = DateTime.UtcNow.ToString("o");
        }
    }

    private static void EnsureRosterStatusForPlayer(PlayerData player, string defaultStatus)
    {
        SetInitialRosterStatus(player, defaultStatus);
        if (player != null && !RosterStatusConfig.IsValidRosterStatus(player.RosterStatus))
        {
            player.RosterStatus = defaultStatus;
        }
    }

    private static List<PlayerData> GetPlayersByRosterStatus(TeamData team, string rosterStatus)
    {
        List<PlayerData> players = new List<PlayerData>();
        EnsureRosterStatusesForTeam(team);
        if (team == null || team.Players == null)
        {
            return players;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null && !player.IsRetired && player.RosterStatus == rosterStatus)
            {
                players.Add(player);
            }
        }

        SortPlayers(players);
        return players;
    }

    private static void SortPlayers(List<PlayerData> players)
    {
        players.Sort(ComparePlayers);
    }

    private static int ComparePlayers(PlayerData left, PlayerData right)
    {
        int positionComparison = GetPositionOrder(left).CompareTo(GetPositionOrder(right));
        if (positionComparison != 0)
        {
            return positionComparison;
        }

        int overallComparison = right.Overall.CompareTo(left.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }

    private static int GetPositionOrder(PlayerData player)
    {
        if (player == null)
        {
            return 99;
        }

        if (player.Position == "C")
        {
            return 0;
        }

        if (player.Position == "LW")
        {
            return 1;
        }

        if (player.Position == "RW")
        {
            return 2;
        }

        if (player.Position == "D")
        {
            return 3;
        }

        if (player.Position == "G")
        {
            return 4;
        }

        return 5;
    }

    private static bool IsForward(PlayerData player)
    {
        return player != null && (player.Position == "C" || player.Position == "LW" || player.Position == "RW");
    }

    private static bool HasPlayersOnWaivers(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return false;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.IsOnWaivers)
            {
                return true;
            }
        }

        return false;
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

    private static void SetRosterStatus(PlayerData player, string toStatus)
    {
        if (player == null)
        {
            return;
        }

        string fromStatus = string.IsNullOrEmpty(player.RosterStatus) ? RosterStatusConfig.NHL : player.RosterStatus;
        player.PreviousRosterStatus = fromStatus;
        player.RosterStatus = toStatus;
        player.RosterStatusUpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    private static void ResetWaiverState(PlayerData player, string waiverStatus)
    {
        if (player == null)
        {
            return;
        }

        player.IsOnWaivers = false;
        player.WaiverStatus = string.IsNullOrEmpty(waiverStatus) ? WaiverConfig.WaiverStatusNone : waiverStatus;
        player.WaiverIntendedDestination = "";
    }

    private static string ClearCaptaincyForNhlExit(TeamData team, PlayerData player)
    {
        if (player == null || (!player.IsCaptain && !player.IsAlternateCaptain))
        {
            return "";
        }

        LeadershipService.ClearCaptaincy(team, player.Id);
        return " Captaincy cleared. Assign a new captain.";
    }

    private static void AddToScratchesIfNeeded(TeamData team, string playerId)
    {
        if (team == null || team.Lineup == null || string.IsNullOrEmpty(playerId))
        {
            return;
        }

        team.Lineup.EnsureCollections();
        if (LineupService.IsPlayerInLineup(team.Lineup, playerId) || team.Lineup.ScratchPlayerIds.Contains(playerId))
        {
            return;
        }

        team.Lineup.ScratchPlayerIds.Add(playerId);
        team.Lineup.Touch();
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

    private static void ValidateLineupAfterNhlMove(TeamData team)
    {
        if (team == null || team.Lineup == null)
        {
            return;
        }

        LineupService.ValidateLineup(team, out string message);
    }

    private static RosterMoveResultData CreateResult(bool success, string message, PlayerData player, string toStatus)
    {
        return CreateResult(
            success,
            message,
            player == null ? "" : player.Id,
            player == null ? "" : player.FirstName + " " + player.LastName,
            player == null ? "" : player.PreviousRosterStatus,
            toStatus);
    }

    private static RosterMoveResultData CreateResult(
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
}
