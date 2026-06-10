using System.Collections.Generic;

public static class TeamSeedData
{
    public static List<TeamData> CreateTeams()
    {
        return new List<TeamData>
        {
            CreateTeam("anaheim-ducks", "Ducks", "Anaheim", "ANA"),
            CreateTeam("boston-bruins", "Bruins", "Boston", "BOS"),
            CreateTeam("buffalo-sabres", "Sabres", "Buffalo", "BUF"),
            CreateTeam("calgary-flames", "Flames", "Calgary", "CGY"),
            CreateTeam("carolina-hurricanes", "Hurricanes", "Carolina", "CAR"),
            CreateTeam("chicago-blackhawks", "Blackhawks", "Chicago", "CHI"),
            CreateTeam("colorado-avalanche", "Avalanche", "Colorado", "COL"),
            CreateTeam("columbus-blue-jackets", "Blue Jackets", "Columbus", "CBJ"),
            CreateTeam("dallas-stars", "Stars", "Dallas", "DAL"),
            CreateTeam("detroit-red-wings", "Red Wings", "Detroit", "DET"),
            CreateTeam("edmonton-oilers", "Oilers", "Edmonton", "EDM"),
            CreateTeam("florida-panthers", "Panthers", "Florida", "FLA"),
            CreateTeam("los-angeles-kings", "Kings", "Los Angeles", "LAK"),
            CreateTeam("minnesota-wild", "Wild", "Minnesota", "MIN"),
            CreateTeam("montreal-canadiens", "Canadiens", "Montreal", "MTL"),
            CreateTeam("nashville-predators", "Predators", "Nashville", "NSH"),
            CreateTeam("new-jersey-devils", "Devils", "New Jersey", "NJD"),
            CreateTeam("new-york-islanders", "Islanders", "New York", "NYI"),
            CreateTeam("new-york-rangers", "Rangers", "New York", "NYR"),
            CreateTeam("ottawa-senators", "Senators", "Ottawa", "OTT"),
            CreateTeam("philadelphia-flyers", "Flyers", "Philadelphia", "PHI"),
            CreateTeam("pittsburgh-penguins", "Penguins", "Pittsburgh", "PIT"),
            CreateTeam("san-jose-sharks", "Sharks", "San Jose", "SJS"),
            CreateTeam("seattle-kraken", "Kraken", "Seattle", "SEA"),
            CreateTeam("st-louis-blues", "Blues", "St. Louis", "STL"),
            CreateTeam("tampa-bay-lightning", "Lightning", "Tampa Bay", "TBL"),
            CreateTeam("toronto-maple-leafs", "Maple Leafs", "Toronto", "TOR"),
            CreateTeam("utah-mammoth", "Mammoth", "Utah", "UTA"),
            CreateTeam("vancouver-canucks", "Canucks", "Vancouver", "VAN"),
            CreateTeam("vegas-golden-knights", "Golden Knights", "Vegas", "VGK"),
            CreateTeam("washington-capitals", "Capitals", "Washington", "WSH"),
            CreateTeam("winnipeg-jets", "Jets", "Winnipeg", "WPG")
        };
    }

    private static TeamData CreateTeam(string id, string name, string city, string abbreviation)
    {
        return new TeamData
        {
            Id = id,
            Name = name,
            City = city,
            Abbreviation = abbreviation,
            Players = new List<PlayerData>()
        };
    }
}
