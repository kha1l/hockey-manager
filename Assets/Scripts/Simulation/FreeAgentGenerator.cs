using System;
using System.Collections.Generic;

public static class FreeAgentGenerator
{
    public static FreeAgentPoolData CreateFreeAgentPool()
    {
        FreeAgentPoolData pool = new FreeAgentPoolData
        {
            IsInitialized = true,
            GeneratedAtUtc = DateTime.UtcNow.ToString("o"),
            FreeAgents = new List<PlayerData>()
        };

        AddPlayers(pool.FreeAgents, "ufa-forward", 72);
        AddPlayers(pool.FreeAgents, "ufa-defense", 36);
        AddPlayers(pool.FreeAgents, "ufa-goalie", 12);

        return pool;
    }

    private static void AddPlayers(List<PlayerData> players, string idPrefix, int count)
    {
        for (int i = 1; i <= count; i++)
        {
            string id = idPrefix + "-" + i.ToString("000");
            Random random = new Random(id.GetHashCode());
            int overall = random.Next(58, 89);
            int potential = random.Next(overall, 93);

            PlayerData player = new PlayerData
            {
                Id = id,
                FirstName = "Free",
                LastName = "Agent " + (players.Count + 1),
                TeamId = "free-agents",
                Position = GetPosition(idPrefix, i),
                Age = random.Next(24, 37),
                Overall = overall,
                Potential = potential,
                ContractYearsRemaining = 0,
                ContractStatus = "UFA",
                HasNoTradeClause = false,
                IsGeneratedContract = true,
                Condition = FatigueConfig.DefaultCondition,
                Fatigue = FatigueConfig.DefaultFatigue,
                ConsecutiveGamesPlayed = 0,
                GamesRested = 0,
                IsResting = false,
                LastGameFatigueChange = 0,
                LastGameConditionChange = 0,
                IsInjured = false,
                InjuryType = "",
                InjurySeverity = "",
                InjuryDaysRemaining = 0,
                CanPlayThroughInjury = false,
                InjuredAtUtc = "",
                ExpectedReturnDate = "",
                TotalInjuries = 0
            };

            ContractGenerator.AssignContract(player);
            PlayerFatigueService.EnsureFatigueFields(player);
            InjuryService.EnsureInjuryFields(player);
            player.Salary = Clamp(player.Salary, SalaryCapConfig.LeagueMinimumSalary, SalaryCapConfig.MaximumPlayerSalary);
            player.ContractYearsRemaining = Clamp(
                player.ContractYearsRemaining <= 0 ? random.Next(1, SalaryCapConfig.MaxContractYearsFreeAgent + 1) : player.ContractYearsRemaining,
                1,
                SalaryCapConfig.MaxContractYearsFreeAgent);
            player.ContractStatus = "UFA";
            player.TeamId = "free-agents";
            player.HasNoTradeClause = false;
            player.IsGeneratedContract = true;

            players.Add(player);
        }
    }

    private static string GetPosition(string idPrefix, int index)
    {
        if (idPrefix == "ufa-defense")
        {
            return "D";
        }

        if (idPrefix == "ufa-goalie")
        {
            return "G";
        }

        int positionIndex = (index - 1) % 3;
        if (positionIndex == 0)
        {
            return "C";
        }

        return positionIndex == 1 ? "LW" : "RW";
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
