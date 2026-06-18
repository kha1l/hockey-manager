public static class RosterStatusConfig
{
    public const string NHL = "NHL";
    public const string Farm = "Farm";
    public const string Reserve = "Reserve";
    public const string FreeAgent = "FreeAgent";
    public const string DraftRights = "DraftRights";

    public const int MinNhlRosterSize = 20;
    public const int MaxNhlRosterSize = 23;

    public static bool IsValidRosterStatus(string rosterStatus)
    {
        return rosterStatus == NHL
            || rosterStatus == Farm
            || rosterStatus == Reserve
            || rosterStatus == FreeAgent
            || rosterStatus == DraftRights;
    }

    public static bool IsNhlRoster(PlayerData player)
    {
        return player != null && player.RosterStatus == NHL;
    }

    public static bool IsFarmRoster(PlayerData player)
    {
        return player != null && player.RosterStatus == Farm;
    }

    public static bool IsReserve(PlayerData player)
    {
        return player != null && player.RosterStatus == Reserve;
    }

    public static bool IsInOrganization(PlayerData player)
    {
        return IsNhlRoster(player) || IsFarmRoster(player) || IsReserve(player);
    }
}
