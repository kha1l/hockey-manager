using System;

public static class JobSecurityEvaluationService
{
    public static void EvaluateJobSecurityAfterSeason(GameState state, TeamData userTeam)
    {
        GmCareerService.EnsureGmCareer(state);
        if (state == null || state.GmCareer == null || userTeam == null)
        {
            return;
        }

        if (state.GmCareer.LastJobSecurityEvaluationSeasonStartYear == state.CurrentSeasonStartYear)
        {
            return;
        }

        OwnerProfileData owner = userTeam.OwnerProfile;
        OwnerSeasonEvaluationData evaluation = owner == null ? null : owner.LastSeasonEvaluation;
        if (evaluation == null)
        {
            return;
        }

        int securityBefore = state.GmCareer.CurrentJobSecurity;
        int trustBefore = state.GmCareer.CurrentOwnerTrust;
        int delta = CalculateJobSecurityDelta(state, userTeam, evaluation);
        int securityAfter = GmJobSecurityConfig.ClampSecurity(securityBefore + delta);

        state.GmCareer.CurrentJobSecurity = securityAfter;
        state.GmCareer.CurrentOwnerTrust = owner == null ? evaluation.TrustAfter : owner.GmTrust;
        state.GmCareer.CareerStatus = GmJobSecurityConfig.GetCareerStatusBySecurity(securityAfter);
        state.GmCareer.LastJobSecurityEvaluationSeasonStartYear = state.CurrentSeasonStartYear;
        state.GmCareer.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
        state.LastGmCareerUpdateUtc = state.GmCareer.UpdatedAtUtc;

        GmCareerEventData reviewEvent = GmCareerService.CreateCareerEvent(
            state,
            "SeasonReview",
            userTeam.Id,
            GetTeamName(userTeam),
            "GM season review",
            BuildJobSecuritySummary(state, userTeam, evaluation, delta),
            trustBefore,
            state.GmCareer.CurrentOwnerTrust,
            securityBefore,
            securityAfter);
        GmCareerService.AddCareerEvent(state, reviewEvent);

        if (ShouldFireGm(state, userTeam, evaluation))
        {
            ApplyFiring(state, userTeam, evaluation);
        }
        else if (ShouldIssueWarning(state, userTeam, evaluation))
        {
            ApplyWarning(state, userTeam, evaluation);
        }

        UpdateCareerTotalsAfterSeason(state, userTeam);
    }

    public static int CalculateJobSecurityDelta(GameState state, TeamData team, OwnerSeasonEvaluationData ownerEvaluation)
    {
        if (ownerEvaluation == null)
        {
            return 0;
        }

        int delta = ownerEvaluation.TrustDelta / 2;

        if (ownerEvaluation.GoalsFailed > ownerEvaluation.GoalsCompleted)
        {
            delta -= ownerEvaluation.GoalsFailed >= 2 ? 18 : 10;
        }
        else if (ownerEvaluation.GoalsCompleted > ownerEvaluation.GoalsFailed)
        {
            delta += ownerEvaluation.GoalsCompleted >= 2 ? 10 : 5;
        }

        string direction = ownerEvaluation.TeamDirection;
        if (!ownerEvaluation.MadePlayoffs
            && (direction == TradeAiConfig.DirectionContender || direction == TradeAiConfig.DirectionPlayoffTeam))
        {
            delta -= 15;
        }

        if (direction == TradeAiConfig.DirectionRebuild && ownerEvaluation.GoalsCompleted > 0)
        {
            delta += 6;
        }

        if (state != null && state.Season != null && state.Season.Playoffs != null && team != null
            && state.Season.Playoffs.ChampionTeamId == team.Id)
        {
            delta += 25;
        }
        else if (ownerEvaluation.MadePlayoffs)
        {
            delta += direction == TradeAiConfig.DirectionBubbleTeam || direction == TradeAiConfig.DirectionRebuild ? 10 : 5;
        }

        if (ownerEvaluation.OwnerSatisfaction < 35)
        {
            delta -= 5;
        }

        return Clamp(delta, -35, 30);
    }

    public static bool ShouldFireGm(GameState state, TeamData team, OwnerSeasonEvaluationData ownerEvaluation)
    {
        if (state == null || state.GmCareer == null || ownerEvaluation == null)
        {
            return false;
        }

        if (state.GmCareer.SeasonsCompleted < 1)
        {
            return false;
        }

        bool primaryFailure = ownerEvaluation.GoalsFailed > ownerEvaluation.GoalsCompleted;
        return (state.GmCareer.CurrentJobSecurity <= GmJobSecurityConfig.FiringThreshold && primaryFailure)
            || state.GmCareer.CurrentOwnerTrust < 15;
    }

    public static bool ShouldIssueWarning(GameState state, TeamData team, OwnerSeasonEvaluationData ownerEvaluation)
    {
        if (state == null || state.GmCareer == null || state.GmCareer.IsFired)
        {
            return false;
        }

        return state.GmCareer.CurrentJobSecurity < GmJobSecurityConfig.DangerThreshold
            || state.GmCareer.CurrentOwnerTrust < 30;
    }

    public static void ApplyFiring(GameState state, TeamData team, OwnerSeasonEvaluationData ownerEvaluation)
    {
        if (state == null || state.GmCareer == null || team == null)
        {
            return;
        }

        state.GmCareer.IsFired = true;
        state.GmCareer.IsUnemployed = true;
        state.GmCareer.CareerStatus = "Fired";
        state.GmCareer.FiredFromTeamId = team.Id;
        state.GmCareer.FiredFromTeamName = GetTeamName(team);
        state.GmCareer.FiredAtUtc = DateTime.UtcNow.ToString("o");
        state.GmCareer.UpdatedAtUtc = state.GmCareer.FiredAtUtc;

        GmCareerEventData firingEvent = GmCareerService.CreateCareerEvent(
            state,
            "Fired",
            team.Id,
            GetTeamName(team),
            "GM fired by " + GetTeamName(team),
            "Владелец решил сменить менеджмент после слабой оценки сезона.",
            ownerEvaluation == null ? 0 : ownerEvaluation.TrustBefore,
            state.GmCareer.CurrentOwnerTrust,
            0,
            state.GmCareer.CurrentJobSecurity);
        GmCareerService.AddCareerEvent(state, firingEvent);
        EventNewsService.CreateGmFiringNews(state, firingEvent);
        GmJobMarketService.GenerateJobOffers(state);
    }

    public static void ApplyWarning(GameState state, TeamData team, OwnerSeasonEvaluationData ownerEvaluation)
    {
        if (state == null || state.GmCareer == null || team == null)
        {
            return;
        }

        state.GmCareer.CareerStatus = "UnderPressure";
        GmCareerEventData warningEvent = GmCareerService.CreateCareerEvent(
            state,
            "Warning",
            team.Id,
            GetTeamName(team),
            "Owner puts GM under pressure",
            "Job security is " + GmJobSecurityConfig.GetJobSecurityLabel(state.GmCareer.CurrentJobSecurity) + ".",
            ownerEvaluation == null ? 0 : ownerEvaluation.TrustBefore,
            state.GmCareer.CurrentOwnerTrust,
            0,
            state.GmCareer.CurrentJobSecurity);
        GmCareerService.AddCareerEvent(state, warningEvent);
        EventNewsService.CreateGmWarningNews(state, warningEvent);
    }

    public static string BuildJobSecuritySummary(GameState state, TeamData team, OwnerSeasonEvaluationData ownerEvaluation, int delta)
    {
        if (ownerEvaluation == null)
        {
            return "Job security evaluation unavailable";
        }

        string signedDelta = delta >= 0 ? "+" + delta : delta.ToString();
        return ownerEvaluation.TeamName
            + ": owner trust " + ownerEvaluation.TrustBefore + " -> " + ownerEvaluation.TrustAfter
            + ", goals " + ownerEvaluation.GoalsCompleted + "/" + (ownerEvaluation.GoalsCompleted + ownerEvaluation.GoalsFailed)
            + ", playoffs " + (ownerEvaluation.MadePlayoffs ? ownerEvaluation.PlayoffResult : "missed")
            + ", job security " + signedDelta + " to "
            + (state == null || state.GmCareer == null ? 0 : state.GmCareer.CurrentJobSecurity);
    }

    public static void UpdateCareerTotalsAfterSeason(GameState state, TeamData team)
    {
        if (state == null || state.GmCareer == null || team == null)
        {
            return;
        }

        if (state.GmCareer.LastCareerSeasonUpdatedStartYear == state.CurrentSeasonStartYear)
        {
            return;
        }

        TeamStandingData standing = FindStanding(state, team);
        if (standing != null)
        {
            state.GmCareer.CareerWins += standing.Wins;
            state.GmCareer.CareerLosses += standing.Losses;
            state.GmCareer.CareerOvertimeLosses += standing.OvertimeLosses;
        }

        if (OwnerGoalProgressService.DidTeamMakePlayoffs(state, team))
        {
            state.GmCareer.CareerPlayoffAppearances++;
        }

        state.GmCareer.CareerPlayoffRoundsWon += OwnerGoalProgressService.GetPlayoffRoundsWon(state, team);
        if (state.Season != null && state.Season.Playoffs != null && state.Season.Playoffs.ChampionTeamId == team.Id)
        {
            state.GmCareer.CareerChampionships++;
        }

        state.GmCareer.SeasonsCompleted++;
        state.GmCareer.LastCareerSeasonUpdatedStartYear = state.CurrentSeasonStartYear;
        state.GmCareer.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
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
