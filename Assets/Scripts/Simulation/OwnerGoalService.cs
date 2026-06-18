using System;
using System.Collections.Generic;

public static class OwnerGoalService
{
    public static void EnsureOwnerProfiles(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureOwnerEvaluationHistory();
        if (state.Teams == null)
        {
            return;
        }

        foreach (TeamData team in state.Teams)
        {
            EnsureOwnerProfile(state, team);
        }

        state.LastOwnerGoalsUpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    public static OwnerProfileData EnsureOwnerProfile(GameState state, TeamData team)
    {
        if (team == null)
        {
            return null;
        }

        if (team.OwnerProfile == null)
        {
            team.OwnerProfile = BuildOwnerProfile(state, team);
        }

        team.OwnerProfile.EnsureCollections();
        team.OwnerProfile.TeamId = team.Id;
        team.OwnerProfile.TeamName = GetTeamName(team);

        if (team.OwnerProfile.GmTrust == 0 && string.IsNullOrEmpty(team.OwnerProfile.JobSecurity))
        {
            team.OwnerProfile.GmTrust = OwnerGoalConfig.DefaultGmTrust;
        }

        if (team.OwnerProfile.OwnerSatisfaction == 0
            && team.OwnerProfile.LastSeasonEvaluation == null
            && (team.OwnerProfile.CurrentGoals == null || team.OwnerProfile.CurrentGoals.Count == 0))
        {
            team.OwnerProfile.OwnerSatisfaction = OwnerGoalConfig.DefaultGmTrust;
        }

        team.OwnerProfile.GmTrust = OwnerGoalConfig.ClampTrust(team.OwnerProfile.GmTrust);
        team.OwnerProfile.OwnerSatisfaction = OwnerGoalConfig.ClampTrust(team.OwnerProfile.OwnerSatisfaction);
        team.OwnerProfile.JobSecurity = OwnerGoalConfig.GetJobSecurityLabel(team.OwnerProfile.GmTrust);
        team.OwnerProfile.TeamDirection = OwnerGoalGenerator.DetermineTeamDirection(state, team);
        EnsureSeasonGoals(state, team);
        team.OwnerProfile.Finances = ClubFinanceService.CalculateClubFinances(state, team);
        team.OwnerProfile.Finances.FinanceSummary = ClubFinanceService.BuildFinanceSummary(team.OwnerProfile.Finances);
        OwnerGoalProgressService.UpdateGoalProgress(state, team);
        team.OwnerProfile.ExpectationsSummary = BuildExpectationsSummary(team.OwnerProfile);
        team.OwnerProfile.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
        return team.OwnerProfile;
    }

    public static OwnerProfileData BuildOwnerProfile(GameState state, TeamData team)
    {
        string direction = OwnerGoalGenerator.DetermineTeamDirection(state, team);
        OwnerProfileData profile = new OwnerProfileData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            GmTrust = OwnerGoalConfig.DefaultGmTrust,
            OwnerSatisfaction = OwnerGoalConfig.DefaultGmTrust,
            JobSecurity = OwnerGoalConfig.GetJobSecurityLabel(OwnerGoalConfig.DefaultGmTrust),
            TeamDirection = direction,
            ExpectationsSummary = "",
            CurrentGoals = new List<OwnerGoalData>(),
            EvaluationHistory = new List<OwnerSeasonEvaluationData>(),
            Finances = ClubFinanceService.CalculateClubFinances(state, team),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        profile.Finances.FinanceSummary = ClubFinanceService.BuildFinanceSummary(profile.Finances);
        profile.ExpectationsSummary = BuildExpectationsSummary(profile);
        return profile;
    }

    public static void EnsureSeasonGoals(GameState state, TeamData team)
    {
        if (team == null || team.OwnerProfile == null)
        {
            return;
        }

        team.OwnerProfile.EnsureCollections();
        if (team.OwnerProfile.CurrentGoals.Count == 0)
        {
            team.OwnerProfile.CurrentGoals = OwnerGoalGenerator.GenerateSeasonGoals(state, team);
        }
    }

    public static void EnsureSeasonGoalsForTeams(GameState state, List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureOwnerProfile(state, team);
        }
    }

    public static OwnerProfileData GetOwnerProfile(GameState state, TeamData team)
    {
        return EnsureOwnerProfile(state, team);
    }

    public static ClubFinanceData GetClubFinances(GameState state, TeamData team)
    {
        OwnerProfileData profile = EnsureOwnerProfile(state, team);
        return profile == null ? ClubFinanceService.CalculateClubFinances(state, team) : profile.Finances;
    }

    public static void UpdateOwnerGoalProgress(GameState state, TeamData team)
    {
        EnsureOwnerProfile(state, team);
        OwnerGoalProgressService.UpdateGoalProgress(state, team);
        if (team != null && team.OwnerProfile != null)
        {
            team.OwnerProfile.Finances = ClubFinanceService.CalculateClubFinances(state, team);
            team.OwnerProfile.Finances.FinanceSummary = ClubFinanceService.BuildFinanceSummary(team.OwnerProfile.Finances);
            team.OwnerProfile.ExpectationsSummary = BuildExpectationsSummary(team.OwnerProfile);
            team.OwnerProfile.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
        }

        if (state != null)
        {
            state.LastOwnerGoalsUpdatedAtUtc = DateTime.UtcNow.ToString("o");
        }
    }

    public static OwnerSeasonEvaluationData EvaluateCurrentTeamSeason(GameState state)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(state.SelectedTeamId))
        {
            return null;
        }

        TeamData team = FindTeam(state, state.SelectedTeamId);
        if (team == null)
        {
            return null;
        }

        OwnerProfileData profile = EnsureOwnerProfile(state, team);
        if (profile != null
            && profile.LastSeasonEvaluation != null
            && profile.LastSeasonEvaluation.SeasonStartYear == state.CurrentSeasonStartYear
            && profile.LastSeasonEvaluation.SeasonEndYear == state.CurrentSeasonEndYear)
        {
            return profile.LastSeasonEvaluation;
        }

        return OwnerGoalEvaluationService.EvaluateSeason(state, team);
    }

    public static void PrepareNewSeasonGoals(GameState state)
    {
        if (state == null || state.Teams == null)
        {
            return;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            OwnerProfileData profile = EnsureOwnerProfile(state, team);
            if (profile != null)
            {
                profile.CurrentGoals = new List<OwnerGoalData>();
                profile.TeamDirection = OwnerGoalGenerator.DetermineTeamDirection(state, team);
                profile.CurrentGoals = OwnerGoalGenerator.GenerateSeasonGoals(state, team);
                profile.Finances = ClubFinanceService.CalculateClubFinances(state, team);
                profile.Finances.FinanceSummary = ClubFinanceService.BuildFinanceSummary(profile.Finances);
                profile.ExpectationsSummary = BuildExpectationsSummary(profile);
                profile.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
            }
        }

        state.LastOwnerGoalsUpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    public static string BuildExpectationsSummary(OwnerProfileData profile)
    {
        if (profile == null)
        {
            return "Owner expectations are not available.";
        }

        string primaryGoal = "none";
        if (profile.CurrentGoals != null)
        {
            foreach (OwnerGoalData goal in profile.CurrentGoals)
            {
                if (goal != null && goal.GoalType == OwnerGoalConfig.GoalTypePrimary)
                {
                    primaryGoal = goal.Title;
                    break;
                }
            }
        }

        return "Direction: " + profile.TeamDirection
            + " | Primary goal: " + primaryGoal
            + " | GM trust: " + profile.GmTrust
            + " | Job security: " + profile.JobSecurity;
    }

    private static TeamData FindTeam(GameState state, string teamId)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
