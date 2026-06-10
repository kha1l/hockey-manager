using System.Collections.Generic;

public static class ContractService
{
    public static bool TryExtendContract(TeamData team, string playerId, out string message)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player == null)
        {
            message = "Игрок не найден";
            return false;
        }

        ContractGenerator.NormalizeContract(player);

        if (player.ContractYearsRemaining >= SalaryCapConfig.MaxContractYearsWithOwnTeam)
        {
            message = "У игрока уже максимальный срок контракта";
            return false;
        }

        int currentSalary = player.Salary;
        int newSalary = currentSalary + currentSalary / 10;
        if (newSalary < SalaryCapConfig.LeagueMinimumSalary)
        {
            newSalary = SalaryCapConfig.LeagueMinimumSalary;
        }

        if (newSalary > SalaryCapConfig.MaximumPlayerSalary)
        {
            newSalary = SalaryCapConfig.MaximumPlayerSalary;
        }

        int payrollWithoutCurrentSalary = SalaryCapService.CalculatePayroll(team) - currentSalary;
        if (payrollWithoutCurrentSalary + newSalary > SalaryCapConfig.SalaryCapUpperLimit)
        {
            message = "Недостаточно места под потолком зарплат";
            return false;
        }

        player.Salary = newSalary;
        player.ContractYearsRemaining += 1;
        player.ContractStatus = "Signed";
        player.IsGeneratedContract = true;

        message = "Контракт продлён";
        return true;
    }

    public static void AdvanceContractYear(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsurePlayers();

            foreach (PlayerData player in team.Players)
            {
                AdvanceContractYear(player);
            }
        }
    }

    private static void AdvanceContractYear(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        if (player.ContractYearsRemaining > 0)
        {
            player.ContractYearsRemaining -= 1;
        }

        if (player.ContractYearsRemaining <= 0)
        {
            player.ContractYearsRemaining = 0;
            player.ContractStatus = player.Age < 27 ? "RFA" : "UFA";
            return;
        }

        if (player.ContractYearsRemaining == 1)
        {
            player.ContractStatus = "Expiring";
        }
    }

    private static PlayerData FindPlayer(TeamData team, string playerId)
    {
        if (team == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        team.EnsurePlayers();

        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }
}
