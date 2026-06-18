public static class SeasonRecapService
{
    public static LeagueSeasonHistoryData GenerateFullSeasonRecap(GameState state)
    {
        if (state == null)
        {
            return null;
        }

        LeagueHistoryService.EnsureLeagueHistory(state);
        if (LeagueHistoryService.HasSeasonHistory(state, state.CurrentSeasonStartYear))
        {
            return state.LastLeagueSeasonHistory;
        }

        CareerStatsService.UpdateCareerStatsAfterSeason(state);
        SeasonAwardsData awards = AwardsService.GenerateSeasonAwards(state);
        LeagueRecordsService.UpdateRecordsAfterSeason(state);
        LeagueSeasonHistoryData history = LeagueHistoryService.CreateSeasonHistory(state);
        if (history != null)
        {
            history.Awards = awards;
            history.EnsureAwards();
            history.Summary = LeagueHistoryService.BuildSeasonSummary(history);
        }

        return history;
    }

    public static void GenerateAndStoreSeasonRecap(GameState state)
    {
        if (state == null)
        {
            return;
        }

        LeagueHistoryService.EnsureLeagueHistory(state);
        if (LeagueHistoryService.HasSeasonHistory(state, state.CurrentSeasonStartYear))
        {
            return;
        }

        LeagueSeasonHistoryData history = GenerateFullSeasonRecap(state);
        if (history == null)
        {
            return;
        }

        LeagueHistoryService.StoreSeasonHistory(state, history);
        TeamData userTeam = FindTeam(state, state.SelectedTeamId);
        UserTeamSeasonHistoryData userHistory = LeagueHistoryService.CreateUserTeamHistory(state, userTeam);
        LeagueHistoryService.StoreUserTeamHistory(state, userHistory);
        SeasonRecapNewsService.GenerateNewsForSeasonRecap(state, history);
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
}
