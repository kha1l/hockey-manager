using System;
using System.Collections.Generic;
using UnityEngine;

public static class MoraleService
{
    public static void EnsureMorale(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureMoraleHistory();
        EnsureMoraleForTeams(state, state.Teams);
        TrimMoraleHistory(state);
    }

    public static void EnsureMoraleForTeam(GameState state, TeamData team)
    {
        if (team == null)
        {
            return;
        }

        if (state != null)
        {
            state.EnsureMoraleHistory();
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        PlayerRoleService.EnsureRolesForTeam(team);
        CoachingStaffService.EnsureStaffForTeam(team);
        foreach (PlayerData player in team.Players)
        {
            InitializePlayerMorale(player);
        }

        LeadershipService.EnsureLeadershipForTeam(team);
    }

    public static void EnsureMoraleForTeams(GameState state, List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureMoraleForTeam(state, team);
        }
    }

    public static void InitializePlayerMorale(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        if (player.HasMoraleInitialized)
        {
            player.Morale = MoraleConfig.ClampMorale(player.Morale);
            player.RoleSatisfaction = MoraleConfig.ClampMorale(player.RoleSatisfaction <= 0 ? MoraleConfig.DefaultMorale : player.RoleSatisfaction);
            player.IceTimeSatisfaction = MoraleConfig.ClampMorale(player.IceTimeSatisfaction <= 0 ? MoraleConfig.DefaultMorale : player.IceTimeSatisfaction);
            player.TeamPerformanceSatisfaction = MoraleConfig.ClampMorale(player.TeamPerformanceSatisfaction <= 0 ? MoraleConfig.DefaultMorale : player.TeamPerformanceSatisfaction);
            player.ContractSatisfaction = MoraleConfig.ClampMorale(player.ContractSatisfaction <= 0 ? MoraleConfig.DefaultMorale : player.ContractSatisfaction);
            player.RosterStatusSatisfaction = MoraleConfig.ClampMorale(player.RosterStatusSatisfaction <= 0 ? MoraleConfig.DefaultMorale : player.RosterStatusSatisfaction);
            player.OverallSatisfaction = MoraleConfig.ClampMorale(player.OverallSatisfaction <= 0 ? MoraleConfig.DefaultMorale : player.OverallSatisfaction);
            if (string.IsNullOrEmpty(player.MoraleStatus))
            {
                player.MoraleStatus = MoraleConfig.GetMoraleStatus(player.Morale);
            }

            if (string.IsNullOrEmpty(player.MoraleTrend))
            {
                player.MoraleTrend = MoraleConfig.TrendStable;
            }

            if (string.IsNullOrEmpty(player.MoraleSummary))
            {
                player.MoraleSummary = "No major concerns";
            }

            if (string.IsNullOrEmpty(player.LastMoraleUpdateUtc))
            {
                player.LastMoraleUpdateUtc = DateTime.UtcNow.ToString("o");
            }

            PlayerExpectationService.EnsurePlayerExpectations(player);
            return;
        }

        player.Morale = MoraleConfig.DefaultMorale;
        player.RoleSatisfaction = MoraleConfig.DefaultMorale;
        player.IceTimeSatisfaction = MoraleConfig.DefaultMorale;
        player.TeamPerformanceSatisfaction = MoraleConfig.DefaultMorale;
        player.ContractSatisfaction = MoraleConfig.DefaultMorale;
        player.RosterStatusSatisfaction = MoraleConfig.DefaultMorale;
        player.OverallSatisfaction = MoraleConfig.DefaultMorale;
        player.WantsTrade = false;
        player.LowMoraleDays = 0;
        player.LowMoraleGames = 0;
        player.MoraleStatus = MoraleConfig.StatusContent;
        player.MoraleTrend = MoraleConfig.TrendStable;
        player.MoraleSummary = "No major concerns";
        player.LastMoraleUpdateUtc = DateTime.UtcNow.ToString("o");
        player.HasMoraleInitialized = true;
        PlayerExpectationService.EnsurePlayerExpectations(player);
    }

    public static void UpdateMoraleAfterGameDay(GameState state)
    {
        EnsureMorale(state);
        if (state == null || state.Teams == null)
        {
            return;
        }

        foreach (TeamData team in state.Teams)
        {
            UpdateMoraleForTeam(state, team);
        }

        TrimMoraleHistory(state);
    }

    public static void UpdateMoraleForTeam(GameState state, TeamData team)
    {
        if (team == null)
        {
            return;
        }

        EnsureMoraleForTeam(state, team);
        IceTimeService.EnsureUsageForTeam(team);
        PlayerRoleService.EnsureRolesForTeam(team);

        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            InitializePlayerMorale(player);
            int moraleBefore = player.Morale;
            int role = PlayerExpectationService.CalculateRoleSatisfaction(player);
            int iceTime = PlayerExpectationService.CalculateIceTimeSatisfaction(player);
            int teamPerformance = CalculateTeamPerformanceSatisfaction(state, team, player);
            int contract = CalculateContractSatisfaction(player);
            int rosterStatus = CalculateRosterStatusSatisfaction(player);
            int overall = ApplyLeadershipImpact(
                team,
                player,
                CalculateWeightedOverall(role, iceTime, rosterStatus, teamPerformance, contract));
            int moraleAfter = MoraleConfig.ClampMorale(Mathf.RoundToInt(moraleBefore * 0.70f + overall * 0.30f));

            player.RoleSatisfaction = role;
            player.IceTimeSatisfaction = iceTime;
            player.TeamPerformanceSatisfaction = teamPerformance;
            player.ContractSatisfaction = contract;
            player.RosterStatusSatisfaction = rosterStatus;
            player.OverallSatisfaction = overall;
            player.Morale = moraleAfter;
            player.MoraleTrend = MoraleConfig.GetMoraleTrend(moraleBefore, moraleAfter);
            player.MoraleStatus = MoraleConfig.GetMoraleStatus(moraleAfter);
            player.MoraleSummary = BuildMoraleSummary(player);
            player.LastMoraleUpdateUtc = DateTime.UtcNow.ToString("o");
            player.HasMoraleInitialized = true;

            if (Mathf.Abs(moraleAfter - moraleBefore) >= 5)
            {
                AddMoraleEvent(state, team, player, "MoraleUpdate", moraleBefore, moraleAfter, player.MoraleSummary);
            }

            UpdateTradeRequestStatus(state, team, player, moraleBefore);
        }
    }

    public static PlayerMoraleSnapshotData BuildMoraleSnapshot(GameState state, TeamData team, PlayerData player)
    {
        InitializePlayerMorale(player);
        LeadershipService.EnsurePlayerLeadershipProfile(player);
        string teamName = GetTeamName(team);
        return new PlayerMoraleSnapshotData
        {
            PlayerId = player == null ? "" : player.Id,
            PlayerName = GetPlayerName(player),
            TeamId = team == null ? "" : team.Id,
            TeamName = teamName,
            Position = player == null ? "" : player.Position,
            Age = player == null ? 0 : player.Age,
            Overall = player == null ? 0 : player.Overall,
            Potential = player == null ? 0 : player.Potential,
            RosterStatus = player == null ? "" : player.RosterStatus,
            PlayerRole = player == null ? "" : player.PlayerRole,
            UsageCategory = player == null ? "" : player.UsageCategory,
            ExpectedRole = player == null ? "" : player.ExpectedRole,
            ExpectedUsageCategory = player == null ? "" : player.ExpectedUsageCategory,
            EstimatedTimeOnIceSeconds = player == null ? 0 : player.EstimatedTimeOnIceSeconds,
            AverageTimeOnIceSeconds = player == null ? 0 : player.AverageTimeOnIceSeconds,
            ExpectedTimeOnIceSeconds = player == null ? 0 : player.ExpectedTimeOnIceSeconds,
            CaptaincyRole = player == null ? LeadershipConfig.RoleNone : player.CaptaincyRole,
            Leadership = player == null ? 0 : player.Leadership,
            Morale = player == null ? MoraleConfig.DefaultMorale : player.Morale,
            RoleSatisfaction = player == null ? MoraleConfig.DefaultMorale : player.RoleSatisfaction,
            IceTimeSatisfaction = player == null ? MoraleConfig.DefaultMorale : player.IceTimeSatisfaction,
            TeamPerformanceSatisfaction = player == null ? MoraleConfig.DefaultMorale : player.TeamPerformanceSatisfaction,
            ContractSatisfaction = player == null ? MoraleConfig.DefaultMorale : player.ContractSatisfaction,
            RosterStatusSatisfaction = player == null ? MoraleConfig.DefaultMorale : player.RosterStatusSatisfaction,
            OverallSatisfaction = player == null ? MoraleConfig.DefaultMorale : player.OverallSatisfaction,
            WantsTrade = player != null && player.WantsTrade,
            MoraleStatus = player == null ? MoraleConfig.StatusContent : player.MoraleStatus,
            MoraleTrend = player == null ? MoraleConfig.TrendStable : player.MoraleTrend,
            MoraleSummary = player == null ? "" : player.MoraleSummary,
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    public static TeamMoraleSummaryData BuildTeamMoraleSummary(GameState state, TeamData team)
    {
        EnsureMoraleForTeam(state, team);
        TeamMoraleSummaryData summary = new TeamMoraleSummaryData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            LowestMorale = 100,
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        if (team == null || team.Players == null || team.Players.Count == 0)
        {
            summary.LowestMorale = 0;
            summary.Summary = "No players";
            return summary;
        }

        int totalMorale = 0;
        int count = 0;
        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            InitializePlayerMorale(player);
            count++;
            totalMorale += player.Morale;
            if (player.Morale < summary.LowestMorale)
            {
                summary.LowestMorale = player.Morale;
                summary.LowestMoralePlayerId = player.Id;
                summary.LowestMoralePlayerName = GetPlayerName(player);
            }

            if (player.WantsTrade)
            {
                summary.TradeRequests++;
            }

            string status = MoraleConfig.GetMoraleStatus(player.Morale);
            if (status == MoraleConfig.StatusHappy)
            {
                summary.HappyPlayers++;
            }
            else if (status == MoraleConfig.StatusContent)
            {
                summary.ContentPlayers++;
            }
            else if (status == MoraleConfig.StatusConcerned)
            {
                summary.ConcernedPlayers++;
            }
            else if (status == MoraleConfig.StatusUnhappy)
            {
                summary.UnhappyPlayers++;
            }
            else
            {
                summary.VeryUnhappyPlayers++;
            }
        }

        summary.AverageMorale = count == 0 ? 0 : totalMorale / count;
        if (count == 0)
        {
            summary.LowestMorale = 0;
        }

        summary.Summary = "Average morale " + summary.AverageMorale
            + " | Unhappy " + (summary.UnhappyPlayers + summary.VeryUnhappyPlayers)
            + " | Trade requests " + summary.TradeRequests;
        return summary;
    }

    public static List<PlayerMoraleSnapshotData> BuildTeamMoraleSnapshots(GameState state, TeamData team)
    {
        List<PlayerMoraleSnapshotData> snapshots = new List<PlayerMoraleSnapshotData>();
        EnsureMoraleForTeam(state, team);
        if (team == null || team.Players == null)
        {
            return snapshots;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null)
            {
                snapshots.Add(BuildMoraleSnapshot(state, team, player));
            }
        }

        snapshots.Sort(CompareSnapshots);
        return snapshots;
    }

    public static int CalculateOverallSatisfaction(GameState state, TeamData team, PlayerData player)
    {
        return ApplyLeadershipImpact(
            team,
            player,
            CalculateWeightedOverall(
                PlayerExpectationService.CalculateRoleSatisfaction(player),
                PlayerExpectationService.CalculateIceTimeSatisfaction(player),
                CalculateRosterStatusSatisfaction(player),
                CalculateTeamPerformanceSatisfaction(state, team, player),
                CalculateContractSatisfaction(player)));
    }

    public static int CalculateTeamPerformanceSatisfaction(GameState state, TeamData team, PlayerData player)
    {
        int satisfaction = 65;
        TeamStandingData standing = FindStanding(state, team);
        if (standing != null && standing.GamesPlayed > 0)
        {
            float pointsPercentage = (float)standing.Points / (standing.GamesPlayed * 2);
            if (pointsPercentage >= 0.650f)
            {
                satisfaction = 85;
            }
            else if (pointsPercentage >= 0.550f)
            {
                satisfaction = 75;
            }
            else if (pointsPercentage >= 0.450f)
            {
                satisfaction = 60;
            }
            else if (pointsPercentage >= 0.350f)
            {
                satisfaction = 45;
            }
            else
            {
                satisfaction = 35;
            }
        }

        string direction = TeamDirectionService.DetermineTeamDirection(state, team);
        if (direction == TradeAiConfig.DirectionRebuild && player != null && player.Age <= 23)
        {
            satisfaction += 10;
        }

        if ((direction == TradeAiConfig.DirectionContender || direction == TradeAiConfig.DirectionPlayoffTeam)
            && player != null
            && player.Overall >= 84
            && satisfaction < 60)
        {
            satisfaction -= 10;
        }

        TeamChemistryData chemistry = team == null ? null : team.Chemistry;
        if (chemistry == null && team != null)
        {
            chemistry = ChemistryService.CalculateTeamChemistry(team);
        }

        int chemistryScore = chemistry == null ? ChemistryConfig.DefaultChemistry : chemistry.TeamChemistryScore;
        if (chemistryScore >= ChemistryConfig.ExcellentThreshold)
        {
            satisfaction += 3;
        }
        else if (chemistryScore < ChemistryConfig.PoorThreshold)
        {
            satisfaction -= 5;
        }

        return MoraleConfig.ClampMorale(satisfaction);
    }

    public static int CalculateContractSatisfaction(PlayerData player)
    {
        if (player == null || player.Salary <= 0)
        {
            return MoraleConfig.DefaultMorale;
        }

        int satisfaction = 70;
        if (player.Overall >= 85 && player.Salary < 4000000)
        {
            satisfaction = 45;
        }
        else if (player.Overall >= 80 && player.Salary < 2500000)
        {
            satisfaction = 50;
        }
        else if (player.Salary >= 8000000 && player.Overall < 82)
        {
            satisfaction = 82;
        }
        else if (player.Overall >= 70 && player.Overall <= 79)
        {
            satisfaction = 70;
        }

        if (player.ContractYearsRemaining <= 1 && player.Overall >= 80)
        {
            satisfaction -= 5;
        }

        return MoraleConfig.ClampMorale(satisfaction);
    }

    public static int CalculateRosterStatusSatisfaction(PlayerData player)
    {
        if (player == null)
        {
            return MoraleConfig.DefaultMorale;
        }

        if (RosterStatusConfig.IsNhlRoster(player))
        {
            return player.IsInjured ? 75 : 82;
        }

        if (RosterStatusConfig.IsFarmRoster(player))
        {
            if (player.Age <= 23 && player.Overall < 74)
            {
                return 70;
            }

            if (player.Overall >= 78)
            {
                return 30;
            }

            if (player.Age >= 28)
            {
                return 35;
            }

            return 55;
        }

        if (RosterStatusConfig.IsReserve(player))
        {
            return 45;
        }

        return player.RosterStatus == RosterStatusConfig.FreeAgent ? 50 : 55;
    }

    public static void UpdateTradeRequestStatus(GameState state, TeamData team, PlayerData player, int moraleBefore)
    {
        if (player == null)
        {
            return;
        }

        if (player.Morale < MoraleConfig.TradeRequestMoraleThreshold)
        {
            player.LowMoraleGames++;
            player.LowMoraleDays++;
        }
        else
        {
            player.LowMoraleGames = Mathf.Max(0, player.LowMoraleGames - 1);
            player.LowMoraleDays = Mathf.Max(0, player.LowMoraleDays - 1);
        }

        if (!player.WantsTrade && player.LowMoraleGames >= MoraleConfig.TradeRequestLowMoraleGamesRequired)
        {
            player.WantsTrade = true;
            AddMoraleEvent(state, team, player, "TradeRequest", moraleBefore, player.Morale, "Player requested a trade after sustained low morale");
            EventNewsService.CreateTradeRequestNews(state, team, player);
        }
        else if (player.WantsTrade && player.Morale >= 55)
        {
            player.WantsTrade = false;
            player.LowMoraleGames = 0;
            player.LowMoraleDays = 0;
            AddMoraleEvent(state, team, player, "TradeRequestCleared", moraleBefore, player.Morale, "Trade request cleared after morale recovered");
        }
    }

    public static void AddMoraleEvent(GameState state, TeamData team, PlayerData player, string eventType, int before, int after, string reason)
    {
        if (state == null || player == null)
        {
            return;
        }

        state.EnsureMoraleHistory();
        MoraleEventData moraleEvent = new MoraleEventData
        {
            PlayerId = player.Id,
            PlayerName = GetPlayerName(player),
            TeamId = team == null ? player.TeamId : team.Id,
            TeamName = team == null ? "" : GetTeamName(team),
            EventType = eventType,
            MoraleBefore = before,
            MoraleAfter = after,
            Delta = after - before,
            Reason = reason,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        state.MoraleHistory.Events.Add(moraleEvent);
        state.MoraleHistory.TotalEvents++;
        state.MoraleHistory.LastEventAtUtc = moraleEvent.CreatedAtUtc;
        TrimMoraleHistory(state);
    }

    public static void TrimMoraleHistory(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureMoraleHistory();
        while (state.MoraleHistory.Events.Count > MoraleConfig.MaxMoraleEventsToKeep)
        {
            state.MoraleHistory.Events.RemoveAt(0);
        }
    }

    private static TeamStandingData FindStanding(GameState state, TeamData team)
    {
        if (state == null || state.Season == null || state.Season.Standings == null || team == null)
        {
            return null;
        }

        foreach (TeamStandingData standing in state.Season.Standings)
        {
            if (standing != null && standing.TeamId == team.Id)
            {
                return standing;
            }
        }

        return null;
    }

    private static int CalculateWeightedOverall(int role, int iceTime, int rosterStatus, int teamPerformance, int contract)
    {
        return MoraleConfig.ClampMorale(Mathf.RoundToInt(
            role * 0.30f
            + iceTime * 0.25f
            + rosterStatus * 0.20f
            + teamPerformance * 0.15f
            + contract * 0.10f));
    }

    private static int ApplyLeadershipImpact(TeamData team, PlayerData player, int overall)
    {
        int leadershipImpact = Mathf.Clamp(LeadershipService.GetTeamMoraleImpact(team), -6, LeadershipConfig.CaptainMoraleBonusMax);
        if (player != null && player.Age <= 23)
        {
            leadershipImpact += Mathf.Max(0, LeadershipService.GetYoungPlayerDevelopmentSupport(team));
        }

        int staffImpact = CoachingStaffService.GetMoraleModifier(team);
        return MoraleConfig.ClampMorale(overall + leadershipImpact + staffImpact);
    }

    private static string BuildMoraleSummary(PlayerData player)
    {
        if (player == null)
        {
            return "";
        }

        int lowest = player.RoleSatisfaction;
        string concern = "No major concerns";

        if (player.IceTimeSatisfaction < lowest)
        {
            lowest = player.IceTimeSatisfaction;
            concern = "Ice time concern";
        }

        if (player.RosterStatusSatisfaction < lowest)
        {
            lowest = player.RosterStatusSatisfaction;
            concern = "Roster status concern";
        }

        if (player.TeamPerformanceSatisfaction < lowest)
        {
            lowest = player.TeamPerformanceSatisfaction;
            concern = "Team performance concern";
        }

        if (player.ContractSatisfaction < lowest)
        {
            lowest = player.ContractSatisfaction;
            concern = "Contract concern";
        }

        if (player.WantsTrade)
        {
            return "Trade request";
        }

        return lowest < 55 ? concern : "No major concerns";
    }

    private static int CompareSnapshots(PlayerMoraleSnapshotData left, PlayerMoraleSnapshotData right)
    {
        int moraleCompare = (left == null ? 100 : left.Morale).CompareTo(right == null ? 100 : right.Morale);
        if (moraleCompare != 0)
        {
            return moraleCompare;
        }

        return string.Compare(left == null ? "" : left.PlayerName, right == null ? "" : right.PlayerName, StringComparison.Ordinal);
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static string GetPlayerName(PlayerData player)
    {
        if (player == null)
        {
            return "";
        }

        return player.FirstName + " " + player.LastName;
    }
}
