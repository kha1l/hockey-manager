using System.Globalization;

public static class ContractExtensionConfig
{
    public const int MinInterest = 0;
    public const int MaxInterest = 100;

    public const int HighInterestThreshold = 75;
    public const int MediumInterestThreshold = 50;
    public const int LowInterestThreshold = 30;

    public const int AcceptanceThreshold = 60;

    public const int MaxOffersToKeep = 300;

    public const int LeagueMinimumSalary = 850000;
    public const int MaxPlayerSalary = 20800000;
    public const int MaxOwnTeamYears = 7;
    public const int MaxFreeAgentYears = 6;

    public const int MinExtensionYears = 1;

    public static int ClampInterest(int value)
    {
        if (value < MinInterest)
        {
            return MinInterest;
        }

        return value > MaxInterest ? MaxInterest : value;
    }

    public static int ClampSalary(int salary)
    {
        return ClampSalary(salary, null);
    }

    public static int ClampSalary(int salary, LeagueRulesData rules)
    {
        int minSalary = GetLeagueMinimumSalary(rules);
        int maxSalary = GetMaximumPlayerSalary(rules);
        if (salary < minSalary)
        {
            return minSalary;
        }

        return salary > maxSalary ? maxSalary : salary;
    }

    public static int ClampOwnTeamYears(int years)
    {
        return ClampOwnTeamYears(years, null);
    }

    public static int ClampOwnTeamYears(int years, LeagueRulesData rules)
    {
        int maxYears = GetMaxOwnTeamYears(rules);
        if (years < MinExtensionYears)
        {
            return MinExtensionYears;
        }

        return years > maxYears ? maxYears : years;
    }

    public static string FormatMoney(int value)
    {
        return "$" + value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }

    public static string GetInterestLabel(int interest)
    {
        if (interest >= HighInterestThreshold)
        {
            return "High";
        }

        if (interest >= MediumInterestThreshold)
        {
            return "Medium";
        }

        return interest >= LowInterestThreshold ? "Low" : "Very Low";
    }

    public static int GetLeagueMinimumSalary(LeagueRulesData rules)
    {
        return rules != null && rules.LeagueMinimumSalary > 0
            ? rules.LeagueMinimumSalary
            : LeagueMinimumSalary;
    }

    public static int GetMaximumPlayerSalary(LeagueRulesData rules)
    {
        return rules != null && rules.MaximumPlayerSalary > 0
            ? rules.MaximumPlayerSalary
            : MaxPlayerSalary;
    }

    public static int GetMaxOwnTeamYears(LeagueRulesData rules)
    {
        return rules != null && rules.MaxContractYearsWithOwnTeam > 0
            ? rules.MaxContractYearsWithOwnTeam
            : MaxOwnTeamYears;
    }
}
