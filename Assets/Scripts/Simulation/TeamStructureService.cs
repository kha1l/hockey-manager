using System.Collections.Generic;

public static class TeamStructureService
{
    public const string EasternConference = "Eastern";
    public const string WesternConference = "Western";
    public const string CapitalDivision = "Capital";
    public const string SouthDivision = "South";
    public const string VolgaUralDivision = "Volga-Ural";
    public const string SiberiaPacificDivision = "Siberia-Pacific";

    private const string Unknown = "Unknown";

    private static Dictionary<string, string> _conferences;
    private static Dictionary<string, string> _divisions;

    public static string GetConference(string teamId)
    {
        EnsureMaps();
        return !string.IsNullOrEmpty(teamId) && _conferences.TryGetValue(teamId, out string conference)
            ? conference
            : Unknown;
    }

    public static string GetDivision(string teamId)
    {
        EnsureMaps();
        return !string.IsNullOrEmpty(teamId) && _divisions.TryGetValue(teamId, out string division)
            ? division
            : Unknown;
    }

    public static bool IsSameConference(string firstTeamId, string secondTeamId)
    {
        string firstConference = GetConference(firstTeamId);
        return firstConference != Unknown && firstConference == GetConference(secondTeamId);
    }

    public static bool IsSameDivision(string firstTeamId, string secondTeamId)
    {
        string firstDivision = GetDivision(firstTeamId);
        return firstDivision != Unknown && firstDivision == GetDivision(secondTeamId);
    }

    private static void EnsureMaps()
    {
        if (_conferences != null && _divisions != null)
        {
            return;
        }

        _conferences = new Dictionary<string, string>();
        _divisions = new Dictionary<string, string>();

        List<TeamIdentityData> identities = TeamIdentitySeedData.CreateTeamIdentities();
        foreach (TeamIdentityData identity in identities)
        {
            if (identity == null || string.IsNullOrEmpty(identity.TeamId))
            {
                continue;
            }

            _conferences[identity.TeamId] = NormalizeConference(identity.ConferenceName);
            _divisions[identity.TeamId] = NormalizeDivision(identity.DivisionName);
        }
    }

    private static string NormalizeConference(string conferenceName)
    {
        if (conferenceName == FictionalLeagueConfig.EasternConference)
        {
            return EasternConference;
        }

        if (conferenceName == FictionalLeagueConfig.WesternConference)
        {
            return WesternConference;
        }

        return string.IsNullOrEmpty(conferenceName) ? Unknown : conferenceName;
    }

    private static string NormalizeDivision(string divisionName)
    {
        if (divisionName == FictionalLeagueConfig.CapitalDivision)
        {
            return CapitalDivision;
        }

        if (divisionName == FictionalLeagueConfig.SouthDivision)
        {
            return SouthDivision;
        }

        if (divisionName == FictionalLeagueConfig.VolgaUralDivision)
        {
            return VolgaUralDivision;
        }

        if (divisionName == FictionalLeagueConfig.SiberiaPacificDivision)
        {
            return SiberiaPacificDivision;
        }

        return string.IsNullOrEmpty(divisionName) ? Unknown : divisionName;
    }
}
