using System;
using System.Collections.Generic;

public static class RetirementService
{
    public static void EnsureRetirementData(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureRetirementHistory();
        JerseyNumberService.EnsureJerseyNumbersForTeams(state.Teams);
        RemoveDuplicateRetiredPlayers(state);
    }

    public static void ProcessRetirementsAfterSeason(GameState state)
    {
        EnsureRetirementData(state);
        if (state == null)
        {
            return;
        }

        List<RetirementCandidate> candidates = GetRetirementCandidateEntries(state);
        foreach (RetirementCandidate candidate in candidates)
        {
            if (candidate == null || candidate.Player == null || IsAlreadyRetired(state, candidate.Player.Id))
            {
                continue;
            }

            if (ShouldRetire(state, candidate.Team, candidate.Player))
            {
                int score = CalculateRetirementScore(state, candidate.Team, candidate.Player);
                RetirePlayer(state, candidate.Team, candidate.Player, RetirementConfig.GetRetirementReasonByScore(score));
            }
        }

        HallOfFameService.ProcessHallOfFameForRetiredPlayers(state);
        RetiredNumberService.ProcessRetiredNumbersAfterSeason(state);
        state.LastRetirementUpdateAtUtc = DateTime.UtcNow.ToString("o");
    }

    public static int CalculateRetirementScore(GameState state, TeamData team, PlayerData player)
    {
        if (player == null)
        {
            return 0;
        }

        int score = 0;
        if (player.Age >= 40)
        {
            score += 75;
        }
        else if (player.Age >= 38)
        {
            score += 55;
        }
        else if (player.Age >= 36)
        {
            score += 35;
        }
        else if (player.Age >= RetirementConfig.MinimumRetirementAge)
        {
            score += 15;
        }

        if (player.Overall < 68)
        {
            score += 25;
        }
        else if (player.Overall <= 74)
        {
            score += 15;
        }
        else if (player.Overall <= 82)
        {
            score += 5;
        }
        else
        {
            score -= 10;
        }

        if (player.ContractYearsRemaining <= 0)
        {
            score += 20;
        }

        if (player.RosterStatus == RosterStatusConfig.FreeAgent)
        {
            score += 15;
        }

        if ((player.RosterStatus == RosterStatusConfig.Farm || player.RosterStatus == RosterStatusConfig.Reserve)
            && player.Age >= RetirementConfig.MinimumRetirementAge)
        {
            score += 20;
        }

        InjuryService.EnsureInjuryFields(player);
        if (player.IsInjured && player.InjuryDaysRemaining >= 30)
        {
            score += player.InjuryDaysRemaining >= 90 ? 20 : 10;
        }

        RetiredPlayerData preview = BuildRetiredPlayerData(state, team, player, "");
        int hallScore = HallOfFameService.CalculateHallOfFameScore(preview);
        player.HallOfFameScore = hallScore;
        if (hallScore >= RetirementConfig.HallOfFameScoreThreshold && player.Age < 40)
        {
            score -= 10;
        }

        if (player.Morale > 0 && player.Morale < 30 && player.ContractYearsRemaining <= 0)
        {
            score += 5;
        }

        if (player.Position == "G" && player.Age < 40)
        {
            score -= 5;
        }

        return Clamp(score, 0, 100);
    }

    public static bool ShouldRetire(GameState state, TeamData team, PlayerData player)
    {
        if (player == null || player.IsRetired || player.Age < RetirementConfig.MinimumRetirementAge)
        {
            return false;
        }

        if (player.Age >= RetirementConfig.MaximumPlayerAge)
        {
            return true;
        }

        int score = CalculateRetirementScore(state, team, player);
        player.RetirementScore = score;
        int threshold = player.Overall >= 84 && player.Age < RetirementConfig.HighRetirementAge ? 85 : RetirementConfig.RetirementScoreThreshold;
        return score >= threshold;
    }

    public static RetiredPlayerData RetirePlayer(GameState state, TeamData team, PlayerData player, string reason)
    {
        EnsureRetirementData(state);
        if (state == null || player == null)
        {
            return null;
        }

        if (IsAlreadyRetired(state, player.Id))
        {
            return FindRetiredPlayer(state, player.Id);
        }

        int score = CalculateRetirementScore(state, team, player);
        player.IsRetired = true;
        player.RetiredAtUtc = DateTime.UtcNow.ToString("o");
        player.RetirementSeasonStartYear = state.CurrentSeasonStartYear;
        player.RetirementSeasonEndYear = state.CurrentSeasonEndYear;
        player.RetirementReason = string.IsNullOrEmpty(reason) ? RetirementConfig.GetRetirementReasonByScore(score) : reason;
        player.RetirementScore = score;

        RetiredPlayerData retiredPlayer = BuildRetiredPlayerData(state, team, player, player.RetirementReason);
        AddRetiredPlayer(state, retiredPlayer);
        RemoveRetiredPlayerFromActiveSystems(state, team, player);
        EventNewsService.CreateRetirementNews(state, retiredPlayer);
        return retiredPlayer;
    }

    public static RetiredPlayerData BuildRetiredPlayerData(GameState state, TeamData team, PlayerData player, string reason)
    {
        if (player == null)
        {
            return null;
        }

        CareerStatsService.EnsureCareerStats(player);
        string primaryTeamId = string.IsNullOrEmpty(player.PrimaryCareerTeamId)
            ? (team == null ? "" : team.Id)
            : player.PrimaryCareerTeamId;
        string primaryTeamName = string.IsNullOrEmpty(player.PrimaryCareerTeamName)
            ? GetTeamName(team)
            : player.PrimaryCareerTeamName;
        int hofScore = HallOfFameService.CalculateHallOfFameScore(new RetiredPlayerData
        {
            Position = player.Position,
            CareerGamesPlayed = player.CareerGamesPlayed,
            CareerGoals = player.CareerGoals,
            CareerAssists = player.CareerAssists,
            CareerPoints = player.CareerPoints,
            CareerWins = player.CareerWins,
            CareerShutouts = player.CareerShutouts,
            CareerAwardsCount = player.CareerAwardsCount,
            ChampionshipsWon = player.ChampionshipsWon,
            PlayoffRoundsWonCareer = player.PlayoffRoundsWonCareer
        });

        player.HallOfFameScore = hofScore;
        player.IsHallOfFameEligible = hofScore >= RetirementConfig.HallOfFameScoreThreshold;
        player.HallOfFameSummary = RetirementConfig.GetHallOfFameLabel(hofScore);

        return new RetiredPlayerData
        {
            PlayerId = player.Id,
            PlayerName = GetPlayerName(player),
            Position = player.Position,
            Age = player.Age,
            JerseyNumber = player.JerseyNumber,
            PrimaryTeamId = primaryTeamId,
            PrimaryTeamName = primaryTeamName,
            SeasonsWithPrimaryTeam = player.SeasonsWithPrimaryTeam,
            LastTeamId = team == null ? player.TeamId : team.Id,
            LastTeamName = team == null ? "" : GetTeamName(team),
            RetirementSeasonStartYear = state == null ? player.RetirementSeasonStartYear : state.CurrentSeasonStartYear,
            RetirementSeasonEndYear = state == null ? player.RetirementSeasonEndYear : state.CurrentSeasonEndYear,
            RetirementReason = string.IsNullOrEmpty(reason) ? player.RetirementReason : reason,
            CareerGamesPlayed = player.CareerGamesPlayed,
            CareerGoals = player.CareerGoals,
            CareerAssists = player.CareerAssists,
            CareerPoints = player.CareerPoints,
            CareerWins = player.CareerWins,
            CareerShutouts = player.CareerShutouts,
            CareerAwardsCount = player.CareerAwardsCount,
            ChampionshipsWon = player.ChampionshipsWon,
            PlayoffRoundsWonCareer = player.PlayoffRoundsWonCareer,
            HallOfFameScore = hofScore,
            IsHallOfFameEligible = hofScore >= RetirementConfig.HallOfFameScoreThreshold,
            IsHallOfFameInducted = player.IsHallOfFameInducted,
            HallOfFameInductionYear = player.HallOfFameInductionYear,
            HasRetiredNumber = player.HasRetiredNumber,
            RetiredNumberTeamId = player.RetiredNumberTeamId,
            RetiredNumberTeamName = player.RetiredNumberTeamName,
            CareerSummary = string.IsNullOrEmpty(player.CareerSummary) ? CareerStatsService.BuildCareerSummary(player) : player.CareerSummary,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    public static void RemoveRetiredPlayerFromActiveSystems(GameState state, TeamData team, PlayerData player)
    {
        if (state == null || player == null)
        {
            return;
        }

        if (team != null)
        {
            team.EnsurePlayers();
            if (player.IsCaptain || player.IsAlternateCaptain)
            {
                LeadershipService.ClearCaptaincy(team, player.Id);
            }

            team.Players.Remove(player);
            if (team.Lineup != null)
            {
                team.Lineup = LineupService.BuildAutoLineup(team);
            }

            if (team.SpecialTeams != null)
            {
                team.SpecialTeams = SpecialTeamsService.BuildAutoSpecialTeams(team);
            }
        }

        if (state.FreeAgentPool != null && state.FreeAgentPool.FreeAgents != null)
        {
            state.FreeAgentPool.FreeAgents.Remove(player);
            for (int i = state.FreeAgentPool.FreeAgents.Count - 1; i >= 0; i--)
            {
                PlayerData freeAgent = state.FreeAgentPool.FreeAgents[i];
                if (freeAgent != null && freeAgent.Id == player.Id)
                {
                    state.FreeAgentPool.FreeAgents.RemoveAt(i);
                }
            }
        }

        CancelRetiredPlayerWaivers(state, player);
        LeadershipService.ClearPlayerCaptaincy(player);
        player.TeamId = "";
        player.RosterStatus = "Retired";
        player.PreviousRosterStatus = "";
        player.IsOnWaivers = false;
        player.WaiverStatus = WaiverConfig.WaiverStatusNone;
        player.WaiverIntendedDestination = "";
    }

    public static void AddRetiredPlayer(GameState state, RetiredPlayerData retiredPlayer)
    {
        EnsureRetirementData(state);
        if (state == null || retiredPlayer == null || string.IsNullOrEmpty(retiredPlayer.PlayerId))
        {
            return;
        }

        RetiredPlayerData existing = FindRetiredPlayer(state, retiredPlayer.PlayerId);
        if (existing != null)
        {
            return;
        }

        state.RetiredPlayers.Players.Add(retiredPlayer);
        state.RetiredPlayers.TotalRetiredPlayers = state.RetiredPlayers.Players.Count;
        state.RetiredPlayers.LastRetirementAtUtc = retiredPlayer.CreatedAtUtc;
    }

    public static bool IsAlreadyRetired(GameState state, string playerId)
    {
        return FindRetiredPlayer(state, playerId) != null;
    }

    public static List<PlayerData> GetRetirementCandidates(GameState state)
    {
        List<PlayerData> players = new List<PlayerData>();
        foreach (RetirementCandidate candidate in GetRetirementCandidateEntries(state))
        {
            if (candidate != null && candidate.Player != null)
            {
                players.Add(candidate.Player);
            }
        }

        return players;
    }

    private static List<RetirementCandidate> GetRetirementCandidateEntries(GameState state)
    {
        List<RetirementCandidate> candidates = new List<RetirementCandidate>();
        if (state == null)
        {
            return candidates;
        }

        if (state.Teams != null)
        {
            foreach (TeamData team in state.Teams)
            {
                if (team == null || team.Players == null)
                {
                    continue;
                }

                foreach (PlayerData player in team.Players)
                {
                    if (player != null && !player.IsRetired)
                    {
                        candidates.Add(new RetirementCandidate(team, player));
                    }
                }
            }
        }

        if (state.FreeAgentPool != null && state.FreeAgentPool.FreeAgents != null)
        {
            foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
            {
                if (player != null && !player.IsRetired)
                {
                    candidates.Add(new RetirementCandidate(null, player));
                }
            }
        }

        return candidates;
    }

    private static TeamData FindTeamByPlayer(GameState state, string playerId)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null || team.Players == null)
            {
                continue;
            }

            foreach (PlayerData player in team.Players)
            {
                if (player != null && player.Id == playerId)
                {
                    return team;
                }
            }
        }

        return null;
    }

    private static RetiredPlayerData FindRetiredPlayer(GameState state, string playerId)
    {
        if (state == null || state.RetiredPlayers == null || state.RetiredPlayers.Players == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (RetiredPlayerData retiredPlayer in state.RetiredPlayers.Players)
        {
            if (retiredPlayer != null && retiredPlayer.PlayerId == playerId)
            {
                return retiredPlayer;
            }
        }

        return null;
    }

    private static void CancelRetiredPlayerWaivers(GameState state, PlayerData player)
    {
        if (state == null || state.WaiverWire == null || player == null)
        {
            return;
        }

        state.WaiverWire.EnsureCollections();
        for (int i = state.WaiverWire.ActiveWaivers.Count - 1; i >= 0; i--)
        {
            WaiverPlayerData waiver = state.WaiverWire.ActiveWaivers[i];
            if (waiver == null || waiver.PlayerId != player.Id)
            {
                continue;
            }

            waiver.Status = WaiverConfig.WaiverWireStatusCancelled;
            waiver.Resolution = "Retired";
            waiver.ResolvedAtUtc = DateTime.UtcNow.ToString("o");
            state.WaiverWire.ActiveWaivers.RemoveAt(i);
            state.WaiverWire.WaiverHistory.Add(waiver);
        }
    }

    private static void RemoveDuplicateRetiredPlayers(GameState state)
    {
        if (state == null || state.RetiredPlayers == null || state.RetiredPlayers.Players == null)
        {
            return;
        }

        HashSet<string> seen = new HashSet<string>();
        for (int i = state.RetiredPlayers.Players.Count - 1; i >= 0; i--)
        {
            RetiredPlayerData retiredPlayer = state.RetiredPlayers.Players[i];
            if (retiredPlayer == null || string.IsNullOrEmpty(retiredPlayer.PlayerId) || !seen.Add(retiredPlayer.PlayerId))
            {
                state.RetiredPlayers.Players.RemoveAt(i);
            }
        }

        state.RetiredPlayers.TotalRetiredPlayers = state.RetiredPlayers.Players.Count;
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : (player.FirstName + " " + player.LastName).Trim();
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }

    private class RetirementCandidate
    {
        public TeamData Team;
        public PlayerData Player;

        public RetirementCandidate(TeamData team, PlayerData player)
        {
            Team = team;
            Player = player;
        }
    }
}
