using UnityEngine;

public static class LiveMatchTacticsService
{
    public static void SetTeamTactic(LiveMatchStateData match, TeamData team, string tacticName)
    {
        if (match == null || team == null || string.IsNullOrEmpty(tacticName))
        {
            return;
        }

        TacticsService.SetTacticsPreset(team, tacticName);
        LiveMatchTeamStatsData stats = GetStats(match, team.Id);
        if (stats != null)
        {
            stats.TacticName = team.Tactics == null ? tacticName : team.Tactics.PresetName;
        }

        LiveMatchSimulator.AddEvent(match, LiveMatchSimulator.CreateEvent(
            match,
            "TacticChanged",
            team,
            null,
            TeamIdentityService.GetDisplayName(team) + ": тактика " + tacticName,
            1));
    }

    public static float GetShotModifier(TeamData team)
    {
        return Mathf.Clamp(TacticsService.GetShotModifier(team) + GetCoachingFit(team), 0.75f, 1.35f);
    }

    public static float GetGoalModifier(TeamData team)
    {
        return Mathf.Clamp(TacticsService.GetGoalModifier(team) + GetCoachingFit(team), 0.78f, 1.30f);
    }

    public static float GetDefenseModifier(TeamData team)
    {
        return Mathf.Clamp(TacticsService.GetDefenseModifier(team) + GetCoachingFit(team), 0.80f, 1.30f);
    }

    public static float GetPenaltyModifier(TeamData team)
    {
        return Mathf.Clamp(TacticsService.GetPenaltyModifier(team) - GetCoachingFit(team), 0.65f, 1.55f);
    }

    private static float GetCoachingFit(TeamData team)
    {
        return Mathf.Clamp(TacticsService.GetTacticalFitModifier(team) * 0.006f, -0.08f, 0.08f);
    }

    private static LiveMatchTeamStatsData GetStats(LiveMatchStateData match, string teamId)
    {
        if (match == null)
        {
            return null;
        }

        if (match.HomeTeamId == teamId)
        {
            return match.HomeStats;
        }

        return match.AwayTeamId == teamId ? match.AwayStats : null;
    }
}
