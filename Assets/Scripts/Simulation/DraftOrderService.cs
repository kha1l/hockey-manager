using System.Collections.Generic;

public static class DraftOrderService
{
    public static List<DraftPickData> CreateDraftOrder(GameState state)
    {
        List<DraftPickData> draftOrder = new List<DraftPickData>();
        if (state == null || state.Teams == null)
        {
            return draftOrder;
        }

        DraftPickOwnershipService.EnsureDraftPickOwnership(state);
        List<TeamData> originalOrder = CreateOriginalTeamOrder(state);
        int overallPick = 1;
        int draftYear = DraftPickOwnershipService.GetDraftYear(state);

        for (int round = 1; round <= DraftConfig.DraftRounds; round++)
        {
            for (int pickInRound = 1; pickInRound <= DraftConfig.PicksPerRound && pickInRound <= originalOrder.Count; pickInRound++)
            {
                TeamData originalTeam = originalOrder[pickInRound - 1];
                string pickId = DraftPickOwnershipService.CreatePickId(draftYear, round, originalTeam.Id);
                DraftPickOwnershipData ownership = DraftPickOwnershipService.FindPick(state, pickId);
                string teamId = ownership == null ? originalTeam.Id : ownership.CurrentOwnerTeamId;
                string teamName = ownership == null ? GetTeamName(originalTeam) : ownership.CurrentOwnerTeamName;

                draftOrder.Add(new DraftPickData
                {
                    PickId = pickId,
                    Round = round,
                    PickInRound = pickInRound,
                    OverallPick = overallPick,
                    OriginalTeamId = originalTeam.Id,
                    OriginalTeamName = GetTeamName(originalTeam),
                    TeamId = teamId,
                    TeamName = teamName,
                    IsUserTeamPick = teamId == state.SelectedTeamId,
                    IsCompleted = false,
                    SelectedProspectId = "",
                    SelectedProspectName = ""
                });

                overallPick++;
            }
        }

        return draftOrder;
    }

    private static List<TeamData> CreateOriginalTeamOrder(GameState state)
    {
        List<TeamData> order = new List<TeamData>();
        if (state.Season == null || state.Season.Standings == null || state.Season.Standings.Count == 0)
        {
            return FirstTeams(state.Teams);
        }

        List<TeamStandingData> nonPlayoffStandings = DraftLotteryService.GetNonPlayoffTeamsSorted(state.Season);
        List<TeamData> nonPlayoffTeams = MapStandingsToTeams(state.Teams, nonPlayoffStandings);
        order.AddRange(DraftLotteryService.ApplySimpleLottery(state, nonPlayoffTeams));

        HashSet<string> added = new HashSet<string>();
        foreach (TeamData team in order)
        {
            if (team != null)
            {
                added.Add(team.Id);
            }
        }

        List<TeamStandingData> playoffStandings = new List<TeamStandingData>();
        foreach (TeamStandingData standing in state.Season.Standings)
        {
            if (standing != null && !added.Contains(standing.TeamId))
            {
                playoffStandings.Add(standing);
            }
        }

        playoffStandings.Sort(CompareStandingsWorstToBest);
        List<TeamData> playoffTeams = MapStandingsToTeams(state.Teams, playoffStandings);
        MoveChampionToEnd(playoffTeams, state.Season.Playoffs == null ? "" : state.Season.Playoffs.ChampionTeamId);
        order.AddRange(playoffTeams);

        foreach (TeamData team in state.Teams)
        {
            if (team != null && !added.Contains(team.Id) && !ContainsTeam(order, team.Id))
            {
                order.Add(team);
            }
        }

        return FirstTeams(order);
    }

    private static List<TeamData> FirstTeams(List<TeamData> teams)
    {
        List<TeamData> result = new List<TeamData>();
        if (teams == null)
        {
            return result;
        }

        foreach (TeamData team in teams)
        {
            if (team != null && result.Count < DraftConfig.PicksPerRound)
            {
                result.Add(team);
            }
        }

        return result;
    }

    private static List<TeamData> MapStandingsToTeams(List<TeamData> teams, List<TeamStandingData> standings)
    {
        List<TeamData> result = new List<TeamData>();
        foreach (TeamStandingData standing in standings)
        {
            TeamData team = FindTeam(teams, standing.TeamId);
            if (team != null)
            {
                result.Add(team);
            }
        }

        return result;
    }

    private static void MoveChampionToEnd(List<TeamData> teams, string championTeamId)
    {
        if (string.IsNullOrEmpty(championTeamId))
        {
            return;
        }

        TeamData champion = FindTeam(teams, championTeamId);
        if (champion == null)
        {
            return;
        }

        teams.Remove(champion);
        teams.Add(champion);
    }

    private static TeamData FindTeam(List<TeamData> teams, string teamId)
    {
        if (teams == null || string.IsNullOrEmpty(teamId))
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

    private static bool ContainsTeam(List<TeamData> teams, string teamId)
    {
        return FindTeam(teams, teamId) != null;
    }

    private static int CompareStandingsWorstToBest(TeamStandingData left, TeamStandingData right)
    {
        int pointsComparison = left.Points.CompareTo(right.Points);
        if (pointsComparison != 0)
        {
            return pointsComparison;
        }

        return left.Wins.CompareTo(right.Wins);
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
