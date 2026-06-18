using System;
using System.Collections.Generic;

public static class PlayoffService
{
    private const string EasternConference = "Eastern";
    private const string WesternConference = "Western";
    private const string StanleyCupConference = "Stanley Cup";

    public static void EnsurePlayoffs(SeasonData season)
    {
        if (season == null || !IsPlayoffAvailable(season))
        {
            return;
        }

        if (season.Playoffs != null && season.Playoffs.IsStarted)
        {
            season.Playoffs.EnsureRounds();
            return;
        }

        season.Playoffs = PlayoffQualificationService.CreatePlayoffs(season);
    }

    public static MatchResultData SimulateNextPlayoffGame(GameState state)
    {
        if (state == null || state.Season == null)
        {
            return null;
        }

        EnsurePlayoffs(state.Season);

        PlayoffData playoffs = state.Season.Playoffs;
        if (playoffs == null || !playoffs.IsStarted || playoffs.IsCompleted)
        {
            return null;
        }

        PlayoffRoundData currentRound = GetCurrentRound(playoffs);
        PlayoffSeriesData series = GetNextActiveSeries(playoffs);
        if (currentRound == null || series == null)
        {
            return null;
        }

        TeamData teamA = FindTeam(state.Teams, series.TeamAId);
        TeamData teamB = FindTeam(state.Teams, series.TeamBId);
        if (teamA == null || teamB == null)
        {
            return null;
        }

        EnsureTeamPlayers(teamA);
        EnsureTeamPlayers(teamB);
        LineupService.EnsureLineup(teamA);
        LineupService.EnsureLineup(teamB);
        SpecialTeamsService.EnsureSpecialTeams(teamA);
        SpecialTeamsService.EnsureSpecialTeams(teamB);
        TacticsService.EnsureTactics(teamA);
        TacticsService.EnsureTactics(teamB);

        TeamData homeTeam = IsTeamAHome(series.Games.Count + 1) ? teamA : teamB;
        TeamData awayTeam = homeTeam == teamA ? teamB : teamA;
        MatchResultData result = MatchSimulator.Simulate(homeTeam, awayTeam);
        result.PlayerStats = PlayerGameStatsGenerator.Generate(result, homeTeam, awayTeam);

        series.EnsureGames();
        series.Games.Add(result);

        if (result.WinnerTeamId == series.TeamAId)
        {
            series.TeamAWins++;
        }
        else if (result.WinnerTeamId == series.TeamBId)
        {
            series.TeamBWins++;
        }

        if (series.TeamAWins >= 4 || series.TeamBWins >= 4)
        {
            CompleteSeries(series);
        }

        state.EnsureMatchHistory();
        state.LastMatchResult = result;
        state.MatchHistory.Add(result);
        state.TotalGamesSimulated++;

        PlayerStatsService.ApplyGameStats(state.Season, result.PlayerStats);
        AdvanceRoundIfNeeded(state);
        SaveLoadService.Save(state);

        return result;
    }

    public static bool IsPlayoffAvailable(SeasonData season)
    {
        if (season == null || !season.IsSeasonFinished || season.Schedule == null)
        {
            return false;
        }

        foreach (ScheduleGameData game in season.Schedule)
        {
            if (game != null && !game.IsPlayed)
            {
                return false;
            }
        }

        return true;
    }

    public static PlayoffRoundData GetCurrentRound(PlayoffData playoffs)
    {
        if (playoffs == null)
        {
            return null;
        }

        playoffs.EnsureRounds();

        foreach (PlayoffRoundData round in playoffs.Rounds)
        {
            if (round != null && round.RoundNumber == playoffs.CurrentRoundNumber)
            {
                round.EnsureSeries();
                return round;
            }
        }

        return null;
    }

    public static PlayoffSeriesData GetNextActiveSeries(PlayoffData playoffs)
    {
        PlayoffRoundData currentRound = GetCurrentRound(playoffs);
        if (currentRound == null)
        {
            return null;
        }

        foreach (PlayoffSeriesData series in currentRound.Series)
        {
            if (series != null && !series.IsCompleted)
            {
                series.EnsureGames();
                return series;
            }
        }

        return null;
    }

    private static void AdvanceRoundIfNeeded(GameState state)
    {
        PlayoffData playoffs = state.Season.Playoffs;
        PlayoffRoundData currentRound = GetCurrentRound(playoffs);
        if (currentRound == null || !AreAllSeriesCompleted(currentRound))
        {
            return;
        }

        currentRound.IsCompleted = true;

        if (currentRound.RoundNumber == 4)
        {
            PlayoffSeriesData finalSeries = currentRound.Series.Count == 0 ? null : currentRound.Series[0];
            if (finalSeries != null)
            {
                playoffs.ChampionTeamId = finalSeries.WinnerTeamId;
                playoffs.ChampionTeamName = finalSeries.WinnerTeamName;
            }

            playoffs.IsCompleted = true;
            return;
        }

        int nextRoundNumber = currentRound.RoundNumber + 1;
        if (HasRound(playoffs, nextRoundNumber))
        {
            playoffs.CurrentRoundNumber = nextRoundNumber;
            return;
        }

        PlayoffRoundData nextRound = CreateNextRound(currentRound);
        playoffs.Rounds.Add(nextRound);
        playoffs.CurrentRoundNumber = nextRound.RoundNumber;
    }

    private static PlayoffRoundData CreateNextRound(PlayoffRoundData completedRound)
    {
        if (completedRound.RoundNumber == 1)
        {
            return CreateSecondRound(completedRound);
        }

        if (completedRound.RoundNumber == 2)
        {
            return CreateConferenceFinals(completedRound);
        }

        return CreateStanleyCupFinal(completedRound);
    }

    private static PlayoffRoundData CreateSecondRound(PlayoffRoundData completedRound)
    {
        PlayoffRoundData round = CreateRound(2, "Второй раунд");
        AddConferenceWinnerSeries(round, completedRound, EasternConference, 2);
        AddConferenceWinnerSeries(round, completedRound, WesternConference, 2);
        return round;
    }

    private static PlayoffRoundData CreateConferenceFinals(PlayoffRoundData completedRound)
    {
        PlayoffRoundData round = CreateRound(3, "Финалы конференций");
        AddConferenceWinnerSeries(round, completedRound, EasternConference, 1);
        AddConferenceWinnerSeries(round, completedRound, WesternConference, 1);
        return round;
    }

    private static PlayoffRoundData CreateStanleyCupFinal(PlayoffRoundData completedRound)
    {
        PlayoffRoundData round = CreateRound(4, "Финал Stanley Cup");
        List<PlayoffSeriesData> easternWinners = GetCompletedSeriesByConference(completedRound, EasternConference);
        List<PlayoffSeriesData> westernWinners = GetCompletedSeriesByConference(completedRound, WesternConference);

        if (easternWinners.Count > 0 && westernWinners.Count > 0)
        {
            round.Series.Add(CreateSeries(round, StanleyCupConference, easternWinners[0], westernWinners[0]));
        }

        return round;
    }

    private static void AddConferenceWinnerSeries(
        PlayoffRoundData targetRound,
        PlayoffRoundData completedRound,
        string conference,
        int seriesCount)
    {
        List<PlayoffSeriesData> completedSeries = GetCompletedSeriesByConference(completedRound, conference);
        int winnerIndex = 0;

        for (int i = 0; i < seriesCount; i++)
        {
            if (winnerIndex + 1 >= completedSeries.Count)
            {
                return;
            }

            targetRound.Series.Add(CreateSeries(targetRound, conference, completedSeries[winnerIndex], completedSeries[winnerIndex + 1]));
            winnerIndex += 2;
        }
    }

    private static PlayoffRoundData CreateRound(int roundNumber, string roundName)
    {
        return new PlayoffRoundData
        {
            RoundNumber = roundNumber,
            RoundName = roundName,
            IsCompleted = false,
            Series = new List<PlayoffSeriesData>()
        };
    }

    private static PlayoffSeriesData CreateSeries(
        PlayoffRoundData round,
        string conference,
        PlayoffSeriesData firstWinner,
        PlayoffSeriesData secondWinner)
    {
        return new PlayoffSeriesData
        {
            SeriesId = Guid.NewGuid().ToString("N"),
            RoundNumber = round.RoundNumber,
            RoundName = round.RoundName,
            Conference = conference,
            TeamAId = firstWinner.WinnerTeamId,
            TeamAName = firstWinner.WinnerTeamName,
            TeamBId = secondWinner.WinnerTeamId,
            TeamBName = secondWinner.WinnerTeamName,
            WinnerTeamId = "",
            WinnerTeamName = "",
            IsCompleted = false,
            Games = new List<MatchResultData>()
        };
    }

    private static bool AreAllSeriesCompleted(PlayoffRoundData round)
    {
        if (round.Series.Count == 0)
        {
            return false;
        }

        foreach (PlayoffSeriesData series in round.Series)
        {
            if (series != null && !series.IsCompleted)
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasRound(PlayoffData playoffs, int roundNumber)
    {
        foreach (PlayoffRoundData round in playoffs.Rounds)
        {
            if (round != null && round.RoundNumber == roundNumber)
            {
                return true;
            }
        }

        return false;
    }

    private static List<PlayoffSeriesData> GetCompletedSeriesByConference(PlayoffRoundData round, string conference)
    {
        List<PlayoffSeriesData> seriesList = new List<PlayoffSeriesData>();

        foreach (PlayoffSeriesData series in round.Series)
        {
            if (series != null && series.IsCompleted && series.Conference == conference)
            {
                seriesList.Add(series);
            }
        }

        return seriesList;
    }

    private static bool IsTeamAHome(int gameNumber)
    {
        return gameNumber == 1 || gameNumber == 2 || gameNumber == 5 || gameNumber == 7;
    }

    private static void CompleteSeries(PlayoffSeriesData series)
    {
        series.IsCompleted = true;

        if (series.TeamAWins >= 4)
        {
            series.WinnerTeamId = series.TeamAId;
            series.WinnerTeamName = series.TeamAName;
            return;
        }

        series.WinnerTeamId = series.TeamBId;
        series.WinnerTeamName = series.TeamBName;
    }

    private static TeamData FindTeam(List<TeamData> teams, string teamId)
    {
        if (teams == null)
        {
            return null;
        }

        foreach (TeamData team in teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
    }

    private static void EnsureTeamPlayers(TeamData team)
    {
        team.EnsurePlayers();

        if (team.Players.Count == 0)
        {
            team.Players = PlayerSeedData.CreatePlayersForTeam(team.Id);
        }

        ContractGenerator.EnsureContractsForTeam(team);
        TeamRosterService.EnsureRosterStatusesForTeam(team);
    }
}
