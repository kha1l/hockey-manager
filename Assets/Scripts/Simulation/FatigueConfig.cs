public static class FatigueConfig
{
    public const int MaxCondition = 100;
    public const int MinCondition = 0;
    public const int MaxFatigue = 100;
    public const int MinFatigue = 0;

    public const int DefaultCondition = 100;
    public const int DefaultFatigue = 0;

    public const int ForwardLine1Fatigue = 7;
    public const int ForwardLine2Fatigue = 6;
    public const int ForwardLine3Fatigue = 5;
    public const int ForwardLine4Fatigue = 4;

    public const int DefensePair1Fatigue = 8;
    public const int DefensePair2Fatigue = 7;
    public const int DefensePair3Fatigue = 6;

    public const int StartingGoalieFatigue = 12;
    public const int BackupGoalieFatigue = 1;

    public const int ScratchRecovery = 8;
    public const int NonPlayingTeamRecovery = 10;
    public const int OffseasonRecovery = 100;

    public const int MaxSingleGameFatigueGain = 18;
    public const int MinSingleGameFatigueGain = 1;

    public static int ClampCondition(int value)
    {
        return Clamp(value, MinCondition, MaxCondition);
    }

    public static int ClampFatigue(int value)
    {
        return Clamp(value, MinFatigue, MaxFatigue);
    }

    private static int Clamp(int value, int minValue, int maxValue)
    {
        if (value < minValue)
        {
            return minValue;
        }

        if (value > maxValue)
        {
            return maxValue;
        }

        return value;
    }
}
