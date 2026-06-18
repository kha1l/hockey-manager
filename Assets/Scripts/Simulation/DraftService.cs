using System.Collections.Generic;

public static class DraftService
{
    public static void EnsureDraft(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureDraftData();
        DraftPickOwnershipService.EnsureDraftPickOwnership(state);

        if (state.DraftHistory == null)
        {
            state.DraftHistory = new DraftHistoryData();
        }

        state.DraftHistory.EnsureCompletedPicks();

        if (!IsDraftAvailable(state))
        {
            return;
        }

        if (state.Draft == null)
        {
            state.Draft = new DraftData();
        }

        state.Draft.EnsureCollections();

        if (state.Draft.TotalRounds != 0 && state.Draft.TotalRounds != DraftConfig.DraftRounds)
        {
            state.Draft = new DraftData();
            state.Draft.EnsureCollections();
        }

        if (!state.Draft.IsInitialized)
        {
            int draftYear = DraftPickOwnershipService.GetDraftYear(state);
            DraftClassProfileData profile = DraftClassProfileGenerator.GenerateProfile(draftYear);
            state.Draft.DraftYear = draftYear;
            state.Draft.TotalRounds = DraftConfig.DraftRounds;
            state.Draft.PicksPerRound = DraftConfig.PicksPerRound;
            state.Draft.ClassProfile = profile;
            state.Draft.Prospects = DraftClassGenerator.CreateDraftClass(draftYear, profile);
            state.Draft.DraftOrder = DraftOrderService.CreateDraftOrder(state);
            state.Draft.CurrentPickIndex = 0;
            state.Draft.DraftStatus = "InProgress";
            state.Draft.IsInitialized = true;
            state.Draft.IsCompleted = false;
        }

        EnsureDraftClassProfile(state);
        ProspectRiskService.EnsureRiskProfilesForDraft(state);
        ScoutingService.EnsureScoutingForDraft(state);
    }

    public static void EnsureDraftClassProfile(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.Draft == null)
        {
            state.Draft = new DraftData();
        }

        state.Draft.EnsureCollections();
        int draftYear = state.Draft.DraftYear > 0
            ? state.Draft.DraftYear
            : DraftPickOwnershipService.GetDraftYear(state);
        state.Draft.ClassProfile = NormalizeDraftClassProfile(state.Draft.ClassProfile, draftYear);

        if (state.Draft.Prospects != null && state.Draft.Prospects.Count > 0)
        {
            DraftClassGenerator.EnsureRanksAndMetadata(state.Draft.Prospects, state.Draft.ClassProfile, draftYear);
        }
    }

    public static string GetDraftClassSummary(GameState state)
    {
        EnsureDraftClassProfile(state);
        if (state == null
            || state.Draft == null
            || state.Draft.ClassProfile == null
            || state.Draft.Prospects == null
            || state.Draft.Prospects.Count == 0)
        {
            return "Draft class: not generated";
        }

        DraftClassProfileData profile = state.Draft.ClassProfile;
        string summary = string.IsNullOrEmpty(profile.Summary)
            ? DraftClassConfig.BuildClassSummary(profile)
            : profile.Summary;
        return "Draft class: " + summary
            + " | Elite: " + profile.ExpectedEliteProspects
            + " | 1st-round talent: " + profile.ExpectedFirstRoundTalent
            + " | Depth: " + profile.ExpectedNhlDepthPlayers;
    }

    public static bool IsDraftAvailable(GameState state)
    {
        return state != null
            && state.Season != null
            && state.Season.Playoffs != null
            && state.Season.Playoffs.IsCompleted;
    }

    public static DraftPickData GetCurrentPick(GameState state)
    {
        EnsureDraft(state);

        if (state == null || state.Draft == null || state.Draft.IsCompleted)
        {
            return null;
        }

        state.Draft.EnsureCollections();
        if (state.Draft.CurrentPickIndex < 0)
        {
            state.Draft.CurrentPickIndex = 0;
        }

        if (state.Draft.CurrentPickIndex >= state.Draft.DraftOrder.Count)
        {
            CompleteDraft(state);
            return null;
        }

        return state.Draft.DraftOrder[state.Draft.CurrentPickIndex];
    }

    public static List<ProspectData> GetAvailableProspects(GameState state)
    {
        EnsureDraft(state);

        List<ProspectData> prospects = new List<ProspectData>();
        if (state == null || state.Draft == null || state.Draft.Prospects == null)
        {
            return prospects;
        }

        foreach (ProspectData prospect in state.Draft.Prospects)
        {
            if (prospect != null && !prospect.IsDrafted)
            {
                ScoutingService.EnsureProspectScouting(prospect, GetProspectRank(prospect, prospects.Count + 1));
                prospects.Add(prospect);
            }
        }

        prospects.Sort(CompareAvailableProspects);
        return prospects;
    }

    public static bool IsUserOnClock(GameState state)
    {
        DraftPickData currentPick = GetCurrentPick(state);
        return currentPick != null && currentPick.TeamId == state.SelectedTeamId;
    }

    public static bool SelectProspectForCurrentPick(
        GameState state,
        string prospectId,
        out string message)
    {
        EnsureDraft(state);

        if (!IsDraftAvailable(state))
        {
            message = "Драфт станет доступен после завершения плей-офф";
            return false;
        }

        DraftPickData currentPick = GetCurrentPick(state);
        if (currentPick == null)
        {
            message = "Драфт завершён";
            return false;
        }

        if (currentPick.TeamId != state.SelectedTeamId)
        {
            message = "Сейчас выбирает другая команда";
            return false;
        }

        ProspectData prospect = FindProspect(state, prospectId);
        if (prospect == null || prospect.IsDrafted)
        {
            message = "Проспект недоступен";
            return false;
        }

        ApplyPick(state, currentPick, prospect);
        message = "Игрок выбран: " + prospect.FirstName + " " + prospect.LastName;
        return true;
    }

    public static bool AutoPickCurrentSelection(GameState state, out string message)
    {
        EnsureDraft(state);

        DraftPickData currentPick = GetCurrentPick(state);
        if (currentPick == null)
        {
            message = "Драфт завершён";
            return false;
        }

        ProspectData prospect = ChooseBestAvailableProspect(state, currentPick);
        if (prospect == null)
        {
            CompleteDraft(state);
            message = "Драфт завершён";
            return false;
        }

        ApplyPick(state, currentPick, prospect);
        message = currentPick.TeamName + " выбрала " + prospect.FirstName + " " + prospect.LastName;
        return true;
    }

    public static void AutoPickUntilUserPickOrDraftEnd(GameState state)
    {
        EnsureDraft(state);

        int safetyCounter = 0;
        while (state != null
            && state.Draft != null
            && !state.Draft.IsCompleted
            && safetyCounter < DraftConfig.TotalDraftPicks)
        {
            if (IsUserOnClock(state))
            {
                return;
            }

            AutoPickCurrentSelection(state, out string message);
            safetyCounter++;
        }

        if (state != null && state.Draft != null && state.Draft.CurrentPickIndex >= state.Draft.DraftOrder.Count)
        {
            CompleteDraft(state);
        }
    }

    private static ProspectData ChooseBestAvailableProspect(GameState state, DraftPickData pick)
    {
        // TODO: Future: CPU draft should account for boom/bust risk, scouting uncertainty, and team needs.
        List<ProspectData> prospects = GetAvailableProspects(state);
        if (prospects.Count == 0)
        {
            return null;
        }

        return prospects[0];
    }

    private static void ApplyPick(GameState state, DraftPickData currentPick, ProspectData prospect)
    {
        ProspectRiskService.EnsureRiskProfile(prospect, GetProspectRank(prospect, currentPick.OverallPick));

        currentPick.IsCompleted = true;
        currentPick.SelectedProspectId = prospect.Id;
        currentPick.SelectedProspectName = prospect.FirstName + " " + prospect.LastName;

        prospect.IsDrafted = true;
        prospect.DraftedByTeamId = currentPick.TeamId;
        prospect.DraftedByTeamName = currentPick.TeamName;
        prospect.DraftRound = currentPick.Round;
        prospect.DraftPickOverall = currentPick.OverallPick;

        TeamData team = FindTeam(state.Teams, currentPick.TeamId);
        if (team != null)
        {
            team.EnsureDraftRights();
            team.DraftRights.Add(prospect);
        }

        state.DraftHistory.EnsureCompletedPicks();
        state.DraftHistory.CompletedPicks.Add(ClonePick(currentPick));
        EventNewsService.CreateDraftPickNews(state, currentPick);

        DraftPickOwnershipData ownership = DraftPickOwnershipService.FindPick(state, currentPick.PickId);
        if (ownership != null)
        {
            ownership.IsUsed = true;
        }

        state.Draft.CurrentPickIndex++;
        if (state.Draft.CurrentPickIndex >= state.Draft.DraftOrder.Count)
        {
            CompleteDraft(state);
        }
    }

    private static void CompleteDraft(GameState state)
    {
        if (state == null || state.Draft == null)
        {
            return;
        }

        state.Draft.DraftStatus = "Completed";
        state.Draft.IsCompleted = true;
    }

    private static ProspectData FindProspect(GameState state, string prospectId)
    {
        if (state == null || state.Draft == null || state.Draft.Prospects == null || string.IsNullOrEmpty(prospectId))
        {
            return null;
        }

        foreach (ProspectData prospect in state.Draft.Prospects)
        {
            if (prospect != null && prospect.Id == prospectId)
            {
                return prospect;
            }
        }

        return null;
    }

    private static TeamData FindTeam(List<TeamData> teams, string teamId)
    {
        if (teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
    }

    private static DraftPickData ClonePick(DraftPickData pick)
    {
        return new DraftPickData
        {
            PickId = pick.PickId,
            Round = pick.Round,
            PickInRound = pick.PickInRound,
            OverallPick = pick.OverallPick,
            OriginalTeamId = pick.OriginalTeamId,
            OriginalTeamName = pick.OriginalTeamName,
            TeamId = pick.TeamId,
            TeamName = pick.TeamName,
            IsUserTeamPick = pick.IsUserTeamPick,
            IsCompleted = pick.IsCompleted,
            SelectedProspectId = pick.SelectedProspectId,
            SelectedProspectName = pick.SelectedProspectName
        };
    }

    private static int CompareAvailableProspects(ProspectData left, ProspectData right)
    {
        int rankComparison = GetProspectRank(left, 999).CompareTo(GetProspectRank(right, 999));
        if (rankComparison != 0)
        {
            return rankComparison;
        }

        int potentialComparison = right.Potential.CompareTo(left.Potential);
        if (potentialComparison != 0)
        {
            return potentialComparison;
        }

        int overallComparison = right.Overall.CompareTo(left.Overall);
        return overallComparison != 0 ? overallComparison : left.Age.CompareTo(right.Age);
    }

    private static DraftClassProfileData NormalizeDraftClassProfile(DraftClassProfileData profile, int draftYear)
    {
        if (profile == null
            || !DraftClassConfig.IsValidStrengthType(profile.StrengthType)
            || !DraftClassConfig.IsValidDepthType(profile.DepthType)
            || !DraftClassConfig.IsValidPositionalTheme(profile.PositionalTheme))
        {
            return DraftClassProfileGenerator.CreateFallbackProfile(draftYear);
        }

        if (profile.DraftYear <= 0)
        {
            profile.DraftYear = draftYear;
        }

        if (string.IsNullOrEmpty(profile.ProfileId))
        {
            profile.ProfileId = "draft-class-profile-fallback-" + profile.DraftYear;
        }

        if (string.IsNullOrEmpty(profile.GeneratedAtUtc))
        {
            profile.GeneratedAtUtc = System.DateTime.UtcNow.ToString("o");
        }

        DraftClassProfileGenerator.ApplyProfileModifiers(profile);
        if (string.IsNullOrEmpty(profile.Summary))
        {
            profile.Summary = DraftClassConfig.BuildClassSummary(profile);
        }

        return profile;
    }

    private static int GetProspectRank(ProspectData prospect, int fallbackRank)
    {
        if (prospect == null)
        {
            return fallbackRank;
        }

        if (prospect.DraftRank > 0)
        {
            return prospect.DraftRank;
        }

        return prospect.ProjectedPick > 0 ? prospect.ProjectedPick : fallbackRank;
    }
}
