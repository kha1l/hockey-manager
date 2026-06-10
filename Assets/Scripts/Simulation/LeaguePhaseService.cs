public static class LeaguePhaseService
{
    public const string RegularSeason = "RegularSeason";
    public const string Playoffs = "Playoffs";
    public const string Draft = "Draft";
    public const string FreeAgency = "FreeAgency";
    public const string Offseason = "Offseason";
    public const string ReadyForNextSeason = "ReadyForNextSeason";

    public static string GetCurrentPhase(GameState state)
    {
        if (state == null || state.Season == null)
        {
            return RegularSeason;
        }

        if (!state.Season.IsSeasonFinished)
        {
            return RegularSeason;
        }

        if (state.Season.Playoffs != null && !state.Season.Playoffs.IsCompleted)
        {
            return Playoffs;
        }

        if (state.Season.Playoffs != null && state.Season.Playoffs.IsCompleted)
        {
            if (state.Draft == null || !state.Draft.IsCompleted)
            {
                return Draft;
            }

            return FreeAgency;
        }

        return RegularSeason;
    }

    public static bool IsFreeAgencyOpen(GameState state)
    {
        return GetCurrentPhase(state) == FreeAgency;
    }

    public static bool IsRegularSeasonActive(GameState state)
    {
        return GetCurrentPhase(state) == RegularSeason;
    }

    public static bool IsPlayoffsActive(GameState state)
    {
        return GetCurrentPhase(state) == Playoffs;
    }

    public static bool IsDraftOpen(GameState state)
    {
        return GetCurrentPhase(state) == Draft;
    }

    public static bool IsOffseason(GameState state)
    {
        string phase = GetCurrentPhase(state);
        return phase == FreeAgency || phase == Offseason || phase == ReadyForNextSeason;
    }

    public static bool CanStartNextSeason(GameState state)
    {
        return state != null
            && state.Season != null
            && state.Season.IsSeasonFinished
            && state.Season.Playoffs != null
            && state.Season.Playoffs.IsCompleted
            && state.Draft != null
            && state.Draft.IsCompleted;
    }
}
