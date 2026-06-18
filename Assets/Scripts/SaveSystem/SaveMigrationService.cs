using System;
using System.Collections.Generic;
using UnityEngine;

public static class SaveMigrationService
{
    public static MigrationReportData Migrate(GameState state)
    {
        MigrationReportData report = new MigrationReportData
        {
            FromSaveVersion = state == null ? 0 : state.SaveVersion,
            ToSaveVersion = SaveMigrationConfig.CurrentSaveVersion,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        if (state == null)
        {
            report.Status = SaveMigrationConfig.MigrationStatusFailed;
            AddError(report, "GameState is null");
            return report;
        }

        if (!SaveMigrationConfig.RequiresMigration(state.SaveVersion))
        {
            report.Status = SaveMigrationConfig.MigrationStatusNotNeeded;
            MigrateLeagueIdentity(state, report);
            state.LastMigrationReport = report;
            state.LastStabilityCheckAtUtc = DateTime.UtcNow.ToString("o");
            return report;
        }

        try
        {
            MigrateCore(state, report);
            MigrateLeagueIdentity(state, report);
            MigrateTeams(state, report);
            MigratePlayers(state, report);
            MigrateRetirementData(state, report);
            MigrateLeagueRules(state, report);
            MigrateSeason(state, report);
            MigrateRosterSystems(state, report);
            MigrateContracts(state, report);
            MigrateDraftAndScouting(state, report);
            MigrateHistoryAndNews(state, report);
            MigrateOwnerAndGmCareer(state, report);

            state.SaveVersion = SaveMigrationConfig.CurrentSaveVersion;
            report.Status = SaveMigrationConfig.MigrationStatusMigrated;
        }
        catch (Exception exception)
        {
            report.Status = SaveMigrationConfig.MigrationStatusFailed;
            AddError(report, "Migration failed: " + exception.Message);
            Debug.LogError("SaveMigrationService: " + exception);
        }

        report.EnsureCollections();
        state.LastMigrationReport = report;
        state.LastStabilityCheckAtUtc = DateTime.UtcNow.ToString("o");
        return report;
    }

    public static void MigrateLeagueIdentity(GameState state, MigrationReportData report)
    {
        if (state == null)
        {
            return;
        }

        if (TeamIdentityService.TeamsMatchFictionalLeague(state.Teams))
        {
            bool changed = state.LeagueIdentityId != FictionalLeagueConfig.LeagueIdentityId
                || state.LeagueIdentityVersion != FictionalLeagueConfig.LeagueIdentityVersion
                || state.LeagueDisplayName != FictionalLeagueConfig.LeagueDisplayName
                || state.GameDisplayName != FictionalLeagueConfig.GameTitle;
            TeamIdentityService.EnsureGameStateIdentity(state);
            if (changed)
            {
                AddRepair(report, "League identity was updated to " + FictionalLeagueConfig.LeagueDisplayName);
            }

            return;
        }

        AddWarning(report, "Save teams do not match " + FictionalLeagueConfig.LeagueDisplayName + "; old league saves are incompatible and are not remapped");
    }

    public static void MigrateCore(GameState state, MigrationReportData report)
    {
        if (state.Teams == null)
        {
            state.Teams = new List<TeamData>();
            AddWarning(report, "Teams list was missing and was recreated empty");
        }

        if (string.IsNullOrEmpty(state.SelectedTeamId) && state.Teams.Count > 0 && state.Teams[0] != null)
        {
            state.SelectedTeamId = state.Teams[0].Id;
            AddRepair(report, "SelectedTeamId was empty and was set to first team");
        }

        if (state.CurrentSeasonStartYear <= 0)
        {
            state.CurrentSeasonStartYear = SalaryCapConfig.RulesSeasonStartYear;
            AddRepair(report, "CurrentSeasonStartYear was set to ruleset start year");
        }

        if (state.CurrentSeasonEndYear <= 0)
        {
            state.CurrentSeasonEndYear = state.CurrentSeasonStartYear + 1;
            AddRepair(report, "CurrentSeasonEndYear was repaired");
        }

        state.EnsureCareerProgress();
        state.EnsureMatchHistory();
        state.EnsureTradeHistory();
        state.EnsureFreeAgentHistory();
        state.EnsureProspectSigningHistory();
        state.EnsureWaiverWire();
        state.EnsureInjuryHistory();
        state.EnsureSeasonHistory();
        state.EnsurePlayerDevelopmentHistory();
        state.EnsureCpuRosterManagementHistory();
        state.EnsureTeamTradeProfiles();
        state.EnsureScoutingHistory();
        state.EnsureMoraleHistory();
        state.EnsureContractExtensionHistory();
        state.EnsureFreeAgencyOfferHistory();
        state.EnsureOwnerEvaluationHistory();
        state.EnsureLeagueHistory();
        state.EnsureNewsFeed();
        state.EnsureGmCareerData();
        state.EnsureRetirementHistory();
        state.EnsureTutorialData();
        state.EnsureAlphaBalanceReports();
        state.EnsureAndroidPerformanceData();
    }

    public static void MigrateTeams(GameState state, MigrationReportData report)
    {
        if (state == null || state.Teams == null)
        {
            return;
        }

        for (int i = 0; i < state.Teams.Count; i++)
        {
            TeamData team = state.Teams[i];
            if (team == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(team.Id))
            {
                team.Id = "team-" + i;
                AddRepair(report, "Team with missing Id was assigned " + team.Id);
            }

            if (string.IsNullOrEmpty(team.Name))
            {
                team.Name = "Unknown Team";
                AddRepair(report, "Team " + team.Id + " missing name repaired");
            }

            if (string.IsNullOrEmpty(team.City))
            {
                team.City = "Unknown";
            }

            if (string.IsNullOrEmpty(team.Abbreviation))
            {
                team.Abbreviation = BuildAbbreviation(team);
            }

            team.EnsurePlayers();
            team.EnsureDraftRights();
            team.EnsureLineupData();
            team.EnsureSpecialTeamsData();
            team.EnsureTacticsData();
            team.EnsureRetiredNumbersData();

            if (team.Tactics == null)
            {
                team.Tactics = new TeamTacticsData();
            }

            if (team.LeaderShipSafeMissing())
            {
                LeadershipService.EnsureLeadershipForTeam(team);
            }

            CoachingStaffService.EnsureStaffForTeam(team);
            ChemistryService.EnsureChemistryForTeam(team);
            OwnerGoalService.EnsureOwnerProfile(state, team);
        }
    }

    public static void MigratePlayers(GameState state, MigrationReportData report)
    {
        if (state == null)
        {
            return;
        }

        int generatedIds = 0;
        if (state.Teams != null)
        {
            foreach (TeamData team in state.Teams)
            {
                if (team == null || team.Players == null)
                {
                    continue;
                }

                for (int i = 0; i < team.Players.Count; i++)
                {
                    NormalizePlayer(team.Players[i], team.Id, false, ref generatedIds);
                }
            }
        }

        if (state.FreeAgentPool != null)
        {
            state.FreeAgentPool.EnsureFreeAgents();
            for (int i = 0; i < state.FreeAgentPool.FreeAgents.Count; i++)
            {
                NormalizePlayer(state.FreeAgentPool.FreeAgents[i], "", true, ref generatedIds);
            }
        }

        if (generatedIds > 0)
        {
            AddRepair(report, "Generated missing player ids: " + generatedIds);
        }
    }

    public static void MigrateRetirementData(GameState state, MigrationReportData report)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureRetirementHistory();
        int movedRetiredPlayers = 0;
        if (state.Teams != null)
        {
            foreach (TeamData team in state.Teams)
            {
                if (team == null)
                {
                    continue;
                }

                team.EnsureRetiredNumbersData();
                team.EnsurePlayers();
                List<PlayerData> retiredPlayers = new List<PlayerData>();
                foreach (PlayerData player in team.Players)
                {
                    if (player != null && player.IsRetired)
                    {
                        retiredPlayers.Add(player);
                    }
                }

                foreach (PlayerData player in retiredPlayers)
                {
                    RetiredPlayerData retiredPlayer = RetirementService.BuildRetiredPlayerData(state, team, player, player.RetirementReason);
                    RetirementService.AddRetiredPlayer(state, retiredPlayer);
                    RetirementService.RemoveRetiredPlayerFromActiveSystems(state, team, player);
                    movedRetiredPlayers++;
                }
            }
        }

        if (state.FreeAgentPool != null && state.FreeAgentPool.FreeAgents != null)
        {
            List<PlayerData> retiredFreeAgents = new List<PlayerData>();
            foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
            {
                if (player != null && player.IsRetired)
                {
                    retiredFreeAgents.Add(player);
                }
            }

            foreach (PlayerData player in retiredFreeAgents)
            {
                RetiredPlayerData retiredPlayer = RetirementService.BuildRetiredPlayerData(state, null, player, player.RetirementReason);
                RetirementService.AddRetiredPlayer(state, retiredPlayer);
                RetirementService.RemoveRetiredPlayerFromActiveSystems(state, null, player);
                movedRetiredPlayers++;
            }
        }

        RetirementService.EnsureRetirementData(state);
        if (movedRetiredPlayers > 0)
        {
            AddRepair(report, "Moved retired players from active systems into history: " + movedRetiredPlayers);
        }
    }

    public static void MigrateLeagueRules(GameState state, MigrationReportData report)
    {
        if (state.LeagueRules == null)
        {
            state.LeagueRules = LeagueRulesConfig.CreateDefaultRules();
            AddRepair(report, "LeagueRules were missing and recreated from config");
        }

        LeagueRulesData defaults = LeagueRulesConfig.CreateDefaultRules();
        if (state.LeagueRules.RegularSeasonGamesPerTeam != SalaryCapConfig.TargetGamesPerTeam)
        {
            state.LeagueRules.RegularSeasonGamesPerTeam = SalaryCapConfig.TargetGamesPerTeam;
            AddRepair(report, "RegularSeasonGamesPerTeam repaired to 84");
        }

        state.LeagueRules.Ruleset = defaults.Ruleset;
        state.LeagueRules.RulesetName = defaults.RulesetName;
        state.LeagueRules.Cba = defaults.Cba;
        state.LeagueRules.CbaName = defaults.CbaName;
        state.LeagueRules.RulesSeasonStartYear = defaults.RulesSeasonStartYear;
        state.LeagueRules.PreseasonGamesPerTeam = defaults.PreseasonGamesPerTeam;
        state.LeagueRules.SalaryCapUpperLimit = SalaryCapConfig.SalaryCapUpperLimit;
        state.LeagueRules.SalaryCapLowerLimit = SalaryCapConfig.SalaryCapLowerLimit;
        state.LeagueRules.LeagueMinimumSalary = SalaryCapConfig.LeagueMinimumSalary;
        state.LeagueRules.MaximumPlayerSalary = SalaryCapConfig.MaximumPlayerSalary;
        state.LeagueRules.MaxContractYearsWithOwnTeam = SalaryCapConfig.MaxContractYearsWithOwnTeam;
        state.LeagueRules.MaxContractYearsFreeAgent = SalaryCapConfig.MaxContractYearsFreeAgent;
        state.LeagueRules.MinRosterSize = SalaryCapConfig.MinRosterSize;
        state.LeagueRules.MaxRosterSize = SalaryCapConfig.MaxRosterSize;

        if (state.LeagueCalendar == null)
        {
            state.LeagueCalendar = LeagueCalendarConfig.CreateDefaultCalendar();
            AddRepair(report, "LeagueCalendar was missing and recreated from config");
        }
    }

    public static void MigrateSeason(GameState state, MigrationReportData report)
    {
        if (state.Season == null)
        {
            AddWarning(report, "Season is missing; GameSession can recreate it when needed");
            return;
        }

        state.Season.EnsureCollections();
        PlayerStatsService.EnsurePlayerStats(state.Season);
        if (state.Season.TargetGamesPerTeam != SalaryCapConfig.TargetGamesPerTeam)
        {
            state.Season.TargetGamesPerTeam = SalaryCapConfig.TargetGamesPerTeam;
            AddRepair(report, "Season TargetGamesPerTeam repaired to 84");
        }
    }

    public static void MigrateRosterSystems(GameState state, MigrationReportData report)
    {
        if (state == null || state.Teams == null)
        {
            return;
        }

        TeamRosterService.EnsureRosterStatusesForTeams(state.Teams);
        WaiverEligibilityService.EnsureWaiverEligibilityForTeams(state.Teams);
        PlayerFatigueService.EnsureFatigueForTeams(state.Teams);
        InjuryService.EnsureInjuryFieldsForTeams(state.Teams);
        PlayerRoleService.EnsureRolesForTeams(state.Teams);
        IceTimeService.EnsureUsageForTeams(state.Teams);
        TacticsService.EnsureTacticsForTeams(state.Teams);
        LeadershipService.EnsureLeadershipForTeams(state.Teams);
        CoachingStaffService.EnsureStaffForTeams(state.Teams);
        ChemistryService.EnsureChemistryForTeams(state.Teams);

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            if (team.Id == state.SelectedTeamId && team.Lineup != null)
            {
                team.Lineup.EnsureCollections();
                LineupService.ValidateLineup(team, out string _);
            }
            else
            {
                LineupService.EnsureLineup(team);
            }

            if (team.Id == state.SelectedTeamId && team.SpecialTeams != null)
            {
                team.SpecialTeams.EnsureCollections();
                SpecialTeamsService.ValidateSpecialTeams(team, out string _);
            }
            else
            {
                SpecialTeamsService.EnsureSpecialTeams(team);
            }
        }
    }

    public static void MigrateContracts(GameState state, MigrationReportData report)
    {
        if (state == null)
        {
            return;
        }

        ContractGenerator.EnsureContractsForTeams(state.Teams);
        ContractExtensionService.EnsureExtensionDataForTeams(state, state.Teams);
        state.EnsureContractExtensionHistory();
        state.EnsureFreeAgentHistory();
        state.EnsureFreeAgencyOfferHistory();
    }

    public static void MigrateDraftAndScouting(GameState state, MigrationReportData report)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureDraftData();
        state.EnsureScoutingHistory();
        if (state.Teams != null)
        {
            foreach (TeamData team in state.Teams)
            {
                if (team != null)
                {
                    team.EnsureDraftRights();
                    foreach (ProspectData prospect in team.DraftRights)
                    {
                        NormalizeProspect(prospect, team.Id);
                    }
                }
            }
        }

        if (state.Draft != null && state.Draft.Prospects != null)
        {
            foreach (ProspectData prospect in state.Draft.Prospects)
            {
                NormalizeProspect(prospect, "");
            }
        }
    }

    public static void MigrateHistoryAndNews(GameState state, MigrationReportData report)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureLeagueHistory();
        state.EnsureNewsFeed();
        int historyRemoved = RemoveDuplicateLeagueHistory(state);
        int newsRemoved = RemoveDuplicateNews(state);
        if (historyRemoved > 0)
        {
            AddRepair(report, "Removed duplicate league history entries: " + historyRemoved);
        }

        if (newsRemoved > 0)
        {
            AddRepair(report, "Removed duplicate news entries: " + newsRemoved);
        }
    }

    public static void MigrateOwnerAndGmCareer(GameState state, MigrationReportData report)
    {
        if (state == null)
        {
            return;
        }

        OwnerGoalService.EnsureOwnerProfiles(state);
        TeamData selected = FindTeam(state, state.SelectedTeamId);
        if (selected == null && state.Teams != null && state.Teams.Count > 0)
        {
            state.SelectedTeamId = state.Teams[0].Id;
            AddWarning(report, "SelectedTeamId was missing from Teams and was repaired to first team");
        }

        state.EnsureGmCareerData();
        GmCareerService.EnsureGmCareer(state);
    }

    public static void AddWarning(MigrationReportData report, string message)
    {
        if (report == null)
        {
            return;
        }

        report.EnsureCollections();
        report.Warnings.Add(message);
        report.WarningsCount = report.Warnings.Count;
    }

    public static void AddRepair(MigrationReportData report, string message)
    {
        if (report == null)
        {
            return;
        }

        report.EnsureCollections();
        report.Repairs.Add(message);
        report.RepairsCount = report.Repairs.Count;
    }

    public static void AddError(MigrationReportData report, string message)
    {
        if (report == null)
        {
            return;
        }

        report.EnsureCollections();
        report.Errors.Add(message);
        report.ErrorsCount = report.Errors.Count;
    }

    private static void NormalizePlayer(PlayerData player, string teamId, bool isFreeAgent, ref int generatedIds)
    {
        if (player == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(player.Id))
        {
            player.Id = "player-" + Guid.NewGuid().ToString("N");
            generatedIds++;
        }

        if (string.IsNullOrEmpty(player.FirstName))
        {
            player.FirstName = "Unknown";
        }

        if (string.IsNullOrEmpty(player.LastName))
        {
            player.LastName = "Player";
        }

        player.Position = NormalizePosition(player.Position);
        player.Age = Clamp(player.Age <= 0 ? 25 : player.Age, 16, 45);
        player.Overall = Clamp(player.Overall <= 0 ? 60 : player.Overall, 1, 99);
        player.Potential = Clamp(player.Potential <= 0 ? player.Overall : player.Potential, player.Overall, 99);

        if (isFreeAgent)
        {
            player.TeamId = "";
            player.RosterStatus = RosterStatusConfig.FreeAgent;
            player.ContractStatus = string.IsNullOrEmpty(player.ContractStatus) ? "FreeAgent" : player.ContractStatus;
        }
        else
        {
            player.TeamId = string.IsNullOrEmpty(player.TeamId) ? teamId : player.TeamId;
            if (string.IsNullOrEmpty(player.RosterStatus) || !RosterStatusConfig.IsValidRosterStatus(player.RosterStatus))
            {
                player.RosterStatus = RosterStatusConfig.NHL;
            }

            if (string.IsNullOrEmpty(player.ContractStatus))
            {
                player.ContractStatus = "Signed";
            }
        }

        if (player.Salary < 0)
        {
            player.Salary = 0;
        }

        if (!isFreeAgent && player.ContractStatus == "Signed" && player.Salary > 0 && player.Salary < SalaryCapConfig.LeagueMinimumSalary)
        {
            player.Salary = SalaryCapConfig.LeagueMinimumSalary;
        }

        player.ContractYearsRemaining = Math.Max(0, player.ContractYearsRemaining);
        player.Condition = player.Condition <= 0 ? 100 : Clamp(player.Condition, 0, 100);
        player.Fatigue = Clamp(player.Fatigue, 0, 100);
        if (player.InjuryDaysRemaining <= 0)
        {
            player.IsInjured = false;
            player.InjuryDaysRemaining = 0;
        }

        if (player.CareerAwardIds == null)
        {
            player.CareerAwardIds = new List<string>();
        }
    }

    private static void NormalizeProspect(ProspectData prospect, string teamId)
    {
        if (prospect == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(prospect.Id))
        {
            prospect.Id = "prospect-" + Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrEmpty(prospect.FirstName))
        {
            prospect.FirstName = "Prospect";
        }

        if (string.IsNullOrEmpty(prospect.LastName))
        {
            prospect.LastName = "Player";
        }

        prospect.Position = NormalizePosition(prospect.Position);
        prospect.Age = Clamp(prospect.Age <= 0 ? 18 : prospect.Age, 16, 30);
        prospect.Overall = Clamp(prospect.Overall <= 0 ? 50 : prospect.Overall, 1, 99);
        prospect.Potential = Clamp(prospect.Potential <= 0 ? prospect.Overall : prospect.Potential, prospect.Overall, 99);
        if (prospect.IsDrafted && string.IsNullOrEmpty(prospect.DraftedByTeamId))
        {
            prospect.DraftedByTeamId = teamId;
        }
    }

    private static int RemoveDuplicateLeagueHistory(GameState state)
    {
        if (state.LeagueHistory == null)
        {
            return 0;
        }

        HashSet<int> seen = new HashSet<int>();
        int removed = 0;
        for (int i = state.LeagueHistory.Count - 1; i >= 0; i--)
        {
            LeagueSeasonHistoryData item = state.LeagueHistory[i];
            if (item == null || seen.Contains(item.SeasonStartYear))
            {
                state.LeagueHistory.RemoveAt(i);
                removed++;
                continue;
            }

            seen.Add(item.SeasonStartYear);
        }

        return removed;
    }

    private static int RemoveDuplicateNews(GameState state)
    {
        if (state.NewsFeed == null || state.NewsFeed.Items == null)
        {
            return 0;
        }

        HashSet<string> seen = new HashSet<string>();
        int removed = 0;
        for (int i = state.NewsFeed.Items.Count - 1; i >= 0; i--)
        {
            NewsItemData item = state.NewsFeed.Items[i];
            if (item == null)
            {
                state.NewsFeed.Items.RemoveAt(i);
                removed++;
                continue;
            }

            string key = item.Category + "|" + item.RelatedId;
            if (!string.IsNullOrEmpty(item.RelatedId) && seen.Contains(key))
            {
                state.NewsFeed.Items.RemoveAt(i);
                removed++;
                continue;
            }

            if (!string.IsNullOrEmpty(item.RelatedId))
            {
                seen.Add(key);
            }
        }

        while (state.NewsFeed.Items.Count > NewsConfig.MaxNewsItemsToKeep)
        {
            state.NewsFeed.Items.RemoveAt(0);
            removed++;
        }

        return removed;
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

    private static string NormalizePosition(string position)
    {
        if (position == "C" || position == "LW" || position == "RW" || position == "D" || position == "G")
        {
            return position;
        }

        return "C";
    }

    private static string BuildAbbreviation(TeamData team)
    {
        string source = string.IsNullOrEmpty(team.Name) ? team.Id : team.Name;
        if (string.IsNullOrEmpty(source))
        {
            return "UNK";
        }

        return source.Length >= 3 ? source.Substring(0, 3).ToUpperInvariant() : source.ToUpperInvariant();
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}

public static class TeamDataMigrationExtensions
{
    public static bool LeaderShipSafeMissing(this TeamData team)
    {
        return team != null && team.LeadershipData == null;
    }
}
