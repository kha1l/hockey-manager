using System;
using System.Collections.Generic;

public static class GameStateValidationService
{
    public const string SeverityInfo = "Info";
    public const string SeverityWarning = "Warning";
    public const string SeverityError = "Error";
    public const string SeverityCritical = "Critical";

    public static GameStateValidationReportData Validate(GameState state)
    {
        GameStateValidationReportData report = new GameStateValidationReportData
        {
            SaveVersion = state == null ? 0 : state.SaveVersion,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        if (state == null)
        {
            AddIssue(report, SeverityCritical, "Save", "GameState is null", suggestedRepair: "Load another save or start a new game");
            RecalculateCounts(report);
            return report;
        }

        ValidateCore(state, report);
        ValidateLeagueIdentity(state, report);
        ValidateTeams(state, report);
        ValidatePlayers(state, report);
        ValidateDuplicatePlayers(state, report);
        ValidateFreeAgentPool(state, report);
        ValidateRosters(state, report);
        ValidateLineups(state, report);
        ValidateSpecialTeams(state, report);
        ValidateSalaryCap(state, report);
        ValidateDraft(state, report);
        ValidateHistory(state, report);
        ValidateNews(state, report);
        ValidateRetirementData(state, report);
        ValidateTutorialData(state, report);
        ValidateOwnerAndGmCareer(state, report);
        ValidateAlphaBalanceReports(state, report);
        ValidateAndroidData(state, report);
        ValidateLongCareerGuards(state, report);
        ValidateTeamBalanceFields(state, report);

        PopulateInventoryCounts(state, report);
        RecalculateCounts(report);
        report.Summary = report.IssuesCount == 0
            ? "Validation passed"
            : "Found " + report.IssuesCount + " issues, " + report.CriticalCount + " critical";
        state.LastValidationReport = report;
        state.LastStabilityCheckAtUtc = DateTime.UtcNow.ToString("o");
        return report;
    }

    public static void ValidateCore(GameState state, GameStateValidationReportData report)
    {
        if (state.Teams == null || state.Teams.Count == 0)
        {
            AddIssue(report, SeverityCritical, "Team", "Teams list is empty", suggestedRepair: "Start a new game or restore save backup");
        }

        if (string.IsNullOrEmpty(state.SelectedTeamId))
        {
            AddIssue(report, SeverityError, "Save", "SelectedTeamId is empty", suggestedRepair: "Repair can set first team as selected", canAutoRepair: true);
        }
        else if (FindTeam(state, state.SelectedTeamId) == null)
        {
            AddIssue(report, SeverityError, "Save", "SelectedTeamId does not match any team", teamId: state.SelectedTeamId, suggestedRepair: "Repair can select first available team", canAutoRepair: true);
        }

        if (SaveMigrationConfig.RequiresMigration(state.SaveVersion))
        {
            AddIssue(report, SeverityWarning, "Save", "SaveVersion is older than current migration version", suggestedRepair: "Run migration", canAutoRepair: true);
        }

        if (state.LeagueRules == null)
        {
            AddIssue(report, SeverityError, "SalaryCap", "LeagueRules are missing", suggestedRepair: "Migration can recreate league rules", canAutoRepair: true);
            return;
        }

        if (state.LeagueRules.RegularSeasonGamesPerTeam != SalaryCapConfig.TargetGamesPerTeam)
        {
            AddIssue(report, SeverityError, "Save", "Regular season games per team must be 84", suggestedRepair: "Migration can repair ruleset value", canAutoRepair: true);
        }

        if (state.LeagueRules.SalaryCapUpperLimit != SalaryCapConfig.SalaryCapUpperLimit
            || state.LeagueRules.SalaryCapLowerLimit != SalaryCapConfig.SalaryCapLowerLimit
            || state.LeagueRules.LeagueMinimumSalary != SalaryCapConfig.LeagueMinimumSalary
            || state.LeagueRules.MaximumPlayerSalary != SalaryCapConfig.MaximumPlayerSalary)
        {
            AddIssue(report, SeverityError, "SalaryCap", "League salary rules do not match Continental League 2026-27 config", suggestedRepair: "Migration can repair ruleset values", canAutoRepair: true);
        }
    }

    public static void ValidateLeagueIdentity(GameState state, GameStateValidationReportData report)
    {
        List<TeamIdentityData> identities = TeamIdentityService.GetAllIdentities();
        int expectedTeamCount = identities.Count;
        int actualTeamCount = state.Teams == null ? 0 : state.Teams.Count;

        if (!TeamIdentityService.IsCurrentLeagueIdentity(state))
        {
            AddIssue(report, SeverityError, "League", "League identity mismatch", suggestedRepair: "Start a new Continental League save", canAutoRepair: TeamIdentityService.TeamsMatchFictionalLeague(state.Teams));
        }

        if (state.LeagueDisplayName != FictionalLeagueConfig.LeagueDisplayName || state.GameDisplayName != FictionalLeagueConfig.GameTitle)
        {
            AddIssue(report, SeverityWarning, "League", "League display names are missing or outdated", suggestedRepair: "Repair can update display names", canAutoRepair: true);
        }

        if (actualTeamCount != expectedTeamCount)
        {
            AddIssue(report, SeverityError, "League", "Team count must be " + expectedTeamCount + " for " + FictionalLeagueConfig.LeagueDisplayName + ", actual " + actualTeamCount, suggestedRepair: "Start a new game with the fictional league seed");
        }

        if (!TeamIdentityService.TeamsMatchFictionalLeague(state.Teams))
        {
            AddIssue(report, SeverityCritical, "League", "Team ids do not match " + FictionalLeagueConfig.LeagueDisplayName, suggestedRepair: "Old league saves are incompatible");
        }

        Dictionary<string, int> conferenceCounts = new Dictionary<string, int>();
        Dictionary<string, int> divisionCounts = new Dictionary<string, int>();
        HashSet<string> abbreviations = new HashSet<string>();

        foreach (TeamData team in SafeTeams(state))
        {
            TeamIdentityData expectedIdentity = TeamIdentityService.GetIdentityByTeamId(team.Id);
            if (expectedIdentity == null)
            {
                AddIssue(report, SeverityError, "League", "Unknown fictional team id: " + team.Id, team.Id, GetTeamName(team), suggestedRepair: "Start a new game with the current team seed");
                continue;
            }

            if (team.Identity == null)
            {
                AddIssue(report, SeverityWarning, "League", "Team identity is missing", team.Id, GetTeamName(team), suggestedRepair: "Repair can restore identity from seed", canAutoRepair: true);
            }

            if (string.IsNullOrEmpty(team.Abbreviation))
            {
                AddIssue(report, SeverityWarning, "League", "Team abbreviation is missing", team.Id, GetTeamName(team), suggestedRepair: "Repair can restore abbreviation from seed", canAutoRepair: true);
            }
            else if (!abbreviations.Add(team.Abbreviation))
            {
                AddIssue(report, SeverityWarning, "League", "Duplicate team abbreviation: " + team.Abbreviation, team.Id, GetTeamName(team), suggestedRepair: "Repair can restore abbreviations from seed", canAutoRepair: true);
            }

            if (string.IsNullOrEmpty(team.ConferenceName))
            {
                AddIssue(report, SeverityWarning, "League", "Conference is missing", team.Id, GetTeamName(team), suggestedRepair: "Repair can restore conference from seed", canAutoRepair: true);
            }
            else
            {
                Increment(conferenceCounts, team.ConferenceName);
            }

            if (string.IsNullOrEmpty(team.DivisionName))
            {
                AddIssue(report, SeverityWarning, "League", "Division is missing", team.Id, GetTeamName(team), suggestedRepair: "Repair can restore division from seed", canAutoRepair: true);
            }
            else
            {
                Increment(divisionCounts, team.DivisionName);
            }

            string logoPath = TeamIdentityService.GetLogoResourcePath(team);
            if (string.IsNullOrEmpty(logoPath))
            {
                AddIssue(report, SeverityWarning, "Assets", "Logo resource path is missing", team.Id, GetTeamName(team), suggestedRepair: "Repair can restore logo path from seed", canAutoRepair: true);
            }
            else if (!TeamAssetValidationService.HasLogo(expectedIdentity))
            {
                AddIssue(report, SeverityWarning, "Assets", "Logo sprite is missing in Resources: " + logoPath, team.Id, GetTeamName(team), suggestedRepair: "Run Tools/Continental Hockey Manager/Import Static Team Assets");
            }
        }

        ValidateCount(report, conferenceCounts, FictionalLeagueConfig.WesternConference, 16, "Conference");
        ValidateCount(report, conferenceCounts, FictionalLeagueConfig.EasternConference, 16, "Conference");
        ValidateCount(report, divisionCounts, FictionalLeagueConfig.CapitalDivision, 8, "Division");
        ValidateCount(report, divisionCounts, FictionalLeagueConfig.SouthDivision, 8, "Division");
        ValidateCount(report, divisionCounts, FictionalLeagueConfig.VolgaUralDivision, 8, "Division");
        ValidateCount(report, divisionCounts, FictionalLeagueConfig.SiberiaPacificDivision, 8, "Division");
    }

    public static void ValidateTeams(GameState state, GameStateValidationReportData report)
    {
        HashSet<string> teamIds = new HashSet<string>();
        foreach (TeamData team in SafeTeams(state))
        {
            if (string.IsNullOrEmpty(team.Id))
            {
                AddIssue(report, SeverityError, "Team", "Team has missing Id", teamName: GetTeamName(team), suggestedRepair: "Migration can generate fallback id", canAutoRepair: true);
            }
            else if (!teamIds.Add(team.Id))
            {
                AddIssue(report, SeverityCritical, "Team", "Duplicate team Id: " + team.Id, teamId: team.Id, teamName: GetTeamName(team));
            }

            if (string.IsNullOrEmpty(team.Name))
            {
                AddIssue(report, SeverityWarning, "Team", "Team name is missing", teamId: team.Id, suggestedRepair: "Migration can set fallback name", canAutoRepair: true);
            }

            if (team.Players == null)
            {
                AddIssue(report, SeverityWarning, "Team", "Team players list is null", teamId: team.Id, teamName: GetTeamName(team), suggestedRepair: "Repair can create empty list", canAutoRepair: true);
            }

            if (team.Lineup == null)
            {
                AddIssue(report, SeverityWarning, "Lineup", "Team lineup is missing", teamId: team.Id, teamName: GetTeamName(team), suggestedRepair: IsUserTeam(state, team) ? "Open Lineup" : "Repair can auto build CPU lineup", canAutoRepair: !IsUserTeam(state, team));
            }

            if (team.SpecialTeams == null)
            {
                AddIssue(report, SeverityWarning, "SpecialTeams", "Special teams are missing", teamId: team.Id, teamName: GetTeamName(team), suggestedRepair: IsUserTeam(state, team) ? "Open Tactics" : "Repair can auto build CPU special teams", canAutoRepair: !IsUserTeam(state, team));
            }

            if (team.OwnerProfile == null)
            {
                AddIssue(report, SeverityWarning, "Owner", "OwnerProfile is missing", teamId: team.Id, teamName: GetTeamName(team), suggestedRepair: "Repair can ensure owner profile", canAutoRepair: true);
            }
        }
    }

    public static void ValidatePlayers(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                ValidatePlayer(player, report, team, false);
            }
        }

        if (state.FreeAgentPool != null && state.FreeAgentPool.FreeAgents != null)
        {
            foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
            {
                ValidatePlayer(player, report, null, true);
            }
        }
    }

    public static void ValidateDuplicatePlayers(GameState state, GameStateValidationReportData report)
    {
        Dictionary<string, string> playerLocations = new Dictionary<string, string>();
        foreach (TeamData team in SafeTeams(state))
        {
            if (team.Players == null)
            {
                continue;
            }

            HashSet<string> local = new HashSet<string>();
            foreach (PlayerData player in team.Players)
            {
                if (player == null || string.IsNullOrEmpty(player.Id))
                {
                    continue;
                }

                if (!local.Add(player.Id))
                {
                    AddIssue(report, SeverityError, "Player", "Duplicate player inside team", team.Id, GetTeamName(team), player.Id, GetPlayerName(player));
                }

                if (playerLocations.TryGetValue(player.Id, out string location))
                {
                    AddIssue(report, SeverityCritical, "Player", "Player exists in multiple teams: " + location + " and " + team.Id, team.Id, GetTeamName(team), player.Id, GetPlayerName(player));
                }
                else
                {
                    playerLocations[player.Id] = team.Id;
                }
            }
        }

        if (state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null)
        {
            return;
        }

        HashSet<string> freeAgentIds = new HashSet<string>();
        foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
        {
            if (player == null || string.IsNullOrEmpty(player.Id))
            {
                continue;
            }

            if (!freeAgentIds.Add(player.Id))
            {
                AddIssue(report, SeverityWarning, "FreeAgency", "Duplicate player inside FreeAgentPool", playerId: player.Id, playerName: GetPlayerName(player), suggestedRepair: "Repair can keep one free agent entry", canAutoRepair: true);
            }

            if (playerLocations.ContainsKey(player.Id))
            {
                AddIssue(report, SeverityError, "FreeAgency", "Player exists in team and FreeAgentPool", playerId: player.Id, playerName: GetPlayerName(player), suggestedRepair: "Repair can remove duplicate from FreeAgentPool", canAutoRepair: true);
            }
        }
    }

    public static void ValidateFreeAgentPool(GameState state, GameStateValidationReportData report)
    {
        if (state.FreeAgentPool == null)
        {
            AddIssue(report, SeverityWarning, "FreeAgency", "FreeAgentPool is missing", suggestedRepair: "Repair can create empty free agent pool", canAutoRepair: true);
            return;
        }

        if (state.FreeAgentPool.FreeAgents == null)
        {
            AddIssue(report, SeverityWarning, "FreeAgency", "FreeAgentPool.FreeAgents is null", suggestedRepair: "Repair can create empty free agent list", canAutoRepair: true);
            return;
        }

        foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
        {
            if (player != null && player.RosterStatus != RosterStatusConfig.FreeAgent)
            {
                AddIssue(report, SeverityWarning, "FreeAgency", "Free agent has non-FreeAgent roster status", playerId: player.Id, playerName: GetPlayerName(player), suggestedRepair: "Repair can set FreeAgent roster status", canAutoRepair: true);
            }
        }
    }

    public static void ValidateRosters(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            int nhl = CountRoster(team, RosterStatusConfig.NHL);
            int forwards = CountPositionGroup(team, RosterStatusConfig.NHL, "Forward");
            int defense = CountPositionGroup(team, RosterStatusConfig.NHL, "Defense");
            int goalies = CountPositionGroup(team, RosterStatusConfig.NHL, "Goalie");
            bool userTeam = IsUserTeam(state, team);

            if (nhl < RosterStatusConfig.MinNhlRosterSize || nhl > RosterStatusConfig.MaxNhlRosterSize)
            {
                AddIssue(report, userTeam ? SeverityError : SeverityWarning, "Roster", "Main roster size invalid: " + nhl, team.Id, GetTeamName(team), suggestedRepair: userTeam ? "Open Organization" : "Repair can run CPU roster management", canAutoRepair: !userTeam);
            }

            if (forwards < 12 || defense < 6 || goalies < 2)
            {
                AddIssue(report, userTeam ? SeverityError : SeverityWarning, "Roster", "Roster lacks 12F/6D/2G: " + forwards + "F/" + defense + "D/" + goalies + "G", team.Id, GetTeamName(team), suggestedRepair: userTeam ? "Open Organization" : "Repair can run CPU roster management", canAutoRepair: !userTeam);
            }
        }
    }

    public static void ValidateLineups(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (!LineupService.ValidateLineup(team, out string message))
            {
                bool userTeam = IsUserTeam(state, team);
                AddIssue(report, userTeam ? SeverityError : SeverityWarning, "Lineup", message, team.Id, GetTeamName(team), suggestedRepair: userTeam ? "Open Lineup or AutoBuild" : "Repair can auto build CPU lineup", canAutoRepair: !userTeam);
            }
        }
    }

    public static void ValidateSpecialTeams(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (!SpecialTeamsService.ValidateSpecialTeams(team, out string message))
            {
                bool userTeam = IsUserTeam(state, team);
                AddIssue(report, userTeam ? SeverityWarning : SeverityWarning, "SpecialTeams", message, team.Id, GetTeamName(team), suggestedRepair: userTeam ? "Open Tactics" : "Repair can auto build CPU special teams", canAutoRepair: !userTeam);
            }
        }
    }

    public static void ValidateSalaryCap(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
            if (finance == null)
            {
                continue;
            }

            if (finance.IsOverCap)
            {
                AddIssue(report, SeverityError, "SalaryCap", "Team is over salary cap", team.Id, GetTeamName(team), suggestedRepair: IsUserTeam(state, team) ? "Open Contracts" : "CPU roster management may fix cap", canAutoRepair: !IsUserTeam(state, team));
            }

            if (finance.IsBelowFloor)
            {
                AddIssue(report, SeverityWarning, "SalaryCap", "Team is below salary floor", team.Id, GetTeamName(team), suggestedRepair: "Sign players or add salary");
            }
        }
    }

    public static void ValidateDraft(GameState state, GameStateValidationReportData report)
    {
        if (state.Draft == null)
        {
            AddIssue(report, SeverityWarning, "Draft", "DraftData is missing", suggestedRepair: "Migration can create empty DraftData", canAutoRepair: true);
            return;
        }

        if (state.Draft.Prospects == null)
        {
            AddIssue(report, SeverityWarning, "Draft", "Draft prospects list is null", suggestedRepair: "Repair can create empty list", canAutoRepair: true);
        }
        else if (state.Draft.IsInitialized && state.Draft.Prospects.Count != DraftConfig.DraftClassSize)
        {
            AddIssue(report, SeverityInfo, "Draft", "Draft class size is " + state.Draft.Prospects.Count + " expected " + DraftConfig.DraftClassSize);
        }

        if (state.Draft.ClassProfile == null)
        {
            AddIssue(report, SeverityWarning, "Draft", "Draft class profile is missing", suggestedRepair: "Migration can create fallback class profile", canAutoRepair: true);
        }

        if (state.DraftPickOwnership == null)
        {
            AddIssue(report, SeverityWarning, "Draft", "Draft pick ownership list is null", suggestedRepair: "Repair can create empty list", canAutoRepair: true);
        }
    }

    public static void ValidateHistory(GameState state, GameStateValidationReportData report)
    {
        if (state.LeagueHistory == null)
        {
            AddIssue(report, SeverityWarning, "History", "LeagueHistory is missing", suggestedRepair: "Repair can create empty list", canAutoRepair: true);
            return;
        }

        HashSet<int> seasons = new HashSet<int>();
        foreach (LeagueSeasonHistoryData history in state.LeagueHistory)
        {
            if (history == null)
            {
                continue;
            }

            if (!seasons.Add(history.SeasonStartYear))
            {
                AddIssue(report, SeverityWarning, "History", "Duplicate LeagueHistory season " + history.SeasonStartYear, suggestedRepair: "Repair can remove duplicate history entry", canAutoRepair: true);
            }
        }

        if (state.LeagueRecords == null)
        {
            AddIssue(report, SeverityWarning, "History", "LeagueRecords are missing", suggestedRepair: "Repair can create records container", canAutoRepair: true);
        }
    }

    public static void ValidateNews(GameState state, GameStateValidationReportData report)
    {
        if (state.NewsFeed == null || state.NewsFeed.Items == null)
        {
            AddIssue(report, SeverityWarning, "News", "NewsFeed is missing", suggestedRepair: "Repair can create news feed", canAutoRepair: true);
            return;
        }

        HashSet<string> keys = new HashSet<string>();
        foreach (NewsItemData item in state.NewsFeed.Items)
        {
            if (item == null)
            {
                AddIssue(report, SeverityWarning, "News", "Null news item", suggestedRepair: "Repair can remove null news item", canAutoRepair: true);
                continue;
            }

            if (string.IsNullOrEmpty(item.Title))
            {
                AddIssue(report, SeverityWarning, "News", "News item title is missing", suggestedRepair: "No automatic repair");
            }

            string key = item.Category + "|" + item.RelatedId;
            if (!string.IsNullOrEmpty(item.RelatedId) && !keys.Add(key))
            {
                AddIssue(report, SeverityWarning, "News", "Duplicate news related id: " + key, suggestedRepair: "Repair can remove duplicate news", canAutoRepair: true);
            }
        }

        if (state.NewsFeed.Items.Count > NewsConfig.MaxNewsItemsToKeep)
        {
            AddIssue(report, SeverityInfo, "News", "NewsFeed exceeds max size", suggestedRepair: "Repair can trim old news", canAutoRepair: true);
        }
    }

    public static void ValidateOwnerAndGmCareer(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (team.OwnerProfile == null)
            {
                AddIssue(report, SeverityWarning, "Owner", "OwnerProfile missing", team.Id, GetTeamName(team), suggestedRepair: "Repair can create owner profile", canAutoRepair: true);
            }
        }

        if (state.GmCareer == null)
        {
            AddIssue(report, SeverityWarning, "GmCareer", "GmCareer is missing", suggestedRepair: "Repair can create GM career data", canAutoRepair: true);
            return;
        }

        if (!string.IsNullOrEmpty(state.GmCareer.CurrentTeamId) && FindTeam(state, state.GmCareer.CurrentTeamId) == null)
        {
            AddIssue(report, SeverityWarning, "GmCareer", "GM current team id not found", teamId: state.GmCareer.CurrentTeamId, suggestedRepair: "Repair can sync to SelectedTeamId", canAutoRepair: true);
        }

        if (state.ActiveGmJobOffers != null)
        {
            foreach (GmJobOfferData offer in state.ActiveGmJobOffers)
            {
                if (offer != null && FindTeam(state, offer.TeamId) == null)
                {
                    AddIssue(report, SeverityWarning, "GmCareer", "Active job offer points to missing team", teamId: offer.TeamId, teamName: offer.TeamName, suggestedRepair: "Decline or regenerate offers");
                }
            }
        }

        if (state.GmCareer.IsUnemployed && (state.ActiveGmJobOffers == null || state.ActiveGmJobOffers.Count == 0))
        {
            AddIssue(report, SeverityWarning, "GmCareer", "GM is unemployed with no active job offers", suggestedRepair: "Open GM Career and generate offers", canAutoRepair: false);
        }
    }

    public static void ValidateRetirementData(GameState state, GameStateValidationReportData report)
    {
        if (state == null)
        {
            return;
        }

        if (state.RetiredPlayers == null || state.RetiredPlayers.Players == null)
        {
            AddIssue(report, SeverityWarning, "Retirement", "RetiredPlayers data is missing", suggestedRepair: "Repair can create retired players history", canAutoRepair: true);
        }

        if (state.HallOfFame == null || state.HallOfFame.Inductees == null)
        {
            AddIssue(report, SeverityWarning, "Retirement", "HallOfFame data is missing", suggestedRepair: "Repair can create Hall of Fame data", canAutoRepair: true);
        }

        if (state.LeagueRetiredNumbers == null)
        {
            AddIssue(report, SeverityWarning, "Retirement", "League retired numbers list is missing", suggestedRepair: "Repair can create retired numbers list", canAutoRepair: true);
        }

        ValidateRetiredPlayersNotActive(state, report);
        ValidateJerseyNumbers(state, report);
        ValidateRetirementDuplicates(state, report);
    }

    public static void ValidateTutorialData(GameState state, GameStateValidationReportData report)
    {
        if (state == null)
        {
            return;
        }

        if (state.Tutorial == null)
        {
            AddIssue(report, SeverityWarning, "Tutorial", "Tutorial data is missing", suggestedRepair: "Repair can create tutorial data", canAutoRepair: true);
            return;
        }

        if (state.Tutorial.CompletedStepIds == null)
        {
            AddIssue(report, SeverityWarning, "Tutorial", "Tutorial completed steps list is missing", suggestedRepair: "Repair can recreate completed step list", canAutoRepair: true);
        }

        if (state.Tutorial.DismissedHintIds == null)
        {
            AddIssue(report, SeverityWarning, "Tutorial", "Tutorial dismissed hints list is missing", suggestedRepair: "Repair can recreate dismissed hint list", canAutoRepair: true);
        }

        if (state.Tutorial.TutorialVersion < TutorialConfig.CurrentTutorialVersion)
        {
            AddIssue(report, SeverityInfo, "Tutorial", "Tutorial version is older than current version", suggestedRepair: "Migration can update tutorial version", canAutoRepair: true);
        }
    }

    public static void ValidateAlphaBalanceReports(GameState state, GameStateValidationReportData report)
    {
        if (state == null)
        {
            return;
        }

        if (state.AlphaBalanceReportHistory == null)
        {
            AddIssue(report, SeverityWarning, "AlphaBalance", "AlphaBalanceReportHistory is missing", suggestedRepair: "Repair can create empty alpha report history", canAutoRepair: true);
            return;
        }

        if (state.AlphaBalanceReportHistory.Count > AlphaBalanceConfig.MaxReportsToKeep)
        {
            AddIssue(report, SeverityWarning, "AlphaBalance", "AlphaBalanceReportHistory exceeds max size", suggestedRepair: "Repair can trim alpha report history", canAutoRepair: true);
        }

        HashSet<string> reportIds = new HashSet<string>();
        foreach (AlphaBalanceReportData alphaReport in state.AlphaBalanceReportHistory)
        {
            if (alphaReport == null)
            {
                AddIssue(report, SeverityWarning, "AlphaBalance", "Null alpha balance report entry", suggestedRepair: "Repair can remove null alpha reports", canAutoRepair: true);
                continue;
            }

            if (!string.IsNullOrEmpty(alphaReport.ReportId) && !reportIds.Add(alphaReport.ReportId))
            {
                AddIssue(report, SeverityWarning, "AlphaBalance", "Duplicate AlphaBalanceReportHistory report id: " + alphaReport.ReportId, suggestedRepair: "Repair can remove duplicate alpha reports", canAutoRepair: true);
            }
        }
    }

    public static void ValidateAndroidData(GameState state, GameStateValidationReportData report)
    {
        if (state == null)
        {
            return;
        }

        if (state.AndroidPerformance == null)
        {
            AddIssue(report, SeverityWarning, "Android", "AndroidPerformance is missing", suggestedRepair: "Repair can create Android performance data", canAutoRepair: true);
        }

        if (state.NewsFeed != null
            && state.NewsFeed.Items != null
            && state.NewsFeed.Items.Count > NewsConfig.MaxNewsItemsToKeep)
        {
            AddIssue(report, SeverityWarning, "Android", "NewsFeed exceeds Android history limit", suggestedRepair: "Repair can trim news feed", canAutoRepair: true);
        }

        if (state.AlphaBalanceReportHistory != null
            && state.AlphaBalanceReportHistory.Count > AlphaBalanceConfig.MaxReportsToKeep)
        {
            AddIssue(report, SeverityWarning, "Android", "Alpha balance history exceeds Android limit", suggestedRepair: "Repair can trim alpha history", canAutoRepair: true);
        }

        if (state.SaveVersion < SaveMigrationConfig.CurrentSaveVersion)
        {
            AddIssue(report, SeverityWarning, "Android", "SaveVersion is below current Android alpha version", suggestedRepair: "Run migration", canAutoRepair: true);
        }
    }

    public static void ValidateLongCareerGuards(GameState state, GameStateValidationReportData report)
    {
        if (state == null)
        {
            return;
        }

        int currentSeason = state.CurrentSeasonStartYear;
        if (state.LastRetirementProcessedSeasonStartYear > currentSeason)
        {
            AddIssue(report, SeverityWarning, "Retirement", "LastRetirementProcessedSeasonStartYear is in the future", suggestedRepair: "Repair can clamp retirement guard", canAutoRepair: true);
        }

        if (state.GmCareer != null && state.GmCareer.LastJobSecurityEvaluationSeasonStartYear > currentSeason)
        {
            AddIssue(report, SeverityWarning, "GmCareer", "LastJobSecurityEvaluationSeasonStartYear is in the future", suggestedRepair: "Repair can clamp GM job security guard", canAutoRepair: true);
        }

        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && player.LastCareerStatsUpdatedSeasonStartYear > currentSeason)
                {
                    AddIssue(report, SeverityWarning, "History", "LastCareerStatsUpdatedSeasonStartYear is in the future", team.Id, GetTeamName(team), player.Id, GetPlayerName(player), "Repair can clamp career stats guard", true);
                }
            }
        }
    }

    public static void ValidateTeamBalanceFields(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null)
            {
                continue;
            }

            if (team.Chemistry != null
                && (team.Chemistry.TeamChemistryScore < ChemistryConfig.MinChemistry
                    || team.Chemistry.TeamChemistryScore > ChemistryConfig.MaxChemistry))
            {
                AddIssue(report, SeverityWarning, "Chemistry", "Team chemistry score out of range", team.Id, GetTeamName(team), suggestedRepair: "Repair can clamp team chemistry", canAutoRepair: true);
            }

            if (team.Staff != null && HasOutOfRangeStaffImpact(team.Staff))
            {
                AddIssue(report, SeverityWarning, "Staff", "Staff effects out of range", team.Id, GetTeamName(team), suggestedRepair: "Repair can clamp staff effects", canAutoRepair: true);
            }
        }
    }

    public static void AddIssue(
        GameStateValidationReportData report,
        string severity,
        string category,
        string message,
        string teamId = "",
        string teamName = "",
        string playerId = "",
        string playerName = "",
        string suggestedRepair = "",
        bool canAutoRepair = false)
    {
        if (report == null)
        {
            return;
        }

        report.EnsureIssues();
        report.Issues.Add(new ValidationIssueData
        {
            Severity = string.IsNullOrEmpty(severity) ? SeverityInfo : severity,
            Category = string.IsNullOrEmpty(category) ? "Save" : category,
            Message = string.IsNullOrEmpty(message) ? "Unknown validation issue" : message,
            TeamId = string.IsNullOrEmpty(teamId) ? "" : teamId,
            TeamName = string.IsNullOrEmpty(teamName) ? "" : teamName,
            PlayerId = string.IsNullOrEmpty(playerId) ? "" : playerId,
            PlayerName = string.IsNullOrEmpty(playerName) ? "" : playerName,
            SuggestedRepair = string.IsNullOrEmpty(suggestedRepair) ? "" : suggestedRepair,
            CanAutoRepair = canAutoRepair,
            WasRepaired = false,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        });
    }

    private static void ValidatePlayer(PlayerData player, GameStateValidationReportData report, TeamData team, bool isFreeAgent)
    {
        if (player == null)
        {
            AddIssue(report, SeverityWarning, "Player", "Null player entry", teamId: team == null ? "" : team.Id, teamName: team == null ? "" : GetTeamName(team));
            return;
        }

        if (string.IsNullOrEmpty(player.Id))
        {
            AddIssue(report, SeverityError, "Player", "Player Id is missing", teamId: team == null ? "" : team.Id, teamName: team == null ? "" : GetTeamName(team), playerName: GetPlayerName(player), suggestedRepair: "Repair can generate player id", canAutoRepair: true);
        }

        if (!IsValidPosition(player.Position))
        {
            AddIssue(report, SeverityWarning, "Player", "Invalid player position: " + player.Position, teamId: team == null ? "" : team.Id, teamName: team == null ? "" : GetTeamName(team), playerId: player.Id, playerName: GetPlayerName(player), suggestedRepair: "Repair can set fallback position", canAutoRepair: true);
        }

        if (player.Age < 16 || player.Age > 45)
        {
            AddIssue(report, SeverityWarning, "Player", "Player age out of expected range: " + player.Age, playerId: player.Id, playerName: GetPlayerName(player), suggestedRepair: "Repair can clamp age", canAutoRepair: true);
        }

        if (player.Overall < 1 || player.Overall > 99 || player.Potential < 1 || player.Potential > 99)
        {
            AddIssue(report, SeverityWarning, "Player", "Player rating out of range", playerId: player.Id, playerName: GetPlayerName(player), suggestedRepair: "Repair can clamp ratings", canAutoRepair: true);
        }

        if (player.Salary < 0 || player.ContractYearsRemaining < 0)
        {
            AddIssue(report, SeverityWarning, "Contracts", "Player contract values are invalid", playerId: player.Id, playerName: GetPlayerName(player), suggestedRepair: "Repair can clamp contract values", canAutoRepair: true);
        }

        if (player.Condition < 0 || player.Condition > 100 || player.Fatigue < 0 || player.Fatigue > 100)
        {
            AddIssue(report, SeverityWarning, "Player", "Condition/Fatigue out of range", playerId: player.Id, playerName: GetPlayerName(player), suggestedRepair: "Repair can clamp fatigue values", canAutoRepair: true);
        }

        if (player.InjuryDaysRemaining <= 0 && player.IsInjured)
        {
            AddIssue(report, SeverityWarning, "Player", "Player marked injured with no remaining injury days", playerId: player.Id, playerName: GetPlayerName(player), suggestedRepair: "Repair can clear injury flag", canAutoRepair: true);
        }

        if (isFreeAgent && player.RosterStatus != RosterStatusConfig.FreeAgent)
        {
            AddIssue(report, SeverityWarning, "FreeAgency", "Free agent roster status mismatch", playerId: player.Id, playerName: GetPlayerName(player), suggestedRepair: "Repair can set FreeAgent status", canAutoRepair: true);
        }
    }

    private static void ValidateRetiredPlayersNotActive(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player == null || !player.IsRetired)
                {
                    continue;
                }

                AddIssue(report, SeverityError, "Retirement", "Retired player still exists in team roster", team.Id, GetTeamName(team), player.Id, GetPlayerName(player), "Repair can move retired player to history", true);
                if (team.Lineup != null && LineupService.IsPlayerInLineup(team.Lineup, player.Id))
                {
                    AddIssue(report, SeverityError, "Retirement", "Retired player is still in lineup", team.Id, GetTeamName(team), player.Id, GetPlayerName(player), "Repair can rebuild lineup", true);
                }
            }
        }

        if (state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null)
        {
            return;
        }

        foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
        {
            if (player != null && player.IsRetired)
            {
                AddIssue(report, SeverityError, "Retirement", "Retired player still exists in FreeAgentPool", playerId: player.Id, playerName: GetPlayerName(player), suggestedRepair: "Repair can remove retired free agent", canAutoRepair: true);
            }
        }
    }

    private static void ValidateJerseyNumbers(GameState state, GameStateValidationReportData report)
    {
        foreach (TeamData team in SafeTeams(state))
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            HashSet<int> usedNumbers = new HashSet<int>();
            HashSet<int> retiredNumbers = JerseyNumberService.GetRetiredNumbers(team);
            foreach (PlayerData player in team.Players)
            {
                if (player == null || player.IsRetired)
                {
                    continue;
                }

                if (player.JerseyNumber <= 0)
                {
                    AddIssue(report, SeverityWarning, "Retirement", "Active player has no jersey number", team.Id, GetTeamName(team), player.Id, GetPlayerName(player), "Repair can assign jersey number", true);
                    continue;
                }

                if (retiredNumbers.Contains(player.JerseyNumber))
                {
                    AddIssue(report, SeverityWarning, "Retirement", "Active player uses retired number #" + player.JerseyNumber, team.Id, GetTeamName(team), player.Id, GetPlayerName(player), "Repair can assign another jersey number", true);
                }

                if (!usedNumbers.Add(player.JerseyNumber))
                {
                    AddIssue(report, SeverityWarning, "Retirement", "Duplicate jersey number #" + player.JerseyNumber + " on team", team.Id, GetTeamName(team), player.Id, GetPlayerName(player), "Repair can reassign duplicate jersey numbers", true);
                }
            }
        }
    }

    private static void ValidateRetirementDuplicates(GameState state, GameStateValidationReportData report)
    {
        if (state == null)
        {
            return;
        }

        HashSet<string> retiredPlayers = new HashSet<string>();
        if (state.RetiredPlayers != null && state.RetiredPlayers.Players != null)
        {
            foreach (RetiredPlayerData player in state.RetiredPlayers.Players)
            {
                if (player == null)
                {
                    AddIssue(report, SeverityWarning, "Retirement", "Null retired player entry", suggestedRepair: "Repair can remove null retired history", canAutoRepair: true);
                    continue;
                }

                if (!string.IsNullOrEmpty(player.PlayerId) && !retiredPlayers.Add(player.PlayerId))
                {
                    AddIssue(report, SeverityWarning, "Retirement", "Duplicate retired player entry: " + player.PlayerName, playerId: player.PlayerId, playerName: player.PlayerName, suggestedRepair: "Repair can remove duplicate retired player", canAutoRepair: true);
                }
            }
        }

        HashSet<string> hofPlayers = new HashSet<string>();
        if (state.HallOfFame != null && state.HallOfFame.Inductees != null)
        {
            foreach (HallOfFameInducteeData inductee in state.HallOfFame.Inductees)
            {
                if (inductee == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(inductee.PlayerId) && !hofPlayers.Add(inductee.PlayerId))
                {
                    AddIssue(report, SeverityWarning, "Retirement", "Duplicate Hall of Fame inductee: " + inductee.PlayerName, playerId: inductee.PlayerId, playerName: inductee.PlayerName, suggestedRepair: "Repair can remove duplicate Hall of Fame entry", canAutoRepair: true);
                }
            }
        }

        HashSet<string> leagueNumbers = new HashSet<string>();
        if (state.LeagueRetiredNumbers != null)
        {
            foreach (RetiredNumberData retiredNumber in state.LeagueRetiredNumbers)
            {
                string key = retiredNumber == null ? "" : retiredNumber.TeamId + "|" + retiredNumber.JerseyNumber;
                if (retiredNumber == null || string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (!leagueNumbers.Add(key))
                {
                    AddIssue(report, SeverityWarning, "Retirement", "Duplicate league retired number: " + key, retiredNumber.TeamId, retiredNumber.TeamName, retiredNumber.PlayerId, retiredNumber.PlayerName, "Repair can remove duplicate retired number", true);
                }
            }
        }
    }

    private static void RecalculateCounts(GameStateValidationReportData report)
    {
        if (report == null)
        {
            return;
        }

        report.EnsureIssues();
        report.IssuesCount = report.Issues.Count;
        report.WarningsCount = 0;
        report.ErrorsCount = 0;
        report.CriticalCount = 0;
        report.AutoRepairableCount = 0;
        report.RepairedCount = 0;

        foreach (ValidationIssueData issue in report.Issues)
        {
            if (issue == null)
            {
                continue;
            }

            if (issue.Severity == SeverityWarning)
            {
                report.WarningsCount++;
            }
            else if (issue.Severity == SeverityError)
            {
                report.ErrorsCount++;
            }
            else if (issue.Severity == SeverityCritical)
            {
                report.CriticalCount++;
            }

            if (issue.CanAutoRepair)
            {
                report.AutoRepairableCount++;
            }

            if (issue.WasRepaired)
            {
                report.RepairedCount++;
            }
        }
    }

    private static void PopulateInventoryCounts(GameState state, GameStateValidationReportData report)
    {
        if (state == null || report == null)
        {
            return;
        }

        report.TeamsCount = state.Teams == null ? 0 : state.Teams.Count;
        report.PlayersCount = 0;
        foreach (TeamData team in SafeTeams(state))
        {
            report.PlayersCount += team == null || team.Players == null ? 0 : team.Players.Count;
            if (team != null && team.DraftRights != null)
            {
                report.ProspectsCount += team.DraftRights.Count;
            }
        }

        report.FreeAgentsCount = state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null
            ? 0
            : state.FreeAgentPool.FreeAgents.Count;
        if (state.Draft != null && state.Draft.Prospects != null)
        {
            report.ProspectsCount += state.Draft.Prospects.Count;
        }
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
        return state != null && team != null && !string.IsNullOrEmpty(state.SelectedTeamId) && team.Id == state.SelectedTeamId;
    }

    private static int CountRoster(TeamData team, string rosterStatus)
    {
        int count = 0;
        if (team == null || team.Players == null)
        {
            return count;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.RosterStatus == rosterStatus)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountPositionGroup(TeamData team, string rosterStatus, string group)
    {
        int count = 0;
        if (team == null || team.Players == null)
        {
            return count;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player == null || player.RosterStatus != rosterStatus)
            {
                continue;
            }

            if ((group == "Forward" && IsForward(player))
                || (group == "Defense" && player.Position == "D")
                || (group == "Goalie" && player.Position == "G"))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsForward(PlayerData player)
    {
        return player != null && (player.Position == "C" || player.Position == "LW" || player.Position == "RW");
    }

    private static bool IsValidPosition(string position)
    {
        return position == "C" || position == "LW" || position == "RW" || position == "D" || position == "G";
    }

    private static bool HasOutOfRangeStaffImpact(TeamStaffData staff)
    {
        if (staff == null)
        {
            return false;
        }

        return IsOutOfRangeStaffImpact(staff.StaffOffenseImpact)
            || IsOutOfRangeStaffImpact(staff.StaffDefenseImpact)
            || IsOutOfRangeStaffImpact(staff.StaffPowerPlayImpact)
            || IsOutOfRangeStaffImpact(staff.StaffPenaltyKillImpact)
            || IsOutOfRangeStaffImpact(staff.StaffDevelopmentImpact)
            || IsOutOfRangeStaffImpact(staff.StaffGoalieDevelopmentImpact)
            || IsOutOfRangeStaffImpact(staff.StaffMoraleImpact)
            || IsOutOfRangeStaffImpact(staff.StaffChemistryImpact)
            || IsOutOfRangeStaffImpact(staff.StaffDisciplineImpact)
            || IsOutOfRangeStaffImpact(staff.StaffTacticalFitImpact);
    }

    private static bool IsOutOfRangeStaffImpact(int value)
    {
        return value < StaffConfig.MinStaffRatingModifier || value > StaffConfig.MaxStaffRatingModifier;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static void Increment(Dictionary<string, int> counts, string key)
    {
        if (counts == null || string.IsNullOrEmpty(key))
        {
            return;
        }

        if (!counts.ContainsKey(key))
        {
            counts[key] = 0;
        }

        counts[key]++;
    }

    private static void ValidateCount(GameStateValidationReportData report, Dictionary<string, int> counts, string key, int expected, string category)
    {
        int actual = counts != null && counts.TryGetValue(key, out int count) ? count : 0;
        if (actual != expected)
        {
            AddIssue(report, SeverityWarning, "League", category + " count mismatch for " + key + ": " + actual + " / " + expected, suggestedRepair: "Repair can restore team divisions from seed", canAutoRepair: true);
        }
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : player.FirstName + " " + player.LastName;
    }
}
