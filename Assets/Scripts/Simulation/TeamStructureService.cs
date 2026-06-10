using System.Collections.Generic;

public static class TeamStructureService
{
    private const string EasternConference = "Eastern";
    private const string WesternConference = "Western";
    private const string AtlanticDivision = "Atlantic";
    private const string MetropolitanDivision = "Metropolitan";
    private const string CentralDivision = "Central";
    private const string PacificDivision = "Pacific";
    private const string Unknown = "Unknown";

    private static readonly Dictionary<string, string> Conferences = new Dictionary<string, string>
    {
        { "boston-bruins", EasternConference },
        { "buffalo-sabres", EasternConference },
        { "detroit-red-wings", EasternConference },
        { "florida-panthers", EasternConference },
        { "montreal-canadiens", EasternConference },
        { "ottawa-senators", EasternConference },
        { "tampa-bay-lightning", EasternConference },
        { "toronto-maple-leafs", EasternConference },
        { "carolina-hurricanes", EasternConference },
        { "columbus-blue-jackets", EasternConference },
        { "new-jersey-devils", EasternConference },
        { "new-york-islanders", EasternConference },
        { "new-york-rangers", EasternConference },
        { "philadelphia-flyers", EasternConference },
        { "pittsburgh-penguins", EasternConference },
        { "washington-capitals", EasternConference },
        { "chicago-blackhawks", WesternConference },
        { "colorado-avalanche", WesternConference },
        { "dallas-stars", WesternConference },
        { "minnesota-wild", WesternConference },
        { "nashville-predators", WesternConference },
        { "st-louis-blues", WesternConference },
        { "utah-mammoth", WesternConference },
        { "winnipeg-jets", WesternConference },
        { "anaheim-ducks", WesternConference },
        { "calgary-flames", WesternConference },
        { "edmonton-oilers", WesternConference },
        { "los-angeles-kings", WesternConference },
        { "san-jose-sharks", WesternConference },
        { "seattle-kraken", WesternConference },
        { "vancouver-canucks", WesternConference },
        { "vegas-golden-knights", WesternConference }
    };

    private static readonly Dictionary<string, string> Divisions = new Dictionary<string, string>
    {
        { "boston-bruins", AtlanticDivision },
        { "buffalo-sabres", AtlanticDivision },
        { "detroit-red-wings", AtlanticDivision },
        { "florida-panthers", AtlanticDivision },
        { "montreal-canadiens", AtlanticDivision },
        { "ottawa-senators", AtlanticDivision },
        { "tampa-bay-lightning", AtlanticDivision },
        { "toronto-maple-leafs", AtlanticDivision },
        { "carolina-hurricanes", MetropolitanDivision },
        { "columbus-blue-jackets", MetropolitanDivision },
        { "new-jersey-devils", MetropolitanDivision },
        { "new-york-islanders", MetropolitanDivision },
        { "new-york-rangers", MetropolitanDivision },
        { "philadelphia-flyers", MetropolitanDivision },
        { "pittsburgh-penguins", MetropolitanDivision },
        { "washington-capitals", MetropolitanDivision },
        { "chicago-blackhawks", CentralDivision },
        { "colorado-avalanche", CentralDivision },
        { "dallas-stars", CentralDivision },
        { "minnesota-wild", CentralDivision },
        { "nashville-predators", CentralDivision },
        { "st-louis-blues", CentralDivision },
        { "utah-mammoth", CentralDivision },
        { "winnipeg-jets", CentralDivision },
        { "anaheim-ducks", PacificDivision },
        { "calgary-flames", PacificDivision },
        { "edmonton-oilers", PacificDivision },
        { "los-angeles-kings", PacificDivision },
        { "san-jose-sharks", PacificDivision },
        { "seattle-kraken", PacificDivision },
        { "vancouver-canucks", PacificDivision },
        { "vegas-golden-knights", PacificDivision }
    };

    public static string GetConference(string teamId)
    {
        return Conferences.TryGetValue(teamId, out string conference) ? conference : Unknown;
    }

    public static string GetDivision(string teamId)
    {
        return Divisions.TryGetValue(teamId, out string division) ? division : Unknown;
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
}
