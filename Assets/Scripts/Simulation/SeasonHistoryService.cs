using System;
using System.Collections.Generic;

public static class SeasonHistoryService
{
    public static void EnsureSeasonHistory(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureSeasonHistory();
    }

    public static SeasonHistoryData CreateSeasonHistorySnapshot(GameState state)
    {
        SeasonHistoryData history = new SeasonHistoryData
        {
            ArchivedAtUtc = DateTime.UtcNow.ToString("o")
        };

        if (state == null)
        {
            return history;
        }

        state.EnsureCareerProgress();
        LeagueRulesData rules = state.LeagueRules == null
            ? LeagueRulesConfig.CreateDefaultRules()
            : state.LeagueRules;
        TeamData userTeam = FindTeam(state, state.SelectedTeamId);

        history.SeasonStartYear = state.CurrentSeasonStartYear;
        history.SeasonEndYear = state.CurrentSeasonEndYear;
        history.RulesetName = GetRulesetName(rules);
        history.CbaName = GetCbaName(rules);
        history.UserTeamId = state.SelectedTeamId;
        history.UserTeamName = GetTeamName(userTeam);

        if (state.Season != null)
        {
            state.Season.EnsureCollections();
            FillChampion(history, state.Season.Playoffs);
            FillTeamStandings(history, state);
            FillPlayerStats(history, state.Season.PlayerStats);
            history.UserTeamMadePlayoffs = DidTeamMakePlayoffs(state.Season.Playoffs, state.SelectedTeamId);
        }

        FillDraftPicks(history, state);
        return history;
    }

    public static bool IsCurrentSeasonAlreadyArchived(GameState state)
    {
        if (state == null || state.SeasonHistory == null)
        {
            return false;
        }

        foreach (SeasonHistoryData history in state.SeasonHistory)
        {
            if (history != null && history.SeasonStartYear == state.CurrentSeasonStartYear)
            {
                return true;
            }
        }

        return false;
    }

    public static void ArchiveCurrentSeasonIfNeeded(GameState state)
    {
        if (state == null || IsCurrentSeasonAlreadyArchived(state) || !LeaguePhaseService.CanStartNextSeason(state))
        {
            return;
        }

        try
        {
            EnsureSeasonHistory(state);
            state.SeasonHistory.Add(CreateSeasonHistorySnapshot(state));
        }
        catch (Exception)
        {
        }
    }

    private static void FillChampion(SeasonHistoryData history, PlayoffData playoffs)
    {
        if (playoffs == null)
        {
            return;
        }

        history.ChampionTeamId = playoffs.ChampionTeamId;
        history.ChampionTeamName = playoffs.ChampionTeamName;
    }

    private static void FillTeamStandings(SeasonHistoryData history, GameState state)
    {
        List<TeamStandingData> sortedStandings = StandingsService.GetSortedStandings(state.Season);
        for (int i = 0; i < sortedStandings.Count; i++)
        {
            TeamStandingData standing = sortedStandings[i];
            if (standing == null)
            {
                continue;
            }

            int rank = i + 1;
            history.TeamStandings.Add(new TeamSeasonHistoryData
            {
                TeamId = standing.TeamId,
                TeamName = standing.TeamName,
                GamesPlayed = standing.GamesPlayed,
                Wins = standing.Wins,
                Losses = standing.Losses,
                OvertimeLosses = standing.OvertimeLosses,
                GoalsFor = standing.GoalsFor,
                GoalsAgainst = standing.GoalsAgainst,
                Points = standing.Points,
                FinalRank = rank
            });

            if (standing.TeamId == state.SelectedTeamId)
            {
                history.UserTeamPoints = standing.Points;
                history.UserTeamRank = rank;
            }
        }
    }

    private static void FillPlayerStats(SeasonHistoryData history, List<PlayerSeasonStatsData> playerStats)
    {
        if (playerStats == null)
        {
            return;
        }

        foreach (PlayerSeasonStatsData stats in playerStats)
        {
            if (stats == null)
            {
                continue;
            }

            history.PlayerStats.Add(new PlayerSeasonHistoryData
            {
                PlayerId = stats.PlayerId,
                TeamId = stats.TeamId,
                PlayerName = stats.PlayerName,
                Position = stats.Position,
                IsGoalie = stats.IsGoalie,
                GamesPlayed = stats.GamesPlayed,
                Goals = stats.Goals,
                Assists = stats.Assists,
                Points = stats.Points,
                Shots = stats.Shots,
                PlusMinus = stats.PlusMinus,
                GoalieGamesPlayed = stats.GoalieGamesPlayed,
                GoalieWins = stats.GoalieWins,
                GoalieLosses = stats.GoalieLosses,
                GoalieOvertimeLosses = stats.GoalieOvertimeLosses,
                Saves = stats.Saves,
                ShotsAgainst = stats.ShotsAgainst,
                GoalsAgainst = stats.GoalsAgainst,
                Shutouts = stats.Shutouts
            });
        }
    }

    private static void FillDraftPicks(SeasonHistoryData history, GameState state)
    {
        if (state == null || state.DraftHistory == null)
        {
            return;
        }

        state.DraftHistory.EnsureCompletedPicks();
        foreach (DraftPickData pick in state.DraftHistory.CompletedPicks)
        {
            if (pick != null)
            {
                history.DraftPicks.Add(CloneDraftPick(pick));
            }
        }
    }

    private static bool DidTeamMakePlayoffs(PlayoffData playoffs, string teamId)
    {
        if (playoffs == null || playoffs.Rounds == null || string.IsNullOrEmpty(teamId))
        {
            return false;
        }

        foreach (PlayoffRoundData round in playoffs.Rounds)
        {
            if (round == null || round.RoundNumber != 1 || round.Series == null)
            {
                continue;
            }

            foreach (PlayoffSeriesData series in round.Series)
            {
                if (series != null && (series.TeamAId == teamId || series.TeamBId == teamId))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static DraftPickData CloneDraftPick(DraftPickData pick)
    {
        return new DraftPickData
        {
            PickId = pick.PickId,
            Round = pick.Round,
            PickInRound = pick.PickInRound,
            OverallPick = pick.OverallPick,
            OriginalTeamId = pick.OriginalTeamId,
            OriginalTeamName = pick.OriginalTeamName,
            TeamId = pick.TeamId,
            TeamName = pick.TeamName,
            IsUserTeamPick = pick.IsUserTeamPick,
            IsCompleted = pick.IsCompleted,
            SelectedProspectId = pick.SelectedProspectId,
            SelectedProspectName = pick.SelectedProspectName
        };
    }

    private static TeamData FindTeam(GameState state, string teamId)
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

    private static string GetTeamName(TeamData team)
    {
        return team == null ? "" : team.City + " " + team.Name;
    }

    private static string GetRulesetName(LeagueRulesData rules)
    {
        if (!string.IsNullOrEmpty(rules.RulesetName))
        {
            return rules.RulesetName;
        }

        return rules.Ruleset;
    }

    private static string GetCbaName(LeagueRulesData rules)
    {
        if (!string.IsNullOrEmpty(rules.CbaName))
        {
            return rules.CbaName;
        }

        return rules.Cba;
    }
}
