using System;

public static class ContractExtensionValuationService
{
    public static int CalculateExpectedSalary(GameState state, TeamData team, PlayerData player)
    {
        if (player == null)
        {
            return ContractExtensionConfig.GetLeagueMinimumSalary(GetRules(state));
        }

        LeagueRulesData rules = GetRules(state);
        long salary = GetBaseSalaryByOverall(player);
        salary += GetPotentialSalaryModifier(player, salary);
        salary += GetAgeSalaryModifier(player, salary);
        salary += GetPositionSalaryModifier(player, salary);
        salary += GetRoleSalaryModifier(player, salary);
        salary += GetStatsSalaryModifier(state, player, salary);

        if (player.IsEntryLevelContract && player.Age <= 24 && player.Potential >= 82)
        {
            salary += salary / 10;
        }

        return RoundSalary(ContractExtensionConfig.ClampSalary((int)Math.Min(int.MaxValue, salary), rules));
    }

    public static int CalculateMinimumSalary(GameState state, TeamData team, PlayerData player)
    {
        LeagueRulesData rules = GetRules(state);
        int expected = player == null || player.ExtensionExpectedSalary <= 0
            ? CalculateExpectedSalary(state, team, player)
            : player.ExtensionExpectedSalary;
        int minimum = expected * 85 / 100;

        if (player != null)
        {
            MoraleService.InitializePlayerMorale(player);
            if (player.Morale < 40 || player.WantsTrade)
            {
                minimum = expected * 95 / 100;
            }
            else if (player.ExtensionInterest >= 80)
            {
                minimum = expected * 80 / 100;
            }
        }

        return RoundSalary(ContractExtensionConfig.ClampSalary(minimum, rules));
    }

    public static int CalculateExpectedYears(GameState state, TeamData team, PlayerData player)
    {
        if (player == null)
        {
            return ContractExtensionConfig.MinExtensionYears;
        }

        int years;
        if (player.ContractStatus == "RFA")
        {
            years = player.Potential >= 84 && player.Age <= 24 ? 3 : 2;
        }
        else if (player.Position == "G" && IsStarter(player))
        {
            years = player.Age <= 31 ? 4 : 2;
        }
        else if (player.Age <= 23 && player.Potential >= 86)
        {
            years = 5;
        }
        else if (player.Age <= 29 && IsCore(player))
        {
            years = player.Overall >= 86 ? 6 : 5;
        }
        else if (player.Age <= 32)
        {
            years = IsDepth(player) ? 2 : 4;
        }
        else
        {
            years = player.Age >= 35 ? 1 : 2;
        }

        return ContractExtensionConfig.ClampOwnTeamYears(years, GetRules(state));
    }

    public static int CalculatePreferredYears(GameState state, TeamData team, PlayerData player)
    {
        int expectedYears = CalculateExpectedYears(state, team, player);
        if (player == null)
        {
            return expectedYears;
        }

        if (player.Age >= 32)
        {
            expectedYears = Math.Max(1, expectedYears - 1);
        }
        else if (IsCore(player) && player.Age <= 29)
        {
            expectedYears++;
        }

        return ContractExtensionConfig.ClampOwnTeamYears(expectedYears, GetRules(state));
    }

    public static string BuildAskSummary(PlayerData player)
    {
        if (player == null)
        {
            return "Запрос: нет данных";
        }

        return "Запрос: " + player.ExtensionExpectedYears + " лет x "
            + ContractExtensionConfig.FormatMoney(player.ExtensionExpectedSalary);
    }

    private static int GetBaseSalaryByOverall(PlayerData player)
    {
        int overall = player == null ? 60 : player.Overall;
        if (overall >= 90)
        {
            return 12000000 + (overall - 90) * 800000;
        }

        if (overall >= 86)
        {
            return 9000000 + (overall - 86) * 750000;
        }

        if (overall >= 82)
        {
            return 6500000 + (overall - 82) * 625000;
        }

        if (overall >= 78)
        {
            return 4000000 + (overall - 78) * 625000;
        }

        if (overall >= 74)
        {
            return 2500000 + (overall - 74) * 375000;
        }

        if (overall >= 70)
        {
            return 1200000 + (overall - 70) * 325000;
        }

        return 850000 + Math.Max(0, overall - 60) * 65000;
    }

    private static int GetPotentialSalaryModifier(PlayerData player, long salary)
    {
        if (player == null)
        {
            return 0;
        }

        if (player.Age <= 24 && player.Potential >= 86)
        {
            return (int)(salary * 15 / 100);
        }

        return player.Potential >= 82 ? (int)(salary * 8 / 100) : 0;
    }

    private static int GetAgeSalaryModifier(PlayerData player, long salary)
    {
        if (player == null)
        {
            return 0;
        }

        if (player.Age >= 34)
        {
            return (int)(salary * -12 / 100);
        }

        if (player.Age >= 31)
        {
            return (int)(salary * -5 / 100);
        }

        return 0;
    }

    private static int GetPositionSalaryModifier(PlayerData player, long salary)
    {
        if (player == null)
        {
            return 0;
        }

        if (player.Position == "G")
        {
            return IsStarter(player) ? (int)(salary / 10) : (int)(salary * -10 / 100);
        }

        if (player.Position == "D" && IsTopRole(player))
        {
            return (int)(salary * 8 / 100);
        }

        if (player.Position == "C" && IsTopRole(player))
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

        if (IsTopRole(player))
        {
            return (int)(salary * 10 / 100);
        }

        if (IsDepth(player))
        {
            return (int)(salary * -10 / 100);
        }

        return 0;
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

            return stats.GoalieGamesPlayed >= 35 && stats.GoalsAgainst * 100 / Math.Max(1, stats.ShotsAgainst) <= 8
                ? (int)(salary * 5 / 100)
                : 0;
        }

        if (stats.Points >= 80 || stats.Goals >= 35)
        {
            return (int)(salary * 10 / 100);
        }

        return stats.Points >= 55 || stats.Goals >= 25 ? (int)(salary * 5 / 100) : 0;
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

    private static bool IsCore(PlayerData player)
    {
        return player != null && (player.Overall >= 82 || player.Potential >= 86);
    }

    private static bool IsStarter(PlayerData player)
    {
        return player != null
            && (player.PlayerRole == PlayerRoleConfig.StarterGoalie
                || player.UsageCategory == "Starter"
                || player.Overall >= 82);
    }

    private static bool IsTopRole(PlayerData player)
    {
        return player != null
            && (player.PlayerRole == PlayerRoleConfig.Sniper
                || player.PlayerRole == PlayerRoleConfig.Playmaker
                || player.PlayerRole == PlayerRoleConfig.PowerForward
                || player.PlayerRole == PlayerRoleConfig.OffensiveDefenseman
                || player.PlayerRole == PlayerRoleConfig.TwoWayDefenseman
                || player.PlayerRole == PlayerRoleConfig.StarterGoalie
                || player.UsageCategory == "TopLine"
                || player.UsageCategory == "TopPair"
                || player.UsageCategory == "Starter");
    }

    private static bool IsDepth(PlayerData player)
    {
        return player != null
            && (player.PlayerRole == PlayerRoleConfig.DepthForward
                || player.PlayerRole == PlayerRoleConfig.DepthGoalie
                || player.UsageCategory == "Depth"
                || RosterStatusConfig.IsFarmRoster(player)
                || RosterStatusConfig.IsReserve(player)
                || player.Overall < 74);
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
