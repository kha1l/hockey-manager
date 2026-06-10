using System;
using UnityEngine;

public static class MatchSimulator
{
    public static MatchResultData Simulate(TeamData homeTeam, TeamData awayTeam)
    {
        LineupService.EnsureLineup(homeTeam);
        LineupService.EnsureLineup(awayTeam);
        SpecialTeamsService.EnsureSpecialTeams(homeTeam);
        SpecialTeamsService.EnsureSpecialTeams(awayTeam);
        TacticsService.EnsureTactics(homeTeam);
        TacticsService.EnsureTactics(awayTeam);
        PlayerFatigueService.EnsureFatigueForTeam(homeTeam);
        PlayerFatigueService.EnsureFatigueForTeam(awayTeam);
        InjuryService.EnsureInjuryFieldsForTeam(homeTeam);
        InjuryService.EnsureInjuryFieldsForTeam(awayTeam);
        PlayerRoleService.EnsureRolesForTeam(homeTeam);
        PlayerRoleService.EnsureRolesForTeam(awayTeam);
        IceTimeService.EnsureUsageForTeam(homeTeam);
        IceTimeService.EnsureUsageForTeam(awayTeam);

        int homeRating = TeamRatingCalculator.CalculateEffectiveLineupOverall(homeTeam) + 3;
        int awayRating = TeamRatingCalculator.CalculateEffectiveLineupOverall(awayTeam);
        int awayDefenseAdjustedRating = AdjustOpponentRating(awayRating, TacticsService.GetDefenseModifier(awayTeam));
        int homeDefenseAdjustedRating = AdjustOpponentRating(homeRating, TacticsService.GetDefenseModifier(homeTeam));

        int homeScore = GenerateGoals(homeRating, awayDefenseAdjustedRating, TacticsService.GetGoalModifier(homeTeam), TacticsService.GetRiskModifier(homeTeam));
        int awayScore = GenerateGoals(awayRating, homeDefenseAdjustedRating, TacticsService.GetGoalModifier(awayTeam), TacticsService.GetRiskModifier(awayTeam));
        bool isOvertime = false;

        if (homeScore == awayScore)
        {
            isOvertime = true;
            bool homeWinsOvertime = UnityEngine.Random.value < GetWinChance(homeRating, awayRating);

            if (homeWinsOvertime)
            {
                homeScore++;
            }
            else
            {
                awayScore++;
            }
        }

        int homeShots = GenerateShots(homeRating, awayRating, TacticsService.GetShotModifier(homeTeam));
        int awayShots = GenerateShots(awayRating, homeRating, TacticsService.GetShotModifier(awayTeam));
        int homePenaltyMinutes = GeneratePenaltyMinutes(TacticsService.GetPenaltyModifier(homeTeam));
        int awayPenaltyMinutes = GeneratePenaltyMinutes(TacticsService.GetPenaltyModifier(awayTeam));
        int homePowerPlayOpportunities = Mathf.Clamp(awayPenaltyMinutes / 2, 0, 7);
        int awayPowerPlayOpportunities = Mathf.Clamp(homePenaltyMinutes / 2, 0, 7);
        int homePowerPlayGoals = GeneratePowerPlayGoals(
            homePowerPlayOpportunities,
            SpecialTeamsService.CalculatePowerPlayRating(homeTeam),
            SpecialTeamsService.CalculatePenaltyKillRating(awayTeam),
            homeScore);
        int awayPowerPlayGoals = GeneratePowerPlayGoals(
            awayPowerPlayOpportunities,
            SpecialTeamsService.CalculatePowerPlayRating(awayTeam),
            SpecialTeamsService.CalculatePenaltyKillRating(homeTeam),
            awayScore);
        string homeTeamName = GetTeamDisplayName(homeTeam);
        string awayTeamName = GetTeamDisplayName(awayTeam);
        string homeTeamId = homeTeam == null ? "" : homeTeam.Id;
        string awayTeamId = awayTeam == null ? "" : awayTeam.Id;
        string winnerTeamId = homeScore > awayScore ? homeTeamId : awayTeamId;
        string summary = homeTeamName + " " + homeScore + " - " + awayScore + " " + awayTeamName;

        if (isOvertime)
        {
            summary += " OT";
        }

        return new MatchResultData
        {
            MatchId = Guid.NewGuid().ToString("N"),
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            HomeTeamName = homeTeamName,
            AwayTeamName = awayTeamName,
            HomeScore = homeScore,
            AwayScore = awayScore,
            HomeShots = homeShots,
            AwayShots = awayShots,
            HomePowerPlayOpportunities = homePowerPlayOpportunities,
            AwayPowerPlayOpportunities = awayPowerPlayOpportunities,
            HomePowerPlayGoals = homePowerPlayGoals,
            AwayPowerPlayGoals = awayPowerPlayGoals,
            HomePenaltyMinutes = homePenaltyMinutes,
            AwayPenaltyMinutes = awayPenaltyMinutes,
            WinnerTeamId = winnerTeamId,
            IsOvertime = isOvertime,
            PlayedAtUtc = DateTime.UtcNow.ToString("o"),
            Summary = summary
        };
    }

    private static int GenerateGoals(int teamRating, int opponentRating, float goalModifier, float riskModifier)
    {
        float expectedGoals = 2.7f + ((teamRating - opponentRating) * 0.055f);
        expectedGoals *= goalModifier;
        expectedGoals += UnityEngine.Random.Range(-0.65f, 0.85f) * riskModifier;
        expectedGoals = Mathf.Clamp(expectedGoals, 1.1f, 4.7f);

        int goals = Mathf.FloorToInt(expectedGoals);
        float fraction = expectedGoals - goals;

        if (UnityEngine.Random.value < fraction)
        {
            goals++;
        }

        if (UnityEngine.Random.value < 0.24f)
        {
            goals++;
        }

        if (UnityEngine.Random.value < 0.09f)
        {
            goals++;
        }

        if (UnityEngine.Random.value < 0.12f)
        {
            goals--;
        }

        return Mathf.Clamp(goals, 0, 8);
    }

    private static int GenerateShots(int teamRating, int opponentRating, float shotModifier)
    {
        float expectedShots = 30f + ((teamRating - opponentRating) * 0.18f);
        expectedShots *= shotModifier;
        expectedShots += UnityEngine.Random.Range(-5f, 6f);
        return Mathf.Clamp(Mathf.RoundToInt(expectedShots), 20, 45);
    }

    private static int GeneratePenaltyMinutes(float penaltyModifier)
    {
        float expectedMinutes = UnityEngine.Random.Range(4f, 11f) * penaltyModifier;
        int minutes = Mathf.RoundToInt(expectedMinutes / 2f) * 2;
        return Mathf.Clamp(minutes, 0, 14);
    }

    private static int GeneratePowerPlayGoals(int opportunities, int powerPlayRating, int penaltyKillRating, int maxGoals)
    {
        int goals = 0;
        float chance = 0.18f + ((powerPlayRating - penaltyKillRating) * 0.004f);
        chance = Mathf.Clamp(chance, 0.10f, 0.30f);

        for (int i = 0; i < opportunities; i++)
        {
            if (UnityEngine.Random.value < chance)
            {
                goals++;
            }
        }

        return Mathf.Clamp(goals, 0, Mathf.Max(0, maxGoals));
    }

    private static int AdjustOpponentRating(int opponentRating, float defenseModifier)
    {
        return Mathf.Clamp(Mathf.RoundToInt(opponentRating + ((defenseModifier - 1f) * 60f)), 50, 99);
    }

    private static float GetWinChance(int teamRating, int opponentRating)
    {
        return Mathf.Clamp01(0.5f + ((teamRating - opponentRating) * 0.015f));
    }

    private static string GetTeamDisplayName(TeamData team)
    {
        if (team == null)
        {
            return "Unknown Team";
        }

        return team.City + " " + team.Name;
    }
}
