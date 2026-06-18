using System.Collections.Generic;
using UnityEngine;

public static class PlayerGameStatsGenerator
{
    public static List<PlayerGameStatData> Generate(MatchResultData result, TeamData homeTeam, TeamData awayTeam)
    {
        List<PlayerGameStatData> gameStats = new List<PlayerGameStatData>();

        if (result == null || homeTeam == null || awayTeam == null)
        {
            return gameStats;
        }

        EnsurePlayers(homeTeam);
        EnsurePlayers(awayTeam);
        PlayerFatigueService.EnsureFatigueForTeam(homeTeam);
        PlayerFatigueService.EnsureFatigueForTeam(awayTeam);
        InjuryService.EnsureInjuryFieldsForTeam(homeTeam);
        InjuryService.EnsureInjuryFieldsForTeam(awayTeam);
        PlayerRoleService.EnsureRolesForTeam(homeTeam);
        PlayerRoleService.EnsureRolesForTeam(awayTeam);
        IceTimeService.EnsureUsageForTeam(homeTeam);
        IceTimeService.EnsureUsageForTeam(awayTeam);
        ChemistryService.EnsureChemistryForTeam(homeTeam);
        ChemistryService.EnsureChemistryForTeam(awayTeam);
        result.EnsurePlayerStats();

        TeamGameStatContext homeContext = CreateTeamContext(result, homeTeam, true);
        TeamGameStatContext awayContext = CreateTeamContext(result, awayTeam, false);
        List<SimulatedGoalEvent> goalEvents = new List<SimulatedGoalEvent>();
        List<SimulatedPenaltyEvent> penaltyEvents = new List<SimulatedPenaltyEvent>();

        AddContextStats(homeContext, gameStats);
        AddContextStats(awayContext, gameStats);

        AssignGoalsAndAssists(homeContext, awayContext, goalEvents);
        AssignGoalsAndAssists(awayContext, homeContext, goalEvents);
        AssignShots(homeContext);
        AssignShots(awayContext);
        AssignPenaltyMinutes(homeContext, penaltyEvents);
        AssignPenaltyMinutes(awayContext, penaltyEvents);
        result.Events = BuildMatchEvents(result, goalEvents, penaltyEvents);

        foreach (PlayerGameStatData stat in gameStats)
        {
            stat.Points = stat.Goals + stat.Assists;
            stat.PowerPlayPoints = stat.PowerPlayGoals + stat.PowerPlayAssists;
            stat.ShortHandedPoints = stat.ShortHandedGoals + stat.ShortHandedAssists;
        }

        IncrementNhlGamesPlayed(homeTeam, gameStats);
        IncrementNhlGamesPlayed(awayTeam, gameStats);

        return gameStats;
    }

    private static TeamGameStatContext CreateTeamContext(MatchResultData result, TeamData team, bool isHomeTeam)
    {
        int teamGoals = isHomeTeam ? result.HomeScore : result.AwayScore;
        int teamPowerPlayGoals = isHomeTeam ? result.HomePowerPlayGoals : result.AwayPowerPlayGoals;
        int teamPenaltyMinutes = isHomeTeam ? result.HomePenaltyMinutes : result.AwayPenaltyMinutes;
        int teamShots = isHomeTeam ? result.HomeShots : result.AwayShots;
        bool teamWon = result.WinnerTeamId == team.Id;
        List<PlayerData> skaters = GetSkaters(team);
        PlayerData startingGoalie = GetStartingGoalie(team);

        TeamGameStatContext context = new TeamGameStatContext
        {
            Team = team,
            IsHomeTeam = isHomeTeam,
            Skaters = skaters,
            StartingGoalie = startingGoalie,
            TeamGoals = teamGoals,
            TeamPowerPlayGoals = teamPowerPlayGoals,
            TeamPenaltyMinutes = teamPenaltyMinutes,
            TeamShots = teamShots,
            TeamWon = teamWon
        };

        List<PlayerGameStatData> skaterStats = new List<PlayerGameStatData>();
        foreach (PlayerData skater in skaters)
        {
            PlayerGameStatData stat = CreateBaseStat(skater, false);
            skaterStats.Add(stat);
        }

        context.SkaterStats = skaterStats;

        if (startingGoalie != null)
        {
            PlayerGameStatData goalieStat = CreateBaseStat(startingGoalie, true);
            int shotsAgainst = isHomeTeam ? result.AwayShots : result.HomeShots;
            int goalsAgainst = isHomeTeam ? result.AwayScore : result.HomeScore;

            goalieStat.ShotsAgainst = shotsAgainst;
            goalieStat.GoalsAgainst = goalsAgainst;
            goalieStat.Saves = Mathf.Max(0, shotsAgainst - goalsAgainst);
            goalieStat.GoalieWin = teamWon;
            goalieStat.GoalieOvertimeLoss = !teamWon && result.IsOvertime;
            goalieStat.GoalieLoss = !teamWon && !result.IsOvertime;
            goalieStat.Shutout = goalsAgainst == 0;
            context.GoalieStat = goalieStat;
        }

        return context;
    }

    private static void AddContextStats(TeamGameStatContext context, List<PlayerGameStatData> gameStats)
    {
        if (context == null || gameStats == null)
        {
            return;
        }

        foreach (PlayerGameStatData stat in context.SkaterStats)
        {
            gameStats.Add(stat);
        }

        if (context.GoalieStat != null)
        {
            gameStats.Add(context.GoalieStat);
        }
    }

    private static void AssignGoalsAndAssists(
        TeamGameStatContext scoringContext,
        TeamGameStatContext defendingContext,
        List<SimulatedGoalEvent> goalEvents)
    {
        TeamData team = scoringContext.Team;
        int goals = scoringContext.TeamGoals;
        int powerPlayGoals = scoringContext.TeamPowerPlayGoals;
        List<PlayerData> skaters = scoringContext.Skaters;
        List<PlayerGameStatData> skaterStats = scoringContext.SkaterStats;

        for (int i = 0; i < goals; i++)
        {
            bool isPowerPlayGoal = i < powerPlayGoals;
            PlayerData scorer = isPowerPlayGoal
                ? PickWeightedPowerPlaySkater(team, skaters, null, "Goal")
                : PickWeightedSkater(team, skaters, null, "Goal");
            if (scorer == null)
            {
                return;
            }

            PlayerGameStatData scorerStat = FindStat(skaterStats, scorer.Id);
            if (scorerStat == null)
            {
                continue;
            }

            scorerStat.Goals++;
            if (isPowerPlayGoal)
            {
                scorerStat.PowerPlayGoals++;
            }

            int assistCount = RollAssistCount();
            List<string> excludedPlayerIds = new List<string> { scorer.Id };
            PlayerData assist1 = null;
            PlayerData assist2 = null;

            for (int assistIndex = 0; assistIndex < assistCount; assistIndex++)
            {
                PlayerData assister = isPowerPlayGoal
                    ? PickWeightedPowerPlaySkater(team, skaters, excludedPlayerIds, "Assist")
                    : PickWeightedSkater(team, skaters, excludedPlayerIds, "Assist");
                if (assister == null)
                {
                    break;
                }

                PlayerGameStatData assisterStat = FindStat(skaterStats, assister.Id);
                if (assisterStat != null)
                {
                    assisterStat.Assists++;
                    if (isPowerPlayGoal)
                    {
                        assisterStat.PowerPlayAssists++;
                    }
                }

                if (assistIndex == 0)
                {
                    assist1 = assister;
                }
                else if (assistIndex == 1)
                {
                    assist2 = assister;
                }

                excludedPlayerIds.Add(assister.Id);
            }

            if (!isPowerPlayGoal)
            {
                ApplyEvenStrengthPlusMinus(scoringContext, defendingContext, scorer, assist1, assist2);
            }

            if (goalEvents != null)
            {
                goalEvents.Add(new SimulatedGoalEvent
                {
                    IsHomeTeam = scoringContext.IsHomeTeam,
                    Team = scoringContext.Team,
                    Scorer = scorer,
                    Assist1 = assist1,
                    Assist2 = assist2,
                    IsPowerPlayGoal = isPowerPlayGoal,
                    GameSecondsElapsed = Random.Range(0, LiveMatchConfig.RegulationPeriods * LiveMatchConfig.RegulationPeriodSeconds)
                });
            }
        }
    }

    private static void AssignPenaltyMinutes(TeamGameStatContext context, List<SimulatedPenaltyEvent> penaltyEvents)
    {
        TeamData team = context.Team;
        int penaltyMinutes = context.TeamPenaltyMinutes;
        List<PlayerData> skaters = context.Skaters;
        List<PlayerGameStatData> skaterStats = context.SkaterStats;
        int remainingMinutes = Mathf.Max(0, penaltyMinutes);
        while (remainingMinutes > 0)
        {
            int minutes = remainingMinutes >= 5 && Random.value < 0.20f ? 5 : 2;
            if (remainingMinutes < minutes)
            {
                minutes = remainingMinutes;
            }

            PlayerData player = PickWeightedSkater(team, skaters, null, "Penalty");
            PlayerGameStatData stat = player == null ? null : FindStat(skaterStats, player.Id);
            if (stat != null)
            {
                stat.PenaltyMinutes += minutes;
            }

            if (penaltyEvents != null && player != null)
            {
                penaltyEvents.Add(new SimulatedPenaltyEvent
                {
                    Team = team,
                    Offender = player,
                    Reason = GetPenaltyReason(),
                    Minutes = minutes,
                    GameSecondsElapsed = Random.Range(0, LiveMatchConfig.RegulationPeriods * LiveMatchConfig.RegulationPeriodSeconds)
                });
            }

            remainingMinutes -= minutes;
        }
    }

    private static void AssignShots(TeamGameStatContext context)
    {
        TeamData team = context.Team;
        int shots = context.TeamShots;
        List<PlayerData> skaters = context.Skaters;
        List<PlayerGameStatData> skaterStats = context.SkaterStats;
        for (int i = 0; i < shots; i++)
        {
            PlayerData shooter = PickWeightedSkater(team, skaters, null, "Shot");
            if (shooter == null)
            {
                return;
            }

            PlayerGameStatData shooterStat = FindStat(skaterStats, shooter.Id);
            if (shooterStat != null)
            {
                shooterStat.Shots++;
            }
        }
    }

    private static int RollAssistCount()
    {
        float roll = Random.value;
        if (roll < 0.65f)
        {
            return 2;
        }

        return roll < 0.90f ? 1 : 0;
    }

    private static void ApplyEvenStrengthPlusMinus(
        TeamGameStatContext scoringContext,
        TeamGameStatContext defendingContext,
        PlayerData scorer,
        PlayerData assist1,
        PlayerData assist2)
    {
        List<PlayerData> scoringPlayers = PickEvenStrengthSkaters(scoringContext, scorer, assist1, assist2);
        List<PlayerData> defendingPlayers = PickEvenStrengthSkaters(defendingContext, null, null, null);

        foreach (PlayerData player in scoringPlayers)
        {
            PlayerGameStatData stat = FindStat(scoringContext.SkaterStats, player == null ? "" : player.Id);
            if (stat != null)
            {
                stat.PlusMinus++;
            }
        }

        foreach (PlayerData player in defendingPlayers)
        {
            PlayerGameStatData stat = FindStat(defendingContext.SkaterStats, player == null ? "" : player.Id);
            if (stat != null)
            {
                stat.PlusMinus--;
            }
        }
    }

    private static List<PlayerData> PickEvenStrengthSkaters(
        TeamGameStatContext context,
        PlayerData locked1,
        PlayerData locked2,
        PlayerData locked3)
    {
        List<PlayerData> players = new List<PlayerData>();
        AddUniquePlayer(players, locked1);
        AddUniquePlayer(players, locked2);
        AddUniquePlayer(players, locked3);

        List<string> excludedPlayerIds = new List<string>();
        foreach (PlayerData player in players)
        {
            if (player != null)
            {
                excludedPlayerIds.Add(player.Id);
            }
        }

        while (players.Count < LiveMatchConfig.RegulationSkaters)
        {
            PlayerData player = PickWeightedSkater(context.Team, context.Skaters, excludedPlayerIds, "Shift");
            if (player == null)
            {
                break;
            }

            players.Add(player);
            excludedPlayerIds.Add(player.Id);
        }

        return players;
    }

    private static void AddUniquePlayer(List<PlayerData> players, PlayerData player)
    {
        if (players == null || player == null || player.Position == "G")
        {
            return;
        }

        foreach (PlayerData existing in players)
        {
            if (existing != null && existing.Id == player.Id)
            {
                return;
            }
        }

        players.Add(player);
    }

    private static List<LiveMatchEventData> BuildMatchEvents(
        MatchResultData result,
        List<SimulatedGoalEvent> goalEvents,
        List<SimulatedPenaltyEvent> penaltyEvents)
    {
        List<LiveMatchEventData> events = new List<LiveMatchEventData>();
        if (goalEvents == null)
        {
            goalEvents = new List<SimulatedGoalEvent>();
        }

        if (penaltyEvents == null)
        {
            penaltyEvents = new List<SimulatedPenaltyEvent>();
        }

        goalEvents.Sort(CompareGoalEvents);
        penaltyEvents.Sort(ComparePenaltyEvents);

        int homeScore = 0;
        int awayScore = 0;
        foreach (SimulatedGoalEvent goalEvent in goalEvents)
        {
            if (goalEvent.IsHomeTeam)
            {
                homeScore++;
            }
            else
            {
                awayScore++;
            }

            LiveMatchEventData matchEvent = CreateTimedEvent(goalEvent.GameSecondsElapsed);
            matchEvent.EventType = "Goal";
            matchEvent.TeamId = goalEvent.Team == null ? "" : goalEvent.Team.Id;
            matchEvent.TeamName = TeamIdentityService.GetDisplayName(goalEvent.Team);
            matchEvent.PlayerId = goalEvent.Scorer == null ? "" : goalEvent.Scorer.Id;
            matchEvent.PlayerName = GetPlayerName(goalEvent.Scorer);
            matchEvent.Assist1PlayerId = goalEvent.Assist1 == null ? "" : goalEvent.Assist1.Id;
            matchEvent.Assist1PlayerName = GetPlayerName(goalEvent.Assist1);
            matchEvent.Assist2PlayerId = goalEvent.Assist2 == null ? "" : goalEvent.Assist2.Id;
            matchEvent.Assist2PlayerName = GetPlayerName(goalEvent.Assist2);
            matchEvent.HomeScoreAfter = homeScore;
            matchEvent.AwayScoreAfter = awayScore;
            matchEvent.Importance = 5;
            matchEvent.Description = FormatGoalDescription(
                matchEvent.TeamName,
                matchEvent.PlayerName,
                homeScore,
                awayScore,
                matchEvent.PeriodLabel,
                matchEvent.ClockLabel,
                goalEvent.IsPowerPlayGoal,
                false,
                matchEvent.Assist1PlayerName,
                matchEvent.Assist2PlayerName);
            events.Add(matchEvent);
        }

        foreach (SimulatedPenaltyEvent penaltyEvent in penaltyEvents)
        {
            LiveMatchEventData matchEvent = CreateTimedEvent(penaltyEvent.GameSecondsElapsed);
            matchEvent.EventType = "Penalty";
            matchEvent.TeamId = penaltyEvent.Team == null ? "" : penaltyEvent.Team.Id;
            matchEvent.TeamName = TeamIdentityService.GetDisplayName(penaltyEvent.Team);
            matchEvent.PlayerId = penaltyEvent.Offender == null ? "" : penaltyEvent.Offender.Id;
            matchEvent.PlayerName = GetPlayerName(penaltyEvent.Offender);
            matchEvent.Importance = 3;
            matchEvent.Description = "Удаление " + matchEvent.TeamName
                + ": " + matchEvent.PlayerName
                + ", " + penaltyEvent.Minutes + " минут (" + penaltyEvent.Reason + ")";
            events.Add(matchEvent);
        }

        events.Sort(CompareEventsAscending);
        return events;
    }

    public static string FormatGoalDescription(
        string teamName,
        string scorerName,
        int homeScore,
        int awayScore,
        string periodLabel,
        string clockLabel,
        bool isPowerPlayGoal,
        bool isShortHandedGoal,
        string assist1Name,
        string assist2Name)
    {
        string strengthText = "";
        if (isPowerPlayGoal)
        {
            strengthText = " в большинстве";
        }
        else if (isShortHandedGoal)
        {
            strengthText = " в меньшинстве";
        }

        string assists = FormatAssists(assist1Name, assist2Name);
        return periodLabel + " " + clockLabel
            + " \u0413\u041e\u041b " + teamName
            + " (" + homeScore + ":" + awayScore + ") "
            + scorerName
            + strengthText
            + assists;
    }

    public static string FormatAssists(string assist1Name, string assist2Name)
    {
        List<string> assists = new List<string>();
        if (!string.IsNullOrEmpty(assist1Name))
        {
            assists.Add(assist1Name);
        }

        if (!string.IsNullOrEmpty(assist2Name))
        {
            assists.Add(assist2Name);
        }

        if (assists.Count == 0)
        {
            return " (без ассистентов)";
        }

        return " (ассистенты: " + string.Join(", ", assists.ToArray()) + ")";
    }

    private static LiveMatchEventData CreateTimedEvent(int gameSecondsElapsed)
    {
        int regulationSeconds = LiveMatchConfig.RegulationPeriods * LiveMatchConfig.RegulationPeriodSeconds;
        gameSecondsElapsed = Mathf.Clamp(gameSecondsElapsed, 0, Mathf.Max(0, regulationSeconds - 1));
        int period = Mathf.Clamp(gameSecondsElapsed / LiveMatchConfig.RegulationPeriodSeconds + 1, 1, LiveMatchConfig.RegulationPeriods);
        int elapsedInPeriod = gameSecondsElapsed % LiveMatchConfig.RegulationPeriodSeconds;
        int secondsRemaining = LiveMatchConfig.RegulationPeriodSeconds - elapsedInPeriod;

        return new LiveMatchEventData
        {
            Period = period,
            PeriodLabel = FormatPeriodLabel(period),
            GameSecondsElapsed = gameSecondsElapsed,
            PeriodSecondsRemaining = secondsRemaining,
            ClockLabel = LiveMatchConfig.FormatClock(secondsRemaining)
        };
    }

    private static string FormatPeriodLabel(int period)
    {
        if (period == 1)
        {
            return "1st";
        }

        if (period == 2)
        {
            return "2nd";
        }

        return period == 3 ? "3rd" : "OT";
    }

    private static string GetPenaltyReason()
    {
        string[] reasons =
        {
            "подножка",
            "задержка соперника",
            "задержка клюшкой",
            "толчок клюшкой",
            "грубость",
            "атака игрока без шайбы",
            "удар клюшкой"
        };

        return reasons[Random.Range(0, reasons.Length)];
    }

    private static int CompareGoalEvents(SimulatedGoalEvent left, SimulatedGoalEvent right)
    {
        return left.GameSecondsElapsed.CompareTo(right.GameSecondsElapsed);
    }

    private static int ComparePenaltyEvents(SimulatedPenaltyEvent left, SimulatedPenaltyEvent right)
    {
        return left.GameSecondsElapsed.CompareTo(right.GameSecondsElapsed);
    }

    private static int CompareEventsAscending(LiveMatchEventData left, LiveMatchEventData right)
    {
        int timeComparison = left.GameSecondsElapsed.CompareTo(right.GameSecondsElapsed);
        if (timeComparison != 0)
        {
            return timeComparison;
        }

        return right.Importance.CompareTo(left.Importance);
    }

    private static PlayerGameStatData CreateBaseStat(PlayerData player, bool isGoalie)
    {
        return new PlayerGameStatData
        {
            PlayerId = player.Id,
            TeamId = player.TeamId,
            PlayerName = GetPlayerName(player),
            Position = player.Position,
            IsGoalie = isGoalie,
            TimeOnIceSeconds = GetStatTimeOnIce(player, isGoalie)
        };
    }

    private static List<PlayerData> GetSkaters(TeamData team)
    {
        LineupService.EnsureLineup(team);
        List<PlayerData> skaters = LineupService.GetActiveSkaters(team);
        skaters = FilterAvailablePlayers(skaters);
        if (skaters.Count > 0)
        {
            return skaters;
        }

        return GetFallbackSkaters(team);
    }

    private static PlayerData GetStartingGoalie(TeamData team)
    {
        LineupService.EnsureLineup(team);
        PlayerData goalie = LineupService.GetStartingGoalie(team);
        if (InjuryService.IsPlayerAvailable(goalie))
        {
            return goalie;
        }

        PlayerData backup = LineupService.GetBackupGoalie(team);
        if (InjuryService.IsPlayerAvailable(backup))
        {
            return backup;
        }

        List<PlayerData> goalies = new List<PlayerData>();
        foreach (PlayerData player in team.Players)
        {
            if (player != null
                && RosterStatusConfig.IsNhlRoster(player)
                && player.Position == "G"
                && InjuryService.IsPlayerAvailable(player))
            {
                goalies.Add(player);
            }
        }

        goalies.Sort((left, right) => right.Overall.CompareTo(left.Overall));
        return goalies.Count == 0 ? null : goalies[0];
    }

    private static PlayerData PickWeightedSkater(TeamData team, List<PlayerData> skaters, List<string> excludedPlayerIds, string context)
    {
        int totalWeight = 0;

        foreach (PlayerData skater in skaters)
        {
            if (IsExcluded(skater, excludedPlayerIds))
            {
                continue;
            }

            totalWeight += GetSkaterWeight(team, skater, context);
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = Random.Range(0, totalWeight);
        foreach (PlayerData skater in skaters)
        {
            if (IsExcluded(skater, excludedPlayerIds))
            {
                continue;
            }

            roll -= GetSkaterWeight(team, skater, context);
            if (roll < 0)
            {
                return skater;
            }
        }

        return null;
    }

    private static PlayerData PickWeightedPowerPlaySkater(TeamData team, List<PlayerData> fallbackSkaters, List<string> excludedPlayerIds, string context)
    {
        List<PlayerData> powerPlayPlayers = SpecialTeamsService.GetPowerPlayPlayers(team);
        if (powerPlayPlayers.Count == 0)
        {
            return PickWeightedSkater(team, fallbackSkaters, excludedPlayerIds, context);
        }

        int totalWeight = 0;
        foreach (PlayerData player in powerPlayPlayers)
        {
            if (IsExcluded(player, excludedPlayerIds) || !InjuryService.IsPlayerAvailable(player))
            {
                continue;
            }

            totalWeight += GetPowerPlayWeight(team, player, context);
        }

        if (totalWeight <= 0)
        {
            return PickWeightedSkater(team, fallbackSkaters, excludedPlayerIds, context);
        }

        int roll = Random.Range(0, totalWeight);
        foreach (PlayerData player in powerPlayPlayers)
        {
            if (IsExcluded(player, excludedPlayerIds) || !InjuryService.IsPlayerAvailable(player))
            {
                continue;
            }

            roll -= GetPowerPlayWeight(team, player, context);
            if (roll < 0)
            {
                return player;
            }
        }

        return PickWeightedSkater(team, fallbackSkaters, excludedPlayerIds, context);
    }

    private static PlayerData PickRandomSkater(List<PlayerData> skaters)
    {
        if (skaters == null || skaters.Count == 0)
        {
            return null;
        }

        return skaters[Random.Range(0, skaters.Count)];
    }

    private static int GetSkaterWeight(TeamData team, PlayerData player, string context)
    {
        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            return 0;
        }

        int positionBonus = player.Position == "D" ? 12 : 32;
        int usageWeight = team == null ? 0 : IceTimeService.GetUsageWeightForStats(team, player);
        if (usageWeight <= 0)
        {
            usageWeight = GetLineupWeightForPlayer(team, player);
        }

        if (usageWeight <= 0)
        {
            return 0;
        }

        float roleModifier = GetRoleModifier(player, context);
        float chemistryModifier = GetLineChemistryStatsModifier(team, player);
        int weight = Mathf.RoundToInt(usageWeight * (PlayerFatigueService.GetEffectiveOverall(player) + positionBonus) * roleModifier * chemistryModifier);
        return Mathf.Max(1, weight);
    }

    private static int GetPowerPlayWeight(TeamData team, PlayerData player, string context)
    {
        if (!RosterStatusConfig.IsNhlRoster(player) || !LineupService.IsPlayerActive(team, player.Id))
        {
            return 0;
        }

        int unitNumber = SpecialTeamsService.GetPowerPlayUnitNumberForPlayer(team, player.Id);
        int unitWeight = unitNumber == 1 ? 5 : 3;
        int positionBonus = player.Position == "D" ? 8 : 28;
        float roleModifier = GetRoleModifier(player, context);
        float chemistryModifier = GetPowerPlayChemistryStatsModifier(team, player);
        return Mathf.Max(1, Mathf.RoundToInt(unitWeight * (PlayerFatigueService.GetEffectiveOverall(player) + positionBonus) * roleModifier * chemistryModifier));
    }

    private static float GetLineChemistryStatsModifier(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return 1f;
        }

        if (team.Chemistry == null)
        {
            ChemistryService.EnsureChemistryForTeam(team);
        }

        TeamChemistryData chemistry = team.Chemistry;
        if (chemistry == null)
        {
            return 1f;
        }

        chemistry.EnsureCollections();
        foreach (LineChemistryData unit in chemistry.ForwardLines)
        {
            if (ContainsPlayer(unit, player.Id))
            {
                return GetChemistryStatsModifier(unit.ChemistryScore);
            }
        }

        foreach (LineChemistryData unit in chemistry.DefensePairs)
        {
            if (ContainsPlayer(unit, player.Id))
            {
                return GetChemistryStatsModifier(unit.ChemistryScore);
            }
        }

        return 1f;
    }

    private static float GetPowerPlayChemistryStatsModifier(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return 1f;
        }

        if (team.Chemistry == null)
        {
            ChemistryService.EnsureChemistryForTeam(team);
        }

        TeamChemistryData chemistry = team.Chemistry;
        if (chemistry == null)
        {
            return 1f;
        }

        chemistry.EnsureCollections();
        foreach (LineChemistryData unit in chemistry.PowerPlayUnits)
        {
            if (ContainsPlayer(unit, player.Id))
            {
                return GetChemistryStatsModifier(unit.ChemistryScore);
            }
        }

        return GetLineChemistryStatsModifier(team, player);
    }

    private static float GetChemistryStatsModifier(int score)
    {
        string label = ChemistryConfig.GetChemistryLabel(score);
        if (label == ChemistryConfig.LabelExcellent)
        {
            return 1.05f;
        }

        if (label == ChemistryConfig.LabelGood)
        {
            return 1.02f;
        }

        if (label == ChemistryConfig.LabelPoor)
        {
            return 0.97f;
        }

        return label == ChemistryConfig.LabelBad ? 0.94f : 1f;
    }

    private static bool ContainsPlayer(LineChemistryData unit, string playerId)
    {
        return unit != null
            && unit.PlayerIds != null
            && !string.IsNullOrEmpty(playerId)
            && unit.PlayerIds.Contains(playerId);
    }

    private static int GetStatTimeOnIce(PlayerData player, bool isGoalie)
    {
        if (player == null)
        {
            return 0;
        }

        int seconds = player.LastGameTimeOnIceSeconds > 0
            ? player.LastGameTimeOnIceSeconds
            : player.EstimatedTimeOnIceSeconds;

        if (isGoalie && seconds <= 0)
        {
            return IceTimeConfig.StartingGoalieSeconds;
        }

        return seconds;
    }

    private static float GetRoleModifier(PlayerData player, string context)
    {
        if (context == "Goal")
        {
            return PlayerRoleService.GetGoalScoringModifier(player);
        }

        if (context == "Assist")
        {
            return PlayerRoleService.GetAssistModifier(player);
        }

        if (context == "Shot")
        {
            return PlayerRoleService.GetShotModifier(player);
        }

        if (context == "Penalty")
        {
            return PlayerRoleService.GetPenaltyModifier(player);
        }

        return 1f;
    }

    private static int GetLineupWeightForPlayer(TeamData team, PlayerData player)
    {
        if (team == null || team.Lineup == null || player == null)
        {
            return 1;
        }

        team.Lineup.EnsureCollections();
        foreach (ForwardLineData line in team.Lineup.ForwardLines)
        {
            if (line == null)
            {
                continue;
            }

            if (line.LeftWingPlayerId == player.Id || line.CenterPlayerId == player.Id || line.RightWingPlayerId == player.Id)
            {
                if (line.LineNumber == 1)
                {
                    return 5;
                }

                if (line.LineNumber == 2)
                {
                    return 4;
                }

                if (line.LineNumber == 3)
                {
                    return 3;
                }

                return 2;
            }
        }

        foreach (DefensePairData pair in team.Lineup.DefensePairs)
        {
            if (pair == null)
            {
                continue;
            }

            if (pair.LeftDefensePlayerId == player.Id || pair.RightDefensePlayerId == player.Id)
            {
                if (pair.PairNumber == 1)
                {
                    return 4;
                }

                if (pair.PairNumber == 2)
                {
                    return 3;
                }

                return 2;
            }
        }

        return 1;
    }

    private static List<PlayerData> GetFallbackSkaters(TeamData team)
    {
        List<PlayerData> skaters = new List<PlayerData>();
        if (team == null || team.Players == null)
        {
            return skaters;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null
                && RosterStatusConfig.IsNhlRoster(player)
                && player.Position != "G"
                && InjuryService.IsPlayerAvailable(player))
            {
                skaters.Add(player);
            }
        }

        skaters.Sort((left, right) => right.Overall.CompareTo(left.Overall));
        if (skaters.Count > LineupConfig.ActiveSkaterCount)
        {
            skaters.RemoveRange(LineupConfig.ActiveSkaterCount, skaters.Count - LineupConfig.ActiveSkaterCount);
        }

        return skaters;
    }

    private static bool IsExcluded(PlayerData player, List<string> excludedPlayerIds)
    {
        if (player == null || !RosterStatusConfig.IsNhlRoster(player) || !InjuryService.IsPlayerAvailable(player))
        {
            return true;
        }

        if (excludedPlayerIds == null)
        {
            return false;
        }

        return excludedPlayerIds.Contains(player.Id);
    }

    private static PlayerGameStatData FindStat(List<PlayerGameStatData> stats, string playerId)
    {
        foreach (PlayerGameStatData stat in stats)
        {
            if (stat.PlayerId == playerId)
            {
                return stat;
            }
        }

        return null;
    }

    private static void EnsurePlayers(TeamData team)
    {
        team.EnsurePlayers();

        if (team.Players.Count == 0)
        {
            team.Players = PlayerSeedData.CreatePlayersForTeam(team.Id);
        }

        PlayerFatigueService.EnsureFatigueForTeam(team);
        InjuryService.EnsureInjuryFieldsForTeam(team);
        TeamRosterService.EnsureRosterStatusesForTeam(team);
    }

    private static List<PlayerData> FilterAvailablePlayers(List<PlayerData> players)
    {
        List<PlayerData> availablePlayers = new List<PlayerData>();
        if (players == null)
        {
            return availablePlayers;
        }

        foreach (PlayerData player in players)
        {
            if (RosterStatusConfig.IsNhlRoster(player) && InjuryService.IsPlayerAvailable(player))
            {
                availablePlayers.Add(player);
            }
        }

        return availablePlayers;
    }

    private static void IncrementNhlGamesPlayed(TeamData team, List<PlayerGameStatData> gameStats)
    {
        if (team == null || gameStats == null)
        {
            return;
        }

        HashSet<string> countedPlayerIds = new HashSet<string>();
        foreach (PlayerGameStatData stat in gameStats)
        {
            if (stat == null || stat.TeamId != team.Id || string.IsNullOrEmpty(stat.PlayerId) || countedPlayerIds.Contains(stat.PlayerId))
            {
                continue;
            }

            PlayerData player = FindPlayer(team, stat.PlayerId);
            if (RosterStatusConfig.IsNhlRoster(player))
            {
                player.NHLGamesThisSeason++;
                countedPlayerIds.Add(stat.PlayerId);
            }
        }
    }

    private static PlayerData FindPlayer(TeamData team, string playerId)
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

    private static string GetPlayerName(PlayerData player)
    {
        if (player == null)
        {
            return "";
        }

        string number = player.JerseyNumber > 0 ? "#" + player.JerseyNumber + " " : "";
        return number + player.FirstName + " " + player.LastName;
    }

    private class TeamGameStatContext
    {
        public TeamData Team;
        public bool IsHomeTeam;
        public List<PlayerData> Skaters = new List<PlayerData>();
        public PlayerData StartingGoalie;
        public int TeamGoals;
        public int TeamPowerPlayGoals;
        public int TeamPenaltyMinutes;
        public int TeamShots;
        public bool TeamWon;
        public List<PlayerGameStatData> SkaterStats = new List<PlayerGameStatData>();
        public PlayerGameStatData GoalieStat;
    }

    private class SimulatedGoalEvent
    {
        public bool IsHomeTeam;
        public TeamData Team;
        public PlayerData Scorer;
        public PlayerData Assist1;
        public PlayerData Assist2;
        public bool IsPowerPlayGoal;
        public int GameSecondsElapsed;
    }

    private class SimulatedPenaltyEvent
    {
        public TeamData Team;
        public PlayerData Offender;
        public string Reason;
        public int Minutes;
        public int GameSecondsElapsed;
    }
}
