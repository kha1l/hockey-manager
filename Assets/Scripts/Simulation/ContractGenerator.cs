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
            NormalizeContract(player);
        }
    }

    public static void AssignContract(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

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
        if (player == null)
        {
            return;
        }

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
