using System.Collections.Generic;

public static class CareerStatsService
{
    public static void UpdateCareerStatsAfterSeason(GameState state)
    {
        if (state == null || state.Season == null)
        {
            return;
        }

        foreach (PlayerData player in GetAllPlayersIncludingFreeAgents(state))
        {
            if (player == null)
            {
                continue;
            }

            EnsureCareerStats(player);
            if (player.LastCareerStatsUpdatedSeasonStartYear == state.CurrentSeasonStartYear)
            {
                continue;
            }

            PlayerSeasonStatsData seasonStats = AwardsService.FindSeasonStats(state, player.Id);
            if (seasonStats == null)
            {
                continue;
            }

            UpdatePlayerCareerStats(state, player, seasonStats);
            player.LastCareerStatsUpdatedSeasonStartYear = state.CurrentSeasonStartYear;
        }
    }

    public static void UpdatePlayerCareerStats(GameState state, PlayerData player, PlayerSeasonStatsData seasonStats)
    {
        if (player == null || seasonStats == null)
        {
            return;
        }

        EnsureCareerStats(player);
        player.CareerGamesPlayed += seasonStats.IsGoalie ? seasonStats.GoalieGamesPlayed : seasonStats.GamesPlayed;
        player.CareerGoals += seasonStats.Goals;
        player.CareerAssists += seasonStats.Assists;
        player.CareerPoints += seasonStats.Points;
        player.CareerShots += seasonStats.Shots;
        player.CareerPenaltyMinutes += seasonStats.PenaltyMinutes;
        player.CareerPowerPlayGoals += seasonStats.PowerPlayGoals;
        player.CareerPowerPlayPoints += seasonStats.PowerPlayPoints;
        player.CareerShortHandedPoints += seasonStats.ShortHandedPoints;
        player.CareerWins += seasonStats.GoalieWins;
        player.CareerLosses += seasonStats.GoalieLosses;
        player.CareerOvertimeLosses += seasonStats.GoalieOvertimeLosses;
        player.CareerShutouts += seasonStats.Shutouts;
        player.CareerGoalsAgainst += seasonStats.GoalsAgainst;
        player.CareerSaves += seasonStats.Saves;
        UpdateBestSeason(player, seasonStats, state.CurrentSeasonStartYear, state.CurrentSeasonEndYear);
        player.CareerSummary = BuildCareerSummary(player);
    }

    public static void EnsureCareerStats(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        if (player.CareerAwardIds == null)
        {
            player.CareerAwardIds = new List<string>();
        }

        player.CareerGamesPlayed = ClampNonNegative(player.CareerGamesPlayed);
        player.CareerGoals = ClampNonNegative(player.CareerGoals);
        player.CareerAssists = ClampNonNegative(player.CareerAssists);
        player.CareerPoints = ClampNonNegative(player.CareerPoints);
        player.CareerShots = ClampNonNegative(player.CareerShots);
        player.CareerPenaltyMinutes = ClampNonNegative(player.CareerPenaltyMinutes);
        player.CareerPowerPlayGoals = ClampNonNegative(player.CareerPowerPlayGoals);
        player.CareerPowerPlayPoints = ClampNonNegative(player.CareerPowerPlayPoints);
        player.CareerShortHandedPoints = ClampNonNegative(player.CareerShortHandedPoints);
        player.CareerWins = ClampNonNegative(player.CareerWins);
        player.CareerLosses = ClampNonNegative(player.CareerLosses);
        player.CareerOvertimeLosses = ClampNonNegative(player.CareerOvertimeLosses);
        player.CareerShutouts = ClampNonNegative(player.CareerShutouts);
        player.CareerGoalsAgainst = ClampNonNegative(player.CareerGoalsAgainst);
        player.CareerSaves = ClampNonNegative(player.CareerSaves);
        player.CareerAwardsCount = ClampNonNegative(player.CareerAwardsCount);

        if (string.IsNullOrEmpty(player.CareerSummary))
        {
            player.CareerSummary = BuildCareerSummary(player);
        }
    }

    public static void UpdateBestSeason(PlayerData player, PlayerSeasonStatsData seasonStats, int seasonStartYear, int seasonEndYear)
    {
        if (player == null || seasonStats == null)
        {
            return;
        }

        string seasonLabel = seasonStartYear + "-" + (seasonEndYear % 100).ToString("D2");
        if (seasonStats.IsGoalie || player.Position == "G")
        {
            if (seasonStats.GoalieWins > player.BestSeasonWins)
            {
                player.BestSeasonWins = seasonStats.GoalieWins;
                player.BestSeasonLabel = seasonLabel + ": " + seasonStats.GoalieWins + "W " + seasonStats.Shutouts + "SO";
            }

            return;
        }

        if (seasonStats.Points > player.BestSeasonPoints)
        {
            player.BestSeasonGoals = seasonStats.Goals;
            player.BestSeasonAssists = seasonStats.Assists;
            player.BestSeasonPoints = seasonStats.Points;
            player.BestSeasonLabel = seasonLabel + ": " + seasonStats.Goals + "G " + seasonStats.Assists + "A " + seasonStats.Points + "P";
        }
    }

    public static string BuildCareerSummary(PlayerData player)
    {
        if (player == null)
        {
            return "";
        }

        if (player.Position == "G")
        {
            return "Career: " + player.CareerGamesPlayed + " GP, "
                + player.CareerWins + "W, "
                + player.CareerShutouts + "SO, "
                + player.CareerAwardsCount + " awards";
        }

        return "Career: " + player.CareerGamesPlayed + " GP, "
            + player.CareerGoals + "G, "
            + player.CareerAssists + "A, "
            + player.CareerPoints + "P, "
            + player.CareerAwardsCount + " awards";
    }

    public static List<PlayerData> GetAllPlayersIncludingFreeAgents(GameState state)
    {
        List<PlayerData> players = new List<PlayerData>();
        HashSet<string> seenIds = new HashSet<string>();
        if (state == null)
        {
            return players;
        }

        if (state.Teams != null)
        {
            foreach (TeamData team in state.Teams)
            {
                if (team == null)
                {
                    continue;
                }

                team.EnsurePlayers();
                foreach (PlayerData player in team.Players)
                {
                    AddUnique(players, seenIds, player);
                }
            }
        }

        if (state.FreeAgentPool != null)
        {
            state.FreeAgentPool.EnsureFreeAgents();
            foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
            {
                AddUnique(players, seenIds, player);
            }
        }

        return players;
    }

    private static void AddUnique(List<PlayerData> players, HashSet<string> seenIds, PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        string key = string.IsNullOrEmpty(player.Id) ? player.FirstName + "|" + player.LastName : player.Id;
        if (seenIds.Contains(key))
        {
            return;
        }

        seenIds.Add(key);
        players.Add(player);
    }

    private static int ClampNonNegative(int value)
    {
        return value < 0 ? 0 : value;
    }
}
