using System;
using System.Collections.Generic;
using UnityEngine;

public static class DraftLotteryService
{
    public static List<TeamStandingData> GetNonPlayoffTeamsSorted(SeasonData season)
    {
        List<TeamStandingData> result = new List<TeamStandingData>();
        if (season == null || season.Standings == null)
        {
            return result;
        }

        HashSet<string> playoffTeamIds = GetPlayoffTeamIds(season);
        foreach (TeamStandingData standing in season.Standings)
        {
            if (standing != null && !playoffTeamIds.Contains(standing.TeamId))
            {
                result.Add(standing);
            }
        }

        result.Sort(CompareStandingsWorstToBest);
        return result;
    }

    public static List<TeamData> ApplySimpleLottery(
        GameState state,
        List<TeamData> nonPlayoffTeamsSorted)
    {
        List<TeamData> order = nonPlayoffTeamsSorted == null
            ? new List<TeamData>()
            : new List<TeamData>(nonPlayoffTeamsSorted);

        int lotteryCount = Math.Min(DraftConfig.LotteryTeamCount, order.Count);
        List<TeamData> eligibleTeams = order.GetRange(0, lotteryCount);
        List<TeamData> winners = DrawLotteryWinners(state, eligibleTeams);

        for (int i = 0; i < winners.Count; i++)
        {
            TeamData winner = winners[i];
            int currentIndex = order.IndexOf(winner);
            if (currentIndex < 0)
            {
                continue;
            }

            int targetIndex = Math.Max(0, currentIndex - DraftConfig.MaxLotteryMoveUpSpots);
            order.RemoveAt(currentIndex);
            order.Insert(targetIndex, winner);
        }

        if (winners.Count > 0)
        {
            Debug.Log("Draft lottery: " + BuildWinnerText(winners));
        }

        return order;
    }

    private static HashSet<string> GetPlayoffTeamIds(SeasonData season)
    {
        HashSet<string> teamIds = new HashSet<string>();
        if (season.Playoffs != null && season.Playoffs.Rounds != null && season.Playoffs.Rounds.Count > 0)
        {
            PlayoffRoundData firstRound = season.Playoffs.Rounds[0];
            if (firstRound != null && firstRound.Series != null)
            {
                foreach (PlayoffSeriesData series in firstRound.Series)
                {
                    if (series == null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(series.TeamAId))
                    {
                        teamIds.Add(series.TeamAId);
                    }

                    if (!string.IsNullOrEmpty(series.TeamBId))
                    {
                        teamIds.Add(series.TeamBId);
                    }
                }
            }
        }

        if (teamIds.Count == 0)
        {
            List<TeamStandingData> standings = new List<TeamStandingData>(season.Standings);
            standings.Sort(CompareStandingsBestToWorst);
            int playoffCount = Math.Min(DraftConfig.LotteryTeamCount, standings.Count);
            for (int i = 0; i < playoffCount; i++)
            {
                teamIds.Add(standings[i].TeamId);
            }
        }

        return teamIds;
    }

    private static List<TeamData> DrawLotteryWinners(GameState state, List<TeamData> eligibleTeams)
    {
        List<TeamData> pool = new List<TeamData>(eligibleTeams);
        List<TeamData> winners = new List<TeamData>();
        int drawCount = Math.Min(DraftConfig.LotteryDrawCount, pool.Count);

        for (int draw = 0; draw < drawCount; draw++)
        {
            TeamData winner = null;
            int bestScore = int.MinValue;
            foreach (TeamData team in pool)
            {
                int score = CreateStableScore(state, team, draw);
                if (score > bestScore)
                {
                    bestScore = score;
                    winner = team;
                }
            }

            if (winner != null)
            {
                winners.Add(winner);
                pool.Remove(winner);
            }
        }

        return winners;
    }

    private static int CreateStableScore(GameState state, TeamData team, int draw)
    {
        int draftYear = DraftPickOwnershipService.GetDraftYear(state);
        int hash = draftYear * 397 + draw * 97;
        string key = team == null ? "" : team.Id;
        for (int i = 0; i < key.Length; i++)
        {
            hash = hash * 31 + key[i];
        }

        return Math.Abs(hash);
    }

    private static int CompareStandingsWorstToBest(TeamStandingData left, TeamStandingData right)
    {
        int pointsComparison = left.Points.CompareTo(right.Points);
        if (pointsComparison != 0)
        {
            return pointsComparison;
        }

        int winsComparison = left.Wins.CompareTo(right.Wins);
        if (winsComparison != 0)
        {
            return winsComparison;
        }

        int leftGoalDiff = left.GoalsFor - left.GoalsAgainst;
        int rightGoalDiff = right.GoalsFor - right.GoalsAgainst;
        return leftGoalDiff.CompareTo(rightGoalDiff);
    }

    private static int CompareStandingsBestToWorst(TeamStandingData left, TeamStandingData right)
    {
        int pointsComparison = right.Points.CompareTo(left.Points);
        if (pointsComparison != 0)
        {
            return pointsComparison;
        }

        return right.Wins.CompareTo(left.Wins);
    }

    private static string BuildWinnerText(List<TeamData> winners)
    {
        List<string> names = new List<string>();
        foreach (TeamData team in winners)
        {
            if (team != null)
            {
                names.Add(team.City + " " + team.Name);
            }
        }

        return string.Join(", ", names.ToArray());
    }
}
