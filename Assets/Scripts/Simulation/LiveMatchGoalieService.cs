using UnityEngine;

public static class LiveMatchGoalieService
{
    public static PlayerData GetCurrentGoalie(LiveMatchStateData match, TeamData team)
    {
        LiveMatchTeamStatsData stats = GetStats(match, team);
        PlayerData goalie = FindPlayer(team, stats == null ? "" : stats.CurrentGoaliePlayerId);
        if (goalie != null)
        {
            return goalie;
        }

        return LineupService.GetStartingGoalie(team);
    }

    public static PlayerData GetBackupGoalie(TeamData team, string currentGoalieId)
    {
        PlayerData backup = LineupService.GetBackupGoalie(team);
        if (backup != null && backup.Id != currentGoalieId && InjuryService.IsPlayerAvailable(backup))
        {
            return backup;
        }

        PlayerData starter = LineupService.GetStartingGoalie(team);
        if (starter != null && starter.Id != currentGoalieId && InjuryService.IsPlayerAvailable(starter))
        {
            return starter;
        }

        return null;
    }

    public static bool ChangeGoalie(LiveMatchStateData match, TeamData team, out string message)
    {
        message = "";
        LiveMatchTeamStatsData stats = GetStats(match, team);
        if (match == null || team == null || stats == null)
        {
            message = "Матч или команда не найдены";
            return false;
        }

        if (stats.IsGoaliePulled)
        {
            message = "Верните вратаря на лёд перед заменой";
            return false;
        }

        PlayerData currentGoalie = GetCurrentGoalie(match, team);
        PlayerData backupGoalie = GetBackupGoalie(team, currentGoalie == null ? "" : currentGoalie.Id);
        if (backupGoalie == null)
        {
            message = "Нет доступного запасного вратаря";
            return false;
        }

        stats.CurrentGoaliePlayerId = backupGoalie.Id;
        stats.CurrentGoalieName = GetPlayerName(backupGoalie);
        MarkGoalieFinished(match, currentGoalie, false);
        MarkGoalieStarted(match, backupGoalie, true);
        LiveMatchSimulator.AddEvent(match, LiveMatchSimulator.CreateEvent(
            match,
            "GoalieChange",
            team,
            backupGoalie,
            TeamIdentityService.GetDisplayName(team) + ": замена вратаря, выходит " + GetPlayerName(backupGoalie),
            2));
        message = "Вратарь заменён";
        return true;
    }

    public static bool PullGoalie(LiveMatchStateData match, TeamData team, out string message)
    {
        message = "";
        LiveMatchTeamStatsData stats = GetStats(match, team);
        if (!CanPullGoalie(match, team, out message) || stats == null)
        {
            return false;
        }

        stats.IsGoaliePulled = true;
        LiveMatchSimulator.AddEvent(match, LiveMatchSimulator.CreateEvent(
            match,
            "GoaliePulled",
            team,
            GetCurrentGoalie(match, team),
            TeamIdentityService.GetDisplayName(team) + ": вратарь снят, шестой полевой",
            3));
        message = "Вратарь снят";
        return true;
    }

    public static bool ReturnGoalie(LiveMatchStateData match, TeamData team, out string message)
    {
        message = "";
        LiveMatchTeamStatsData stats = GetStats(match, team);
        if (match == null || team == null || stats == null || !stats.IsGoaliePulled)
        {
            message = "Вратарь уже на льду";
            return false;
        }

        stats.IsGoaliePulled = false;
        LiveMatchSimulator.AddEvent(match, LiveMatchSimulator.CreateEvent(
            match,
            "GoalieReturned",
            team,
            GetCurrentGoalie(match, team),
            TeamIdentityService.GetDisplayName(team) + ": вратарь вернулся на лёд",
            2));
        message = "Вратарь возвращён";
        return true;
    }

    public static bool ShouldAutoPullGoalie(LiveMatchStateData match, TeamData team)
    {
        if (match == null || team == null || match.IsShootout || match.IsCompleted || match.Period < 3)
        {
            return false;
        }

        int diff = GetScoreDiff(match, team);
        if (diff >= 0)
        {
            return false;
        }

        if (diff <= -4)
        {
            return false;
        }

        if (diff == -3)
        {
            return match.PeriodSecondsRemaining <= GetStablePullThresholdSeconds(
                match,
                team,
                3,
                LiveMatchConfig.ThreeGoalPullGoalieMinSeconds,
                LiveMatchConfig.ThreeGoalPullGoalieMaxSeconds);
        }

        if (diff == -2)
        {
            return match.PeriodSecondsRemaining <= GetStablePullThresholdSeconds(
                match,
                team,
                2,
                LiveMatchConfig.TwoGoalPullGoalieMinSeconds,
                LiveMatchConfig.TwoGoalPullGoalieMaxSeconds);
        }

        return match.PeriodSecondsRemaining <= GetStablePullThresholdSeconds(
            match,
            team,
            1,
            LiveMatchConfig.OneGoalPullGoalieMinSeconds,
            LiveMatchConfig.OneGoalPullGoalieMaxSeconds);
    }

    public static bool ShouldAutoReturnGoalie(LiveMatchStateData match, TeamData team)
    {
        LiveMatchTeamStatsData stats = GetStats(match, team);
        if (match == null || team == null || stats == null || !stats.IsGoaliePulled || match.IsCompleted)
        {
            return false;
        }

        return GetScoreDiff(match, team) >= 0;
    }

    private static bool CanPullGoalie(LiveMatchStateData match, TeamData team, out string message)
    {
        message = "";
        LiveMatchTeamStatsData stats = GetStats(match, team);
        if (match == null || team == null || stats == null)
        {
            message = "Матч или команда не найдены";
            return false;
        }

        if (stats.IsGoaliePulled)
        {
            message = "Вратарь уже снят";
            return false;
        }

        if (match.IsShootout || match.IsCompleted)
        {
            message = "В этой фазе матча нельзя снять вратаря";
            return false;
        }

        if (GetScoreDiff(match, team) >= 0)
        {
            message = "Снимать вратаря можно только когда команда уступает";
            return false;
        }

        if (match.Period < 3)
        {
            message = "Снимать вратаря рано";
            return false;
        }

        return true;
    }

    private static void MarkGoalieStarted(LiveMatchStateData match, PlayerData goalie, bool finished)
    {
        LiveMatchPlayerStatData stat = LiveMatchSimulator.GetOrCreatePlayerStat(match, goalie);
        if (stat != null)
        {
            stat.IsGoalie = true;
            stat.StartedGame = true;
            stat.FinishedGame = finished;
        }
    }

    private static void MarkGoalieFinished(LiveMatchStateData match, PlayerData goalie, bool finished)
    {
        LiveMatchPlayerStatData stat = LiveMatchSimulator.GetOrCreatePlayerStat(match, goalie);
        if (stat != null)
        {
            stat.FinishedGame = finished;
        }
    }

    private static int GetScoreDiff(LiveMatchStateData match, TeamData team)
    {
        if (match.HomeTeamId == team.Id)
        {
            return match.HomeScore - match.AwayScore;
        }

        return match.AwayScore - match.HomeScore;
    }

    private static int GetStablePullThresholdSeconds(
        LiveMatchStateData match,
        TeamData team,
        int deficit,
        int minSeconds,
        int maxSeconds)
    {
        if (maxSeconds < minSeconds)
        {
            int swap = maxSeconds;
            maxSeconds = minSeconds;
            minSeconds = swap;
        }

        int range = maxSeconds - minSeconds + 1;
        string key = (match == null ? "" : match.LiveMatchId)
            + ":"
            + (team == null ? "" : team.Id)
            + ":pull:"
            + deficit;
        return minSeconds + StableRange(key, range);
    }

    private static int StableRange(string key, int range)
    {
        if (range <= 1)
        {
            return 0;
        }

        unchecked
        {
            uint hash = 2166136261u;
            string source = string.IsNullOrEmpty(key) ? "goalie-pull" : key;
            for (int i = 0; i < source.Length; i++)
            {
                hash ^= source[i];
                hash *= 16777619u;
            }

            return (int)(hash % (uint)range);
        }
    }

    private static LiveMatchTeamStatsData GetStats(LiveMatchStateData match, TeamData team)
    {
        if (match == null || team == null)
        {
            return null;
        }

        return match.HomeTeamId == team.Id ? match.HomeStats : match.AwayTeamId == team.Id ? match.AwayStats : null;
    }

    private static PlayerData FindPlayer(TeamData team, string playerId)
    {
        if (team == null || team.Players == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : player.FirstName + " " + player.LastName;
    }
}
