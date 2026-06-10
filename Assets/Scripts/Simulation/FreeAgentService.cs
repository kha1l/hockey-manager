using System;
using System.Collections.Generic;

public static class FreeAgentService
{
    public static void EnsureFreeAgentData(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.LeagueRules == null)
        {
            state.LeagueRules = LeagueRulesConfig.CreateDefaultRules();
        }

        if (state.FreeAgentPool == null)
        {
            state.FreeAgentPool = FreeAgentGenerator.CreateFreeAgentPool();
        }

        state.FreeAgentPool.EnsureFreeAgents();

        if (!state.FreeAgentPool.IsInitialized || state.FreeAgentPool.FreeAgents.Count == 0)
        {
            state.FreeAgentPool = FreeAgentGenerator.CreateFreeAgentPool();
        }

        if (state.FreeAgentHistory == null)
        {
            state.FreeAgentHistory = new FreeAgentHistoryData();
        }

        state.FreeAgentHistory.EnsureSignings();
    }

    public static bool TrySignFreeAgent(
        GameState state,
        string playerId,
        out FreeAgentSigningData signing,
        out string message)
    {
        EnsureFreeAgentData(state);

        TeamData team = state == null ? null : FindTeam(state.Teams, state.SelectedTeamId);
        PlayerData freeAgent = FindFreeAgent(state, playerId);
        signing = CreateSigning(team, freeAgent);

        if (!ValidateSigning(state, team, freeAgent, out message))
        {
            signing.Salary = freeAgent == null ? 0 : freeAgent.Salary;
            signing.ContractYears = freeAgent == null ? 0 : freeAgent.ContractYearsRemaining;
            signing.Status = "Rejected";
            signing.RejectionReason = message;
            AddSigningToHistory(state, signing);
            return false;
        }

        state.FreeAgentPool.FreeAgents.Remove(freeAgent);
        team.EnsurePlayers();

        freeAgent.TeamId = team.Id;
        freeAgent.ContractStatus = "Signed";
        freeAgent.IsGeneratedContract = true;
        PlayerFatigueService.EnsureFatigueFields(freeAgent);
        team.Players.Add(freeAgent);

        signing.Salary = freeAgent.Salary;
        signing.ContractYears = freeAgent.ContractYearsRemaining;
        signing.Status = "Accepted";
        signing.RejectionReason = "";
        AddSigningToHistory(state, signing);

        ContractGenerator.EnsureContractsForTeam(team);
        message = "Свободный агент подписан: " + signing.PlayerName;
        return true;
    }

    public static List<PlayerData> GetAvailableFreeAgents(GameState state)
    {
        EnsureFreeAgentData(state);
        List<PlayerData> freeAgents = state == null || state.FreeAgentPool == null
            ? new List<PlayerData>()
            : new List<PlayerData>(state.FreeAgentPool.FreeAgents);

        freeAgents.Sort(CompareFreeAgents);
        return freeAgents;
    }

    public static PlayerData FindFreeAgent(GameState state, string playerId)
    {
        if (state == null || state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static bool ValidateSigning(
        GameState state,
        TeamData team,
        PlayerData freeAgent,
        out string message)
    {
        if (state == null)
        {
            message = "Состояние игры не найдено";
            return false;
        }

        if (team == null)
        {
            message = "Команда не найдена";
            return false;
        }

        if (freeAgent == null)
        {
            message = "Свободный агент не найден";
            return false;
        }

        if (!LeaguePhaseService.IsFreeAgencyOpen(state))
        {
            message = "Рынок свободных агентов откроется после завершения плей-офф";
            return false;
        }

        if (freeAgent.ContractStatus == "RFA")
        {
            message = "Подписание RFA пока не реализовано";
            return false;
        }

        if (freeAgent.ContractStatus != "UFA")
        {
            message = "Игрок не является UFA";
            return false;
        }

        team.EnsurePlayers();
        LeagueRulesData rules = GetRules(state);

        if (team.Players.Count >= rules.MaxRosterSize)
        {
            message = "Достигнут максимальный размер состава";
            return false;
        }

        if (freeAgent.Salary < rules.LeagueMinimumSalary)
        {
            freeAgent.Salary = rules.LeagueMinimumSalary;
        }

        if (freeAgent.Salary > rules.MaximumPlayerSalary)
        {
            freeAgent.Salary = rules.MaximumPlayerSalary;
        }

        if (freeAgent.ContractYearsRemaining <= 0)
        {
            freeAgent.ContractYearsRemaining = 1;
        }

        if (freeAgent.ContractYearsRemaining > rules.MaxContractYearsFreeAgent)
        {
            freeAgent.ContractYearsRemaining = rules.MaxContractYearsFreeAgent;
        }

        int payrollAfter = SalaryCapService.CalculatePayroll(team) + freeAgent.Salary;
        if (payrollAfter > rules.SalaryCapUpperLimit)
        {
            message = "Недостаточно места под потолком зарплат";
            return false;
        }

        message = "Подписание возможно";
        return true;
    }

    private static FreeAgentSigningData CreateSigning(TeamData team, PlayerData freeAgent)
    {
        return new FreeAgentSigningData
        {
            SigningId = Guid.NewGuid().ToString("N"),
            PlayerId = freeAgent == null ? "" : freeAgent.Id,
            PlayerName = freeAgent == null ? "" : freeAgent.FirstName + " " + freeAgent.LastName,
            TeamId = team == null ? "" : team.Id,
            TeamName = team == null ? "" : team.City + " " + team.Name,
            SignedAtUtc = DateTime.UtcNow.ToString("o"),
            Salary = freeAgent == null ? 0 : freeAgent.Salary,
            ContractYears = freeAgent == null ? 0 : freeAgent.ContractYearsRemaining,
            Status = "",
            RejectionReason = ""
        };
    }

    private static void AddSigningToHistory(GameState state, FreeAgentSigningData signing)
    {
        EnsureFreeAgentData(state);
        if (state != null && state.FreeAgentHistory != null && signing != null)
        {
            state.FreeAgentHistory.Signings.Add(signing);
        }
    }

    private static TeamData FindTeam(List<TeamData> teams, string teamId)
    {
        if (teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
    }

    private static int CompareFreeAgents(PlayerData left, PlayerData right)
    {
        int overallComparison = right.Overall.CompareTo(left.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        int salaryComparison = left.Salary.CompareTo(right.Salary);
        if (salaryComparison != 0)
        {
            return salaryComparison;
        }

        return left.Age.CompareTo(right.Age);
    }

    private static LeagueRulesData GetRules(GameState state)
    {
        return state.LeagueRules == null ? LeagueRulesConfig.CreateDefaultRules() : state.LeagueRules;
    }
}
