using System;
using System.Collections.Generic;

public static class RetiredNumberService
{
    public static void EnsureRetiredNumbers(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureRetirementHistory();
        DeduplicateLeagueRetiredNumbers(state);
    }

    public static void ProcessRetiredNumbersAfterSeason(GameState state)
    {
        EnsureRetiredNumbers(state);
        if (state == null || state.RetiredPlayers == null || state.RetiredPlayers.Players == null)
        {
            return;
        }

        foreach (RetiredPlayerData player in state.RetiredPlayers.Players)
        {
            if (player == null || player.HasRetiredNumber)
            {
                continue;
            }

            TeamData team = FindPrimaryTeam(state, player);
            if (ShouldRetireNumber(state, team, player))
            {
                RetireNumber(state, team, player, BuildRetiredNumberReason(player, CalculateRetiredNumberScore(state, team, player)));
            }
        }
    }

    public static int CalculateRetiredNumberScore(GameState state, TeamData team, RetiredPlayerData player)
    {
        if (team == null || player == null || player.PrimaryTeamId != team.Id)
        {
            return 0;
        }

        int score = 0;
        if (player.SeasonsWithPrimaryTeam >= 10)
        {
            score += 35;
        }
        else if (player.SeasonsWithPrimaryTeam >= 7)
        {
            score += 25;
        }
        else if (player.SeasonsWithPrimaryTeam >= 5)
        {
            score += 15;
        }

        if (player.IsHallOfFameInducted)
        {
            score += 35;
        }

        if (player.ChampionshipsWon > 0)
        {
            score += 15;
        }

        score += Math.Min(30, player.CareerAwardsCount * 10);
        if (player.CareerPoints >= 700 || player.CareerWins >= 300)
        {
            score += 15;
        }

        if (player.JerseyNumber <= 0)
        {
            score -= 100;
        }

        return score;
    }

    public static bool ShouldRetireNumber(GameState state, TeamData team, RetiredPlayerData player)
    {
        if (team == null || player == null || player.PrimaryTeamId != team.Id || player.JerseyNumber <= 0)
        {
            return false;
        }

        return !IsNumberAlreadyRetired(team, player.JerseyNumber)
            && CalculateRetiredNumberScore(state, team, player) >= RetirementConfig.RetiredNumberScoreThreshold;
    }

    public static RetiredNumberData RetireNumber(GameState state, TeamData team, RetiredPlayerData player, string reason)
    {
        EnsureRetiredNumbers(state);
        if (state == null || team == null || player == null || IsNumberAlreadyRetired(team, player.JerseyNumber))
        {
            return null;
        }

        string id = "retired-number-" + team.Id + "-" + player.JerseyNumber;
        RetiredNumberData retiredNumber = new RetiredNumberData
        {
            RetiredNumberId = id,
            TeamId = team.Id,
            TeamName = GetTeamName(team),
            PlayerId = player.PlayerId,
            PlayerName = player.PlayerName,
            JerseyNumber = player.JerseyNumber,
            RetirementSeasonStartYear = player.RetirementSeasonStartYear,
            RetirementSeasonEndYear = player.RetirementSeasonEndYear,
            RetiredNumberYear = state.CurrentSeasonEndYear,
            RetiredNumberScore = CalculateRetiredNumberScore(state, team, player),
            Reason = string.IsNullOrEmpty(reason) ? BuildRetiredNumberReason(player, CalculateRetiredNumberScore(state, team, player)) : reason,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        team.EnsureRetiredNumbersData();
        team.RetiredNumbersData.RetiredNumbers.Add(retiredNumber);
        team.RetiredNumbersData.UpdatedAtUtc = retiredNumber.CreatedAtUtc;
        if (!LeagueNumberExists(state, retiredNumber.TeamId, retiredNumber.JerseyNumber))
        {
            state.LeagueRetiredNumbers.Add(retiredNumber);
        }

        player.HasRetiredNumber = true;
        player.RetiredNumberTeamId = team.Id;
        player.RetiredNumberTeamName = GetTeamName(team);
        EventNewsService.CreateRetiredNumberNews(state, retiredNumber);
        return retiredNumber;
    }

    public static bool IsNumberAlreadyRetired(TeamData team, int jerseyNumber)
    {
        if (team == null || jerseyNumber <= 0)
        {
            return false;
        }

        team.EnsureRetiredNumbersData();
        foreach (RetiredNumberData retiredNumber in team.RetiredNumbersData.RetiredNumbers)
        {
            if (retiredNumber != null && retiredNumber.JerseyNumber == jerseyNumber)
            {
                return true;
            }
        }

        return false;
    }

    public static TeamData FindPrimaryTeam(GameState state, RetiredPlayerData player)
    {
        if (state == null || state.Teams == null || player == null || string.IsNullOrEmpty(player.PrimaryTeamId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == player.PrimaryTeamId)
            {
                return team;
            }
        }

        return null;
    }

    public static string BuildRetiredNumberReason(RetiredPlayerData player, int score)
    {
        if (player == null)
        {
            return "Legendary career";
        }

        return "Retired number after " + RetirementConfig.GetHallOfFameLabel(player.HallOfFameScore)
            + " career, score " + score
            + ", " + player.SeasonsWithPrimaryTeam + " seasons with club";
    }

    private static void DeduplicateLeagueRetiredNumbers(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureRetirementHistory();
        HashSet<string> seen = new HashSet<string>();
        for (int i = state.LeagueRetiredNumbers.Count - 1; i >= 0; i--)
        {
            RetiredNumberData retiredNumber = state.LeagueRetiredNumbers[i];
            string key = retiredNumber == null ? "" : retiredNumber.TeamId + "|" + retiredNumber.JerseyNumber;
            if (retiredNumber == null || string.IsNullOrEmpty(key) || !seen.Add(key))
            {
                state.LeagueRetiredNumbers.RemoveAt(i);
            }
        }
    }

    private static bool LeagueNumberExists(GameState state, string teamId, int jerseyNumber)
    {
        if (state == null || state.LeagueRetiredNumbers == null)
        {
            return false;
        }

        foreach (RetiredNumberData retiredNumber in state.LeagueRetiredNumbers)
        {
            if (retiredNumber != null && retiredNumber.TeamId == teamId && retiredNumber.JerseyNumber == jerseyNumber)
            {
                return true;
            }
        }

        return false;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
