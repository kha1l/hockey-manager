using System.Collections.Generic;
using UnityEngine;

public static class LeagueSeedService
{
    public const string LeagueSeedResourcePath = "Seeds/league_seed";
    public const string LeagueSeedVersionResourcePath = "Seeds/league_seed_version";
    private static LeagueSeedData _cachedSeed;

    public static List<TeamData> CreateTeams()
    {
        LeagueSeedData seed = GetOrCreateSeed();
        return CloneTeams(seed.Teams);
    }

    public static List<TeamData> CreateTeamSummaries()
    {
        List<TeamData> teams = new List<TeamData>();
        List<TeamIdentityData> identities = TeamIdentitySeedData.CreateTeamIdentities();
        foreach (TeamIdentityData identity in identities)
        {
            if (identity == null)
            {
                continue;
            }

            TeamData team = new TeamData
            {
                Players = new List<PlayerData>(),
                DraftRights = new List<ProspectData>()
            };
            TeamIdentityService.ApplyIdentityToTeam(team, identity);
            teams.Add(team);
        }

        return teams;
    }

    public static LeagueSeedData LoadSeedFromResources()
    {
        TextAsset versionAsset = Resources.Load<TextAsset>(LeagueSeedVersionResourcePath);
        if (versionAsset == null || versionAsset.text.Trim() != LeagueSeedGenerator.CurrentSeedVersion)
        {
            return null;
        }

        TextAsset asset = Resources.Load<TextAsset>(LeagueSeedResourcePath);
        if (asset == null || string.IsNullOrEmpty(asset.text))
        {
            return null;
        }

        if (!ContainsCurrentSeedVersion(asset.text))
        {
            Debug.LogWarning("League seed version mismatch. Expected "
                + LeagueSeedGenerator.CurrentSeedVersion
                + ". Runtime seed generator will be used.");
            return null;
        }

        LeagueSeedData seed = JsonUtility.FromJson<LeagueSeedData>(asset.text);
        if (seed == null || seed.Teams == null || seed.Teams.Count == 0)
        {
            return null;
        }

        if (seed.SeedVersion != LeagueSeedGenerator.CurrentSeedVersion)
        {
            Debug.LogWarning("League seed version mismatch. Expected "
                + LeagueSeedGenerator.CurrentSeedVersion
                + ", got "
                + seed.SeedVersion
                + ". Runtime seed generator will be used.");
            return null;
        }

        seed.EnsureCollections();
        return seed;
    }

    private static bool ContainsCurrentSeedVersion(string text)
    {
        string compactVersion = "\"SeedVersion\":\"" + LeagueSeedGenerator.CurrentSeedVersion + "\"";
        string prettyVersion = "\"SeedVersion\": \"" + LeagueSeedGenerator.CurrentSeedVersion + "\"";
        return text.Contains(compactVersion) || text.Contains(prettyVersion);
    }

    private static LeagueSeedData GetOrCreateSeed()
    {
        if (_cachedSeed != null)
        {
            return _cachedSeed;
        }

        _cachedSeed = LoadSeedFromResources();
        if (_cachedSeed == null)
        {
            _cachedSeed = LeagueSeedGenerator.CreateLeagueSeed();
        }

        _cachedSeed.EnsureCollections();
        return _cachedSeed;
    }

    private static List<TeamData> CloneTeams(List<TeamData> teams)
    {
        LeagueSeedData wrapper = new LeagueSeedData
        {
            SeedVersion = LeagueSeedGenerator.CurrentSeedVersion,
            CreatedAtUtc = "",
            Teams = teams
        };

        string json = JsonUtility.ToJson(wrapper);
        LeagueSeedData clone = JsonUtility.FromJson<LeagueSeedData>(json);
        if (clone == null)
        {
            clone = LeagueSeedGenerator.CreateLeagueSeed();
        }

        clone.EnsureCollections();
        TeamIdentityService.EnsureTeamIdentities(clone.Teams);
        return clone.Teams;
    }
}
