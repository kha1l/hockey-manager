using System;
using System.Collections.Generic;

public static class TeamIdentitySeedData
{
    public static List<TeamIdentityData> CreateTeamIdentities()
    {
        return new List<TeamIdentityData>
        {
            Create("moscow_stars", "Moscow", "Stars", "Moscow Stars", "Stars", "MST", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.CapitalDivision, "Moscow Stars", "красный", "синий", "белый"),
            Create("moscow_commanders", "Moscow", "Commanders", "Moscow Commanders", "Commanders", "MCD", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.CapitalDivision, "Moscow Commanders", "красный", "серебряный", "золотой"),
            Create("moscow_ice_wolves", "Moscow", "Ice Wolves", "Moscow Ice Wolves", "Ice Wolves", "MIW", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.CapitalDivision, "Moscow Ice Wolves", "белый", "голубой", "серебряный"),
            Create("saint_petersburg_admirals", "Saint Petersburg", "Admirals", "Saint Petersburg Admirals", "Admirals", "SPA", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.CapitalDivision, "Saint Petersburg Admirals", "тёмно-синий", "красный", "белый"),
            Create("saint_petersburg_knights", "Saint Petersburg", "Knights", "Saint Petersburg Knights", "Knights", "SPK", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.CapitalDivision, "Saint Petersburg Knights", "чёрный", "серебряный", "синий"),
            Create("kazan_bars", "Kazan", "Bars", "Kazan Bars", "Bars", "KAZ", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.VolgaUralDivision, "Kazan Bars", "зелёный", "красный", "белый"),
            Create("yaroslavl_ironclads", "Yaroslavl", "Ironclads", "Yaroslavl Ironclads", "Ironclads", "YAR", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.CapitalDivision, "Yaroslavl Ironclads", "красный", "белый", "синий"),
            Create("yekaterinburg_hammers", "Yekaterinburg", "Hammers", "Yekaterinburg Hammers", "Hammers", "YEK", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.VolgaUralDivision, "Yekaterinburg Hammers", "красный", "чёрный", "белый"),
            Create("magnitogorsk_steel_foxes", "Magnitogorsk", "Steel Foxes", "Magnitogorsk Steel Foxes", "Steel Foxes", "MAG", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.VolgaUralDivision, "Magnitogorsk Steel Foxes", "белый", "тёмно-синий", "оранжевый"),
            Create("omsk_hawks", "Omsk", "Hawks", "Omsk Hawks", "Hawks", "OMS", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.SiberiaPacificDivision, "Omsk Hawks", "чёрный", "красный", "белый"),
            Create("ufa_nomads", "Ufa", "Nomads", "Ufa Nomads", "Nomads", "UFA", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.VolgaUralDivision, "Ufa Nomads", "зелёный", "белый", "золотой"),
            Create("novosibirsk_siberians", "Novosibirsk", "Siberians", "Novosibirsk Siberians", "Siberians", "NVS", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.SiberiaPacificDivision, "Novosibirsk Siberians", "синий", "белый", "серебряный"),
            Create("chelyabinsk_transformers", "Chelyabinsk", "Transformers", "Chelyabinsk Transformers", "Transformers", "CHT", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.VolgaUralDivision, "Chelyabinsk Transformers", "чёрный", "белый", "серебряный"),
            Create("nizhny_novgorod_stags", "Nizhny Novgorod", "Stags", "Nizhny Novgorod Stags", "Stags", "NNS", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.SouthDivision, "Nizhny Novgorod Stags", "тёмно-синий", "белый", "красный"),
            Create("sochi_leopards", "Sochi", "Leopards", "Sochi Leopards", "Leopards", "SOC", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.SouthDivision, "Sochi Leopards", "тёмно-синий", "бирюзовый", "белый"),
            Create("vladivostok_mariners", "Vladivostok", "Mariners", "Vladivostok Mariners", "Mariners", "VLM", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.SiberiaPacificDivision, "Vladivostok Mariners", "синий", "оранжевый", "белый"),
            Create("khabarovsk_tigers", "Khabarovsk", "Tigers", "Khabarovsk Tigers", "Tigers", "KHT", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.SiberiaPacificDivision, "Khabarovsk Tigers", "оранжевый", "чёрный", "белый"),
            Create("cherepovets_steelmen", "Cherepovets", "Steelmen", "Cherepovets Steelmen", "Steelmen", "CHR", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.CapitalDivision, "Cherepovets Steelmen", "чёрный", "жёлтый", "серебряный"),
            Create("tolyatti_motors", "Tolyatti", "Motors", "Tolyatti Motors", "Motors", "TOL", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.VolgaUralDivision, "Tolyatti Motors", "синий", "белый", "серебряный"),
            Create("minsk_bisons", "Minsk", "Bisons", "Minsk Bisons", "Bisons", "MIN", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.CapitalDivision, "Minsk Bisons", "синий", "белый", "чёрный"),
            Create("astana_golden_eagles", "Astana", "Golden Eagles", "Astana Golden Eagles", "Golden Eagles", "AST", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.SiberiaPacificDivision, "Astana Golden Eagles", "голубой", "золотой", "белый"),
            Create("nizhnekamsk_timberwolves", "Nizhnekamsk", "Timberwolves", "Nizhnekamsk Timberwolves", "Timberwolves", "NTW", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.VolgaUralDivision, "Nizhnekamsk Timberwolves", "белый", "тёмно-синий", "зелёный"),
            Create("krasnoyarsk_red_bears", "Krasnoyarsk", "Red Bears", "Krasnoyarsk Red Bears", "Red Bears", "KRB", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.SiberiaPacificDivision, "Krasnoyarsk Red Bears", "красный", "белый", "чёрный"),
            Create("samara_wings", "Samara", "Wings", "Samara Wings", "Wings", "SAM", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.VolgaUralDivision, "Samara Wings", "синий", "белый", "красный"),
            Create("rostov_on_don_cossacks", "Rostov-on-Don", "Cossacks", "Rostov-on-Don Cossacks", "Cossacks", "ROD", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.SouthDivision, "Rostov-on-Don Cossacks", "тёмно-синий", "красный", "золотой"),
            Create("krasnodar_bulls", "Krasnodar", "Bulls", "Krasnodar Bulls", "Bulls", "KRD", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.SouthDivision, "Krasnodar Bulls", "зелёный", "чёрный", "белый"),
            Create("voronezh_ravens", "Voronezh", "Ravens", "Voronezh Ravens", "Ravens", "VOR", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.SouthDivision, "Voronezh Ravens", "чёрный", "фиолетовый", "серебряный"),
            Create("volgograd_warriors", "Volgograd", "Warriors", "Volgograd Warriors", "Warriors", "VLG", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.SouthDivision, "Volgograd Warriors", "синий", "серый", "белый"),
            Create("kursk_sentinels", "Kursk", "Sentinels", "Kursk Sentinels", "Sentinels", "KUR", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.SouthDivision, "Kursk Sentinels", "красный", "белый", "золотой"),
            Create("tyumen_oilmen", "Tyumen", "Oilmen", "Tyumen Oilmen", "Oilmen", "TYU", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.SiberiaPacificDivision, "Tyumen Oilmen", "чёрный", "белый", "синий"),
            Create("barnaul_lynxes", "Barnaul", "Lynxes", "Barnaul Lynxes", "Lynxes", "BAR", FictionalLeagueConfig.EasternConference, FictionalLeagueConfig.SiberiaPacificDivision, "Barnaul Lynxes", "белый", "голубой", "серебряный"),
            Create("belgorod_lions", "Belgorod", "Lions", "Belgorod Lions", "Lions", "BEL", FictionalLeagueConfig.WesternConference, FictionalLeagueConfig.SouthDivision, "Belgorod Lions", "белый", "золотой", "синий")
        };
    }

    private static TeamIdentityData Create(
        string teamId,
        string city,
        string clubName,
        string displayName,
        string shortName,
        string abbreviation,
        string conferenceName,
        string divisionName,
        string assetFolderName,
        string primaryColorName,
        string secondaryColorName,
        string tertiaryColorName)
    {
        return new TeamIdentityData
        {
            TeamId = teamId,
            City = city,
            ClubName = clubName,
            DisplayName = displayName,
            ShortName = shortName,
            Abbreviation = abbreviation,
            ConferenceName = conferenceName,
            DivisionName = divisionName,
            AssetFolderName = assetFolderName,
            PrimaryColorName = primaryColorName,
            SecondaryColorName = secondaryColorName,
            TertiaryColorName = tertiaryColorName,
            PrimaryColorHex = TeamColorConfig.ColorNameToHex(primaryColorName),
            SecondaryColorHex = TeamColorConfig.ColorNameToHex(secondaryColorName),
            TertiaryColorHex = TeamColorConfig.ColorNameToHex(tertiaryColorName),
            LogoResourcePath = FictionalLeagueConfig.BuildResourcesTeamPath(teamId, "logo"),
            HomeJerseyResourcePath = FictionalLeagueConfig.BuildResourcesTeamPath(teamId, "home"),
            AwayJerseyResourcePath = FictionalLeagueConfig.BuildResourcesTeamPath(teamId, "away"),
            FullBodyResourcePath = FictionalLeagueConfig.BuildResourcesTeamPath(teamId, "full"),
            HasLogo = true,
            HasHomeJersey = true,
            HasAwayJersey = true,
            HasFullBody = true,
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }
}
