using System;
using System.Collections.Generic;

public static class HallOfFameService
{
    public static void EnsureHallOfFame(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureRetirementHistory();
    }

    public static void ProcessHallOfFameForRetiredPlayers(GameState state)
    {
        EnsureHallOfFame(state);
        if (state == null || state.RetiredPlayers == null || state.RetiredPlayers.Players == null)
        {
            return;
        }

        foreach (RetiredPlayerData player in state.RetiredPlayers.Players)
        {
            if (player == null || IsAlreadyInducted(state, player.PlayerId))
            {
                continue;
            }

            player.HallOfFameScore = CalculateHallOfFameScore(player);
            player.IsHallOfFameEligible = player.HallOfFameScore >= RetirementConfig.HallOfFameScoreThreshold;
            if (ShouldInduct(player))
            {
                InductPlayer(state, player);
            }
        }
    }

    public static int CalculateHallOfFameScore(RetiredPlayerData player)
    {
        if (player == null)
        {
            return 0;
        }

        int score;
        if (player.Position == "G")
        {
            score = player.CareerWins / 5
                + player.CareerShutouts * 2
                + player.CareerGamesPlayed / 100;
        }
        else
        {
            score = player.CareerPoints / 10
                + player.CareerGoals / 8
                + player.CareerGamesPlayed / 100;
        }

        score += player.CareerAwardsCount * 12;
        score += player.ChampionshipsWon * 15;
        score += player.PlayoffRoundsWonCareer * 2;
        if (player.CareerGamesPlayed >= 1000)
        {
            score += 15;
        }

        return score;
    }

    public static bool ShouldInduct(RetiredPlayerData player)
    {
        return player != null && player.HallOfFameScore >= RetirementConfig.HallOfFameScoreThreshold;
    }

    public static HallOfFameInducteeData InductPlayer(GameState state, RetiredPlayerData player)
    {
        EnsureHallOfFame(state);
        if (state == null || player == null || IsAlreadyInducted(state, player.PlayerId))
        {
            return null;
        }

        int score = CalculateHallOfFameScore(player);
        player.HallOfFameScore = score;
        player.IsHallOfFameEligible = true;
        player.IsHallOfFameInducted = true;
        player.HallOfFameInductionYear = state.CurrentSeasonEndYear;

        HallOfFameInducteeData inductee = new HallOfFameInducteeData
        {
            InducteeId = "hof-" + player.PlayerId,
            PlayerId = player.PlayerId,
            PlayerName = player.PlayerName,
            Position = player.Position,
            JerseyNumber = player.JerseyNumber,
            PrimaryTeamId = player.PrimaryTeamId,
            PrimaryTeamName = player.PrimaryTeamName,
            InductionYear = state.CurrentSeasonEndYear,
            RetirementSeasonStartYear = player.RetirementSeasonStartYear,
            HallOfFameScore = score,
            CareerGamesPlayed = player.CareerGamesPlayed,
            CareerGoals = player.CareerGoals,
            CareerAssists = player.CareerAssists,
            CareerPoints = player.CareerPoints,
            CareerWins = player.CareerWins,
            CareerShutouts = player.CareerShutouts,
            CareerAwardsCount = player.CareerAwardsCount,
            ChampionshipsWon = player.ChampionshipsWon,
            InductionSummary = BuildInductionSummary(player, score),
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        state.HallOfFame.Inductees.Add(inductee);
        state.HallOfFame.TotalInductees = state.HallOfFame.Inductees.Count;
        state.HallOfFame.LastInductionAtUtc = inductee.CreatedAtUtc;
        EventNewsService.CreateHallOfFameNews(state, inductee);
        return inductee;
    }

    public static bool IsAlreadyInducted(GameState state, string playerId)
    {
        if (state == null || state.HallOfFame == null || state.HallOfFame.Inductees == null || string.IsNullOrEmpty(playerId))
        {
            return false;
        }

        foreach (HallOfFameInducteeData inductee in state.HallOfFame.Inductees)
        {
            if (inductee != null && inductee.PlayerId == playerId)
            {
                return true;
            }
        }

        return false;
    }

    public static string BuildInductionSummary(RetiredPlayerData player, int score)
    {
        if (player == null)
        {
            return "Hall of Fame induction";
        }

        return RetirementConfig.GetHallOfFameLabel(score)
            + " | " + player.CareerGamesPlayed + " GP"
            + " | " + player.CareerPoints + " P"
            + " | " + player.CareerWins + " W"
            + " | awards " + player.CareerAwardsCount
            + " | cups " + player.ChampionshipsWon;
    }
}
