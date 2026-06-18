public static class AwardsConfig
{
    public const string LeagueMvp = "LeagueMvp";
    public const string BestForward = "BestForward";
    public const string BestDefenseman = "BestDefenseman";
    public const string BestGoalie = "BestGoalie";
    public const string BestRookie = "BestRookie";
    public const string TopScorer = "TopScorer";
    public const string BestCoach = "BestCoach";
    public const string PlayoffMvp = "PlayoffMvp";

    public static string GetAwardName(string awardType)
    {
        if (awardType == LeagueMvp)
        {
            return "League MVP";
        }

        if (awardType == BestForward)
        {
            return "Best Forward";
        }

        if (awardType == BestDefenseman)
        {
            return "Best Defenseman";
        }

        if (awardType == BestGoalie)
        {
            return "Best Goalie";
        }

        if (awardType == BestRookie)
        {
            return "Best Rookie";
        }

        if (awardType == TopScorer)
        {
            return "Top Scorer";
        }

        if (awardType == BestCoach)
        {
            return "Best Coach";
        }

        if (awardType == PlayoffMvp)
        {
            return "Playoff MVP";
        }

        return "Unknown Award";
    }

    public static bool IsValidAwardType(string awardType)
    {
        return awardType == LeagueMvp
            || awardType == BestForward
            || awardType == BestDefenseman
            || awardType == BestGoalie
            || awardType == BestRookie
            || awardType == TopScorer
            || awardType == BestCoach
            || awardType == PlayoffMvp;
    }
}
