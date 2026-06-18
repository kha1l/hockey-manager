using System.Collections.Generic;

public static class AlphaBalanceSnapshotService
{
    public static AlphaBalanceSeasonSnapshotData CreateSnapshot(GameState state)
    {
        AlphaBalanceSeasonSnapshotData snapshot = new AlphaBalanceSeasonSnapshotData
        {
            SeasonStartYear = state == null ? 0 : state.CurrentSeasonStartYear,
            SeasonEndYear = state == null ? 0 : state.CurrentSeasonEndYear,
            GamesPlayed = CountGamesPlayed(state),
            TotalGoals = CountTotalGoals(state),
            AverageGoalsPerGameTimes100 = CalculateAverageGoalsPerGameTimes100(state),
            AverageTeamPoints = CalculateAverageTeamPoints(state),
            HighestTeamPoints = CalculateHighestTeamPoints(state),
            LowestTeamPoints = CalculateLowestTeamPoints(state),
            InvalidRosterTeams = CountInvalidRosterTeams(state),
            InvalidLineupTeams = CountInvalidLineupTeams(state),
            CapViolationTeams = CountCapViolationTeams(state),
            AveragePayroll = CalculateAveragePayroll(state),
            AverageCapSpace = CalculateAverageCapSpace(state),
            FreeAgentsCount = CountFreeAgents(state),
            WaiverPlayersCount = CountWaiverPlayers(state),
            InjuredPlayersCount = CountInjuredPlayers(state),
            MajorInjuriesCount = CountMajorInjuries(state),
            AverageMorale = CalculateAverageMorale(state),
            AverageChemistry = CalculateAverageChemistry(state),
            PlayersOverall90Plus = CountPlayersOverallAtLeast(state, 90),
            PlayersOverall95Plus = CountPlayersOverallAtLeast(state, 95),
            DraftClassSize = GetDraftClassSize(state),
            DraftClassSummary = GetDraftClassSummary(state),
            NewsCount = CountNewsItems(state),
            LeagueHistorySeasonsCount = CountLeagueHistorySeasons(state),
            RetiredPlayersCount = CountRetiredPlayers(state),
            HallOfFameCount = CountHallOfFameInductees(state),
            GmJobSecurity = state == null || state.GmCareer == null ? 0 : state.GmCareer.CurrentJobSecurity,
            OwnerJobSecurity = GetCurrentOwnerJobSecurity(state)
        };

        snapshot.Summary = "Snapshot: games " + snapshot.GamesPlayed
            + ", goals/game " + FormatTimes100(snapshot.AverageGoalsPerGameTimes100)
            + ", invalid roster/lineup/cap " + snapshot.InvalidRosterTeams
            + "/" + snapshot.InvalidLineupTeams
            + "/" + snapshot.CapViolationTeams + ".";
        return snapshot;
    }

    public static int CountGamesPlayed(GameState state)
    {
        int matchHistoryCount = state == null || state.MatchHistory == null ? 0 : state.MatchHistory.Count;
        int schedulePlayed = 0;
        if (state != null && state.Season != null && state.Season.Schedule != null)
        {
            foreach (ScheduleGameData game in state.Season.Schedule)
            {
                if (game != null && game.IsPlayed)
                {
                    schedulePlayed++;
                }
            }
        }

        return matchHistoryCount > schedulePlayed ? matchHistoryCount : schedulePlayed;
    }

    public static int CountTotalGoals(GameState state)
    {
        int total = 0;
        foreach (MatchResultData result in SafeResults(state))
        {
            total += result.HomeScore + result.AwayScore;
        }

        return total;
    }

    public static int CalculateAverageGoalsPerGameTimes100(GameState state)
    {
        int games = CountGamesPlayed(state);
        return games <= 0 ? 0 : CountTotalGoals(state) * 100 / games;
    }

    public static int CalculateAverageTeamPoints(GameState state)
    {
        int total = 0;
        int count = 0;
        foreach (TeamStandingData standing in SafeStandings(state))
        {
            if (standing == null)
            {
                continue;
            }

            total += standing.Points;
            count++;
        }

        return count == 0 ? 0 : total / count;
    }

    public static int CalculateHighestTeamPoints(GameState state)
    {
        int value = 0;
        foreach (TeamStandingData standing in SafeStandings(state))
        {
            if (standing != null && standing.Points > value)
            {
                value = standing.Points;
            }
        }

        return value;
    }

    public static int CalculateLowestTeamPoints(GameState state)
    {
        int value = -1;
        foreach (TeamStandingData standing in SafeStandings(state))
        {
            if (standing == null)
            {
                continue;
            }

            if (value < 0 || standing.Points < value)
            {
                value = standing.Points;
            }
        }

        return value < 0 ? 0 : value;
    }

    public static int CountInvalidRosterTeams(GameState state)
    {
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            if (!IsRosterPlayable(team))
            {
                count++;
            }
        }

        return count;
    }

    public static int CountInvalidLineupTeams(GameState state)
    {
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            if (!LineupService.ValidateLineup(team, out string _))
            {
                count++;
            }
        }

        return count;
    }

    public static int CountCapViolationTeams(GameState state)
    {
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
            if (finance != null && (finance.IsOverCap || finance.IsBelowFloor))
            {
                count++;
            }
        }

        return count;
    }

    public static int CalculateAveragePayroll(GameState state)
    {
        int total = 0;
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
            total += finance == null ? 0 : finance.Payroll;
            count++;
        }

        return count == 0 ? 0 : total / count;
    }

    public static int CalculateAverageCapSpace(GameState state)
    {
        int total = 0;
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
            total += finance == null ? 0 : finance.CapSpace;
            count++;
        }

        return count == 0 ? 0 : total / count;
    }

    public static int CountFreeAgents(GameState state)
    {
        return state == null || state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null
            ? 0
            : state.FreeAgentPool.FreeAgents.Count;
    }

    public static int CountWaiverPlayers(GameState state)
    {
        return state == null || state.WaiverWire == null || state.WaiverWire.ActiveWaivers == null
            ? 0
            : state.WaiverWire.ActiveWaivers.Count;
    }

    public static int CountInjuredPlayers(GameState state)
    {
        int count = 0;
        foreach (PlayerData player in SafePlayers(state))
        {
            if (player != null && player.IsInjured)
            {
                count++;
            }
        }

        return count;
    }

    public static int CountMajorInjuries(GameState state)
    {
        int count = 0;
        foreach (PlayerData player in SafePlayers(state))
        {
            if (player == null || !player.IsInjured)
            {
                continue;
            }

            if (player.InjurySeverity == "Major" || player.InjurySeverity == "LongTerm")
            {
                count++;
            }
        }

        return count;
    }

    public static int CalculateAverageMorale(GameState state)
    {
        int total = 0;
        int count = 0;
        foreach (PlayerData player in SafePlayers(state))
        {
            if (player != null && player.Morale > 0)
            {
                total += player.Morale;
                count++;
            }
        }

        return count == 0 ? 0 : total / count;
    }

    public static int CalculateAverageChemistry(GameState state)
    {
        int total = 0;
        int count = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            if (team != null && team.Chemistry != null)
            {
                total += team.Chemistry.TeamChemistryScore;
                count++;
            }
        }

        return count == 0 ? 0 : total / count;
    }

    public static int CountPlayersOverallAtLeast(GameState state, int overall)
    {
        int count = 0;
        foreach (PlayerData player in SafePlayers(state))
        {
            if (player != null && !player.IsRetired && player.Overall >= overall)
            {
                count++;
            }
        }

        return count;
    }

    public static int GetDraftClassSize(GameState state)
    {
        return state == null || state.Draft == null || state.Draft.Prospects == null
            ? 0
            : state.Draft.Prospects.Count;
    }

    public static string GetDraftClassSummary(GameState state)
    {
        if (state == null || state.Draft == null)
        {
            return "Draft not initialized";
        }

        string profile = state.Draft.ClassProfile == null
            ? "no profile"
            : DraftClassConfig.BuildClassSummary(state.Draft.ClassProfile);
        return GetDraftClassSize(state) + " prospects | " + profile;
    }

    public static int CountNewsItems(GameState state)
    {
        return state == null || state.NewsFeed == null || state.NewsFeed.Items == null
            ? 0
            : state.NewsFeed.Items.Count;
    }

    public static int CountLeagueHistorySeasons(GameState state)
    {
        return state == null || state.LeagueHistory == null ? 0 : state.LeagueHistory.Count;
    }

    public static int CountRetiredPlayers(GameState state)
    {
        return state == null || state.RetiredPlayers == null || state.RetiredPlayers.Players == null
            ? 0
            : state.RetiredPlayers.Players.Count;
    }

    public static int CountHallOfFameInductees(GameState state)
    {
        return state == null || state.HallOfFame == null || state.HallOfFame.Inductees == null
            ? 0
            : state.HallOfFame.Inductees.Count;
    }

    private static bool IsRosterPlayable(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return false;
        }

        int nhl = 0;
        int forwards = 0;
        int defense = 0;
        int goalies = 0;
        int healthyForwards = 0;
        int healthyDefense = 0;
        int healthyGoalies = 0;

        foreach (PlayerData player in team.Players)
        {
            if (player == null || player.RosterStatus != RosterStatusConfig.NHL)
            {
                continue;
            }

            nhl++;
            bool available = InjuryService.IsPlayerAvailable(player);
            if (IsForward(player))
            {
                forwards++;
                if (available)
                {
                    healthyForwards++;
                }
            }
            else if (player.Position == "D")
            {
                defense++;
                if (available)
                {
                    healthyDefense++;
                }
            }
            else if (player.Position == "G")
            {
                goalies++;
                if (available)
                {
                    healthyGoalies++;
                }
            }
        }

        return nhl >= RosterStatusConfig.MinNhlRosterSize
            && nhl <= RosterStatusConfig.MaxNhlRosterSize
            && forwards >= 12
            && defense >= 6
            && goalies >= 2
            && healthyForwards >= 12
            && healthyDefense >= 6
            && healthyGoalies >= 2;
    }

    private static string GetCurrentOwnerJobSecurity(GameState state)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(state.SelectedTeamId))
        {
            return "";
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == state.SelectedTeamId && team.OwnerProfile != null)
            {
                return team.OwnerProfile.JobSecurity;
            }
        }

        return "";
    }

    private static IEnumerable<MatchResultData> SafeResults(GameState state)
    {
        if (state != null && state.MatchHistory != null && state.MatchHistory.Count > 0)
        {
            return state.MatchHistory;
        }

        List<MatchResultData> results = new List<MatchResultData>();
        if (state == null || state.Season == null || state.Season.Schedule == null)
        {
            return results;
        }

        foreach (ScheduleGameData game in state.Season.Schedule)
        {
            if (game != null && game.IsPlayed && game.Result != null)
            {
                results.Add(game.Result);
            }
        }

        return results;
    }

    private static IEnumerable<TeamStandingData> SafeStandings(GameState state)
    {
        return state == null || state.Season == null || state.Season.Standings == null
            ? new List<TeamStandingData>()
            : state.Season.Standings;
    }

    private static IEnumerable<TeamData> SafeTeams(GameState state)
    {
        return state == null || state.Teams == null ? new List<TeamData>() : state.Teams;
    }

    private static IEnumerable<PlayerData> SafePlayers(GameState state)
    {
        List<PlayerData> players = new List<PlayerData>();
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            players.AddRange(team.Players);
        }

        if (state != null && state.FreeAgentPool != null && state.FreeAgentPool.FreeAgents != null)
        {
            players.AddRange(state.FreeAgentPool.FreeAgents);
        }

        return players;
    }

    private static bool IsForward(PlayerData player)
    {
        return player != null && (player.Position == "C" || player.Position == "LW" || player.Position == "RW");
    }

    private static string FormatTimes100(int value)
    {
        return (value / 100) + "." + (value % 100).ToString("D2");
    }
}
