public static class ProspectRiskConfig
{
    public const string DevelopmentTypeSafe = "Safe";
    public const string DevelopmentTypeBoomBust = "BoomBust";
    public const string DevelopmentTypeLateBloomer = "LateBloomer";
    public const string DevelopmentTypeHighFloor = "HighFloor";
    public const string DevelopmentTypeRawTalent = "RawTalent";

    public const int MinRisk = 0;
    public const int MaxRisk = 100;

    public const int MinBoomChance = 0;
    public const int MaxBoomChance = 50;

    public const int MinBustChance = 0;
    public const int MaxBustChance = 50;

    public const int MinCeiling = 50;
    public const int MaxCeiling = 99;

    public const int MinFloor = 40;
    public const int MaxFloor = 95;

    public static int ClampRisk(int value)
    {
        return Clamp(value, MinRisk, MaxRisk);
    }

    public static int ClampChance(int value)
    {
        return Clamp(value, MinBoomChance, MaxBoomChance);
    }

    public static int ClampCeiling(int value)
    {
        return Clamp(value, MinCeiling, MaxCeiling);
    }

    public static int ClampFloor(int value)
    {
        return Clamp(value, MinFloor, MaxFloor);
    }

    public static bool IsValidDevelopmentType(string developmentType)
    {
        return developmentType == DevelopmentTypeSafe
            || developmentType == DevelopmentTypeBoomBust
            || developmentType == DevelopmentTypeLateBloomer
            || developmentType == DevelopmentTypeHighFloor
            || developmentType == DevelopmentTypeRawTalent;
    }

    public static string GetRiskHint(int risk)
    {
        if (risk <= 20)
        {
            return "Low Risk";
        }

        if (risk <= 45)
        {
            return "Medium Risk";
        }

        if (risk <= 70)
        {
            return "High Risk";
        }

        return "Very High Risk";
    }

    public static string GetCeilingHint(int ceiling)
    {
        if (ceiling >= 90)
        {
            return "Star upside";
        }

        if (ceiling >= 85)
        {
            return "Top-line upside";
        }

        if (ceiling >= 80)
        {
            return "Regular pro upside";
        }

        if (ceiling >= 74)
        {
            return "Depth pro upside";
        }

        return "Limited upside";
    }

    public static string GetFloorHint(int floor)
    {
        if (floor >= 80)
        {
            return "High floor";
        }

        if (floor >= 72)
        {
            return "Likely pro contributor";
        }

        if (floor >= 65)
        {
            return "Depth floor";
        }

        if (floor >= 58)
        {
            return "Replacement risk";
        }

        return "Bust risk";
    }

    public static string GetDevelopmentTypeHint(string developmentType)
    {
        if (developmentType == DevelopmentTypeBoomBust)
        {
            return "Boom/Bust profile";
        }

        if (developmentType == DevelopmentTypeLateBloomer)
        {
            return "Late bloomer";
        }

        if (developmentType == DevelopmentTypeHighFloor)
        {
            return "High floor prospect";
        }

        if (developmentType == DevelopmentTypeRawTalent)
        {
            return "Raw but talented";
        }

        return "Safe projection";
    }

    private static int Clamp(int value, int minValue, int maxValue)
    {
        if (value < minValue)
        {
            return minValue;
        }

        return value > maxValue ? maxValue : value;
    }
}
