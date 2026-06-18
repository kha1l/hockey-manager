using System;
using System.Collections.Generic;

public static class TeamTradeProfileService
{
    public static void EnsureTradeProfiles(GameState state)
    {
        if (state == null)
        {
            return;
        }

        List<TeamTradeProfileData> profiles = new List<TeamTradeProfileData>();
        if (state.Teams != null)
        {
            foreach (TeamData team in state.Teams)
            {
                if (team != null)
                {
                    profiles.Add(BuildTradeProfile(state, team));
                }
            }
        }

        StoreTradeProfiles(state, profiles);
    }

    public static TeamTradeProfileData BuildTradeProfile(GameState state, TeamData team)
    {
        EnsureTeamPlayers(team);
        TeamNeedData needs = TeamNeedService.CalculateTeamNeeds(state, team);
        List<TradeBlockPlayerData> tradeBlock = TradeBlockService.BuildTradeBlock(state, team, needs);
        return new TeamTradeProfileData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = TeamIdentityService.GetDisplayName(team),
            Direction = needs == null ? "" : needs.Direction,
            Needs = needs,
            TradeBlock = tradeBlock,
            BuyerScore = TeamDirectionService.CalculateBuyerScore(state, team),
            SellerScore = TeamDirectionService.CalculateSellerScore(state, team),
            CapPressureScore = needs == null ? 0 : needs.NeedCapSpace,
            RosterPressureScore = needs == null ? 0 : needs.NeedRosterSpace,
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    public static TeamTradeProfileData GetTradeProfile(GameState state, string teamId)
    {
        if (state == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        state.EnsureTeamTradeProfiles();
        foreach (TeamTradeProfileData profile in state.TeamTradeProfiles)
        {
            if (profile != null && profile.TeamId == teamId)
            {
                profile.EnsureCollections();
                return profile;
            }
        }

        TeamData team = FindTeam(state, teamId);
        return team == null ? null : BuildTradeProfile(state, team);
    }

    public static List<TeamTradeProfileData> GetAllTradeProfiles(GameState state)
    {
        if (state == null)
        {
            return new List<TeamTradeProfileData>();
        }

        state.EnsureTeamTradeProfiles();
        return new List<TeamTradeProfileData>(state.TeamTradeProfiles);
    }

    public static void StoreTradeProfiles(GameState state, List<TeamTradeProfileData> profiles)
    {
        if (state == null)
        {
            return;
        }

        state.TeamTradeProfiles = profiles ?? new List<TeamTradeProfileData>();
        foreach (TeamTradeProfileData profile in state.TeamTradeProfiles)
        {
            if (profile != null)
            {
                profile.EnsureCollections();
            }
        }

        state.LastTradeProfilesUpdatedAtUtc = DateTime.UtcNow.ToString("o");
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

    private static void EnsureTeamPlayers(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        team.EnsureDraftRights();
        if (team.Players.Count == 0)
        {
            team.Players = PlayerSeedData.CreatePlayersForTeam(team.Id);
        }

        ContractGenerator.EnsureContractsForTeam(team);
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        WaiverEligibilityService.EnsureWaiverEligibilityForTeam(team);
    }
}
