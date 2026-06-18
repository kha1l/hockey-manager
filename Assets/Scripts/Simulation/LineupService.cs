using System;
using System.Collections.Generic;
using UnityEngine;

public static class LineupService
{
    public static void EnsureLineup(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        TeamRosterService.EnsureRosterStatusesForTeam(team);
        team.EnsurePlayers();
        if (team.Lineup == null)
        {
            team.Lineup = BuildAutoLineup(team);
        }
        else
        {
            team.Lineup.EnsureCollections();
        }

        if (!ValidateLineup(team, out string message))
        {
            if (team.Lineup.IsManual || IsInjuryValidationMessage(message))
            {
                Debug.LogWarning("Lineup invalid for " + team.Name + ": " + message);
                team.Lineup.IsValid = false;
                team.Lineup.ValidationMessage = message;
                return;
            }

            team.Lineup = BuildAutoLineup(team);

            if (!ValidateLineup(team, out message))
            {
                Debug.LogWarning("Lineup invalid for " + team.Name + ": " + message);
                team.Lineup.IsValid = false;
                team.Lineup.ValidationMessage = message;
            }
        }
    }

    public static void EnsureLineupsForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureLineup(team);
        }
    }

    public static TeamLineupData BuildAutoLineup(TeamData team)
    {
        TeamLineupData lineup = new TeamLineupData
        {
            TeamId = team == null ? "" : team.Id,
            ForwardLines = new List<ForwardLineData>(),
            DefensePairs = new List<DefensePairData>(),
            Goalies = new GoalieLineupData(),
            ScratchPlayerIds = new List<string>(),
            IsValid = false,
            ValidationMessage = "",
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        if (team == null)
        {
            lineup.ValidationMessage = "Команда не найдена";
            return lineup;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        List<PlayerData> forwards = GetPlayersByCategory(team, "F");
        List<PlayerData> defensemen = GetPlayersByCategory(team, "D");
        List<PlayerData> goalies = GetPlayersByCategory(team, "G");
        SortByRating(forwards);
        SortByRating(defensemen);
        SortByRating(goalies);

        List<PlayerData> selectedForwards = TakePlayers(forwards, LineupConfig.ActiveForwardCount);
        List<PlayerData> selectedDefensemen = TakePlayers(defensemen, LineupConfig.ActiveDefenseCount);
        List<PlayerData> selectedGoalies = TakePlayers(goalies, LineupConfig.ActiveGoalieCount);
        HashSet<string> activePlayerIds = new HashSet<string>();

        for (int lineNumber = 1; lineNumber <= LineupConfig.ForwardLineCount; lineNumber++)
        {
            ForwardLineData line = new ForwardLineData
            {
                LineNumber = lineNumber
            };

            line.CenterPlayerId = PickForwardForSlot(selectedForwards, activePlayerIds, "C");
            line.LeftWingPlayerId = PickForwardForSlot(selectedForwards, activePlayerIds, "LW");
            line.RightWingPlayerId = PickForwardForSlot(selectedForwards, activePlayerIds, "RW");
            lineup.ForwardLines.Add(line);
        }

        for (int pairNumber = 1; pairNumber <= LineupConfig.DefensePairCount; pairNumber++)
        {
            DefensePairData pair = new DefensePairData
            {
                PairNumber = pairNumber,
                LeftDefensePlayerId = PickNextPlayer(selectedDefensemen, activePlayerIds),
                RightDefensePlayerId = PickNextPlayer(selectedDefensemen, activePlayerIds)
            };
            lineup.DefensePairs.Add(pair);
        }

        lineup.Goalies.StarterGoaliePlayerId = selectedGoalies.Count > 0 ? selectedGoalies[0].Id : "";
        if (!string.IsNullOrEmpty(lineup.Goalies.StarterGoaliePlayerId))
        {
            activePlayerIds.Add(lineup.Goalies.StarterGoaliePlayerId);
        }

        lineup.Goalies.BackupGoaliePlayerId = selectedGoalies.Count > 1 ? selectedGoalies[1].Id : "";
        if (!string.IsNullOrEmpty(lineup.Goalies.BackupGoaliePlayerId))
        {
            activePlayerIds.Add(lineup.Goalies.BackupGoaliePlayerId);
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null
                && RosterStatusConfig.IsNhlRoster(player)
                && !player.IsRetired
                && !string.IsNullOrEmpty(player.Id)
                && !activePlayerIds.Contains(player.Id))
            {
                lineup.ScratchPlayerIds.Add(player.Id);
            }
        }

        if (!HasMinimumPositionCounts(selectedForwards, selectedDefensemen, selectedGoalies))
        {
            lineup.IsValid = false;
            lineup.ValidationMessage = "Недостаточно игроков для полного состава";
        }
        else
        {
            TeamLineupData previousLineup = team.Lineup;
            team.Lineup = lineup;
            bool isValid = ValidateLineup(team, out string message);
            lineup.IsValid = isValid;
            lineup.ValidationMessage = message;
            team.Lineup = previousLineup;
        }

        lineup.Touch();
        return lineup;
    }

    public static bool ValidateLineup(TeamData team, out string message)
    {
        if (team == null)
        {
            message = "Команда не найдена";
            return false;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        TeamLineupData lineup = team.Lineup;
        if (lineup == null)
        {
            message = "Состав на матч не создан";
            return false;
        }

        lineup.EnsureCollections();
        if (lineup.ForwardLines.Count != LineupConfig.ForwardLineCount)
        {
            message = "Нужно 4 звена нападения";
            SetValidation(lineup, false, message);
            return false;
        }

        if (lineup.DefensePairs.Count != LineupConfig.DefensePairCount)
        {
            message = "Нужно 3 пары защитников";
            SetValidation(lineup, false, message);
            return false;
        }

        if (lineup.Goalies == null)
        {
            message = "Не выбраны вратари";
            SetValidation(lineup, false, message);
            return false;
        }

        HashSet<string> usedIds = new HashSet<string>();
        int forwardCount = 0;
        int defenseCount = 0;
        int goalieCount = 0;

        foreach (ForwardLineData line in lineup.ForwardLines)
        {
            int lineNumber = line == null ? 0 : line.LineNumber;
            if (!ValidateSlot(team, line == null ? "" : line.LeftWingPlayerId, usedIds, "Forward", lineNumber, "LW", out message)
                || !ValidateSlot(team, line == null ? "" : line.CenterPlayerId, usedIds, "Forward", lineNumber, "C", out message)
                || !ValidateSlot(team, line == null ? "" : line.RightWingPlayerId, usedIds, "Forward", lineNumber, "RW", out message))
            {
                SetValidation(lineup, false, message);
                return false;
            }

            forwardCount += 3;
        }

        foreach (DefensePairData pair in lineup.DefensePairs)
        {
            int pairNumber = pair == null ? 0 : pair.PairNumber;
            if (!ValidateSlot(team, pair == null ? "" : pair.LeftDefensePlayerId, usedIds, "Defense", pairNumber, "LD", out message)
                || !ValidateSlot(team, pair == null ? "" : pair.RightDefensePlayerId, usedIds, "Defense", pairNumber, "RD", out message))
            {
                SetValidation(lineup, false, message);
                return false;
            }

            defenseCount += 2;
        }

        if (!ValidateSlot(team, lineup.Goalies.StarterGoaliePlayerId, usedIds, "Goalie", 1, "Starter", out message)
            || !ValidateSlot(team, lineup.Goalies.BackupGoaliePlayerId, usedIds, "Goalie", 1, "Backup", out message))
        {
            SetValidation(lineup, false, message);
            return false;
        }

        goalieCount = 2;
        if (forwardCount != LineupConfig.ActiveForwardCount
            || defenseCount != LineupConfig.ActiveDefenseCount
            || goalieCount != LineupConfig.ActiveGoalieCount
            || usedIds.Count != LineupConfig.ActivePlayerCount)
        {
            message = "Активный состав должен содержать 12 нападающих, 6 защитников и 2 вратарей";
            SetValidation(lineup, false, message);
            return false;
        }

        foreach (string scratchPlayerId in lineup.ScratchPlayerIds)
        {
            if (usedIds.Contains(scratchPlayerId))
            {
                message = "Запасные не должны содержать активного игрока: " + scratchPlayerId;
                SetValidation(lineup, false, message);
                return false;
            }
        }

        message = "OK";
        SetValidation(lineup, true, message);
        return true;
    }

    public static List<LineupSlotData> GetLineupSlots(TeamData team)
    {
        List<LineupSlotData> slots = new List<LineupSlotData>();
        EnsureLineup(team);
        if (team == null || team.Lineup == null)
        {
            return slots;
        }

        team.Lineup.EnsureCollections();
        foreach (ForwardLineData line in team.Lineup.ForwardLines)
        {
            int lineNumber = line == null ? 0 : line.LineNumber;
            slots.Add(CreateSlot(team, "Forward", lineNumber, "LW", line == null ? "" : line.LeftWingPlayerId));
            slots.Add(CreateSlot(team, "Forward", lineNumber, "C", line == null ? "" : line.CenterPlayerId));
            slots.Add(CreateSlot(team, "Forward", lineNumber, "RW", line == null ? "" : line.RightWingPlayerId));
        }

        foreach (DefensePairData pair in team.Lineup.DefensePairs)
        {
            int pairNumber = pair == null ? 0 : pair.PairNumber;
            slots.Add(CreateSlot(team, "Defense", pairNumber, "LD", pair == null ? "" : pair.LeftDefensePlayerId));
            slots.Add(CreateSlot(team, "Defense", pairNumber, "RD", pair == null ? "" : pair.RightDefensePlayerId));
        }

        GoalieLineupData goalies = team.Lineup.Goalies;
        slots.Add(CreateSlot(team, "Goalie", 1, "Starter", goalies == null ? "" : goalies.StarterGoaliePlayerId));
        slots.Add(CreateSlot(team, "Goalie", 1, "Backup", goalies == null ? "" : goalies.BackupGoaliePlayerId));
        return slots;
    }

    public static LineupSlotData GetSlot(TeamData team, string slotType, int lineOrPairNumber, string slotPosition)
    {
        EnsureLineup(team);
        string playerId = team == null || team.Lineup == null
            ? ""
            : GetPlayerIdFromSlot(team.Lineup, slotType, lineOrPairNumber, slotPosition);
        return CreateSlot(team, slotType, lineOrPairNumber, slotPosition, playerId);
    }

    public static List<PlayerData> GetEligiblePlayersForSlot(TeamData team, string slotType, string slotPosition)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (team == null || string.IsNullOrEmpty(slotType))
        {
            return players;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (RosterStatusConfig.IsNhlRoster(player)
                && !player.IsRetired
                && !player.IsOnWaivers
                && IsPlayerEligibleForSlot(player, slotType, slotPosition))
            {
                players.Add(player);
            }
        }

        players.Sort(CompareEligiblePlayers);
        return players;
    }

    public static bool TryAssignPlayerToSlot(
        TeamData team,
        string slotType,
        int lineOrPairNumber,
        string slotPosition,
        string playerId,
        out string message)
    {
        if (team == null || team.Lineup == null)
        {
            message = "Состав не найден";
            return false;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        team.Lineup.EnsureCollections();
        PlayerData player = FindPlayer(team, playerId);
        if (player == null)
        {
            message = "Игрок не найден";
            return false;
        }

        if (!RosterStatusConfig.IsNhlRoster(player) || player.IsRetired)
        {
            message = "В линии можно поставить только игрока Pro roster";
            return false;
        }

        if (player.IsRetired)
        {
            message = "Игрок завершил карьеру";
            return false;
        }

        InjuryService.EnsureInjuryFields(player);
        if (player.IsInjured)
        {
            message = "Нельзя поставить травмированного игрока";
            return false;
        }

        if (!IsPlayerEligibleForSlot(player, slotType, slotPosition))
        {
            message = "Игрок не подходит для выбранного слота";
            return false;
        }

        if (!SlotExists(team.Lineup, slotType, lineOrPairNumber, slotPosition))
        {
            message = "Слот не найден";
            return false;
        }

        string oldPlayerId = GetPlayerIdFromSlot(team.Lineup, slotType, lineOrPairNumber, slotPosition);
        if (oldPlayerId == playerId)
        {
            MarkManualUpdate(team.Lineup, slotType, lineOrPairNumber, slotPosition);
            ValidateLineup(team, out string validationMessage);
            message = "Игрок уже находится в выбранном слоте";
            return true;
        }

        bool playerWasActive = IsPlayerInAnyActiveSlot(
            team.Lineup,
            playerId,
            out string existingSlotType,
            out int existingLineOrPairNumber,
            out string existingSlotPosition);

        SetPlayerIdToSlot(team.Lineup, slotType, lineOrPairNumber, slotPosition, playerId);
        if (playerWasActive)
        {
            SetPlayerIdToSlot(team.Lineup, existingSlotType, existingLineOrPairNumber, existingSlotPosition, oldPlayerId);
        }

        NormalizeScratchPlayerIds(team);
        if (HasDuplicateActivePlayers(team.Lineup))
        {
            SetPlayerIdToSlot(team.Lineup, slotType, lineOrPairNumber, slotPosition, oldPlayerId);
            if (playerWasActive)
            {
                SetPlayerIdToSlot(team.Lineup, existingSlotType, existingLineOrPairNumber, existingSlotPosition, playerId);
            }

            NormalizeScratchPlayerIds(team);
            ValidateLineup(team, out string duplicateMessage);
            message = "Ошибка состава: игрок назначен в несколько активных слотов";
            return false;
        }

        MarkManualUpdate(team.Lineup, slotType, lineOrPairNumber, slotPosition);
        ValidateLineup(team, out string validationMessageAfterAssign);
        message = "Игрок назначен";
        return true;
    }

    public static string GetPlayerIdInSlot(
        TeamData team,
        string slotType,
        int lineOrPairNumber,
        string slotPosition)
    {
        if (team == null || team.Lineup == null)
        {
            return "";
        }

        return GetPlayerIdFromSlot(team.Lineup, slotType, lineOrPairNumber, slotPosition);
    }

    public static bool TryRestorePlayerToSlot(
        TeamData team,
        string playerId,
        string slotType,
        int lineOrPairNumber,
        string slotPosition,
        out string displacedPlayerId,
        out string message)
    {
        displacedPlayerId = "";
        if (team == null || team.Lineup == null)
        {
            message = "Состав не найден";
            return false;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        team.Lineup.EnsureCollections();

        if (!SlotExists(team.Lineup, slotType, lineOrPairNumber, slotPosition))
        {
            message = "Слот не найден";
            return false;
        }

        PlayerData player = FindPlayer(team, playerId);
        if (!IsPlayerEligibleForSlot(player, slotType, slotPosition))
        {
            message = "Игрок не подходит для возвращения в слот";
            return false;
        }

        string oldPlayerId = GetPlayerIdFromSlot(team.Lineup, slotType, lineOrPairNumber, slotPosition);
        if (oldPlayerId == playerId)
        {
            NormalizeScratchPlayerIds(team);
            ValidateLineup(team, out message);
            message = "Игрок уже находится в своём слоте";
            return true;
        }

        bool playerWasActive = IsPlayerInAnyActiveSlot(
            team.Lineup,
            playerId,
            out string existingSlotType,
            out int existingLineOrPairNumber,
            out string existingSlotPosition);

        string existingSlotOldPlayerId = playerWasActive
            ? GetPlayerIdFromSlot(team.Lineup, existingSlotType, existingLineOrPairNumber, existingSlotPosition)
            : "";

        SetPlayerIdToSlot(team.Lineup, slotType, lineOrPairNumber, slotPosition, playerId);
        if (playerWasActive)
        {
            SetPlayerIdToSlot(team.Lineup, existingSlotType, existingLineOrPairNumber, existingSlotPosition, oldPlayerId);
        }

        NormalizeScratchPlayerIds(team);
        string validationMessage = "";
        bool hasDuplicatePlayers = HasDuplicateActivePlayers(team.Lineup);
        bool isLineupValid = !hasDuplicatePlayers && ValidateLineup(team, out validationMessage);
        if (hasDuplicatePlayers || !isLineupValid)
        {
            SetPlayerIdToSlot(team.Lineup, slotType, lineOrPairNumber, slotPosition, oldPlayerId);
            if (playerWasActive)
            {
                SetPlayerIdToSlot(team.Lineup, existingSlotType, existingLineOrPairNumber, existingSlotPosition, existingSlotOldPlayerId);
            }

            NormalizeScratchPlayerIds(team);
            ValidateLineup(team, out string _);
            if (hasDuplicatePlayers)
            {
                validationMessage = "игрок назначен в несколько активных слотов";
            }

            message = "Не удалось вернуть игрока в слот: " + validationMessage;
            return false;
        }

        displacedPlayerId = oldPlayerId;
        team.Lineup.Touch();
        message = "Игрок возвращён в прежний слот";
        return true;
    }

    public static bool TrySwapGoalies(TeamData team, out string message)
    {
        if (team == null || team.Lineup == null || team.Lineup.Goalies == null)
        {
            message = "Вратари не найдены";
            return false;
        }

        PlayerData starter = FindPlayer(team, team.Lineup.Goalies.StarterGoaliePlayerId);
        PlayerData backup = FindPlayer(team, team.Lineup.Goalies.BackupGoaliePlayerId);
        if (!IsPlayerEligibleForSlot(starter, "Goalie", "Starter") || !IsPlayerEligibleForSlot(backup, "Goalie", "Backup"))
        {
            message = "Нельзя поменять травмированного или неподходящего вратаря";
            return false;
        }

        string starterId = team.Lineup.Goalies.StarterGoaliePlayerId;
        team.Lineup.Goalies.StarterGoaliePlayerId = team.Lineup.Goalies.BackupGoaliePlayerId;
        team.Lineup.Goalies.BackupGoaliePlayerId = starterId;
        MarkManualUpdate(team.Lineup, "Goalie", 1, "Starter");
        ValidateLineup(team, out string validationMessage);
        message = "Вратари поменяны местами";
        return true;
    }

    public static void SyncScratchPlayers(TeamData team)
    {
        NormalizeScratchPlayerIds(team);
    }

    public static bool IsPlayerEligibleForSlot(PlayerData player, string slotType, string slotPosition)
    {
        if (player == null)
        {
            return false;
        }

        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            return false;
        }

        InjuryService.EnsureInjuryFields(player);
        if (player.IsInjured)
        {
            return false;
        }

        if (slotType == "Forward")
        {
            return IsForward(player);
        }

        if (slotType == "Defense")
        {
            return player.Position == "D";
        }

        if (slotType == "Goalie")
        {
            return player.Position == "G";
        }

        return false;
    }

    public static bool IsPlayerInAnyActiveSlot(
        TeamLineupData lineup,
        string playerId,
        out string existingSlotType,
        out int existingLineOrPairNumber,
        out string existingSlotPosition)
    {
        existingSlotType = "";
        existingLineOrPairNumber = 0;
        existingSlotPosition = "";
        if (lineup == null || string.IsNullOrEmpty(playerId))
        {
            return false;
        }

        lineup.EnsureCollections();
        foreach (ForwardLineData line in lineup.ForwardLines)
        {
            if (line == null)
            {
                continue;
            }

            if (line.LeftWingPlayerId == playerId)
            {
                SetSlotInfo("Forward", line.LineNumber, "LW", out existingSlotType, out existingLineOrPairNumber, out existingSlotPosition);
                return true;
            }

            if (line.CenterPlayerId == playerId)
            {
                SetSlotInfo("Forward", line.LineNumber, "C", out existingSlotType, out existingLineOrPairNumber, out existingSlotPosition);
                return true;
            }

            if (line.RightWingPlayerId == playerId)
            {
                SetSlotInfo("Forward", line.LineNumber, "RW", out existingSlotType, out existingLineOrPairNumber, out existingSlotPosition);
                return true;
            }
        }

        foreach (DefensePairData pair in lineup.DefensePairs)
        {
            if (pair == null)
            {
                continue;
            }

            if (pair.LeftDefensePlayerId == playerId)
            {
                SetSlotInfo("Defense", pair.PairNumber, "LD", out existingSlotType, out existingLineOrPairNumber, out existingSlotPosition);
                return true;
            }

            if (pair.RightDefensePlayerId == playerId)
            {
                SetSlotInfo("Defense", pair.PairNumber, "RD", out existingSlotType, out existingLineOrPairNumber, out existingSlotPosition);
                return true;
            }
        }

        if (lineup.Goalies != null && lineup.Goalies.StarterGoaliePlayerId == playerId)
        {
            SetSlotInfo("Goalie", 1, "Starter", out existingSlotType, out existingLineOrPairNumber, out existingSlotPosition);
            return true;
        }

        if (lineup.Goalies != null && lineup.Goalies.BackupGoaliePlayerId == playerId)
        {
            SetSlotInfo("Goalie", 1, "Backup", out existingSlotType, out existingLineOrPairNumber, out existingSlotPosition);
            return true;
        }

        return false;
    }

    public static List<PlayerData> GetActivePlayers(TeamData team)
    {
        List<PlayerData> players = GetActiveSkaters(team);
        PlayerData starter = GetStartingGoalie(team);
        PlayerData backup = GetBackupGoalie(team);

        if (starter != null)
        {
            players.Add(starter);
        }

        if (backup != null)
        {
            players.Add(backup);
        }

        return players;
    }

    public static List<PlayerData> GetActiveSkaters(TeamData team)
    {
        List<PlayerData> skaters = GetActiveForwards(team);
        skaters.AddRange(GetActiveDefensemen(team));
        return skaters;
    }

    public static List<PlayerData> GetActiveForwards(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        EnsureLineup(team);

        if (team == null || team.Lineup == null)
        {
            return players;
        }

        foreach (ForwardLineData line in team.Lineup.ForwardLines)
        {
            AddPlayerById(team, line == null ? "" : line.LeftWingPlayerId, players);
            AddPlayerById(team, line == null ? "" : line.CenterPlayerId, players);
            AddPlayerById(team, line == null ? "" : line.RightWingPlayerId, players);
        }

        return players;
    }

    public static List<PlayerData> GetActiveDefensemen(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        EnsureLineup(team);

        if (team == null || team.Lineup == null)
        {
            return players;
        }

        foreach (DefensePairData pair in team.Lineup.DefensePairs)
        {
            AddPlayerById(team, pair == null ? "" : pair.LeftDefensePlayerId, players);
            AddPlayerById(team, pair == null ? "" : pair.RightDefensePlayerId, players);
        }

        return players;
    }

    public static PlayerData GetStartingGoalie(TeamData team)
    {
        EnsureLineup(team);
        PlayerData player = team == null || team.Lineup == null || team.Lineup.Goalies == null
            ? null
            : FindPlayer(team, team.Lineup.Goalies.StarterGoaliePlayerId);
        return RosterStatusConfig.IsNhlRoster(player) && !player.IsRetired ? player : null;
    }

    public static PlayerData GetBackupGoalie(TeamData team)
    {
        EnsureLineup(team);
        PlayerData player = team == null || team.Lineup == null || team.Lineup.Goalies == null
            ? null
            : FindPlayer(team, team.Lineup.Goalies.BackupGoaliePlayerId);
        return RosterStatusConfig.IsNhlRoster(player) && !player.IsRetired ? player : null;
    }

    public static List<PlayerData> GetScratchPlayers(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        EnsureLineup(team);

        if (team == null || team.Lineup == null)
        {
            return players;
        }

        team.Lineup.EnsureCollections();
        foreach (string playerId in team.Lineup.ScratchPlayerIds)
        {
            AddPlayerById(team, playerId, players);
        }

        return players;
    }

    public static bool IsPlayerActive(TeamData team, string playerId)
    {
        if (team == null || string.IsNullOrEmpty(playerId))
        {
            return false;
        }

        EnsureLineup(team);
        PlayerData player = FindPlayer(team, playerId);
        return RosterStatusConfig.IsNhlRoster(player) && !player.IsRetired && team.Lineup != null && IsPlayerInLineup(team.Lineup, playerId);
    }

    public static bool IsPlayerInLineup(TeamLineupData lineup, string playerId)
    {
        if (lineup == null || string.IsNullOrEmpty(playerId))
        {
            return false;
        }

        lineup.EnsureCollections();
        foreach (ForwardLineData line in lineup.ForwardLines)
        {
            if (line != null
                && (line.LeftWingPlayerId == playerId || line.CenterPlayerId == playerId || line.RightWingPlayerId == playerId))
            {
                return true;
            }
        }

        foreach (DefensePairData pair in lineup.DefensePairs)
        {
            if (pair != null
                && (pair.LeftDefensePlayerId == playerId || pair.RightDefensePlayerId == playerId))
            {
                return true;
            }
        }

        return lineup.Goalies != null
            && (lineup.Goalies.StarterGoaliePlayerId == playerId || lineup.Goalies.BackupGoaliePlayerId == playerId);
    }

    public static bool HasInjuredActivePlayers(TeamData team, out string message)
    {
        message = "";
        if (team == null || team.Lineup == null)
        {
            return false;
        }

        team.EnsurePlayers();
        team.Lineup.EnsureCollections();
        List<string> activePlayerIds = new List<string>();
        foreach (ForwardLineData line in team.Lineup.ForwardLines)
        {
            activePlayerIds.Add(line == null ? "" : line.LeftWingPlayerId);
            activePlayerIds.Add(line == null ? "" : line.CenterPlayerId);
            activePlayerIds.Add(line == null ? "" : line.RightWingPlayerId);
        }

        foreach (DefensePairData pair in team.Lineup.DefensePairs)
        {
            activePlayerIds.Add(pair == null ? "" : pair.LeftDefensePlayerId);
            activePlayerIds.Add(pair == null ? "" : pair.RightDefensePlayerId);
        }

        if (team.Lineup.Goalies != null)
        {
            activePlayerIds.Add(team.Lineup.Goalies.StarterGoaliePlayerId);
            activePlayerIds.Add(team.Lineup.Goalies.BackupGoaliePlayerId);
        }

        foreach (string playerId in activePlayerIds)
        {
            PlayerData player = FindPlayer(team, playerId);
            InjuryService.EnsureInjuryFields(player);
            if (player != null && player.IsInjured)
            {
                message = "В активном составе травмирован игрок: "
                    + player.FirstName + " " + player.LastName
                    + " (" + player.InjurySeverity + ", " + player.InjuryDaysRemaining + " дн.)";
                return true;
            }
        }

        return false;
    }

    public static bool HasNonNhlActivePlayers(TeamData team, out string message)
    {
        message = "";
        if (team == null || team.Lineup == null)
        {
            return false;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        foreach (string playerId in GetActivePlayerIds(team.Lineup))
        {
            PlayerData player = FindPlayer(team, playerId);
            if (player != null && (player.IsRetired || !RosterStatusConfig.IsNhlRoster(player)))
            {
                message = "В активных линиях игрок не из Pro roster: "
                    + player.FirstName + " " + player.LastName
                    + " (" + player.RosterStatus + ")";
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

    private static List<PlayerData> GetPlayersByCategory(TeamData team, string category)
    {
        List<PlayerData> players = new List<PlayerData>();
        List<PlayerData> availablePlayers = new List<PlayerData>();
        if (team == null || team.Players == null)
        {
            return players;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            if (!RosterStatusConfig.IsNhlRoster(player) || player.IsRetired || player.IsOnWaivers)
            {
                continue;
            }

            if ((category == "F" && IsForward(player))
                || (category == "D" && player.Position == "D")
                || (category == "G" && player.Position == "G"))
            {
                players.Add(player);
                if (InjuryService.IsPlayerAvailable(player))
                {
                    availablePlayers.Add(player);
                }
            }
        }

        if (availablePlayers.Count >= GetRequiredCountForCategory(category))
        {
            return availablePlayers;
        }

        return players;
    }

    private static bool IsForward(PlayerData player)
    {
        return player != null && (player.Position == "C" || player.Position == "LW" || player.Position == "RW");
    }

    private static void SortByRating(List<PlayerData> players)
    {
        players.Sort(ComparePlayersByRating);
    }

    private static int ComparePlayersByRating(PlayerData left, PlayerData right)
    {
        int overallComparison = right.Overall.CompareTo(left.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        int potentialComparison = right.Potential.CompareTo(left.Potential);
        if (potentialComparison != 0)
        {
            return potentialComparison;
        }

        return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }

    private static List<PlayerData> TakePlayers(List<PlayerData> players, int count)
    {
        List<PlayerData> selected = new List<PlayerData>();
        if (players == null)
        {
            return selected;
        }

        for (int i = 0; i < players.Count && selected.Count < count; i++)
        {
            selected.Add(players[i]);
        }

        return selected;
    }

    private static string PickForwardForSlot(List<PlayerData> forwards, HashSet<string> usedIds, string preferredPosition)
    {
        PlayerData preferred = null;
        foreach (PlayerData player in forwards)
        {
            if (player != null && player.Position == preferredPosition && !usedIds.Contains(player.Id))
            {
                preferred = player;
                break;
            }
        }

        if (preferred == null)
        {
            foreach (PlayerData player in forwards)
            {
                if (player != null && !usedIds.Contains(player.Id))
                {
                    preferred = player;
                    break;
                }
            }
        }

        if (preferred == null)
        {
            return "";
        }

        usedIds.Add(preferred.Id);
        return preferred.Id;
    }

    private static string PickNextPlayer(List<PlayerData> players, HashSet<string> usedIds)
    {
        foreach (PlayerData player in players)
        {
            if (player != null && !usedIds.Contains(player.Id))
            {
                usedIds.Add(player.Id);
                return player.Id;
            }
        }

        return "";
    }

    private static bool HasMinimumPositionCounts(List<PlayerData> forwards, List<PlayerData> defensemen, List<PlayerData> goalies)
    {
        return forwards.Count >= LineupConfig.ActiveForwardCount
            && defensemen.Count >= LineupConfig.ActiveDefenseCount
            && goalies.Count >= LineupConfig.ActiveGoalieCount;
    }

    private static bool ValidateSlot(
        TeamData team,
        string playerId,
        HashSet<string> usedIds,
        string slotType,
        int lineOrPairNumber,
        string slotPosition,
        out string message)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            message = "Пустой слот: " + FormatSlot(slotType, lineOrPairNumber, slotPosition);
            return false;
        }

        PlayerData player = FindPlayer(team, playerId);
        if (player == null)
        {
            message = "Игрок из слота " + FormatSlot(slotType, lineOrPairNumber, slotPosition) + " не найден в roster";
            return false;
        }

        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            message = "Игрок из слота " + FormatSlot(slotType, lineOrPairNumber, slotPosition)
                + " не находится в Pro roster: " + player.FirstName + " " + player.LastName
                + " (" + player.RosterStatus + ")";
            return false;
        }

        if (player.IsRetired)
        {
            message = "Игрок завершил карьеру в слоте " + FormatSlot(slotType, lineOrPairNumber, slotPosition)
                + ": " + player.FirstName + " " + player.LastName;
            return false;
        }

        InjuryService.EnsureInjuryFields(player);
        if (player != null && player.IsInjured)
        {
            message = "Игрок травмирован в слоте " + FormatSlot(slotType, lineOrPairNumber, slotPosition) + ": "
                + player.FirstName + " " + player.LastName
                + " (" + player.InjurySeverity + ", " + player.InjuryDaysRemaining + " дн.)";
            return false;
        }

        if (!IsPlayerEligibleForSlot(player, slotType, slotPosition))
        {
            message = "Игрок не подходит для слота " + FormatSlot(slotType, lineOrPairNumber, slotPosition)
                + ": " + player.FirstName + " " + player.LastName + " (" + player.Position + ")";
            return false;
        }

        if (usedIds.Contains(playerId))
        {
            message = "Один игрок используется в составе больше одного раза: " + player.FirstName + " " + player.LastName;
            return false;
        }

        usedIds.Add(playerId);
        message = "";
        return true;
    }

    private static LineupSlotData CreateSlot(TeamData team, string slotType, int lineOrPairNumber, string slotPosition, string playerId)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player != null)
        {
            PlayerFatigueService.EnsureFatigueFields(player);
            InjuryService.EnsureInjuryFields(player);
            PlayerRoleService.EnsureRole(player);
            MoraleService.InitializePlayerMorale(player);
        }

        return new LineupSlotData
        {
            SlotType = slotType,
            LineOrPairNumber = lineOrPairNumber,
            SlotPosition = slotPosition,
            PlayerId = player == null ? "" : player.Id,
            PlayerName = player == null ? "Пусто" : player.FirstName + " " + player.LastName,
            Position = player == null ? "" : player.Position,
            Overall = player == null ? 0 : player.Overall,
            EffectiveOverall = player == null ? 0 : PlayerFatigueService.GetEffectiveOverall(player),
            Condition = player == null ? 0 : player.Condition,
            Fatigue = player == null ? 0 : player.Fatigue,
            IsInjured = player != null && player.IsInjured,
            InjuryLabel = player == null || !player.IsInjured
                ? ""
                : "INJ " + player.InjuryDaysRemaining + " дн.",
            PlayerRole = player == null ? "" : player.PlayerRole,
            UsageCategory = player == null ? "" : player.UsageCategory,
            EstimatedTimeOnIceSeconds = player == null ? 0 : player.EstimatedTimeOnIceSeconds,
            Morale = player == null ? 0 : player.Morale,
            MoraleStatus = player == null ? "" : player.MoraleStatus
        };
    }

    private static string GetPlayerIdFromSlot(
        TeamLineupData lineup,
        string slotType,
        int lineOrPairNumber,
        string slotPosition)
    {
        if (lineup == null)
        {
            return "";
        }

        if (slotType == "Forward")
        {
            ForwardLineData line = FindForwardLine(lineup, lineOrPairNumber);
            if (line == null)
            {
                return "";
            }

            if (slotPosition == "LW")
            {
                return line.LeftWingPlayerId;
            }

            if (slotPosition == "C")
            {
                return line.CenterPlayerId;
            }

            if (slotPosition == "RW")
            {
                return line.RightWingPlayerId;
            }
        }

        if (slotType == "Defense")
        {
            DefensePairData pair = FindDefensePair(lineup, lineOrPairNumber);
            if (pair == null)
            {
                return "";
            }

            if (slotPosition == "LD")
            {
                return pair.LeftDefensePlayerId;
            }

            if (slotPosition == "RD")
            {
                return pair.RightDefensePlayerId;
            }
        }

        if (slotType == "Goalie" && lineup.Goalies != null)
        {
            if (slotPosition == "Starter")
            {
                return lineup.Goalies.StarterGoaliePlayerId;
            }

            if (slotPosition == "Backup")
            {
                return lineup.Goalies.BackupGoaliePlayerId;
            }
        }

        return "";
    }

    private static void SetPlayerIdToSlot(
        TeamLineupData lineup,
        string slotType,
        int lineOrPairNumber,
        string slotPosition,
        string playerId)
    {
        if (lineup == null)
        {
            return;
        }

        if (slotType == "Forward")
        {
            ForwardLineData line = FindForwardLine(lineup, lineOrPairNumber);
            if (line == null)
            {
                return;
            }

            if (slotPosition == "LW")
            {
                line.LeftWingPlayerId = playerId;
            }
            else if (slotPosition == "C")
            {
                line.CenterPlayerId = playerId;
            }
            else if (slotPosition == "RW")
            {
                line.RightWingPlayerId = playerId;
            }
        }
        else if (slotType == "Defense")
        {
            DefensePairData pair = FindDefensePair(lineup, lineOrPairNumber);
            if (pair == null)
            {
                return;
            }

            if (slotPosition == "LD")
            {
                pair.LeftDefensePlayerId = playerId;
            }
            else if (slotPosition == "RD")
            {
                pair.RightDefensePlayerId = playerId;
            }
        }
        else if (slotType == "Goalie" && lineup.Goalies != null)
        {
            if (slotPosition == "Starter")
            {
                lineup.Goalies.StarterGoaliePlayerId = playerId;
            }
            else if (slotPosition == "Backup")
            {
                lineup.Goalies.BackupGoaliePlayerId = playerId;
            }
        }
    }

    private static bool SlotExists(TeamLineupData lineup, string slotType, int lineOrPairNumber, string slotPosition)
    {
        if (lineup == null)
        {
            return false;
        }

        if (slotType == "Forward")
        {
            return FindForwardLine(lineup, lineOrPairNumber) != null
                && (slotPosition == "LW" || slotPosition == "C" || slotPosition == "RW");
        }

        if (slotType == "Defense")
        {
            return FindDefensePair(lineup, lineOrPairNumber) != null
                && (slotPosition == "LD" || slotPosition == "RD");
        }

        return slotType == "Goalie" && lineup.Goalies != null
            && (slotPosition == "Starter" || slotPosition == "Backup");
    }

    private static ForwardLineData FindForwardLine(TeamLineupData lineup, int lineNumber)
    {
        if (lineup == null)
        {
            return null;
        }

        lineup.EnsureCollections();
        foreach (ForwardLineData line in lineup.ForwardLines)
        {
            if (line != null && line.LineNumber == lineNumber)
            {
                return line;
            }
        }

        return null;
    }

    private static DefensePairData FindDefensePair(TeamLineupData lineup, int pairNumber)
    {
        if (lineup == null)
        {
            return null;
        }

        lineup.EnsureCollections();
        foreach (DefensePairData pair in lineup.DefensePairs)
        {
            if (pair != null && pair.PairNumber == pairNumber)
            {
                return pair;
            }
        }

        return null;
    }

    private static void NormalizeScratchPlayerIds(TeamData team)
    {
        if (team == null || team.Lineup == null)
        {
            return;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        team.Lineup.EnsureCollections();
        HashSet<string> activeIds = GetActivePlayerIds(team.Lineup);
        HashSet<string> scratchIds = new HashSet<string>();
        List<string> normalizedScratchIds = new List<string>();

        foreach (PlayerData player in team.Players)
        {
            if (player == null
                || !RosterStatusConfig.IsNhlRoster(player)
                || player.IsRetired
                || string.IsNullOrEmpty(player.Id)
                || activeIds.Contains(player.Id)
                || scratchIds.Contains(player.Id))
            {
                continue;
            }

            scratchIds.Add(player.Id);
            normalizedScratchIds.Add(player.Id);
        }

        team.Lineup.ScratchPlayerIds = normalizedScratchIds;
    }

    private static HashSet<string> GetActivePlayerIds(TeamLineupData lineup)
    {
        HashSet<string> activeIds = new HashSet<string>();
        if (lineup == null)
        {
            return activeIds;
        }

        lineup.EnsureCollections();
        foreach (ForwardLineData line in lineup.ForwardLines)
        {
            AddId(activeIds, line == null ? "" : line.LeftWingPlayerId);
            AddId(activeIds, line == null ? "" : line.CenterPlayerId);
            AddId(activeIds, line == null ? "" : line.RightWingPlayerId);
        }

        foreach (DefensePairData pair in lineup.DefensePairs)
        {
            AddId(activeIds, pair == null ? "" : pair.LeftDefensePlayerId);
            AddId(activeIds, pair == null ? "" : pair.RightDefensePlayerId);
        }

        if (lineup.Goalies != null)
        {
            AddId(activeIds, lineup.Goalies.StarterGoaliePlayerId);
            AddId(activeIds, lineup.Goalies.BackupGoaliePlayerId);
        }

        return activeIds;
    }

    private static bool HasDuplicateActivePlayers(TeamLineupData lineup)
    {
        HashSet<string> usedIds = new HashSet<string>();
        foreach (LineupSlotIdentity slot in GetSlotIdentities(lineup))
        {
            string playerId = GetPlayerIdFromSlot(lineup, slot.SlotType, slot.LineOrPairNumber, slot.SlotPosition);
            if (string.IsNullOrEmpty(playerId))
            {
                continue;
            }

            if (usedIds.Contains(playerId))
            {
                return true;
            }

            usedIds.Add(playerId);
        }

        return false;
    }

    private static List<LineupSlotIdentity> GetSlotIdentities(TeamLineupData lineup)
    {
        List<LineupSlotIdentity> slots = new List<LineupSlotIdentity>();
        if (lineup == null)
        {
            return slots;
        }

        lineup.EnsureCollections();
        foreach (ForwardLineData line in lineup.ForwardLines)
        {
            int lineNumber = line == null ? 0 : line.LineNumber;
            slots.Add(new LineupSlotIdentity("Forward", lineNumber, "LW"));
            slots.Add(new LineupSlotIdentity("Forward", lineNumber, "C"));
            slots.Add(new LineupSlotIdentity("Forward", lineNumber, "RW"));
        }

        foreach (DefensePairData pair in lineup.DefensePairs)
        {
            int pairNumber = pair == null ? 0 : pair.PairNumber;
            slots.Add(new LineupSlotIdentity("Defense", pairNumber, "LD"));
            slots.Add(new LineupSlotIdentity("Defense", pairNumber, "RD"));
        }

        slots.Add(new LineupSlotIdentity("Goalie", 1, "Starter"));
        slots.Add(new LineupSlotIdentity("Goalie", 1, "Backup"));
        return slots;
    }

    private static void MarkManualUpdate(TeamLineupData lineup, string slotType, int lineOrPairNumber, string slotPosition)
    {
        if (lineup == null)
        {
            return;
        }

        string now = DateTime.UtcNow.ToString("o");
        lineup.IsManual = true;
        lineup.UpdatedAtUtc = now;
        lineup.LastManualUpdateUtc = now;
        lineup.LastSelectedSlotType = slotType;
        lineup.LastSelectedLineOrPairNumber = lineOrPairNumber;
        lineup.LastSelectedSlotPosition = slotPosition;
    }

    private static int CompareEligiblePlayers(PlayerData left, PlayerData right)
    {
        int overallComparison = right.Overall.CompareTo(left.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        int potentialComparison = right.Potential.CompareTo(left.Potential);
        if (potentialComparison != 0)
        {
            return potentialComparison;
        }

        PlayerFatigueService.EnsureFatigueFields(left);
        PlayerFatigueService.EnsureFatigueFields(right);
        int conditionComparison = right.Condition.CompareTo(left.Condition);
        if (conditionComparison != 0)
        {
            return conditionComparison;
        }

        int fatigueComparison = left.Fatigue.CompareTo(right.Fatigue);
        if (fatigueComparison != 0)
        {
            return fatigueComparison;
        }

        return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }

    private static void SetSlotInfo(
        string slotType,
        int lineOrPairNumber,
        string slotPosition,
        out string existingSlotType,
        out int existingLineOrPairNumber,
        out string existingSlotPosition)
    {
        existingSlotType = slotType;
        existingLineOrPairNumber = lineOrPairNumber;
        existingSlotPosition = slotPosition;
    }

    private static string FormatSlot(string slotType, int lineOrPairNumber, string slotPosition)
    {
        return slotType + " " + lineOrPairNumber + " " + slotPosition;
    }

    private static void AddId(HashSet<string> ids, string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            ids.Add(id);
        }
    }

    private struct LineupSlotIdentity
    {
        public string SlotType;
        public int LineOrPairNumber;
        public string SlotPosition;

        public LineupSlotIdentity(string slotType, int lineOrPairNumber, string slotPosition)
        {
            SlotType = slotType;
            LineOrPairNumber = lineOrPairNumber;
            SlotPosition = slotPosition;
        }
    }

    private static void AddPlayerById(TeamData team, string playerId, List<PlayerData> players)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (RosterStatusConfig.IsNhlRoster(player) && !player.IsRetired)
        {
            players.Add(player);
        }
    }

    private static void SetValidation(TeamLineupData lineup, bool isValid, string message)
    {
        if (lineup == null)
        {
            return;
        }

        lineup.IsValid = isValid;
        lineup.ValidationMessage = message;
    }

    private static int GetRequiredCountForCategory(string category)
    {
        if (category == "F")
        {
            return LineupConfig.ActiveForwardCount;
        }

        if (category == "D")
        {
            return LineupConfig.ActiveDefenseCount;
        }

        if (category == "G")
        {
            return LineupConfig.ActiveGoalieCount;
        }

        return 0;
    }

    private static bool IsInjuryValidationMessage(string message)
    {
        return !string.IsNullOrEmpty(message) && message.Contains("травм");
    }
}
