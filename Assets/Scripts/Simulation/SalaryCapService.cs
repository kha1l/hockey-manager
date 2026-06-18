using System.Collections.Generic;

public static class SalaryCapService
{
    public static int CalculatePayroll(TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        int payroll = 0;

        foreach (PlayerData player in team.Players)
        {
            if (player != null && !player.IsRetired && RosterStatusConfig.IsNhlRoster(player))
            {
                // TODO: implement buried cap hit and two-way salary when AHL/cap details are added.
                ContractGenerator.NormalizeContract(player);
                payroll += player.Salary;
            }
        }

        return payroll;
    }

    public static int CalculateCapSpace(TeamData team)
    {
        return SalaryCapConfig.SalaryCapUpperLimit - CalculatePayroll(team);
    }

    public static int CalculateFloorSpace(TeamData team)
    {
        return CalculatePayroll(team) - SalaryCapConfig.SalaryCapLowerLimit;
    }

    public static bool IsOverCap(TeamData team)
    {
        return CalculatePayroll(team) > SalaryCapConfig.SalaryCapUpperLimit;
    }

    public static bool IsBelowFloor(TeamData team)
    {
        return CalculatePayroll(team) < SalaryCapConfig.SalaryCapLowerLimit;
    }

    public static bool CanAddSalary(TeamData team, int salary)
    {
        return CalculatePayroll(team) + salary <= SalaryCapConfig.SalaryCapUpperLimit;
    }

    public static bool CanFitSalary(TeamData team, int salary)
    {
        return CanAddSalary(team, salary);
    }

    public static TeamFinanceData CalculateTeamFinance(TeamData team)
    {
        int payroll = CalculatePayroll(team);
        int nhlPlayerCount = team == null ? 0 : TeamRosterService.GetNhlPlayers(team).Count;

        return new TeamFinanceData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = TeamIdentityService.GetDisplayName(team),
            SalaryCapUpperLimit = SalaryCapConfig.SalaryCapUpperLimit,
            SalaryCapLowerLimit = SalaryCapConfig.SalaryCapLowerLimit,
            Payroll = payroll,
            CapSpace = SalaryCapConfig.SalaryCapUpperLimit - payroll,
            FloorSpace = payroll - SalaryCapConfig.SalaryCapLowerLimit,
            PlayerCount = nhlPlayerCount,
            IsOverCap = payroll > SalaryCapConfig.SalaryCapUpperLimit,
            IsBelowFloor = payroll < SalaryCapConfig.SalaryCapLowerLimit
        };
    }

    public static List<PlayerData> GetExpiringContracts(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (team == null)
        {
            return players;
        }

        team.EnsurePlayers();

        foreach (PlayerData player in team.Players)
        {
            if (player == null || player.IsRetired)
            {
                continue;
            }

            ContractGenerator.NormalizeContract(player);
            if (player.ContractYearsRemaining == 1 || player.ContractStatus == "Expiring")
            {
                players.Add(player);
            }
        }

        return players;
    }
}
