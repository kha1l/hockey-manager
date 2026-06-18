using System;
using System.Collections.Generic;
using UnityEngine;

public static class IceTimeService
{
    public static void EnsureUsageForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        TeamRosterService.EnsureRosterStatusesForTeam(team);
        PlayerRoleService.EnsureRolesForTeam(team);
        LineupService.EnsureLineup(team);
        SpecialTeamsService.EnsureSpecialTeams(team);
        ApplyEstimatedUsageToTeam(team);
    }

    public static void EnsureUsageForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureUsageForTeam(team);
        }
    }

    public static List<PlayerUsageData> CalculateTeamUsage(TeamData team)
    {
        List<PlayerUsageData> usageList = new List<PlayerUsageData>();
        EnsureUsageForTeam(team);
        if (team == null)
        {
            return usageList;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            PlayerRoleService.EnsureRole(player);
            PlayerFatigueService.EnsureFatigueFields(player);
            InjuryService.EnsureInjuryFields(player);
            MoraleService.InitializePlayerMorale(player);

            usageList.Add(new PlayerUsageData
            {
                PlayerId = player.Id,
                PlayerName = player.FirstName + " " + player.LastName,
                TeamId = team.Id,
                TeamName = TeamIdentityService.GetDisplayName(team),
                Position = player.Position,
                PlayerRole = player.PlayerRole,
                UsageCategory = player.UsageCategory,
                EstimatedTimeOnIceSeconds = player.EstimatedTimeOnIceSeconds,
                EffectiveOverall = PlayerFatigueService.GetEffectiveOverall(player),
                Condition = player.Condition,
                Fatigue = player.Fatigue,
                Morale = player.Morale,
                RoleSatisfaction = player.RoleSatisfaction,
                IceTimeSatisfaction = player.IceTimeSatisfaction,
                MoraleStatus = player.MoraleStatus,
                WantsTrade = player.WantsTrade,
                IsInjured = player.IsInjured,
                IsActive = LineupService.IsPlayerActive(team, player.Id),
                IsOnPowerPlay = SpecialTeamsService.IsPlayerOnPowerPlay(team, player.Id),
                IsOnPenaltyKill = SpecialTeamsService.IsPlayerOnPenaltyKill(team, player.Id)
            });
        }

        usageList.Sort(CompareUsage);
        return usageList;
    }

    public static TeamUsageSummaryData CalculateTeamUsageSummary(TeamData team)
    {
        TeamUsageSummaryData summary = new TeamUsageSummaryData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = TeamIdentityService.GetDisplayName(team),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        EnsureUsageForTeam(team);
        if (team == null)
        {
            return summary;
        }

        int activeTotal = 0;
        int activeCount = 0;
        int scratchCount = 0;
        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            if (RosterStatusConfig.IsNhlRoster(player) && LineupService.IsPlayerActive(team, player.Id))
            {
                activeTotal += player.EstimatedTimeOnIceSeconds;
                activeCount++;
            }
            else if (RosterStatusConfig.IsNhlRoster(player))
            {
                scratchCount++;
            }

            if (player.Position == "G" && player.UsageCategory == "StartingGoalie")
            {
                summary.StartingGoalieTimeOnIceSeconds = Mathf.Max(summary.StartingGoalieTimeOnIceSeconds, player.EstimatedTimeOnIceSeconds);
            }
            else if (player.Position == "D")
            {
                summary.TopDefenseTimeOnIceSeconds = Mathf.Max(summary.TopDefenseTimeOnIceSeconds, player.EstimatedTimeOnIceSeconds);
            }
            else if (player.Position == "C" || player.Position == "LW" || player.Position == "RW")
            {
                summary.TopForwardTimeOnIceSeconds = Mathf.Max(summary.TopForwardTimeOnIceSeconds, player.EstimatedTimeOnIceSeconds);
            }
        }

        summary.ActivePlayerCount = activeCount;
        summary.ScratchPlayerCount = scratchCount;
        summary.AverageActiveTimeOnIceSeconds = activeCount == 0 ? 0 : activeTotal / activeCount;
        return summary;
    }

    public static int CalculateEstimatedTimeOnIceSeconds(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return 0;
        }

        InjuryService.EnsureInjuryFields(player);
        if (!RosterStatusConfig.IsNhlRoster(player) || player.IsInjured)
        {
            return 0;
        }

        int seconds = GetBaseTimeOnIceSeconds(team, player);
        if (seconds <= 0 || player.Position == "G")
        {
            return seconds;
        }

        int powerPlayUnit = SpecialTeamsService.GetPowerPlayUnitNumberForPlayer(team, player.Id);
        if (powerPlayUnit == 1)
        {
            seconds += IceTimeConfig.PowerPlayUnit1BonusSeconds;
        }
        else if (powerPlayUnit == 2)
        {
            seconds += IceTimeConfig.PowerPlayUnit2BonusSeconds;
        }

        int penaltyKillUnit = GetPenaltyKillUnitNumberForPlayer(team, player.Id);
        if (penaltyKillUnit == 1)
        {
            seconds += IceTimeConfig.PenaltyKillUnit1BonusSeconds;
        }
        else if (penaltyKillUnit == 2)
        {
            seconds += IceTimeConfig.PenaltyKillUnit2BonusSeconds;
        }

        return seconds;
    }

    public static string DetermineUsageCategory(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return "Scratch";
        }

        if (RosterStatusConfig.IsFarmRoster(player))
        {
            return "Farm";
        }

        if (RosterStatusConfig.IsReserve(player))
        {
            return "Reserve";
        }

        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            return "Scratch";
        }

        LineupService.EnsureLineup(team);
        if (team.Lineup == null)
        {
            return "Scratch";
        }

        team.Lineup.EnsureCollections();
        foreach (ForwardLineData line in team.Lineup.ForwardLines)
        {
            if (line == null)
            {
                continue;
            }

            if (line.LeftWingPlayerId == player.Id || line.CenterPlayerId == player.Id || line.RightWingPlayerId == player.Id)
            {
                if (line.LineNumber == 1)
                {
                    return "FirstLine";
                }

                if (line.LineNumber == 2)
                {
                    return "SecondLine";
                }

                if (line.LineNumber == 3)
                {
                    return "ThirdLine";
                }

                return "FourthLine";
            }
        }

        foreach (DefensePairData pair in team.Lineup.DefensePairs)
        {
            if (pair == null)
            {
                continue;
            }

            if (pair.LeftDefensePlayerId == player.Id || pair.RightDefensePlayerId == player.Id)
            {
                if (pair.PairNumber == 1)
                {
                    return "FirstPair";
                }

                if (pair.PairNumber == 2)
                {
                    return "SecondPair";
                }

                return "ThirdPair";
            }
        }

        if (team.Lineup.Goalies != null)
        {
            if (team.Lineup.Goalies.StarterGoaliePlayerId == player.Id)
            {
                return "StartingGoalie";
            }

            if (team.Lineup.Goalies.BackupGoaliePlayerId == player.Id)
            {
                return "BackupGoalie";
            }
        }

        return "Scratch";
    }

    public static void ApplyEstimatedUsageToTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            PlayerRoleService.EnsureRole(player);
            player.UsageCategory = DetermineUsageCategory(team, player);
            player.EstimatedTimeOnIceSeconds = CalculateEstimatedTimeOnIceSeconds(team, player);
        }
    }

    public static void ApplyLastGameIceTime(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        EnsureUsageForTeam(team);
        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            if (RosterStatusConfig.IsNhlRoster(player)
                && LineupService.IsPlayerActive(team, player.Id)
                && player.EstimatedTimeOnIceSeconds > 0)
            {
                player.LastGameTimeOnIceSeconds = player.EstimatedTimeOnIceSeconds;
                player.TotalTimeOnIceSeconds += player.EstimatedTimeOnIceSeconds;
                player.GamesWithTimeOnIce++;
                player.AverageTimeOnIceSeconds = player.GamesWithTimeOnIce == 0
                    ? 0
                    : player.TotalTimeOnIceSeconds / player.GamesWithTimeOnIce;
            }
            else
            {
                player.LastGameTimeOnIceSeconds = 0;
            }
        }
    }

    public static int GetUsageWeightForStats(TeamData team, PlayerData player)
    {
        if (team == null
            || player == null
            || !RosterStatusConfig.IsNhlRoster(player)
            || !LineupService.IsPlayerActive(team, player.Id)
            || !InjuryService.IsPlayerAvailable(player))
        {
            return 0;
        }

        int seconds = player.EstimatedTimeOnIceSeconds > 0
            ? player.EstimatedTimeOnIceSeconds
            : CalculateEstimatedTimeOnIceSeconds(team, player);

        if (seconds <= 0)
        {
            return 0;
        }

        int weight;
        if (seconds >= 1500)
        {
            weight = 5;
        }
        else if (seconds >= 1200)
        {
            weight = 4;
        }
        else if (seconds >= 900)
        {
            weight = 3;
        }
        else if (seconds >= 600)
        {
            weight = 2;
        }
        else
        {
            weight = LineupService.IsPlayerActive(team, player.Id) ? 1 : 0;
        }

        if (SpecialTeamsService.IsPlayerOnPowerPlay(team, player.Id) || SpecialTeamsService.IsPlayerOnPenaltyKill(team, player.Id))
        {
            weight++;
        }

        return Mathf.Clamp(weight, 0, 6);
    }

    private static bool IsPlayerOnPowerPlayUnit(TeamData team, string playerId, int unitNumber)
    {
        return SpecialTeamsService.GetPowerPlayUnitNumberForPlayer(team, playerId) == unitNumber;
    }

    private static bool IsPlayerOnPenaltyKillUnit(TeamData team, string playerId, int unitNumber)
    {
        return GetPenaltyKillUnitNumberForPlayer(team, playerId) == unitNumber;
    }

    private static int GetBaseTimeOnIceSeconds(TeamData team, PlayerData player)
    {
        string usageCategory = DetermineUsageCategory(team, player);
        if (usageCategory == "FirstLine")
        {
            return IceTimeConfig.ForwardLine1Seconds;
        }

        if (usageCategory == "SecondLine")
        {
            return IceTimeConfig.ForwardLine2Seconds;
        }

        if (usageCategory == "ThirdLine")
        {
            return IceTimeConfig.ForwardLine3Seconds;
        }

        if (usageCategory == "FourthLine")
        {
            return IceTimeConfig.ForwardLine4Seconds;
        }

        if (usageCategory == "FirstPair")
        {
            return IceTimeConfig.DefensePair1Seconds;
        }

        if (usageCategory == "SecondPair")
        {
            return IceTimeConfig.DefensePair2Seconds;
        }

        if (usageCategory == "ThirdPair")
        {
            return IceTimeConfig.DefensePair3Seconds;
        }

        if (usageCategory == "StartingGoalie")
        {
            return IceTimeConfig.StartingGoalieSeconds;
        }

        if (usageCategory == "BackupGoalie")
        {
            return IceTimeConfig.BackupGoalieSeconds;
        }

        return IceTimeConfig.ScratchSeconds;
    }

    private static int GetPenaltyKillUnitNumberForPlayer(TeamData team, string playerId)
    {
        if (team == null || team.SpecialTeams == null || string.IsNullOrEmpty(playerId))
        {
            return 0;
        }

        PlayerData player = FindPlayer(team, playerId);
        if (!RosterStatusConfig.IsNhlRoster(player) || !LineupService.IsPlayerActive(team, playerId))
        {
            return 0;
        }

        team.SpecialTeams.EnsureCollections();
        foreach (PenaltyKillUnitData unit in team.SpecialTeams.PenaltyKillUnits)
        {
            if (unit == null)
            {
                continue;
            }

            if (unit.Player1Id == playerId
                || unit.Player2Id == playerId
                || unit.Player3Id == playerId
                || unit.Player4Id == playerId)
            {
                return unit.UnitNumber;
            }
        }

        return 0;
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

    private static int CompareUsage(PlayerUsageData left, PlayerUsageData right)
    {
        int activeComparison = right.IsActive.CompareTo(left.IsActive);
        if (activeComparison != 0)
        {
            return activeComparison;
        }

        int toiComparison = right.EstimatedTimeOnIceSeconds.CompareTo(left.EstimatedTimeOnIceSeconds);
        if (toiComparison != 0)
        {
            return toiComparison;
        }

        int overallComparison = right.EffectiveOverall.CompareTo(left.EffectiveOverall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        return string.Compare(left.PlayerId, right.PlayerId, StringComparison.Ordinal);
    }
}
