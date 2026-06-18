using System;
using System.Collections.Generic;

public static class OwnerGoalProgressService
{
    public static void UpdateGoalProgress(GameState state, TeamData team)
    {
        if (team == null || team.OwnerProfile == null || team.OwnerProfile.CurrentGoals == null)
        {
            return;
        }

        foreach (OwnerGoalData goal in team.OwnerProfile.CurrentGoals)
        {
            UpdateGoalProgress(state, team, goal);
        }
    }

    public static void UpdateGoalProgress(GameState state, TeamData team, OwnerGoalData goal)
    {
        if (goal == null)
        {
            return;
        }

        int currentValue = 0;
        int targetValue = goal.TargetValue <= 0 ? 1 : goal.TargetValue;

        if (goal.Title == OwnerGoalConfig.GoalMakePlayoffs)
        {
            currentValue = DidTeamMakePlayoffs(state, team) ? 1 : 0;
        }
        else if (goal.Title == OwnerGoalConfig.GoalWinPlayoffRound)
        {
            currentValue = GetPlayoffRoundsWon(state, team);
            targetValue = Math.Max(1, goal.TargetValue);
        }
        else if (goal.Title == OwnerGoalConfig.GoalReachConferenceFinal)
        {
            currentValue = GetPlayoffRoundsWon(state, team);
            targetValue = 2;
        }
        else if (goal.Title == OwnerGoalConfig.GoalReachStanleyCupFinal)
        {
            currentValue = GetPlayoffRoundsWon(state, team);
            targetValue = 3;
        }
        else if (goal.Title == OwnerGoalConfig.GoalWinCup)
        {
            currentValue = IsStanleyCupChampion(state, team) ? 4 : GetPlayoffRoundsWon(state, team);
            targetValue = 4;
        }
        else if (goal.Title == OwnerGoalConfig.GoalImproveTeam)
        {
            currentValue = GetProjectedRegularSeasonPoints(state, team);
        }
        else if (goal.Title == OwnerGoalConfig.GoalDevelopYoungPlayers)
        {
            currentValue = CountDevelopedYoungPlayers(state, team);
        }
        else if (goal.Title == OwnerGoalConfig.GoalAcquireDraftPicks)
        {
            currentValue = CountAcquiredDraftPicks(state, team);
        }
        else if (goal.Title == OwnerGoalConfig.GoalReducePayroll)
        {
            int payroll = SalaryCapService.CalculatePayroll(team);
            currentValue = payroll <= goal.TargetValue ? goal.TargetValue : Math.Max(0, goal.TargetValue - (payroll - goal.TargetValue));
        }
        else if (goal.Title == OwnerGoalConfig.GoalStayUnderCap)
        {
            LeagueRulesData rules = state == null || state.LeagueRules == null
                ? LeagueRulesConfig.CreateDefaultRules()
                : state.LeagueRules;
            currentValue = SalaryCapService.CalculatePayroll(team) <= rules.SalaryCapUpperLimit ? 1 : 0;
            targetValue = 1;
        }
        else if (goal.Title == OwnerGoalConfig.GoalReSignCorePlayer)
        {
            currentValue = DidReSignCorePlayer(state, team) ? 1 : 0;
            targetValue = 1;
        }
        else if (goal.Title == OwnerGoalConfig.GoalImproveMorale)
        {
            currentValue = CalculateTeamMoraleScore(state, team);
        }
        else if (goal.Title == OwnerGoalConfig.GoalBuildForFuture)
        {
            currentValue = CountDevelopedYoungPlayers(state, team)
                + CountAcquiredDraftPicks(state, team)
                + CountHighPotentialDraftRights(team);
        }

        goal.CurrentValue = currentValue;
        goal.ProgressPercent = ClampPercent(currentValue * 100 / Math.Max(1, targetValue));
        goal.IsCompleted = goal.ProgressPercent >= 100;
        if (goal.IsCompleted)
        {
            goal.IsFailed = false;
            goal.Status = OwnerGoalConfig.StatusCompleted;
        }
        else if (goal.Status != OwnerGoalConfig.StatusFailed)
        {
            goal.Status = OwnerGoalConfig.StatusActive;
        }

        goal.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    public static bool DidTeamMakePlayoffs(GameState state, TeamData team)
    {
        if (state == null || state.Season == null || team == null)
        {
            return false;
        }

        if (state.Season.Playoffs != null && state.Season.Playoffs.Rounds != null)
        {
            foreach (PlayoffRoundData round in state.Season.Playoffs.Rounds)
            {
                if (round == null || round.Series == null)
                {
                    continue;
                }

                foreach (PlayoffSeriesData series in round.Series)
                {
                    if (series != null && (series.TeamAId == team.Id || series.TeamBId == team.Id))
                    {
                        return true;
                    }
                }
            }
        }

        int rank = GetLeagueRank(state, team);
        return rank > 0 && rank <= 16;
    }

    public static int GetPlayoffRoundsWon(GameState state, TeamData team)
    {
        if (state == null || state.Season == null || state.Season.Playoffs == null || team == null)
        {
            return 0;
        }

        int roundsWon = 0;
        state.Season.Playoffs.EnsureRounds();
        foreach (PlayoffRoundData round in state.Season.Playoffs.Rounds)
        {
            if (round == null || round.Series == null)
            {
                continue;
            }

            foreach (PlayoffSeriesData series in round.Series)
            {
                if (series != null && series.IsCompleted && series.WinnerTeamId == team.Id)
                {
                    roundsWon++;
                }
            }
        }

        return roundsWon;
    }

    public static int GetRegularSeasonPoints(GameState state, TeamData team)
    {
        TeamStandingData standing = FindStanding(state, team);
        return standing == null ? 0 : standing.Points;
    }

    public static int GetLeagueRank(GameState state, TeamData team)
    {
        if (state == null || state.Season == null || team == null)
        {
            return 0;
        }

        List<TeamStandingData> standings = StandingsService.GetSortedStandings(state.Season);
        for (int i = 0; i < standings.Count; i++)
        {
            TeamStandingData standing = standings[i];
            if (standing != null && standing.TeamId == team.Id)
            {
                return i + 1;
            }
        }

        return 0;
    }

    public static int CountDevelopedYoungPlayers(GameState state, TeamData team)
    {
        if (state == null || state.PlayerDevelopmentHistory == null || team == null)
        {
            return CountYoungPlayersWithRecentGrowth(team);
        }

        state.PlayerDevelopmentHistory.EnsureChanges();
        int count = 0;
        foreach (PlayerDevelopmentChangeData change in state.PlayerDevelopmentHistory.Changes)
        {
            if (change != null
                && change.TeamId == team.Id
                && change.Age <= 24
                && change.OverallDelta >= 2
                && (change.SeasonStartYear == state.CurrentSeasonStartYear
                    || change.SeasonStartYear == state.CurrentSeasonStartYear - 1))
            {
                count++;
            }
        }

        return count > 0 ? count : CountYoungPlayersWithRecentGrowth(team);
    }

    public static int CountAcquiredDraftPicks(GameState state, TeamData team)
    {
        if (state == null || team == null)
        {
            return 0;
        }

        List<DraftPickOwnershipData> picks = DraftPickOwnershipService.GetOwnedPicks(state, team.Id);
        int count = 0;
        foreach (DraftPickOwnershipData pick in picks)
        {
            if (pick == null)
            {
                continue;
            }

            if (pick.OriginalTeamId != team.Id || pick.Round <= DraftConfig.DraftRounds)
            {
                count++;
            }
        }

        return count;
    }

    public static bool DidReSignCorePlayer(GameState state, TeamData team)
    {
        if (state != null
            && state.ContractExtensionHistory != null
            && state.ContractExtensionHistory.Offers != null
            && team != null)
        {
            foreach (ContractExtensionOfferData offer in state.ContractExtensionHistory.Offers)
            {
                if (offer != null
                    && offer.TeamId == team.Id
                    && offer.Accepted
                    && offer.OfferedYears > 1)
                {
                    return true;
                }
            }
        }

        if (team == null)
        {
            return false;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null
                && RosterStatusConfig.IsNhlRoster(player)
                && player.Overall >= 82
                && player.ContractYearsRemaining > 1
                && player.ContractStatus == "Signed")
            {
                return true;
            }
        }

        return false;
    }

    public static int CalculateTeamMoraleScore(GameState state, TeamData team)
    {
        TeamMoraleSummaryData summary = MoraleService.BuildTeamMoraleSummary(state, team);
        return summary == null ? 50 : summary.AverageMorale;
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

    private static int GetProjectedRegularSeasonPoints(GameState state, TeamData team)
    {
        TeamStandingData standing = FindStanding(state, team);
        if (standing != null && standing.GamesPlayed > 0)
        {
            return standing.Points * SalaryCapConfig.TargetGamesPerTeam / standing.GamesPlayed;
        }

        int overall = TeamRatingCalculator.CalculateOverall(team);
        return Clamp(overall + 4, 55, 110);
    }

    private static bool IsStanleyCupChampion(GameState state, TeamData team)
    {
        return state != null
            && state.Season != null
            && state.Season.Playoffs != null
            && team != null
            && state.Season.Playoffs.ChampionTeamId == team.Id;
    }

    private static int CountYoungPlayersWithRecentGrowth(TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        int count = 0;
        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null
                && RosterStatusConfig.IsInOrganization(player)
                && player.Age <= 24
                && player.LastDevelopmentDelta >= 2)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountHighPotentialDraftRights(TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        team.EnsureDraftRights();
        int count = 0;
        foreach (ProspectData prospect in team.DraftRights)
        {
            if (prospect != null && prospect.Potential >= 82)
            {
                count++;
            }
        }

        return count;
    }

    private static int ClampPercent(int value)
    {
        return Clamp(value, 0, 100);
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}
