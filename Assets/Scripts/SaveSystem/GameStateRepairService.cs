using System;
using System.Collections.Generic;

public static class GameStateRepairService
{
    public static GameStateValidationReportData RepairSafeIssues(GameState state)
    {
        if (state == null)
        {
            return GameStateValidationService.Validate(state);
        }

        if (SaveMigrationConfig.RequiresMigration(state.SaveVersion))
        {
            SaveMigrationService.Migrate(state);
        }

        GameStateValidationReportData repairLog = new GameStateValidationReportData
        {
            SaveVersion = state.SaveVersion,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        RepairCore(state, repairLog);
        RepairLeagueIdentity(state, repairLog);
        RepairTeams(state, repairLog);
        RepairPlayers(state, repairLog);
        RepairRetiredPlayerState(state, repairLog);
        RepairJerseyNumbers(state, repairLog);
        RepairDuplicateFreeAgents(state, repairLog);
        RepairRosterStatuses(state, repairLog);
        RepairCpuLineups(state, repairLog);
        RepairSpecialTeams(state, repairLog);
        RepairCaptains(state, repairLog);
        RepairStaff(state, repairLog);
        RepairOwnerGoals(state, repairLog);
        RepairHistoryDuplicates(state, repairLog);
        RepairNewsDuplicates(state, repairLog);
        RepairAlphaBalanceReports(state, repairLog);
        RepairBalanceRanges(state, repairLog);

        GameStateValidationReportData finalReport = GameStateValidationService.Validate(state);
        int repairs = CountRepairIssues(repairLog);
        finalReport.RepairedCount += repairs;
        if (repairs > 0)
        {
            finalReport.Summary += " | Safe repairs applied: " + repairs;
        }

        state.LastValidationReport = finalReport;
        state.LastStabilityCheckAtUtc = DateTime.UtcNow.ToString("o");
        return finalReport;
    }

    public static void RepairCore(GameState state, GameStateValidationReportData report)
    {
        if (state.Teams == null)
        {
            state.Teams = new List<TeamData>();
            AddRepair(report, "Team", "Created missing Teams list");
        }

        if ((string.IsNullOrEmpty(state.SelectedTeamId) || FindTeam(state, state.SelectedTeamId) == null)
            && state.Teams.Count > 0
            && state.Teams[0] != null)
        {
            state.SelectedTeamId = state.Teams[0].Id;
            AddRepair(report, "Save", "SelectedTeamId repaired to first available team");
        }

        state.EnsureMatchHistory();
        state.EnsureTradeHistory();
        state.EnsureFreeAgentHistory();
        state.EnsureDraftData();
        state.EnsureProspectSigningHistory();
        state.EnsureWaiverWire();
        state.EnsureInjuryHistory();
        state.EnsureCareerProgress();
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

        if (state.LeagueRules == null)
        {
            state.LeagueRules = LeagueRulesConfig.CreateDefaultRules();
            AddRepair(report, "Save", "Created missing LeagueRules");
        }

        if (state.LeagueCalendar == null)
        {
            state.LeagueCalendar = LeagueCalendarConfig.CreateDefaultCalendar();
            AddRepair(report, "Save", "Created missing LeagueCalendar");
        }
    }

    public static void RepairLeagueIdentity(GameState state, GameStateValidationReportData report)
    {
        if (state == null)
        {
            return;
        }

        if (!TeamIdentityService.TeamsMatchFictionalLeague(state.Teams))
        {
            AddRepair(report, "League", "Skipped league identity repair for incompatible team list");
            return;
        }

        bool changed = !TeamIdentityService.IsCurrentLeagueIdentity(state)
            || state.LeagueDisplayName != FictionalLeagueConfig.LeagueDisplayName
            || state.GameDisplayName != FictionalLeagueConfig.GameTitle;
        TeamIdentityService.EnsureGameStateIdentity(state);
        if (changed)
        {
            AddRepair(report, "League", "Updated league identity to " + FictionalLeagueConfig.LeagueDisplayName);
        }
    }

    public static void RepairTeams(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null)
            {
                continue;
            }

            if (team.Players == null)
            {
                team.Players = new List<PlayerData>();
                AddRepair(report, "Team", "Created missing Players list for " + GetTeamName(team));
            }

            if (team.DraftRights == null)
            {
                team.DraftRights = new List<ProspectData>();
                AddRepair(report, "Draft", "Created missing DraftRights list for " + GetTeamName(team));
            }

            team.EnsureRetiredNumbersData();

            if (team.Tactics == null)
            {
                team.Tactics = new TeamTacticsData();
                AddRepair(report, "Team", "Created missing tactics for " + GetTeamName(team));
            }

            if (team.Lineup != null)
            {
                team.Lineup.EnsureCollections();
            }

            if (team.SpecialTeams != null)
            {
                team.SpecialTeams.EnsureCollections();
            }

            CoachingStaffService.EnsureStaffForTeam(team);
            LeadershipService.EnsureLeadershipForTeam(team);
            ChemistryService.EnsureChemistryForTeam(team);
        }
    }

    public static void RepairPlayers(GameState state, GameStateValidationReportData report)
    {
        int repaired = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (RepairPlayer(player, team.Id, false))
                {
                    repaired++;
                }
            }
        }

        if (state.FreeAgentPool != null && state.FreeAgentPool.FreeAgents != null)
        {
            foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
            {
                if (RepairPlayer(player, "", true))
                {
                    repaired++;
                }
            }
        }

        if (repaired > 0)
        {
            AddRepair(report, "Player", "Repaired player defaults: " + repaired);
        }
    }

    public static void RepairDuplicateFreeAgents(GameState state, GameStateValidationReportData report)
    {
        if (state.FreeAgentPool == null)
        {
            state.FreeAgentPool = new FreeAgentPoolData();
            AddRepair(report, "FreeAgency", "Created missing FreeAgentPool");
            return;
        }

        state.FreeAgentPool.EnsureFreeAgents();
        HashSet<string> teamPlayerIds = new HashSet<string>();
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && !string.IsNullOrEmpty(player.Id))
                {
                    teamPlayerIds.Add(player.Id);
                }
            }
        }

        HashSet<string> seenFreeAgents = new HashSet<string>();
        int removed = 0;
        for (int i = state.FreeAgentPool.FreeAgents.Count - 1; i >= 0; i--)
        {
            PlayerData player = state.FreeAgentPool.FreeAgents[i];
            if (player == null)
            {
                state.FreeAgentPool.FreeAgents.RemoveAt(i);
                removed++;
                continue;
            }

            if (!string.IsNullOrEmpty(player.Id) && (teamPlayerIds.Contains(player.Id) || !seenFreeAgents.Add(player.Id)))
            {
                state.FreeAgentPool.FreeAgents.RemoveAt(i);
                removed++;
            }
            else
            {
                player.RosterStatus = RosterStatusConfig.FreeAgent;
                player.TeamId = "";
            }
        }

        if (removed > 0)
        {
            AddRepair(report, "FreeAgency", "Removed duplicate FreeAgentPool entries: " + removed);
        }
    }

    public static void RepairRetiredPlayerState(GameState state, GameStateValidationReportData report)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureRetirementHistory();
        int moved = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

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
                RetirementService.AddRetiredPlayer(state, RetirementService.BuildRetiredPlayerData(state, team, player, player.RetirementReason));
                RetirementService.RemoveRetiredPlayerFromActiveSystems(state, team, player);
                moved++;
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
                RetirementService.AddRetiredPlayer(state, RetirementService.BuildRetiredPlayerData(state, null, player, player.RetirementReason));
                RetirementService.RemoveRetiredPlayerFromActiveSystems(state, null, player);
                moved++;
            }
        }

        RemoveDuplicateRetiredHistory(state, report);
        if (moved > 0)
        {
            AddRepair(report, "Retirement", "Moved retired players out of active systems: " + moved);
        }
    }

    public static void RepairJerseyNumbers(GameState state, GameStateValidationReportData report)
    {
        int teams = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null)
            {
                continue;
            }

            JerseyNumberService.EnsureJerseyNumbersForTeam(team);
            teams++;
        }

        if (teams > 0)
        {
            AddRepair(report, "Retirement", "Ensured jersey numbers for teams: " + teams);
        }
    }

    public static void RepairRosterStatuses(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            int repaired = 0;
            foreach (PlayerData player in team.Players)
            {
                if (player == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(player.RosterStatus) || !RosterStatusConfig.IsValidRosterStatus(player.RosterStatus) || player.RosterStatus == RosterStatusConfig.FreeAgent)
                {
                    player.RosterStatus = RosterStatusConfig.NHL;
                    repaired++;
                }

                if (string.IsNullOrEmpty(player.TeamId))
                {
                    player.TeamId = team.Id;
                }
            }

            if (repaired > 0)
            {
                AddRepair(report, "Roster", "Repaired roster status for " + repaired + " players on " + GetTeamName(team));
            }
        }
    }

    public static void RepairCpuLineups(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || IsUserTeam(state, team))
            {
                continue;
            }

            if (!LineupService.ValidateLineup(team, out string _))
            {
                team.Lineup = LineupService.BuildAutoLineup(team);
                LineupService.ValidateLineup(team, out string message);
                AddRepair(report, "Lineup", "Auto built CPU lineup for " + GetTeamName(team) + ": " + message);
            }
        }
    }

    public static void RepairSpecialTeams(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || IsUserTeam(state, team))
            {
                continue;
            }

            if (!SpecialTeamsService.ValidateSpecialTeams(team, out string _))
            {
                team.SpecialTeams = SpecialTeamsService.BuildAutoSpecialTeams(team);
                SpecialTeamsService.ValidateSpecialTeams(team, out string message);
                AddRepair(report, "SpecialTeams", "Auto built CPU special teams for " + GetTeamName(team) + ": " + message);
            }
        }
    }

    public static void RepairCaptains(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null)
            {
                continue;
            }

            LeadershipService.EnsureLeadershipForTeam(team);
            if (!IsUserTeam(state, team) && team.LeadershipData != null && string.IsNullOrEmpty(team.LeadershipData.CaptainPlayerId))
            {
                CaptaincyActionResultData result = LeadershipService.AutoAssignCaptains(team);
                if (result != null && result.Success)
                {
                    AddRepair(report, "Team", "Auto assigned CPU captains for " + GetTeamName(team));
                }
            }
        }
    }

    public static void RepairStaff(GameState state, GameStateValidationReportData report)
    {
        CoachingStaffService.EnsureStaffForTeams(state == null ? null : state.Teams);
        AddRepair(report, "Team", "Ensured coaching staff containers");
    }

    public static void RepairOwnerGoals(GameState state, GameStateValidationReportData report)
    {
        OwnerGoalService.EnsureOwnerProfiles(state);
        GmCareerService.EnsureGmCareer(state);
        AddRepair(report, "Owner", "Ensured owner profiles and GM career");
    }

    public static void RepairHistoryDuplicates(GameState state, GameStateValidationReportData report)
    {
        if (state == null || state.LeagueHistory == null)
        {
            return;
        }

        HashSet<int> seen = new HashSet<int>();
        int removed = 0;
        for (int i = state.LeagueHistory.Count - 1; i >= 0; i--)
        {
            LeagueSeasonHistoryData history = state.LeagueHistory[i];
            if (history == null || seen.Contains(history.SeasonStartYear))
            {
                state.LeagueHistory.RemoveAt(i);
                removed++;
                continue;
            }

            seen.Add(history.SeasonStartYear);
        }

        if (removed > 0)
        {
            AddRepair(report, "History", "Removed duplicate LeagueHistory entries: " + removed);
        }
    }

    public static void RepairNewsDuplicates(GameState state, GameStateValidationReportData report)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureNewsFeed();
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
            if (!string.IsNullOrEmpty(item.RelatedId) && !seen.Add(key))
            {
                state.NewsFeed.Items.RemoveAt(i);
                removed++;
            }
        }

        while (state.NewsFeed.Items.Count > NewsConfig.MaxNewsItemsToKeep)
        {
            state.NewsFeed.Items.RemoveAt(0);
            removed++;
        }

        if (removed > 0)
        {
            AddRepair(report, "News", "Removed duplicate or excess news entries: " + removed);
        }
    }

    public static void RepairAlphaBalanceReports(GameState state, GameStateValidationReportData report)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureAlphaBalanceReports();
        HashSet<string> seen = new HashSet<string>();
        int removed = 0;
        for (int i = state.AlphaBalanceReportHistory.Count - 1; i >= 0; i--)
        {
            AlphaBalanceReportData alphaReport = state.AlphaBalanceReportHistory[i];
            if (alphaReport == null)
            {
                state.AlphaBalanceReportHistory.RemoveAt(i);
                removed++;
                continue;
            }

            alphaReport.EnsureCollections();
            if (!string.IsNullOrEmpty(alphaReport.ReportId) && !seen.Add(alphaReport.ReportId))
            {
                state.AlphaBalanceReportHistory.RemoveAt(i);
                removed++;
            }
        }

        while (state.AlphaBalanceReportHistory.Count > AlphaBalanceConfig.MaxReportsToKeep)
        {
            state.AlphaBalanceReportHistory.RemoveAt(0);
            removed++;
        }

        if (removed > 0)
        {
            AddRepair(report, "AlphaBalance", "Trimmed duplicate or excess alpha balance reports: " + removed);
        }
    }

    public static void RepairBalanceRanges(GameState state, GameStateValidationReportData report)
    {
        if (state == null)
        {
            return;
        }

        int repaired = 0;
        if (state.LastRetirementProcessedSeasonStartYear > state.CurrentSeasonStartYear)
        {
            state.LastRetirementProcessedSeasonStartYear = state.CurrentSeasonStartYear;
            repaired++;
        }

        if (state.GmCareer != null && state.GmCareer.LastJobSecurityEvaluationSeasonStartYear > state.CurrentSeasonStartYear)
        {
            state.GmCareer.LastJobSecurityEvaluationSeasonStartYear = state.CurrentSeasonStartYear;
            repaired++;
        }

        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null)
            {
                continue;
            }

            if (team.Chemistry != null)
            {
                int chemistry = team.Chemistry.TeamChemistryScore;
                if (chemistry < ChemistryConfig.MinChemistry)
                {
                    team.Chemistry.TeamChemistryScore = ChemistryConfig.MinChemistry;
                    repaired++;
                }
                else if (chemistry > ChemistryConfig.MaxChemistry)
                {
                    team.Chemistry.TeamChemistryScore = ChemistryConfig.MaxChemistry;
                    repaired++;
                }
            }

            if (team.Staff != null && ClampStaffEffects(team.Staff))
            {
                repaired++;
            }

            if (team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && player.LastCareerStatsUpdatedSeasonStartYear > state.CurrentSeasonStartYear)
                {
                    player.LastCareerStatsUpdatedSeasonStartYear = state.CurrentSeasonStartYear;
                    repaired++;
                }
            }
        }

        if (repaired > 0)
        {
            AddRepair(report, "AlphaBalance", "Clamped alpha/long-career balance fields: " + repaired);
        }
    }

    private static bool RepairPlayer(PlayerData player, string teamId, bool freeAgent)
    {
        if (player == null)
        {
            return false;
        }

        bool changed = false;
        if (string.IsNullOrEmpty(player.Id))
        {
            player.Id = "player-" + Guid.NewGuid().ToString("N");
            changed = true;
        }

        if (string.IsNullOrEmpty(player.FirstName))
        {
            player.FirstName = "Unknown";
            changed = true;
        }

        if (string.IsNullOrEmpty(player.LastName))
        {
            player.LastName = "Player";
            changed = true;
        }

        if (!IsValidPosition(player.Position))
        {
            player.Position = "C";
            changed = true;
        }

        changed |= ClampField(ref player.Age, player.Age <= 0 ? 25 : player.Age, 16, 45);
        changed |= ClampField(ref player.Overall, player.Overall <= 0 ? 60 : player.Overall, 1, 99);
        changed |= ClampField(ref player.Potential, player.Potential <= 0 ? player.Overall : player.Potential, player.Overall, 99);
        changed |= ClampField(ref player.Salary, Math.Max(0, player.Salary), 0, SalaryCapConfig.MaximumPlayerSalary);
        changed |= ClampField(ref player.ContractYearsRemaining, Math.Max(0, player.ContractYearsRemaining), 0, 8);
        changed |= ClampField(ref player.Condition, player.Condition <= 0 ? 100 : player.Condition, 0, 100);
        changed |= ClampField(ref player.Fatigue, player.Fatigue, 0, 100);

        if (player.InjuryDaysRemaining <= 0 && player.IsInjured)
        {
            player.IsInjured = false;
            player.InjuryDaysRemaining = 0;
            changed = true;
        }

        if (player.CareerAwardIds == null)
        {
            player.CareerAwardIds = new List<string>();
            changed = true;
        }

        if (freeAgent)
        {
            if (player.RosterStatus != RosterStatusConfig.FreeAgent)
            {
                player.RosterStatus = RosterStatusConfig.FreeAgent;
                changed = true;
            }

            if (!string.IsNullOrEmpty(player.TeamId))
            {
                player.TeamId = "";
                changed = true;
            }
        }
        else if (string.IsNullOrEmpty(player.TeamId))
        {
            player.TeamId = teamId;
            changed = true;
        }

        return changed;
    }

    private static bool ClampField(ref int field, int value, int min, int max)
    {
        int clamped = value < min ? min : value > max ? max : value;
        if (field == clamped)
        {
            return false;
        }

        field = clamped;
        return true;
    }

    private static bool ClampStaffEffects(TeamStaffData staff)
    {
        bool changed = false;
        changed |= ClampField(ref staff.StaffOffenseImpact, staff.StaffOffenseImpact, StaffConfig.MinStaffRatingModifier, StaffConfig.MaxStaffRatingModifier);
        changed |= ClampField(ref staff.StaffDefenseImpact, staff.StaffDefenseImpact, StaffConfig.MinStaffRatingModifier, StaffConfig.MaxStaffRatingModifier);
        changed |= ClampField(ref staff.StaffPowerPlayImpact, staff.StaffPowerPlayImpact, StaffConfig.MinStaffRatingModifier, StaffConfig.MaxStaffRatingModifier);
        changed |= ClampField(ref staff.StaffPenaltyKillImpact, staff.StaffPenaltyKillImpact, StaffConfig.MinStaffRatingModifier, StaffConfig.MaxStaffRatingModifier);
        changed |= ClampField(ref staff.StaffDevelopmentImpact, staff.StaffDevelopmentImpact, StaffConfig.MinStaffRatingModifier, StaffConfig.MaxStaffRatingModifier);
        changed |= ClampField(ref staff.StaffGoalieDevelopmentImpact, staff.StaffGoalieDevelopmentImpact, StaffConfig.MinStaffRatingModifier, StaffConfig.MaxStaffRatingModifier);
        changed |= ClampField(ref staff.StaffMoraleImpact, staff.StaffMoraleImpact, StaffConfig.MinStaffRatingModifier, StaffConfig.MaxStaffRatingModifier);
        changed |= ClampField(ref staff.StaffChemistryImpact, staff.StaffChemistryImpact, StaffConfig.MinStaffRatingModifier, StaffConfig.MaxStaffRatingModifier);
        changed |= ClampField(ref staff.StaffDisciplineImpact, staff.StaffDisciplineImpact, StaffConfig.MinStaffRatingModifier, StaffConfig.MaxStaffRatingModifier);
        changed |= ClampField(ref staff.StaffTacticalFitImpact, staff.StaffTacticalFitImpact, StaffConfig.MinStaffRatingModifier, StaffConfig.MaxStaffRatingModifier);
        return changed;
    }

    private static void RemoveDuplicateRetiredHistory(GameState state, GameStateValidationReportData report)
    {
        int removed = 0;
        if (state == null)
        {
            return;
        }

        state.EnsureRetirementHistory();
        HashSet<string> retiredPlayerIds = new HashSet<string>();
        for (int i = state.RetiredPlayers.Players.Count - 1; i >= 0; i--)
        {
            RetiredPlayerData player = state.RetiredPlayers.Players[i];
            if (player == null || string.IsNullOrEmpty(player.PlayerId) || !retiredPlayerIds.Add(player.PlayerId))
            {
                state.RetiredPlayers.Players.RemoveAt(i);
                removed++;
            }
        }

        HashSet<string> hofPlayerIds = new HashSet<string>();
        for (int i = state.HallOfFame.Inductees.Count - 1; i >= 0; i--)
        {
            HallOfFameInducteeData inductee = state.HallOfFame.Inductees[i];
            if (inductee == null || string.IsNullOrEmpty(inductee.PlayerId) || !hofPlayerIds.Add(inductee.PlayerId))
            {
                state.HallOfFame.Inductees.RemoveAt(i);
                removed++;
            }
        }

        HashSet<string> leagueNumbers = new HashSet<string>();
        for (int i = state.LeagueRetiredNumbers.Count - 1; i >= 0; i--)
        {
            RetiredNumberData retiredNumber = state.LeagueRetiredNumbers[i];
            string key = retiredNumber == null ? "" : retiredNumber.TeamId + "|" + retiredNumber.JerseyNumber;
            if (retiredNumber == null || string.IsNullOrEmpty(key) || !leagueNumbers.Add(key))
            {
                state.LeagueRetiredNumbers.RemoveAt(i);
                removed++;
            }
        }

        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null)
            {
                continue;
            }

            team.EnsureRetiredNumbersData();
            HashSet<int> teamNumbers = new HashSet<int>();
            for (int i = team.RetiredNumbersData.RetiredNumbers.Count - 1; i >= 0; i--)
            {
                RetiredNumberData retiredNumber = team.RetiredNumbersData.RetiredNumbers[i];
                if (retiredNumber == null || retiredNumber.JerseyNumber <= 0 || !teamNumbers.Add(retiredNumber.JerseyNumber))
                {
                    team.RetiredNumbersData.RetiredNumbers.RemoveAt(i);
                    removed++;
                }
            }
        }

        state.RetiredPlayers.TotalRetiredPlayers = state.RetiredPlayers.Players.Count;
        state.HallOfFame.TotalInductees = state.HallOfFame.Inductees.Count;
        if (removed > 0)
        {
            AddRepair(report, "Retirement", "Removed duplicate retired history entries: " + removed);
        }
    }

    private static void AddRepair(GameStateValidationReportData report, string category, string message)
    {
        GameStateValidationService.AddIssue(
            report,
            GameStateValidationService.SeverityInfo,
            category,
            message,
            suggestedRepair: "Applied safe repair",
            canAutoRepair: true);
        if (report != null && report.Issues != null && report.Issues.Count > 0)
        {
            report.Issues[report.Issues.Count - 1].WasRepaired = true;
        }
    }

    private static int CountRepairIssues(GameStateValidationReportData report)
    {
        if (report == null || report.Issues == null)
        {
            return 0;
        }

        int count = 0;
        foreach (ValidationIssueData issue in report.Issues)
        {
            if (issue != null && issue.WasRepaired)
            {
                count++;
            }
        }

        return count;
    }

    private static IEnumerable<TeamData> SafeTeams(GameState state)
    {
        return state == null || state.Teams == null ? new List<TeamData>() : state.Teams;
    }

    private static TeamData FindTeam(GameState state, string teamId)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
    }

    private static bool IsUserTeam(GameState state, TeamData team)
    {
        return state != null && team != null && team.Id == state.SelectedTeamId;
    }

    private static bool IsValidPosition(string position)
    {
        return position == "C" || position == "LW" || position == "RW" || position == "D" || position == "G";
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
