using System.Collections.Generic;
using UnityEngine;

public static class PlayerFatigueService
{
    public static void EnsureFatigueFields(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        if (player.Condition <= 0 && player.Fatigue <= 0)
        {
            player.Condition = FatigueConfig.DefaultCondition;
            player.Fatigue = FatigueConfig.DefaultFatigue;
        }

        player.Condition = FatigueConfig.ClampCondition(player.Condition);
        player.Fatigue = FatigueConfig.ClampFatigue(player.Fatigue);
    }

    public static void EnsureFatigueForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            EnsureFatigueFields(player);
        }
    }

    public static void EnsureFatigueForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureFatigueForTeam(team);
        }
    }

    public static int GetEffectiveOverall(PlayerData player)
    {
        if (player == null)
        {
            return 50;
        }

        EnsureFatigueFields(player);
        int effectiveOverall = player.Overall - GetOverallPenaltyFromCondition(player);
        int morale = player.HasMoraleInitialized ? player.Morale : MoraleConfig.DefaultMorale;
        effectiveOverall -= MoraleConfig.GetEffectiveOverallPenalty(morale);
        return Mathf.Clamp(effectiveOverall, 40, 99);
    }

    public static int GetOverallPenaltyFromCondition(PlayerData player)
    {
        if (player == null)
        {
            return 0;
        }

        EnsureFatigueFields(player);
        if (player.Condition >= 95)
        {
            return 0;
        }

        if (player.Condition >= 90)
        {
            return 1;
        }

        if (player.Condition >= 80)
        {
            return 2;
        }

        if (player.Condition >= 70)
        {
            return 4;
        }

        if (player.Condition >= 60)
        {
            return 6;
        }

        if (player.Condition >= 50)
        {
            return 8;
        }

        return 10;
    }

    public static void ApplyFatigueAfterMatch(TeamData homeTeam, TeamData awayTeam)
    {
        ApplyTeamMatchFatigue(homeTeam);
        ApplyTeamMatchFatigue(awayTeam);
    }

    public static void ApplyTeamMatchFatigue(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        LineupService.EnsureLineup(team);
        List<PlayerData> activePlayers = LineupService.GetActivePlayers(team);
        foreach (PlayerData player in activePlayers)
        {
            EnsureFatigueFields(player);
            int oldFatigue = player.Fatigue;
            int fatigueGain = GetFatigueGainForPlayer(team, player);
            player.Fatigue = FatigueConfig.ClampFatigue(player.Fatigue + fatigueGain);
            player.Condition = FatigueConfig.ClampCondition(FatigueConfig.DefaultCondition - player.Fatigue);
            player.ConsecutiveGamesPlayed++;
            player.GamesRested = 0;
            player.LastGameFatigueChange = player.Fatigue - oldFatigue;
            player.LastGameConditionChange = -player.LastGameFatigueChange;
        }

        RecoverScratches(team);
        RecoverNonNhlOrganizationPlayers(team);
    }

    public static void RecoverScratches(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        List<PlayerData> scratches = LineupService.GetScratchPlayers(team);
        foreach (PlayerData player in scratches)
        {
            RecoverPlayer(player, FatigueConfig.ScratchRecovery);
        }
    }

    public static void RecoverNonNhlOrganizationPlayers(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        foreach (PlayerData player in team.Players)
        {
            if (player != null && (RosterStatusConfig.IsFarmRoster(player) || RosterStatusConfig.IsReserve(player)))
            {
                RecoverPlayer(player, FatigueConfig.ScratchRecovery);
            }
        }
    }

    public static void RecoverNonPlayingTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            RecoverPlayer(player, FatigueConfig.NonPlayingTeamRecovery);
        }
    }

    public static void ResetFatigueForNewSeason(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
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

                player.Condition = FatigueConfig.DefaultCondition;
                player.Fatigue = FatigueConfig.DefaultFatigue;
                player.ConsecutiveGamesPlayed = 0;
                player.GamesRested = 0;
                player.IsResting = false;
                player.LastGameFatigueChange = 0;
                player.LastGameConditionChange = 0;
            }
        }
    }

    public static int GetFatigueGainForPlayer(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return FatigueConfig.MinSingleGameFatigueGain;
        }

        EnsureFatigueFields(player);
        int baseFatigue = GetBaseFatigueForPlayer(team, player);
        int fatigueGain = Mathf.RoundToInt(baseFatigue * GetTacticsFatigueModifier(team));

        if (player.ConsecutiveGamesPlayed >= 5)
        {
            fatigueGain++;
        }

        if (player.ConsecutiveGamesPlayed >= 10)
        {
            fatigueGain++;
        }

        if (player.Position == "G" && HasHighQualityGoalieCoach(team))
        {
            fatigueGain--;
        }

        return Mathf.Clamp(
            fatigueGain,
            FatigueConfig.MinSingleGameFatigueGain,
            FatigueConfig.MaxSingleGameFatigueGain);
    }

    private static bool HasHighQualityGoalieCoach(TeamData team)
    {
        if (team == null)
        {
            return false;
        }

        CoachingStaffService.EnsureStaffForTeam(team);
        return team.Staff != null
            && team.Staff.GoalieCoach != null
            && team.Staff.GoalieCoach.GoalieDevelopmentRating >= 82;
    }

    private static float GetTacticsFatigueModifier(TeamData team)
    {
        if (team == null || team.Tactics == null)
        {
            return 1f;
        }

        if (team.Tactics.PresetName == "Aggressive")
        {
            return 1.18f;
        }

        if (team.Tactics.PresetName == "Offensive")
        {
            return 1.08f;
        }

        if (team.Tactics.PresetName == "Defensive")
        {
            return 0.96f;
        }

        return 1f;
    }

    private static int GetBaseFatigueForPlayer(TeamData team, PlayerData player)
    {
        LineupService.EnsureLineup(team);
        if (team == null || team.Lineup == null || player == null || !RosterStatusConfig.IsNhlRoster(player))
        {
            return FatigueConfig.ForwardLine4Fatigue;
        }

        int timeOnIceSeconds = player.EstimatedTimeOnIceSeconds;
        if (timeOnIceSeconds <= 0)
        {
            timeOnIceSeconds = IceTimeService.CalculateEstimatedTimeOnIceSeconds(team, player);
        }

        if (timeOnIceSeconds > 0)
        {
            return GetBaseFatigueFromIceTime(team, player, timeOnIceSeconds);
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
                return GetForwardLineFatigue(line.LineNumber);
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
                return GetDefensePairFatigue(pair.PairNumber);
            }
        }

        if (team.Lineup.Goalies != null)
        {
            if (team.Lineup.Goalies.StarterGoaliePlayerId == player.Id)
            {
                return FatigueConfig.StartingGoalieFatigue;
            }

            if (team.Lineup.Goalies.BackupGoaliePlayerId == player.Id)
            {
                return FatigueConfig.BackupGoalieFatigue;
            }
        }

        return FatigueConfig.ForwardLine4Fatigue;
    }

    private static int GetBaseFatigueFromIceTime(TeamData team, PlayerData player, int timeOnIceSeconds)
    {
        int fatigueGain;
        if (timeOnIceSeconds >= 1500)
        {
            fatigueGain = 9;
        }
        else if (timeOnIceSeconds >= 1200)
        {
            fatigueGain = 7;
        }
        else if (timeOnIceSeconds >= 900)
        {
            fatigueGain = 5;
        }
        else if (timeOnIceSeconds >= 600)
        {
            fatigueGain = 4;
        }
        else
        {
            fatigueGain = 3;
        }

        if (team != null
            && team.Lineup != null
            && team.Lineup.Goalies != null
            && team.Lineup.Goalies.StarterGoaliePlayerId == player.Id)
        {
            fatigueGain = Mathf.Max(fatigueGain, FatigueConfig.StartingGoalieFatigue);
        }

        return fatigueGain;
    }

    private static int GetForwardLineFatigue(int lineNumber)
    {
        if (lineNumber == 1)
        {
            return FatigueConfig.ForwardLine1Fatigue;
        }

        if (lineNumber == 2)
        {
            return FatigueConfig.ForwardLine2Fatigue;
        }

        if (lineNumber == 3)
        {
            return FatigueConfig.ForwardLine3Fatigue;
        }

        return FatigueConfig.ForwardLine4Fatigue;
    }

    private static int GetDefensePairFatigue(int pairNumber)
    {
        if (pairNumber == 1)
        {
            return FatigueConfig.DefensePair1Fatigue;
        }

        if (pairNumber == 2)
        {
            return FatigueConfig.DefensePair2Fatigue;
        }

        return FatigueConfig.DefensePair3Fatigue;
    }

    private static void RecoverPlayer(PlayerData player, int recovery)
    {
        if (player == null)
        {
            return;
        }

        EnsureFatigueFields(player);
        int oldFatigue = player.Fatigue;
        player.Fatigue = FatigueConfig.ClampFatigue(player.Fatigue - recovery);
        player.Condition = FatigueConfig.ClampCondition(FatigueConfig.DefaultCondition - player.Fatigue);
        player.ConsecutiveGamesPlayed = 0;
        player.GamesRested++;
        player.LastGameFatigueChange = player.Fatigue - oldFatigue;
        player.LastGameConditionChange = -player.LastGameFatigueChange;
    }
}
