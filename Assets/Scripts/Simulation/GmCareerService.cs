using System;
using System.Collections.Generic;

public static class GmCareerService
{
    public static void EnsureGmCareer(GameState state)
    {
        if (state == null)
        {
            return;
        }

        EnsureCareerLists(state);
        TeamData currentTeam = FindTeam(state, state.SelectedTeamId);

        if (state.GmCareer == null)
        {
            OwnerProfileData owner = currentTeam == null ? null : OwnerGoalService.GetOwnerProfile(state, currentTeam);
            state.GmCareer = new GmCareerData
            {
                GmId = Guid.NewGuid().ToString("N"),
                GmName = "User GM",
                CurrentTeamId = state.SelectedTeamId,
                CurrentTeamName = GetTeamName(currentTeam),
                CareerStartYear = state.CurrentSeasonStartYear,
                CurrentSeasonStartYear = state.CurrentSeasonStartYear,
                CurrentJobSecurity = GmJobSecurityConfig.DefaultJobSecurity,
                CurrentOwnerTrust = owner == null ? GmJobSecurityConfig.DefaultJobSecurity : owner.GmTrust,
                CareerStatus = "Employed",
                UpdatedAtUtc = DateTime.UtcNow.ToString("o")
            };
        }

        state.GmCareer.EnsureCollections();

        if (string.IsNullOrEmpty(state.GmCareer.GmId))
        {
            state.GmCareer.GmId = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrEmpty(state.GmCareer.GmName))
        {
            state.GmCareer.GmName = "User GM";
        }

        if (state.GmCareer.CareerStartYear <= 0)
        {
            state.GmCareer.CareerStartYear = state.CurrentSeasonStartYear;
        }

        state.GmCareer.CurrentSeasonStartYear = state.CurrentSeasonStartYear;
        state.GmCareer.CurrentJobSecurity = GmJobSecurityConfig.ClampSecurity(
            state.GmCareer.CurrentJobSecurity <= 0 ? GmJobSecurityConfig.DefaultJobSecurity : state.GmCareer.CurrentJobSecurity);

        if (!state.GmCareer.IsUnemployed)
        {
            state.GmCareer.CurrentTeamId = state.SelectedTeamId;
            state.GmCareer.CurrentTeamName = GetTeamName(currentTeam);
            OwnerProfileData owner = currentTeam == null ? null : OwnerGoalService.GetOwnerProfile(state, currentTeam);
            state.GmCareer.CurrentOwnerTrust = owner == null
                ? GmJobSecurityConfig.DefaultJobSecurity
                : OwnerGoalConfig.ClampTrust(owner.GmTrust);
            state.GmCareer.CareerStatus = state.GmCareer.IsFired
                ? "Fired"
                : GmJobSecurityConfig.GetCareerStatusBySecurity(state.GmCareer.CurrentJobSecurity);
            AddManagedTeam(state.GmCareer, state.GmCareer.CurrentTeamId, state.GmCareer.CurrentTeamName);
        }
        else
        {
            state.GmCareer.CareerStatus = state.GmCareer.IsFired ? "Fired" : "Unemployed";
        }

        state.GmCareer.TeamsManaged = state.GmCareer.ManagedTeamIds.Count;
        state.GmCareer.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
        state.LastGmCareerUpdateUtc = state.GmCareer.UpdatedAtUtc;
        TrimCareerEvents(state);
    }

    public static void EnsureCareerLists(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureGmCareerData();
    }

    public static void UpdateCareerFromCurrentState(GameState state)
    {
        EnsureGmCareer(state);
    }

    public static void AddCareerEvent(GameState state, GmCareerEventData careerEvent)
    {
        if (state == null || careerEvent == null)
        {
            return;
        }

        EnsureCareerLists(state);
        state.GmCareerEvents.Add(careerEvent);
        if (state.GmCareer != null)
        {
            state.GmCareer.LastCareerEventSummary = careerEvent.Title + ": " + careerEvent.Summary;
            state.GmCareer.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
            state.LastGmCareerUpdateUtc = state.GmCareer.UpdatedAtUtc;
        }

        TrimCareerEvents(state);
    }

    public static GmCareerEventData CreateCareerEvent(
        GameState state,
        string eventType,
        string teamId,
        string teamName,
        string title,
        string summary,
        int trustBefore,
        int trustAfter,
        int securityBefore,
        int securityAfter)
    {
        return new GmCareerEventData
        {
            EventId = Guid.NewGuid().ToString("N"),
            EventType = string.IsNullOrEmpty(eventType) ? "TrustChanged" : eventType,
            SeasonStartYear = state == null ? 0 : state.CurrentSeasonStartYear,
            SeasonEndYear = state == null ? 0 : state.CurrentSeasonEndYear,
            TeamId = string.IsNullOrEmpty(teamId) ? "" : teamId,
            TeamName = string.IsNullOrEmpty(teamName) ? "" : teamName,
            Title = string.IsNullOrEmpty(title) ? "GM career update" : title,
            Summary = string.IsNullOrEmpty(summary) ? "" : summary,
            TrustBefore = trustBefore,
            TrustAfter = trustAfter,
            JobSecurityBefore = securityBefore,
            JobSecurityAfter = securityAfter,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    public static void TrimCareerEvents(GameState state)
    {
        if (state == null || state.GmCareerEvents == null)
        {
            return;
        }

        while (state.GmCareerEvents.Count > GmJobSecurityConfig.MaxCareerEventsToKeep)
        {
            state.GmCareerEvents.RemoveAt(0);
        }
    }

    public static string BuildCareerSummary(GameState state)
    {
        EnsureGmCareer(state);
        GmCareerData career = state == null ? null : state.GmCareer;
        if (career == null)
        {
            return "GM career: нет данных";
        }

        return "GM Job Security: " + GmJobSecurityConfig.GetJobSecurityLabel(career.CurrentJobSecurity)
            + " " + career.CurrentJobSecurity
            + " | Career: " + career.CareerStatus
            + " with " + SafeText(career.CurrentTeamName, "no team")
            + "\nOwner trust: " + career.CurrentOwnerTrust
            + " | Seasons: " + career.SeasonsCompleted
            + " | Teams: " + career.TeamsManaged
            + " | Record: " + MobileUiConfig.FormatRecord(career.CareerWins, career.CareerLosses, career.CareerOvertimeLosses)
            + " | Cups: " + career.CareerChampionships;
    }

    public static void AddManagedTeam(GmCareerData career, string teamId, string teamName)
    {
        if (career == null || string.IsNullOrEmpty(teamId))
        {
            return;
        }

        career.EnsureCollections();
        if (!career.ManagedTeamIds.Contains(teamId))
        {
            career.ManagedTeamIds.Add(teamId);
        }

        if (!string.IsNullOrEmpty(teamName) && !career.ManagedTeamNames.Contains(teamName))
        {
            career.ManagedTeamNames.Add(teamName);
        }

        career.TeamsManaged = career.ManagedTeamIds.Count;
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

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrEmpty(value) ? fallback : value;
    }
}
