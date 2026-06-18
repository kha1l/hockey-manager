using System;
using System.Collections.Generic;

public static class CpuRosterManagementService
{
    private const string PositionForward = "Forward";
    private const string PositionDefense = "Defense";
    private const string PositionGoalie = "Goalie";
    private const string PositionAny = "Any";

    public static CpuRosterManagementReportData RunForAllCpuTeams(
        GameState state,
        string userTeamId,
        string reason)
    {
        if (state == null)
        {
            return new CpuRosterManagementReportData();
        }

        return RunForTeams(state, state.Teams, userTeamId, reason);
    }

    public static CpuRosterManagementReportData RunForTeams(
        GameState state,
        List<TeamData> teams,
        string userTeamId,
        string reason)
    {
        CpuRosterManagementReportData report = new CpuRosterManagementReportData();
        if (state == null)
        {
            return report;
        }

        if (teams != null)
        {
            foreach (TeamData team in teams)
            {
                RunForCpuTeam(state, team, userTeamId, report, reason);
            }
        }

        StoreReport(state, report);
        return report;
    }

    public static void RunForCpuTeam(
        GameState state,
        TeamData team,
        string userTeamId,
        CpuRosterManagementReportData report,
        string reason)
    {
        if (state == null || team == null || IsUserTeam(team, userTeamId))
        {
            return;
        }

        if (report != null)
        {
            report.EnsureActions();
            report.TeamsChecked++;
        }

        int actionsBefore = report == null || report.Actions == null ? 0 : report.Actions.Count;

        PrepareTeamForCpuRosterManagement(state, team);
        FixHealthyPlayerShortage(state, team, report, reason);
        FixRosterSizeAndPositions(state, team, report, reason);
        FixExcessNhlPlayers(state, team, report, reason);
        FixSalaryCapIfNeeded(state, team, report, reason);
        FixLineupAndSpecialTeams(state, team, report, reason);
        PlayerRoleService.EnsureRolesForTeam(team);
        IceTimeService.EnsureUsageForTeam(team);
        EnsureCpuCaptains(team, report, reason);

        if (report != null && report.Actions != null && TeamHadSuccessfulAction(report, actionsBefore))
        {
            report.TeamsChanged++;
        }
    }

    public static bool IsUserTeam(TeamData team, string userTeamId)
    {
        return team != null && !string.IsNullOrEmpty(userTeamId) && team.Id == userTeamId;
    }

    public static void EnsureCpuTeamReadyForGame(
        GameState state,
        TeamData team,
        string userTeamId,
        CpuRosterManagementReportData report)
    {
        if (state == null || team == null || IsUserTeam(team, userTeamId))
        {
            return;
        }

        PrepareTeamForCpuRosterManagement(state, team);
        FixHealthyPlayerShortage(state, team, report, "BeforeGame");
        FixRosterSizeAndPositions(state, team, report, "BeforeGame");
        FixExcessNhlPlayers(state, team, report, "BeforeGame");
        FixLineupAndSpecialTeams(state, team, report, "BeforeGame");
        EnsureCpuCaptains(team, report, "BeforeGame");
    }

    public static void FixRosterSizeAndPositions(
        GameState state,
        TeamData team,
        CpuRosterManagementReportData report,
        string reason)
    {
        PrepareTeamForCpuRosterManagement(state, team);
        if (team == null)
        {
            return;
        }

        int attempts = 0;
        while (TeamRosterService.GetNhlPlayers(team).Count < CpuRosterManagementConfig.MinNhlRosterSize && attempts < 8)
        {
            if (!CallUpBestAvailablePlayer(state, team, PositionAny, report, reason + ": roster below 20"))
            {
                AddAction(report, team, "NoAction", null, "", "", reason, false, "No available call-up to reach 20 Pro players");
                break;
            }

            attempts++;
        }

        EnsureTotalPositionMinimum(state, team, PositionForward, CpuRosterManagementConfig.RequiredForwards, report, reason);
        EnsureTotalPositionMinimum(state, team, PositionDefense, CpuRosterManagementConfig.RequiredDefensemen, report, reason);
        EnsureTotalPositionMinimum(state, team, PositionGoalie, CpuRosterManagementConfig.RequiredGoalies, report, reason);

        if (!TeamRosterService.ValidateNhlRoster(team, out string message))
        {
            AddAction(report, team, "NoAction", null, "", "", reason, false, message);
        }
    }

    public static void FixHealthyPlayerShortage(
        GameState state,
        TeamData team,
        CpuRosterManagementReportData report,
        string reason)
    {
        PrepareTeamForCpuRosterManagement(state, team);
        if (team == null)
        {
            return;
        }

        EnsureHealthyPositionMinimum(state, team, PositionForward, CpuRosterManagementConfig.MinHealthyForwardsForGame, report, reason);
        EnsureHealthyPositionMinimum(state, team, PositionDefense, CpuRosterManagementConfig.MinHealthyDefensemenForGame, report, reason);
        EnsureHealthyPositionMinimum(state, team, PositionGoalie, CpuRosterManagementConfig.MinHealthyGoaliesForGame, report, reason);
    }

    public static void FixExcessNhlPlayers(
        GameState state,
        TeamData team,
        CpuRosterManagementReportData report,
        string reason)
    {
        PrepareTeamForCpuRosterManagement(state, team);
        if (team == null)
        {
            return;
        }

        int attempts = 0;
        while (TeamRosterService.GetNhlPlayers(team).Count > CpuRosterManagementConfig.MaxNhlRosterSize && attempts < 10)
        {
            PlayerData candidate = FindBestNhlSendDownCandidate(team);
            if (!TrySendPlayerDown(state, team, candidate, report, reason + ": roster > 23"))
            {
                break;
            }

            attempts++;
        }

        attempts = 0;
        while (CountNhlByGroup(team, PositionGoalie) > CpuRosterManagementConfig.MaxNhlGoalies
            && TeamRosterService.GetNhlPlayers(team).Count > CpuRosterManagementConfig.MinNhlRosterSize
            && attempts < 4)
        {
            PlayerData goalie = FindBestNhlSendDownCandidateByPosition(team, PositionGoalie);
            if (!TrySendPlayerDown(state, team, goalie, report, reason + ": extra goalie"))
            {
                break;
            }

            attempts++;
        }
    }

    public static void FixLineupAndSpecialTeams(
        GameState state,
        TeamData team,
        CpuRosterManagementReportData report,
        string reason)
    {
        PrepareTeamForCpuRosterManagement(state, team);
        if (team == null)
        {
            return;
        }

        if (!TeamRosterService.ValidateNhlRoster(team, out string rosterMessage))
        {
            FixHealthyPlayerShortage(state, team, report, reason + ": invalid roster");
            FixRosterSizeAndPositions(state, team, report, reason + ": invalid roster");
        }

        bool lineupInvalid = team.Lineup == null
            || !LineupService.ValidateLineup(team, out string lineupMessage)
            || LineupService.HasInjuredActivePlayers(team, out lineupMessage)
            || LineupService.HasNonNhlActivePlayers(team, out lineupMessage);

        if (lineupInvalid)
        {
            team.Lineup = LineupService.BuildAutoLineup(team);
            bool success = LineupService.ValidateLineup(team, out lineupMessage);
            AddAction(report, team, "AutoBuildLineup", null, "", "", reason, success, success ? "Auto lineup rebuilt" : lineupMessage);
        }

        bool specialTeamsInvalid = team.SpecialTeams == null
            || !SpecialTeamsService.ValidateSpecialTeams(team, out string specialTeamsMessage);

        if (specialTeamsInvalid)
        {
            team.SpecialTeams = SpecialTeamsService.BuildAutoSpecialTeams(team);
            bool success = SpecialTeamsService.ValidateSpecialTeams(team, out specialTeamsMessage);
            AddAction(report, team, "AutoBuildSpecialTeams", null, "", "", reason, success, success ? "Auto special teams rebuilt" : specialTeamsMessage);
        }

        TacticsService.EnsureTactics(team);
        PlayerRoleService.EnsureRolesForTeam(team);
        IceTimeService.EnsureUsageForTeam(team);
        LeadershipService.EnsureLeadershipForTeam(team);
    }

    public static void FixSalaryCapIfNeeded(
        GameState state,
        TeamData team,
        CpuRosterManagementReportData report,
        string reason)
    {
        PrepareTeamForCpuRosterManagement(state, team);
        if (state == null || team == null)
        {
            return;
        }

        if (state.LeagueRules == null)
        {
            state.LeagueRules = LeagueRulesConfig.CreateDefaultRules();
        }

        int attempts = 0;
        int cap = state.LeagueRules.SalaryCapUpperLimit;
        while (SalaryCapService.CalculatePayroll(team) > cap && attempts < 5)
        {
            PlayerData candidate = FindBestCapSendDownCandidate(team);
            if (candidate == null)
            {
                AddAction(report, team, "CapFix", null, "", "", reason, false, "No cap fix send-down candidate");
                break;
            }

            string fromStatus = candidate.RosterStatus;
            bool success = TrySendPlayerDown(state, team, candidate, report, reason + ": cap fix");
            AddAction(
                report,
                team,
                "CapFix",
                candidate,
                fromStatus,
                candidate.IsOnWaivers ? WaiverConfig.WaiverStatusOnWaivers : candidate.RosterStatus,
                reason,
                success,
                success ? "Attempted cap fix" : "Cap fix failed");

            if (!success)
            {
                break;
            }

            attempts++;
        }
    }

    public static PlayerData FindBestFarmCallUp(
        TeamData team,
        string neededPositionGroup)
    {
        return FindBestCallUp(team, RosterStatusConfig.Farm, neededPositionGroup);
    }

    public static PlayerData FindBestReserveCallUp(
        TeamData team,
        string neededPositionGroup)
    {
        return FindBestCallUp(team, RosterStatusConfig.Reserve, neededPositionGroup);
    }

    public static PlayerData FindBestNhlSendDownCandidate(TeamData team)
    {
        List<PlayerData> candidates = GetSendDownCandidates(team, PositionAny);
        candidates.Sort(CompareSendDownCandidates);
        return candidates.Count == 0 ? null : candidates[0];
    }

    public static PlayerData FindBestNhlSendDownCandidateByPosition(
        TeamData team,
        string positionGroup)
    {
        List<PlayerData> candidates = GetSendDownCandidates(team, NormalizePositionGroup(positionGroup));
        candidates.Sort(CompareSendDownCandidates);
        return candidates.Count == 0 ? null : candidates[0];
    }

    public static bool TryCallUpPlayer(
        GameState state,
        TeamData team,
        PlayerData player,
        CpuRosterManagementReportData report,
        string reason)
    {
        if (team == null || player == null)
        {
            return false;
        }

        TeamRosterService.EnsureRosterStatusesForTeam(team);
        string fromStatus = player.RosterStatus;
        if (TeamRosterService.GetNhlPlayers(team).Count >= CpuRosterManagementConfig.MaxNhlRosterSize)
        {
            AddAction(report, team, "CallUp", player, fromStatus, RosterStatusConfig.NHL, reason, false, "Pro roster full");
            return false;
        }

        RosterMoveResultData result = null;
        if (RosterStatusConfig.IsFarmRoster(player))
        {
            result = TeamRosterService.CallUpPlayerToNhl(team, player.Id);
        }
        else if (RosterStatusConfig.IsReserve(player))
        {
            result = TeamRosterService.MoveReservePlayerToNhl(team, player.Id);
        }
        else
        {
            AddAction(report, team, "CallUp", player, fromStatus, RosterStatusConfig.NHL, reason, false, "Player is not in Farm or Reserve");
            return false;
        }

        bool success = result != null && result.Success;
        AddAction(
            report,
            team,
            "CallUp",
            player,
            fromStatus,
            RosterStatusConfig.NHL,
            reason,
            success,
            result == null ? "Call-up failed" : result.Message);
        return success;
    }

    public static bool TrySendPlayerDown(
        GameState state,
        TeamData team,
        PlayerData player,
        CpuRosterManagementReportData report,
        string reason)
    {
        if (team == null || player == null)
        {
            return false;
        }

        TeamRosterService.EnsureRosterStatusesForTeam(team);
        string fromStatus = player.RosterStatus;
        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            AddAction(report, team, "SendDown", player, fromStatus, RosterStatusConfig.Farm, reason, false, "Player is not in Pro roster");
            return false;
        }

        if (TeamRosterService.GetNhlPlayers(team).Count <= CpuRosterManagementConfig.MinNhlRosterSize)
        {
            AddAction(report, team, "SendDown", player, fromStatus, RosterStatusConfig.Farm, reason, false, "Pro roster would fall below 20");
            return false;
        }

        bool requiredWaivers = player.RequiresWaivers;
        RosterMoveResultData result = TeamRosterService.SendPlayerToFarm(state, team, player.Id);
        bool success = result != null && result.Success;
        string toStatus = player.IsOnWaivers ? WaiverConfig.WaiverStatusOnWaivers : player.RosterStatus;
        string message = result == null ? "Send-down failed" : result.Message;
        if (success && requiredWaivers && player.IsOnWaivers)
        {
            message = "Placed on waivers: " + message;
        }

        AddAction(report, team, "SendDown", player, fromStatus, toStatus, reason, success, message);
        return success;
    }

    public static void AddAction(
        CpuRosterManagementReportData report,
        TeamData team,
        string actionType,
        PlayerData player,
        string fromStatus,
        string toStatus,
        string reason,
        bool success,
        string message)
    {
        if (report == null)
        {
            return;
        }

        report.EnsureActions();
        CpuRosterActionData action = new CpuRosterActionData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = team == null ? "" : GetTeamDisplayName(team),
            ActionType = actionType,
            PlayerId = player == null ? "" : player.Id,
            PlayerName = player == null ? "" : player.FirstName + " " + player.LastName,
            FromStatus = string.IsNullOrEmpty(fromStatus) ? "" : fromStatus,
            ToStatus = string.IsNullOrEmpty(toStatus) ? "" : toStatus,
            Reason = string.IsNullOrEmpty(reason) ? "" : reason,
            Success = success,
            Message = string.IsNullOrEmpty(message) ? "" : message
        };

        report.Actions.Add(action);
        report.ActionsCount = report.Actions.Count;
    }

    public static void StoreReport(GameState state, CpuRosterManagementReportData report)
    {
        if (state == null || report == null)
        {
            return;
        }

        state.EnsureCpuRosterManagementHistory();
        report.EnsureActions();
        state.LastCpuRosterManagementReport = report;
        state.CpuRosterManagementHistory.Add(report);

        while (state.CpuRosterManagementHistory.Count > CpuRosterManagementConfig.MaxReportsToKeep)
        {
            state.CpuRosterManagementHistory.RemoveAt(0);
        }
    }

    private static void PrepareTeamForCpuRosterManagement(GameState state, TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        WaiverEligibilityService.EnsureWaiverEligibilityForTeam(team);
        PlayerFatigueService.EnsureFatigueForTeam(team);
        InjuryService.EnsureInjuryFieldsForTeam(team);
        TacticsService.EnsureTactics(team);
        LeadershipService.EnsureLeadershipForTeam(team);
        CoachingStaffService.EnsureStaffForTeam(team);
    }

    private static void EnsureCpuCaptains(TeamData team, CpuRosterManagementReportData report, string reason)
    {
        LeadershipService.EnsureLeadershipForTeam(team);
        PlayerData captain = FindCaptain(team);
        if (captain != null && LeadershipService.IsEligibleForCaptaincy(captain))
        {
            return;
        }

        CaptaincyActionResultData result = LeadershipService.AutoAssignCaptains(team);
        AddAction(
            report,
            team,
            "RosterFix",
            captain,
            captain == null ? "" : captain.CaptaincyRole,
            LeadershipConfig.RoleCaptain,
            reason,
            result != null && result.Success,
            result == null ? "Captaincy auto-assign failed" : result.Message);
    }

    private static PlayerData FindCaptain(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return null;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.IsCaptain)
            {
                return player;
            }
        }

        return null;
    }

    private static void EnsureHealthyPositionMinimum(
        GameState state,
        TeamData team,
        string positionGroup,
        int requiredCount,
        CpuRosterManagementReportData report,
        string reason)
    {
        int attempts = 0;
        while (CountHealthyNhlByGroup(team, positionGroup) < requiredCount && attempts < 8)
        {
            if (TeamRosterService.GetNhlPlayers(team).Count >= CpuRosterManagementConfig.MaxNhlRosterSize)
            {
                bool roomCreated = CreateRosterRoomForCallUp(state, team, positionGroup, report, reason + ": healthy shortage");
                if (!roomCreated || TeamRosterService.GetNhlPlayers(team).Count >= CpuRosterManagementConfig.MaxNhlRosterSize)
                {
                    AddAction(report, team, "NoAction", null, "", "", reason, false, "No Pro roster room for " + positionGroup + " call-up");
                    break;
                }
            }

            if (!CallUpBestAvailablePlayer(state, team, positionGroup, report, reason + ": healthy " + positionGroup + " shortage"))
            {
                AddAction(report, team, "NoAction", null, "", "", reason, false, "No available " + positionGroup + " call-up");
                break;
            }

            attempts++;
        }
    }

    private static void EnsureTotalPositionMinimum(
        GameState state,
        TeamData team,
        string positionGroup,
        int requiredCount,
        CpuRosterManagementReportData report,
        string reason)
    {
        int attempts = 0;
        while (CountNhlByGroup(team, positionGroup) < requiredCount && attempts < 8)
        {
            if (TeamRosterService.GetNhlPlayers(team).Count >= CpuRosterManagementConfig.MaxNhlRosterSize)
            {
                bool roomCreated = CreateRosterRoomForCallUp(state, team, positionGroup, report, reason + ": position shortage");
                if (!roomCreated || TeamRosterService.GetNhlPlayers(team).Count >= CpuRosterManagementConfig.MaxNhlRosterSize)
                {
                    AddAction(report, team, "NoAction", null, "", "", reason, false, "No Pro roster room for " + positionGroup + " call-up");
                    break;
                }
            }

            if (!CallUpBestAvailablePlayer(state, team, positionGroup, report, reason + ": " + positionGroup + " shortage"))
            {
                AddAction(report, team, "NoAction", null, "", "", reason, false, "No available " + positionGroup + " call-up");
                break;
            }

            attempts++;
        }
    }

    private static bool CallUpBestAvailablePlayer(
        GameState state,
        TeamData team,
        string positionGroup,
        CpuRosterManagementReportData report,
        string reason)
    {
        PlayerData player = FindBestFarmCallUp(team, positionGroup);
        if (player == null)
        {
            player = FindBestReserveCallUp(team, positionGroup);
        }

        return TryCallUpPlayer(state, team, player, report, reason);
    }

    private static bool CreateRosterRoomForCallUp(
        GameState state,
        TeamData team,
        string neededPositionGroup,
        CpuRosterManagementReportData report,
        string reason)
    {
        string expendableGroup = GetExpendableGroupForNeed(team, neededPositionGroup);
        PlayerData candidate = FindBestNhlSendDownCandidateByPosition(team, expendableGroup);
        if (candidate == null)
        {
            candidate = FindBestNhlSendDownCandidate(team);
        }

        return TrySendPlayerDown(state, team, candidate, report, reason);
    }

    private static PlayerData FindBestCallUp(TeamData team, string rosterStatus, string neededPositionGroup)
    {
        PrepareTeamForCpuRosterManagement(null, team);
        List<PlayerData> candidates = new List<PlayerData>();
        if (team == null || team.Players == null)
        {
            return null;
        }

        string group = NormalizePositionGroup(neededPositionGroup);
        foreach (PlayerData player in team.Players)
        {
            if (player == null || player.RosterStatus != rosterStatus || player.IsOnWaivers)
            {
                continue;
            }

            if (!PlayerMatchesGroup(player, group))
            {
                continue;
            }

            candidates.Add(player);
        }

        candidates.Sort(CompareCallUpCandidates);
        return candidates.Count == 0 ? null : candidates[0];
    }

    private static List<PlayerData> GetSendDownCandidates(TeamData team, string positionGroup)
    {
        PrepareTeamForCpuRosterManagement(null, team);
        List<PlayerData> candidates = new List<PlayerData>();
        if (team == null)
        {
            return candidates;
        }

        if (TeamRosterService.GetNhlPlayers(team).Count <= CpuRosterManagementConfig.MinNhlRosterSize)
        {
            return candidates;
        }

        string group = NormalizePositionGroup(positionGroup);
        foreach (PlayerData player in TeamRosterService.GetNhlPlayers(team))
        {
            if (player == null || player.IsOnWaivers)
            {
                continue;
            }

            if (!PlayerMatchesGroup(player, group))
            {
                continue;
            }

            if (WouldViolatePositionMinimum(team, player) || WouldViolateHealthyMinimum(team, player))
            {
                continue;
            }

            candidates.Add(player);
        }

        return candidates;
    }

    private static PlayerData FindBestCapSendDownCandidate(TeamData team)
    {
        List<PlayerData> candidates = GetSendDownCandidates(team, PositionAny);
        candidates.Sort(CompareCapSendDownCandidates);
        return candidates.Count == 0 ? null : candidates[0];
    }

    private static bool WouldViolatePositionMinimum(TeamData team, PlayerData player)
    {
        string group = GetPlayerGroup(player);
        if (group == PositionForward)
        {
            return CountNhlByGroup(team, PositionForward) <= CpuRosterManagementConfig.RequiredForwards;
        }

        if (group == PositionDefense)
        {
            return CountNhlByGroup(team, PositionDefense) <= CpuRosterManagementConfig.RequiredDefensemen;
        }

        if (group == PositionGoalie)
        {
            return CountNhlByGroup(team, PositionGoalie) <= CpuRosterManagementConfig.RequiredGoalies;
        }

        return false;
    }

    private static bool WouldViolateHealthyMinimum(TeamData team, PlayerData player)
    {
        if (!InjuryService.IsPlayerAvailable(player))
        {
            return false;
        }

        string group = GetPlayerGroup(player);
        if (group == PositionForward)
        {
            return CountHealthyNhlByGroup(team, PositionForward) <= CpuRosterManagementConfig.MinHealthyForwardsForGame;
        }

        if (group == PositionDefense)
        {
            return CountHealthyNhlByGroup(team, PositionDefense) <= CpuRosterManagementConfig.MinHealthyDefensemenForGame;
        }

        if (group == PositionGoalie)
        {
            return CountHealthyNhlByGroup(team, PositionGoalie) <= CpuRosterManagementConfig.MinHealthyGoaliesForGame;
        }

        return false;
    }

    private static int CountNhlByGroup(TeamData team, string positionGroup)
    {
        int count = 0;
        foreach (PlayerData player in TeamRosterService.GetNhlPlayers(team))
        {
            if (PlayerMatchesGroup(player, positionGroup))
            {
                count++;
            }
        }

        return count;
    }

    private static int CountHealthyNhlByGroup(TeamData team, string positionGroup)
    {
        int count = 0;
        foreach (PlayerData player in TeamRosterService.GetNhlPlayers(team))
        {
            if (player != null
                && !player.IsOnWaivers
                && PlayerMatchesGroup(player, positionGroup)
                && InjuryService.IsPlayerAvailable(player))
            {
                count++;
            }
        }

        return count;
    }

    private static string GetExpendableGroupForNeed(TeamData team, string neededPositionGroup)
    {
        string needed = NormalizePositionGroup(neededPositionGroup);
        string bestGroup = PositionAny;
        int bestSurplus = 0;

        TryUseGroupSurplus(team, needed, PositionGoalie, CpuRosterManagementConfig.RequiredGoalies, ref bestGroup, ref bestSurplus);
        TryUseGroupSurplus(team, needed, PositionDefense, CpuRosterManagementConfig.RequiredDefensemen, ref bestGroup, ref bestSurplus);
        TryUseGroupSurplus(team, needed, PositionForward, CpuRosterManagementConfig.RequiredForwards, ref bestGroup, ref bestSurplus);

        return bestGroup;
    }

    private static void TryUseGroupSurplus(
        TeamData team,
        string neededGroup,
        string group,
        int requiredCount,
        ref string bestGroup,
        ref int bestSurplus)
    {
        if (group == neededGroup)
        {
            return;
        }

        int surplus = CountNhlByGroup(team, group) - requiredCount;
        if (surplus > bestSurplus)
        {
            bestGroup = group;
            bestSurplus = surplus;
        }
    }

    private static bool TeamHadSuccessfulAction(CpuRosterManagementReportData report, int startIndex)
    {
        if (report == null || report.Actions == null)
        {
            return false;
        }

        for (int i = startIndex; i < report.Actions.Count; i++)
        {
            CpuRosterActionData action = report.Actions[i];
            if (action != null && action.Success && action.ActionType != "NoAction")
            {
                return true;
            }
        }

        return false;
    }

    private static bool PlayerMatchesGroup(PlayerData player, string positionGroup)
    {
        string group = NormalizePositionGroup(positionGroup);
        if (group == PositionAny)
        {
            return player != null;
        }

        if (group == PositionForward)
        {
            return CpuRosterManagementConfig.IsForward(player);
        }

        if (group == PositionDefense)
        {
            return CpuRosterManagementConfig.IsDefenseman(player);
        }

        if (group == PositionGoalie)
        {
            return CpuRosterManagementConfig.IsGoalie(player);
        }

        return false;
    }

    private static string GetPlayerGroup(PlayerData player)
    {
        if (CpuRosterManagementConfig.IsForward(player))
        {
            return PositionForward;
        }

        if (CpuRosterManagementConfig.IsDefenseman(player))
        {
            return PositionDefense;
        }

        if (CpuRosterManagementConfig.IsGoalie(player))
        {
            return PositionGoalie;
        }

        return PositionAny;
    }

    private static string NormalizePositionGroup(string positionGroup)
    {
        if (positionGroup == "F" || positionGroup == "Forward" || positionGroup == "C" || positionGroup == "LW" || positionGroup == "RW")
        {
            return PositionForward;
        }

        if (positionGroup == "D" || positionGroup == "Defense")
        {
            return PositionDefense;
        }

        if (positionGroup == "G" || positionGroup == "Goalie")
        {
            return PositionGoalie;
        }

        return PositionAny;
    }

    private static int CompareCallUpCandidates(PlayerData left, PlayerData right)
    {
        int injuryComparison = InjuryService.IsPlayerAvailable(right).CompareTo(InjuryService.IsPlayerAvailable(left));
        if (injuryComparison != 0)
        {
            return injuryComparison;
        }

        int overallComparison = right.Overall.CompareTo(left.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        int potentialComparison = right.Potential.CompareTo(left.Potential);
        if (potentialComparison != 0)
        {
            return potentialComparison;
        }

        int ageComparison = left.Age.CompareTo(right.Age);
        if (ageComparison != 0)
        {
            return ageComparison;
        }

        int conditionComparison = right.Condition.CompareTo(left.Condition);
        if (conditionComparison != 0)
        {
            return conditionComparison;
        }

        return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }

    private static int CompareSendDownCandidates(PlayerData left, PlayerData right)
    {
        int scratchComparison = IsScratch(right).CompareTo(IsScratch(left));
        if (scratchComparison != 0)
        {
            return scratchComparison;
        }

        int waiverComparison = (!right.RequiresWaivers).CompareTo(!left.RequiresWaivers);
        if (waiverComparison != 0)
        {
            return waiverComparison;
        }

        int injuryComparison = InjuryService.IsPlayerAvailable(right).CompareTo(InjuryService.IsPlayerAvailable(left));
        if (injuryComparison != 0)
        {
            return injuryComparison;
        }

        int overallComparison = left.Overall.CompareTo(right.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        int potentialComparison = left.Potential.CompareTo(right.Potential);
        if (potentialComparison != 0)
        {
            return potentialComparison;
        }

        int youngComparison = IsYoungDevelopmentPlayer(right).CompareTo(IsYoungDevelopmentPlayer(left));
        if (youngComparison != 0)
        {
            return youngComparison;
        }

        int salaryComparison = right.Salary.CompareTo(left.Salary);
        if (salaryComparison != 0)
        {
            return salaryComparison;
        }

        return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }

    private static int CompareCapSendDownCandidates(PlayerData left, PlayerData right)
    {
        int scratchComparison = IsScratch(right).CompareTo(IsScratch(left));
        if (scratchComparison != 0)
        {
            return scratchComparison;
        }

        int salaryComparison = right.Salary.CompareTo(left.Salary);
        if (salaryComparison != 0)
        {
            return salaryComparison;
        }

        int waiverComparison = (!right.RequiresWaivers).CompareTo(!left.RequiresWaivers);
        if (waiverComparison != 0)
        {
            return waiverComparison;
        }

        int overallComparison = left.Overall.CompareTo(right.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }

    private static bool IsScratch(PlayerData player)
    {
        return player != null && player.UsageCategory == "Scratch";
    }

    private static bool IsYoungDevelopmentPlayer(PlayerData player)
    {
        return player != null && player.Age <= CpuRosterManagementConfig.ProspectAgeMax;
    }

    private static string GetTeamDisplayName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
