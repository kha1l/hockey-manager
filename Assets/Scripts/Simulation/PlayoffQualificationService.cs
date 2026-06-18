using System;
using System.Collections.Generic;

public static class PlayoffQualificationService
{
    public static PlayoffData CreatePlayoffs(SeasonData season)
    {
        PlayoffData playoffs = new PlayoffData
        {
            IsStarted = false,
            IsCompleted = false,
            CurrentRoundNumber = 1
        };

        if (season == null || season.Standings == null || season.Standings.Count == 0)
        {
            return playoffs;
        }

        PlayoffRoundData firstRound = new PlayoffRoundData
        {
            RoundNumber = 1,
            RoundName = "Первый раунд",
            IsCompleted = false
        };

        AddConferenceFirstRound(
            firstRound,
            TeamStructureService.EasternConference,
            TeamStructureService.VolgaUralDivision,
            TeamStructureService.SiberiaPacificDivision,
            season.Standings);
        AddConferenceFirstRound(
            firstRound,
            TeamStructureService.WesternConference,
            TeamStructureService.CapitalDivision,
            TeamStructureService.SouthDivision,
            season.Standings);

        playoffs.IsStarted = firstRound.Series.Count > 0;
        playoffs.Rounds.Add(firstRound);

        return playoffs;
    }

    private static void AddConferenceFirstRound(
        PlayoffRoundData round,
        string conference,
        string firstDivision,
        string secondDivision,
        List<TeamStandingData> standings)
    {
        List<TeamStandingData> firstDivisionTeams = GetSortedDivisionTeams(standings, conference, firstDivision);
        List<TeamStandingData> secondDivisionTeams = GetSortedDivisionTeams(standings, conference, secondDivision);

        if (firstDivisionTeams.Count < 3 || secondDivisionTeams.Count < 3)
        {
            return;
        }

        List<TeamStandingData> automaticTeams = new List<TeamStandingData>
        {
            firstDivisionTeams[0],
            firstDivisionTeams[1],
            firstDivisionTeams[2],
            secondDivisionTeams[0],
            secondDivisionTeams[1],
            secondDivisionTeams[2]
        };

        List<TeamStandingData> wildCards = GetWildCards(standings, conference, automaticTeams);
        if (wildCards.Count < 2)
        {
            return;
        }

        TeamStandingData firstWinner = firstDivisionTeams[0];
        TeamStandingData secondWinner = secondDivisionTeams[0];
        TeamStandingData strongerWinner = CompareStandings(firstWinner, secondWinner) <= 0 ? firstWinner : secondWinner;
        TeamStandingData weakerWinner = strongerWinner == firstWinner ? secondWinner : firstWinner;
        TeamStandingData betterWildCard = wildCards[0];
        TeamStandingData worseWildCard = wildCards[1];

        round.Series.Add(CreateSeries(round, conference, strongerWinner, worseWildCard));
        round.Series.Add(CreateSeries(round, conference, weakerWinner, betterWildCard));
        round.Series.Add(CreateSeries(round, conference, firstDivisionTeams[1], firstDivisionTeams[2]));
        round.Series.Add(CreateSeries(round, conference, secondDivisionTeams[1], secondDivisionTeams[2]));
    }

    private static List<TeamStandingData> GetSortedDivisionTeams(
        List<TeamStandingData> standings,
        string conference,
        string division)
    {
        List<TeamStandingData> teams = new List<TeamStandingData>();

        foreach (TeamStandingData standing in standings)
        {
            if (standing != null
                && TeamStructureService.GetConference(standing.TeamId) == conference
                && TeamStructureService.GetDivision(standing.TeamId) == division)
            {
                teams.Add(standing);
            }
        }

        teams.Sort(CompareStandings);
        return teams;
    }

    private static List<TeamStandingData> GetWildCards(
        List<TeamStandingData> standings,
        string conference,
        List<TeamStandingData> automaticTeams)
    {
        List<TeamStandingData> wildCards = new List<TeamStandingData>();

        foreach (TeamStandingData standing in standings)
        {
            if (standing == null
                || TeamStructureService.GetConference(standing.TeamId) != conference
                || ContainsTeam(automaticTeams, standing.TeamId))
            {
                continue;
            }

            wildCards.Add(standing);
        }

        wildCards.Sort(CompareStandings);
        return wildCards;
    }

    private static PlayoffSeriesData CreateSeries(PlayoffRoundData round, string conference, TeamStandingData teamA, TeamStandingData teamB)
    {
        return new PlayoffSeriesData
        {
            SeriesId = Guid.NewGuid().ToString("N"),
            RoundNumber = round.RoundNumber,
            RoundName = round.RoundName,
            Conference = conference,
            TeamAId = teamA.TeamId,
            TeamAName = teamA.TeamName,
            TeamBId = teamB.TeamId,
            TeamBName = teamB.TeamName,
            TeamAWins = 0,
            TeamBWins = 0,
            WinnerTeamId = "",
            WinnerTeamName = "",
            IsCompleted = false,
            Games = new List<MatchResultData>()
        };
    }

    private static bool ContainsTeam(List<TeamStandingData> teams, string teamId)
    {
        foreach (TeamStandingData team in teams)
        {
            if (team != null && team.TeamId == teamId)
            {
                return true;
            }
        }

        return false;
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

        int goalDifferenceComparison = GetGoalDifference(right).CompareTo(GetGoalDifference(left));
        if (goalDifferenceComparison != 0)
        {
            return goalDifferenceComparison;
        }

        return string.Compare(left.TeamName, right.TeamName, StringComparison.Ordinal);
    }

    private static int GetGoalDifference(TeamStandingData standing)
    {
        return standing.GoalsFor - standing.GoalsAgainst;
    }
}
