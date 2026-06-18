using System.Text;

public static class FictionalLeagueConfig
{
    public const string LeagueIdentityId = "continental_fictional_v1";
    public const int LeagueIdentityVersion = 1;
    public const string GameTitle = "Continental Hockey Manager";
    public const string LeagueDisplayName = "Continental League";

    public const string WesternConference = "Western Conference";
    public const string EasternConference = "Eastern Conference";

    public const string CapitalDivision = "Capital Division";
    public const string SouthDivision = "South Division";
    public const string VolgaUralDivision = "Volga-Ural Division";
    public const string SiberiaPacificDivision = "Siberia-Pacific Division";

    public static string NormalizeTeamId(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
        {
            return "";
        }

        string normalized = displayName.Trim().ToLowerInvariant();
        StringBuilder builder = new StringBuilder();
        bool previousUnderscore = false;
        foreach (char character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousUnderscore = false;
                continue;
            }

            if ((character == ' ' || character == '-' || character == '_') && !previousUnderscore)
            {
                builder.Append('_');
                previousUnderscore = true;
            }
        }

        return builder.ToString().Trim('_');
    }

    public static string BuildResourcesTeamPath(string teamId, string assetName)
    {
        if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(assetName))
        {
            return "";
        }

        return "Teams/" + teamId + "/" + assetName;
    }
}
