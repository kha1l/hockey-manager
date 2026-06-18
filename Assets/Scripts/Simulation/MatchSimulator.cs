using System;
using UnityEngine;

public static class MatchSimulator
{
    public static MatchResultData Simulate(TeamData homeTeam, TeamData awayTeam)
    {
        TeamRosterService.EnsureRosterStatusesForTeam(homeTeam);
        TeamRosterService.EnsureRosterStatusesForTeam(awayTeam);
        CoachingStaffService.EnsureStaffForTeam(homeTeam);
        CoachingStaffService.EnsureStaffForTeam(awayTeam);
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
        ChemistryService.EnsureChemistryForTeam(homeTeam);
        ChemistryService.EnsureChemistryForTeam(awayTeam);

        return SimulatePrepared(homeTeam, awayTeam);
    }

    public static MatchResultData SimulateFast(TeamData homeTeam, TeamData awayTeam)
    {
        return SimulatePrepared(homeTeam, awayTeam);
    }

    private static MatchResultData SimulatePrepared(TeamData homeTeam, TeamData awayTeam)
    {
        int homeRating = TeamRatingCalculator.CalculateEffectiveLineupOverall(homeTeam) + 4;
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

        ApplyLateGameScoreVariance(ref homeScore, ref awayScore, homeRating, awayRating, isOvertime);

        int homeShots = GenerateShots(homeRating, awayRating, TacticsService.GetShotModifier(homeTeam));
        int awayShots = GenerateShots(awayRating, homeRating, TacticsService.GetShotModifier(awayTeam));
        int homePenaltyMinutes = GeneratePenaltyMinutes(GetCoachingAdjustedPenaltyModifier(homeTeam));
        int awayPenaltyMinutes = GeneratePenaltyMinutes(GetCoachingAdjustedPenaltyModifier(awayTeam));
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
        // Alpha tuning note: keep these scoring constants report-driven. Use AlphaBalanceReport before changing conversion.
        float expectedGoals = 2.75f + ((teamRating - opponentRating) * 0.070f);
        expectedGoals *= goalModifier;
        expectedGoals += UnityEngine.Random.Range(-1.15f, 1.25f) * riskModifier;
        expectedGoals = Mathf.Clamp(expectedGoals, 0.7f, 5.5f);

        int goals = Mathf.FloorToInt(expectedGoals);
        float fraction = expectedGoals - goals;

        if (UnityEngine.Random.value < fraction)
        {
            goals++;
        }

        if (UnityEngine.Random.value < 0.20f)
        {
            goals++;
        }

        if (UnityEngine.Random.value < 0.11f)
        {
            goals++;
        }

        if (UnityEngine.Random.value < 0.14f)
        {
            goals--;
        }

        return Mathf.Clamp(goals, 0, 8);
    }

    private static void ApplyLateGameScoreVariance(ref int homeScore, ref int awayScore, int homeRating, int awayRating, bool isOvertime)
    {
        if (isOvertime || homeScore == awayScore)
        {
            return;
        }

        int margin = Mathf.Abs(homeScore - awayScore);
        if (margin == 1 && UnityEngine.Random.value < 0.30f)
        {
            if (homeScore > awayScore)
            {
                homeScore++;
            }
            else
            {
                awayScore++;
            }

            margin++;
        }

        int ratingGap = Mathf.Abs(homeRating - awayRating);
        float extraGoalChance = Mathf.Clamp(0.06f + (ratingGap * 0.008f), 0.06f, 0.18f);
        if (margin >= 2 && UnityEngine.Random.value < extraGoalChance)
        {
            bool homeIsFavorite = homeRating >= awayRating;
            if ((homeScore > awayScore && homeIsFavorite) || (homeScore > awayScore && UnityEngine.Random.value < 0.35f))
            {
                homeScore++;
            }
            else if (awayScore > homeScore)
            {
                awayScore++;
            }
        }
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

    private static float GetCoachingAdjustedPenaltyModifier(TeamData team)
    {
        float modifier = TacticsService.GetPenaltyModifier(team);
        int disciplineModifier = CoachingStaffService.GetDisciplineModifier(team);
        modifier -= disciplineModifier * 0.05f;

        if (team != null
            && team.Tactics != null
            && team.Tactics.PresetName == "Aggressive"
            && disciplineModifier < 0)
        {
            modifier += 0.08f;
        }

        return Mathf.Clamp(modifier, 0.75f, 1.40f);
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

        return TeamIdentityService.GetDisplayName(team);
    }
}
