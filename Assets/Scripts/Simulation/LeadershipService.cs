using System;
using System.Collections.Generic;
using UnityEngine;

public static class LeadershipService
{
    public static void EnsureLeadershipForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        PlayerRoleService.EnsureRolesForTeam(team);
        foreach (PlayerData player in team.Players)
        {
            EnsurePlayerLeadershipProfile(player);
        }

        NormalizeCaptaincy(team);
        team.LeadershipData = CalculateTeamLeadership(team);
    }

    public static void EnsureLeadershipForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureLeadershipForTeam(team);
        }
    }

    public static void EnsurePlayerLeadershipProfile(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        if (player.HasLeadershipProfile)
        {
            player.Leadership = LeadershipConfig.ClampLeadership(player.Leadership);
            player.Professionalism = LeadershipConfig.ClampLeadership(player.Professionalism);
            player.LockerRoomInfluence = LeadershipConfig.ClampLeadership(player.LockerRoomInfluence);
            NormalizeCaptaincyRole(player);
            return;
        }

        MoraleService.InitializePlayerMorale(player);
        TeamRosterService.SetInitialRosterStatus(player, RosterStatusConfig.NHL);
        int leadership = 45;
        if (player.Age >= 34)
        {
            leadership += 15;
        }
        else if (player.Age >= 30)
        {
            leadership += 12;
        }
        else if (player.Age >= 26)
        {
            leadership += 6;
        }
        else if (player.Age <= 22)
        {
            leadership -= 5;
        }

        if (player.Overall >= 85)
        {
            leadership += 10;
        }
        else if (player.Overall >= 80)
        {
            leadership += 6;
        }
        else if (player.Overall >= 74)
        {
            leadership += 2;
        }

        if (player.Morale >= 80)
        {
            leadership += 5;
        }
        else if (player.Morale < 40)
        {
            leadership -= 8;
        }

        leadership += RosterStatusConfig.IsNhlRoster(player) ? 5 : -5;
        leadership += StableRange(GetPlayerSeed(player) + ":leadership", -8, 8);

        int professionalism = StableRange(GetPlayerSeed(player) + ":professionalism", 45, 85);
        if (player.Age >= 30)
        {
            professionalism += 5;
        }
        else if (player.Age <= 21)
        {
            professionalism -= 3;
        }

        int moraleLike = player.Morale <= 0 ? MoraleConfig.DefaultMorale : player.Morale;
        int influence = Mathf.RoundToInt((leadership * 0.45f) + (professionalism * 0.35f) + (moraleLike * 0.20f));
        influence += StableRange(GetPlayerSeed(player) + ":influence", -5, 5);

        player.Leadership = LeadershipConfig.ClampLeadership(leadership);
        player.Professionalism = LeadershipConfig.ClampLeadership(professionalism);
        player.LockerRoomInfluence = LeadershipConfig.ClampLeadership(influence);
        player.IsAlternateCaptain = player.IsAlternateCaptain && !player.IsCaptain;
        NormalizeCaptaincyRole(player);
        player.HasLeadershipProfile = true;
    }

    public static TeamLeadershipData CalculateTeamLeadership(TeamData team)
    {
        TeamLeadershipData data = new TeamLeadershipData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        if (team == null)
        {
            data.LeadershipScore = LeadershipConfig.DefaultLeadership;
            data.LeadershipLabel = LeadershipConfig.GetLeadershipLabel(data.LeadershipScore);
            data.LeadershipSummary = "No team data";
            return data;
        }

        team.EnsurePlayers();
        List<LeadershipCandidateData> candidates = BuildLeadershipCandidates(team);
        data.Candidates = candidates;
        PlayerData captain = FindCaptain(team);
        List<PlayerData> alternates = FindAlternates(team);
        int teamAverageLeadership = CalculateTeamAverageLeadership(team);
        int alternateAverage = CalculateAlternateAverage(alternates);
        int penalties = 0;

        if (captain == null)
        {
            int averageCandidateScore = AverageCandidateScore(candidates);
            data.LeadershipScore = LeadershipConfig.ClampLeadership(averageCandidateScore + LeadershipConfig.NoCaptainPenalty);
            penalties += LeadershipConfig.NoCaptainPenalty;
            data.LeadershipSummary = "No captain assigned";
        }
        else
        {
            data.CaptainPlayerId = captain.Id;
            data.CaptainName = GetPlayerName(captain);
            data.LeadershipScore = LeadershipConfig.ClampLeadership(Mathf.RoundToInt(
                CalculateCaptaincyScore(captain) * 0.60f
                + alternateAverage * 0.30f
                + teamAverageLeadership * 0.10f));

            if (captain.Morale < 40)
            {
                penalties += LeadershipConfig.UnhappyCaptainPenalty;
                data.LeadershipSummary = "Captain morale issue";
            }
            else if (captain.WantsTrade)
            {
                penalties += LeadershipConfig.TradeRequestCaptainPenalty;
                data.LeadershipSummary = "Captain wants trade";
            }
            else if (data.LeadershipScore >= LeadershipConfig.GoodLeadershipThreshold)
            {
                data.LeadershipSummary = "Strong leadership group";
            }
            else
            {
                data.LeadershipSummary = "Average leadership";
            }
        }

        if (alternates.Count > 0)
        {
            data.Alternate1PlayerId = alternates[0].Id;
            data.Alternate1Name = GetPlayerName(alternates[0]);
        }

        if (alternates.Count > 1)
        {
            data.Alternate2PlayerId = alternates[1].Id;
            data.Alternate2Name = GetPlayerName(alternates[1]);
        }

        data.LeadershipLabel = LeadershipConfig.GetLeadershipLabel(data.LeadershipScore);
        data.LockerRoomImpact = LeadershipConfig.ClampLeadership(Mathf.RoundToInt(
            data.LeadershipScore * 0.60f + teamAverageLeadership * 0.40f));
        data.MoraleImpact = Mathf.Clamp(LeadershipConfig.GetMoraleImpactByLeadership(data.LeadershipScore) + penalties, -6, LeadershipConfig.CaptainMoraleBonusMax);
        data.ChemistryImpact = Mathf.Clamp(LeadershipConfig.GetChemistryImpactByLeadership(data.LeadershipScore) + penalties, -6, LeadershipConfig.CaptainChemistryBonusMax);
        data.EnsureCandidates();
        team.LeadershipData = data;
        return data;
    }

    public static List<LeadershipCandidateData> BuildLeadershipCandidates(TeamData team)
    {
        List<LeadershipCandidateData> candidates = new List<LeadershipCandidateData>();
        if (team == null || team.Players == null)
        {
            return candidates;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null)
            {
                candidates.Add(BuildCandidate(team, player));
            }
        }

        candidates.Sort(CompareCandidates);
        return candidates;
    }

    public static LeadershipCandidateData BuildCandidate(TeamData team, PlayerData player)
    {
        EnsurePlayerLeadershipProfile(player);
        return new LeadershipCandidateData
        {
            PlayerId = player == null ? "" : player.Id,
            PlayerName = GetPlayerName(player),
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            Position = player == null ? "" : player.Position,
            Age = player == null ? 0 : player.Age,
            Overall = player == null ? 0 : player.Overall,
            RosterStatus = player == null ? "" : player.RosterStatus,
            PlayerRole = player == null ? "" : player.PlayerRole,
            Morale = player == null ? 0 : player.Morale,
            WantsTrade = player != null && player.WantsTrade,
            Leadership = player == null ? 0 : player.Leadership,
            Professionalism = player == null ? 0 : player.Professionalism,
            LockerRoomInfluence = player == null ? 0 : player.LockerRoomInfluence,
            CaptaincyScore = CalculateCaptaincyScore(player),
            CurrentCaptaincyRole = player == null ? LeadershipConfig.RoleNone : NormalizeCaptaincyRole(player),
            CandidateSummary = BuildCandidateSummary(player),
            IsEligible = IsEligibleForCaptaincy(player)
        };
    }

    public static int CalculateCaptaincyScore(PlayerData player)
    {
        if (player == null)
        {
            return 0;
        }

        EnsurePlayerLeadershipProfile(player);
        int score = Mathf.RoundToInt(
            player.Leadership * 0.45f
            + player.Professionalism * 0.25f
            + player.LockerRoomInfluence * 0.20f
            + player.Morale * 0.10f);

        if (player.Age >= 30)
        {
            score += 5;
        }

        if (player.Overall >= 82)
        {
            score += 5;
        }

        if (IsTopLeadershipRole(player))
        {
            score += 3;
        }

        if (player.WantsTrade)
        {
            score -= 40;
        }

        if (player.Morale < 40)
        {
            score -= 20;
        }

        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            score -= 50;
        }

        if (player.IsOnWaivers)
        {
            score -= 30;
        }

        return LeadershipConfig.ClampLeadership(score);
    }

    public static bool IsEligibleForCaptaincy(PlayerData player)
    {
        if (player == null)
        {
            return false;
        }

        EnsurePlayerLeadershipProfile(player);
        return RosterStatusConfig.IsNhlRoster(player)
            && !player.IsOnWaivers
            && !string.IsNullOrEmpty(player.Position)
            && !player.WantsTrade;
    }

    public static CaptaincyActionResultData AutoAssignCaptains(TeamData team)
    {
        EnsureLeadershipForTeam(team);
        if (team == null)
        {
            return CreateResult(false, "Команда не найдена", null, null, "AutoAssign", LeadershipConfig.RoleNone);
        }

        List<LeadershipCandidateData> candidates = BuildLeadershipCandidates(team);
        List<PlayerData> eligiblePlayers = new List<PlayerData>();
        foreach (LeadershipCandidateData candidate in candidates)
        {
            if (candidate != null && candidate.IsEligible)
            {
                PlayerData player = FindPlayer(team, candidate.PlayerId);
                if (player != null)
                {
                    eligiblePlayers.Add(player);
                }
            }
        }

        if (eligiblePlayers.Count < 1)
        {
            return CreateResult(false, "Нет подходящих кандидатов", team, null, "AutoAssign", LeadershipConfig.RoleNone);
        }

        ClearAllCaptaincy(team);
        AssignCaptainFlags(eligiblePlayers[0], LeadershipConfig.RoleCaptain);
        if (eligiblePlayers.Count > 1)
        {
            AssignCaptainFlags(eligiblePlayers[1], LeadershipConfig.RoleAlternate);
        }

        if (eligiblePlayers.Count > 2)
        {
            AssignCaptainFlags(eligiblePlayers[2], LeadershipConfig.RoleAlternate);
        }

        team.LeadershipData = CalculateTeamLeadership(team);
        return CreateResult(true, "Капитаны назначены автоматически", team, eligiblePlayers[0], "AutoAssign", LeadershipConfig.RoleCaptain);
    }

    public static CaptaincyActionResultData AssignCaptain(TeamData team, string playerId)
    {
        EnsureLeadershipForTeam(team);
        PlayerData player = FindPlayer(team, playerId);
        if (team == null || player == null)
        {
            return CreateResult(false, "Игрок не найден", team, player, "AssignCaptain", LeadershipConfig.RoleCaptain);
        }

        if (!IsEligibleForCaptaincy(player))
        {
            return CreateResult(false, "Игрок не подходит для роли капитана", team, player, "AssignCaptain", LeadershipConfig.RoleCaptain);
        }

        ClearCurrentCaptain(team);
        ClearAlternateIfNeeded(team, player.Id);
        AssignCaptainFlags(player, LeadershipConfig.RoleCaptain);
        team.LeadershipData = CalculateTeamLeadership(team);
        return CreateResult(true, "Капитан назначен: " + GetPlayerName(player), team, player, "AssignCaptain", LeadershipConfig.RoleCaptain);
    }

    public static CaptaincyActionResultData AssignAlternateCaptain(TeamData team, string playerId)
    {
        EnsureLeadershipForTeam(team);
        PlayerData player = FindPlayer(team, playerId);
        if (team == null || player == null)
        {
            return CreateResult(false, "Игрок не найден", team, player, "AssignAlternate", LeadershipConfig.RoleAlternate);
        }

        if (!IsEligibleForCaptaincy(player))
        {
            return CreateResult(false, "Игрок не подходит для роли ассистента", team, player, "AssignAlternate", LeadershipConfig.RoleAlternate);
        }

        if (player.IsCaptain)
        {
            return CreateResult(false, "Капитан не может быть alternate", team, player, "AssignAlternate", LeadershipConfig.RoleAlternate);
        }

        if (player.IsAlternateCaptain)
        {
            return CreateResult(true, "Игрок уже alternate", team, player, "AssignAlternate", LeadershipConfig.RoleAlternate);
        }

        List<PlayerData> alternates = FindAlternates(team);
        string replacementText = "";
        if (alternates.Count >= 2)
        {
            alternates.Sort((left, right) => CalculateCaptaincyScore(left).CompareTo(CalculateCaptaincyScore(right)));
            PlayerData replaced = alternates[0];
            ClearCaptainFlags(replaced);
            replacementText = ". Заменён weakest alternate: " + GetPlayerName(replaced);
        }

        AssignCaptainFlags(player, LeadershipConfig.RoleAlternate);
        team.LeadershipData = CalculateTeamLeadership(team);
        return CreateResult(true, "Alternate назначен: " + GetPlayerName(player) + replacementText, team, player, "AssignAlternate", LeadershipConfig.RoleAlternate);
    }

    public static CaptaincyActionResultData ClearCaptaincy(TeamData team, string playerId)
    {
        EnsureLeadershipForTeam(team);
        PlayerData player = FindPlayer(team, playerId);
        if (team == null || player == null)
        {
            return CreateResult(false, "Игрок не найден", team, player, "ClearCaptaincy", LeadershipConfig.RoleNone);
        }

        ClearCaptainFlags(player);
        team.LeadershipData = CalculateTeamLeadership(team);
        return CreateResult(true, "Роль капитанства снята", team, player, "ClearCaptaincy", LeadershipConfig.RoleNone);
    }

    public static int GetTeamMoraleImpact(TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        if (team.LeadershipData == null)
        {
            team.LeadershipData = CalculateTeamLeadership(team);
        }

        return team.LeadershipData == null ? 0 : team.LeadershipData.MoraleImpact;
    }

    public static int GetTeamChemistryImpact(TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        if (team.LeadershipData == null)
        {
            team.LeadershipData = CalculateTeamLeadership(team);
        }

        return team.LeadershipData == null ? 0 : team.LeadershipData.ChemistryImpact;
    }

    public static int GetYoungPlayerDevelopmentSupport(TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        if (team.LeadershipData == null)
        {
            team.LeadershipData = CalculateTeamLeadership(team);
        }

        if (string.IsNullOrEmpty(team.LeadershipData.CaptainPlayerId))
        {
            return -1;
        }

        if (team.LeadershipData.LeadershipScore >= 80)
        {
            return 2;
        }

        return team.LeadershipData.LeadershipScore >= 65 ? 1 : 0;
    }

    public static void ClearPlayerCaptaincy(PlayerData player)
    {
        ClearCaptainFlags(player);
    }

    private static PlayerData FindPlayer(TeamData team, string playerId)
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

    private static void ClearCaptainFlags(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        player.IsCaptain = false;
        player.IsAlternateCaptain = false;
        player.CaptaincyRole = LeadershipConfig.RoleNone;
        player.CaptaincyAssignedAtUtc = "";
    }

    private static void ClearCurrentCaptain(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.IsCaptain)
            {
                ClearCaptainFlags(player);
            }
        }
    }

    private static void ClearAlternateIfNeeded(TeamData team, string playerId)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player != null && player.IsAlternateCaptain)
        {
            ClearCaptainFlags(player);
        }
    }

    private static CaptaincyActionResultData CreateResult(
        bool success,
        string message,
        TeamData team,
        PlayerData player,
        string actionType,
        string assignedRole)
    {
        return new CaptaincyActionResultData
        {
            Success = success,
            Message = string.IsNullOrEmpty(message) ? "" : message,
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            PlayerId = player == null ? "" : player.Id,
            PlayerName = GetPlayerName(player),
            ActionType = string.IsNullOrEmpty(actionType) ? "" : actionType,
            AssignedRole = string.IsNullOrEmpty(assignedRole) ? LeadershipConfig.RoleNone : assignedRole,
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    private static void NormalizeCaptaincy(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return;
        }

        List<PlayerData> captains = new List<PlayerData>();
        List<PlayerData> alternates = new List<PlayerData>();
        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            NormalizeCaptaincyRole(player);
            if ((player.IsCaptain || player.IsAlternateCaptain) && !RosterStatusConfig.IsNhlRoster(player))
            {
                ClearCaptainFlags(player);
                continue;
            }

            if (player.IsCaptain)
            {
                captains.Add(player);
            }
            else if (player.IsAlternateCaptain)
            {
                alternates.Add(player);
            }
        }

        captains.Sort((left, right) => CalculateCaptaincyScore(right).CompareTo(CalculateCaptaincyScore(left)));
        for (int i = 1; i < captains.Count; i++)
        {
            if (alternates.Count < 2)
            {
                AssignCaptainFlags(captains[i], LeadershipConfig.RoleAlternate);
                alternates.Add(captains[i]);
            }
            else
            {
                ClearCaptainFlags(captains[i]);
            }
        }

        alternates.Sort((left, right) => CalculateCaptaincyScore(right).CompareTo(CalculateCaptaincyScore(left)));
        for (int i = 2; i < alternates.Count; i++)
        {
            ClearCaptainFlags(alternates[i]);
        }
    }

    private static string NormalizeCaptaincyRole(PlayerData player)
    {
        if (player == null)
        {
            return LeadershipConfig.RoleNone;
        }

        if (player.IsCaptain)
        {
            player.IsAlternateCaptain = false;
            player.CaptaincyRole = LeadershipConfig.RoleCaptain;
        }
        else if (player.IsAlternateCaptain)
        {
            player.CaptaincyRole = LeadershipConfig.RoleAlternate;
        }
        else if (string.IsNullOrEmpty(player.CaptaincyRole))
        {
            player.CaptaincyRole = LeadershipConfig.RoleNone;
        }
        else if (player.CaptaincyRole == LeadershipConfig.RoleCaptain)
        {
            player.IsCaptain = true;
            player.IsAlternateCaptain = false;
        }
        else if (player.CaptaincyRole == LeadershipConfig.RoleAlternate)
        {
            player.IsAlternateCaptain = true;
        }
        else
        {
            player.CaptaincyRole = LeadershipConfig.RoleNone;
            player.IsCaptain = false;
            player.IsAlternateCaptain = false;
        }

        return player.CaptaincyRole;
    }

    private static void ClearAllCaptaincy(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return;
        }

        foreach (PlayerData player in team.Players)
        {
            ClearCaptainFlags(player);
        }
    }

    private static void AssignCaptainFlags(PlayerData player, string role)
    {
        if (player == null)
        {
            return;
        }

        player.IsCaptain = role == LeadershipConfig.RoleCaptain;
        player.IsAlternateCaptain = role == LeadershipConfig.RoleAlternate;
        player.CaptaincyRole = role;
        player.CaptaincyAssignedAtUtc = DateTime.UtcNow.ToString("o");
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

    private static List<PlayerData> FindAlternates(TeamData team)
    {
        List<PlayerData> alternates = new List<PlayerData>();
        if (team == null || team.Players == null)
        {
            return alternates;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.IsAlternateCaptain)
            {
                alternates.Add(player);
            }
        }

        alternates.Sort((left, right) => CalculateCaptaincyScore(right).CompareTo(CalculateCaptaincyScore(left)));
        if (alternates.Count > 2)
        {
            alternates.RemoveRange(2, alternates.Count - 2);
        }

        return alternates;
    }

    private static int CalculateTeamAverageLeadership(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return LeadershipConfig.DefaultLeadership;
        }

        int total = 0;
        int count = 0;
        foreach (PlayerData player in team.Players)
        {
            if (player == null || !RosterStatusConfig.IsNhlRoster(player))
            {
                continue;
            }

            EnsurePlayerLeadershipProfile(player);
            total += player.Leadership;
            count++;
        }

        return count == 0 ? LeadershipConfig.DefaultLeadership : total / count;
    }

    private static int CalculateAlternateAverage(List<PlayerData> alternates)
    {
        if (alternates == null || alternates.Count == 0)
        {
            return LeadershipConfig.DefaultLeadership;
        }

        int total = 0;
        foreach (PlayerData player in alternates)
        {
            total += CalculateCaptaincyScore(player);
        }

        return total / alternates.Count;
    }

    private static int AverageCandidateScore(List<LeadershipCandidateData> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return LeadershipConfig.DefaultLeadership;
        }

        int total = 0;
        int count = 0;
        foreach (LeadershipCandidateData candidate in candidates)
        {
            if (candidate == null || !candidate.IsEligible)
            {
                continue;
            }

            total += candidate.CaptaincyScore;
            count++;
        }

        return count == 0 ? LeadershipConfig.DefaultLeadership : total / count;
    }

    private static int CompareCandidates(LeadershipCandidateData left, LeadershipCandidateData right)
    {
        int eligibleComparison = right.IsEligible.CompareTo(left.IsEligible);
        if (eligibleComparison != 0)
        {
            return eligibleComparison;
        }

        int scoreComparison = right.CaptaincyScore.CompareTo(left.CaptaincyScore);
        if (scoreComparison != 0)
        {
            return scoreComparison;
        }

        int leadershipComparison = right.Leadership.CompareTo(left.Leadership);
        if (leadershipComparison != 0)
        {
            return leadershipComparison;
        }

        return right.Overall.CompareTo(left.Overall);
    }

    private static bool IsTopLeadershipRole(PlayerData player)
    {
        if (player == null)
        {
            return false;
        }

        PlayerRoleService.EnsureRole(player);
        return player.PlayerRole == PlayerRoleConfig.TwoWayForward
            || player.PlayerRole == PlayerRoleConfig.TwoWayDefenseman
            || player.PlayerRole == PlayerRoleConfig.Playmaker
            || player.PlayerRole == PlayerRoleConfig.DefensiveDefenseman
            || player.PlayerRole == PlayerRoleConfig.PowerForward;
    }

    private static string BuildCandidateSummary(PlayerData player)
    {
        if (player == null)
        {
            return "";
        }

        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            return "Not in Pro roster";
        }

        if (player.IsOnWaivers)
        {
            return "On waivers";
        }

        if (player.WantsTrade)
        {
            return "Wants trade";
        }

        if (player.Morale < 40)
        {
            return "Morale concern";
        }

        if (player.Leadership >= LeadershipConfig.GoodLeadershipThreshold)
        {
            return "Strong candidate";
        }

        return "Depth leadership option";
    }

    private static int StableRange(string seed, int minInclusive, int maxInclusive)
    {
        if (maxInclusive < minInclusive)
        {
            return minInclusive;
        }

        uint hash = 2166136261u;
        for (int i = 0; i < seed.Length; i++)
        {
            hash ^= seed[i];
            hash *= 16777619u;
        }

        uint range = (uint)(maxInclusive - minInclusive + 1);
        return minInclusive + (int)(hash % range);
    }

    private static string GetPlayerSeed(PlayerData player)
    {
        if (player == null)
        {
            return "";
        }

        return player.Id + ":" + player.FirstName + ":" + player.LastName;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : player.FirstName + " " + player.LastName;
    }
}
