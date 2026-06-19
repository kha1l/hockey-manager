public static class LiveMatchConfig
{
    public const int RegulationPeriods = 3;
    public const int RegulationPeriodSeconds = 20 * 60;
    public const int RegularSeasonOvertimeSeconds = 5 * 60;
    public const int PlayoffOvertimeSeconds = 20 * 60;
    public const int LiveTickGameSeconds = 30;

    public const int RegularSeasonOvertimeSkaters = 3;
    public const int PlayoffOvertimeSkaters = 5;
    public const int RegulationSkaters = 5;
    public const int PulledGoalieSkaters = 6;

    public const int ShootoutInitialRounds = 3;

    public const int OneGoalPullGoalieMinSeconds = 90;
    public const int OneGoalPullGoalieMaxSeconds = 150;
    public const int TwoGoalPullGoalieMinSeconds = 120;
    public const int TwoGoalPullGoalieMaxSeconds = 270;
    public const int ThreeGoalPullGoalieMinSeconds = 270;
    public const int ThreeGoalPullGoalieMaxSeconds = 330;

    public const int MaxEventFeedItems = 100;
    public const int MaxLiveEventsToStore = 300;

    public const float TokenMoveLerpSpeed = 8f;

    public static bool IsRegulationPeriod(int period)
    {
        return period >= 1 && period <= RegulationPeriods;
    }

    public static bool IsRegularSeasonOvertimePeriod(int period)
    {
        return period == 4;
    }

    public static bool IsPlayoffOvertimePeriod(int period)
    {
        return period >= 4;
    }

    public static string FormatClock(int secondsRemaining)
    {
        if (secondsRemaining < 0)
        {
            secondsRemaining = 0;
        }

        int minutes = secondsRemaining / 60;
        int seconds = secondsRemaining % 60;
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public static string FormatPeriodLabel(LiveMatchStateData state)
    {
        if (state == null)
        {
            return "";
        }

        if (state.IsShootout)
        {
            return "SO";
        }

        if (state.Period == 1) return "1st";
        if (state.Period == 2) return "2nd";
        if (state.Period == 3) return "3rd";
        if (!state.IsPlayoffGame) return "OT";

        int overtimeNumber = state.Period - RegulationPeriods;
        return overtimeNumber <= 1 ? "OT" : "OT" + overtimeNumber;
    }
}
