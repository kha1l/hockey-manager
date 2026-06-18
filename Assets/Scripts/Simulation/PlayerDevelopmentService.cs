using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerDevelopmentService
{
    private static HashSet<string> _processedDevelopmentKeys;

    private class DevelopmentAdjustmentResult
    {
        public int Delta;
        public string Event = "Normal";
    }

    public static void EnsureDevelopmentHistory(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.PlayerDevelopmentHistory == null)
        {
            state.PlayerDevelopmentHistory = new PlayerDevelopmentHistoryData();
        }

        state.PlayerDevelopmentHistory.EnsureChanges();
    }

    public static void ApplyYearlyDevelopment(GameState state)
    {
        EnsureDevelopmentHistory(state);
        MoraleService.EnsureMorale(state);
        CoachingStaffService.EnsureStaffForTeams(state == null ? null : state.Teams);
        if (state == null || state.CurrentSeasonStartYear <= 0)
        {
            return;
        }

        if (state.PlayerDevelopmentHistory.LastProcessedSeasonStartYear == state.CurrentSeasonStartYear)
        {
            Debug.Log("Development already processed for this season");
            return;
        }

        int changesBefore = state.PlayerDevelopmentHistory.Changes.Count;
        _processedDevelopmentKeys = new HashSet<string>();

        ApplyDevelopmentToRosterPlayers(state);
        ApplyDevelopmentToFreeAgents(state);
        ApplyDevelopmentToDraftRights(state);

        _processedDevelopmentKeys = null;
        state.PlayerDevelopmentHistory.LastProcessedSeasonStartYear = state.CurrentSeasonStartYear;

        int createdChanges = state.PlayerDevelopmentHistory.Changes.Count - changesBefore;
        Debug.Log("Player development processed: " + createdChanges + " changes");
    }

    public static void ApplyDevelopmentToRosterPlayers(GameState state)
    {
        EnsureDevelopmentHistory(state);
        if (state == null || state.Teams == null)
        {
            return;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsurePlayers();
            TeamRosterService.EnsureRosterStatusesForTeam(team);
            LeadershipService.EnsureLeadershipForTeam(team);
            CoachingStaffService.EnsureStaffForTeam(team);
            string teamName = GetTeamName(team);
            int leadershipSupport = LeadershipService.GetYoungPlayerDevelopmentSupport(team);
            foreach (PlayerData player in team.Players)
            {
                PlayerDevelopmentChangeData change = ApplyDevelopmentToPlayer(
                    state,
                    player,
                    "RosterPlayer",
                    team.Id,
                    teamName,
                    leadershipSupport,
                    team);

                AddChange(state, change);
            }
        }
    }

    public static void ApplyDevelopmentToFreeAgents(GameState state)
    {
        EnsureDevelopmentHistory(state);
        if (state == null || state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null)
        {
            return;
        }

        foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
        {
            if (player != null)
            {
                player.Age++;
            }

            PlayerDevelopmentChangeData change = ApplyDevelopmentToPlayer(
                state,
                player,
                "FreeAgent",
                "free-agents",
                "Free Agents",
                0,
                null);

            AddChange(state, change);
        }
    }

    public static void ApplyDevelopmentToDraftRights(GameState state)
    {
        EnsureDevelopmentHistory(state);
        if (state == null || state.Teams == null)
        {
            return;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsureDraftRights();
            CoachingStaffService.EnsureStaffForTeam(team);
            string teamName = GetTeamName(team);
            foreach (ProspectData prospect in team.DraftRights)
            {
                if (prospect != null)
                {
                    prospect.Age++;
                }

                PlayerDevelopmentChangeData change = ApplyDevelopmentToProspect(
                    state,
                    prospect,
                    team.Id,
                    teamName,
                    team);

                AddChange(state, change);
            }
        }
    }

    private static PlayerDevelopmentChangeData ApplyDevelopmentToPlayer(
        GameState state,
        PlayerData player,
        string entityType,
        string teamId,
        string teamName,
        int leadershipSupport,
        TeamData team)
    {
        if (state == null || player == null || IsAlreadyProcessed(entityType, player.Id))
        {
            return null;
        }

        MarkProcessed(entityType, player.Id);
        EnsureDevelopmentProfile(player);

        int oldOverall = player.Overall;
        int oldPotential = player.Potential;
        int baseDelta = CalculatePlayerDelta(player, state.CurrentSeasonStartYear);
        DevelopmentAdjustmentResult adjustment = CalculateRiskAdjustedDevelopment(
            player.Id,
            player.Age,
            player.Overall,
            player.HiddenCeiling,
            player.HiddenFloor,
            player.DevelopmentType,
            player.BoomChance,
            player.BustChance,
            baseDelta,
            state.CurrentSeasonStartYear,
            PlayerDevelopmentConfig.MaxYearlyGrowth);
        int delta = adjustment.Delta;
        MoraleService.InitializePlayerMorale(player);
        int moraleAtTime = player.Morale;
        int moraleDevelopmentModifier = CalculateMoraleDevelopmentModifier(player, state.CurrentSeasonStartYear);
        if (moraleDevelopmentModifier != 0)
        {
            delta = ClampDelta(delta + moraleDevelopmentModifier, PlayerDevelopmentConfig.MaxYearlyRegression, PlayerDevelopmentConfig.MaxYearlyGrowth + 2);
            if (adjustment.Event == "Normal")
            {
                adjustment.Event = moraleDevelopmentModifier > 0 ? "MoraleBoost" : "MoraleDrag";
            }
        }

        int leadershipDevelopmentModifier = CalculateLeadershipDevelopmentModifier(player, leadershipSupport, state.CurrentSeasonStartYear);
        if (leadershipDevelopmentModifier != 0)
        {
            delta = ClampDelta(delta + leadershipDevelopmentModifier, PlayerDevelopmentConfig.MaxYearlyRegression, PlayerDevelopmentConfig.MaxYearlyGrowth + 2);
            if (adjustment.Event == "Normal")
            {
                adjustment.Event = "LeadershipBoost";
            }
        }

        int staffDevelopmentModifier = CalculateStaffDevelopmentModifier(team, player, state.CurrentSeasonStartYear);
        string staffDevelopmentSummary = BuildStaffDevelopmentSummary(team, player, staffDevelopmentModifier);
        // TODO: Later seasons can use MVP/Best Rookie awards as a small morale or development confidence boost.
        if (staffDevelopmentModifier != 0)
        {
            delta = ClampDelta(delta + staffDevelopmentModifier, PlayerDevelopmentConfig.MaxYearlyRegression, PlayerDevelopmentConfig.MaxYearlyGrowth + 2);
            if (adjustment.Event == "Normal")
            {
                adjustment.Event = staffDevelopmentModifier > 0 ? "StaffDevelopmentBoost" : "StaffDevelopmentDrag";
            }
        }

        int potentialDelta = CalculatePotentialDelta(
            player.Age,
            oldOverall,
            oldPotential,
            state.CurrentSeasonStartYear,
            player.Id);
        potentialDelta = CalculateRiskAdjustedPotentialDelta(
            oldPotential,
            player.HiddenCeiling,
            player.HiddenFloor,
            player.Age,
            potentialDelta,
            adjustment.Event);
        int newOverall = PlayerDevelopmentConfig.ClampOverall(oldOverall + delta);
        int newPotential = PlayerDevelopmentConfig.ClampPotential(oldPotential + potentialDelta);

        if (newOverall > newPotential)
        {
            newPotential = newOverall;
        }

        string developmentType = GetDevelopmentType(newOverall - oldOverall);

        player.LastSeasonOverall = oldOverall;
        player.LastSeasonPotential = oldPotential;
        player.Overall = newOverall;
        player.Potential = newPotential;
        player.LastDevelopmentDelta = newOverall - oldOverall;
        player.LastDevelopmentType = adjustment.Event == "Normal" ? developmentType : adjustment.Event;

        if (player.LastDevelopmentDelta == 0 && newPotential - oldPotential == 0)
        {
            return null;
        }

        return CreateChange(
            state,
            entityType,
            player.Id,
            player.FirstName + " " + player.LastName,
            teamId,
            teamName,
            player.Position,
            player.Age,
            oldOverall,
            newOverall,
            oldPotential,
            newPotential,
            player.DevelopmentType,
            adjustment.Event,
            player.HiddenCeiling,
            player.HiddenFloor,
            player.DevelopmentRisk,
            moraleAtTime,
            moraleDevelopmentModifier,
            leadershipSupport,
            leadershipDevelopmentModifier,
            staffDevelopmentModifier,
            staffDevelopmentSummary,
            BuildDevelopmentReason(GetReason(entityType, player.Age, player.LastDevelopmentDelta), adjustment.Event));
    }

    private static PlayerDevelopmentChangeData ApplyDevelopmentToProspect(
        GameState state,
        ProspectData prospect,
        string teamId,
        string teamName,
        TeamData team)
    {
        if (state == null || prospect == null || IsAlreadyProcessed("DraftRightsProspect", prospect.Id))
        {
            return null;
        }

        MarkProcessed("DraftRightsProspect", prospect.Id);
        int draftRank = prospect.DraftRank > 0
            ? prospect.DraftRank
            : (prospect.DraftPickOverall > 0 ? prospect.DraftPickOverall : prospect.ProjectedPick);
        ProspectRiskService.EnsureRiskProfile(prospect, draftRank);

        int oldOverall = prospect.Overall;
        int oldPotential = prospect.Potential;
        int baseDelta = CalculateProspectDelta(prospect, state.CurrentSeasonStartYear);
        DevelopmentAdjustmentResult adjustment = CalculateRiskAdjustedDevelopment(
            prospect.Id,
            prospect.Age,
            prospect.Overall,
            prospect.HiddenCeiling,
            prospect.HiddenFloor,
            prospect.DevelopmentType,
            prospect.BoomChance,
            prospect.BustChance,
            baseDelta,
            state.CurrentSeasonStartYear,
            PlayerDevelopmentConfig.MaxProspectYearlyGrowth);
        int delta = adjustment.Delta;
        PlayerData prospectAsPlayer = CreateProspectPlayerProxy(prospect);
        int staffDevelopmentModifier = CalculateStaffDevelopmentModifier(team, prospectAsPlayer, state.CurrentSeasonStartYear);
        string staffDevelopmentSummary = BuildStaffDevelopmentSummary(team, prospectAsPlayer, staffDevelopmentModifier);
        if (staffDevelopmentModifier != 0)
        {
            delta = ClampDelta(delta + staffDevelopmentModifier, PlayerDevelopmentConfig.MaxYearlyRegression, PlayerDevelopmentConfig.MaxProspectYearlyGrowth + 2);
            if (adjustment.Event == "Normal")
            {
                adjustment.Event = staffDevelopmentModifier > 0 ? "StaffDevelopmentBoost" : "StaffDevelopmentDrag";
            }
        }
        int potentialDelta = CalculatePotentialDelta(
            prospect.Age,
            oldOverall,
            oldPotential,
            state.CurrentSeasonStartYear,
            prospect.Id);
        potentialDelta = CalculateRiskAdjustedPotentialDelta(
            oldPotential,
            prospect.HiddenCeiling,
            prospect.HiddenFloor,
            prospect.Age,
            potentialDelta,
            adjustment.Event);
        int newOverall = PlayerDevelopmentConfig.ClampOverall(oldOverall + delta);
        int newPotential = PlayerDevelopmentConfig.ClampPotential(oldPotential + potentialDelta);

        if (newOverall > newPotential)
        {
            newPotential = newOverall;
        }

        string developmentType = GetDevelopmentType(newOverall - oldOverall);

        prospect.LastSeasonOverall = oldOverall;
        prospect.LastSeasonPotential = oldPotential;
        prospect.Overall = newOverall;
        prospect.Potential = newPotential;
        prospect.LastDevelopmentDelta = newOverall - oldOverall;
        prospect.LastDevelopmentType = adjustment.Event == "Normal" ? developmentType : adjustment.Event;

        if (prospect.LastDevelopmentDelta == 0 && newPotential - oldPotential == 0)
        {
            return null;
        }

        return CreateChange(
            state,
            "DraftRightsProspect",
            prospect.Id,
            prospect.FirstName + " " + prospect.LastName,
            teamId,
            teamName,
            prospect.Position,
            prospect.Age,
            oldOverall,
            newOverall,
            oldPotential,
            newPotential,
            prospect.DevelopmentType,
            adjustment.Event,
            prospect.HiddenCeiling,
            prospect.HiddenFloor,
            prospect.DevelopmentRisk,
            0,
            0,
            0,
            0,
            staffDevelopmentModifier,
            staffDevelopmentSummary,
            BuildDevelopmentReason(GetReason("DraftRightsProspect", prospect.Age, prospect.LastDevelopmentDelta), adjustment.Event));
    }

    public static void EnsureDevelopmentProfile(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        if (player.HasDevelopmentProfile)
        {
            if (!ProspectRiskConfig.IsValidDevelopmentType(player.DevelopmentType))
            {
                player.DevelopmentType = ProspectRiskConfig.DevelopmentTypeSafe;
            }

            player.HiddenCeiling = ProspectRiskConfig.ClampCeiling(Math.Max(player.HiddenCeiling, Math.Max(player.Potential, player.Overall)));
            player.HiddenFloor = ProspectRiskConfig.ClampFloor(player.HiddenFloor <= 0 ? Math.Max(40, player.Overall - 5) : player.HiddenFloor);
            if (player.HiddenFloor > player.HiddenCeiling)
            {
                player.HiddenFloor = Math.Min(player.HiddenCeiling, ProspectRiskConfig.MaxFloor);
            }

            player.DevelopmentRisk = ProspectRiskConfig.ClampRisk(player.DevelopmentRisk);
            player.BoomChance = ProspectRiskConfig.ClampChance(player.BoomChance);
            player.BustChance = ProspectRiskConfig.ClampChance(player.BustChance);
            player.DevelopmentTypeHint = ProspectRiskConfig.GetDevelopmentTypeHint(player.DevelopmentType);
            return;
        }

        player.HiddenCeiling = ProspectRiskConfig.ClampCeiling(Math.Max(player.Potential, player.Overall));
        player.HiddenFloor = ProspectRiskConfig.ClampFloor(Math.Max(40, player.Overall - 5));
        player.DevelopmentRisk = 20;
        player.BoomChance = 3;
        player.BustChance = 3;
        player.DevelopmentType = ProspectRiskConfig.DevelopmentTypeSafe;
        player.DevelopmentTypeHint = ProspectRiskConfig.GetDevelopmentTypeHint(player.DevelopmentType);
        player.HasDevelopmentProfile = true;
    }

    public static int CalculateRiskAdjustedDevelopmentDelta(PlayerData player, int baseDelta, int seasonStartYear)
    {
        if (player == null)
        {
            return baseDelta;
        }

        EnsureDevelopmentProfile(player);
        return CalculateRiskAdjustedDevelopment(
            player.Id,
            player.Age,
            player.Overall,
            player.HiddenCeiling,
            player.HiddenFloor,
            player.DevelopmentType,
            player.BoomChance,
            player.BustChance,
            baseDelta,
            seasonStartYear,
            PlayerDevelopmentConfig.MaxYearlyGrowth).Delta;
    }

    private static DevelopmentAdjustmentResult CalculateRiskAdjustedDevelopment(
        string playerId,
        int age,
        int overall,
        int hiddenCeiling,
        int hiddenFloor,
        string developmentProfileType,
        int boomChance,
        int bustChance,
        int baseDelta,
        int seasonStartYear,
        int maxGrowth)
    {
        DevelopmentAdjustmentResult result = new DevelopmentAdjustmentResult
        {
            Delta = baseDelta,
            Event = "Normal"
        };

        string type = ProspectRiskConfig.IsValidDevelopmentType(developmentProfileType)
            ? developmentProfileType
            : ProspectRiskConfig.DevelopmentTypeSafe;
        string seed = playerId + ":" + seasonStartYear + ":" + type;
        bool boom = StableRange(seed + ":boom-roll", 0, 99) < ProspectRiskConfig.ClampChance(boomChance);
        bool bust = StableRange(seed + ":bust-roll", 0, 99) < ProspectRiskConfig.ClampChance(bustChance);

        if (boom)
        {
            result.Delta += StableRange(seed + ":boom-delta", 2, 5);
            result.Event = "Boom";
        }
        else if (bust)
        {
            result.Delta -= StableRange(seed + ":bust-delta", 2, 5);
            result.Event = "Bust";
        }

        if (type == ProspectRiskConfig.DevelopmentTypeSafe)
        {
            result.Delta = ClampDelta(result.Delta, -2, 3);
        }
        else if (type == ProspectRiskConfig.DevelopmentTypeHighFloor)
        {
            if (overall < hiddenFloor && age <= 24)
            {
                result.Delta += 1;
                if (result.Event == "Normal")
                {
                    result.Event = "HighFloorCatchUp";
                }
            }

            if (hiddenCeiling - overall <= 2 && result.Delta > 1)
            {
                result.Delta = 1;
            }
        }
        else if (type == ProspectRiskConfig.DevelopmentTypeRawTalent)
        {
            int variance = StableRange(seed + ":raw-variance", -2, 3);
            result.Delta += variance;
            if (variance != 0 && result.Event == "Normal")
            {
                result.Event = "RawTalentVariance";
            }
        }
        else if (type == ProspectRiskConfig.DevelopmentTypeBoomBust)
        {
            int variance = StableRange(seed + ":boom-bust-variance", -3, 4);
            result.Delta += variance;
            if (variance != 0 && result.Event == "Normal")
            {
                result.Event = variance > 0 ? "Boom" : "Bust";
            }
        }
        else if (type == ProspectRiskConfig.DevelopmentTypeLateBloomer)
        {
            if (age <= 21 && result.Delta > 0)
            {
                result.Delta -= 1;
                if (result.Event == "Normal")
                {
                    result.Event = "LateBloomer";
                }
            }
            else if (age >= 22 && age <= 25)
            {
                result.Delta += 1;
                if (result.Event == "Normal")
                {
                    result.Event = "LateBloomer";
                }
            }
        }

        if (overall >= hiddenCeiling && result.Delta > 0)
        {
            result.Delta = 0;
            result.Event = "CeilingLimited";
        }
        else if (overall + result.Delta > hiddenCeiling)
        {
            result.Delta = hiddenCeiling - overall;
            result.Event = "CeilingLimited";
        }

        if (overall < hiddenFloor && age <= 24 && result.Delta < 1)
        {
            result.Delta = 1;
            result.Event = "HighFloorCatchUp";
        }

        result.Delta = ClampDelta(result.Delta, PlayerDevelopmentConfig.MaxYearlyRegression, maxGrowth + 2);
        return result;
    }

    private static int CalculateRiskAdjustedPotentialDelta(
        int oldPotential,
        int hiddenCeiling,
        int hiddenFloor,
        int age,
        int basePotentialDelta,
        string developmentEvent)
    {
        int delta = basePotentialDelta;
        if (developmentEvent == "Boom" && oldPotential < hiddenCeiling)
        {
            delta += 1;
        }
        else if (developmentEvent == "Bust" && oldPotential > hiddenFloor)
        {
            delta -= 1;
        }

        if (oldPotential + delta > hiddenCeiling)
        {
            delta = hiddenCeiling - oldPotential;
        }

        if (oldPotential + delta < hiddenFloor && age <= 24)
        {
            delta = hiddenFloor - oldPotential;
        }

        return ClampDelta(delta, -1, 1);
    }

    private static string BuildDevelopmentReason(string baseReason, string developmentEvent)
    {
        if (string.IsNullOrEmpty(developmentEvent) || developmentEvent == "Normal")
        {
            return baseReason;
        }

        if (developmentEvent == "Boom")
        {
            return baseReason + " | Boom development";
        }

        if (developmentEvent == "Bust")
        {
            return baseReason + " | Bust development";
        }

        if (developmentEvent == "LateBloomer")
        {
            return baseReason + " | Late bloomer growth";
        }

        if (developmentEvent == "HighFloorCatchUp")
        {
            return baseReason + " | High floor progression";
        }

        if (developmentEvent == "CeilingLimited")
        {
            return baseReason + " | Ceiling limited";
        }

        if (developmentEvent == "MoraleBoost")
        {
            return baseReason + " | High morale boost";
        }

        if (developmentEvent == "MoraleDrag")
        {
            return baseReason + " | Low morale drag";
        }

        if (developmentEvent == "LeadershipBoost")
        {
            return baseReason + " | Leadership support";
        }

        if (developmentEvent == "StaffDevelopmentBoost")
        {
            return baseReason + " | Coaching staff development support";
        }

        if (developmentEvent == "StaffDevelopmentDrag")
        {
            return baseReason + " | Coaching staff development drag";
        }

        return baseReason + " | " + developmentEvent;
    }

    private static int CalculateMoraleDevelopmentModifier(PlayerData player, int seasonStartYear)
    {
        if (player == null)
        {
            return 0;
        }

        MoraleService.InitializePlayerMorale(player);
        int roll = StableRange(player.Id + ":" + seasonStartYear + ":morale-development", 0, 99);
        int modifier = 0;
        if (player.Morale >= 80 && player.Age <= 25 && roll >= 88)
        {
            modifier += 1;
        }

        if (player.Morale < 35 && roll < 30)
        {
            modifier -= 1;
        }

        if (player.WantsTrade && roll < 45)
        {
            modifier -= 1;
        }

        return ClampDelta(modifier, -1, 1);
    }

    private static int CalculateLeadershipDevelopmentModifier(PlayerData player, int leadershipSupport, int seasonStartYear)
    {
        if (player == null || player.Age > 23 || leadershipSupport <= 0)
        {
            return 0;
        }

        int roll = StableRange(player.Id + ":" + seasonStartYear + ":leadership-development", 0, 99);
        int threshold = leadershipSupport >= 2 ? 82 : 90;
        return roll >= threshold ? 1 : 0;
    }

    private static int CalculateStaffDevelopmentModifier(TeamData team, PlayerData player, int seasonStartYear)
    {
        if (team == null || player == null)
        {
            return 0;
        }

        CoachingStaffService.EnsureStaffForTeam(team);
        int staffModifier = player.Position == "G"
            ? CoachingStaffService.GetGoalieDevelopmentModifier(team, player)
            : CoachingStaffService.GetDevelopmentModifier(team, player);

        if (staffModifier == 0)
        {
            return 0;
        }

        int roll = StableRange(player.Id + ":" + seasonStartYear + ":staff-development", 0, 99);
        if (staffModifier > 0)
        {
            int threshold = player.Age <= 23 ? 80 : 93;
            if (RosterStatusConfig.IsFarmRoster(player) && player.Age <= 23)
            {
                threshold -= 5;
            }

            if (player.DevelopmentType == ProspectRiskConfig.DevelopmentTypeRawTalent
                || player.DevelopmentType == ProspectRiskConfig.DevelopmentTypeLateBloomer)
            {
                threshold -= 4;
            }

            return roll >= Mathf.Clamp(threshold - staffModifier * 2, 70, 96) ? 1 : 0;
        }

        int dragThreshold = staffModifier <= -2 ? 28 : 16;
        return roll < dragThreshold ? -1 : 0;
    }

    private static string BuildStaffDevelopmentSummary(TeamData team, PlayerData player, int appliedModifier)
    {
        if (team == null || player == null)
        {
            return "";
        }

        int staffModifier = player.Position == "G"
            ? CoachingStaffService.GetGoalieDevelopmentModifier(team, player)
            : CoachingStaffService.GetDevelopmentModifier(team, player);
        if (staffModifier == 0 && appliedModifier == 0)
        {
            return "Coaching staff 0";
        }

        string coachLabel = player.Position == "G" ? "Goalie Coach" : "Development Coach";
        return coachLabel + " " + FormatSigned(staffModifier) + " | applied " + FormatSigned(appliedModifier);
    }

    private static PlayerData CreateProspectPlayerProxy(ProspectData prospect)
    {
        if (prospect == null)
        {
            return null;
        }

        return new PlayerData
        {
            Id = prospect.Id,
            FirstName = prospect.FirstName,
            LastName = prospect.LastName,
            TeamId = prospect.DraftedByTeamId,
            Position = prospect.Position,
            Age = prospect.Age,
            Overall = prospect.Overall,
            Potential = prospect.Potential,
            RosterStatus = RosterStatusConfig.Farm,
            DevelopmentType = prospect.DevelopmentType
        };
    }

    private static int CalculatePlayerDelta(PlayerData player, int seasonSeed)
    {
        if (player == null)
        {
            return 0;
        }

        int roll = StableRange(player.Id + ":" + seasonSeed + ":" + player.Age + ":player-delta", 0, 99);
        int growthRoom = player.Potential - player.Overall;
        int delta;

        if (player.Age <= PlayerDevelopmentConfig.ProspectFastGrowthMaxAge)
        {
            delta = WeightedDelta(roll, 0, 1, 2, 3, 4, PlayerDevelopmentConfig.MaxYearlyGrowth);
        }
        else if (player.Age <= PlayerDevelopmentConfig.YoungGrowthMaxAge)
        {
            delta = WeightedDelta(roll, -1, 0, 1, 2, 3, 4);
        }
        else if (player.Age <= PlayerDevelopmentConfig.SlowGrowthMaxAge)
        {
            delta = WeightedDelta(roll, -1, 0, 0, 1, 1, 2);
        }
        else if (player.Age <= PlayerDevelopmentConfig.PrimeAgeMax)
        {
            delta = WeightedDelta(roll, -1, -1, 0, 0, 1, 1);
        }
        else if (player.Age < PlayerDevelopmentConfig.HeavyRegressionStartAge)
        {
            delta = WeightedDelta(roll, -3, -2, -1, -1, 0, 1);
        }
        else
        {
            delta = WeightedDelta(roll, PlayerDevelopmentConfig.MaxYearlyRegression, -4, -3, -2, -1, 0);
        }

        if (growthRoom >= 10 && player.Age <= PlayerDevelopmentConfig.SlowGrowthMaxAge)
        {
            delta += 1;
        }

        if (growthRoom <= 2 && delta > 1)
        {
            delta = 1;
        }

        if (player.Overall >= player.Potential && delta > 1)
        {
            delta = 1;
        }

        if (player.Age >= PlayerDevelopmentConfig.VeteranRegressionStartAge && delta > 1)
        {
            delta = 1;
        }

        if (player.Age >= PlayerDevelopmentConfig.HeavyRegressionStartAge && delta > 0)
        {
            delta = 0;
        }

        delta = ApplyRosterStatusDevelopmentModifier(player, delta);
        return ClampDelta(delta, PlayerDevelopmentConfig.MaxYearlyRegression, PlayerDevelopmentConfig.MaxYearlyGrowth);
    }

    private static int ApplyRosterStatusDevelopmentModifier(PlayerData player, int delta)
    {
        if (player == null)
        {
            return delta;
        }

        if (RosterStatusConfig.IsFarmRoster(player) && player.Age <= PlayerDevelopmentConfig.YoungGrowthMaxAge && delta >= 0)
        {
            delta += 1;
        }
        else if (RosterStatusConfig.IsReserve(player) && delta > 0)
        {
            delta -= 1;
        }

        if ((RosterStatusConfig.IsFarmRoster(player) || RosterStatusConfig.IsReserve(player))
            && player.Age >= PlayerDevelopmentConfig.VeteranRegressionStartAge)
        {
            delta -= RosterStatusConfig.IsReserve(player) ? 2 : 1;
        }

        return delta;
    }

    private static int CalculateProspectDelta(ProspectData prospect, int seasonSeed)
    {
        if (prospect == null)
        {
            return 0;
        }

        int roll = StableRange(prospect.Id + ":" + seasonSeed + ":" + prospect.Age + ":prospect-delta", 0, 99);
        int growthRoom = prospect.Potential - prospect.Overall;
        int delta;

        if (prospect.Age <= PlayerDevelopmentConfig.ProspectFastGrowthMaxAge)
        {
            delta = WeightedDelta(roll, 0, 1, 2, 3, 4, PlayerDevelopmentConfig.MaxProspectYearlyGrowth);
        }
        else if (prospect.Age <= PlayerDevelopmentConfig.YoungGrowthMaxAge)
        {
            delta = WeightedDelta(roll, -1, 0, 1, 2, 3, 4);
        }
        else if (prospect.Age == 24)
        {
            delta = WeightedDelta(roll, -1, 0, 0, 1, 1, 2);
        }
        else
        {
            delta = WeightedDelta(roll, -3, -2, -1, 0, 0, 1);
        }

        if (growthRoom >= 15)
        {
            delta += 1;
        }

        if (prospect.Potential >= 90 && prospect.Age <= PlayerDevelopmentConfig.YoungGrowthMaxAge)
        {
            delta += 1;
        }

        if (growthRoom <= 2 && delta > 1)
        {
            delta = 1;
        }

        if (prospect.Overall >= prospect.Potential && delta > 1)
        {
            delta = 1;
        }

        return ClampDelta(delta, PlayerDevelopmentConfig.MaxYearlyRegression, PlayerDevelopmentConfig.MaxProspectYearlyGrowth);
    }

    private static int CalculatePotentialDelta(int age, int overall, int potential, int seasonSeed, string id)
    {
        int roll = StableRange(id + ":" + seasonSeed + ":" + age + ":potential-delta", 0, 99);
        int growthRoom = potential - overall;

        if (age >= PlayerDevelopmentConfig.HeavyRegressionStartAge && roll < 25)
        {
            return -1;
        }

        if (age >= PlayerDevelopmentConfig.VeteranRegressionStartAge && roll < 12)
        {
            return -1;
        }

        if (age <= PlayerDevelopmentConfig.YoungGrowthMaxAge && growthRoom >= 10 && roll >= 88)
        {
            return 1;
        }

        if (age <= PlayerDevelopmentConfig.SlowGrowthMaxAge && growthRoom >= 15 && roll >= 94)
        {
            return 1;
        }

        return 0;
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 23;
            string safeValue = value ?? "";
            for (int i = 0; i < safeValue.Length; i++)
            {
                hash = hash * 31 + safeValue[i];
            }

            return hash == int.MinValue ? 0 : Math.Abs(hash);
        }
    }

    private static int StableRange(string seed, int minInclusive, int maxInclusive)
    {
        if (maxInclusive <= minInclusive)
        {
            return minInclusive;
        }

        int range = maxInclusive - minInclusive + 1;
        return minInclusive + (StableHash(seed) % range);
    }

    private static int WeightedDelta(int roll, int veryLow, int low, int stableLow, int stableHigh, int high, int veryHigh)
    {
        if (roll < 10)
        {
            return veryLow;
        }

        if (roll < 25)
        {
            return low;
        }

        if (roll < 55)
        {
            return stableLow;
        }

        if (roll < 75)
        {
            return stableHigh;
        }

        if (roll < 92)
        {
            return high;
        }

        return veryHigh;
    }

    private static int ClampDelta(int value, int minValue, int maxValue)
    {
        if (value < minValue)
        {
            return minValue;
        }

        if (value > maxValue)
        {
            return maxValue;
        }

        return value;
    }

    private static PlayerDevelopmentChangeData CreateChange(
        GameState state,
        string entityType,
        string playerId,
        string playerName,
        string teamId,
        string teamName,
        string position,
        int age,
        int oldOverall,
        int newOverall,
        int oldPotential,
        int newPotential,
        string developmentType,
        string developmentEvent,
        int hiddenCeilingAtTime,
        int hiddenFloorAtTime,
        int developmentRiskAtTime,
        int moraleAtTime,
        int moraleDevelopmentModifier,
        int leadershipSupportAtTime,
        int leadershipDevelopmentModifier,
        int staffDevelopmentModifier,
        string staffDevelopmentSummary,
        string reason)
    {
        return new PlayerDevelopmentChangeData
        {
            ChangeId = Guid.NewGuid().ToString("N"),
            SeasonStartYear = state.CurrentSeasonStartYear,
            SeasonEndYear = state.CurrentSeasonEndYear,
            EntityType = entityType,
            PlayerId = playerId,
            PlayerName = playerName,
            TeamId = teamId,
            TeamName = teamName,
            Position = position,
            Age = age,
            OldOverall = oldOverall,
            NewOverall = newOverall,
            OverallDelta = newOverall - oldOverall,
            OldPotential = oldPotential,
            NewPotential = newPotential,
            PotentialDelta = newPotential - oldPotential,
            DevelopmentType = developmentType,
            DevelopmentEvent = string.IsNullOrEmpty(developmentEvent) ? "Normal" : developmentEvent,
            HiddenCeilingAtTime = hiddenCeilingAtTime,
            HiddenFloorAtTime = hiddenFloorAtTime,
            DevelopmentRiskAtTime = developmentRiskAtTime,
            MoraleAtTime = moraleAtTime,
            MoraleDevelopmentModifier = moraleDevelopmentModifier,
            LeadershipSupportAtTime = leadershipSupportAtTime,
            LeadershipDevelopmentModifier = leadershipDevelopmentModifier,
            StaffDevelopmentModifier = staffDevelopmentModifier,
            StaffDevelopmentSummary = staffDevelopmentSummary,
            Reason = reason,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    private static string FormatSigned(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }

    private static void AddChange(GameState state, PlayerDevelopmentChangeData change)
    {
        if (state == null || change == null)
        {
            return;
        }

        EnsureDevelopmentHistory(state);
        state.PlayerDevelopmentHistory.Changes.Add(change);
        TeamData team = FindTeamById(state, change.TeamId);
        PlayerData player = FindPlayerById(team, change.PlayerId);
        EventNewsService.CreateDevelopmentBreakoutNews(state, team, player, change.OverallDelta);
    }

    private static TeamData FindTeamById(GameState state, string teamId)
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

    private static PlayerData FindPlayerById(TeamData team, string playerId)
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

    private static bool IsAlreadyProcessed(string entityType, string id)
    {
        if (_processedDevelopmentKeys == null || string.IsNullOrEmpty(id))
        {
            return false;
        }

        return _processedDevelopmentKeys.Contains(GetProcessedKey(entityType, id));
    }

    private static void MarkProcessed(string entityType, string id)
    {
        if (_processedDevelopmentKeys != null && !string.IsNullOrEmpty(id))
        {
            _processedDevelopmentKeys.Add(GetProcessedKey(entityType, id));
        }
    }

    private static string GetProcessedKey(string entityType, string id)
    {
        string prefix = entityType == "DraftRightsProspect" ? "prospect:" : "player:";
        return prefix + id;
    }

    private static string GetDevelopmentType(int overallDelta)
    {
        if (overallDelta > 0)
        {
            return "Growth";
        }

        if (overallDelta < 0)
        {
            return "Regression";
        }

        return "Stable";
    }

    private static string GetReason(string entityType, int age, int overallDelta)
    {
        string baseReason;
        if (overallDelta > 0 && age <= PlayerDevelopmentConfig.SlowGrowthMaxAge)
        {
            baseReason = "Young player development";
        }
        else if (overallDelta < 0 && age >= PlayerDevelopmentConfig.VeteranRegressionStartAge)
        {
            baseReason = "Age-related regression";
        }
        else
        {
            baseReason = "No significant change";
        }

        if (entityType == "FreeAgent")
        {
            return baseReason + " | Free agent development";
        }

        if (entityType == "RosterPlayer")
        {
            return baseReason + " | Roster player development";
        }

        if (entityType == "DraftRightsProspect")
        {
            return baseReason + " | Draft rights prospect development";
        }

        return baseReason;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
