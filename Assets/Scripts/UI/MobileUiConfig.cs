public static class MobileUiConfig
{
    public const int HeaderFontSize = 30;
    public const int SectionHeaderFontSize = 24;
    public const int BodyFontSize = 18;
    public const int SmallFontSize = 15;
    public const int ButtonFontSize = 18;

    public const float ButtonHeight = 56f;
    public const float SmallButtonHeight = 44f;
    public const float RowHeight = 64f;
    public const float CardMinHeight = 96f;
    public const float PanelPadding = 16f;
    public const float SectionSpacing = 12f;
    public const float RowSpacing = 6f;

    public const int MaxDashboardNewsItems = 3;
    public const int MaxDashboardAlerts = 8;
    public const int MaxRecentItems = 8;

    public static string FormatMoney(int value)
    {
        int absolute = value < 0 ? -value : value;
        string sign = value < 0 ? "-" : "";

        if (absolute >= 1000000)
        {
            int whole = absolute / 1000000;
            int tenths = (absolute % 1000000) / 100000;
            return sign + "$" + whole + (tenths > 0 ? "." + tenths : "") + "M";
        }

        if (absolute >= 1000)
        {
            return sign + "$" + (absolute / 1000) + "K";
        }

        return sign + "$" + absolute;
    }

    public static string FormatOverall(int value)
    {
        return "OVR " + value;
    }

    public static string FormatPercent(int value)
    {
        return value + "%";
    }

    public static string FormatRecord(int wins, int losses, int overtimeLosses)
    {
        return wins + "-" + losses + "-" + overtimeLosses;
    }

    public static string FormatShortStatus(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        if (value.Length <= 14)
        {
            return value;
        }

        return value.Substring(0, 13) + ".";
    }
}
