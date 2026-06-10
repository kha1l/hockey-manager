public static class IceTimeConfig
{
    public const int SecondsPerMinute = 60;

    public const int ForwardLine1Seconds = 19 * SecondsPerMinute;
    public const int ForwardLine2Seconds = 17 * SecondsPerMinute;
    public const int ForwardLine3Seconds = 14 * SecondsPerMinute;
    public const int ForwardLine4Seconds = 10 * SecondsPerMinute;

    public const int DefensePair1Seconds = 24 * SecondsPerMinute;
    public const int DefensePair2Seconds = 20 * SecondsPerMinute;
    public const int DefensePair3Seconds = 16 * SecondsPerMinute;

    public const int StartingGoalieSeconds = 60 * SecondsPerMinute;
    public const int BackupGoalieSeconds = 0;

    public const int PowerPlayUnit1BonusSeconds = 3 * SecondsPerMinute;
    public const int PowerPlayUnit2BonusSeconds = 2 * SecondsPerMinute;

    public const int PenaltyKillUnit1BonusSeconds = 3 * SecondsPerMinute;
    public const int PenaltyKillUnit2BonusSeconds = 2 * SecondsPerMinute;

    public const int ScratchSeconds = 0;

    public static string FormatSeconds(int seconds)
    {
        if (seconds < 0)
        {
            seconds = 0;
        }

        int minutes = seconds / SecondsPerMinute;
        int remainingSeconds = seconds % SecondsPerMinute;
        return minutes.ToString("00") + ":" + remainingSeconds.ToString("00");
    }
}
