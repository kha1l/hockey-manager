using System.Collections.Generic;
using UnityEngine;

public static class TacticsService
{
    public static void EnsureTactics(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        if (team.Tactics == null || string.IsNullOrEmpty(team.Tactics.PresetName))
        {
            team.Tactics = TacticsConfig.CreateBalanced(team.Id);
        }
    }

    public static void EnsureTacticsForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureTactics(team);
        }
    }

    public static void SetTacticsPreset(TeamData team, string presetName)
    {
        if (team == null)
        {
            return;
        }

        team.Tactics = TacticsConfig.CreateByPreset(team.Id, presetName);
        team.Tactics.Touch();
    }

    public static float GetShotModifier(TeamData team)
    {
        TeamTacticsData tactics = GetTactics(team);
        float modifier = 1f
            + ((tactics.Tempo - 50) * 0.003f)
            + ((tactics.ShootingFrequency - 50) * 0.004f);
        return Mathf.Clamp(modifier, 0.86f, 1.18f);
    }

    public static float GetGoalModifier(TeamData team)
    {
        TeamTacticsData tactics = GetTactics(team);
        float modifier = 1f
            + ((tactics.OffensiveFocus - 50) * 0.0025f)
            + ((tactics.RiskLevel - 50) * 0.0015f);
        return Mathf.Clamp(modifier, 0.90f, 1.12f);
    }

    public static float GetDefenseModifier(TeamData team)
    {
        TeamTacticsData tactics = GetTactics(team);
        float modifier = 1f
            + ((tactics.DefensiveFocus - 50) * 0.0025f)
            - ((tactics.RiskLevel - 50) * 0.002f);
        return Mathf.Clamp(modifier, 0.90f, 1.12f);
    }

    public static float GetPenaltyModifier(TeamData team)
    {
        TeamTacticsData tactics = GetTactics(team);
        float modifier = 1f
            + ((tactics.Aggressiveness - 50) * 0.006f)
            + ((tactics.RiskLevel - 50) * 0.003f);
        return Mathf.Clamp(modifier, 0.80f, 1.35f);
    }

    public static float GetRiskModifier(TeamData team)
    {
        TeamTacticsData tactics = GetTactics(team);
        float modifier = 1f + ((tactics.RiskLevel - 50) * 0.006f);
        return Mathf.Clamp(modifier, 0.75f, 1.35f);
    }

    private static TeamTacticsData GetTactics(TeamData team)
    {
        EnsureTactics(team);
        return team == null || team.Tactics == null
            ? TacticsConfig.CreateBalanced("")
            : team.Tactics;
    }
}
