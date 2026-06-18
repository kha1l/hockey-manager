using System.Collections.Generic;
using UnityEngine;

public static class TeamAssetService
{
    private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
    private static readonly HashSet<string> MissingWarnings = new HashSet<string>();

    public static Sprite LoadLogo(TeamData team)
    {
        return LoadSprite(team == null || team.Identity == null ? TeamIdentityService.GetLogoResourcePath(team) : team.Identity.LogoResourcePath);
    }

    public static Sprite LoadHomeJersey(TeamData team)
    {
        TeamIdentityService.EnsureTeamIdentity(team);
        return LoadSprite(team == null || team.Identity == null ? "" : team.Identity.HomeJerseyResourcePath);
    }

    public static Sprite LoadAwayJersey(TeamData team)
    {
        TeamIdentityService.EnsureTeamIdentity(team);
        return LoadSprite(team == null || team.Identity == null ? "" : team.Identity.AwayJerseyResourcePath);
    }

    public static Sprite LoadFullBody(TeamData team)
    {
        TeamIdentityService.EnsureTeamIdentity(team);
        return LoadSprite(team == null || team.Identity == null ? "" : team.Identity.FullBodyResourcePath);
    }

    public static Sprite LoadSprite(string resourcePath)
    {
        if (string.IsNullOrEmpty(resourcePath))
        {
            return null;
        }

        if (Cache.TryGetValue(resourcePath, out Sprite cached))
        {
            return cached;
        }

        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            if (MissingWarnings.Add(resourcePath))
            {
                Debug.LogWarning("Team sprite not found in Resources: " + resourcePath);
            }

            return null;
        }

        Cache[resourcePath] = sprite;
        return sprite;
    }

    public static void ClearCache()
    {
        Cache.Clear();
        MissingWarnings.Clear();
    }
}
