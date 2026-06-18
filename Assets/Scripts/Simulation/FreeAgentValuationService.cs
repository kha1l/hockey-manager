using System;

public static class FreeAgentValuationService
{
    public static int CalculateExpectedSalary(GameState state, PlayerData player)
    {
        if (player == null)
        {
            return BetterFreeAgencyConfig.GetLeagueMinimumSalary(GetRules(state));
        }

        LeagueRulesData rules = GetRules(state);
        long salary = GetBaseSalaryByOverall(player);
        salary += GetPotentialSalaryModifier(player, salary);
        salary += GetAgeSalaryModifier(player, salary);
        salary += GetPositionSalaryModifier(player, salary);
        salary += GetRoleSalaryModifier(player, salary);
        salary += GetStatsSalaryModifier(state, player, salary);

        // UFA market usually costs more than an own-team extension.
        salary = salary * 108 / 100;

        return RoundSalary(BetterFreeAgencyConfig.ClampSalary((int)Math.Min(int.MaxValue, salary), rules));
    }

    public static int CalculateMinimumSalary(GameState state, PlayerData player)
    {
        LeagueRulesData rules = GetRules(state);
        int expected = player == null || player.FreeAgencyExpectedSalary <= 0
            ? CalculateExpectedSalary(state, player)
            : player.FreeAgencyExpectedSalary;

        int minimum = expected * 88 / 100;
        if (player != null)
        {
            if (player.Age >= 34)
            {
                minimum = expected * 82 / 100;
            }
            else if (player.Overall >= 84 || player.Potential >= 88)
            {
                minimum = expected * 92 / 100;
            }
        }

        return RoundSalary(BetterFreeAgencyConfig.ClampSalary(minimum, rules));
    }

    public static int CalculateExpectedYears(GameState state, PlayerData player)
    {
        if (player == null)
        {
            return BetterFreeAgencyConfig.MinFreeAgentYears;
        }

        int years;
        if (player.Position == "G")
        {
            years = player.Overall >= 82 && player.Age <= 31 ? 4 : player.Age >= 34 ? 1 : 2;
        }
        else if (player.Age <= 25 && player.Potential >= 86)
        {
            years = 5;
        }
        else if (player.Age <= 29 && player.Overall >= 82)
        {
            years = player.Overall >= 87 ? 6 : 5;
        }
        else if (player.Age <= 32)
        {
            years = player.Overall >= 76 ? 3 : 2;
        }
        else
        {
            years = player.Age >= 35 ? 1 : 2;
        }

        return BetterFreeAgencyConfig.ClampFreeAgentYears(years, GetRules(state));
    }

    public static int CalculatePreferredYears(GameState state, PlayerData player)
    {
        int years = CalculateExpectedYears(state, player);
        if (player == null)
        {
            return years;
        }

        if (player.Age >= 33)
        {
            years--;
        }
        else if (player.Overall >= 84 && player.Age <= 29)
        {
            years++;
        }

        return BetterFreeAgencyConfig.ClampFreeAgentYears(years, GetRules(state));
    }

    public static string BuildAskSummary(PlayerData player)
    {
        if (player == null)
        {
            return "Запрос: нет данных";
        }

        return "Запрос: " + BetterFreeAgencyConfig.FormatMoney(player.FreeAgencyExpectedSalary)
            + " x " + player.FreeAgencyExpectedYears + " лет"
            + " | минимум " + BetterFreeAgencyConfig.FormatMoney(player.FreeAgencyMinimumSalary);
    }

    private static int GetBaseSalaryByOverall(PlayerData player)
    {
        int overall = player == null ? 60 : player.Overall;
        if (overall >= 90)
        {
            return 12500000 + (overall - 90) * 850000;
        }

        if (overall >= 86)
        {
            return 9500000 + (overall - 86) * 775000;
        }

        if (overall >= 82)
        {
            return 7000000 + (overall - 82) * 650000;
        }

        if (overall >= 78)
        {
            return 4500000 + (overall - 78) * 600000;
        }

        if (overall >= 74)
        {
            return 2700000 + (overall - 74) * 400000;
        }

        if (overall >= 70)
        {
            return 1300000 + (overall - 70) * 325000;
        }

        return 850000 + Math.Max(0, overall - 60) * 70000;
    }

    private static int GetPotentialSalaryModifier(PlayerData player, long salary)
    {
        if (player == null)
        {
            return 0;
        }

        if (player.Age <= 25 && player.Potential >= 87)
        {
            return (int)(salary * 16 / 100);
        }

        return player.Potential >= 83 ? (int)(salary * 7 / 100) : 0;
    }

    private static int GetAgeSalaryModifier(PlayerData player, long salary)
    {
        if (player == null)
        {
            return 0;
        }

        if (player.Age >= 35)
        {
            return (int)(salary * -18 / 100);
        }

        if (player.Age >= 32)
        {
            return (int)(salary * -8 / 100);
        }

        return player.Age <= 24 && player.Potential >= 84 ? (int)(salary * 6 / 100) : 0;
    }

    private static int GetPositionSalaryModifier(PlayerData player, long salary)
    {
        if (player == null)
        {
            return 0;
        }

        if (player.Position == "G")
        {
            return player.Overall >= 82 ? (int)(salary * 10 / 100) : (int)(salary * -8 / 100);
        }

        if (player.Position == "D" && player.Overall >= 80)
        {
            return (int)(salary * 8 / 100);
        }

        if (player.Position == "C" && player.Overall >= 80)
        {
            return (int)(salary * 5 / 100);
        }

        return 0;
    }

    private static int GetRoleSalaryModifier(PlayerData player, long salary)
    {
        if (player == null)
        {
            return 0;
        }

        PlayerRoleService.EnsureRole(player);
        if (player.UsageCategory == "TopLine" || player.UsageCategory == "TopPair" || player.UsageCategory == "Starter")
        {
            return (int)(salary * 8 / 100);
        }

        return player.Overall < 73 ? (int)(salary * -8 / 100) : 0;
    }

    private static int GetStatsSalaryModifier(GameState state, PlayerData player, long salary)
    {
        PlayerSeasonStatsData stats = FindStats(state, player);
        if (stats == null || stats.GamesPlayed <= 0 && stats.GoalieGamesPlayed <= 0)
        {
            return 0;
        }

        if (player != null && player.Position == "G")
        {
            if (stats.GoalieWins >= 30 || stats.Shutouts >= 5)
            {
                return (int)(salary * 8 / 100);
            }

            return stats.GoalieGamesPlayed >= 35 ? (int)(salary * 4 / 100) : 0;
        }

        if (stats.Points >= 80 || stats.Goals >= 35)
        {
            return (int)(salary * 12 / 100);
        }

        return stats.Points >= 55 || stats.Goals >= 25 ? (int)(salary * 6 / 100) : 0;
    }

    private static PlayerSeasonStatsData FindStats(GameState state, PlayerData player)
    {
        if (state == null || state.Season == null || state.Season.PlayerStats == null || player == null)
        {
            return null;
        }

        foreach (PlayerSeasonStatsData stats in state.Season.PlayerStats)
        {
            if (stats != null && stats.PlayerId == player.Id)
            {
                return stats;
            }
        }

        return null;
    }

    private static int RoundSalary(int value)
    {
        return (int)(Math.Round(value / 50000.0) * 50000);
    }

    private static LeagueRulesData GetRules(GameState state)
    {
        return state != null && state.LeagueRules != null ? state.LeagueRules : LeagueRulesConfig.CreateDefaultRules();
    }
}
