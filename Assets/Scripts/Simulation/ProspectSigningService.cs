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
        EnsureDraftRightsMetadata(state, team);
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
        EnsureDraftRightsMetadata(state, team);
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
        TeamRosterService.EnsureRosterStatusesForTeam(team);
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

        ProspectRiskService.EnsureRiskProfile(prospect, GetProspectRank(prospect));
        PlayerData player = ConvertProspectToPlayer(prospect, team, state.LeagueRules);
        bool willAssignToNhl = TeamRosterService.GetNhlPlayers(team).Count < state.LeagueRules.MaxRosterSize;
        player.RosterStatus = willAssignToNhl ? RosterStatusConfig.NHL : RosterStatusConfig.Farm;
        player.PreviousRosterStatus = RosterStatusConfig.DraftRights;
        player.RosterStatusUpdatedAtUtc = DateTime.UtcNow.ToString("o");
        player.IsOnWaivers = false;
        player.WaiverStatus = WaiverConfig.WaiverStatusNone;
        player.WaiverIntendedDestination = "";
        LeadershipService.ClearPlayerCaptaincy(player);
        LeadershipService.EnsurePlayerLeadershipProfile(player);
        WaiverEligibilityService.EnsureWaiverEligibility(player);

        int payrollAfter = SalaryCapService.CalculatePayroll(team) + (willAssignToNhl ? player.Salary : 0);
        if (willAssignToNhl && payrollAfter > state.LeagueRules.SalaryCapUpperLimit)
        {
            message = "Недостаточно места под потолком зарплат";
            signing.Salary = player.Salary;
            signing.ContractYears = player.ContractYearsRemaining;
            Reject(state, signing, message);
            return false;
        }

        team.Players.Add(player);
        team.DraftRights.Remove(prospect);
        LineupService.SyncScratchPlayers(team);

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
        ProspectRiskService.EnsureRiskProfile(prospect, GetProspectRank(prospect));
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
            HiddenCeiling = prospect.HiddenCeiling,
            HiddenFloor = prospect.HiddenFloor,
            DevelopmentRisk = prospect.DevelopmentRisk,
            BoomChance = prospect.BoomChance,
            BustChance = prospect.BustChance,
            DevelopmentType = prospect.DevelopmentType,
            DevelopmentTypeHint = prospect.DevelopmentTypeHint,
            HasDevelopmentProfile = true,
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
        return TeamIdentityService.GetDisplayName(team);
    }

    private static void EnsureDraftRightsMetadata(GameState state, TeamData team)
    {
        if (team == null || team.DraftRights == null)
        {
            return;
        }

        DraftClassProfileData profile = state == null || state.Draft == null
            ? null
            : state.Draft.ClassProfile;
        int draftYear = state != null && state.Draft != null && state.Draft.DraftYear > 0
            ? state.Draft.DraftYear
            : DraftPickOwnershipService.GetDraftYear(state);

        if (profile == null)
        {
            profile = DraftClassProfileGenerator.CreateFallbackProfile(draftYear);
        }

        DraftClassProfileGenerator.ApplyProfileModifiers(profile);
        for (int i = 0; i < team.DraftRights.Count; i++)
        {
            ProspectData prospect = team.DraftRights[i];
            if (prospect == null)
            {
                continue;
            }

            int rank = GetProspectRank(prospect);
            if (rank <= 0 || rank == 999)
            {
                rank = i + 1;
            }

            if (prospect.DraftRank <= 0)
            {
                prospect.DraftRank = rank;
            }

            if (prospect.ProjectedPick <= 0)
            {
                prospect.ProjectedPick = prospect.DraftRank;
            }

            if (string.IsNullOrEmpty(prospect.ProjectedRound))
            {
                prospect.ProjectedRound = DraftClassConfig.GetProjectedRoundByRank(prospect.DraftRank);
            }

            if (prospect.ProjectedRoundNumber <= 0)
            {
                prospect.ProjectedRoundNumber = DraftClassConfig.GetProjectedRoundNumberByRank(prospect.DraftRank);
            }

            if (string.IsNullOrEmpty(prospect.ProspectArchetype))
            {
                prospect.ProspectArchetype = ProspectArchetypeGenerator.DetermineArchetype(
                    prospect.Position,
                    prospect.DraftRank,
                    profile,
                    prospect.Id);
            }

            prospect.DraftClassStrengthType = profile.StrengthType;
            prospect.DraftClassDepthType = profile.DepthType;
            prospect.DraftClassPositionalTheme = profile.PositionalTheme;
            if (prospect.ClassAdjustedOverall <= 0)
            {
                prospect.ClassAdjustedOverall = prospect.Overall;
            }

            if (prospect.ClassAdjustedPotential <= 0)
            {
                prospect.ClassAdjustedPotential = prospect.Potential;
            }
        }
    }

    private static int GetProspectRank(ProspectData prospect)
    {
        if (prospect == null)
        {
            return 999;
        }

        if (prospect.DraftRank > 0)
        {
            return prospect.DraftRank;
        }

        if (prospect.DraftPickOverall > 0)
        {
            return prospect.DraftPickOverall;
        }

        return prospect.ProjectedPick > 0 ? prospect.ProjectedPick : 999;
    }
}
