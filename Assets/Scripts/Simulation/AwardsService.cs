using System;
using System.Collections.Generic;

public static class AwardsService
{
    public static SeasonAwardsData GenerateSeasonAwards(GameState state)
    {
        SeasonAwardsData existing = state == null ? null : state.LastSeasonAwards;
        if (existing != null && existing.SeasonStartYear == state.CurrentSeasonStartYear)
        {
            existing.EnsureAwards();
            return existing;
        }

        SeasonAwardsData awards = new SeasonAwardsData
        {
            SeasonStartYear = state == null ? 0 : state.CurrentSeasonStartYear,
            SeasonEndYear = state == null ? 0 : state.CurrentSeasonEndYear,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        AddAward(awards, SelectTopScorer(state));
        AddAward(awards, SelectLeagueMvp(state));
        AddAward(awards, SelectBestForward(state));
        AddAward(awards, SelectBestDefenseman(state));
        AddAward(awards, SelectBestGoalie(state));
        AddAward(awards, SelectBestRookie(state));
        AddAward(awards, SelectBestCoach(state));
        AddAward(awards, SelectPlayoffMvp(state));

        foreach (AwardWinnerData award in awards.Awards)
        {
            PlayerData player = FindPlayerById(state, award.PlayerId);
            if (player == null)
            {
                continue;
            }

            CareerStatsService.EnsureCareerStats(player);
            if (!player.CareerAwardIds.Contains(award.AwardId))
            {
                player.CareerAwardIds.Add(award.AwardId);
                player.CareerAwardsCount++;
                player.CareerSummary = CareerStatsService.BuildCareerSummary(player);
            }
        }

        if (state != null)
        {
            state.LastSeasonAwards = awards;
        }

        return awards;
    }

    public static AwardWinnerData SelectLeagueMvp(GameState state)
    {
        return SelectBestSkaterAward(
            state,
            AwardsConfig.LeagueMvp,
            "League MVP",
            "Best all-around skater by production and team success.",
            null);
    }

    public static AwardWinnerData SelectBestForward(GameState state)
    {
        return SelectBestSkaterAward(
            state,
            AwardsConfig.BestForward,
            "Best Forward",
            "Top forward by scoring, role, and overall rating.",
            IsForward);
    }

    public static AwardWinnerData SelectBestDefenseman(GameState state)
    {
        return SelectBestSkaterAward(
            state,
            AwardsConfig.BestDefenseman,
            "Best Defenseman",
            "Top defenseman by scoring and overall impact.",
            IsDefenseman);
    }

    public static AwardWinnerData SelectBestGoalie(GameState state)
    {
        PlayerData bestPlayer = null;
        PlayerSeasonStatsData bestStats = null;
        int bestScore = int.MinValue;

        foreach (PlayerData player in GetAllPlayers(state))
        {
            if (player == null || player.Position != "G")
            {
                continue;
            }

            PlayerSeasonStatsData stats = FindSeasonStats(state, player.Id);
            int score = CalculateGoalieAwardScore(state, player, stats);
            if (score > bestScore)
            {
                bestScore = score;
                bestPlayer = player;
                bestStats = stats;
            }
        }

        return bestPlayer == null
            ? null
            : CreateAward(state, AwardsConfig.BestGoalie, bestPlayer, bestScore, "Best goalie by wins, shutouts, and overall.");
    }

    public static AwardWinnerData SelectBestRookie(GameState state)
    {
        PlayerData bestPlayer = null;
        int bestScore = int.MinValue;

        foreach (PlayerData player in GetAllPlayers(state))
        {
            if (player == null || player.Position == "G")
            {
                continue;
            }

            bool rookieEligible = player.Age <= 21 || player.IsEntryLevelContract;
            if (!rookieEligible && player.Age > 23)
            {
                continue;
            }

            PlayerSeasonStatsData stats = FindSeasonStats(state, player.Id);
            int points = stats == null ? 0 : stats.Points;
            int goals = stats == null ? 0 : stats.Goals;
            int score = points * 3 + goals * 2 + player.Potential + player.Overall + (rookieEligible ? 25 : 0);
            if (score > bestScore)
            {
                bestScore = score;
                bestPlayer = player;
            }
        }

        return bestPlayer == null
            ? null
            : CreateAward(state, AwardsConfig.BestRookie, bestPlayer, bestScore, "Best young player by production and potential.");
    }

    public static AwardWinnerData SelectTopScorer(GameState state)
    {
        PlayerSeasonStatsData topStats = null;
        if (state != null && state.Season != null && state.Season.PlayerStats != null)
        {
            foreach (PlayerSeasonStatsData stats in state.Season.PlayerStats)
            {
                if (stats == null || stats.IsGoalie)
                {
                    continue;
                }

                if (topStats == null || stats.Points > topStats.Points)
                {
                    topStats = stats;
                }
            }
        }

        PlayerData topPlayer = topStats == null ? FindBestOverallSkater(state) : FindPlayerById(state, topStats.PlayerId);
        int score = topStats == null ? (topPlayer == null ? 0 : topPlayer.Overall) : topStats.Points;
        return topPlayer == null
            ? null
            : CreateAward(state, AwardsConfig.TopScorer, topPlayer, score, "Led the league in points.");
    }

    public static AwardWinnerData SelectBestCoach(GameState state)
    {
        TeamData team = LeagueHistoryService.FindBestRegularSeasonTeam(state);
        if (team == null || team.Staff == null || team.Staff.HeadCoach == null)
        {
            return null;
        }

        StaffData coach = team.Staff.HeadCoach;
        TeamStandingData standing = FindStanding(state, team);
        int score = coach.Overall * 2 + (standing == null ? 0 : standing.Points);
        return new AwardWinnerData
        {
            AwardId = Guid.NewGuid().ToString("N"),
            AwardType = AwardsConfig.BestCoach,
            AwardName = AwardsConfig.GetAwardName(AwardsConfig.BestCoach),
            SeasonStartYear = state == null ? 0 : state.CurrentSeasonStartYear,
            SeasonEndYear = state == null ? 0 : state.CurrentSeasonEndYear,
            PlayerId = coach.StaffId,
            PlayerName = coach.FullName,
            TeamId = team.Id,
            TeamName = GetTeamName(team),
            Position = "Coach",
            Age = coach.Age,
            Overall = coach.Overall,
            AwardScore = score,
            Reason = "Coach of the best regular season team.",
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    public static AwardWinnerData SelectPlayoffMvp(GameState state)
    {
        TeamData champion = LeagueHistoryService.FindChampion(state);
        if (champion == null)
        {
            return null;
        }

        PlayerData bestPlayer = null;
        int bestScore = int.MinValue;
        champion.EnsurePlayers();
        foreach (PlayerData player in champion.Players)
        {
            if (player == null || !RosterStatusConfig.IsNhlRoster(player))
            {
                continue;
            }

            PlayerSeasonStatsData stats = FindSeasonStats(state, player.Id);
            int score = player.Position == "G"
                ? CalculateGoalieAwardScore(state, player, stats)
                : CalculateSkaterAwardScore(state, player, stats);
            if (score > bestScore)
            {
                bestScore = score;
                bestPlayer = player;
            }
        }

        return bestPlayer == null
            ? null
            : CreateAward(state, AwardsConfig.PlayoffMvp, bestPlayer, bestScore, "Best player on the championship team.");
    }

    public static List<PlayerData> GetAllPlayers(GameState state)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (state == null || state.Teams == null)
        {
            return players;
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
                if (player != null)
                {
                    players.Add(player);
                }
            }
        }

        return players;
    }

    public static PlayerSeasonStatsData FindSeasonStats(GameState state, string playerId)
    {
        if (state == null || state.Season == null || state.Season.PlayerStats == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (PlayerSeasonStatsData stats in state.Season.PlayerStats)
        {
            if (stats != null && stats.PlayerId == playerId)
            {
                return stats;
            }
        }

        return null;
    }

    public static int CalculateSkaterAwardScore(GameState state, PlayerData player, PlayerSeasonStatsData stats)
    {
        if (player == null)
        {
            return 0;
        }

        int points = stats == null ? 0 : stats.Points;
        int goals = stats == null ? 0 : stats.Goals;
        TeamData team = FindTeamByPlayer(state, player.Id);
        TeamStandingData standing = FindStanding(state, team);
        int teamBonus = standing == null ? 0 : standing.Points / 2;
        int roleBonus = player.IsCaptain ? 12 : player.IsAlternateCaptain ? 6 : 0;
        return points * 3 + goals * 2 + player.Overall + teamBonus + roleBonus;
    }

    public static int CalculateGoalieAwardScore(GameState state, PlayerData player, PlayerSeasonStatsData stats)
    {
        if (player == null)
        {
            return 0;
        }

        int wins = stats == null ? 0 : stats.GoalieWins;
        int shutouts = stats == null ? 0 : stats.Shutouts;
        int goalsAgainst = stats == null ? 0 : stats.GoalsAgainst;
        return wins * 4 + shutouts * 8 + player.Overall * 2 - goalsAgainst / 10;
    }

    public static AwardWinnerData CreateAward(
        GameState state,
        string awardType,
        PlayerData player,
        int awardScore,
        string reason)
    {
        if (player == null || !AwardsConfig.IsValidAwardType(awardType))
        {
            return null;
        }

        PlayerSeasonStatsData stats = FindSeasonStats(state, player.Id);
        TeamData team = FindTeamByPlayer(state, player.Id);
        return new AwardWinnerData
        {
            AwardId = Guid.NewGuid().ToString("N"),
            AwardType = awardType,
            AwardName = AwardsConfig.GetAwardName(awardType),
            SeasonStartYear = state == null ? 0 : state.CurrentSeasonStartYear,
            SeasonEndYear = state == null ? 0 : state.CurrentSeasonEndYear,
            PlayerId = player.Id,
            PlayerName = player.FirstName + " " + player.LastName,
            TeamId = team == null ? player.TeamId : team.Id,
            TeamName = team == null ? "" : GetTeamName(team),
            Position = player.Position,
            Age = player.Age,
            Overall = player.Overall,
            Goals = stats == null ? 0 : stats.Goals,
            Assists = stats == null ? 0 : stats.Assists,
            Points = stats == null ? 0 : stats.Points,
            Wins = stats == null ? 0 : stats.GoalieWins,
            Shutouts = stats == null ? 0 : stats.Shutouts,
            AwardScore = awardScore,
            Reason = reason,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    private static AwardWinnerData SelectBestSkaterAward(
        GameState state,
        string awardType,
        string fallbackReason,
        string reason,
        Func<PlayerData, bool> filter)
    {
        PlayerData bestPlayer = null;
        int bestScore = int.MinValue;

        foreach (PlayerData player in GetAllPlayers(state))
        {
            if (player == null || player.Position == "G")
            {
                continue;
            }

            if (filter != null && !filter(player))
            {
                continue;
            }

            PlayerSeasonStatsData stats = FindSeasonStats(state, player.Id);
            int score = awardType == AwardsConfig.BestDefenseman
                ? CalculateDefensemanScore(state, player, stats)
                : CalculateSkaterAwardScore(state, player, stats);
            if (score > bestScore)
            {
                bestScore = score;
                bestPlayer = player;
            }
        }

        return bestPlayer == null ? null : CreateAward(state, awardType, bestPlayer, bestScore, reason ?? fallbackReason);
    }

    private static int CalculateDefensemanScore(GameState state, PlayerData player, PlayerSeasonStatsData stats)
    {
        int points = stats == null ? 0 : stats.Points;
        int goals = stats == null ? 0 : stats.Goals;
        TeamData team = FindTeamByPlayer(state, player == null ? "" : player.Id);
        TeamStandingData standing = FindStanding(state, team);
        int teamBonus = standing == null ? 0 : Math.Max(0, standing.GoalsFor - standing.GoalsAgainst) / 5;
        return points * 3 + goals + (player == null ? 0 : player.Overall * 2) + teamBonus;
    }

    private static PlayerData FindBestOverallSkater(GameState state)
    {
        PlayerData bestPlayer = null;
        foreach (PlayerData player in GetAllPlayers(state))
        {
            if (player == null || player.Position == "G")
            {
                continue;
            }

            if (bestPlayer == null || player.Overall > bestPlayer.Overall)
            {
                bestPlayer = player;
            }
        }

        return bestPlayer;
    }

    private static void AddAward(SeasonAwardsData awards, AwardWinnerData award)
    {
        if (awards != null && award != null)
        {
            awards.EnsureAwards();
            awards.Awards.Add(award);
        }
    }

    private static PlayerData FindPlayerById(GameState state, string playerId)
    {
        foreach (PlayerData player in CareerStatsService.GetAllPlayersIncludingFreeAgents(state))
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static TeamData FindTeamByPlayer(GameState state, string playerId)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(playerId))
        {
            return null;
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
                if (player != null && player.Id == playerId)
                {
                    return team;
                }
            }
        }

        return null;
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

    private static bool IsForward(PlayerData player)
    {
        return player != null && (player.Position == "C" || player.Position == "LW" || player.Position == "RW");
    }

    private static bool IsDefenseman(PlayerData player)
    {
        return player != null && player.Position == "D";
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
