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
                    RecoverPlayer(state, player);
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
                    RecoverPlayer(state, player);
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
            TeamName = team.City + " " + team.Name,
            Position = player.Position,
            Age = player.Age,
            InjuryType = type,
            InjurySeverity = severity,
            InjuryDays = days,
            InjuryDaysRemainingAtCreation = days,
            InjuredAtUtc = injuredAt,
            ExpectedReturnDate = expectedReturnDate,
            Status = "Active",
            Source = string.IsNullOrEmpty(source) ? "Generated" : source
        };

        state.InjuryHistory.Injuries.Add(record);
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

    private static void RecoverPlayer(GameState state, PlayerData player)
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

        if (state == null || state.InjuryHistory == null || state.InjuryHistory.Injuries == null)
        {
            return;
        }

        foreach (InjuryRecordData record in state.InjuryHistory.Injuries)
        {
            if (record != null && record.PlayerId == playerId && record.Status == "Active")
            {
                record.Status = "Recovered";
            }
        }
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
