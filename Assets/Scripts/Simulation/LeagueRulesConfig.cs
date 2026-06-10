public static class LeagueRulesConfig
{
    public static LeagueRulesData CreateDefaultRules()
    {
        return new LeagueRulesData
        {
            Ruleset = "NHL 2026-27",
            RulesetName = "NHL 2026-27",
            Cba = "NHL/NHLPA CBA 2026-2030",
            CbaName = "NHL/NHLPA CBA 2026-2030",
            RulesSeasonStartYear = SalaryCapConfig.RulesSeasonStartYear,
            RegularSeasonGamesPerTeam = SalaryCapConfig.TargetGamesPerTeam,
            PreseasonGamesPerTeam = 4,
            SalaryCapUpperLimit = SalaryCapConfig.SalaryCapUpperLimit,
            SalaryCapLowerLimit = SalaryCapConfig.SalaryCapLowerLimit,
            LeagueMinimumSalary = SalaryCapConfig.LeagueMinimumSalary,
            MaximumPlayerSalary = SalaryCapConfig.MaximumPlayerSalary,
            MaxContractYearsWithOwnTeam = SalaryCapConfig.MaxContractYearsWithOwnTeam,
            MaxContractYearsFreeAgent = SalaryCapConfig.MaxContractYearsFreeAgent,
            MinRosterSize = SalaryCapConfig.MinRosterSize,
            MaxRosterSize = SalaryCapConfig.MaxRosterSize
        };
    }
}
