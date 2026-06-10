using System;
using System.Collections.Generic;
using UnityEngine;

public static class SpecialTeamsService
{
    public static void EnsureSpecialTeams(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        LineupService.EnsureLineup(team);
        if (team.SpecialTeams == null)
        {
            team.SpecialTeams = BuildAutoSpecialTeams(team);
        }
        else
        {
            team.SpecialTeams.EnsureCollections();
        }

        if (!ValidateSpecialTeams(team, out string message))
        {
            Debug.LogWarning("Special teams invalid for " + team.Name + ": " + message);
            if (IsInjuryValidationMessage(message))
            {
                team.SpecialTeams.IsValid = false;
                team.SpecialTeams.ValidationMessage = message;
                return;
            }

            team.SpecialTeams = BuildAutoSpecialTeams(team);

            if (!ValidateSpecialTeams(team, out message))
            {
                team.SpecialTeams.IsValid = false;
                team.SpecialTeams.ValidationMessage = message;
            }
        }
    }

    public static void EnsureSpecialTeamsForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureSpecialTeams(team);
        }
    }

    public static SpecialTeamsData BuildAutoSpecialTeams(TeamData team)
    {
        SpecialTeamsData specialTeams = new SpecialTeamsData
        {
            TeamId = team == null ? "" : team.Id,
            PowerPlayUnits = new List<PowerPlayUnitData>(),
            PenaltyKillUnits = new List<PenaltyKillUnitData>(),
            IsValid = false,
            ValidationMessage = "",
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        if (team == null)
        {
            specialTeams.ValidationMessage = "Команда не найдена";
            return specialTeams;
        }

        List<PlayerData> skaters = GetAvailableSkaters(team);
        List<PlayerData> powerPlayCandidates = new List<PlayerData>(skaters);
        List<PlayerData> penaltyKillCandidates = new List<PlayerData>(skaters);
        powerPlayCandidates.Sort(ComparePowerPlayPlayers);
        penaltyKillCandidates.Sort(ComparePenaltyKillPlayers);

        for (int unitNumber = 1; unitNumber <= SpecialTeamsConfig.PowerPlayUnitCount; unitNumber++)
        {
            int startIndex = (unitNumber - 1) * SpecialTeamsConfig.PowerPlayPlayersPerUnit;
            specialTeams.PowerPlayUnits.Add(CreatePowerPlayUnit(unitNumber, powerPlayCandidates, startIndex));
        }

        for (int unitNumber = 1; unitNumber <= SpecialTeamsConfig.PenaltyKillUnitCount; unitNumber++)
        {
            int startIndex = (unitNumber - 1) * SpecialTeamsConfig.PenaltyKillPlayersPerUnit;
            specialTeams.PenaltyKillUnits.Add(CreatePenaltyKillUnit(unitNumber, penaltyKillCandidates, startIndex));
        }

        SpecialTeamsData previousSpecialTeams = team.SpecialTeams;
        team.SpecialTeams = specialTeams;
        bool isValid = ValidateSpecialTeams(team, out string message);
        specialTeams.IsValid = isValid;
        specialTeams.ValidationMessage = message;
        team.SpecialTeams = previousSpecialTeams;

        specialTeams.Touch();
        return specialTeams;
    }

    public static bool ValidateSpecialTeams(TeamData team, out string message)
    {
        if (team == null)
        {
            message = "Команда не найдена";
            return false;
        }

        team.EnsurePlayers();
        SpecialTeamsData specialTeams = team.SpecialTeams;
        if (specialTeams == null)
        {
            message = "Спецбригады не созданы";
            return false;
        }

        specialTeams.EnsureCollections();
        if (specialTeams.PowerPlayUnits.Count != SpecialTeamsConfig.PowerPlayUnitCount)
        {
            message = "Нужно 2 power play unit";
            SetValidation(specialTeams, false, message);
            return false;
        }

        if (specialTeams.PenaltyKillUnits.Count != SpecialTeamsConfig.PenaltyKillUnitCount)
        {
            message = "Нужно 2 penalty kill unit";
            SetValidation(specialTeams, false, message);
            return false;
        }

        foreach (PowerPlayUnitData unit in specialTeams.PowerPlayUnits)
        {
            if (!ValidateUnit(team, GetPowerPlayIds(unit), SpecialTeamsConfig.PowerPlayPlayersPerUnit, out message))
            {
                SetValidation(specialTeams, false, message);
                return false;
            }
        }

        foreach (PenaltyKillUnitData unit in specialTeams.PenaltyKillUnits)
        {
            if (!ValidateUnit(team, GetPenaltyKillIds(unit), SpecialTeamsConfig.PenaltyKillPlayersPerUnit, out message))
            {
                SetValidation(specialTeams, false, message);
                return false;
            }
        }

        message = "Спецбригады валидны";
        SetValidation(specialTeams, true, message);
        return true;
    }

    public static List<PlayerData> GetPowerPlayPlayers(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        EnsureSpecialTeams(team);
        if (team == null || team.SpecialTeams == null)
        {
            return players;
        }

        foreach (PowerPlayUnitData unit in team.SpecialTeams.PowerPlayUnits)
        {
            AddPlayersByIds(team, GetPowerPlayIds(unit), players);
        }

        return players;
    }

    public static List<PlayerData> GetPenaltyKillPlayers(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        EnsureSpecialTeams(team);
        if (team == null || team.SpecialTeams == null)
        {
            return players;
        }

        foreach (PenaltyKillUnitData unit in team.SpecialTeams.PenaltyKillUnits)
        {
            AddPlayersByIds(team, GetPenaltyKillIds(unit), players);
        }

        return players;
    }

    public static int CalculatePowerPlayRating(TeamData team)
    {
        EnsureSpecialTeams(team);
        if (team == null || team.SpecialTeams == null || team.SpecialTeams.PowerPlayUnits.Count == 0)
        {
            return TeamRatingCalculator.CalculateOffenseRating(team);
        }

        float rating = 0f;
        foreach (PowerPlayUnitData unit in team.SpecialTeams.PowerPlayUnits)
        {
            float weight = unit != null && unit.UnitNumber == 1
                ? SpecialTeamsConfig.PowerPlayUnit1Weight
                : SpecialTeamsConfig.PowerPlayUnit2Weight;
            rating += AveragePlayers(team, GetPowerPlayIds(unit), false) * weight;
        }

        return Mathf.Clamp(Mathf.RoundToInt(rating), 40, 99);
    }

    public static int CalculatePenaltyKillRating(TeamData team)
    {
        EnsureSpecialTeams(team);
        if (team == null || team.SpecialTeams == null || team.SpecialTeams.PenaltyKillUnits.Count == 0)
        {
            return TeamRatingCalculator.CalculateDefenseRating(team);
        }

        float rating = 0f;
        foreach (PenaltyKillUnitData unit in team.SpecialTeams.PenaltyKillUnits)
        {
            float weight = unit != null && unit.UnitNumber == 1
                ? SpecialTeamsConfig.PenaltyKillUnit1Weight
                : SpecialTeamsConfig.PenaltyKillUnit2Weight;
            rating += AveragePlayers(team, GetPenaltyKillIds(unit), true) * weight;
        }

        return Mathf.Clamp(Mathf.RoundToInt(rating), 40, 99);
    }

    public static bool IsPlayerOnPowerPlay(TeamData team, string playerId)
    {
        if (team == null || string.IsNullOrEmpty(playerId))
        {
            return false;
        }

        EnsureSpecialTeams(team);
        foreach (PowerPlayUnitData unit in team.SpecialTeams.PowerPlayUnits)
        {
            if (ContainsId(GetPowerPlayIds(unit), playerId))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsPlayerOnPenaltyKill(TeamData team, string playerId)
    {
        if (team == null || string.IsNullOrEmpty(playerId))
        {
            return false;
        }

        EnsureSpecialTeams(team);
        foreach (PenaltyKillUnitData unit in team.SpecialTeams.PenaltyKillUnits)
        {
            if (ContainsId(GetPenaltyKillIds(unit), playerId))
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

    public static int GetPowerPlayUnitNumberForPlayer(TeamData team, string playerId)
    {
        if (team == null || team.SpecialTeams == null || string.IsNullOrEmpty(playerId))
        {
            return 0;
        }

        foreach (PowerPlayUnitData unit in team.SpecialTeams.PowerPlayUnits)
        {
            if (unit != null && ContainsId(GetPowerPlayIds(unit), playerId))
            {
                return unit.UnitNumber;
            }
        }

        return 0;
    }

    private static List<PlayerData> GetAvailableSkaters(TeamData team)
    {
        List<PlayerData> skaters = LineupService.GetActiveSkaters(team);
        if (skaters.Count > 0)
        {
            List<PlayerData> availableSkaters = FilterAvailablePlayers(skaters);
            return availableSkaters.Count >= GetRequiredSpecialTeamsSkaterCount() ? availableSkaters : skaters;
        }

        skaters = new List<PlayerData>();
        List<PlayerData> fallbackAvailableSkaters = new List<PlayerData>();
        if (team == null || team.Players == null)
        {
            return skaters;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.Position != "G")
            {
                skaters.Add(player);
                if (InjuryService.IsPlayerAvailable(player))
                {
                    fallbackAvailableSkaters.Add(player);
                }
            }
        }

        if (fallbackAvailableSkaters.Count >= GetRequiredSpecialTeamsSkaterCount())
        {
            return fallbackAvailableSkaters;
        }

        return skaters;
    }

    private static PowerPlayUnitData CreatePowerPlayUnit(int unitNumber, List<PlayerData> players, int startIndex)
    {
        return new PowerPlayUnitData
        {
            UnitNumber = unitNumber,
            Player1Id = GetPlayerId(players, startIndex),
            Player2Id = GetPlayerId(players, startIndex + 1),
            Player3Id = GetPlayerId(players, startIndex + 2),
            Player4Id = GetPlayerId(players, startIndex + 3),
            Player5Id = GetPlayerId(players, startIndex + 4)
        };
    }

    private static PenaltyKillUnitData CreatePenaltyKillUnit(int unitNumber, List<PlayerData> players, int startIndex)
    {
        return new PenaltyKillUnitData
        {
            UnitNumber = unitNumber,
            Player1Id = GetPlayerId(players, startIndex),
            Player2Id = GetPlayerId(players, startIndex + 1),
            Player3Id = GetPlayerId(players, startIndex + 2),
            Player4Id = GetPlayerId(players, startIndex + 3)
        };
    }

    private static string GetPlayerId(List<PlayerData> players, int index)
    {
        return players != null && index >= 0 && index < players.Count && players[index] != null
            ? players[index].Id
            : "";
    }

    private static int ComparePowerPlayPlayers(PlayerData left, PlayerData right)
    {
        int leftScore = left.Overall + left.Potential + (left.Position == "D" ? 0 : 5);
        int rightScore = right.Overall + right.Potential + (right.Position == "D" ? 0 : 5);
        return rightScore.CompareTo(leftScore);
    }

    private static int ComparePenaltyKillPlayers(PlayerData left, PlayerData right)
    {
        int leftScore = left.Overall + (left.Position == "D" ? 6 : 0);
        int rightScore = right.Overall + (right.Position == "D" ? 6 : 0);
        return rightScore.CompareTo(leftScore);
    }

    private static bool ValidateUnit(TeamData team, List<string> playerIds, int requiredCount, out string message)
    {
        if (playerIds == null || playerIds.Count != requiredCount)
        {
            message = "Неверное количество игроков в спецбригаде";
            return false;
        }

        HashSet<string> usedIds = new HashSet<string>();
        foreach (string playerId in playerIds)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                message = "Есть пустые слоты в спецбригаде";
                return false;
            }

            PlayerData player = FindPlayer(team, playerId);
            if (player == null)
            {
                message = "Игрок спецбригады не найден в roster";
                return false;
            }

            InjuryService.EnsureInjuryFields(player);
            if (player.IsInjured)
            {
                message = "Игрок спецбригады травмирован: " + player.FirstName + " " + player.LastName
                    + " (" + player.InjurySeverity + ", " + player.InjuryDaysRemaining + " дн.)";
                return false;
            }

            if (usedIds.Contains(playerId))
            {
                message = "Игрок повторяется внутри одной спецбригады";
                return false;
            }

            usedIds.Add(playerId);
        }

        message = "";
        return true;
    }

    private static List<string> GetPowerPlayIds(PowerPlayUnitData unit)
    {
        return new List<string>
        {
            unit == null ? "" : unit.Player1Id,
            unit == null ? "" : unit.Player2Id,
            unit == null ? "" : unit.Player3Id,
            unit == null ? "" : unit.Player4Id,
            unit == null ? "" : unit.Player5Id
        };
    }

    private static List<string> GetPenaltyKillIds(PenaltyKillUnitData unit)
    {
        return new List<string>
        {
            unit == null ? "" : unit.Player1Id,
            unit == null ? "" : unit.Player2Id,
            unit == null ? "" : unit.Player3Id,
            unit == null ? "" : unit.Player4Id
        };
    }

    private static void AddPlayersByIds(TeamData team, List<string> playerIds, List<PlayerData> players)
    {
        foreach (string playerId in playerIds)
        {
            PlayerData player = FindPlayer(team, playerId);
            if (player != null)
            {
                players.Add(player);
            }
        }
    }

    private static float AveragePlayers(TeamData team, List<string> playerIds, bool defenseBonus)
    {
        int total = 0;
        int count = 0;
        foreach (string playerId in playerIds)
        {
            PlayerData player = FindPlayer(team, playerId);
            if (player == null)
            {
                continue;
            }

            if (!InjuryService.IsPlayerAvailable(player))
            {
                continue;
            }

            total += player.Overall + (defenseBonus && player.Position == "D" ? 2 : 0);
            count++;
        }

        return count == 0 ? 70f : (float)total / count;
    }

    private static bool ContainsId(List<string> playerIds, string playerId)
    {
        foreach (string id in playerIds)
        {
            if (id == playerId)
            {
                return true;
            }
        }

        return false;
    }

    private static void SetValidation(SpecialTeamsData specialTeams, bool isValid, string message)
    {
        if (specialTeams == null)
        {
            return;
        }

        specialTeams.IsValid = isValid;
        specialTeams.ValidationMessage = message;
    }

    private static List<PlayerData> FilterAvailablePlayers(List<PlayerData> players)
    {
        List<PlayerData> availablePlayers = new List<PlayerData>();
        if (players == null)
        {
            return availablePlayers;
        }

        foreach (PlayerData player in players)
        {
            if (InjuryService.IsPlayerAvailable(player))
            {
                availablePlayers.Add(player);
            }
        }

        return availablePlayers;
    }

    private static int GetRequiredSpecialTeamsSkaterCount()
    {
        int powerPlayCount = SpecialTeamsConfig.PowerPlayUnitCount * SpecialTeamsConfig.PowerPlayPlayersPerUnit;
        int penaltyKillCount = SpecialTeamsConfig.PenaltyKillUnitCount * SpecialTeamsConfig.PenaltyKillPlayersPerUnit;
        return Mathf.Max(powerPlayCount, penaltyKillCount);
    }

    private static bool IsInjuryValidationMessage(string message)
    {
        return !string.IsNullOrEmpty(message) && message.Contains("травм");
    }
}
