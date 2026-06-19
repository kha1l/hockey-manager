using System;
using System.Collections.Generic;
using UnityEngine;

public static class SeasonGenerator
{
    public const int CurrentScheduleVersion = 6;
    public const int TargetGamesPerTeam = SalaryCapConfig.TargetGamesPerTeam;
    private const int MaxGamesPerDay = 12;
    private const int FallbackRegularSeasonCalendarDays = 203;

    public static SeasonData CreateSimpleSeason(string selectedTeamId, List<TeamData> teams)
    {
        return CreateSimpleSeason(selectedTeamId, teams, SalaryCapConfig.RulesSeasonStartYear);
    }

    public static SeasonData CreateSimpleSeason(string selectedTeamId, List<TeamData> teams, int seasonStartYear)
    {
        SeasonData season = new SeasonData
        {
            SeasonYear = seasonStartYear,
            ScheduleVersion = CurrentScheduleVersion,
            TargetGamesPerTeam = TargetGamesPerTeam,
            CurrentDay = 1,
            CurrentGameIndex = 0,
            IsSeasonFinished = false
        };

        List<TeamData> sortedTeams = GetSortedTeams(teams);
        StandingsService.EnsureStandings(season, sortedTeams);

        Dictionary<string, int> homeGamesByTeam = CreateCounter(sortedTeams);
        int gameNumber = 1;

        for (int i = 0; i < sortedTeams.Count; i++)
        {
            for (int j = i + 1; j < sortedTeams.Count; j++)
            {
                TeamData firstTeam = sortedTeams[i];
                TeamData secondTeam = sortedTeams[j];
                int gamesBetweenTeams = GetGamesBetweenTeams(firstTeam, secondTeam, sortedTeams);

                AddPairGames(season, firstTeam, secondTeam, gamesBetweenTeams, homeGamesByTeam, ref gameNumber);
            }
        }

        AssignGameDays(season);
        ValidateSchedule(season, sortedTeams);

        return season;
    }

    private static void AddPairGames(
        SeasonData season,
        TeamData firstTeam,
        TeamData secondTeam,
        int gamesBetweenTeams,
        Dictionary<string, int> homeGamesByTeam,
        ref int gameNumber)
    {
        if (gamesBetweenTeams == 2)
        {
            AddGame(season, firstTeam, secondTeam, homeGamesByTeam, ref gameNumber);
            AddGame(season, secondTeam, firstTeam, homeGamesByTeam, ref gameNumber);
            return;
        }

        if (gamesBetweenTeams == 3)
        {
            AddGame(season, firstTeam, secondTeam, homeGamesByTeam, ref gameNumber);
            AddGame(season, secondTeam, firstTeam, homeGamesByTeam, ref gameNumber);

            TeamData extraHomeTeam = homeGamesByTeam[firstTeam.Id] <= homeGamesByTeam[secondTeam.Id]
                ? firstTeam
                : secondTeam;
            TeamData extraAwayTeam = extraHomeTeam == firstTeam ? secondTeam : firstTeam;

            AddGame(season, extraHomeTeam, extraAwayTeam, homeGamesByTeam, ref gameNumber);
            return;
        }

        AddGame(season, firstTeam, secondTeam, homeGamesByTeam, ref gameNumber);
        AddGame(season, secondTeam, firstTeam, homeGamesByTeam, ref gameNumber);
        AddGame(season, firstTeam, secondTeam, homeGamesByTeam, ref gameNumber);
        AddGame(season, secondTeam, firstTeam, homeGamesByTeam, ref gameNumber);
    }

    private static void AddGame(
        SeasonData season,
        TeamData homeTeam,
        TeamData awayTeam,
        Dictionary<string, int> homeGamesByTeam,
        ref int gameNumber)
    {
        season.Schedule.Add(new ScheduleGameData
        {
            GameId = Guid.NewGuid().ToString("N"),
            GameNumber = gameNumber,
            DayNumber = 0,
            HomeTeamId = homeTeam.Id,
            AwayTeamId = awayTeam.Id,
            HomeTeamName = GetTeamDisplayName(homeTeam),
            AwayTeamName = GetTeamDisplayName(awayTeam),
            IsPlayed = false,
            Result = null
        });

        homeGamesByTeam[homeTeam.Id]++;
        gameNumber++;
    }

    private static int GetGamesBetweenTeams(TeamData firstTeam, TeamData secondTeam, List<TeamData> teams)
    {
        if (!TeamStructureService.IsSameConference(firstTeam.Id, secondTeam.Id))
        {
            return 2;
        }

        if (TeamStructureService.IsSameDivision(firstTeam.Id, secondTeam.Id))
        {
            return IsThreeGameDivisionPair(firstTeam.Id, secondTeam.Id, teams) ? 3 : 4;
        }

        return IsFourGameConferencePair(firstTeam.Id, secondTeam.Id, teams) ? 4 : 3;
    }

    private static bool IsThreeGameDivisionPair(string firstTeamId, string secondTeamId, List<TeamData> teams)
    {
        string division = TeamStructureService.GetDivision(firstTeamId);
        List<string> divisionTeamIds = GetDivisionTeamIds(teams, division);
        int firstIndex = divisionTeamIds.IndexOf(firstTeamId);
        int secondIndex = divisionTeamIds.IndexOf(secondTeamId);

        if (firstIndex < 0 || secondIndex < 0)
        {
            return false;
        }

        int distance = Math.Abs(firstIndex - secondIndex);
        return distance == 1 || distance == divisionTeamIds.Count - 1;
    }

    private static bool IsFourGameConferencePair(string firstTeamId, string secondTeamId, List<TeamData> teams)
    {
        string firstDivision = TeamStructureService.GetDivision(firstTeamId);
        string secondDivision = TeamStructureService.GetDivision(secondTeamId);

        if (string.Compare(firstDivision, secondDivision, StringComparison.Ordinal) > 0)
        {
            string tempTeamId = firstTeamId;
            firstTeamId = secondTeamId;
            secondTeamId = tempTeamId;

            string tempDivision = firstDivision;
            firstDivision = secondDivision;
            secondDivision = tempDivision;
        }

        List<string> firstDivisionTeams = GetDivisionTeamIds(teams, firstDivision);
        List<string> secondDivisionTeams = GetDivisionTeamIds(teams, secondDivision);
        int firstIndex = firstDivisionTeams.IndexOf(firstTeamId);
        int secondIndex = secondDivisionTeams.IndexOf(secondTeamId);

        if (firstIndex < 0 || secondIndex < 0 || firstDivisionTeams.Count == 0)
        {
            return false;
        }

        return secondIndex == firstIndex || secondIndex == (firstIndex + 1) % firstDivisionTeams.Count;
    }

    private static void AssignGameDays(SeasonData season)
    {
        SortScheduleForSpacing(season);

        Dictionary<int, HashSet<string>> teamsByDay = new Dictionary<int, HashSet<string>>();
        Dictionary<int, int> gamesByDay = new Dictionary<int, int>();
        Dictionary<string, HashSet<int>> daysByTeam = new Dictionary<string, HashSet<int>>();
        int calendarDays = GetRegularSeasonCalendarDayCount();
        int scheduleCount = season == null || season.Schedule == null ? 0 : season.Schedule.Count;

        for (int i = 0; i < scheduleCount; i++)
        {
            ScheduleGameData game = season.Schedule[i];
            int targetDay = GetTargetDayForSlot(i, scheduleCount, calendarDays);
            int dayNumber = FindBestDayForGame(game, targetDay, calendarDays, teamsByDay, gamesByDay, daysByTeam);

            game.DayNumber = Mathf.Max(1, dayNumber);

            if (!teamsByDay.ContainsKey(dayNumber))
            {
                teamsByDay[dayNumber] = new HashSet<string>();
                gamesByDay[dayNumber] = 0;
            }

            teamsByDay[dayNumber].Add(game.HomeTeamId);
            teamsByDay[dayNumber].Add(game.AwayTeamId);
            gamesByDay[dayNumber]++;
            AddTeamDay(daysByTeam, game.HomeTeamId, dayNumber);
            AddTeamDay(daysByTeam, game.AwayTeamId, dayNumber);
        }

        RenumberSchedule(season);
    }

    private static int GetRegularSeasonCalendarDayCount()
    {
        LeagueCalendarData calendar = LeagueCalendarConfig.CreateDefaultCalendar();
        if (calendar == null
            || !DateTime.TryParse(calendar.RegularSeasonStartDate, out DateTime startDate)
            || !DateTime.TryParse(calendar.RegularSeasonEndDate, out DateTime endDate)
            || endDate.Date < startDate.Date)
        {
            return FallbackRegularSeasonCalendarDays;
        }

        return Mathf.Max(1, (int)(endDate.Date - startDate.Date).TotalDays + 1);
    }

    private static int GetTargetDayForSlot(int slotIndex, int scheduleCount, int calendarDays)
    {
        if (scheduleCount <= 1 || calendarDays <= 1)
        {
            return 1;
        }

        float progress = slotIndex / (float)(scheduleCount - 1);
        return Mathf.Clamp(1 + Mathf.RoundToInt((calendarDays - 1) * progress), 1, calendarDays);
    }

    private static int FindBestDayForGame(
        ScheduleGameData game,
        int targetDay,
        int calendarDays,
        Dictionary<int, HashSet<string>> teamsByDay,
        Dictionary<int, int> gamesByDay,
        Dictionary<string, HashSet<int>> daysByTeam)
    {
        int day = FindBestDayForGame(game, targetDay, calendarDays, teamsByDay, gamesByDay, daysByTeam, true);
        if (day > 0)
        {
            return day;
        }

        day = FindBestDayForGame(game, targetDay, calendarDays, teamsByDay, gamesByDay, daysByTeam, false);
        if (day > 0)
        {
            return day;
        }

        day = calendarDays + 1;
        while (!CanPlaceGameOnDay(game, day, int.MaxValue, teamsByDay, gamesByDay, daysByTeam, false))
        {
            day++;
        }

        return day;
    }

    private static int FindBestDayForGame(
        ScheduleGameData game,
        int targetDay,
        int calendarDays,
        Dictionary<int, HashSet<string>> teamsByDay,
        Dictionary<int, int> gamesByDay,
        Dictionary<string, HashSet<int>> daysByTeam,
        bool avoidBackToBack)
    {
        for (int radius = 0; radius < calendarDays; radius++)
        {
            int forwardDay = targetDay + radius;
            if (CanPlaceGameOnDay(game, forwardDay, calendarDays, teamsByDay, gamesByDay, daysByTeam, avoidBackToBack))
            {
                return forwardDay;
            }

            int backwardDay = targetDay - radius;
            if (radius > 0 && CanPlaceGameOnDay(game, backwardDay, calendarDays, teamsByDay, gamesByDay, daysByTeam, avoidBackToBack))
            {
                return backwardDay;
            }
        }

        return 0;
    }

    private static void SortScheduleForSpacing(SeasonData season)
    {
        if (season == null || season.Schedule == null || season.Schedule.Count <= 1)
        {
            return;
        }

        List<ScheduleGameData> remaining = new List<ScheduleGameData>(season.Schedule);
        List<ScheduleGameData> ordered = new List<ScheduleGameData>();
        Dictionary<string, string> lastOpponentByTeam = new Dictionary<string, string>();
        int slotIndex = 0;

        while (remaining.Count > 0)
        {
            int bestIndex = 0;
            int bestScore = int.MaxValue;
            int bestTieBreaker = int.MaxValue;

            for (int i = 0; i < remaining.Count; i++)
            {
                ScheduleGameData game = remaining[i];
                int score = GetSpacingScore(game, lastOpponentByTeam);
                int tieBreaker = GetStableScheduleTieBreaker(game, slotIndex);
                if (score < bestScore || (score == bestScore && tieBreaker < bestTieBreaker))
                {
                    bestScore = score;
                    bestTieBreaker = tieBreaker;
                    bestIndex = i;
                }
            }

            ScheduleGameData selectedGame = remaining[bestIndex];
            remaining.RemoveAt(bestIndex);
            ordered.Add(selectedGame);
            if (selectedGame != null)
            {
                lastOpponentByTeam[selectedGame.HomeTeamId] = selectedGame.AwayTeamId;
                lastOpponentByTeam[selectedGame.AwayTeamId] = selectedGame.HomeTeamId;
            }

            slotIndex++;
        }

        season.Schedule.Clear();
        season.Schedule.AddRange(ordered);
    }

    private static int GetSpacingScore(ScheduleGameData game, Dictionary<string, string> lastOpponentByTeam)
    {
        if (game == null || lastOpponentByTeam == null)
        {
            return 100;
        }

        int score = 0;
        if (lastOpponentByTeam.TryGetValue(game.HomeTeamId, out string homeLastOpponent)
            && homeLastOpponent == game.AwayTeamId)
        {
            score += 1000;
        }

        if (lastOpponentByTeam.TryGetValue(game.AwayTeamId, out string awayLastOpponent)
            && awayLastOpponent == game.HomeTeamId)
        {
            score += 1000;
        }

        return score;
    }

    private static int GetStableScheduleTieBreaker(ScheduleGameData game, int slotIndex)
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 31) + StableHash(game == null ? "" : game.HomeTeamId);
            hash = (hash * 31) + StableHash(game == null ? "" : game.AwayTeamId);
            hash = (hash * 31) + (game == null ? 0 : game.GameNumber);
            hash = (hash * 31) + slotIndex;
            return hash == int.MinValue ? int.MaxValue : Math.Abs(hash);
        }
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 23;
            if (!string.IsNullOrEmpty(value))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    hash = (hash * 31) + value[i];
                }
            }

            return hash;
        }
    }

    private static void RenumberSchedule(SeasonData season)
    {
        if (season == null || season.Schedule == null)
        {
            return;
        }

        season.Schedule.Sort(CompareGamesByDay);
        for (int i = 0; i < season.Schedule.Count; i++)
        {
            if (season.Schedule[i] != null)
            {
                season.Schedule[i].GameNumber = i + 1;
            }
        }
    }

    private static int CompareGamesByDay(ScheduleGameData left, ScheduleGameData right)
    {
        int dayComparison = (left == null ? 0 : left.DayNumber).CompareTo(right == null ? 0 : right.DayNumber);
        if (dayComparison != 0)
        {
            return dayComparison;
        }

        return string.Compare(
            left == null ? "" : left.GameId,
            right == null ? "" : right.GameId,
            StringComparison.Ordinal);
    }

    private static bool CanPlaceGameOnDay(
        ScheduleGameData game,
        int dayNumber,
        Dictionary<int, HashSet<string>> teamsByDay,
        Dictionary<int, int> gamesByDay)
    {
        return CanPlaceGameOnDay(game, dayNumber, int.MaxValue, teamsByDay, gamesByDay, null, false);
    }

    private static bool CanPlaceGameOnDay(
        ScheduleGameData game,
        int dayNumber,
        int maxDayNumber,
        Dictionary<int, HashSet<string>> teamsByDay,
        Dictionary<int, int> gamesByDay,
        Dictionary<string, HashSet<int>> daysByTeam,
        bool avoidBackToBack)
    {
        if (game == null || dayNumber <= 0 || dayNumber > maxDayNumber)
        {
            return false;
        }

        if (teamsByDay.ContainsKey(dayNumber))
        {
            if (gamesByDay[dayNumber] >= MaxGamesPerDay
                || teamsByDay[dayNumber].Contains(game.HomeTeamId)
                || teamsByDay[dayNumber].Contains(game.AwayTeamId))
            {
                return false;
            }
        }

        if (avoidBackToBack && HasAdjacentTeamGame(daysByTeam, game.HomeTeamId, dayNumber))
        {
            return false;
        }

        if (avoidBackToBack && HasAdjacentTeamGame(daysByTeam, game.AwayTeamId, dayNumber))
        {
            return false;
        }

        return true;
    }

    private static bool HasAdjacentTeamGame(Dictionary<string, HashSet<int>> daysByTeam, string teamId, int dayNumber)
    {
        if (daysByTeam == null || string.IsNullOrEmpty(teamId) || !daysByTeam.TryGetValue(teamId, out HashSet<int> days))
        {
            return false;
        }

        return days.Contains(dayNumber - 1) || days.Contains(dayNumber + 1);
    }

    private static void AddTeamDay(Dictionary<string, HashSet<int>> daysByTeam, string teamId, int dayNumber)
    {
        if (daysByTeam == null || string.IsNullOrEmpty(teamId))
        {
            return;
        }

        if (!daysByTeam.ContainsKey(teamId))
        {
            daysByTeam[teamId] = new HashSet<int>();
        }

        daysByTeam[teamId].Add(dayNumber);
    }

    private static void ValidateSchedule(SeasonData season, List<TeamData> teams)
    {
        int expectedGames = teams.Count * TargetGamesPerTeam / 2;
        if (season.Schedule.Count != expectedGames)
        {
            Debug.LogError("Календарь содержит " + season.Schedule.Count + " матчей вместо " + expectedGames);
        }

        Dictionary<string, int> gamesByTeam = CreateCounter(teams);
        Dictionary<string, int> homeGamesByTeam = CreateCounter(teams);
        Dictionary<string, int> awayGamesByTeam = CreateCounter(teams);
        Dictionary<int, HashSet<string>> teamsByDay = new Dictionary<int, HashSet<string>>();
        int maxDayNumber = 0;

        foreach (ScheduleGameData game in season.Schedule)
        {
            gamesByTeam[game.HomeTeamId]++;
            gamesByTeam[game.AwayTeamId]++;
            homeGamesByTeam[game.HomeTeamId]++;
            awayGamesByTeam[game.AwayTeamId]++;
            maxDayNumber = Mathf.Max(maxDayNumber, game.DayNumber);

            if (!teamsByDay.ContainsKey(game.DayNumber))
            {
                teamsByDay[game.DayNumber] = new HashSet<string>();
            }

            if (!teamsByDay[game.DayNumber].Add(game.HomeTeamId))
            {
                Debug.LogError("Команда играет дважды в день " + game.DayNumber + ": " + game.HomeTeamId);
            }

            if (!teamsByDay[game.DayNumber].Add(game.AwayTeamId))
            {
                Debug.LogError("Команда играет дважды в день " + game.DayNumber + ": " + game.AwayTeamId);
            }
        }

        bool allTeamsHaveTargetGames = true;
        int imbalancedHomeAwayTeams = 0;
        foreach (TeamData team in teams)
        {
            if (gamesByTeam[team.Id] != TargetGamesPerTeam)
            {
                Debug.LogError(team.Id + " имеет " + gamesByTeam[team.Id] + " матчей вместо " + TargetGamesPerTeam);
                allTeamsHaveTargetGames = false;
            }

            if (homeGamesByTeam[team.Id] != TargetGamesPerTeam / 2 || awayGamesByTeam[team.Id] != TargetGamesPerTeam / 2)
            {
                imbalancedHomeAwayTeams++;
            }
        }

        Debug.Log("Создан календарь: " + season.Schedule.Count + " матчей, " + maxDayNumber + " игровых дней");
        if (imbalancedHomeAwayTeams > 0)
        {
            Debug.Log("Календарь: home/away баланс не идеально ровный у " + imbalancedHomeAwayTeams + " команд");
        }

        if (allTeamsHaveTargetGames)
        {
            Debug.Log("Каждая команда имеет " + TargetGamesPerTeam + " матча");
        }
    }

    private static List<TeamData> GetSortedTeams(List<TeamData> teams)
    {
        List<TeamData> sortedTeams = new List<TeamData>();

        if (teams != null)
        {
            foreach (TeamData team in teams)
            {
                if (team != null)
                {
                    sortedTeams.Add(team);
                }
            }
        }

        sortedTeams.Sort((left, right) => string.Compare(left.Id, right.Id, StringComparison.Ordinal));
        return sortedTeams;
    }

    private static List<string> GetDivisionTeamIds(List<TeamData> teams, string division)
    {
        List<string> teamIds = new List<string>();

        foreach (TeamData team in teams)
        {
            if (team != null && TeamStructureService.GetDivision(team.Id) == division)
            {
                teamIds.Add(team.Id);
            }
        }

        teamIds.Sort(StringComparer.Ordinal);
        return teamIds;
    }

    private static Dictionary<string, int> CreateCounter(List<TeamData> teams)
    {
        Dictionary<string, int> counter = new Dictionary<string, int>();

        foreach (TeamData team in teams)
        {
            if (team != null)
            {
                counter[team.Id] = 0;
            }
        }

        return counter;
    }

    private static string GetTeamDisplayName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
