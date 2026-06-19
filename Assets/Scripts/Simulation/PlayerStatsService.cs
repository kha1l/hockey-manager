using System.Collections.Generic;

public static class PlayerStatsService
{
    public static void EnsurePlayerStats(SeasonData season)
    {
        if (season == null)
        {
            return;
        }

        season.EnsureCollections();
    }

    public static void ApplyGameStats(SeasonData season, List<PlayerGameStatData> gameStats)
    {
        if (season == null || gameStats == null)
        {
            return;
        }

        EnsurePlayerStats(season);

        foreach (PlayerGameStatData gameStat in gameStats)
        {
            PlayerSeasonStatsData seasonStats = GetOrCreatePlayerStats(season, gameStat);

            if (!gameStat.IsGoalie)
            {
                seasonStats.GamesPlayed++;
                seasonStats.Goals += gameStat.Goals;
                seasonStats.Assists += gameStat.Assists;
                seasonStats.Points = seasonStats.Goals + seasonStats.Assists;
                seasonStats.PowerPlayGoals += gameStat.PowerPlayGoals;
                seasonStats.PowerPlayAssists += gameStat.PowerPlayAssists;
                seasonStats.PowerPlayPoints = seasonStats.PowerPlayGoals + seasonStats.PowerPlayAssists;
                seasonStats.ShortHandedGoals += gameStat.ShortHandedGoals;
                seasonStats.ShortHandedAssists += gameStat.ShortHandedAssists;
                seasonStats.ShortHandedPoints = seasonStats.ShortHandedGoals + seasonStats.ShortHandedAssists;
                seasonStats.Shots += gameStat.Shots;
                seasonStats.PenaltyMinutes += gameStat.PenaltyMinutes;
                seasonStats.PlusMinus += gameStat.PlusMinus;
                seasonStats.TotalTimeOnIceSeconds += gameStat.TimeOnIceSeconds;
                seasonStats.AverageTimeOnIceSeconds = seasonStats.GamesPlayed > 0
                    ? seasonStats.TotalTimeOnIceSeconds / seasonStats.GamesPlayed
                    : 0;
                continue;
            }

            seasonStats.GoalieGamesPlayed++;

            if (gameStat.GoalieWin)
            {
                seasonStats.GoalieWins++;
            }

            if (gameStat.GoalieLoss)
            {
                seasonStats.GoalieLosses++;
            }

            if (gameStat.GoalieOvertimeLoss)
            {
                seasonStats.GoalieOvertimeLosses++;
            }

            seasonStats.Saves += gameStat.Saves;
            seasonStats.ShotsAgainst += gameStat.ShotsAgainst;
            seasonStats.GoalsAgainst += gameStat.GoalsAgainst;
            seasonStats.TotalTimeOnIceSeconds += gameStat.TimeOnIceSeconds;
            seasonStats.AverageTimeOnIceSeconds = seasonStats.GoalieGamesPlayed > 0
                ? seasonStats.TotalTimeOnIceSeconds / seasonStats.GoalieGamesPlayed
                : 0;

            if (gameStat.Shutout)
            {
                seasonStats.Shutouts++;
            }
        }
    }

    public static List<PlayerSeasonStatsData> GetTeamSkaterStats(SeasonData season, string teamId)
    {
        List<PlayerSeasonStatsData> stats = GetTeamStats(season, teamId, false);
        stats.Sort(CompareSkaters);
        return stats;
    }

    public static List<PlayerSeasonStatsData> GetTeamGoalieStats(SeasonData season, string teamId)
    {
        List<PlayerSeasonStatsData> stats = GetTeamStats(season, teamId, true);
        stats.Sort(CompareGoalies);
        return stats;
    }

    public static List<PlayerSeasonStatsData> GetLeagueSkaterLeaders(SeasonData season, int limit)
    {
        List<PlayerSeasonStatsData> leaders = new List<PlayerSeasonStatsData>();

        if (season == null)
        {
            return leaders;
        }

        EnsurePlayerStats(season);

        foreach (PlayerSeasonStatsData stats in season.PlayerStats)
        {
            if (stats != null && !stats.IsGoalie)
            {
                leaders.Add(stats);
            }
        }

        leaders.Sort(CompareSkaters);

        if (limit > 0 && leaders.Count > limit)
        {
            leaders.RemoveRange(limit, leaders.Count - limit);
        }

        return leaders;
    }

    private static List<PlayerSeasonStatsData> GetTeamStats(SeasonData season, string teamId, bool goalies)
    {
        List<PlayerSeasonStatsData> statsForTeam = new List<PlayerSeasonStatsData>();

        if (season == null)
        {
            return statsForTeam;
        }

        EnsurePlayerStats(season);

        foreach (PlayerSeasonStatsData stats in season.PlayerStats)
        {
            if (stats != null && stats.TeamId == teamId && stats.IsGoalie == goalies)
            {
                statsForTeam.Add(stats);
            }
        }

        return statsForTeam;
    }

    private static PlayerSeasonStatsData GetOrCreatePlayerStats(SeasonData season, PlayerGameStatData gameStat)
    {
        foreach (PlayerSeasonStatsData stats in season.PlayerStats)
        {
            if (stats != null && stats.PlayerId == gameStat.PlayerId)
            {
                return stats;
            }
        }

        PlayerSeasonStatsData newStats = new PlayerSeasonStatsData
        {
            PlayerId = gameStat.PlayerId,
            TeamId = gameStat.TeamId,
            PlayerName = gameStat.PlayerName,
            Position = gameStat.Position,
            IsGoalie = gameStat.IsGoalie
        };

        season.PlayerStats.Add(newStats);
        return newStats;
    }

    private static int CompareSkaters(PlayerSeasonStatsData left, PlayerSeasonStatsData right)
    {
        int pointsComparison = right.Points.CompareTo(left.Points);
        if (pointsComparison != 0)
        {
            return pointsComparison;
        }

        int goalsComparison = right.Goals.CompareTo(left.Goals);
        return goalsComparison != 0 ? goalsComparison : right.Shots.CompareTo(left.Shots);
    }

    private static int CompareGoalies(PlayerSeasonStatsData left, PlayerSeasonStatsData right)
    {
        float rightSavePercentage = GetSavePercentage(right);
        float leftSavePercentage = GetSavePercentage(left);
        int saveComparison = rightSavePercentage.CompareTo(leftSavePercentage);
        if (saveComparison != 0)
        {
            return saveComparison;
        }

        int winsComparison = right.GoalieWins.CompareTo(left.GoalieWins);
        if (winsComparison != 0)
        {
            return winsComparison;
        }

        return GetGoalsAgainstAverage(left).CompareTo(GetGoalsAgainstAverage(right));
    }

    private static float GetSavePercentage(PlayerSeasonStatsData stats)
    {
        return stats == null || stats.ShotsAgainst <= 0 ? 0f : stats.Saves / (float)stats.ShotsAgainst;
    }

    private static float GetGoalsAgainstAverage(PlayerSeasonStatsData stats)
    {
        return stats == null || stats.GoalieGamesPlayed <= 0 ? 99f : stats.GoalsAgainst / (float)stats.GoalieGamesPlayed;
    }
}
