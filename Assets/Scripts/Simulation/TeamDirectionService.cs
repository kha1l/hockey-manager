using System.Collections.Generic;

public static class TeamDirectionService
{
    public static string DetermineTeamDirection(GameState state, TeamData team)
    {
        if (team == null)
        {
            return TradeAiConfig.DirectionRebuild;
        }

        TeamRosterService.EnsureRosterStatusesForTeam(team);
        int overall = TeamRatingCalculator.CalculateEffectiveLineupOverall(team);
        if (overall <= 0)
        {
            overall = TeamRatingCalculator.CalculateOverall(team);
        }

        TeamStandingData standing = FindStanding(state, team);
        if (standing != null && standing.GamesPlayed > 0)
        {
            int pointsPercentage = standing.Points * 100 / (standing.GamesPlayed * 2);
            if (pointsPercentage >= 62 || overall >= 82)
            {
                return TradeAiConfig.DirectionContender;
            }

            if (pointsPercentage >= 55 || overall >= 79)
            {
                return TradeAiConfig.DirectionPlayoffTeam;
            }

            if (pointsPercentage >= 48 || overall >= 76)
            {
                return TradeAiConfig.DirectionBubbleTeam;
            }
        }

        if (overall >= 82)
        {
            return TradeAiConfig.DirectionContender;
        }

        if (overall >= 79)
        {
            return TradeAiConfig.DirectionPlayoffTeam;
        }

        if (overall >= 76)
        {
            return TradeAiConfig.DirectionBubbleTeam;
        }

        int averageAge = CalculateAverageRosterAge(team);
        int youngCoreScore = CalculateYoungCoreScore(team);
        if (overall >= 73 || (averageAge >= 29 && youngCoreScore < 4))
        {
            return TradeAiConfig.DirectionRetool;
        }

        return TradeAiConfig.DirectionRebuild;
    }

    public static int CalculateBuyerScore(GameState state, TeamData team)
    {
        string direction = DetermineTeamDirection(state, team);
        int score = 15;
        if (direction == TradeAiConfig.DirectionContender)
        {
            score = 90;
        }
        else if (direction == TradeAiConfig.DirectionPlayoffTeam)
        {
            score = 75;
        }
        else if (direction == TradeAiConfig.DirectionBubbleTeam)
        {
            score = 50;
        }
        else if (direction == TradeAiConfig.DirectionRetool)
        {
            score = 35;
        }

        if ((direction == TradeAiConfig.DirectionContender || direction == TradeAiConfig.DirectionPlayoffTeam || direction == TradeAiConfig.DirectionBubbleTeam)
            && CountInjuredPlayers(team) >= 3)
        {
            score += 10;
        }

        if (TeamNeedService.CalculateCapPressureScore(state, team) >= TradeAiConfig.CapPressureHighThreshold)
        {
            score -= 15;
        }

        return TradeAiConfig.ClampScore(score);
    }

    public static int CalculateSellerScore(GameState state, TeamData team)
    {
        string direction = DetermineTeamDirection(state, team);
        int score = 10;
        if (direction == TradeAiConfig.DirectionRebuild)
        {
            score = 90;
        }
        else if (direction == TradeAiConfig.DirectionRetool)
        {
            score = 70;
        }
        else if (direction == TradeAiConfig.DirectionBubbleTeam)
        {
            score = 45;
        }
        else if (direction == TradeAiConfig.DirectionPlayoffTeam)
        {
            score = 25;
        }

        if (CountPendingUfas(team) >= 3)
        {
            score += 15;
        }

        if (TeamNeedService.CalculateCapPressureScore(state, team) >= TradeAiConfig.CapPressureHighThreshold)
        {
            score += 20;
        }

        return TradeAiConfig.ClampScore(score);
    }

    private static TeamStandingData FindStanding(GameState state, TeamData team)
    {
        if (state == null || state.Season == null || state.Season.Standings == null || team == null)
        {
            return null;
        }

        foreach (TeamStandingData standing in state.Season.Standings)
        {
            if (standing != null && standing.TeamId == team.Id)
            {
                return standing;
            }
        }

        return null;
    }

    private static int CalculateAverageRosterAge(TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        team.EnsurePlayers();
        int totalAge = 0;
        int count = 0;
        foreach (PlayerData player in team.Players)
        {
            if (player != null && RosterStatusConfig.IsInOrganization(player))
            {
                totalAge += player.Age;
                count++;
            }
        }

        return count == 0 ? 0 : totalAge / count;
    }

    private static int CalculateYoungCoreScore(TeamData team)
    {
        int score = 0;
        if (team == null)
        {
            return score;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null
                && RosterStatusConfig.IsInOrganization(player)
                && player.Age <= 24
                && player.Potential >= 82)
            {
                score++;
            }
        }

        return score;
    }

    private static int CountInjuredPlayers(TeamData team)
    {
        int count = 0;
        if (team == null)
        {
            return count;
        }

        foreach (PlayerData player in TeamRosterService.GetNhlPlayers(team))
        {
            InjuryService.EnsureInjuryFields(player);
            if (player != null && player.IsInjured)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountPendingUfas(TeamData team)
    {
        int count = 0;
        if (team == null)
        {
            return count;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && TradeAiConfig.IsPendingUfa(player))
            {
                count++;
            }
        }

        return count;
    }
}
