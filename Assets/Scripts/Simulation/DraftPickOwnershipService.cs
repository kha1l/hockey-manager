using System.Collections.Generic;

public static class DraftPickOwnershipService
{
    public static void EnsureDraftPickOwnership(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureDraftData();

        if (state.Teams == null)
        {
            return;
        }

        if (HasInvalidRound(state.DraftPickOwnership))
        {
            state.DraftPickOwnership = new List<DraftPickOwnershipData>();
        }

        int draftYear = GetDraftYear(state);
        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            for (int round = 1; round <= DraftConfig.DraftRounds; round++)
            {
                string pickId = CreatePickId(draftYear, round, team.Id);
                if (FindPick(state, pickId) != null)
                {
                    continue;
                }

                string teamName = GetTeamName(team);
                state.DraftPickOwnership.Add(new DraftPickOwnershipData
                {
                    PickId = pickId,
                    DraftYear = draftYear,
                    Round = round,
                    OriginalTeamId = team.Id,
                    OriginalTeamName = teamName,
                    CurrentOwnerTeamId = team.Id,
                    CurrentOwnerTeamName = teamName,
                    IsTraded = false,
                    IsUsed = false,
                    LastTradeId = ""
                });
            }
        }
    }

    public static List<DraftPickOwnershipData> GetOwnedPicks(GameState state, string teamId)
    {
        EnsureDraftPickOwnership(state);

        List<DraftPickOwnershipData> picks = new List<DraftPickOwnershipData>();
        if (state == null || state.DraftPickOwnership == null || string.IsNullOrEmpty(teamId))
        {
            return picks;
        }

        foreach (DraftPickOwnershipData pick in state.DraftPickOwnership)
        {
            if (pick != null && pick.CurrentOwnerTeamId == teamId && !pick.IsUsed)
            {
                picks.Add(pick);
            }
        }

        picks.Sort(ComparePicks);
        return picks;
    }

    public static DraftPickOwnershipData FindPick(GameState state, string pickId)
    {
        if (state == null || state.DraftPickOwnership == null || string.IsNullOrEmpty(pickId))
        {
            return null;
        }

        foreach (DraftPickOwnershipData pick in state.DraftPickOwnership)
        {
            if (pick != null && pick.PickId == pickId)
            {
                return pick;
            }
        }

        return null;
    }

    public static void TransferPick(
        GameState state,
        string pickId,
        string newOwnerTeamId,
        string newOwnerTeamName,
        string tradeId)
    {
        DraftPickOwnershipData pick = FindPick(state, pickId);
        if (pick == null)
        {
            return;
        }

        pick.CurrentOwnerTeamId = newOwnerTeamId;
        pick.CurrentOwnerTeamName = newOwnerTeamName;
        pick.IsTraded = true;
        pick.LastTradeId = tradeId;

        UpdateDraftOrderPick(state, pick);
    }

    public static int GetDraftYear(GameState state)
    {
        if (state != null && state.LeagueCalendar != null && state.LeagueCalendar.SeasonEndYear > 0)
        {
            return state.LeagueCalendar.SeasonEndYear;
        }

        return 2027;
    }

    public static string CreatePickId(int draftYear, int round, string originalTeamId)
    {
        return "draft-" + draftYear + "-round-" + round + "-original-" + originalTeamId;
    }

    private static void UpdateDraftOrderPick(GameState state, DraftPickOwnershipData ownership)
    {
        if (state == null || state.Draft == null || state.Draft.DraftOrder == null || ownership == null)
        {
            return;
        }

        foreach (DraftPickData pick in state.Draft.DraftOrder)
        {
            if (pick != null && pick.PickId == ownership.PickId && !pick.IsCompleted)
            {
                pick.TeamId = ownership.CurrentOwnerTeamId;
                pick.TeamName = ownership.CurrentOwnerTeamName;
                pick.IsUserTeamPick = pick.TeamId == state.SelectedTeamId;
            }
        }
    }

    private static bool HasInvalidRound(List<DraftPickOwnershipData> picks)
    {
        if (picks == null)
        {
            return false;
        }

        foreach (DraftPickOwnershipData pick in picks)
        {
            if (pick != null && pick.Round > DraftConfig.DraftRounds)
            {
                return true;
            }
        }

        return false;
    }

    private static int ComparePicks(DraftPickOwnershipData left, DraftPickOwnershipData right)
    {
        int roundComparison = left.Round.CompareTo(right.Round);
        if (roundComparison != 0)
        {
            return roundComparison;
        }

        return string.Compare(left.OriginalTeamName, right.OriginalTeamName, System.StringComparison.Ordinal);
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
