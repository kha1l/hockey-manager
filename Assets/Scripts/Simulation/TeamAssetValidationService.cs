using System.Collections.Generic;
using UnityEngine;

public static class TeamAssetValidationService
{
    public static List<string> ValidateTeamAssets()
    {
        List<string> warnings = new List<string>();
        foreach (TeamIdentityData identity in TeamIdentityService.GetAllIdentities())
        {
            if (identity == null)
            {
                continue;
            }

            if (!HasLogo(identity)) warnings.Add(identity.DisplayName + ": missing logo");
            if (!HasHomeJersey(identity)) warnings.Add(identity.DisplayName + ": missing home jersey");
            if (!HasAwayJersey(identity)) warnings.Add(identity.DisplayName + ": missing away jersey");
            if (!HasFullBody(identity)) warnings.Add(identity.DisplayName + ": missing full body asset");
        }

        return warnings;
    }

    public static bool HasLogo(TeamIdentityData identity)
    {
        return HasSprite(identity == null ? "" : identity.LogoResourcePath);
    }

    public static bool HasHomeJersey(TeamIdentityData identity)
    {
        return HasSprite(identity == null ? "" : identity.HomeJerseyResourcePath);
    }

    public static bool HasAwayJersey(TeamIdentityData identity)
    {
        return HasSprite(identity == null ? "" : identity.AwayJerseyResourcePath);
    }

    public static bool HasFullBody(TeamIdentityData identity)
    {
        return HasSprite(identity == null ? "" : identity.FullBodyResourcePath);
    }

    private static bool HasSprite(string resourcePath)
    {
        return !string.IsNullOrEmpty(resourcePath) && Resources.Load<Sprite>(resourcePath) != null;
    }
}

public static class TeamJerseySelectionService
{
    public static string GetHomeJerseyPath(TeamData homeTeam)
    {
        TeamIdentityData identity = homeTeam == null ? null : homeTeam.Identity;
        return identity == null ? "" : identity.HomeJerseyResourcePath;
    }

    public static string GetAwayJerseyPath(TeamData homeTeam, TeamData awayTeam)
    {
        TeamIdentityData awayIdentity = awayTeam == null ? null : awayTeam.Identity;
        if (awayIdentity == null)
        {
            return "";
        }

        return HasDistinctHomeAwayColors(homeTeam, awayTeam)
            ? awayIdentity.AwayJerseyResourcePath
            : awayIdentity.HomeJerseyResourcePath;
    }

    private static bool HasDistinctHomeAwayColors(TeamData homeTeam, TeamData awayTeam)
    {
        TeamIdentityData homeIdentity = homeTeam == null ? null : homeTeam.Identity;
        TeamIdentityData awayIdentity = awayTeam == null ? null : awayTeam.Identity;
        if (homeIdentity == null || awayIdentity == null)
        {
            return true;
        }

        string awayJerseyColor = string.IsNullOrEmpty(awayIdentity.TertiaryColorHex)
            ? awayIdentity.SecondaryColorHex
            : awayIdentity.TertiaryColorHex;
        return !SameColor(homeIdentity.PrimaryColorHex, awayJerseyColor);
    }

    private static bool SameColor(string left, string right)
    {
        if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
        {
            return false;
        }

        return string.Equals(left.Trim(), right.Trim(), System.StringComparison.OrdinalIgnoreCase);
    }
}
