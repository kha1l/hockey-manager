using System.Collections.Generic;

public static class StandingsService
{
    public static void EnsureStandings(SeasonData season, List<TeamData> teams)
    {
        if (season == null)
        {
            return;
        }

        season.EnsureCollections();

        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            if (team == null || HasStanding(season, team.Id))
            {
                continue;
            }

            season.Standings.Add(new TeamStandingData
            {
                TeamId = team.Id,
                TeamName = GetTeamDisplayName(team)
            });
        }
    }

    public static void ApplyMatchResult(SeasonData season, MatchResultData result)
    {
        if (season == null || result == null)
        {
            return;
        }

        season.EnsureCollections();

        TeamStandingData homeStanding = GetOrCreateStanding(season, result.HomeTeamId, result.HomeTeamName);
        TeamStandingData awayStanding = GetOrCreateStanding(season, result.AwayTeamId, result.AwayTeamName);

        homeStanding.GamesPlayed++;
        awayStanding.GamesPlayed++;

        homeStanding.GoalsFor += result.HomeScore;
        homeStanding.GoalsAgainst += result.AwayScore;
        awayStanding.GoalsFor += result.AwayScore;
        awayStanding.GoalsAgainst += result.HomeScore;

        TeamStandingData winner = result.WinnerTeamId == result.HomeTeamId ? homeStanding : awayStanding;
        TeamStandingData loser = winner == homeStanding ? awayStanding : homeStanding;

        winner.Wins++;
        winner.Points += 2;

        if (result.IsOvertime)
        {
            loser.OvertimeLosses++;
            loser.Points += 1;
        }
        else
        {
            loser.Losses++;
        }
    }

    public static List<TeamStandingData> GetSortedStandings(SeasonData season)
    {
        if (season == null)
        {
            return new List<TeamStandingData>();
        }

        season.EnsureCollections();
        List<TeamStandingData> sortedStandings = new List<TeamStandingData>(season.Standings);
        sortedStandings.Sort(CompareStandings);

        return sortedStandings;
    }

    private static bool HasStanding(SeasonData season, string teamId)
    {
        foreach (TeamStandingData standing in season.Standings)
        {
            if (standing != null && standing.TeamId == teamId)
            {
                return true;
            }
        }

        return false;
    }

    private static TeamStandingData GetOrCreateStanding(SeasonData season, string teamId, string teamName)
    {
        foreach (TeamStandingData standing in season.Standings)
        {
            if (standing != null && standing.TeamId == teamId)
            {
                return standing;
            }
        }

        TeamStandingData newStanding = new TeamStandingData
        {
            TeamId = teamId,
            TeamName = teamName
        };

        season.Standings.Add(newStanding);
        return newStanding;
    }

    private static int CompareStandings(TeamStandingData left, TeamStandingData right)
    {
        int pointsComparison = right.Points.CompareTo(left.Points);
        if (pointsComparison != 0)
        {
            return pointsComparison;
        }

        int winsComparison = right.Wins.CompareTo(left.Wins);
        if (winsComparison != 0)
        {
            return winsComparison;
        }

        return GetGoalDifference(right).CompareTo(GetGoalDifference(left));
    }

    private static int GetGoalDifference(TeamStandingData standing)
    {
        return standing.GoalsFor - standing.GoalsAgainst;
    }

    private static string GetTeamDisplayName(TeamData team)
    {
        return team.City + " " + team.Name;
    }
}
