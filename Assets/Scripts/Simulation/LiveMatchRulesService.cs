public static class LiveMatchRulesService
{
    public static bool IsRegulationComplete(LiveMatchStateData state)
    {
        return state != null && state.Period >= LiveMatchConfig.RegulationPeriods && state.PeriodSecondsRemaining <= 0;
    }

    public static bool NeedsRegularSeasonOvertime(LiveMatchStateData state)
    {
        return state != null
            && !state.IsPlayoffGame
            && state.Period == LiveMatchConfig.RegulationPeriods
            && state.PeriodSecondsRemaining <= 0
            && state.HomeScore == state.AwayScore;
    }

    public static bool NeedsPlayoffOvertime(LiveMatchStateData state)
    {
        return state != null
            && state.IsPlayoffGame
            && state.Period >= LiveMatchConfig.RegulationPeriods
            && state.PeriodSecondsRemaining <= 0
            && state.HomeScore == state.AwayScore;
    }

    public static bool NeedsShootout(LiveMatchStateData state)
    {
        return state != null
            && !state.IsPlayoffGame
            && state.Period == 4
            && state.PeriodSecondsRemaining <= 0
            && state.HomeScore == state.AwayScore;
    }

    public static void AdvanceToNextPeriodOrPhase(LiveMatchStateData state)
    {
        if (state == null || state.IsCompleted)
        {
            return;
        }

        if (state.HomeScore != state.AwayScore && state.Period >= LiveMatchConfig.RegulationPeriods)
        {
            CompleteByScore(state, "Regulation/OT complete");
            return;
        }

        if (NeedsShootout(state))
        {
            state.IsShootout = true;
            state.IsOvertime = false;
            state.IsSuddenDeath = true;
            state.PeriodSecondsRemaining = 0;
            return;
        }

        state.Period++;
        state.IsOvertime = state.Period > LiveMatchConfig.RegulationPeriods;
        state.IsSuddenDeath = state.IsOvertime;
        state.PeriodSecondsRemaining = state.IsPlayoffGame && state.IsOvertime
            ? LiveMatchConfig.PlayoffOvertimeSeconds
            : (state.IsOvertime ? LiveMatchConfig.RegularSeasonOvertimeSeconds : LiveMatchConfig.RegulationPeriodSeconds);
    }

    public static bool IsGameComplete(LiveMatchStateData state)
    {
        return state != null && state.IsCompleted;
    }

    public static bool IsSuddenDeathGoal(LiveMatchStateData state)
    {
        return state != null && state.IsOvertime && state.HomeScore != state.AwayScore;
    }

    public static int GetCurrentSkaterCount(LiveMatchStateData state, bool forHomeTeam)
    {
        if (state == null)
        {
            return LiveMatchConfig.RegulationSkaters;
        }

        LiveMatchTeamStatsData stats = forHomeTeam ? state.HomeStats : state.AwayStats;
        if (stats != null && stats.IsGoaliePulled)
        {
            return LiveMatchConfig.PulledGoalieSkaters;
        }

        if (state.IsOvertime)
        {
            return state.IsPlayoffGame
                ? LiveMatchConfig.PlayoffOvertimeSkaters
                : LiveMatchConfig.RegularSeasonOvertimeSkaters;
        }

        return LiveMatchConfig.RegulationSkaters;
    }

    public static void CompleteByScore(LiveMatchStateData state, string reason)
    {
        if (state == null)
        {
            return;
        }

        state.IsCompleted = true;
        state.IsActive = false;
        state.IsPaused = true;
        state.WinnerTeamId = state.HomeScore >= state.AwayScore ? state.HomeTeamId : state.AwayTeamId;
        state.WinnerTeamName = state.WinnerTeamId == state.HomeTeamId ? state.HomeTeamName : state.AwayTeamName;
        state.CompletionReason = reason;
        state.CompletedAtUtc = System.DateTime.UtcNow.ToString("o");
    }
}
