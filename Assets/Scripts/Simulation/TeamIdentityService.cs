using System.Collections.Generic;
using UnityEngine;

public static class TeamIdentityService
{
    private static List<TeamIdentityData> _cachedIdentities;
    private static Dictionary<string, TeamIdentityData> _identitiesByTeamId;
    private static Dictionary<string, TeamIdentityData> _identitiesByNormalizedName;

    public static List<TeamIdentityData> GetAllIdentities()
    {
        return new List<TeamIdentityData>(GetCachedIdentities());
    }

    public static TeamIdentityData GetIdentityByTeamId(string teamId)
    {
        if (string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        EnsureIdentityCache();
        string normalized = FictionalLeagueConfig.NormalizeTeamId(teamId);
        return _identitiesByTeamId.TryGetValue(normalized, out TeamIdentityData identity) ? identity : null;
    }

    public static TeamIdentityData GetIdentityByDisplayName(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
        {
            return null;
        }

        string normalized = FictionalLeagueConfig.NormalizeTeamId(displayName);
        EnsureIdentityCache();
        if (_identitiesByTeamId.TryGetValue(normalized, out TeamIdentityData byId))
        {
            return byId;
        }

        return _identitiesByNormalizedName.TryGetValue(normalized, out TeamIdentityData byName) ? byName : null;
    }

    public static void ApplyIdentityToTeam(TeamData team, TeamIdentityData identity)
    {
        if (team == null || identity == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(team.Id) || GetIdentityByTeamId(team.Id) == null)
        {
            team.Id = identity.TeamId;
        }

        team.Name = identity.DisplayName;
        team.City = identity.City;
        team.Abbreviation = identity.Abbreviation;
        team.ConferenceName = identity.ConferenceName;
        team.DivisionName = identity.DivisionName;
        team.PrimaryColorHex = identity.PrimaryColorHex;
        team.SecondaryColorHex = identity.SecondaryColorHex;
        team.TertiaryColorHex = identity.TertiaryColorHex;
        team.Identity = identity;
    }

    public static void EnsureTeamIdentity(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        TeamIdentityData identity = team.Identity;
        if (IsIdentityApplied(team, identity))
        {
            return;
        }

        if (identity == null)
        {
            identity = GetIdentityByTeamId(team.Id) ?? GetIdentityByDisplayName(team.Name);
        }

        if (identity == null)
        {
            identity = CreateMinimalIdentity(team);
        }

        ApplyIdentityToTeam(team, identity);
    }

    public static void EnsureTeamIdentities(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureTeamIdentity(team);
        }
    }

    public static void EnsureGameStateIdentity(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.LeagueIdentityId = FictionalLeagueConfig.LeagueIdentityId;
        state.LeagueIdentityVersion = FictionalLeagueConfig.LeagueIdentityVersion;
        state.LeagueDisplayName = FictionalLeagueConfig.LeagueDisplayName;
        state.GameDisplayName = FictionalLeagueConfig.GameTitle;
        EnsureTeamIdentities(state.Teams);
    }

    public static bool IsCurrentLeagueIdentity(GameState state)
    {
        return state != null
            && state.LeagueIdentityId == FictionalLeagueConfig.LeagueIdentityId
            && state.LeagueIdentityVersion == FictionalLeagueConfig.LeagueIdentityVersion;
    }

    public static bool TeamsMatchFictionalLeague(List<TeamData> teams)
    {
        List<TeamIdentityData> identities = GetCachedIdentities();
        if (teams == null || teams.Count != identities.Count)
        {
            return false;
        }

        HashSet<string> expectedIds = new HashSet<string>();
        foreach (TeamIdentityData identity in identities)
        {
            if (identity != null)
            {
                expectedIds.Add(identity.TeamId);
            }
        }

        foreach (TeamData team in teams)
        {
            if (team == null || string.IsNullOrEmpty(team.Id) || !expectedIds.Contains(team.Id))
            {
                return false;
            }
        }

        return true;
    }

    private static List<TeamIdentityData> GetCachedIdentities()
    {
        EnsureIdentityCache();
        return _cachedIdentities;
    }

    private static void EnsureIdentityCache()
    {
        if (_cachedIdentities != null && _identitiesByTeamId != null && _identitiesByNormalizedName != null)
        {
            return;
        }

        _cachedIdentities = TeamIdentitySeedData.CreateTeamIdentities();
        _identitiesByTeamId = new Dictionary<string, TeamIdentityData>();
        _identitiesByNormalizedName = new Dictionary<string, TeamIdentityData>();

        foreach (TeamIdentityData identity in _cachedIdentities)
        {
            if (identity == null)
            {
                continue;
            }

            AddIdentityLookup(_identitiesByTeamId, identity.TeamId, identity);
            AddIdentityLookup(_identitiesByNormalizedName, identity.DisplayName, identity);
            AddIdentityLookup(_identitiesByNormalizedName, identity.ClubName, identity);
            AddIdentityLookup(_identitiesByNormalizedName, identity.ShortName, identity);
        }
    }

    private static void AddIdentityLookup(Dictionary<string, TeamIdentityData> lookup, string key, TeamIdentityData identity)
    {
        if (lookup == null || string.IsNullOrEmpty(key) || identity == null)
        {
            return;
        }

        string normalized = FictionalLeagueConfig.NormalizeTeamId(key);
        if (!lookup.ContainsKey(normalized))
        {
            lookup.Add(normalized, identity);
        }
    }

    private static bool IsIdentityApplied(TeamData team, TeamIdentityData identity)
    {
        return team != null
            && identity != null
            && team.Id == identity.TeamId
            && team.Name == identity.DisplayName
            && team.City == identity.City
            && team.Abbreviation == identity.Abbreviation
            && team.ConferenceName == identity.ConferenceName
            && team.DivisionName == identity.DivisionName
            && team.PrimaryColorHex == identity.PrimaryColorHex
            && team.SecondaryColorHex == identity.SecondaryColorHex
            && team.TertiaryColorHex == identity.TertiaryColorHex;
    }

    public static bool TryEnsureCompatibleGameState(GameState state)
    {
        if (state == null)
        {
            return false;
        }

        if (IsCurrentLeagueIdentity(state) && TeamsMatchFictionalLeague(state.Teams))
        {
            EnsureGameStateIdentity(state);
            return true;
        }

        if (string.IsNullOrEmpty(state.LeagueIdentityId) && TeamsMatchFictionalLeague(state.Teams))
        {
            EnsureGameStateIdentity(state);
            return true;
        }

        return false;
    }

    public static string GetDisplayName(TeamData team)
    {
        if (team == null)
        {
            return "";
        }

        EnsureTeamIdentity(team);
        if (team.Identity != null && !string.IsNullOrEmpty(team.Identity.DisplayName))
        {
            return team.Identity.DisplayName;
        }

        return string.IsNullOrEmpty(team.Name) ? (team.City ?? "") : team.Name;
    }

    public static string GetAbbreviation(TeamData team)
    {
        if (team == null)
        {
            return "";
        }

        EnsureTeamIdentity(team);
        return team.Identity != null && !string.IsNullOrEmpty(team.Identity.Abbreviation)
            ? team.Identity.Abbreviation
            : (team.Abbreviation ?? "");
    }

    public static string GetLogoResourcePath(TeamData team)
    {
        EnsureTeamIdentity(team);
        return team == null || team.Identity == null ? "" : team.Identity.LogoResourcePath;
    }

    public static Color GetPrimaryColor(TeamData team)
    {
        EnsureTeamIdentity(team);
        return TeamColorConfig.HexToColor(team == null ? "" : team.PrimaryColorHex);
    }

    public static Color GetSecondaryColor(TeamData team)
    {
        EnsureTeamIdentity(team);
        return TeamColorConfig.HexToColor(team == null ? "" : team.SecondaryColorHex);
    }

    private static TeamIdentityData CreateMinimalIdentity(TeamData team)
    {
        string displayName = string.IsNullOrEmpty(team.Name) ? team.Id : team.Name;
        string teamId = string.IsNullOrEmpty(team.Id)
            ? FictionalLeagueConfig.NormalizeTeamId(displayName)
            : team.Id;
        return new TeamIdentityData
        {
            TeamId = teamId,
            City = string.IsNullOrEmpty(team.City) ? "" : team.City,
            ClubName = displayName,
            DisplayName = displayName,
            ShortName = displayName,
            Abbreviation = string.IsNullOrEmpty(team.Abbreviation) ? "" : team.Abbreviation,
            ConferenceName = string.IsNullOrEmpty(team.ConferenceName) ? "" : team.ConferenceName,
            DivisionName = string.IsNullOrEmpty(team.DivisionName) ? "" : team.DivisionName,
            PrimaryColorHex = string.IsNullOrEmpty(team.PrimaryColorHex) ? "#FFFFFF" : team.PrimaryColorHex,
            SecondaryColorHex = string.IsNullOrEmpty(team.SecondaryColorHex) ? "#111111" : team.SecondaryColorHex,
            TertiaryColorHex = string.IsNullOrEmpty(team.TertiaryColorHex) ? "#808080" : team.TertiaryColorHex,
            LogoResourcePath = FictionalLeagueConfig.BuildResourcesTeamPath(teamId, "logo"),
            HomeJerseyResourcePath = FictionalLeagueConfig.BuildResourcesTeamPath(teamId, "home"),
            AwayJerseyResourcePath = FictionalLeagueConfig.BuildResourcesTeamPath(teamId, "away"),
            FullBodyResourcePath = FictionalLeagueConfig.BuildResourcesTeamPath(teamId, "full"),
            UpdatedAtUtc = System.DateTime.UtcNow.ToString("o")
        };
    }
}
