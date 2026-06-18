using System;
using System.Collections.Generic;

public static class OwnerGoalEvaluationService
{
    public static OwnerSeasonEvaluationData EvaluateSeason(GameState state, TeamData team)
    {
        OwnerGoalService.EnsureOwnerProfile(state, team);
        OwnerGoalProgressService.UpdateGoalProgress(state, team);

        OwnerProfileData profile = team == null ? null : team.OwnerProfile;
        int trustBefore = profile == null ? OwnerGoalConfig.DefaultGmTrust : OwnerGoalConfig.ClampTrust(profile.GmTrust);
        int trustDelta = 0;
        int completed = 0;
        int failed = 0;
        int active = 0;
        List<OwnerGoalData> evaluatedGoals = new List<OwnerGoalData>();

        if (profile != null && profile.CurrentGoals != null)
        {
            foreach (OwnerGoalData goal in profile.CurrentGoals)
            {
                if (goal == null)
                {
                    continue;
                }

                bool success = goal.IsCompleted || goal.ProgressPercent >= 100;
                if (success)
                {
                    goal.IsCompleted = true;
                    goal.IsFailed = false;
                    goal.Status = OwnerGoalConfig.StatusCompleted;
                    goal.ResultSummary = "Completed";
                    trustDelta += goal.TrustImpactOnSuccess;
                    completed++;
                }
                else
                {
                    goal.IsCompleted = false;
                    goal.IsFailed = true;
                    goal.Status = OwnerGoalConfig.StatusFailed;
                    goal.ResultSummary = "Failed at " + goal.ProgressPercent + "%";
                    trustDelta += goal.TrustImpactOnFailure;
                    failed++;
                }

                goal.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
                evaluatedGoals.Add(CloneGoal(goal));
            }
        }

        active = Math.Max(0, evaluatedGoals.Count - completed - failed);
        int trustAfter = OwnerGoalConfig.ClampTrust(trustBefore + trustDelta);
        int satisfaction = CalculateOwnerSatisfaction(state, team, completed, failed);
        string jobSecurity = OwnerGoalConfig.GetJobSecurityLabel(trustAfter);

        OwnerSeasonEvaluationData evaluation = new OwnerSeasonEvaluationData
        {
            EvaluationId = Guid.NewGuid().ToString("N"),
            SeasonStartYear = state == null ? 0 : state.CurrentSeasonStartYear,
            SeasonEndYear = state == null ? 0 : state.CurrentSeasonEndYear,
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            TeamDirection = profile == null ? OwnerGoalGenerator.DetermineTeamDirection(state, team) : profile.TeamDirection,
            RegularSeasonPoints = OwnerGoalProgressService.GetRegularSeasonPoints(state, team),
            LeagueRank = OwnerGoalProgressService.GetLeagueRank(state, team),
            MadePlayoffs = OwnerGoalProgressService.DidTeamMakePlayoffs(state, team),
            PlayoffRoundsWon = OwnerGoalProgressService.GetPlayoffRoundsWon(state, team),
            PlayoffResult = BuildPlayoffResult(state, team),
            GoalsCompleted = completed,
            GoalsFailed = failed,
            GoalsActive = active,
            TrustBefore = trustBefore,
            TrustAfter = trustAfter,
            TrustDelta = trustAfter - trustBefore,
            OwnerSatisfaction = satisfaction,
            JobSecurity = jobSecurity,
            CreatedAtUtc = DateTime.UtcNow.ToString("o"),
            EvaluatedGoals = evaluatedGoals
        };

        evaluation.EvaluationSummary = BuildEvaluationSummary(evaluation);
        ApplySeasonEvaluation(state, team, evaluation);
        return evaluation;
    }

    public static void ApplySeasonEvaluation(GameState state, TeamData team, OwnerSeasonEvaluationData evaluation)
    {
        if (team == null || evaluation == null)
        {
            return;
        }

        OwnerGoalService.EnsureOwnerProfile(state, team);
        team.OwnerProfile.GmTrust = evaluation.TrustAfter;
        team.OwnerProfile.OwnerSatisfaction = evaluation.OwnerSatisfaction;
        team.OwnerProfile.JobSecurity = evaluation.JobSecurity;
        team.OwnerProfile.LastSeasonEvaluation = evaluation;
        team.OwnerProfile.UpdatedAtUtc = DateTime.UtcNow.ToString("o");

        StoreEvaluation(state, team, evaluation);
        if (state != null && team.Id == state.SelectedTeamId)
        {
            JobSecurityEvaluationService.EvaluateJobSecurityAfterSeason(state, team);
        }
    }

    public static int CalculateOwnerSatisfaction(GameState state, TeamData team, int goalsCompleted, int goalsFailed)
    {
        int satisfaction = 50;
        satisfaction += goalsCompleted * 8;
        satisfaction -= goalsFailed * 7;

        if (OwnerGoalProgressService.DidTeamMakePlayoffs(state, team))
        {
            satisfaction += 8;
        }

        satisfaction += OwnerGoalProgressService.GetPlayoffRoundsWon(state, team) * 5;

        ClubFinanceData finances = ClubFinanceService.CalculateClubFinances(state, team);
        if (finances != null)
        {
            satisfaction += (finances.FinancialHealthScore - 50) / 5;
        }

        satisfaction += (OwnerGoalProgressService.CalculateTeamMoraleScore(state, team) - 50) / 10;
        return Clamp(satisfaction, 0, 100);
    }

    public static string BuildEvaluationSummary(OwnerSeasonEvaluationData evaluation)
    {
        if (evaluation == null)
        {
            return "Оценка сезона недоступна";
        }

        string trustDelta = evaluation.TrustDelta >= 0 ? "+" + evaluation.TrustDelta : evaluation.TrustDelta.ToString();
        return evaluation.TeamName + ": goals " + evaluation.GoalsCompleted + "/" + (evaluation.GoalsCompleted + evaluation.GoalsFailed)
            + ", points " + evaluation.RegularSeasonPoints
            + ", playoffs " + (evaluation.MadePlayoffs ? evaluation.PlayoffResult : "missed")
            + ", trust " + evaluation.TrustBefore + " -> " + evaluation.TrustAfter
            + " (" + trustDelta + "), job security " + evaluation.JobSecurity;
    }

    public static void StoreEvaluation(GameState state, TeamData team, OwnerSeasonEvaluationData evaluation)
    {
        if (evaluation == null)
        {
            return;
        }

        if (team != null && team.OwnerProfile != null)
        {
            team.OwnerProfile.EnsureCollections();
            RemoveDuplicateEvaluation(team.OwnerProfile.EvaluationHistory, evaluation);
            team.OwnerProfile.EvaluationHistory.Add(evaluation);
            TrimEvaluationHistory(team.OwnerProfile.EvaluationHistory, OwnerGoalConfig.MaxEvaluationHistoryToKeep);
        }

        if (state != null)
        {
            state.EnsureOwnerEvaluationHistory();
            RemoveDuplicateEvaluation(state.OwnerEvaluationHistory, evaluation);
            state.OwnerEvaluationHistory.Add(evaluation);
            TrimEvaluationHistory(state.OwnerEvaluationHistory, OwnerGoalConfig.MaxGlobalEvaluationHistoryToKeep);
        }
    }

    public static void TrimEvaluationHistory(List<OwnerSeasonEvaluationData> history, int maxRecords)
    {
        if (history == null)
        {
            return;
        }

        while (history.Count > maxRecords)
        {
            history.RemoveAt(0);
        }
    }

    private static OwnerGoalData CloneGoal(OwnerGoalData goal)
    {
        return new OwnerGoalData
        {
            GoalId = goal.GoalId,
            GoalType = goal.GoalType,
            Title = goal.Title,
            Description = goal.Description,
            TargetValueLabel = goal.TargetValueLabel,
            TargetValue = goal.TargetValue,
            CurrentValue = goal.CurrentValue,
            ProgressPercent = goal.ProgressPercent,
            IsCompleted = goal.IsCompleted,
            IsFailed = goal.IsFailed,
            TrustImpactOnSuccess = goal.TrustImpactOnSuccess,
            TrustImpactOnFailure = goal.TrustImpactOnFailure,
            Status = goal.Status,
            ResultSummary = goal.ResultSummary,
            CreatedAtUtc = goal.CreatedAtUtc,
            UpdatedAtUtc = goal.UpdatedAtUtc
        };
    }

    private static string BuildPlayoffResult(GameState state, TeamData team)
    {
        if (state == null || state.Season == null || state.Season.Playoffs == null || team == null)
        {
            return "Not started";
        }

        if (state.Season.Playoffs.ChampionTeamId == team.Id)
        {
            return "Stanley Cup Champion";
        }

        int roundsWon = OwnerGoalProgressService.GetPlayoffRoundsWon(state, team);
        if (roundsWon <= 0)
        {
            return OwnerGoalProgressService.DidTeamMakePlayoffs(state, team) ? "Lost round 1" : "Missed playoffs";
        }

        return "Won " + roundsWon + " playoff round(s)";
    }

    private static void RemoveDuplicateEvaluation(List<OwnerSeasonEvaluationData> history, OwnerSeasonEvaluationData evaluation)
    {
        if (history == null || evaluation == null)
        {
            return;
        }

        for (int i = history.Count - 1; i >= 0; i--)
        {
            OwnerSeasonEvaluationData existing = history[i];
            if (existing != null
                && existing.TeamId == evaluation.TeamId
                && existing.SeasonStartYear == evaluation.SeasonStartYear
                && existing.SeasonEndYear == evaluation.SeasonEndYear)
            {
                history.RemoveAt(i);
            }
        }
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
}
