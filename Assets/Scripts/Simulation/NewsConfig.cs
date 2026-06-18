public static class NewsConfig
{
    public const int MaxNewsItemsToKeep = 300;

    public const string CategorySeasonRecap = "SeasonRecap";
    public const string CategoryAward = "Award";
    public const string CategoryRecord = "Record";
    public const string CategoryTrade = "Trade";
    public const string CategoryInjury = "Injury";
    public const string CategoryContract = "Contract";
    public const string CategoryFreeAgency = "FreeAgency";
    public const string CategoryDraft = "Draft";
    public const string CategoryOwner = "Owner";
    public const string CategoryDevelopment = "Development";
    public const string CategoryMilestone = "Milestone";
    public const string CategoryPlayoffs = "Playoffs";
    public const string CategoryTeam = "Team";

    public static int ClampImportance(int value)
    {
        if (value < 0)
        {
            return 0;
        }

        return value > 100 ? 100 : value;
    }

    public static string GetImportanceLabel(int importance)
    {
        importance = ClampImportance(importance);
        if (importance >= 85)
        {
            return "Major";
        }

        if (importance >= 65)
        {
            return "Important";
        }

        return importance >= 40 ? "Normal" : "Minor";
    }
}
