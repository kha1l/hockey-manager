using System;
using System.Collections.Generic;

public static class TradeBlockService
{
    public static List<TradeBlockPlayerData> BuildTradeBlock(
        GameState state,
        TeamData team,
        TeamNeedData needs)
    {
        List<TradeBlockPlayerData> tradeBlock = new List<TradeBlockPlayerData>();
        if (team == null)
        {
            return tradeBlock;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        PlayerRoleService.EnsureRolesForTeam(team);
        MoraleService.EnsureMoraleForTeam(state, team);
        LeadershipService.EnsureLeadershipForTeam(team);

        foreach (PlayerData player in team.Players)
        {
            if (player == null || player.IsRetired || player.IsOnWaivers)
            {
                continue;
            }

            int availability = CalculateAvailabilityScore(state, team, player, needs);
            if (availability < 35)
            {
                continue;
            }

            TradeAssetData asset = TradeService.CreatePlayerAsset(player, team);
            tradeBlock.Add(new TradeBlockPlayerData
            {
                TeamId = team.Id,
                TeamName = GetTeamName(team),
                PlayerId = player.Id,
                PlayerName = player.FirstName + " " + player.LastName,
                Position = player.Position,
                Age = player.Age,
                Overall = player.Overall,
                Potential = player.Potential,
                Salary = player.Salary,
                ContractYearsRemaining = player.ContractYearsRemaining,
                RosterStatus = player.RosterStatus,
                PlayerRole = player.PlayerRole,
                Morale = player.Morale,
                WantsTrade = player.WantsTrade,
                MoraleStatus = player.MoraleStatus,
                IsCaptain = player.IsCaptain,
                IsAlternateCaptain = player.IsAlternateCaptain,
                CaptaincyRole = player.CaptaincyRole,
                Leadership = player.Leadership,
                TradeValue = asset == null ? TradeValueCalculator.CalculatePlayerTradeValue(player) : asset.EstimatedTradeValue,
                AvailabilityScore = availability,
                Reason = DetermineTradeBlockReason(state, team, player, needs)
            });
        }

        tradeBlock.Sort(CompareTradeBlockPlayers);
        if (tradeBlock.Count > TradeAiConfig.MaxTradeBlockPlayers)
        {
            tradeBlock.RemoveRange(TradeAiConfig.MaxTradeBlockPlayers, tradeBlock.Count - TradeAiConfig.MaxTradeBlockPlayers);
        }

        return tradeBlock;
    }

    public static int CalculateAvailabilityScore(
        GameState state,
        TeamData team,
        PlayerData player,
        TeamNeedData needs)
    {
        if (player == null)
        {
            return 0;
        }

        int score = 0;
        MoraleService.InitializePlayerMorale(player);
        LeadershipService.EnsurePlayerLeadershipProfile(player);
        if (player.WantsTrade)
        {
            score += 35;
        }

        if (player.RefusesExtensionThisSeason)
        {
            score += 30;
        }

        if (TradeAiConfig.IsPendingUfa(player) && player.ExtensionInterest < ContractExtensionConfig.LowInterestThreshold)
        {
            score += 20;
        }

        if (TradeBlockService.IsCorePlayer(team, player)
            && player.ExtensionInterest >= ContractExtensionConfig.HighInterestThreshold)
        {
            score -= 10;
        }

        if (player.Morale < 35)
        {
            score += 15;
        }

        if (player.Morale >= 85 && IsCorePlayer(team, player))
        {
            score -= 10;
        }

        if (RosterStatusConfig.IsFarmRoster(player))
        {
            score += 25;
        }

        if (RosterStatusConfig.IsReserve(player))
        {
            score += 20;
        }

        if (IsScratch(team, player))
        {
            score += 20;
        }

        if (IsExcessPositionPlayer(team, player))
        {
            score += 20;
        }

        string direction = needs == null || string.IsNullOrEmpty(needs.Direction)
            ? TeamDirectionService.DetermineTeamDirection(state, team)
            : needs.Direction;

        if (direction == TradeAiConfig.DirectionRebuild && TradeAiConfig.IsVeteran(player))
        {
            score += 35;
        }

        if (direction == TradeAiConfig.DirectionRebuild && TradeAiConfig.IsPendingUfa(player))
        {
            score += 30;
        }

        if (direction == TradeAiConfig.DirectionContender && player.Overall < 72)
        {
            score += 20;
        }

        if (needs != null && needs.NeedCapSpace >= TradeAiConfig.NeedHigh && IsExpensiveForRole(player))
        {
            score += 25;
        }

        if (needs != null && needs.NeedRosterSpace >= TradeAiConfig.NeedHigh)
        {
            score += 20;
        }

        if (!InjuryService.IsPlayerAvailable(player))
        {
            score -= 15;
        }

        if (player.Potential >= 85 && player.Age <= 23)
        {
            score -= 40;
        }

        if (player.Overall >= 84)
        {
            score -= 45;
        }

        if (IsTopGoalie(team, player))
        {
            score -= 50;
        }

        if (player.HasNoTradeClause)
        {
            score -= 50;
        }

        if (player.IsCaptain)
        {
            score += player.WantsTrade || player.Morale < 35 ? 20 : -35;
        }
        else if (player.IsAlternateCaptain)
        {
            score -= 15;
        }

        if (IsCorePlayer(team, player))
        {
            score -= 25;
        }

        return TradeAiConfig.ClampScore(score);
    }

    public static string DetermineTradeBlockReason(
        GameState state,
        TeamData team,
        PlayerData player,
        TeamNeedData needs)
    {
        if (player == null)
        {
            return "";
        }

        string direction = needs == null || string.IsNullOrEmpty(needs.Direction)
            ? TeamDirectionService.DetermineTeamDirection(state, team)
            : needs.Direction;

        MoraleService.InitializePlayerMorale(player);
        LeadershipService.EnsurePlayerLeadershipProfile(player);
        if (player.IsCaptain && player.WantsTrade)
        {
            return "Captain wants trade";
        }

        if (player.IsCaptain)
        {
            return "Team captain";
        }

        if (player.IsAlternateCaptain)
        {
            return "Alternate captain";
        }

        if (player.WantsTrade)
        {
            return "Trade request";
        }

        if (player.RefusesExtensionThisSeason)
        {
            return "Extension refusal";
        }

        if (TradeAiConfig.IsPendingUfa(player) && player.ExtensionInterest < ContractExtensionConfig.LowInterestThreshold)
        {
            return "Pending UFA";
        }

        if (needs != null && needs.NeedCapSpace >= TradeAiConfig.NeedHigh && IsExpensiveForRole(player))
        {
            return "Cap pressure";
        }

        if (IsExcessPositionPlayer(team, player))
        {
            return TradeAiConfig.IsGoalie(player) ? "Extra goalie" : "Excess position";
        }

        if (direction == TradeAiConfig.DirectionRebuild && TradeAiConfig.IsVeteran(player))
        {
            return "Rebuild veteran";
        }

        if (TradeAiConfig.IsPendingUfa(player))
        {
            return "Pending UFA";
        }

        if (RosterStatusConfig.IsFarmRoster(player))
        {
            return "Farm depth";
        }

        return "Depth player";
    }

    public static bool IsCorePlayer(TeamData team, PlayerData player)
    {
        if (player == null)
        {
            return false;
        }

        if (player.Overall >= 84 || (player.Potential >= 86 && player.Age <= 24))
        {
            return true;
        }

        if (IsTopGoalie(team, player))
        {
            return true;
        }

        if (TradeAiConfig.IsForward(player))
        {
            return IsTopRankedByGroup(team, player, "Forward", 3);
        }

        if (TradeAiConfig.IsDefenseman(player))
        {
            return IsTopRankedByGroup(team, player, "Defense", 2);
        }

        return false;
    }

    public static bool IsExcessPositionPlayer(TeamData team, PlayerData player)
    {
        if (team == null || player == null || IsCorePlayer(team, player))
        {
            return false;
        }

        int forwards = 0;
        int defensemen = 0;
        int goalies = 0;
        foreach (PlayerData organizationPlayer in GetNhlAndFarmPlayers(team))
        {
            if (TradeAiConfig.IsForward(organizationPlayer))
            {
                forwards++;
            }
            else if (TradeAiConfig.IsDefenseman(organizationPlayer))
            {
                defensemen++;
            }
            else if (TradeAiConfig.IsGoalie(organizationPlayer))
            {
                goalies++;
            }
        }

        return (TradeAiConfig.IsForward(player) && forwards > 15)
            || (TradeAiConfig.IsDefenseman(player) && defensemen > 9)
            || (TradeAiConfig.IsGoalie(player) && goalies > 3);
    }

    private static bool IsScratch(TeamData team, PlayerData player)
    {
        if (team == null || team.Lineup == null || player == null)
        {
            return false;
        }

        team.Lineup.EnsureCollections();
        return team.Lineup.ScratchPlayerIds.Contains(player.Id);
    }

    private static bool IsExpensiveForRole(PlayerData player)
    {
        if (player == null)
        {
            return false;
        }

        if (player.Overall < 75 && player.Salary >= 2500000)
        {
            return true;
        }

        return player.Overall < 82 && player.Salary >= 6500000;
    }

    private static bool IsTopGoalie(TeamData team, PlayerData player)
    {
        return TradeAiConfig.IsGoalie(player) && IsTopRankedByGroup(team, player, "Goalie", 1);
    }

    private static bool IsTopRankedByGroup(TeamData team, PlayerData player, string group, int topCount)
    {
        List<PlayerData> players = GetNhlAndFarmPlayers(team);
        List<PlayerData> groupPlayers = new List<PlayerData>();
        foreach (PlayerData candidate in players)
        {
            if (group == "Forward" && TradeAiConfig.IsForward(candidate))
            {
                groupPlayers.Add(candidate);
            }
            else if (group == "Defense" && TradeAiConfig.IsDefenseman(candidate))
            {
                groupPlayers.Add(candidate);
            }
            else if (group == "Goalie" && TradeAiConfig.IsGoalie(candidate))
            {
                groupPlayers.Add(candidate);
            }
        }

        groupPlayers.Sort(ComparePlayersByOverall);
        for (int i = 0; i < groupPlayers.Count && i < topCount; i++)
        {
            if (groupPlayers[i] != null && groupPlayers[i].Id == player.Id)
            {
                return true;
            }
        }

        return false;
    }

    private static List<PlayerData> GetNhlAndFarmPlayers(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (team == null)
        {
            return players;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        foreach (PlayerData player in team.Players)
        {
            if (player != null && !player.IsRetired && (RosterStatusConfig.IsNhlRoster(player) || RosterStatusConfig.IsFarmRoster(player)))
            {
                players.Add(player);
            }
        }

        return players;
    }

    private static int ComparePlayersByOverall(PlayerData left, PlayerData right)
    {
        int comparison = right.Overall.CompareTo(left.Overall);
        if (comparison != 0)
        {
            return comparison;
        }

        comparison = right.Potential.CompareTo(left.Potential);
        return comparison != 0 ? comparison : string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }

    private static int CompareTradeBlockPlayers(TradeBlockPlayerData left, TradeBlockPlayerData right)
    {
        int comparison = right.AvailabilityScore.CompareTo(left.AvailabilityScore);
        if (comparison != 0)
        {
            return comparison;
        }

        comparison = right.TradeValue.CompareTo(left.TradeValue);
        return comparison != 0 ? comparison : string.Compare(left.PlayerId, right.PlayerId, StringComparison.Ordinal);
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
