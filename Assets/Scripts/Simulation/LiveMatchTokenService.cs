using System.Collections.Generic;
using UnityEngine;

public static class LiveMatchTokenService
{
    public static void UpdateTokensForTick(LiveMatchStateData match, TeamData homeTeam, TeamData awayTeam)
    {
        if (match == null)
        {
            return;
        }

        match.EnsureCollections();
        match.Tokens.Clear();

        AddTeamTokens(match, homeTeam, true, GetStats(match, homeTeam));
        AddTeamTokens(match, awayTeam, false, GetStats(match, awayTeam));
    }

    private static void AddTeamTokens(LiveMatchStateData match, TeamData team, bool isHome, LiveMatchTeamStatsData stats)
    {
        if (team == null || stats == null)
        {
            return;
        }

        bool goaliePulled = stats.IsGoaliePulled;
        List<PlayerData> skaters = LiveMatchLineSelectorService.SelectSkaters(team, match, goaliePulled);
        PlayerData goalie = goaliePulled ? null : LiveMatchGoalieService.GetCurrentGoalie(match, team);
        float[,] positions = isHome
            ? new float[,] { { 0.30f, 0.30f }, { 0.35f, 0.50f }, { 0.30f, 0.70f }, { 0.18f, 0.38f }, { 0.18f, 0.62f }, { 0.43f, 0.50f } }
            : new float[,] { { 0.70f, 0.30f }, { 0.65f, 0.50f }, { 0.70f, 0.70f }, { 0.82f, 0.38f }, { 0.82f, 0.62f }, { 0.57f, 0.50f } };

        for (int i = 0; i < skaters.Count && i < positions.GetLength(0); i++)
        {
            AddToken(match, team, skaters[i], isHome, false, goaliePulled && i == skaters.Count - 1, positions[i, 0], positions[i, 1]);
        }

        if (goalie != null)
        {
            AddToken(match, team, goalie, isHome, true, false, isHome ? 0.07f : 0.93f, 0.50f);
        }
    }

    private static void AddToken(
        LiveMatchStateData match,
        TeamData team,
        PlayerData player,
        bool isHome,
        bool isGoalie,
        bool isPulledGoalieReplacement,
        float x,
        float y)
    {
        if (player == null)
        {
            return;
        }

        JerseyNumberService.EnsureJerseyNumber(player, team);
        float jitter = Mathf.Sin((match.TotalGameSecondsElapsed + player.JerseyNumber * 19) * 0.08f) * 0.018f;
        TeamIdentityData identity = team == null ? null : team.Identity;
        match.Tokens.Add(new LiveMatchPlayerTokenData
        {
            PlayerId = player.Id,
            PlayerName = player.FirstName + " " + player.LastName,
            TeamId = team == null ? "" : team.Id,
            TeamName = TeamIdentityService.GetDisplayName(team),
            Position = player.Position,
            JerseyNumber = player.JerseyNumber,
            IsGoalie = isGoalie,
            IsPulledGoalieReplacement = isPulledGoalieReplacement,
            IsHomeTeam = isHome,
            NormalizedX = Mathf.Clamp01(x + jitter),
            NormalizedY = Mathf.Clamp01(y - jitter),
            JerseyResourcePath = identity == null ? "" : isHome ? identity.HomeJerseyResourcePath : identity.AwayJerseyResourcePath,
            FullBodyResourcePath = identity == null ? "" : identity.FullBodyResourcePath,
            IsOnIce = true,
            IsInjured = player.IsInjured,
            Condition = player.Condition,
            Morale = player.Morale
        });
    }

    private static LiveMatchTeamStatsData GetStats(LiveMatchStateData match, TeamData team)
    {
        if (match == null || team == null)
        {
            return null;
        }

        return match.HomeTeamId == team.Id ? match.HomeStats : match.AwayTeamId == team.Id ? match.AwayStats : null;
    }
}
