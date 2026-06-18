using System;
using System.Collections.Generic;
using UnityEngine;

public static class InjuryService
{
    public static void EnsureInjuryHistory(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.InjuryHistory == null)
        {
            state.InjuryHistory = new InjuryHistoryData();
        }

        state.InjuryHistory.EnsureInjuries();
    }

    public static void EnsureInjuryFields(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(player.InjuryType))
        {
            player.InjuryType = "";
        }

        if (string.IsNullOrEmpty(player.InjurySeverity))
        {
            player.InjurySeverity = "";
        }

        if (string.IsNullOrEmpty(player.InjuredAtUtc))
        {
            player.InjuredAtUtc = "";
        }

        if (string.IsNullOrEmpty(player.ExpectedReturnDate))
        {
            player.ExpectedReturnDate = "";
        }

        if (player.InjuryDaysRemaining < 0)
        {
            player.InjuryDaysRemaining = 0;
        }

        if (player.InjuryDaysRemaining > 0)
        {
            player.IsInjured = true;
            player.CanPlayThroughInjury = false;
            return;
        }

        player.IsInjured = false;
        player.InjuryDaysRemaining = 0;
        player.InjuryType = "";
        player.InjurySeverity = "";
        player.CanPlayThroughInjury = false;
        player.InjuredAtUtc = "";
        player.ExpectedReturnDate = "";
    }

    public static void EnsureInjuryFieldsForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            EnsureInjuryFields(player);
        }
    }

    public static void EnsureInjuryFieldsForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureInjuryFieldsForTeam(team);
        }
    }

    public static bool IsPlayerAvailable(PlayerData player)
    {
        EnsureInjuryFields(player);
        return player != null && !player.IsInjured;
    }

    public static List<PlayerData> GetInjuredPlayers(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (team == null)
        {
            return players;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            EnsureInjuryFields(player);
            if (player != null && player.IsInjured)
            {
                players.Add(player);
            }
        }

        players.Sort(CompareInjuredPlayers);
        return players;
    }

    public static List<PlayerData> GetAvailablePlayers(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (team == null)
        {
            return players;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (IsPlayerAvailable(player))
            {
                players.Add(player);
            }
        }

        return players;
    }

    public static void ApplyInjuryChecksAfterMatch(GameState state, TeamData homeTeam, TeamData awayTeam, string source)
    {
        ApplyInjuryChecksForTeam(state, homeTeam, source);
        ApplyInjuryChecksForTeam(state, awayTeam, source);
    }

    public static void ApplyInjuryChecksForTeam(GameState state, TeamData team, string source)
    {
        EnsureInjuryHistory(state);
        EnsureInjuryFieldsForTeam(team);
        if (state == null || team == null)
        {
            return;
        }

        LineupService.EnsureLineup(team);
        List<PlayerData> activePlayers = LineupService.GetActivePlayers(team);
        if (activePlayers.Count == 0)
        {
            activePlayers = new List<PlayerData>(team.Players);
        }

        int day = state.Season == null ? 0 : state.Season.CurrentDay;
        foreach (PlayerData player in activePlayers)
        {
            if (!IsPlayerAvailable(player))
            {
                continue;
            }

            int riskBasisPoints = CalculateInjuryRiskBasisPoints(player, team);
            int seed = StableHash(player.Id + "|" + team.Id + "|" + source + "|" + state.CurrentSeasonStartYear + "|" + day + "|" + state.TotalGamesSimulated);
            if (ShouldInjurePlayer(player, team, riskBasisPoints, seed))
            {
                CreateInjury(state, team, player, source, seed);
            }
        }
    }

    public static void AdvanceInjuryRecovery(GameState state)
    {
        EnsureInjuryHistory(state);
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
                EnsureInjuryFields(player);
                if (player == null || !player.IsInjured)
                {
                    continue;
                }

                player.InjuryDaysRemaining--;
                if (player.InjuryDaysRemaining <= 0)
                {
                    RecoverPlayer(state, team, player);
                }
            }
        }
    }

    public static void ResetInjuriesForNewSeason(GameState state)
    {
        EnsureInjuryHistory(state);
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
                EnsureInjuryFields(player);
                if (player == null || !player.IsInjured)
                {
                    continue;
                }

                if (player.InjurySeverity != "LongTerm")
                {
                    RecoverPlayer(state, team, player);
                }
            }
        }
    }

    private static int CalculateInjuryRiskBasisPoints(PlayerData player, TeamData team)
    {
        if (player == null)
        {
            return 0;
        }

        PlayerFatigueService.EnsureFatigueFields(player);
        int risk = InjuryConfig.BaseInjuryRiskPerGameBasisPoints;

        if (player.Fatigue >= 30)
        {
            risk += InjuryConfig.HighFatigueRiskBonusBasisPoints;
        }

        if (player.Fatigue >= 50)
        {
            risk += InjuryConfig.VeryHighFatigueRiskBonusBasisPoints;
        }

        if (player.Condition <= 75)
        {
            risk += InjuryConfig.LowConditionRiskBonusBasisPoints;
        }

        if (player.Age >= 32)
        {
            risk += InjuryConfig.VeteranRiskBonusBasisPoints;
        }

        if (team != null && team.Tactics != null && team.Tactics.PresetName == "Aggressive")
        {
            risk += InjuryConfig.AggressiveTacticsRiskBonusBasisPoints;
        }

        if (player.ConsecutiveGamesPlayed >= 8)
        {
            risk += InjuryConfig.ConsecutiveGamesRiskBonusBasisPoints;
        }

        if (player.Position == "G" && player.ConsecutiveGamesPlayed >= 4)
        {
            risk += 20;
        }

        return Mathf.Clamp(risk, 0, 600);
    }

    private static bool ShouldInjurePlayer(PlayerData player, TeamData team, int riskBasisPoints, int seed)
    {
        if (player == null || team == null || riskBasisPoints <= 0)
        {
            return false;
        }

        int roll = StableRange(seed.ToString(), 0, 9999);
        return roll < riskBasisPoints;
    }

    private static InjuryRecordData CreateInjury(GameState state, TeamData team, PlayerData player, string source, int seed)
    {
        if (state == null || team == null || player == null || player.IsInjured)
        {
            return null;
        }

        EnsureInjuryHistory(state);
        int days = GenerateInjuryDays(player, seed);
        string type = InjuryConfig.GetRandomInjuryType(player.Position, seed);
        string severity = InjuryConfig.GetSeverityByDays(days);
        string injuredAt = DateTime.UtcNow.ToString("o");
        string expectedReturnDate = DateTime.UtcNow.Date.AddDays(days).ToString("yyyy-MM-dd");

        string originalSlotType = "";
        int originalLineOrPairNumber = 0;
        string originalSlotPosition = "";
        if (RosterStatusConfig.IsNhlRoster(player))
        {
            LineupService.EnsureLineup(team);
            if (team.Lineup != null)
            {
                LineupService.IsPlayerInAnyActiveSlot(
                    team.Lineup,
                    player.Id,
                    out originalSlotType,
                    out originalLineOrPairNumber,
                    out originalSlotPosition);
            }
        }

        player.IsInjured = true;
        player.InjuryType = type;
        player.InjurySeverity = severity;
        player.InjuryDaysRemaining = days;
        player.CanPlayThroughInjury = false;
        player.InjuredAtUtc = injuredAt;
        player.ExpectedReturnDate = expectedReturnDate;
        player.TotalInjuries++;

        InjuryRecordData record = new InjuryRecordData
        {
            InjuryId = Guid.NewGuid().ToString("N"),
            PlayerId = player.Id,
            PlayerName = player.FirstName + " " + player.LastName,
            TeamId = team.Id,
            TeamName = TeamIdentityService.GetDisplayName(team),
            Position = player.Position,
            Age = player.Age,
            InjuryType = type,
            InjurySeverity = severity,
            InjuryDays = days,
            InjuryDaysRemainingAtCreation = days,
            InjuredAtUtc = injuredAt,
            ExpectedReturnDate = expectedReturnDate,
            Status = "Active",
            Source = string.IsNullOrEmpty(source) ? "Generated" : source,
            OriginalRosterStatus = player.RosterStatus,
            OriginalSlotType = originalSlotType,
            OriginalLineOrPairNumber = originalLineOrPairNumber,
            OriginalSlotPosition = originalSlotPosition,
            ReplacementPlayerId = "",
            ReplacementPlayerName = ""
        };

        state.InjuryHistory.Injuries.Add(record);
        EventNewsService.CreateMajorInjuryNews(state, team, player);
        Debug.Log("Травма: " + record.PlayerName + " | " + record.InjuryType + " | " + record.InjuryDays + " дн.");
        return record;
    }

    private static int GenerateInjuryDays(PlayerData player, int seed)
    {
        int roll = StableRange("injury-days-roll-" + seed, 1, 100);
        if (roll <= 65)
        {
            return StableRange("minor-" + seed, InjuryConfig.MinorInjuryMinDays, InjuryConfig.MinorInjuryMaxDays);
        }

        if (roll <= 90)
        {
            return StableRange("medium-" + seed, InjuryConfig.MediumInjuryMinDays, InjuryConfig.MediumInjuryMaxDays);
        }

        if (roll <= 98)
        {
            return StableRange("major-" + seed, InjuryConfig.MajorInjuryMinDays, InjuryConfig.MajorInjuryMaxDays);
        }

        return StableRange("long-term-" + seed, InjuryConfig.LongTermInjuryMinDays, InjuryConfig.LongTermInjuryMaxDays);
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 23;
            if (value == null)
            {
                return hash;
            }

            for (int i = 0; i < value.Length; i++)
            {
                hash = (hash * 31) + value[i];
            }

            return hash;
        }
    }

    private static int StableRange(string seed, int minInclusive, int maxInclusive)
    {
        if (maxInclusive <= minInclusive)
        {
            return minInclusive;
        }

        int hash = StableHash(seed);
        int range = maxInclusive - minInclusive + 1;
        int positiveHash = hash == int.MinValue ? 0 : Math.Abs(hash);
        return minInclusive + (positiveHash % range);
    }

    private static void RecoverPlayer(GameState state, TeamData team, PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        string playerId = player.Id;
        player.IsInjured = false;
        player.InjuryDaysRemaining = 0;
        player.InjuryType = "";
        player.InjurySeverity = "";
        player.CanPlayThroughInjury = false;
        player.InjuredAtUtc = "";
        player.ExpectedReturnDate = "";

        InjuryRecordData recoveredRecord = null;
        if (state == null || state.InjuryHistory == null || state.InjuryHistory.Injuries == null)
        {
            TryRestoreRecoveredPlayerToLineup(state, team, player, null);
            return;
        }

        foreach (InjuryRecordData record in state.InjuryHistory.Injuries)
        {
            if (record != null && record.PlayerId == playerId && record.Status == "Active")
            {
                record.Status = "Recovered";
                recoveredRecord = record;
            }
        }

        TryRestoreRecoveredPlayerToLineup(state, team, player, recoveredRecord);
    }

    private static void TryRestoreRecoveredPlayerToLineup(
        GameState state,
        TeamData team,
        PlayerData player,
        InjuryRecordData record)
    {
        if (team == null || player == null || string.IsNullOrEmpty(player.Id))
        {
            return;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        LineupService.EnsureLineup(team);

        if (record == null
            || string.IsNullOrEmpty(record.OriginalSlotType)
            || string.IsNullOrEmpty(record.OriginalSlotPosition))
        {
            if (!LineupService.ValidateLineup(team, out string _))
            {
                team.Lineup = LineupService.BuildAutoLineup(team);
                team.SpecialTeams = SpecialTeamsService.BuildAutoSpecialTeams(team);
            }

            return;
        }

        if (!EnsureRecoveredPlayerInNhlRoster(state, team, player, record))
        {
            Debug.LogWarning("Игрок выздоровел, но не возвращён в Pro roster: " + GetPlayerName(player));
            return;
        }

        if (!LineupService.TryRestorePlayerToSlot(
            team,
            player.Id,
            record.OriginalSlotType,
            record.OriginalLineOrPairNumber,
            record.OriginalSlotPosition,
            out string displacedPlayerId,
            out string message))
        {
            Debug.LogWarning("Игрок выздоровел, но не возвращён в прежний слот: " + message);
            return;
        }

        PlayerData displacedPlayer = FindPlayer(team, displacedPlayerId);
        record.ReplacementPlayerId = displacedPlayer == null ? "" : displacedPlayer.Id;
        record.ReplacementPlayerName = GetPlayerName(displacedPlayer);

        team.SpecialTeams = SpecialTeamsService.BuildAutoSpecialTeams(team);
        TacticsService.EnsureTactics(team);
        PlayerRoleService.EnsureRolesForTeam(team);
        IceTimeService.EnsureUsageForTeam(team);

        Debug.Log("Игрок восстановился и вернулся в состав: " + GetPlayerName(player));
    }

    private static bool EnsureRecoveredPlayerInNhlRoster(
        GameState state,
        TeamData team,
        PlayerData player,
        InjuryRecordData record)
    {
        if (RosterStatusConfig.IsNhlRoster(player))
        {
            return true;
        }

        if (!RosterStatusConfig.IsFarmRoster(player) && !RosterStatusConfig.IsReserve(player))
        {
            return false;
        }

        if (TeamRosterService.GetNhlPlayers(team).Count >= RosterStatusConfig.MaxNhlRosterSize
            && !TryCreateRosterRoomForRecoveredPlayer(state, team, player, record))
        {
            return false;
        }

        RosterMoveResultData result = RosterStatusConfig.IsReserve(player)
            ? TeamRosterService.MoveReservePlayerToNhl(team, player.Id)
            : TeamRosterService.CallUpPlayerToNhl(team, player.Id);
        return result != null && result.Success;
    }

    private static bool TryCreateRosterRoomForRecoveredPlayer(
        GameState state,
        TeamData team,
        PlayerData recoveredPlayer,
        InjuryRecordData record)
    {
        PlayerData originalSlotPlayer = FindPlayer(
            team,
            LineupService.GetPlayerIdInSlot(
                team,
                record.OriginalSlotType,
                record.OriginalLineOrPairNumber,
                record.OriginalSlotPosition));

        if (TryMoveTemporaryReplacementDown(state, team, originalSlotPlayer, recoveredPlayer))
        {
            return true;
        }

        List<PlayerData> scratches = LineupService.GetScratchPlayers(team);
        scratches.Sort(CompareRecoverySendDownCandidates);
        foreach (PlayerData scratch in scratches)
        {
            if (TryMoveTemporaryReplacementDown(state, team, scratch, recoveredPlayer))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryMoveTemporaryReplacementDown(
        GameState state,
        TeamData team,
        PlayerData candidate,
        PlayerData recoveredPlayer)
    {
        if (candidate == null
            || recoveredPlayer == null
            || candidate.Id == recoveredPlayer.Id
            || !RosterStatusConfig.IsNhlRoster(candidate)
            || candidate.IsRetired)
        {
            return false;
        }

        if (TeamRosterService.GetNhlPlayers(team).Count <= RosterStatusConfig.MinNhlRosterSize)
        {
            return false;
        }

        WaiverEligibilityService.EnsureWaiverEligibility(candidate);
        if (candidate.RequiresWaivers)
        {
            return false;
        }

        RosterMoveResultData result = TeamRosterService.SendPlayerToFarm(state, team, candidate.Id);
        if (result != null && result.Success && !RosterStatusConfig.IsNhlRoster(candidate))
        {
            return true;
        }

        result = TeamRosterService.MovePlayerToReserve(state, team, candidate.Id);
        return result != null && result.Success && !RosterStatusConfig.IsNhlRoster(candidate);
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

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : player.FirstName + " " + player.LastName;
    }

    private static int CompareRecoverySendDownCandidates(PlayerData left, PlayerData right)
    {
        int injuredComparison = (left != null && left.IsInjured ? 1 : 0).CompareTo(right != null && right.IsInjured ? 1 : 0);
        if (injuredComparison != 0)
        {
            return injuredComparison;
        }

        int overallComparison = (left == null ? 0 : left.Overall).CompareTo(right == null ? 0 : right.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        return string.Compare(left == null ? "" : left.Id, right == null ? "" : right.Id, StringComparison.Ordinal);
    }

    private static int CompareInjuredPlayers(PlayerData left, PlayerData right)
    {
        int daysComparison = right.InjuryDaysRemaining.CompareTo(left.InjuryDaysRemaining);
        if (daysComparison != 0)
        {
            return daysComparison;
        }

        return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }
}
