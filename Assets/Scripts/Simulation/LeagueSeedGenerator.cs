using System;
using System.Collections.Generic;

public static class LeagueSeedGenerator
{
    public const string CurrentSeedVersion = "league-seed-v2";
    private const string SeedCreatedAtUtc = "2026-07-01T00:00:00.0000000Z";

    private static readonly PlayerSlot[] PlayerSlots =
    {
        new PlayerSlot("C", RosterStatusConfig.NHL, 0),
        new PlayerSlot("C", RosterStatusConfig.NHL, 1),
        new PlayerSlot("C", RosterStatusConfig.NHL, 2),
        new PlayerSlot("C", RosterStatusConfig.NHL, 3),
        new PlayerSlot("C", RosterStatusConfig.NHL, 4),
        new PlayerSlot("LW", RosterStatusConfig.NHL, 0),
        new PlayerSlot("LW", RosterStatusConfig.NHL, 1),
        new PlayerSlot("LW", RosterStatusConfig.NHL, 2),
        new PlayerSlot("LW", RosterStatusConfig.NHL, 3),
        new PlayerSlot("RW", RosterStatusConfig.NHL, 0),
        new PlayerSlot("RW", RosterStatusConfig.NHL, 1),
        new PlayerSlot("RW", RosterStatusConfig.NHL, 2),
        new PlayerSlot("RW", RosterStatusConfig.NHL, 3),
        new PlayerSlot("D", RosterStatusConfig.NHL, 0),
        new PlayerSlot("D", RosterStatusConfig.NHL, 1),
        new PlayerSlot("D", RosterStatusConfig.NHL, 2),
        new PlayerSlot("D", RosterStatusConfig.NHL, 3),
        new PlayerSlot("D", RosterStatusConfig.NHL, 4),
        new PlayerSlot("D", RosterStatusConfig.NHL, 5),
        new PlayerSlot("D", RosterStatusConfig.NHL, 6),
        new PlayerSlot("G", RosterStatusConfig.NHL, 0),
        new PlayerSlot("G", RosterStatusConfig.NHL, 1),
        new PlayerSlot("G", RosterStatusConfig.NHL, 2),
        new PlayerSlot("C", RosterStatusConfig.Farm, 5),
        new PlayerSlot("C", RosterStatusConfig.Farm, 6),
        new PlayerSlot("LW", RosterStatusConfig.Farm, 4),
        new PlayerSlot("LW", RosterStatusConfig.Farm, 5),
        new PlayerSlot("RW", RosterStatusConfig.Farm, 4),
        new PlayerSlot("RW", RosterStatusConfig.Farm, 5),
        new PlayerSlot("D", RosterStatusConfig.Farm, 7),
        new PlayerSlot("D", RosterStatusConfig.Farm, 8),
        new PlayerSlot("D", RosterStatusConfig.Farm, 9),
        new PlayerSlot("G", RosterStatusConfig.Farm, 3),
        new PlayerSlot("LW", RosterStatusConfig.Reserve, 6),
        new PlayerSlot("D", RosterStatusConfig.Reserve, 10),
        new PlayerSlot("G", RosterStatusConfig.Reserve, 4)
    };

    public static LeagueSeedData CreateLeagueSeed()
    {
        LeagueSeedData seed = new LeagueSeedData
        {
            SeedVersion = CurrentSeedVersion,
            CreatedAtUtc = SeedCreatedAtUtc,
            Teams = new List<TeamData>()
        };

        List<TeamIdentityData> identities = TeamIdentitySeedData.CreateTeamIdentities();
        for (int i = 0; i < identities.Count; i++)
        {
            TeamIdentityData identity = identities[i];
            if (identity == null)
            {
                continue;
            }

            TeamData team = new TeamData
            {
                DraftRights = new List<ProspectData>()
            };
            TeamIdentityService.ApplyIdentityToTeam(team, identity);
            team.Players = CreatePlayersForTeam(identity, i);
            InitializeTeamForSeed(team);
            seed.Teams.Add(team);
        }

        seed.EnsureCollections();
        return seed;
    }

    public static List<PlayerData> CreatePlayersForTeam(string teamId)
    {
        List<TeamIdentityData> identities = TeamIdentitySeedData.CreateTeamIdentities();
        for (int i = 0; i < identities.Count; i++)
        {
            TeamIdentityData identity = identities[i];
            if (identity != null && identity.TeamId == teamId)
            {
                return CreatePlayersForTeam(identity, i);
            }
        }

        TeamIdentityData fallbackIdentity = new TeamIdentityData
        {
            TeamId = teamId,
            DisplayName = teamId,
            Abbreviation = ""
        };
        return CreatePlayersForTeam(fallbackIdentity, 0);
    }

    public static int GetTeamPreviewRating(string teamId)
    {
        return Clamp(78 + GetTeamStrengthBonus(teamId) * 2, 70, 88);
    }

    private static List<PlayerData> CreatePlayersForTeam(TeamIdentityData identity, int teamIndex)
    {
        List<PlayerData> players = new List<PlayerData>();
        int teamBonus = GetTeamStrengthBonus(identity.TeamId);
        Random random = new Random(CreateStableSeed(identity.TeamId));

        for (int i = 0; i < PlayerSlots.Length; i++)
        {
            PlayerSlot slot = PlayerSlots[i];
            players.Add(CreatePlayer(identity, teamIndex, teamBonus, slot, i + 1, random));
        }

        AddBelgorodProspect(identity, players);
        return players;
    }

    private static void AddBelgorodProspect(TeamIdentityData identity, List<PlayerData> players)
    {
        if (identity == null || identity.TeamId != "belgorod_lions" || players == null)
        {
            return;
        }

        foreach (PlayerData existing in players)
        {
            if (existing != null && existing.JerseyNumber == 89)
            {
                existing.JerseyNumber = 88;
            }
        }

        PlayerData player = new PlayerData
        {
            Id = identity.TeamId + "-aleksey-kharlanov",
            FirstName = "Aleksey",
            LastName = "Kharlanov",
            Nationality = "Russia",
            TeamId = identity.TeamId,
            Position = "RW",
            Age = 18,
            Overall = 74,
            Potential = 97,
            Salary = 950000,
            ContractYearsRemaining = 3,
            ContractStatus = "Signed",
            HasNoTradeClause = false,
            IsGeneratedContract = true,
            IsEntryLevelContract = true,
            SourceProspectId = "",
            DraftRound = 0,
            DraftPickOverall = 0,
            LastSeasonOverall = 74,
            LastSeasonPotential = 97,
            LastDevelopmentDelta = 0,
            LastDevelopmentType = "",
            Condition = FatigueConfig.DefaultCondition,
            Fatigue = FatigueConfig.DefaultFatigue,
            ConsecutiveGamesPlayed = 0,
            GamesRested = 0,
            IsResting = false,
            LastGameFatigueChange = 0,
            LastGameConditionChange = 0,
            IsInjured = false,
            InjuryType = "",
            InjurySeverity = "",
            InjuryDaysRemaining = 0,
            CanPlayThroughInjury = false,
            InjuredAtUtc = "",
            ExpectedReturnDate = "",
            TotalInjuries = 0,
            RosterStatus = RosterStatusConfig.NHL,
            PreviousRosterStatus = RosterStatusConfig.NHL,
            RosterStatusUpdatedAtUtc = SeedCreatedAtUtc,
            WaiverStatus = WaiverConfig.WaiverStatusNone,
            WaiverPlacedAtUtc = "",
            WaiverExpiresAtUtc = "",
            WaiverOriginalTeamId = "",
            WaiverOriginalTeamName = "",
            WaiverIntendedDestination = "",
            CareerAwardIds = new List<string>(),
            JerseyNumber = 89
        };

        PlayerDevelopmentService.EnsureDevelopmentProfile(player);
        player.HiddenCeiling = 99;
        player.HiddenFloor = 82;
        player.DevelopmentRisk = 12;
        player.BoomChance = 28;
        player.BustChance = 3;
        player.DevelopmentType = "Elite";
        player.DevelopmentTypeHint = "Huge potential";
        PlayerFatigueService.EnsureFatigueFields(player);
        InjuryService.EnsureInjuryFields(player);
        PlayerRoleService.EnsureRole(player);
        player.PlayerRole = "Sniper";
        player.UsageCategory = "ThirdLine";
        LeadershipService.EnsurePlayerLeadershipProfile(player);
        WaiverEligibilityService.EnsureWaiverEligibility(player);
        MoveWeakestRightWingToFarm(players);
        players.Add(player);
    }

    private static void MoveWeakestRightWingToFarm(List<PlayerData> players)
    {
        PlayerData weakest = null;
        foreach (PlayerData candidate in players)
        {
            if (candidate == null || candidate.Position != "RW" || candidate.RosterStatus != RosterStatusConfig.NHL)
            {
                continue;
            }

            if (weakest == null || candidate.Overall < weakest.Overall)
            {
                weakest = candidate;
            }
        }

        if (weakest == null)
        {
            return;
        }

        weakest.RosterStatus = RosterStatusConfig.Farm;
        weakest.PreviousRosterStatus = RosterStatusConfig.Farm;
        weakest.UsageCategory = "FourthLine";
    }

    private static void InitializeTeamForSeed(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        team.EnsureDraftRights();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        team.Lineup = LineupService.BuildAutoLineup(team);
        team.SpecialTeams = SpecialTeamsService.BuildAutoSpecialTeams(team);
        TacticsService.EnsureTactics(team);
        IceTimeService.EnsureUsageForTeam(team);
    }

    private static PlayerData CreatePlayer(
        TeamIdentityData identity,
        int teamIndex,
        int teamBonus,
        PlayerSlot slot,
        int playerNumber,
        Random random)
    {
        string nationality = PlayerNameSeedData.PickNationality(random);
        PlayerNameSeedData.PickName(nationality, random, out string firstName, out string lastName);

        int overall = CalculateOverall(slot, teamBonus, random);
        int age = CalculateAge(slot, overall, random);
        int potential = CalculatePotential(overall, age, slot, random);
        int salary = CalculateSalary(overall, slot.RosterStatus, random);

        PlayerData player = new PlayerData
        {
            Id = identity.TeamId + "-seed-" + playerNumber.ToString("00"),
            FirstName = firstName,
            LastName = lastName,
            Nationality = nationality,
            TeamId = identity.TeamId,
            Position = slot.Position,
            Age = age,
            Overall = overall,
            Potential = potential,
            Salary = salary,
            ContractYearsRemaining = CalculateContractYears(age, overall, random),
            ContractStatus = "Signed",
            HasNoTradeClause = overall >= 86 && random.Next(0, 100) < 25,
            IsGeneratedContract = true,
            IsEntryLevelContract = false,
            SourceProspectId = "",
            DraftRound = 0,
            DraftPickOverall = 0,
            LastSeasonOverall = overall,
            LastSeasonPotential = potential,
            LastDevelopmentDelta = 0,
            LastDevelopmentType = "",
            Condition = FatigueConfig.DefaultCondition,
            Fatigue = FatigueConfig.DefaultFatigue,
            ConsecutiveGamesPlayed = 0,
            GamesRested = 0,
            IsResting = false,
            LastGameFatigueChange = 0,
            LastGameConditionChange = 0,
            IsInjured = false,
            InjuryType = "",
            InjurySeverity = "",
            InjuryDaysRemaining = 0,
            CanPlayThroughInjury = false,
            InjuredAtUtc = "",
            ExpectedReturnDate = "",
            TotalInjuries = 0,
            RosterStatus = slot.RosterStatus,
            PreviousRosterStatus = slot.RosterStatus,
            RosterStatusUpdatedAtUtc = SeedCreatedAtUtc,
            WaiverStatus = WaiverConfig.WaiverStatusNone,
            WaiverPlacedAtUtc = "",
            WaiverExpiresAtUtc = "",
            WaiverOriginalTeamId = "",
            WaiverOriginalTeamName = "",
            WaiverIntendedDestination = "",
            CareerAwardIds = new List<string>(),
            JerseyNumber = 1 + ((teamIndex * 37 + playerNumber * 7) % 98)
        };

        PlayerDevelopmentService.EnsureDevelopmentProfile(player);
        PlayerFatigueService.EnsureFatigueFields(player);
        InjuryService.EnsureInjuryFields(player);
        PlayerRoleService.EnsureRole(player);
        LeadershipService.EnsurePlayerLeadershipProfile(player);
        WaiverEligibilityService.EnsureWaiverEligibility(player);
        return player;
    }

    private static int CalculateOverall(PlayerSlot slot, int teamBonus, Random random)
    {
        int baseOverall;
        if (slot.RosterStatus == RosterStatusConfig.Farm)
        {
            baseOverall = slot.Position == "G"
                ? GetRandomInclusive(random, 60, 70)
                : GetRandomInclusive(random, 58, 72);
            return Clamp(baseOverall + teamBonus / 2, 50, 78);
        }

        if (slot.RosterStatus == RosterStatusConfig.Reserve)
        {
            return Clamp(GetRandomInclusive(random, 52, 66) + teamBonus / 3, 48, 72);
        }

        if (slot.Position == "G")
        {
            if (slot.DepthRank == 0) return Clamp(GetRandomInclusive(random, 82, 88) + teamBonus, 74, 92);
            if (slot.DepthRank == 1) return Clamp(GetRandomInclusive(random, 75, 81) + teamBonus / 2, 68, 86);
            return Clamp(GetRandomInclusive(random, 68, 74) + teamBonus / 2, 60, 80);
        }

        if (slot.Position == "D")
        {
            if (slot.DepthRank == 0) return Clamp(GetRandomInclusive(random, 82, 88) + teamBonus, 74, 92);
            if (slot.DepthRank <= 2) return Clamp(GetRandomInclusive(random, 77, 83) + teamBonus, 70, 88);
            if (slot.DepthRank <= 4) return Clamp(GetRandomInclusive(random, 72, 78) + teamBonus / 2, 65, 83);
            return Clamp(GetRandomInclusive(random, 68, 74) + teamBonus / 2, 60, 80);
        }

        if (slot.DepthRank == 0) return Clamp(GetRandomInclusive(random, 83, 89) + teamBonus, 74, 93);
        if (slot.DepthRank <= 2) return Clamp(GetRandomInclusive(random, 78, 84) + teamBonus, 70, 89);
        if (slot.DepthRank <= 4) return Clamp(GetRandomInclusive(random, 73, 79) + teamBonus / 2, 65, 84);
        return Clamp(GetRandomInclusive(random, 68, 74) + teamBonus / 2, 60, 80);
    }

    private static int CalculateAge(PlayerSlot slot, int overall, Random random)
    {
        if (slot.RosterStatus == RosterStatusConfig.Farm)
        {
            return GetRandomInclusive(random, 19, 25);
        }

        if (slot.RosterStatus == RosterStatusConfig.Reserve)
        {
            return GetRandomInclusive(random, 18, 29);
        }

        if (overall >= 84)
        {
            return GetRandomInclusive(random, 24, 33);
        }

        if (overall >= 76)
        {
            return GetRandomInclusive(random, 22, 34);
        }

        return GetRandomInclusive(random, 20, 36);
    }

    private static int CalculatePotential(int overall, int age, PlayerSlot slot, Random random)
    {
        int growthWindow = age <= 22 ? 12 : age <= 25 ? 8 : age <= 29 ? 4 : 2;
        if (slot.RosterStatus == RosterStatusConfig.Farm || slot.RosterStatus == RosterStatusConfig.Reserve)
        {
            growthWindow += 4;
        }

        return Clamp(overall + GetRandomInclusive(random, 0, growthWindow), overall, 95);
    }

    private static int CalculateSalary(int overall, string rosterStatus, Random random)
    {
        if (rosterStatus != RosterStatusConfig.NHL)
        {
            return RoundToNearest(GetRandomInclusive(random, SalaryCapConfig.LeagueMinimumSalary, 1200000), 50000);
        }

        if (overall >= 90)
        {
            return RoundToNearest(GetRandomInclusive(random, 11000000, 15500000), 50000);
        }

        if (overall >= 85)
        {
            return RoundToNearest(GetRandomInclusive(random, 7500000, 11000000), 50000);
        }

        if (overall >= 80)
        {
            return RoundToNearest(GetRandomInclusive(random, 4500000, 7500000), 50000);
        }

        if (overall >= 75)
        {
            return RoundToNearest(GetRandomInclusive(random, 2200000, 4500000), 50000);
        }

        if (overall >= 70)
        {
            return RoundToNearest(GetRandomInclusive(random, 1100000, 2200000), 50000);
        }

        return RoundToNearest(GetRandomInclusive(random, SalaryCapConfig.LeagueMinimumSalary, 1300000), 50000);
    }

    private static int CalculateContractYears(int age, int overall, Random random)
    {
        if (overall >= 86)
        {
            return GetRandomInclusive(random, 3, SalaryCapConfig.MaxContractYearsWithOwnTeam);
        }

        if (age <= 23)
        {
            return GetRandomInclusive(random, 2, 3);
        }

        if (age <= 30)
        {
            return GetRandomInclusive(random, 2, 5);
        }

        return GetRandomInclusive(random, 1, 3);
    }

    private static int GetTeamStrengthBonus(string teamId)
    {
        switch (teamId)
        {
            case "moscow_stars":
            case "saint_petersburg_admirals":
            case "kazan_bars":
            case "magnitogorsk_steel_foxes":
                return 4;
            case "yekaterinburg_hammers":
            case "omsk_hawks":
            case "ufa_nomads":
            case "yaroslavl_ironclads":
                return 3;
            case "moscow_commanders":
            case "saint_petersburg_knights":
            case "chelyabinsk_transformers":
            case "minsk_bisons":
            case "vladivostok_mariners":
                return 2;
            case "barnaul_lynxes":
            case "belgorod_lions":
            case "kursk_sentinels":
            case "tyumen_oilmen":
                return -2;
            case "voronezh_ravens":
            case "volgograd_warriors":
            case "astana_golden_eagles":
            case "krasnoyarsk_red_bears":
                return -1;
            default:
                return 0;
        }
    }

    private static int CreateStableSeed(string value)
    {
        unchecked
        {
            int hash = 216613626;
            string source = string.IsNullOrEmpty(value) ? "league-seed" : value;
            for (int i = 0; i < source.Length; i++)
            {
                hash ^= source[i];
                hash *= 16777619;
            }

            return hash == int.MinValue ? 0 : Math.Abs(hash);
        }
    }

    private static int GetRandomInclusive(Random random, int minValue, int maxValue)
    {
        return random.Next(minValue, maxValue + 1);
    }

    private static int RoundToNearest(int value, int step)
    {
        return (int)Math.Round(value / (double)step) * step;
    }

    private static int Clamp(int value, int minValue, int maxValue)
    {
        if (value < minValue)
        {
            return minValue;
        }

        return value > maxValue ? maxValue : value;
    }

    private struct PlayerSlot
    {
        public readonly string Position;
        public readonly string RosterStatus;
        public readonly int DepthRank;

        public PlayerSlot(string position, string rosterStatus, int depthRank)
        {
            Position = position;
            RosterStatus = rosterStatus;
            DepthRank = depthRank;
        }
    }
}
