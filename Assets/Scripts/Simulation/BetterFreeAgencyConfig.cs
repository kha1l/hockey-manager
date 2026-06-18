using System.Globalization;

public static class BetterFreeAgencyConfig
{
    public const int MinInterest = 0;
    public const int MaxInterest = 100;
    public const int AcceptanceThreshold = 60;
    public const int CpuSigningThreshold = 70;
    public const int MaxOffersToKeep = 300;

    public const int LeagueMinimumSalary = 850000;
    public const int MaxPlayerSalary = 20800000;
    public const int MaxFreeAgentYears = 6;
    public const int MinFreeAgentYears = 1;

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
        int minimum = GetLeagueMinimumSalary(rules);
        int maximum = GetMaximumPlayerSalary(rules);
        if (salary < minimum)
        {
            return minimum;
        }

        return salary > maximum ? maximum : salary;
    }

    public static int ClampFreeAgentYears(int years)
    {
        return ClampFreeAgentYears(years, null);
    }

    public static int ClampFreeAgentYears(int years, LeagueRulesData rules)
    {
        int maximum = GetMaxFreeAgentYears(rules);
        if (years < MinFreeAgentYears)
        {
            return MinFreeAgentYears;
        }

        return years > maximum ? maximum : years;
    }

    public static string FormatMoney(int value)
    {
        return "$" + value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }

    public static string GetInterestLabel(int interest)
    {
        interest = ClampInterest(interest);
        if (interest >= 80)
        {
            return "High";
        }

        if (interest >= 60)
        {
            return "Medium";
        }

        if (interest >= 40)
        {
            return "Low";
        }

        return "Very Low";
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

    public static int GetMaxFreeAgentYears(LeagueRulesData rules)
    {
        return rules != null && rules.MaxContractYearsFreeAgent > 0
            ? rules.MaxContractYearsFreeAgent
            : MaxFreeAgentYears;
    }
}
