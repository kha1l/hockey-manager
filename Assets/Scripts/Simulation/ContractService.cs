using System.Collections.Generic;

public static class ContractService
{
    // Future: morale should affect extension willingness and free agency decisions.
    public static int GetMoraleContractModifier(PlayerData player)
    {
        if (player == null)
        {
            return 0;
        }

        MoraleService.InitializePlayerMorale(player);
        if (player.WantsTrade)
        {
            return -40;
        }

        if (player.Morale >= 80)
        {
            return 10;
        }

        if (player.Morale >= 60)
        {
            return 0;
        }

        return player.Morale >= 40 ? -10 : -25;
    }

    public static bool TryExtendContract(TeamData team, string playerId, out string message)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player == null)
        {
            message = "Игрок не найден";
            return false;
        }

        if (player.IsRetired)
        {
            message = "Игрок завершил карьеру";
            return false;
        }

        TeamRosterService.EnsureRosterStatusesForTeam(team);
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

        int payrollWithoutCurrentSalary = RosterStatusConfig.IsNhlRoster(player)
            ? SalaryCapService.CalculatePayroll(team) - currentSalary
            : SalaryCapService.CalculatePayroll(team);
        if (RosterStatusConfig.IsNhlRoster(player) && payrollWithoutCurrentSalary + newSalary > SalaryCapConfig.SalaryCapUpperLimit)
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

    public static bool ApplyContractExtension(PlayerData player, int salary, int years, out string message)
    {
        if (player == null)
        {
            message = "Игрок не найден";
            return false;
        }

        if (player.IsRetired)
        {
            message = "Игрок завершил карьеру";
            return false;
        }

        if (salary < SalaryCapConfig.LeagueMinimumSalary)
        {
            message = "Зарплата ниже минимума лиги";
            return false;
        }

        if (salary > SalaryCapConfig.MaximumPlayerSalary)
        {
            message = "Зарплата превышает максимум игрока";
            return false;
        }

        if (years < 1 || years > SalaryCapConfig.MaxContractYearsWithOwnTeam)
        {
            message = "Некорректный срок контракта";
            return false;
        }

        player.Salary = salary;
        player.ContractYearsRemaining = years;
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
                if (player != null && !player.IsRetired)
                {
                    AdvanceContractYear(player);
                }
            }
        }
    }

    private static void AdvanceContractYear(PlayerData player)
    {
        if (player == null || player.IsRetired)
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
            if (player != null && !player.IsRetired && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }
}
