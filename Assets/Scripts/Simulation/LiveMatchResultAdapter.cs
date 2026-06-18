using System;
using System.Collections.Generic;
using UnityEngine;

public static class LiveMatchResultAdapter
{
    public static MatchResultData ToMatchResult(LiveMatchStateData match)
    {
        MatchResultData result = new MatchResultData
        {
            MatchId = string.IsNullOrEmpty(match.LiveMatchId) ? Guid.NewGuid().ToString("N") : match.LiveMatchId,
            HomeTeamId = match.HomeTeamId,
            AwayTeamId = match.AwayTeamId,
            HomeTeamName = match.HomeTeamName,
            AwayTeamName = match.AwayTeamName,
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            HomeShots = match.HomeStats == null ? 0 : match.HomeStats.Shots,
            AwayShots = match.AwayStats == null ? 0 : match.AwayStats.Shots,
            HomePowerPlayOpportunities = match.HomeStats == null ? 0 : match.HomeStats.PowerPlayOpportunities,
            AwayPowerPlayOpportunities = match.AwayStats == null ? 0 : match.AwayStats.PowerPlayOpportunities,
            HomePowerPlayGoals = match.HomeStats == null ? 0 : match.HomeStats.PowerPlayGoals,
            AwayPowerPlayGoals = match.AwayStats == null ? 0 : match.AwayStats.PowerPlayGoals,
            HomePenaltyMinutes = match.HomeStats == null ? 0 : match.HomeStats.PenaltyMinutes,
            AwayPenaltyMinutes = match.AwayStats == null ? 0 : match.AwayStats.PenaltyMinutes,
            WinnerTeamId = match.WinnerTeamId,
            IsOvertime = match.IsOvertime || match.IsShootout,
            PlayedAtUtc = DateTime.UtcNow.ToString("o"),
            Summary = match.HomeTeamName + " " + match.HomeScore + " - " + match.AwayScore + " " + match.AwayTeamName
        };

        if (match.IsShootout)
        {
            result.Summary += " SO";
        }
        else if (result.IsOvertime)
        {
            result.Summary += " OT";
        }

        result.Events = match.Events == null ? new List<LiveMatchEventData>() : new List<LiveMatchEventData>(match.Events);
        result.PlayerStats = ToPlayerGameStats(match, result);
        return result;
    }

    public static PostGameSummaryData ToPostGameSummary(MatchResultData result)
    {
        if (result == null)
        {
            return null;
        }

        result.EnsurePlayerStats();
        string winnerName = result.WinnerTeamId == result.HomeTeamId ? result.HomeTeamName : result.AwayTeamName;
        PostGameSummaryData summary = new PostGameSummaryData
        {
            LiveMatchId = result.MatchId,
            HomeTeamId = result.HomeTeamId,
            HomeTeamName = result.HomeTeamName,
            AwayTeamId = result.AwayTeamId,
            AwayTeamName = result.AwayTeamName,
            HomeScore = result.HomeScore,
            AwayScore = result.AwayScore,
            WinnerTeamId = result.WinnerTeamId,
            WinnerTeamName = winnerName,
            WentToOvertime = result.IsOvertime,
            HomeShots = result.HomeShots,
            AwayShots = result.AwayShots,
            HomePowerPlayGoals = result.HomePowerPlayGoals,
            HomePowerPlayOpportunities = result.HomePowerPlayOpportunities,
            AwayPowerPlayGoals = result.AwayPowerPlayGoals,
            AwayPowerPlayOpportunities = result.AwayPowerPlayOpportunities,
            Summary = "Победитель: " + winnerName
        };

        summary.EnsureCollections();
        foreach (LiveMatchEventData matchEvent in result.Events)
        {
            AddSummaryEvent(summary, matchEvent);
        }

        FillStars(result, summary);
        return summary;
    }

    public static PostGameSummaryData ToPostGameSummary(LiveMatchStateData match)
    {
        PostGameSummaryData summary = new PostGameSummaryData
        {
            LiveMatchId = match.LiveMatchId,
            HomeTeamId = match.HomeTeamId,
            HomeTeamName = match.HomeTeamName,
            AwayTeamId = match.AwayTeamId,
            AwayTeamName = match.AwayTeamName,
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            WinnerTeamId = match.WinnerTeamId,
            WinnerTeamName = match.WinnerTeamName,
            WentToOvertime = match.IsOvertime,
            WentToShootout = match.IsShootout,
            HomeShots = match.HomeStats == null ? 0 : match.HomeStats.Shots,
            AwayShots = match.AwayStats == null ? 0 : match.AwayStats.Shots,
            HomePowerPlayGoals = match.HomeStats == null ? 0 : match.HomeStats.PowerPlayGoals,
            HomePowerPlayOpportunities = match.HomeStats == null ? 0 : match.HomeStats.PowerPlayOpportunities,
            AwayPowerPlayGoals = match.AwayStats == null ? 0 : match.AwayStats.PowerPlayGoals,
            AwayPowerPlayOpportunities = match.AwayStats == null ? 0 : match.AwayStats.PowerPlayOpportunities,
            Summary = "Победитель: " + match.WinnerTeamName
        };

        summary.EnsureCollections();
        foreach (LiveMatchEventData matchEvent in match.Events)
        {
            if (matchEvent == null)
            {
                continue;
            }

            AddSummaryEvent(summary, matchEvent);
        }

        FillStars(match, summary);
        return summary;
    }

    public static MatchResultData ApplyLiveMatchResultToSeason(GameState state, LiveMatchStateData match, TeamData homeTeam, TeamData awayTeam)
    {
        if (state == null || state.Season == null || match == null)
        {
            return null;
        }

        MatchResultData result = ToMatchResult(match);
        ScheduleGameData scheduledGame = FindScheduledGame(state.Season, match.ScheduledGameId);
        if (scheduledGame != null)
        {
            scheduledGame.Result = result;
            scheduledGame.IsPlayed = true;
            state.Season.CurrentGameIndex = scheduledGame.GameNumber;
        }

        state.EnsureMatchHistory();
        if (!HasMatchHistory(state, result.MatchId))
        {
            state.MatchHistory.Add(result);
            state.TotalGamesSimulated++;
        }

        state.LastMatchResult = result;
        StandingsService.ApplyMatchResult(state.Season, result);
        PlayerStatsService.ApplyGameStats(state.Season, result.PlayerStats);
        IceTimeService.ApplyLastGameIceTime(homeTeam);
        IceTimeService.ApplyLastGameIceTime(awayTeam);
        PlayerFatigueService.ApplyFatigueAfterMatch(homeTeam, awayTeam);
        if (!HasLiveInjuryEvent(match))
        {
            InjuryService.ApplyInjuryChecksAfterMatch(state, homeTeam, awayTeam, match.IsPlayoffGame ? "LivePlayoffs" : "LiveRegularSeason");
        }

        return result;
    }

    private static List<PlayerGameStatData> ToPlayerGameStats(LiveMatchStateData match, MatchResultData result)
    {
        List<PlayerGameStatData> stats = new List<PlayerGameStatData>();
        foreach (LiveMatchPlayerStatData liveStat in match.PlayerStats)
        {
            if (liveStat == null)
            {
                continue;
            }

            PlayerGameStatData stat = new PlayerGameStatData
            {
                PlayerId = liveStat.PlayerId,
                TeamId = liveStat.TeamId,
                PlayerName = liveStat.PlayerName,
                Position = liveStat.Position,
                IsGoalie = liveStat.IsGoalie,
                Goals = liveStat.Goals,
                Assists = liveStat.Assists,
                Points = liveStat.Goals + liveStat.Assists,
                Shots = liveStat.Shots,
                PenaltyMinutes = liveStat.PenaltyMinutes,
                PlusMinus = liveStat.PlusMinus,
                Saves = liveStat.Saves,
                GoalsAgainst = liveStat.GoalsAgainst,
                ShotsAgainst = liveStat.Saves + liveStat.GoalsAgainst,
                TimeOnIceSeconds = Mathf.Max(liveStat.TimeOnIceSeconds, liveStat.IsGoalie ? 0 : 240)
            };

            if (liveStat.IsGoalie && liveStat.FinishedGame)
            {
                bool won = result.WinnerTeamId == liveStat.TeamId;
                stat.GoalieWin = won;
                stat.GoalieLoss = !won && !result.IsOvertime;
                stat.GoalieOvertimeLoss = !won && result.IsOvertime;
                stat.Shutout = liveStat.GoalsAgainst == 0;
            }

            stats.Add(stat);
        }

        return stats;
    }

    private static void FillStars(LiveMatchStateData match, PostGameSummaryData summary)
    {
        List<LiveMatchPlayerStatData> players = new List<LiveMatchPlayerStatData>(match.PlayerStats);
        players.Sort((left, right) => GetStarScore(right).CompareTo(GetStarScore(left)));
        summary.FirstStarPlayerName = players.Count > 0 ? players[0].PlayerName : "";
        summary.SecondStarPlayerName = players.Count > 1 ? players[1].PlayerName : "";
        summary.ThirdStarPlayerName = players.Count > 2 ? players[2].PlayerName : "";
    }

    private static void FillStars(MatchResultData result, PostGameSummaryData summary)
    {
        List<PlayerGameStatData> players = new List<PlayerGameStatData>(result.PlayerStats);
        players.Sort((left, right) => GetStarScore(right).CompareTo(GetStarScore(left)));
        summary.FirstStarPlayerName = players.Count > 0 ? players[0].PlayerName : "";
        summary.SecondStarPlayerName = players.Count > 1 ? players[1].PlayerName : "";
        summary.ThirdStarPlayerName = players.Count > 2 ? players[2].PlayerName : "";
    }

    private static void AddSummaryEvent(PostGameSummaryData summary, LiveMatchEventData matchEvent)
    {
        if (summary == null || matchEvent == null)
        {
            return;
        }

        if (matchEvent.EventType == "Goal" || matchEvent.EventType == "ShootoutGoal")
        {
            summary.ScoringEvents.Add(matchEvent);
        }
        else if (matchEvent.EventType == "Penalty")
        {
            summary.PenaltyEvents.Add(matchEvent);
        }
        else if (matchEvent.EventType == "Injury")
        {
            summary.InjuryEvents.Add(matchEvent);
        }
    }

    private static int GetStarScore(LiveMatchPlayerStatData stat)
    {
        if (stat == null)
        {
            return 0;
        }

        return stat.Goals * 5 + stat.Assists * 3 + stat.Shots + stat.Saves / 4 - stat.GoalsAgainst * 2;
    }

    private static int GetStarScore(PlayerGameStatData stat)
    {
        if (stat == null)
        {
            return 0;
        }

        return stat.Goals * 5 + stat.Assists * 3 + stat.Shots + stat.Saves / 4 - stat.GoalsAgainst * 2;
    }

    private static ScheduleGameData FindScheduledGame(SeasonData season, string gameId)
    {
        if (season == null || season.Schedule == null || string.IsNullOrEmpty(gameId))
        {
            return null;
        }

        foreach (ScheduleGameData game in season.Schedule)
        {
            if (game != null && game.GameId == gameId)
            {
                return game;
            }
        }

        return null;
    }

    private static bool HasMatchHistory(GameState state, string matchId)
    {
        if (state == null || state.MatchHistory == null || string.IsNullOrEmpty(matchId))
        {
            return false;
        }

        foreach (MatchResultData result in state.MatchHistory)
        {
            if (result != null && result.MatchId == matchId)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasLiveInjuryEvent(LiveMatchStateData match)
    {
        if (match == null || match.Events == null)
        {
            return false;
        }

        foreach (LiveMatchEventData matchEvent in match.Events)
        {
            if (matchEvent != null && matchEvent.EventType == "Injury")
            {
                return true;
            }
        }

        return false;
    }
}
