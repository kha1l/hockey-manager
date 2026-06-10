using System;
using System.Collections.Generic;

public static class ProspectSigningService
{
    public static void EnsureProspectSigningHistory(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.ProspectSigningHistory == null)
        {
            state.ProspectSigningHistory = new ProspectSigningHistoryData();
        }

        state.ProspectSigningHistory.EnsureSignings();
    }

    public static List<ProspectData> GetTeamDraftRights(GameState state, string teamId)
    {
        List<ProspectData> rights = new List<ProspectData>();
        TeamData team = FindTeam(state, teamId);
        if (team == null)
        {
            return rights;
        }

        team.EnsureDraftRights();
        foreach (ProspectData prospect in team.DraftRights)
        {
            if (prospect != null)
            {
                rights.Add(prospect);
            }
        }

        rights.Sort(CompareDraftRights);
        return rights;
    }

    public static ProspectData FindProspectRights(GameState state, string teamId, string prospectId)
    {
        TeamData team = FindTeam(state, teamId);
        if (team == null || string.IsNullOrEmpty(prospectId))
        {
            return null;
        }

        team.EnsureDraftRights();
        foreach (ProspectData prospect in team.DraftRights)
        {
            if (prospect != null && prospect.Id == prospectId)
            {
                return prospect;
            }
        }

        return null;
    }

    public static bool TrySignProspectToElc(
        GameState state,
        string prospectId,
        out ProspectSigningData signing,
        out string message)
    {
        EnsureProspectSigningHistory(state);

        if (state == null)
        {
            signing = CreateSigning(null, null);
            message = "Состояние игры не найдено";
            signing.Status = "Rejected";
            signing.RejectionReason = message;
            return false;
        }

        if (state.LeagueRules == null)
        {
            state.LeagueRules = LeagueRulesConfig.CreateDefaultRules();
        }

        TeamData team = FindTeam(state, state.SelectedTeamId);
        if (team == null)
        {
            signing = CreateSigning(null, null);
            message = "Команда не найдена";
            Reject(state, signing, message);
            return false;
        }

        team.EnsurePlayers();
        team.EnsureDraftRights();

        ProspectData prospect = FindProspectRights(state, team.Id, prospectId);
        signing = CreateSigning(prospect, team);

        if (prospect == null)
        {
            message = "Права на проспекта не найдены";
            Reject(state, signing, message);
            return false;
        }

        if (!EntryLevelContractConfig.IsEligibleForEntryLevelContract(prospect.Age))
        {
            message = "Игрок не подходит для entry-level contract";
            Reject(state, signing, message);
            return false;
        }

        if (HasPlayerFromProspect(team, prospect.Id))
        {
            message = "Проспект уже подписан";
            Reject(state, signing, message);
            return false;
        }

        if (team.Players.Count >= state.LeagueRules.MaxRosterSize)
        {
            message = "Достигнут максимальный размер состава";
            Reject(state, signing, message);
            return false;
        }

        PlayerData player = ConvertProspectToPlayer(prospect, team, state.LeagueRules);
        int payrollAfter = SalaryCapService.CalculatePayroll(team) + player.Salary;
        if (payrollAfter > state.LeagueRules.SalaryCapUpperLimit)
        {
            message = "Недостаточно места под потолком зарплат";
            signing.Salary = player.Salary;
            signing.ContractYears = player.ContractYearsRemaining;
            Reject(state, signing, message);
            return false;
        }

        team.Players.Add(player);
        team.DraftRights.Remove(prospect);

        signing.Salary = player.Salary;
        signing.ContractYears = player.ContractYearsRemaining;
        signing.Status = "Accepted";
        signing.RejectionReason = "";
        AddSigningToHistory(state, signing);

        message = "Проспект подписан на ELC: " + signing.ProspectName;
        return true;
    }

    private static PlayerData ConvertProspectToPlayer(
        ProspectData prospect,
        TeamData team,
        LeagueRulesData rules)
    {
        return new PlayerData
        {
            Id = "player-from-" + prospect.Id,
            FirstName = prospect.FirstName,
            LastName = prospect.LastName,
            TeamId = team.Id,
            Position = prospect.Position,
            Age = prospect.Age,
            Overall = prospect.Overall,
            Potential = prospect.Potential,
            Salary = CalculateElcSalary(prospect, rules),
            ContractYearsRemaining = EntryLevelContractConfig.GetContractYearsByAge(prospect.Age),
            ContractStatus = "Signed",
            HasNoTradeClause = false,
            IsGeneratedContract = true,
            IsEntryLevelContract = true,
            SourceProspectId = prospect.Id,
            DraftRound = prospect.DraftRound,
            DraftPickOverall = prospect.DraftPickOverall,
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
    }

    private static int CalculateElcSalary(ProspectData prospect, LeagueRulesData rules)
    {
        int salary = rules.LeagueMinimumSalary;

        if (prospect.DraftRound == 1)
        {
            salary += EntryLevelContractConfig.SalaryPremiumForRound1;
        }
        else if (prospect.DraftRound == 2)
        {
            salary += EntryLevelContractConfig.SalaryPremiumForRound2;
        }
        else if (prospect.DraftRound == 3)
        {
            salary += EntryLevelContractConfig.SalaryPremiumForRound3;
        }

        if (prospect.Potential >= 90)
        {
            salary += 200000;
        }
        else if (prospect.Potential >= 85)
        {
            salary += 100000;
        }

        if (salary < rules.LeagueMinimumSalary)
        {
            salary = rules.LeagueMinimumSalary;
        }

        if (salary > rules.MaximumPlayerSalary)
        {
            salary = rules.MaximumPlayerSalary;
        }

        return salary;
    }

    private static ProspectSigningData CreateSigning(ProspectData prospect, TeamData team)
    {
        return new ProspectSigningData
        {
            SigningId = Guid.NewGuid().ToString("N"),
            ProspectId = prospect == null ? "" : prospect.Id,
            ProspectName = prospect == null ? "" : prospect.FirstName + " " + prospect.LastName,
            TeamId = team == null ? "" : team.Id,
            TeamName = team == null ? "" : GetTeamName(team),
            SignedAtUtc = DateTime.UtcNow.ToString("o"),
            Salary = 0,
            ContractYears = prospect == null ? 0 : EntryLevelContractConfig.GetContractYearsByAge(prospect.Age),
            Status = "",
            RejectionReason = ""
        };
    }

    private static void Reject(GameState state, ProspectSigningData signing, string reason)
    {
        signing.Status = "Rejected";
        signing.RejectionReason = reason;
        AddSigningToHistory(state, signing);
    }

    private static void AddSigningToHistory(GameState state, ProspectSigningData signing)
    {
        EnsureProspectSigningHistory(state);
        if (state != null && state.ProspectSigningHistory != null && signing != null)
        {
            state.ProspectSigningHistory.Signings.Add(signing);
        }
    }

    private static TeamData FindTeam(GameState state, string teamId)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
    }

    private static int CompareDraftRights(ProspectData left, ProspectData right)
    {
        if (left == null && right == null)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int roundComparison = left.DraftRound.CompareTo(right.DraftRound);
        if (roundComparison != 0)
        {
            return roundComparison;
        }

        int pickComparison = left.DraftPickOverall.CompareTo(right.DraftPickOverall);
        if (pickComparison != 0)
        {
            return pickComparison;
        }

        return right.Potential.CompareTo(left.Potential);
    }

    private static bool HasPlayerFromProspect(TeamData team, string prospectId)
    {
        if (team == null || string.IsNullOrEmpty(prospectId))
        {
            return false;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.SourceProspectId == prospectId)
            {
                return true;
            }
        }

        return false;
    }

    private static string GetTeamName(TeamData team)
    {
        return team == null ? "" : team.City + " " + team.Name;
    }
}
