public static class GmJobSecurityConfig
{
    public const int MinSecurity = 0;
    public const int MaxSecurity = 100;
    public const int DefaultJobSecurity = 65;

    public const int SafeThreshold = 65;
    public const int PressureThreshold = 45;
    public const int DangerThreshold = 25;
    public const int FiringThreshold = 15;

    public const int MaxJobOffers = 5;
    public const int JobOfferExpiryDays = 90;
    public const int MaxCareerEventsToKeep = 200;

    public static int ClampSecurity(int value)
    {
        if (value < MinSecurity)
        {
            return MinSecurity;
        }

        return value > MaxSecurity ? MaxSecurity : value;
    }

    public static string GetCareerStatusBySecurity(int security)
    {
        return ClampSecurity(security) >= SafeThreshold ? "Employed" : "UnderPressure";
    }

    public static string GetJobSecurityLabel(int security)
    {
        security = ClampSecurity(security);
        if (security >= 85)
        {
            return "Excellent";
        }

        if (security >= SafeThreshold)
        {
            return "Safe";
        }

        if (security >= PressureThreshold)
        {
            return "Pressure";
        }

        return security >= DangerThreshold ? "Danger" : "Critical";
    }
}
