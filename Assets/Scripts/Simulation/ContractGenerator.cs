using System;
using System.Collections.Generic;

public static class ContractGenerator
{
    public static void EnsureContractsForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureContractsForTeam(team);
        }
    }

    public static void EnsureContractsForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();

        foreach (PlayerData player in team.Players)
        {
            if (player != null && !player.IsRetired)
            {
                NormalizeContract(player);
            }
        }
    }

    public static void AssignContract(PlayerData player)
    {
        if (player == null || player.IsRetired)
        {
            return;
        }

        PlayerDevelopmentService.EnsureDevelopmentProfile(player);

        if (player.Salary > 0 && player.ContractYearsRemaining > 0)
        {
            return;
        }

        Random random = new Random(player.Id == null ? 0 : player.Id.GetHashCode());
        GetSalaryRange(player.Overall, out int minSalary, out int maxSalary);
        int salary = RoundToNearest(GetRandomInclusive(random, minSalary, maxSalary), 50000);

        player.Salary = Clamp(salary, SalaryCapConfig.LeagueMinimumSalary, SalaryCapConfig.MaximumPlayerSalary);
        player.ContractYearsRemaining = Clamp(GetContractYears(player.Age, random), 1, SalaryCapConfig.MaxContractYearsWithOwnTeam);
        player.ContractStatus = "Signed";
        player.HasNoTradeClause = false;
        player.IsGeneratedContract = true;
    }

    public static void NormalizeContract(PlayerData player)
    {
        if (player == null || player.IsRetired)
        {
            return;
        }

        PlayerDevelopmentService.EnsureDevelopmentProfile(player);

        if (player.Salary <= 0)
        {
            AssignContract(player);
        }

        if (player.Salary < SalaryCapConfig.LeagueMinimumSalary)
        {
            player.Salary = SalaryCapConfig.LeagueMinimumSalary;
        }

        if (player.Salary > SalaryCapConfig.MaximumPlayerSalary)
        {
            player.Salary = SalaryCapConfig.MaximumPlayerSalary;
        }

        if (player.ContractYearsRemaining <= 0)
        {
            player.ContractYearsRemaining = 0;
            player.ContractStatus = player.Age < 27 ? "RFA" : "UFA";
            return;
        }

        if (player.ContractYearsRemaining > SalaryCapConfig.MaxContractYearsWithOwnTeam)
        {
            player.ContractYearsRemaining = SalaryCapConfig.MaxContractYearsWithOwnTeam;
        }

        if (string.IsNullOrEmpty(player.ContractStatus))
        {
            player.ContractStatus = "Signed";
        }
    }

    public static void NormalizeInitialNhlPayrollToCapBand(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        List<PlayerData> nhlPlayers = TeamRosterService.GetNhlPlayers(team);
        if (nhlPlayers == null || nhlPlayers.Count == 0)
        {
            return;
        }

        foreach (PlayerData player in nhlPlayers)
        {
            NormalizeContract(player);
        }

        int payroll = SumPayroll(nhlPlayers);
        if (payroll > SalaryCapConfig.SalaryCapUpperLimit)
        {
            ReducePayrollToCap(nhlPlayers, payroll - SalaryCapConfig.SalaryCapUpperLimit + 250000);
        }

        payroll = SumPayroll(nhlPlayers);
        if (payroll < SalaryCapConfig.SalaryCapLowerLimit)
        {
            RaisePayrollToFloor(nhlPlayers, SalaryCapConfig.SalaryCapLowerLimit - payroll + 250000);
        }
    }

    private static void GetSalaryRange(int overall, out int minSalary, out int maxSalary)
    {
        if (overall < 65)
        {
            minSalary = SalaryCapConfig.LeagueMinimumSalary;
            maxSalary = 1300000;
            return;
        }

        if (overall <= 74)
        {
            minSalary = 1300000;
            maxSalary = 3500000;
            return;
        }

        if (overall <= 84)
        {
            minSalary = 3500000;
            maxSalary = 7500000;
            return;
        }

        if (overall <= 89)
        {
            minSalary = 7500000;
            maxSalary = 12000000;
            return;
        }

        minSalary = 12000000;
        maxSalary = SalaryCapConfig.MaximumPlayerSalary;
    }

    private static void ReducePayrollToCap(List<PlayerData> players, int amount)
    {
        if (players == null || amount <= 0)
        {
            return;
        }

        players.Sort(CompareSalaryDepthForReduction);
        int remaining = amount;
        int guard = 0;
        while (remaining > 0 && guard < 8)
        {
            guard++;
            bool changed = false;
            foreach (PlayerData player in players)
            {
                if (player == null || player.IsRetired)
                {
                    continue;
                }

                int minimum = SalaryCapConfig.LeagueMinimumSalary;
                int reducible = player.Salary - minimum;
                if (reducible <= 0)
                {
                    continue;
                }

                int reduction = RoundToNearest(Math.Min(reducible, Math.Min(remaining, 500000)), 50000);
                if (reduction <= 0)
                {
                    reduction = Math.Min(reducible, remaining);
                }

                player.Salary -= reduction;
                remaining -= reduction;
                changed = true;
                if (remaining <= 0)
                {
                    return;
                }
            }

            if (!changed)
            {
                return;
            }
        }
    }

    private static void RaisePayrollToFloor(List<PlayerData> players, int amount)
    {
        if (players == null || amount <= 0)
        {
            return;
        }

        players.Sort(CompareOverallForRaise);
        int remaining = amount;
        int guard = 0;
        while (remaining > 0 && guard < 8)
        {
            guard++;
            bool changed = false;
            foreach (PlayerData player in players)
            {
                if (player == null || player.IsRetired)
                {
                    continue;
                }

                int room = SalaryCapConfig.MaximumPlayerSalary - player.Salary;
                if (room <= 0)
                {
                    continue;
                }

                int raise = RoundToNearest(Math.Min(room, Math.Min(remaining, 750000)), 50000);
                if (raise <= 0)
                {
                    raise = Math.Min(room, remaining);
                }

                player.Salary += raise;
                remaining -= raise;
                changed = true;
                if (remaining <= 0)
                {
                    return;
                }
            }

            if (!changed)
            {
                return;
            }
        }
    }

    private static int SumPayroll(List<PlayerData> players)
    {
        int total = 0;
        if (players == null)
        {
            return total;
        }

        foreach (PlayerData player in players)
        {
            if (player != null && !player.IsRetired)
            {
                total += player.Salary;
            }
        }

        return total;
    }

    private static int CompareSalaryDepthForReduction(PlayerData left, PlayerData right)
    {
        int salary = (right == null ? 0 : right.Salary).CompareTo(left == null ? 0 : left.Salary);
        if (salary != 0)
        {
            return salary;
        }

        return (left == null ? 99 : left.Overall).CompareTo(right == null ? 99 : right.Overall);
    }

    private static int CompareOverallForRaise(PlayerData left, PlayerData right)
    {
        int overall = (right == null ? 0 : right.Overall).CompareTo(left == null ? 0 : left.Overall);
        if (overall != 0)
        {
            return overall;
        }

        return (right == null ? 0 : right.Potential).CompareTo(left == null ? 0 : left.Potential);
    }

    private static int GetContractYears(int age, Random random)
    {
        if (age < 23)
        {
            return GetRandomInclusive(random, 2, 3);
        }

        if (age <= 30)
        {
            return GetRandomInclusive(random, 3, 6);
        }

        return GetRandomInclusive(random, 1, 3);
    }

    private static int GetRandomInclusive(Random random, int minValue, int maxValue)
    {
        return random.Next(minValue, maxValue + 1);
    }

    private static int RoundToNearest(int value, int step)
    {
        return (int)Math.Round(value / (double)step) * step;
    }

    private static int Clamp(int value, int minValue, int maxValue)
    {
        if (value < minValue)
        {
            return minValue;
        }

        if (value > maxValue)
        {
            return maxValue;
        }

        return value;
    }
}
