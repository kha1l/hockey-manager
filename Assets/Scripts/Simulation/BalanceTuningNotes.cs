public static class BalanceTuningNotes
{
    public const string Notes = "Alpha tuning is report-first: inspect metrics before changing gameplay constants.";

    public static string GetMatchBalanceNotes()
    {
        return "If AverageGoalsPerGame is above target, reduce MatchSimulator shot or goal conversion carefully.";
    }

    public static string GetInjuryBalanceNotes()
    {
        return "If injuries are high, reduce InjuryConfig risk bonuses or improve fatigue recovery.";
    }

    public static string GetContractBalanceNotes()
    {
        return "If cap violations are high, review CPU roster/free agency salary decisions before changing league rules.";
    }

    public static string GetDevelopmentBalanceNotes()
    {
        return "If 90+ players are too common, tune PlayerDevelopment growth and draft class potential.";
    }

    public static string GetOwnerBalanceNotes()
    {
        return "If job security is too harsh, review OwnerGoal and GM job security penalties.";
    }
}
