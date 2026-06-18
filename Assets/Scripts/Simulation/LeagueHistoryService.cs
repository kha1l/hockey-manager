using System;
using System.Collections.Generic;

public static class LeagueHistoryService
{
    public static void EnsureLeagueHistory(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureLeagueHistory();
        LeagueRecordsService.EnsureLeagueRecords(state);
    }

    public static LeagueSeasonHistoryData CreateSeasonHistory(GameState state)
    {
        EnsureLeagueHistory(state);
        if (state == null)
        {
            return null;
        }

        LeagueSeasonHistoryData existing = FindSeasonHistory(state, state.CurrentSeasonStartYear);
        if (existing != null)
        {
            return existing;
        }

        TeamData champion = FindChampion(state);
        TeamData finalist = FindFinalist(state);
        TeamData bestRegularSeasonTeam = FindBestRegularSeasonTeam(state);
        PlayerData topScorer = FindTopScorer(state, out int topScorerPoints);
        SeasonAwardsData awards = state.LastSeasonAwards;
        if (awards == null || awards.SeasonStartYear != state.CurrentSeasonStartYear)
        {
            awards = AwardsService.GenerateSeasonAwards(state);
        }

        AwardWinnerData mvp = FindAward(awards, AwardsConfig.LeagueMvp);
        AwardWinnerData bestGoalie = FindAward(awards, AwardsConfig.BestGoalie);
        TeamData userTeam = FindTeamById(state, state.SelectedTeamId);

        LeagueSeasonHistoryData history = new LeagueSeasonHistoryData
        {
            HistoryId = "league-history-" + state.CurrentSeasonStartYear,
            SeasonStartYear = state.CurrentSeasonStartYear,
            SeasonEndYear = state.CurrentSeasonEndYear,
            ChampionTeamId = champion == null ? "" : champion.Id,
            ChampionTeamName = champion == null ? "" : GetTeamName(champion),
            FinalistTeamId = finalist == null ? "" : finalist.Id,
            FinalistTeamName = finalist == null ? "" : GetTeamName(finalist),
            BestRegularSeasonTeamId = bestRegularSeasonTeam == null ? "" : bestRegularSeasonTeam.Id,
            BestRegularSeasonTeamName = bestRegularSeasonTeam == null ? "" : GetTeamName(bestRegularSeasonTeam),
            BestRegularSeasonPoints = GetPoints(state, bestRegularSeasonTeam),
            TopScorerPlayerId = topScorer == null ? "" : topScorer.Id,
            TopScorerPlayerName = topScorer == null ? "" : topScorer.FirstName + " " + topScorer.LastName,
            TopScorerTeamName = topScorer == null ? "" : GetTeamName(FindTeamByPlayer(state, topScorer.Id)),
            TopScorerPoints = topScorerPoints,
            MvpPlayerId = mvp == null ? "" : mvp.PlayerId,
            MvpPlayerName = mvp == null ? "" : mvp.PlayerName,
            BestGoaliePlayerId = bestGoalie == null ? "" : bestGoalie.PlayerId,
            BestGoaliePlayerName = bestGoalie == null ? "" : bestGoalie.PlayerName,
            UserTeamId = userTeam == null ? "" : userTeam.Id,
            UserTeamName = userTeam == null ? "" : GetTeamName(userTeam),
            UserTeamPoints = GetPoints(state, userTeam),
            UserTeamMadePlayoffs = OwnerGoalProgressService.DidTeamMakePlayoffs(state, userTeam),
            UserTeamPlayoffRoundsWon = OwnerGoalProgressService.GetPlayoffRoundsWon(state, userTeam),
            UserTeamResult = BuildUserTeamResult(state, userTeam),
            Awards = awards,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        history.Summary = BuildSeasonSummary(history);
        return history;
    }

    public static UserTeamSeasonHistoryData CreateUserTeamHistory(GameState state, TeamData userTeam)
    {
        if (state == null || userTeam == null)
        {
            return null;
        }

        TeamStandingData standing = FindStanding(state, userTeam);
        OwnerSeasonEvaluationData ownerEvaluation = userTeam.OwnerProfile == null
            ? null
            : userTeam.OwnerProfile.LastSeasonEvaluation;
        UserTeamSeasonHistoryData history = new UserTeamSeasonHistoryData
        {
            SeasonStartYear = state.CurrentSeasonStartYear,
            SeasonEndYear = state.CurrentSeasonEndYear,
            TeamId = userTeam.Id,
            TeamName = GetTeamName(userTeam),
            Wins = standing == null ? 0 : standing.Wins,
            Losses = standing == null ? 0 : standing.Losses,
            OvertimeLosses = standing == null ? 0 : standing.OvertimeLosses,
            Points = standing == null ? 0 : standing.Points,
            LeagueRank = OwnerGoalProgressService.GetLeagueRank(state, userTeam),
            MadePlayoffs = OwnerGoalProgressService.DidTeamMakePlayoffs(state, userTeam),
            PlayoffRoundsWon = OwnerGoalProgressService.GetPlayoffRoundsWon(state, userTeam),
            PlayoffResult = BuildUserTeamResult(state, userTeam),
            OwnerEvaluationSummary = ownerEvaluation == null ? "" : ownerEvaluation.EvaluationSummary,
            GmTrustAfterSeason = userTeam.OwnerProfile == null ? 0 : userTeam.OwnerProfile.GmTrust,
            JobSecurityAfterSeason = userTeam.OwnerProfile == null ? "" : userTeam.OwnerProfile.JobSecurity,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        history.Summary = history.TeamName + ": " + history.Wins + "-"
            + history.Losses + "-" + history.OvertimeLosses
            + ", " + history.Points + " points, " + history.PlayoffResult;
        return history;
    }

    public static void StoreSeasonHistory(GameState state, LeagueSeasonHistoryData history)
    {
        EnsureLeagueHistory(state);
        if (state == null || history == null)
        {
            return;
        }

        RemoveLeagueHistoryForSeason(state, history.SeasonStartYear);
        history.EnsureAwards();
        state.LeagueHistory.Add(history);
        state.LastLeagueSeasonHistory = history;
        state.LastSeasonAwards = history.Awards;
        state.LastHistoryUpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    public static void StoreUserTeamHistory(GameState state, UserTeamSeasonHistoryData history)
    {
        EnsureLeagueHistory(state);
        if (state == null || history == null)
        {
            return;
        }

        for (int i = state.UserTeamHistory.Count - 1; i >= 0; i--)
        {
            UserTeamSeasonHistoryData existing = state.UserTeamHistory[i];
            if (existing != null && existing.SeasonStartYear == history.SeasonStartYear)
            {
                state.UserTeamHistory.RemoveAt(i);
            }
        }

        state.UserTeamHistory.Add(history);
        state.LastHistoryUpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    public static bool HasSeasonHistory(GameState state, int seasonStartYear)
    {
        return FindSeasonHistory(state, seasonStartYear) != null;
    }

    public static TeamData FindChampion(GameState state)
    {
        if (state == null || state.Season == null || state.Season.Playoffs == null)
        {
            return null;
        }

        return FindTeamById(state, state.Season.Playoffs.ChampionTeamId);
    }

    public static TeamData FindFinalist(GameState state)
    {
        if (state == null || state.Season == null || state.Season.Playoffs == null)
        {
            return null;
        }

        PlayoffSeriesData finalSeries = FindFinalSeries(state.Season.Playoffs);
        if (finalSeries == null)
        {
            return null;
        }

        string finalistId = finalSeries.WinnerTeamId == finalSeries.TeamAId
            ? finalSeries.TeamBId
            : finalSeries.TeamAId;
        return FindTeamById(state, finalistId);
    }

    public static TeamData FindBestRegularSeasonTeam(GameState state)
    {
        if (state == null || state.Season == null)
        {
            return null;
        }

        List<TeamStandingData> standings = StandingsService.GetSortedStandings(state.Season);
        return standings.Count == 0 ? null : FindTeamById(state, standings[0].TeamId);
    }

    public static PlayerData FindTopScorer(GameState state, out int points)
    {
        points = 0;
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

        points = topStats == null ? 0 : topStats.Points;
        return topStats == null ? null : FindPlayerById(state, topStats.PlayerId);
    }

    public static string BuildSeasonSummary(LeagueSeasonHistoryData history)
    {
        if (history == null)
        {
            return "Season history unavailable.";
        }

        string champion = string.IsNullOrEmpty(history.ChampionTeamName) ? "No champion" : history.ChampionTeamName;
        string finalist = string.IsNullOrEmpty(history.FinalistTeamName) ? "unknown finalist" : history.FinalistTeamName;
        string topScorer = string.IsNullOrEmpty(history.TopScorerPlayerName)
            ? "no top scorer"
            : history.TopScorerPlayerName + " led with " + history.TopScorerPoints + " points";
        return history.SeasonStartYear + "-" + (history.SeasonEndYear % 100).ToString("D2")
            + ": " + champion + " won the championship over " + finalist + ". " + topScorer + ".";
    }

    public static string BuildUserTeamResult(GameState state, TeamData userTeam)
    {
        if (userTeam == null)
        {
            return "Команда пользователя не найдена";
        }

        if (state != null
            && state.Season != null
            && state.Season.Playoffs != null
            && state.Season.Playoffs.ChampionTeamId == userTeam.Id)
        {
            return "Champion";
        }

        int roundsWon = OwnerGoalProgressService.GetPlayoffRoundsWon(state, userTeam);
        if (roundsWon > 0)
        {
            return "Won " + roundsWon + " playoff round(s)";
        }

        return OwnerGoalProgressService.DidTeamMakePlayoffs(state, userTeam)
            ? "Lost in Round 1"
            : "Missed playoffs";
    }

    private static LeagueSeasonHistoryData FindSeasonHistory(GameState state, int seasonStartYear)
    {
        if (state == null || state.LeagueHistory == null)
        {
            return null;
        }

        foreach (LeagueSeasonHistoryData history in state.LeagueHistory)
        {
            if (history != null && history.SeasonStartYear == seasonStartYear)
            {
                return history;
            }
        }

        return null;
    }

    private static void RemoveLeagueHistoryForSeason(GameState state, int seasonStartYear)
    {
        if (state == null || state.LeagueHistory == null)
        {
            return;
        }

        for (int i = state.LeagueHistory.Count - 1; i >= 0; i--)
        {
            LeagueSeasonHistoryData existing = state.LeagueHistory[i];
            if (existing != null && existing.SeasonStartYear == seasonStartYear)
            {
                state.LeagueHistory.RemoveAt(i);
            }
        }
    }

    private static AwardWinnerData FindAward(SeasonAwardsData awards, string awardType)
    {
        if (awards == null || awards.Awards == null)
        {
            return null;
        }

        foreach (AwardWinnerData award in awards.Awards)
        {
            if (award != null && award.AwardType == awardType)
            {
                return award;
            }
        }

        return null;
    }

    private static PlayoffSeriesData FindFinalSeries(PlayoffData playoffs)
    {
        if (playoffs == null || playoffs.Rounds == null)
        {
            return null;
        }

        PlayoffSeriesData finalSeries = null;
        foreach (PlayoffRoundData round in playoffs.Rounds)
        {
            if (round == null || round.Series == null)
            {
                continue;
            }

            foreach (PlayoffSeriesData series in round.Series)
            {
                if (series != null && series.IsCompleted && !string.IsNullOrEmpty(series.WinnerTeamId))
                {
                    if (finalSeries == null || series.RoundNumber > finalSeries.RoundNumber)
                    {
                        finalSeries = series;
                    }
                }
            }
        }

        return finalSeries;
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

    private static int GetPoints(GameState state, TeamData team)
    {
        TeamStandingData standing = FindStanding(state, team);
        return standing == null ? 0 : standing.Points;
    }

    private static TeamData FindTeamById(GameState state, string teamId)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
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

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
