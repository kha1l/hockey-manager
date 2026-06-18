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
