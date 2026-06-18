public static class PrimaryCareerTeamService
{
    public static void UpdatePlayerPrimaryTeamAfterSeason(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                UpdatePlayerPrimaryTeam(player, team, state);
            }
        }
    }

    public static void UpdatePlayerPrimaryTeam(PlayerData player, TeamData currentTeam)
    {
        UpdatePlayerPrimaryTeam(player, currentTeam, null);
    }

    public static string BuildPrimaryTeamSummary(PlayerData player)
    {
        if (player == null || string.IsNullOrEmpty(player.PrimaryCareerTeamName))
        {
            return "Primary team unknown";
        }

        return player.PrimaryCareerTeamName + " | seasons " + player.SeasonsWithPrimaryTeam;
    }

    private static void UpdatePlayerPrimaryTeam(PlayerData player, TeamData currentTeam, GameState state)
    {
        if (player == null || currentTeam == null || player.IsRetired)
        {
            return;
        }

        int seasonStartYear = state == null ? 0 : state.CurrentSeasonStartYear;
        if (seasonStartYear > 0 && player.LastPrimaryTeamSeasonUpdatedStartYear == seasonStartYear)
        {
            return;
        }

        string currentTeamName = GetTeamName(currentTeam);
        if (string.IsNullOrEmpty(player.PrimaryCareerTeamId))
        {
            player.PrimaryCareerTeamId = currentTeam.Id;
            player.PrimaryCareerTeamName = currentTeamName;
            player.SeasonsWithPrimaryTeam = 1;
        }
        else if (player.PrimaryCareerTeamId == currentTeam.Id)
        {
            player.SeasonsWithPrimaryTeam++;
            player.PrimaryCareerTeamName = currentTeamName;
        }
        else if (player.SeasonsWithPrimaryTeam <= 0)
        {
            player.PrimaryCareerTeamId = currentTeam.Id;
            player.PrimaryCareerTeamName = currentTeamName;
            player.SeasonsWithPrimaryTeam = 1;
        }

        if (state != null && state.Season != null && state.Season.Playoffs != null)
        {
            if (state.Season.Playoffs.ChampionTeamId == currentTeam.Id)
            {
                player.ChampionshipsWon++;
            }

            player.PlayoffRoundsWonCareer += CountPlayoffRoundsWonByTeam(state, currentTeam.Id);
        }

        player.LastPrimaryTeamSeasonUpdatedStartYear = seasonStartYear;
    }

    private static int CountPlayoffRoundsWonByTeam(GameState state, string teamId)
    {
        if (state == null || state.Season == null || state.Season.Playoffs == null || state.Season.Playoffs.Rounds == null)
        {
            return 0;
        }

        int won = 0;
        foreach (PlayoffRoundData round in state.Season.Playoffs.Rounds)
        {
            if (round == null || round.Series == null)
            {
                continue;
            }

            foreach (PlayoffSeriesData series in round.Series)
            {
                if (series != null && series.WinnerTeamId == teamId)
                {
                    won++;
                }
            }
        }

        return won;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
