public static class TradeAiConfig
{
    public const string DirectionContender = "Contender";
    public const string DirectionPlayoffTeam = "PlayoffTeam";
    public const string DirectionBubbleTeam = "BubbleTeam";
    public const string DirectionRetool = "Retool";
    public const string DirectionRebuild = "Rebuild";

    public const int NeedLow = 25;
    public const int NeedMedium = 50;
    public const int NeedHigh = 75;
    public const int NeedCritical = 90;

    public const int CpuAcceptThreshold = 15;
    public const int CpuRejectThreshold = -15;

    public const int MaxTradeBlockPlayers = 12;

    public const int CapPressureHighThreshold = 85;
    public const int RosterPressureHighThreshold = 85;

    public static int ClampScore(int value)
    {
        if (value < 0)
        {
            return 0;
        }

        return value > 100 ? 100 : value;
    }

    public static bool IsForward(PlayerData player)
    {
        return player != null && (player.Position == "C" || player.Position == "LW" || player.Position == "RW");
    }

    public static bool IsDefenseman(PlayerData player)
    {
        return player != null && player.Position == "D";
    }

    public static bool IsGoalie(PlayerData player)
    {
        return player != null && player.Position == "G";
    }

    public static bool IsVeteran(PlayerData player)
    {
        return player != null && player.Age >= 30;
    }

    public static bool IsYoungPlayer(PlayerData player)
    {
        return player != null && player.Age <= 23;
    }

    public static bool IsPendingUfa(PlayerData player)
    {
        if (player == null || player.ContractYearsRemaining > 1)
        {
            return false;
        }

        if (player.ContractStatus == "UFA")
        {
            return true;
        }

        return string.IsNullOrEmpty(player.ContractStatus) && player.Age >= 27;
    }
}
