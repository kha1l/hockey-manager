using System.Collections.Generic;
using UnityEngine;

public static class TeamRatingCalculator
{
    public static int CalculateOverall(TeamData team)
    {
        if (team == null || team.Players == null || team.Players.Count == 0)
        {
            return 70;
        }

        int totalOverall = 0;
        int playerCount = 0;
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        foreach (PlayerData player in team.Players)
        {
            if (RosterStatusConfig.IsNhlRoster(player) && !player.IsRetired && IsAvailable(player))
            {
                totalOverall += player.Overall;
                playerCount++;
            }
        }

        if (playerCount == 0)
        {
            return 70;
        }

        int averageOverall = Mathf.RoundToInt((float)totalOverall / playerCount);
        return Mathf.Clamp(averageOverall, 50, 99);
    }

    public static int CalculateLineupOverall(TeamData team)
    {
        LineupService.EnsureLineup(team);
        if (team == null || team.Lineup == null || !team.Lineup.IsValid)
        {
            return CalculateOverall(team);
        }

        int offense = CalculateOffenseRating(team);
        int defense = CalculateDefenseRating(team);
        int goalie = CalculateGoalieRating(team);
        int total = Mathf.RoundToInt((offense * 0.45f) + (defense * 0.30f) + (goalie * 0.25f));
        total += CoachingStaffService.GetTeamRatingModifier(team);
        return Mathf.Clamp(total, 50, 99);
    }

    public static int CalculateOffenseRating(TeamData team)
    {
        LineupService.EnsureLineup(team);
        if (team == null || team.Lineup == null)
        {
            return CalculateOverall(team);
        }

        float total = 0f;
        foreach (ForwardLineData line in team.Lineup.ForwardLines)
        {
            float weight = GetForwardLineWeight(line == null ? 0 : line.LineNumber);
            total += AveragePlayers(GetForwardLinePlayers(team, line)) * weight;
        }

        return Mathf.Clamp(Mathf.RoundToInt(total) + CoachingStaffService.GetOffenseModifier(team), 50, 99);
    }

    public static int CalculateDefenseRating(TeamData team)
    {
        LineupService.EnsureLineup(team);
        if (team == null || team.Lineup == null)
        {
            return CalculateOverall(team);
        }

        float total = 0f;
        foreach (DefensePairData pair in team.Lineup.DefensePairs)
        {
            float weight = GetDefensePairWeight(pair == null ? 0 : pair.PairNumber);
            total += AveragePlayers(GetDefensePairPlayers(team, pair)) * weight;
        }

        return Mathf.Clamp(Mathf.RoundToInt(total) + CoachingStaffService.GetDefenseModifier(team), 50, 99);
    }

    public static int CalculateGoalieRating(TeamData team)
    {
        LineupService.EnsureLineup(team);
        if (team == null)
        {
            return 70;
        }

        PlayerData starter = LineupService.GetStartingGoalie(team);
        PlayerData backup = LineupService.GetBackupGoalie(team);
        if (!IsAvailable(starter))
        {
            starter = IsAvailable(backup) ? backup : FindBestAvailableGoalie(team);
            backup = FindBestAvailableGoalie(team, starter == null ? "" : starter.Id);
        }

        if (starter == null)
        {
            return CalculateOverall(team);
        }

        int backupOverall = IsAvailable(backup) ? backup.Overall : starter.Overall;
        float total = (starter.Overall * LineupConfig.StarterGoalieWeight)
            + (backupOverall * LineupConfig.BackupGoalieWeight);
        int goalieModifier = Mathf.Clamp(CoachingStaffService.GetGoalieDevelopmentModifier(team, starter), -1, 1);
        return Mathf.Clamp(Mathf.RoundToInt(total) + goalieModifier, 50, 99);
    }

    public static int CalculatePowerPlayRating(TeamData team)
    {
        return SpecialTeamsService.CalculatePowerPlayRating(team);
    }

    public static int CalculatePenaltyKillRating(TeamData team)
    {
        return SpecialTeamsService.CalculatePenaltyKillRating(team);
    }

    public static int CalculateTacticsAdjustedOverall(TeamData team)
    {
        int baseRating = CalculateLineupOverall(team);
        TacticsService.EnsureTactics(team);
        if (team == null || team.Tactics == null)
        {
            return baseRating;
        }

        float modifier = 1f;
        modifier += (team.Tactics.OffensiveFocus - 50) * 0.0008f;
        modifier += (team.Tactics.DefensiveFocus - 50) * 0.0006f;
        modifier -= Mathf.Abs(team.Tactics.RiskLevel - 50) * 0.00025f;

        return Mathf.Clamp(Mathf.RoundToInt(baseRating * modifier) + CoachingStaffService.GetTacticalFitModifier(team), 50, 99);
    }

    public static int CalculateEffectiveLineupOverall(TeamData team)
    {
        LineupService.EnsureLineup(team);
        PlayerFatigueService.EnsureFatigueForTeam(team);
        if (team == null || team.Lineup == null || !team.Lineup.IsValid)
        {
            return ClampFinalRating(CalculateLineupOverall(team) + ChemistryService.GetTeamChemistryRatingModifier(team));
        }

        int offense = CalculateEffectiveOffenseRating(team);
        int defense = CalculateEffectiveDefenseRating(team);
        int goalie = CalculateEffectiveGoalieRating(team);
        int total = Mathf.RoundToInt((offense * 0.45f) + (defense * 0.30f) + (goalie * 0.25f));
        total += ChemistryService.GetTeamChemistryRatingModifier(team);
        total += CoachingStaffService.GetTeamRatingModifier(team);
        return ClampFinalRating(total);
    }

    public static int CalculateEffectiveOffenseRating(TeamData team)
    {
        LineupService.EnsureLineup(team);
        PlayerFatigueService.EnsureFatigueForTeam(team);
        if (team == null || team.Lineup == null || !team.Lineup.IsValid)
        {
            return CalculateOffenseRating(team);
        }

        float total = 0f;
        foreach (ForwardLineData line in team.Lineup.ForwardLines)
        {
            float weight = GetForwardLineWeight(line == null ? 0 : line.LineNumber);
            total += AverageEffectivePlayers(GetForwardLinePlayers(team, line)) * weight;
        }

        return Mathf.Clamp(Mathf.RoundToInt(total) + CoachingStaffService.GetOffenseModifier(team), 50, 99);
    }

    public static int CalculateEffectiveDefenseRating(TeamData team)
    {
        LineupService.EnsureLineup(team);
        PlayerFatigueService.EnsureFatigueForTeam(team);
        if (team == null || team.Lineup == null || !team.Lineup.IsValid)
        {
            return CalculateDefenseRating(team);
        }

        float total = 0f;
        foreach (DefensePairData pair in team.Lineup.DefensePairs)
        {
            float weight = GetDefensePairWeight(pair == null ? 0 : pair.PairNumber);
            total += AverageEffectivePlayers(GetDefensePairPlayers(team, pair)) * weight;
        }

        return Mathf.Clamp(Mathf.RoundToInt(total) + CoachingStaffService.GetDefenseModifier(team), 50, 99);
    }

    public static int CalculateEffectiveGoalieRating(TeamData team)
    {
        LineupService.EnsureLineup(team);
        PlayerFatigueService.EnsureFatigueForTeam(team);
        if (team == null || team.Lineup == null || !team.Lineup.IsValid)
        {
            return CalculateGoalieRating(team);
        }

        PlayerData starter = LineupService.GetStartingGoalie(team);
        PlayerData backup = LineupService.GetBackupGoalie(team);
        if (!IsAvailable(starter))
        {
            starter = IsAvailable(backup) ? backup : FindBestAvailableGoalie(team);
            backup = FindBestAvailableGoalie(team, starter == null ? "" : starter.Id);
        }

        if (starter == null)
        {
            return CalculateGoalieRating(team);
        }

        int starterOverall = PlayerFatigueService.GetEffectiveOverall(starter);
        int backupOverall = IsAvailable(backup) ? PlayerFatigueService.GetEffectiveOverall(backup) : starterOverall;
        float total = (starterOverall * LineupConfig.StarterGoalieWeight)
            + (backupOverall * LineupConfig.BackupGoalieWeight);
        int goalieModifier = Mathf.Clamp(CoachingStaffService.GetGoalieDevelopmentModifier(team, starter), -1, 1);
        return Mathf.Clamp(Mathf.RoundToInt(total) + goalieModifier, 50, 99);
    }

    private static float AveragePlayers(List<PlayerData> players)
    {
        if (players == null || players.Count == 0)
        {
            return 70f;
        }

        int total = 0;
        int count = 0;
        foreach (PlayerData player in players)
        {
            if (IsAvailable(player))
            {
                total += player.Overall;
                count++;
            }
        }

        return count == 0 ? 70f : (float)total / count;
    }

    private static int ClampFinalRating(int rating)
    {
        return Mathf.Clamp(rating, 40, 99);
    }

    private static float AverageEffectivePlayers(List<PlayerData> players)
    {
        if (players == null || players.Count == 0)
        {
            return 70f;
        }

        int total = 0;
        int count = 0;
        foreach (PlayerData player in players)
        {
            if (IsAvailable(player))
            {
                total += PlayerFatigueService.GetEffectiveOverall(player);
                count++;
            }
        }

        return count == 0 ? 70f : (float)total / count;
    }

    private static List<PlayerData> GetForwardLinePlayers(TeamData team, ForwardLineData line)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (line == null)
        {
            return players;
        }

        AddPlayer(players, FindPlayer(team, line.LeftWingPlayerId));
        AddPlayer(players, FindPlayer(team, line.CenterPlayerId));
        AddPlayer(players, FindPlayer(team, line.RightWingPlayerId));
        return players;
    }

    private static List<PlayerData> GetDefensePairPlayers(TeamData team, DefensePairData pair)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (pair == null)
        {
            return players;
        }

        AddPlayer(players, FindPlayer(team, pair.LeftDefensePlayerId));
        AddPlayer(players, FindPlayer(team, pair.RightDefensePlayerId));
        return players;
    }

    private static void AddPlayer(List<PlayerData> players, PlayerData player)
    {
        if (player != null && !player.IsRetired)
        {
            players.Add(player);
        }
    }

    private static bool IsAvailable(PlayerData player)
    {
        return RosterStatusConfig.IsNhlRoster(player) && !player.IsRetired && InjuryService.IsPlayerAvailable(player);
    }

    private static PlayerData FindBestAvailableGoalie(TeamData team)
    {
        return FindBestAvailableGoalie(team, "");
    }

    private static PlayerData FindBestAvailableGoalie(TeamData team, string excludedPlayerId)
    {
        if (team == null || team.Players == null)
        {
            return null;
        }

        PlayerData bestGoalie = null;
        foreach (PlayerData player in team.Players)
        {
            if (player == null
                || !RosterStatusConfig.IsNhlRoster(player)
                || player.IsRetired
                || player.Position != "G"
                || player.Id == excludedPlayerId
                || !IsAvailable(player))
            {
                continue;
            }

            if (bestGoalie == null || player.Overall > bestGoalie.Overall)
            {
                bestGoalie = player;
            }
        }

        return bestGoalie;
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
            if (player != null && !player.IsRetired && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static float GetForwardLineWeight(int lineNumber)
    {
        if (lineNumber == 1)
        {
            return LineupConfig.ForwardLine1Weight;
        }

        if (lineNumber == 2)
        {
            return LineupConfig.ForwardLine2Weight;
        }

        if (lineNumber == 3)
        {
            return LineupConfig.ForwardLine3Weight;
        }

        return LineupConfig.ForwardLine4Weight;
    }

    private static float GetDefensePairWeight(int pairNumber)
    {
        if (pairNumber == 1)
        {
            return LineupConfig.DefensePair1Weight;
        }

        if (pairNumber == 2)
        {
            return LineupConfig.DefensePair2Weight;
        }

        return LineupConfig.DefensePair3Weight;
    }
}
