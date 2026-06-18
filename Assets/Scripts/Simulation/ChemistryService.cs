using System;
using System.Collections.Generic;
using UnityEngine;

public static class ChemistryService
{
    public static void EnsureChemistryForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        LineupService.EnsureLineup(team);
        SpecialTeamsService.EnsureSpecialTeams(team);
        PlayerRoleService.EnsureRolesForTeam(team);
        PlayerFatigueService.EnsureFatigueForTeam(team);
        InjuryService.EnsureInjuryFieldsForTeam(team);
        MoraleService.EnsureMoraleForTeam(null, team);
        CoachingStaffService.EnsureStaffForTeam(team);
        IceTimeService.EnsureUsageForTeam(team);

        TeamChemistryData chemistry = CalculateTeamChemistry(team);
        ApplyChemistryToTeamData(team, chemistry);
    }

    public static void EnsureChemistryForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureChemistryForTeam(team);
        }
    }

    public static TeamChemistryData CalculateTeamChemistry(TeamData team)
    {
        TeamChemistryData chemistry = new TeamChemistryData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o"),
            BestUnitScore = 0,
            WorstUnitScore = 100
        };

        if (team == null)
        {
            chemistry.TeamChemistryScore = ChemistryConfig.DefaultChemistry;
            chemistry.TeamChemistryLabel = ChemistryConfig.GetChemistryLabel(chemistry.TeamChemistryScore);
            chemistry.TeamChemistrySummary = "No team data";
            return chemistry;
        }

        LineupService.EnsureLineup(team);
        SpecialTeamsService.EnsureSpecialTeams(team);

        if (team.Lineup != null)
        {
            team.Lineup.EnsureCollections();
            foreach (ForwardLineData line in team.Lineup.ForwardLines)
            {
                AddUnit(chemistry.ForwardLines, CalculateForwardLineChemistry(team, line));
            }

            foreach (DefensePairData pair in team.Lineup.DefensePairs)
            {
                AddUnit(chemistry.DefensePairs, CalculateDefensePairChemistry(team, pair));
            }
        }

        if (team.SpecialTeams != null)
        {
            team.SpecialTeams.EnsureCollections();
            foreach (PowerPlayUnitData unit in team.SpecialTeams.PowerPlayUnits)
            {
                AddUnit(chemistry.PowerPlayUnits, CalculatePowerPlayChemistry(team, unit));
            }

            foreach (PenaltyKillUnitData unit in team.SpecialTeams.PenaltyKillUnits)
            {
                AddUnit(chemistry.PenaltyKillUnits, CalculatePenaltyKillChemistry(team, unit));
            }
        }

        chemistry.ForwardChemistryAverage = AverageChemistry(chemistry.ForwardLines);
        chemistry.DefenseChemistryAverage = AverageChemistry(chemistry.DefensePairs);
        chemistry.SpecialTeamsChemistryAverage = AverageChemistry(CombineUnits(chemistry.PowerPlayUnits, chemistry.PenaltyKillUnits));
        int moraleAverage = CalculateTeamMoraleAverage(team);
        int leadershipImpact = LeadershipService.GetTeamChemistryImpact(team);
        int staffImpact = CoachingStaffService.GetChemistryModifier(team);
        chemistry.MoraleChemistryImpact = moraleAverage - MoraleConfig.DefaultMorale;
        chemistry.TeamChemistryScore = ChemistryConfig.ClampChemistry(Mathf.RoundToInt(
            chemistry.ForwardChemistryAverage * 0.40f
            + chemistry.DefenseChemistryAverage * 0.30f
            + chemistry.SpecialTeamsChemistryAverage * 0.15f
            + moraleAverage * 0.15f
            + leadershipImpact
            + staffImpact));
        chemistry.TeamChemistryLabel = ChemistryConfig.GetChemistryLabel(chemistry.TeamChemistryScore);
        FillBestAndWorst(chemistry);
        chemistry.TeamChemistrySummary = BuildTeamSummary(chemistry) + BuildLeadershipSummary(team) + BuildStaffSummary(team);
        chemistry.EnsureCollections();
        return chemistry;
    }

    public static LineChemistryData CalculateForwardLineChemistry(TeamData team, ForwardLineData line)
    {
        List<string> playerIds = new List<string>();
        if (line != null)
        {
            playerIds.Add(line.LeftWingPlayerId);
            playerIds.Add(line.CenterPlayerId);
            playerIds.Add(line.RightWingPlayerId);
        }

        List<PlayerData> players = ResolvePlayers(team, playerIds);
        int roleBalance = ChemistryRoleFitService.CalculateForwardLineRoleBalance(players);
        int morale = CalculateMoraleScore(players);
        int condition = CalculateConditionScore(players);
        int stability = CalculateStabilityScore(team, playerIds, "ForwardLine:" + (line == null ? 0 : line.LineNumber));
        int positionFit = ChemistryRoleFitService.CalculatePositionFit(players, "ForwardLine");
        int specialTeamsFit = ChemistryConfig.DefaultChemistry;
        int score = ChemistryConfig.ClampChemistry(Mathf.RoundToInt(
            roleBalance * 0.35f
            + morale * 0.20f
            + condition * 0.15f
            + stability * 0.10f
            + positionFit * 0.20f));

        if (HasInvalidUnitPlayers(players, 3))
        {
            score = Mathf.Min(score, 35);
        }

        return BuildUnit(team, "ForwardLine", line == null ? 0 : line.LineNumber, "Line " + (line == null ? 0 : line.LineNumber), playerIds, players, score, roleBalance, morale, condition, stability, positionFit, specialTeamsFit);
    }

    public static LineChemistryData CalculateDefensePairChemistry(TeamData team, DefensePairData pair)
    {
        List<string> playerIds = new List<string>();
        if (pair != null)
        {
            playerIds.Add(pair.LeftDefensePlayerId);
            playerIds.Add(pair.RightDefensePlayerId);
        }

        List<PlayerData> players = ResolvePlayers(team, playerIds);
        int roleBalance = ChemistryRoleFitService.CalculateDefensePairRoleBalance(players);
        int morale = CalculateMoraleScore(players);
        int condition = CalculateConditionScore(players);
        int stability = CalculateStabilityScore(team, playerIds, "DefensePair:" + (pair == null ? 0 : pair.PairNumber));
        int positionFit = ChemistryRoleFitService.CalculatePositionFit(players, "DefensePair");
        int specialTeamsFit = ChemistryConfig.DefaultChemistry;
        int score = ChemistryConfig.ClampChemistry(Mathf.RoundToInt(
            roleBalance * 0.40f
            + morale * 0.20f
            + condition * 0.15f
            + stability * 0.10f
            + positionFit * 0.15f));

        if (HasInvalidUnitPlayers(players, 2))
        {
            score = Mathf.Min(score, 35);
        }

        return BuildUnit(team, "DefensePair", pair == null ? 0 : pair.PairNumber, "Pair " + (pair == null ? 0 : pair.PairNumber), playerIds, players, score, roleBalance, morale, condition, stability, positionFit, specialTeamsFit);
    }

    public static LineChemistryData CalculatePowerPlayChemistry(TeamData team, PowerPlayUnitData unit)
    {
        List<string> playerIds = GetPowerPlayIds(unit);
        List<PlayerData> players = ResolvePlayers(team, playerIds);
        int roleBalance = ChemistryRoleFitService.CalculatePowerPlayRoleBalance(players);
        int morale = CalculateMoraleScore(players);
        int condition = CalculateConditionScore(players);
        int stability = CalculateStabilityScore(team, playerIds, "PowerPlay:" + (unit == null ? 0 : unit.UnitNumber));
        int positionFit = ChemistryRoleFitService.CalculatePositionFit(players, "PowerPlay");
        int specialTeamsFit = CalculateSpecialTeamsFitScore(players, "PowerPlay");
        int score = ChemistryConfig.ClampChemistry(Mathf.RoundToInt(
            roleBalance * 0.40f
            + morale * 0.15f
            + condition * 0.15f
            + stability * 0.10f
            + positionFit * 0.10f
            + specialTeamsFit * 0.10f));

        if (HasInvalidUnitPlayers(players, 5))
        {
            score = Mathf.Min(score, 35);
        }

        return BuildUnit(team, "PowerPlay", unit == null ? 0 : unit.UnitNumber, "PP" + (unit == null ? 0 : unit.UnitNumber), playerIds, players, score, roleBalance, morale, condition, stability, positionFit, specialTeamsFit);
    }

    public static LineChemistryData CalculatePenaltyKillChemistry(TeamData team, PenaltyKillUnitData unit)
    {
        List<string> playerIds = GetPenaltyKillIds(unit);
        List<PlayerData> players = ResolvePlayers(team, playerIds);
        int roleBalance = ChemistryRoleFitService.CalculatePenaltyKillRoleBalance(players);
        int morale = CalculateMoraleScore(players);
        int condition = CalculateConditionScore(players);
        int stability = CalculateStabilityScore(team, playerIds, "PenaltyKill:" + (unit == null ? 0 : unit.UnitNumber));
        int positionFit = ChemistryRoleFitService.CalculatePositionFit(players, "PenaltyKill");
        int specialTeamsFit = CalculateSpecialTeamsFitScore(players, "PenaltyKill");
        int score = ChemistryConfig.ClampChemistry(Mathf.RoundToInt(
            roleBalance * 0.45f
            + morale * 0.15f
            + condition * 0.15f
            + stability * 0.10f
            + positionFit * 0.10f
            + specialTeamsFit * 0.05f));

        if (HasInvalidUnitPlayers(players, 4))
        {
            score = Mathf.Min(score, 35);
        }

        return BuildUnit(team, "PenaltyKill", unit == null ? 0 : unit.UnitNumber, "PK" + (unit == null ? 0 : unit.UnitNumber), playerIds, players, score, roleBalance, morale, condition, stability, positionFit, specialTeamsFit);
    }

    public static int CalculateMoraleScore(List<PlayerData> players)
    {
        if (players == null || players.Count == 0)
        {
            return MoraleConfig.DefaultMorale;
        }

        int total = 0;
        int count = 0;
        int penalty = 0;
        foreach (PlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            MoraleService.InitializePlayerMorale(player);
            total += player.Morale;
            count++;
            if (player.WantsTrade)
            {
                penalty += 5;
            }
        }

        return ChemistryConfig.ClampChemistry((count == 0 ? MoraleConfig.DefaultMorale : total / count) - penalty);
    }

    public static int CalculateConditionScore(List<PlayerData> players)
    {
        if (players == null || players.Count == 0)
        {
            return 100;
        }

        int total = 0;
        int count = 0;
        foreach (PlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            PlayerFatigueService.EnsureFatigueFields(player);
            InjuryService.EnsureInjuryFields(player);
            int condition = player.Condition <= 0 ? 100 : player.Condition;
            if (player.IsInjured)
            {
                condition -= 35;
            }

            total += Mathf.Clamp(condition, 0, 100);
            count++;
        }

        return ChemistryConfig.ClampChemistry(count == 0 ? 100 : total / count);
    }

    public static int CalculateStabilityScore(TeamData team, List<string> playerIds, string unitKey)
    {
        // TODO: Future: track games played together per unit.
        if (team == null)
        {
            return ChemistryConfig.DefaultChemistry;
        }

        LineChemistryData previous = FindPreviousUnit(team.Chemistry, unitKey);
        if (previous != null && SamePlayers(previous.PlayerIds, playerIds))
        {
            return 75;
        }

        return team.Lineup != null && team.Lineup.IsManual ? 65 : ChemistryConfig.DefaultChemistry;
    }

    public static int CalculateSpecialTeamsFitScore(List<PlayerData> players, string unitType)
    {
        if (players == null || players.Count == 0)
        {
            return ChemistryConfig.DefaultChemistry;
        }

        int score = 60;
        foreach (PlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            PlayerRoleService.EnsureRole(player);
            if (unitType == "PowerPlay")
            {
                if (player.PlayerRole == PlayerRoleConfig.Sniper || player.PlayerRole == PlayerRoleConfig.Playmaker || player.PlayerRole == PlayerRoleConfig.OffensiveDefenseman)
                {
                    score += 5;
                }
                else if (player.PlayerRole == PlayerRoleConfig.Grinder || player.PlayerRole == PlayerRoleConfig.DepthForward || player.PlayerRole == PlayerRoleConfig.StayAtHomeDefenseman)
                {
                    score -= 3;
                }
            }
            else if (unitType == "PenaltyKill")
            {
                if (player.PlayerRole == PlayerRoleConfig.TwoWayForward
                    || player.PlayerRole == PlayerRoleConfig.Grinder
                    || player.PlayerRole == PlayerRoleConfig.DefensiveDefenseman
                    || player.PlayerRole == PlayerRoleConfig.TwoWayDefenseman
                    || player.PlayerRole == PlayerRoleConfig.StayAtHomeDefenseman)
                {
                    score += 5;
                }
                else if (player.PlayerRole == PlayerRoleConfig.Sniper || player.PlayerRole == PlayerRoleConfig.OffensiveDefenseman)
                {
                    score -= 3;
                }
            }
        }

        return ChemistryConfig.ClampChemistry(score);
    }

    public static int CalculateUnitChemistryScore(int roleBalance, int morale, int condition, int stability, int positionFit, int specialTeamsFit)
    {
        return ChemistryConfig.ClampChemistry(Mathf.RoundToInt(
            roleBalance * 0.35f
            + morale * 0.18f
            + condition * 0.15f
            + stability * 0.10f
            + positionFit * 0.14f
            + specialTeamsFit * 0.08f));
    }

    public static int GetTeamChemistryRatingModifier(TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        if (team.Chemistry == null)
        {
            ApplyChemistryToTeamData(team, CalculateTeamChemistry(team));
        }

        return ChemistryConfig.GetTeamRatingModifier(team.Chemistry.TeamChemistryScore);
    }

    public static List<PlayerData> ResolvePlayers(TeamData team, List<string> playerIds)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (playerIds == null)
        {
            return players;
        }

        foreach (string playerId in playerIds)
        {
            PlayerData player = FindPlayer(team, playerId);
            if (player != null)
            {
                players.Add(player);
            }
        }

        return players;
    }

    public static PlayerData FindPlayer(TeamData team, string playerId)
    {
        if (team == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static void ApplyChemistryToTeamData(TeamData team, TeamChemistryData chemistry)
    {
        if (team == null || chemistry == null)
        {
            return;
        }

        team.Chemistry = chemistry;
        team.Chemistry.EnsureCollections();
        if (team.Lineup != null)
        {
            team.Lineup.TeamChemistryScore = chemistry.TeamChemistryScore;
            team.Lineup.TeamChemistryLabel = chemistry.TeamChemistryLabel;
            team.Lineup.LastChemistryUpdateUtc = DateTime.UtcNow.ToString("o");
        }
    }

    private static LineChemistryData BuildUnit(TeamData team, string unitType, int unitNumber, string unitName, List<string> playerIds, List<PlayerData> players, int score, int roleBalance, int morale, int condition, int stability, int positionFit, int specialTeamsFit)
    {
        LineChemistryData chemistry = new LineChemistryData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            UnitType = unitType,
            UnitNumber = unitNumber,
            UnitName = unitName,
            ChemistryScore = ChemistryConfig.ClampChemistry(score),
            RoleBalanceScore = ChemistryConfig.ClampChemistry(roleBalance),
            MoraleScore = ChemistryConfig.ClampChemistry(morale),
            ConditionScore = ChemistryConfig.ClampChemistry(condition),
            StabilityScore = ChemistryConfig.ClampChemistry(stability),
            PositionFitScore = ChemistryConfig.ClampChemistry(positionFit),
            SpecialTeamsFitScore = ChemistryConfig.ClampChemistry(specialTeamsFit),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        chemistry.ChemistryLabel = ChemistryConfig.GetChemistryLabel(chemistry.ChemistryScore);
        chemistry.ChemistrySummary = ChemistryConfig.BuildChemistrySummary(chemistry.ChemistryScore, chemistry.RoleBalanceScore, chemistry.MoraleScore, chemistry.ConditionScore, chemistry.StabilityScore);
        chemistry.EnsureCollections();
        if (playerIds != null)
        {
            chemistry.PlayerIds.AddRange(playerIds);
        }

        if (players != null)
        {
            foreach (PlayerData player in players)
            {
                chemistry.PlayerNames.Add(GetPlayerName(player));
            }
        }

        return chemistry;
    }

    private static List<string> GetPowerPlayIds(PowerPlayUnitData unit)
    {
        List<string> ids = new List<string>();
        if (unit == null)
        {
            return ids;
        }

        ids.Add(unit.Player1Id);
        ids.Add(unit.Player2Id);
        ids.Add(unit.Player3Id);
        ids.Add(unit.Player4Id);
        ids.Add(unit.Player5Id);
        return ids;
    }

    private static List<string> GetPenaltyKillIds(PenaltyKillUnitData unit)
    {
        List<string> ids = new List<string>();
        if (unit == null)
        {
            return ids;
        }

        ids.Add(unit.Player1Id);
        ids.Add(unit.Player2Id);
        ids.Add(unit.Player3Id);
        ids.Add(unit.Player4Id);
        return ids;
    }

    private static bool HasInvalidUnitPlayers(List<PlayerData> players, int expectedCount)
    {
        if (players == null || players.Count < expectedCount)
        {
            return true;
        }

        foreach (PlayerData player in players)
        {
            if (player == null || !RosterStatusConfig.IsNhlRoster(player) || !InjuryService.IsPlayerAvailable(player))
            {
                return true;
            }
        }

        return false;
    }

    private static int AverageChemistry(List<LineChemistryData> units)
    {
        if (units == null || units.Count == 0)
        {
            return ChemistryConfig.DefaultChemistry;
        }

        int total = 0;
        int count = 0;
        foreach (LineChemistryData unit in units)
        {
            if (unit == null)
            {
                continue;
            }

            total += unit.ChemistryScore;
            count++;
        }

        return count == 0 ? ChemistryConfig.DefaultChemistry : total / count;
    }

    private static List<LineChemistryData> CombineUnits(List<LineChemistryData> first, List<LineChemistryData> second)
    {
        List<LineChemistryData> units = new List<LineChemistryData>();
        if (first != null)
        {
            units.AddRange(first);
        }

        if (second != null)
        {
            units.AddRange(second);
        }

        return units;
    }

    private static int CalculateTeamMoraleAverage(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return MoraleConfig.DefaultMorale;
        }

        int total = 0;
        int count = 0;
        foreach (PlayerData player in team.Players)
        {
            if (player == null || !RosterStatusConfig.IsNhlRoster(player))
            {
                continue;
            }

            MoraleService.InitializePlayerMorale(player);
            total += player.Morale;
            count++;
        }

        return count == 0 ? MoraleConfig.DefaultMorale : total / count;
    }

    private static void FillBestAndWorst(TeamChemistryData chemistry)
    {
        foreach (LineChemistryData unit in CombineUnits(CombineUnits(chemistry.ForwardLines, chemistry.DefensePairs), CombineUnits(chemistry.PowerPlayUnits, chemistry.PenaltyKillUnits)))
        {
            if (unit == null)
            {
                continue;
            }

            if (unit.ChemistryScore > chemistry.BestUnitScore)
            {
                chemistry.BestUnitScore = unit.ChemistryScore;
                chemistry.BestUnitName = unit.UnitName;
            }

            if (unit.ChemistryScore < chemistry.WorstUnitScore)
            {
                chemistry.WorstUnitScore = unit.ChemistryScore;
                chemistry.WorstUnitName = unit.UnitName;
            }
        }

        if (string.IsNullOrEmpty(chemistry.BestUnitName))
        {
            chemistry.BestUnitScore = 0;
            chemistry.BestUnitName = "none";
        }

        if (string.IsNullOrEmpty(chemistry.WorstUnitName))
        {
            chemistry.WorstUnitScore = 0;
            chemistry.WorstUnitName = "none";
        }
    }

    private static string BuildTeamSummary(TeamChemistryData chemistry)
    {
        if (chemistry == null)
        {
            return "";
        }

        return ChemistryConfig.GetChemistryLabel(chemistry.TeamChemistryScore)
            + " chemistry | F " + chemistry.ForwardChemistryAverage
            + " | D " + chemistry.DefenseChemistryAverage
            + " | ST " + chemistry.SpecialTeamsChemistryAverage;
    }

    private static string BuildLeadershipSummary(TeamData team)
    {
        TeamLeadershipData leadership = team == null ? null : team.LeadershipData;
        if (leadership == null && team != null)
        {
            leadership = LeadershipService.CalculateTeamLeadership(team);
        }

        if (leadership == null || string.IsNullOrEmpty(leadership.LeadershipSummary))
        {
            return "";
        }

        return " | " + leadership.LeadershipSummary;
    }

    private static string BuildStaffSummary(TeamData team)
    {
        if (team == null)
        {
            return "";
        }

        StaffEffectSummaryData summary = CoachingStaffService.BuildStaffEffectSummary(team);
        if (summary == null || string.IsNullOrEmpty(summary.HeadCoachName))
        {
            return "";
        }

        return " | Coach chemistry " + FormatSigned(summary.ChemistryModifier)
            + " (" + summary.HeadCoachName + ")";
    }

    private static string FormatSigned(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }

    private static void AddUnit(List<LineChemistryData> units, LineChemistryData unit)
    {
        if (units != null && unit != null)
        {
            units.Add(unit);
        }
    }

    private static LineChemistryData FindPreviousUnit(TeamChemistryData chemistry, string unitKey)
    {
        if (chemistry == null || string.IsNullOrEmpty(unitKey))
        {
            return null;
        }

        chemistry.EnsureCollections();
        foreach (LineChemistryData unit in CombineUnits(CombineUnits(chemistry.ForwardLines, chemistry.DefensePairs), CombineUnits(chemistry.PowerPlayUnits, chemistry.PenaltyKillUnits)))
        {
            if (unit != null && unit.UnitType + ":" + unit.UnitNumber == unitKey)
            {
                return unit;
            }
        }

        return null;
    }

    private static bool SamePlayers(List<string> left, List<string> right)
    {
        if (left == null || right == null || left.Count != right.Count)
        {
            return false;
        }

        for (int i = 0; i < left.Count; i++)
        {
            if (left[i] != right[i])
            {
                return false;
            }
        }

        return true;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : player.FirstName + " " + player.LastName;
    }
}
