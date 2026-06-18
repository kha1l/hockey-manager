public static class WaiverConfig
{
    public const int WaiverDurationGameDays = 1;

    public const string WaiverStatusNone = "None";
    public const string WaiverStatusOnWaivers = "OnWaivers";
    public const string WaiverStatusCleared = "Cleared";
    public const string WaiverStatusClaimed = "Claimed";

    public const string WaiverWireStatusActive = "Active";
    public const string WaiverWireStatusCleared = "Cleared";
    public const string WaiverWireStatusClaimed = "Claimed";
    public const string WaiverWireStatusCancelled = "Cancelled";

    public const string DestinationFarm = "Farm";
    public const string DestinationReserve = "Reserve";

    public const int UserClaimMinimumRosterSpace = 1;
    public const int MaxCpuClaimScore = 1000;

    public static bool IsValidDestination(string destination)
    {
        return destination == DestinationFarm || destination == DestinationReserve;
    }

    public static bool IsActiveWaiverStatus(string status)
    {
        return status == WaiverWireStatusActive;
    }
}
