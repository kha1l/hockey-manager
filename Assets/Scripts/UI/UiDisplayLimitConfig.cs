public static class UiDisplayLimitConfig
{
    public const int MaxRosterRows = 80;
    public const int MaxFreeAgentRows = 100;
    public const int MaxDraftProspectRows = 140;
    public const int MaxScoutingRows = 140;
    public const int MaxNewsRows = 80;
    public const int MaxHistoryRows = 50;
    public const int MaxAwardRows = 80;
    public const int MaxRecordRows = 50;
    public const int MaxDiagnosticsIssues = 50;
    public const int MaxAlphaMetrics = 50;
    public const int MaxTradeBlockRows = 50;
    public const int MaxWaiverRows = 50;
    public const int MaxInjuryRows = 50;
    public const int MaxDevelopmentRows = 80;
    public const int MaxCalendarRows = 180;

    public static int ClampRowCount(int requested, int max)
    {
        if (requested <= 0)
        {
            return 0;
        }

        if (max <= 0)
        {
            return requested;
        }

        return requested < max ? requested : max;
    }

    public static string BuildLimitMessage(int shown, int total)
    {
        if (total <= shown)
        {
            return "";
        }

        return "Показано " + shown + " из " + total + ". Используйте фильтры или уточните список.";
    }
}
