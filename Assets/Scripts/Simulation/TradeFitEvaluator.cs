using System;
using System.Collections.Generic;

public static class TradeFitEvaluator
{
    // TODO: Future: trade AI should value prospects based on scouting uncertainty and team scouting accuracy.
    public static TradeFitEvaluationData EvaluateTradeForTeam(
        GameState state,
        TeamData evaluatingTeam,
        List<TradeAssetData> incomingAssets,
        List<TradeAssetData> outgoingAssets)
    {
        incomingAssets = incomingAssets ?? new List<TradeAssetData>();
        outgoingAssets = outgoingAssets ?? new List<TradeAssetData>();

        int incomingValue = CalculateAssetsValue(state, evaluatingTeam, incomingAssets);
        int outgoingValue = CalculateAssetsValue(state, evaluatingTeam, outgoingAssets);
        int needFit = CalculateNeedFitScore(state, evaluatingTeam, incomingAssets, outgoingAssets);
        int capFit = CalculateCapFitScore(state, evaluatingTeam, incomingAssets, outgoingAssets);
        int rosterFit = CalculateRosterFitScore(state, evaluatingTeam, incomingAssets, outgoingAssets);
        int directionFit = CalculateDirectionFitScore(state, evaluatingTeam, incomingAssets, outgoingAssets);
        int contractFit = CalculateContractFitScore(state, evaluatingTeam, incomingAssets, outgoingAssets);
        int finalScore = incomingValue - outgoingValue + needFit + capFit + rosterFit + directionFit + contractFit;

        TradeFitEvaluationData evaluation = new TradeFitEvaluationData
        {
            EvaluatingTeamId = evaluatingTeam == null ? "" : evaluatingTeam.Id,
            EvaluatingTeamName = TeamIdentityService.GetDisplayName(evaluatingTeam),
            IncomingValue = incomingValue,
            OutgoingValue = outgoingValue,
            ValueDelta = incomingValue - outgoingValue,
            NeedFitScore = needFit,
            CapFitScore = capFit,
            RosterFitScore = rosterFit,
            DirectionFitScore = directionFit,
            ContractFitScore = contractFit,
            FinalScore = finalScore,
            Accepted = finalScore >= TradeAiConfig.CpuAcceptThreshold
        };

        evaluation.Reason = BuildReason(evaluation);
        return evaluation;
    }

    public static int CalculateAssetsValue(
        GameState state,
        TeamData evaluatingTeam,
        List<TradeAssetData> assets)
    {
        if (assets == null)
        {
            return 0;
        }

        int value = 0;
        foreach (TradeAssetData asset in assets)
        {
            if (asset == null)
            {
                continue;
            }

            if (asset.AssetType == "Player")
            {
                PlayerData player = ResolvePlayerAsset(state, asset);
                value += player == null ? CalculateFallbackPlayerValue(asset) : TradeValueCalculator.CalculatePlayerTradeValue(player);
            }
            else if (IsDraftPickAsset(asset))
            {
                value += TradeValueCalculator.CalculateAssetValue(asset, state);
            }
        }

        return value;
    }

    public static int CalculateNeedFitScore(
        GameState state,
        TeamData evaluatingTeam,
        List<TradeAssetData> incomingAssets,
        List<TradeAssetData> outgoingAssets)
    {
        TeamNeedData needs = TeamNeedService.GetTeamNeeds(state, evaluatingTeam);
        int score = 0;
        foreach (TradeAssetData asset in SafeAssets(incomingAssets))
        {
            PlayerData player = ResolvePlayerAsset(state, asset);
            if (AssetMatchesNeed(asset, player, needs == null ? "" : needs.PrimaryNeed))
            {
                score += 35;
            }

            if (AssetMatchesNeed(asset, player, needs == null ? "" : needs.SecondaryNeed))
            {
                score += 18;
            }

            if (needs != null && needs.Direction == TradeAiConfig.DirectionRebuild && IsDraftPickAsset(asset))
            {
                score += 25;
            }

            if (needs != null
                && needs.Direction == TradeAiConfig.DirectionContender
                && player != null
                && TradeAiConfig.IsVeteran(player)
                && player.Overall >= 80)
            {
                score += 20;
            }
        }

        foreach (TradeAssetData asset in SafeAssets(outgoingAssets))
        {
            PlayerData player = ResolvePlayerAsset(state, asset);
            if (TradeBlockService.IsCorePlayer(evaluatingTeam, player))
            {
                score -= 50;
            }

            if (AssetMatchesNeed(asset, player, needs == null ? "" : needs.PrimaryNeed))
            {
                score -= 35;
            }
        }

        return score;
    }

    public static int CalculateCapFitScore(
        GameState state,
        TeamData evaluatingTeam,
        List<TradeAssetData> incomingAssets,
        List<TradeAssetData> outgoingAssets)
    {
        if (evaluatingTeam == null)
        {
            return 0;
        }

        int cap = state != null && state.LeagueRules != null && state.LeagueRules.SalaryCapUpperLimit > 0
            ? state.LeagueRules.SalaryCapUpperLimit
            : SalaryCapConfig.SalaryCapUpperLimit;
        int payrollAfter = EstimatePayrollAfterTrade(evaluatingTeam, incomingAssets, outgoingAssets);
        if (payrollAfter > cap)
        {
            return -100;
        }

        TeamNeedData needs = TeamNeedService.GetTeamNeeds(state, evaluatingTeam);
        int salaryOut = SumNhlSalary(outgoingAssets);
        int salaryIn = SumNhlSalary(incomingAssets);
        int delta = salaryOut - salaryIn;
        if (needs != null && needs.NeedCapSpace >= TradeAiConfig.NeedHigh && delta > 0)
        {
            return delta >= 5000000 ? 40 : 20;
        }

        if (SalaryCapService.CalculatePayroll(evaluatingTeam) > cap * 90 / 100 && delta < -3000000)
        {
            return -30;
        }

        return delta > 0 ? 8 : 0;
    }

    public static int CalculateRosterFitScore(
        GameState state,
        TeamData evaluatingTeam,
        List<TradeAssetData> incomingAssets,
        List<TradeAssetData> outgoingAssets)
    {
        if (evaluatingTeam == null)
        {
            return 0;
        }

        RosterEstimate estimate = BuildRosterEstimate(evaluatingTeam, incomingAssets, outgoingAssets);
        int score = 0;
        if (estimate.NhlCount > RosterStatusConfig.MaxNhlRosterSize)
        {
            score -= 20;
        }

        if (estimate.Forwards < 12 || estimate.Defensemen < 6 || estimate.Goalies < 2)
        {
            score -= 40;
        }

        foreach (TradeAssetData asset in SafeAssets(outgoingAssets))
        {
            PlayerData player = ResolvePlayerAsset(state, asset);
            if (TradeBlockService.IsExcessPositionPlayer(evaluatingTeam, player))
            {
                score += 15;
            }
        }

        TeamNeedData needs = TeamNeedService.GetTeamNeeds(state, evaluatingTeam);
        foreach (TradeAssetData asset in SafeAssets(incomingAssets))
        {
            PlayerData player = ResolvePlayerAsset(state, asset);
            if (AssetMatchesNeed(asset, player, needs == null ? "" : needs.PrimaryNeed))
            {
                score += 15;
            }
        }

        return score;
    }

    public static int CalculateDirectionFitScore(
        GameState state,
        TeamData evaluatingTeam,
        List<TradeAssetData> incomingAssets,
        List<TradeAssetData> outgoingAssets)
    {
        string direction = TeamDirectionService.DetermineTeamDirection(state, evaluatingTeam);
        bool needsChemistryStabilizer = HasLowTeamChemistry(evaluatingTeam);
        bool needsLeadership = HasLowTeamLeadership(evaluatingTeam);
        int score = 0;
        foreach (TradeAssetData asset in SafeAssets(incomingAssets))
        {
            PlayerData player = ResolvePlayerAsset(state, asset);
            MoraleService.InitializePlayerMorale(player);
            LeadershipService.EnsurePlayerLeadershipProfile(player);
            if (player != null && player.WantsTrade)
            {
                score -= 5;
            }

            if (needsChemistryStabilizer && IsChemistryStabilizer(player))
            {
                score += 6;
            }

            if (needsLeadership && player != null && player.Leadership >= 80)
            {
                score += 10;
            }

            if (direction == TradeAiConfig.DirectionRebuild)
            {
                if (IsDraftPickAsset(asset))
                {
                    score += 25;
                }

                if (player != null && TradeAiConfig.IsYoungPlayer(player) && player.Potential >= 78)
                {
                    score += 20;
                    score += CalculateDevelopmentProfileDirectionModifier(player, direction);
                }

                if (player != null && TradeAiConfig.IsVeteran(player) && player.Salary >= 5000000)
                {
                    score -= 20;
                }
            }
            else if (direction == TradeAiConfig.DirectionContender || direction == TradeAiConfig.DirectionPlayoffTeam)
            {
                if (player != null && player.Morale < 25 && player.Salary >= 5000000)
                {
                    score -= 10;
                }

                if (player != null && player.Overall >= 80 && RosterStatusConfig.IsNhlRoster(player))
                {
                    score += 25;
                }

                if (player != null && TradeAiConfig.IsYoungPlayer(player))
                {
                    score += CalculateDevelopmentProfileDirectionModifier(player, direction);
                }

                if (IsDraftPickAsset(asset))
                {
                    score -= 5;
                }
            }
        }

        foreach (TradeAssetData asset in SafeAssets(outgoingAssets))
        {
            PlayerData player = ResolvePlayerAsset(state, asset);
            MoraleService.InitializePlayerMorale(player);
            LeadershipService.EnsurePlayerLeadershipProfile(player);
            if (player != null && player.WantsTrade)
            {
                score += 10;
            }

            if (player != null && player.IsCaptain && !player.WantsTrade)
            {
                score -= 20;
            }
            else if (player != null && player.IsAlternateCaptain)
            {
                score -= 8;
            }

            if (player != null
                && direction == TradeAiConfig.DirectionRebuild
                && TradeAiConfig.IsVeteran(player)
                && player.Morale < 40)
            {
                score += 10;
            }

            if (TradeBlockService.IsCorePlayer(evaluatingTeam, player))
            {
                score -= direction == TradeAiConfig.DirectionRebuild ? 20 : 50;
            }
        }

        return score;
    }

    private static bool HasLowTeamChemistry(TeamData team)
    {
        if (team == null)
        {
            return false;
        }

        if (team.Chemistry == null)
        {
            ChemistryService.EnsureChemistryForTeam(team);
        }

        return team.Chemistry != null && team.Chemistry.TeamChemistryScore < 45;
    }

    private static bool HasLowTeamLeadership(TeamData team)
    {
        if (team == null)
        {
            return false;
        }

        if (team.LeadershipData == null)
        {
            LeadershipService.EnsureLeadershipForTeam(team);
        }

        return team.LeadershipData != null && team.LeadershipData.LeadershipScore < 60;
    }

    private static bool IsChemistryStabilizer(PlayerData player)
    {
        if (player == null)
        {
            return false;
        }

        PlayerRoleService.EnsureRole(player);
        return player.PlayerRole == PlayerRoleConfig.TwoWayForward
            || player.PlayerRole == PlayerRoleConfig.TwoWayDefenseman
            || player.PlayerRole == PlayerRoleConfig.Playmaker
            || player.PlayerRole == PlayerRoleConfig.DefensiveDefenseman;
    }

    private static int CalculateDevelopmentProfileDirectionModifier(PlayerData player, string direction)
    {
        if (player == null)
        {
            return 0;
        }

        PlayerDevelopmentService.EnsureDevelopmentProfile(player);
        if (direction == TradeAiConfig.DirectionRebuild)
        {
            if ((player.DevelopmentType == ProspectRiskConfig.DevelopmentTypeRawTalent
                    || player.DevelopmentType == ProspectRiskConfig.DevelopmentTypeBoomBust)
                && player.HiddenCeiling >= 90)
            {
                return 10;
            }

            if (player.DevelopmentType == ProspectRiskConfig.DevelopmentTypeHighFloor && player.Age <= 23)
            {
                return 8;
            }
        }

        if ((direction == TradeAiConfig.DirectionContender || direction == TradeAiConfig.DirectionPlayoffTeam)
            && player.DevelopmentRisk >= 71)
        {
            return -5;
        }

        return 0;
    }

    public static int CalculateContractFitScore(
        GameState state,
        TeamData evaluatingTeam,
        List<TradeAssetData> incomingAssets,
        List<TradeAssetData> outgoingAssets)
    {
        string direction = TeamDirectionService.DetermineTeamDirection(state, evaluatingTeam);
        TeamNeedData needs = TeamNeedService.GetTeamNeeds(state, evaluatingTeam);
        int score = 0;

        foreach (TradeAssetData asset in SafeAssets(incomingAssets))
        {
            PlayerData player = ResolvePlayerAsset(state, asset);
            if (player == null)
            {
                continue;
            }

            if (TradeAiConfig.IsPendingUfa(player))
            {
                score += direction == TradeAiConfig.DirectionContender ? 12 : direction == TradeAiConfig.DirectionRebuild ? -14 : -6;
                if (player.ExtensionInterest < ContractExtensionConfig.LowInterestThreshold || player.RefusesExtensionThisSeason)
                {
                    score += direction == TradeAiConfig.DirectionContender ? -2 : -10;
                }
            }

            if (player.Salary >= 8000000 && player.ContractYearsRemaining >= 4 && needs != null && needs.NeedCapSpace >= TradeAiConfig.NeedMedium)
            {
                score -= 25;
            }

            if (TradeAiConfig.IsYoungPlayer(player) && player.Salary <= 2000000 && player.Potential >= 78)
            {
                score += 15;
            }
        }

        return score;
    }

    private static PlayerData ResolvePlayerAsset(
        GameState state,
        TradeAssetData asset)
    {
        if (state == null || state.Teams == null || asset == null || asset.AssetType != "Player")
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsurePlayers();
            foreach (PlayerData player in team.Players)
            {
                if (player != null && player.Id == asset.PlayerId)
                {
                    return player;
                }
            }
        }

        return null;
    }

    private static bool IsDraftPickAsset(TradeAssetData asset)
    {
        return asset != null && asset.AssetType == "DraftPick";
    }

    private static int CalculateFallbackPlayerValue(TradeAssetData asset)
    {
        int value = asset.Overall * 2 + asset.Potential;
        if (asset.Age <= 23)
        {
            value += 25;
        }

        if (asset.Salary > 8000000 && asset.Overall < 82)
        {
            value -= 25;
        }

        return Math.Max(0, value);
    }

    private static bool AssetMatchesNeed(TradeAssetData asset, PlayerData player, string needName)
    {
        if (string.IsNullOrEmpty(needName) || needName == "None")
        {
            return false;
        }

        if (needName == "DraftPicks")
        {
            return IsDraftPickAsset(asset);
        }

        if (needName == "Top6Forward")
        {
            return player != null && TradeAiConfig.IsForward(player) && player.Overall >= 78;
        }

        if (needName == "Bottom6Forward")
        {
            return player != null && TradeAiConfig.IsForward(player) && player.Overall >= 68;
        }

        if (needName == "Defenseman")
        {
            return player != null && TradeAiConfig.IsDefenseman(player) && player.Overall >= 74;
        }

        if (needName == "Goalie")
        {
            return player != null && TradeAiConfig.IsGoalie(player);
        }

        if (needName == "Prospects" || needName == "YoungPlayers")
        {
            return player != null && TradeAiConfig.IsYoungPlayer(player) && player.Potential >= 78;
        }

        return false;
    }

    private static int EstimatePayrollAfterTrade(
        TeamData team,
        List<TradeAssetData> incomingAssets,
        List<TradeAssetData> outgoingAssets)
    {
        int payroll = SalaryCapService.CalculatePayroll(team);
        payroll -= SumNhlSalary(outgoingAssets);
        payroll += SumNhlSalary(incomingAssets);
        return payroll;
    }

    private static int SumNhlSalary(List<TradeAssetData> assets)
    {
        int salary = 0;
        foreach (TradeAssetData asset in SafeAssets(assets))
        {
            if (asset.AssetType == "Player" && asset.RosterStatus == RosterStatusConfig.NHL)
            {
                salary += asset.Salary;
            }
        }

        return salary;
    }

    private static RosterEstimate BuildRosterEstimate(
        TeamData team,
        List<TradeAssetData> incomingAssets,
        List<TradeAssetData> outgoingAssets)
    {
        RosterEstimate estimate = new RosterEstimate();
        foreach (PlayerData player in TeamRosterService.GetNhlPlayers(team))
        {
            AddPlayerToEstimate(estimate, player.Position, 1);
        }

        foreach (TradeAssetData asset in SafeAssets(outgoingAssets))
        {
            if (asset.AssetType == "Player" && asset.RosterStatus == RosterStatusConfig.NHL)
            {
                AddPlayerToEstimate(estimate, asset.Position, -1);
            }
        }

        foreach (TradeAssetData asset in SafeAssets(incomingAssets))
        {
            if (asset.AssetType == "Player" && asset.RosterStatus == RosterStatusConfig.NHL)
            {
                AddPlayerToEstimate(estimate, asset.Position, 1);
            }
        }

        return estimate;
    }

    private static void AddPlayerToEstimate(RosterEstimate estimate, string position, int delta)
    {
        estimate.NhlCount += delta;
        if (position == "C" || position == "LW" || position == "RW")
        {
            estimate.Forwards += delta;
        }
        else if (position == "D")
        {
            estimate.Defensemen += delta;
        }
        else if (position == "G")
        {
            estimate.Goalies += delta;
        }
    }

    private static List<TradeAssetData> SafeAssets(List<TradeAssetData> assets)
    {
        return assets ?? new List<TradeAssetData>();
    }

    private static string BuildReason(TradeFitEvaluationData evaluation)
    {
        if (evaluation == null)
        {
            return "";
        }

        string status = evaluation.Accepted ? "Accepted" : "Rejected";
        string reason = status + ": score " + evaluation.FinalScore
            + " | value " + evaluation.ValueDelta
            + " | need " + evaluation.NeedFitScore
            + " | cap " + evaluation.CapFitScore
            + " | roster " + evaluation.RosterFitScore
            + " | direction " + evaluation.DirectionFitScore;
        return reason;
    }

    private class RosterEstimate
    {
        public int NhlCount;
        public int Forwards;
        public int Defensemen;
        public int Goalies;
    }
}
