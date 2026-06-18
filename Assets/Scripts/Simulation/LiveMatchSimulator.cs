using System;
using System.Collections.Generic;
using UnityEngine;

public static class LiveMatchSimulator
{
    public static LiveMatchStateData CreateLiveMatch(
        ScheduleGameData scheduledGame,
        TeamData homeTeam,
        TeamData awayTeam,
        string userTeamId,
        bool isPlayoffGame)
    {
        PrepareTeam(homeTeam);
        PrepareTeam(awayTeam);

        PlayerData homeGoalie = LineupService.GetStartingGoalie(homeTeam);
        PlayerData awayGoalie = LineupService.GetStartingGoalie(awayTeam);
        LiveMatchStateData match = new LiveMatchStateData
        {
            ScheduledGameId = scheduledGame == null ? "" : scheduledGame.GameId,
            IsActive = true,
            IsCompleted = false,
            IsPlayoffGame = isPlayoffGame,
            DayIndex = scheduledGame == null ? 0 : scheduledGame.DayNumber,
            HomeTeamId = homeTeam == null ? "" : homeTeam.Id,
            HomeTeamName = TeamIdentityService.GetDisplayName(homeTeam),
            AwayTeamId = awayTeam == null ? "" : awayTeam.Id,
            AwayTeamName = TeamIdentityService.GetDisplayName(awayTeam),
            Period = 1,
            PeriodSecondsRemaining = LiveMatchConfig.RegulationPeriodSeconds,
            SpeedMultiplier = 1,
            IsPaused = false,
            UserCanSave = false,
            UserTeamId = userTeamId,
            UserTeamName = userTeamId == (homeTeam == null ? "" : homeTeam.Id)
                ? TeamIdentityService.GetDisplayName(homeTeam)
                : TeamIdentityService.GetDisplayName(awayTeam),
            StartedAtUtc = DateTime.UtcNow.ToString("o")
        };

        match.EnsureCollections();
        InitializeStats(match.HomeStats, homeTeam, homeGoalie);
        InitializeStats(match.AwayStats, awayTeam, awayGoalie);
        MarkActivePlayers(match, homeTeam);
        MarkActivePlayers(match, awayTeam);
        LiveMatchTokenService.UpdateTokensForTick(match, homeTeam, awayTeam);
        AddEvent(match, CreateEvent(match, "MatchStart", null, null, "Матч начался", 2));
        return match;
    }

    public static void AdvanceTick(LiveMatchStateData match, TeamData homeTeam, TeamData awayTeam)
    {
        AdvanceTickInternal(match, homeTeam, awayTeam, false);
    }

    public static void AdvanceUntilPeriodEnd(LiveMatchStateData match, TeamData homeTeam, TeamData awayTeam)
    {
        if (match == null || match.IsCompleted)
        {
            return;
        }

        int startingPeriod = match.Period;
        int safety = 0;
        while (!match.IsCompleted && match.Period == startingPeriod && safety < 300)
        {
            AdvanceTickInternal(match, homeTeam, awayTeam, true);
            safety++;
        }
    }

    public static void AdvanceUntilMatchEnd(LiveMatchStateData match, TeamData homeTeam, TeamData awayTeam)
    {
        int safety = 0;
        while (match != null && !match.IsCompleted && safety < 1200)
        {
            AdvanceTickInternal(match, homeTeam, awayTeam, true);
            safety++;
        }

        if (match != null && !match.IsCompleted)
        {
            ForceComplete(match);
        }
    }

    public static LiveMatchEventData CreateEvent(
        LiveMatchStateData match,
        string eventType,
        TeamData team,
        PlayerData player,
        string description,
        int importance)
    {
        return new LiveMatchEventData
        {
            EventType = eventType,
            Period = match == null ? 1 : match.Period,
            PeriodLabel = match == null ? "" : LiveMatchConfig.FormatPeriodLabel(match),
            GameSecondsElapsed = match == null ? 0 : match.TotalGameSecondsElapsed,
            PeriodSecondsRemaining = match == null ? 0 : match.PeriodSecondsRemaining,
            ClockLabel = match == null ? "" : LiveMatchConfig.FormatClock(match.PeriodSecondsRemaining),
            TeamId = team == null ? "" : team.Id,
            TeamName = TeamIdentityService.GetDisplayName(team),
            PlayerId = player == null ? "" : player.Id,
            PlayerName = GetPlayerName(player),
            Description = description,
            Importance = importance,
            HomeScoreAfter = match == null ? 0 : match.HomeScore,
            AwayScoreAfter = match == null ? 0 : match.AwayScore
        };
    }

    public static void AddEvent(LiveMatchStateData match, LiveMatchEventData matchEvent)
    {
        if (match == null || matchEvent == null)
        {
            return;
        }

        match.EnsureCollections();
        match.Events.Insert(0, matchEvent);
        while (match.Events.Count > LiveMatchConfig.MaxEventFeedItems)
        {
            match.Events.RemoveAt(match.Events.Count - 1);
        }
    }

    public static LiveMatchPlayerStatData GetOrCreatePlayerStat(LiveMatchStateData match, PlayerData player)
    {
        if (match == null || player == null)
        {
            return null;
        }

        match.EnsureCollections();
        foreach (LiveMatchPlayerStatData stat in match.PlayerStats)
        {
            if (stat != null && stat.PlayerId == player.Id)
            {
                return stat;
            }
        }

        LiveMatchPlayerStatData created = new LiveMatchPlayerStatData
        {
            PlayerId = player.Id,
            PlayerName = GetPlayerName(player),
            TeamId = player.TeamId,
            TeamName = "",
            Position = player.Position,
            IsGoalie = player.Position == "G"
        };
        match.PlayerStats.Add(created);
        return created;
    }

    private static void AdvanceTickInternal(LiveMatchStateData match, TeamData homeTeam, TeamData awayTeam, bool ignorePause)
    {
        if (match == null || match.IsCompleted || (!ignorePause && match.IsPaused))
        {
            return;
        }

        if (match.IsShootout)
        {
            SimulateShootout(match, homeTeam, awayTeam);
            return;
        }

        MaybeAutoPullGoalie(match, homeTeam);
        MaybeAutoPullGoalie(match, awayTeam);
        GenerateTeamPossession(match, homeTeam, awayTeam, true);
        GenerateTeamPossession(match, awayTeam, homeTeam, false);
        ApplySpecialTeamsClock(match);
        AddTimeOnIce(match, homeTeam);
        AddTimeOnIce(match, awayTeam);

        int tickSeconds = Mathf.Min(LiveMatchConfig.LiveTickGameSeconds, match.PeriodSecondsRemaining);
        match.TotalGameSecondsElapsed += tickSeconds;
        match.PeriodSecondsRemaining -= tickSeconds;
        LiveMatchTokenService.UpdateTokensForTick(match, homeTeam, awayTeam);

        if (match.PeriodSecondsRemaining <= 0 && !match.IsCompleted)
        {
            AddEvent(match, CreateEvent(match, "PeriodEnd", null, null, "Период завершён", 2));
            LiveMatchRulesService.AdvanceToNextPeriodOrPhase(match);
            if (match.IsShootout)
            {
                SimulateShootout(match, homeTeam, awayTeam);
            }
        }
    }

    private static void GenerateTeamPossession(LiveMatchStateData match, TeamData attackingTeam, TeamData defendingTeam, bool attackingHome)
    {
        if (attackingTeam == null || defendingTeam == null)
        {
            return;
        }

        LiveMatchTeamStatsData attackStats = attackingHome ? match.HomeStats : match.AwayStats;
        LiveMatchTeamStatsData defenseStats = attackingHome ? match.AwayStats : match.HomeStats;
        float ratingDelta = TeamRatingCalculator.CalculateEffectiveLineupOverall(attackingTeam)
            - TeamRatingCalculator.CalculateEffectiveLineupOverall(defendingTeam);
        ratingDelta += attackingHome ? 1.8f : -1.8f;
        float shotChance = 0.235f + ratingDelta * 0.0025f;
        shotChance *= LiveMatchTacticsService.GetShotModifier(attackingTeam);
        if (attackStats.PowerPlaySecondsRemaining > 0)
        {
            shotChance += 0.08f;
        }

        if (defenseStats.IsGoaliePulled)
        {
            shotChance += 0.10f;
        }

        if (UnityEngine.Random.value > Mathf.Clamp(shotChance, 0.08f, 0.48f))
        {
            MaybePenalty(match, attackingTeam, defendingTeam, attackingHome);
            return;
        }

        PlayerData shooter = PickShooter(attackingTeam);
        PlayerData goalie = LiveMatchGoalieService.GetCurrentGoalie(match, defendingTeam);
        RegisterShot(match, attackStats, defenseStats, shooter);

        float goalChance = 0.082f + ratingDelta * 0.0015f;
        goalChance *= LiveMatchTacticsService.GetGoalModifier(attackingTeam);
        goalChance /= LiveMatchTacticsService.GetDefenseModifier(defendingTeam);
        if (attackStats.PowerPlaySecondsRemaining > 0)
        {
            goalChance += 0.035f;
        }

        if (defenseStats.IsGoaliePulled)
        {
            goalChance += 0.16f;
        }
        else if (goalie != null)
        {
            goalChance -= (goalie.Overall - 75) * 0.002f;
        }

        if (UnityEngine.Random.value < Mathf.Clamp(goalChance, 0.025f, 0.32f))
        {
            RegisterGoal(match, attackingTeam, defendingTeam, shooter, goalie, attackingHome, attackStats.PowerPlaySecondsRemaining > 0);
        }
        else
        {
            defenseStats.Saves++;
            LiveMatchPlayerStatData goalieStat = GetOrCreatePlayerStat(match, goalie);
            if (goalieStat != null)
            {
                goalieStat.Saves++;
            }

            MaybeInjury(match, attackingTeam, shooter);
        }

        MaybePenalty(match, attackingTeam, defendingTeam, attackingHome);
    }

    private static void RegisterShot(LiveMatchStateData match, LiveMatchTeamStatsData attackStats, LiveMatchTeamStatsData defenseStats, PlayerData shooter)
    {
        attackStats.Shots++;
        LiveMatchPlayerStatData shooterStat = GetOrCreatePlayerStat(match, shooter);
        if (shooterStat != null)
        {
            shooterStat.Shots++;
        }
    }

    private static void RegisterGoal(
        LiveMatchStateData match,
        TeamData scoringTeam,
        TeamData defendingTeam,
        PlayerData scorer,
        PlayerData goalie,
        bool scoringHome,
        bool isPowerPlayGoal)
    {
        LiveMatchTeamStatsData scoringStats = scoringHome ? match.HomeStats : match.AwayStats;
        LiveMatchTeamStatsData defendingStats = scoringHome ? match.AwayStats : match.HomeStats;
        if (scoringHome)
        {
            match.HomeScore++;
            match.HomeStats.Score = match.HomeScore;
        }
        else
        {
            match.AwayScore++;
            match.AwayStats.Score = match.AwayScore;
        }

        PlayerData assist1 = PickAssist(scoringTeam, scorer);
        PlayerData assist2 = PickAssist(scoringTeam, scorer, assist1);
        LiveMatchPlayerStatData scorerStat = GetOrCreatePlayerStat(match, scorer);
        if (scorerStat != null)
        {
            scorerStat.Goals++;
            scorerStat.Points++;
        }

        AddAssist(match, assist1);
        if (UnityEngine.Random.value < 0.55f)
        {
            AddAssist(match, assist2);
        }

        LiveMatchPlayerStatData goalieStat = GetOrCreatePlayerStat(match, goalie);
        if (goalieStat != null)
        {
            goalieStat.GoalsAgainst++;
        }

        if (isPowerPlayGoal)
        {
            scoringStats.PowerPlayGoals++;
            scoringStats.PowerPlaySecondsRemaining = 0;
            defendingStats.PenaltyKillSecondsRemaining = 0;
        }

        string description = TeamIdentityService.GetDisplayName(scoringTeam)
            + ": гол, " + GetPlayerName(scorer)
            + " (" + match.HomeScore + ":" + match.AwayScore + ")";
        LiveMatchEventData goalEvent = CreateEvent(match, "Goal", scoringTeam, scorer, description, 5);
        goalEvent.Assist1PlayerId = assist1 == null ? "" : assist1.Id;
        goalEvent.Assist1PlayerName = GetPlayerName(assist1);
        goalEvent.Assist2PlayerId = assist2 == null ? "" : assist2.Id;
        goalEvent.Assist2PlayerName = GetPlayerName(assist2);
        goalEvent.GoaliePlayerId = goalie == null ? "" : goalie.Id;
        goalEvent.GoaliePlayerName = GetPlayerName(goalie);
        goalEvent.HomeScoreAfter = match.HomeScore;
        goalEvent.AwayScoreAfter = match.AwayScore;
        AddEvent(match, goalEvent);

        if (LiveMatchRulesService.IsSuddenDeathGoal(match))
        {
            CompleteMatch(match, scoringTeam.Id, TeamIdentityService.GetDisplayName(scoringTeam), "SuddenDeathGoal");
        }
    }

    private static void MaybePenalty(LiveMatchStateData match, TeamData attackingTeam, TeamData defendingTeam, bool attackingHome)
    {
        float chance = 0.022f * LiveMatchTacticsService.GetPenaltyModifier(attackingTeam);
        if (UnityEngine.Random.value > Mathf.Clamp(chance, 0.008f, 0.050f))
        {
            return;
        }

        PlayerData offender = PickShooter(attackingTeam);
        LiveMatchTeamStatsData offenderStats = attackingHome ? match.HomeStats : match.AwayStats;
        LiveMatchTeamStatsData powerPlayStats = attackingHome ? match.AwayStats : match.HomeStats;
        offenderStats.PenaltyMinutes += 2;
        offenderStats.PenaltyKillSecondsRemaining = 120;
        powerPlayStats.PowerPlaySecondsRemaining = 120;
        powerPlayStats.PowerPlayOpportunities++;

        LiveMatchPlayerStatData stat = GetOrCreatePlayerStat(match, offender);
        if (stat != null)
        {
            stat.PenaltyMinutes += 2;
        }

        AddEvent(match, CreateEvent(match, "Penalty", attackingTeam, offender, "Удаление: " + GetPlayerName(offender) + ", 2 минуты", 3));
        AddEvent(match, CreateEvent(match, "PowerPlayStart", defendingTeam, null, TeamIdentityService.GetDisplayName(defendingTeam) + ": большинство", 2));
    }

    private static void MaybeInjury(LiveMatchStateData match, TeamData team, PlayerData player)
    {
        if (match == null || team == null || player == null || player.IsInjured)
        {
            return;
        }

        float risk = player.Condition <= 70 ? 0.004f : 0.0013f;
        if (UnityEngine.Random.value > risk)
        {
            return;
        }

        player.IsInjured = true;
        player.InjuryType = "Game injury";
        player.InjurySeverity = "Minor";
        player.InjuryDaysRemaining = UnityEngine.Random.Range(3, 12);
        player.CanPlayThroughInjury = false;
        player.InjuredAtUtc = DateTime.UtcNow.ToString("o");
        player.ExpectedReturnDate = DateTime.UtcNow.Date.AddDays(player.InjuryDaysRemaining).ToString("yyyy-MM-dd");
        player.TotalInjuries++;
        LiveMatchPlayerStatData stat = GetOrCreatePlayerStat(match, player);
        if (stat != null)
        {
            stat.WasInjured = true;
        }

        AddEvent(match, CreateEvent(match, "Injury", team, player, "Травма: " + GetPlayerName(player), 4));
    }

    private static void SimulateShootout(LiveMatchStateData match, TeamData homeTeam, TeamData awayTeam)
    {
        if (match == null || match.IsCompleted)
        {
            return;
        }

        match.IsShootout = true;
        int homeShootoutGoals = 0;
        int awayShootoutGoals = 0;
        int round = 0;
        while (round < 3 || homeShootoutGoals == awayShootoutGoals)
        {
            PlayerData homeShooter = LiveMatchLineSelectorService.SelectShooter(homeTeam, round);
            PlayerData awayShooter = LiveMatchLineSelectorService.SelectShooter(awayTeam, round);
            if (UnityEngine.Random.value < GetShootoutChance(homeShooter, awayTeam))
            {
                homeShootoutGoals++;
                AddEvent(match, CreateEvent(match, "ShootoutGoal", homeTeam, homeShooter, "Буллит реализован: " + GetPlayerName(homeShooter), 4));
            }

            if (UnityEngine.Random.value < GetShootoutChance(awayShooter, homeTeam))
            {
                awayShootoutGoals++;
                AddEvent(match, CreateEvent(match, "ShootoutGoal", awayTeam, awayShooter, "Буллит реализован: " + GetPlayerName(awayShooter), 4));
            }

            round++;
            if (round > 12)
            {
                break;
            }
        }

        if (homeShootoutGoals >= awayShootoutGoals)
        {
            match.HomeScore++;
            match.HomeStats.Score = match.HomeScore;
            CompleteMatch(match, match.HomeTeamId, match.HomeTeamName, "Shootout");
        }
        else
        {
            match.AwayScore++;
            match.AwayStats.Score = match.AwayScore;
            CompleteMatch(match, match.AwayTeamId, match.AwayTeamName, "Shootout");
        }
    }

    private static void ApplySpecialTeamsClock(LiveMatchStateData match)
    {
        TickSpecialTeams(match.HomeStats);
        TickSpecialTeams(match.AwayStats);
    }

    private static void TickSpecialTeams(LiveMatchTeamStatsData stats)
    {
        if (stats == null)
        {
            return;
        }

        stats.PowerPlaySecondsRemaining = Mathf.Max(0, stats.PowerPlaySecondsRemaining - LiveMatchConfig.LiveTickGameSeconds);
        stats.PenaltyKillSecondsRemaining = Mathf.Max(0, stats.PenaltyKillSecondsRemaining - LiveMatchConfig.LiveTickGameSeconds);
    }

    private static void AddTimeOnIce(LiveMatchStateData match, TeamData team)
    {
        LiveMatchTeamStatsData stats = team == null || match == null
            ? null
            : match.HomeTeamId == team.Id ? match.HomeStats : match.AwayStats;
        if (stats == null)
        {
            return;
        }

        List<PlayerData> skaters = LiveMatchLineSelectorService.SelectSkaters(team, match, stats.IsGoaliePulled);
        foreach (PlayerData player in skaters)
        {
            LiveMatchPlayerStatData stat = GetOrCreatePlayerStat(match, player);
            if (stat != null)
            {
                stat.TimeOnIceSeconds += LiveMatchConfig.LiveTickGameSeconds;
            }
        }

        PlayerData goalie = stats.IsGoaliePulled ? null : LiveMatchGoalieService.GetCurrentGoalie(match, team);
        LiveMatchPlayerStatData goalieStat = GetOrCreatePlayerStat(match, goalie);
        if (goalieStat != null)
        {
            goalieStat.TimeOnIceSeconds += LiveMatchConfig.LiveTickGameSeconds;
        }
    }

    private static void CompleteMatch(LiveMatchStateData match, string winnerTeamId, string winnerTeamName, string reason)
    {
        match.IsCompleted = true;
        match.IsActive = false;
        match.WinnerTeamId = winnerTeamId;
        match.WinnerTeamName = winnerTeamName;
        match.CompletionReason = reason;
        match.CompletedAtUtc = DateTime.UtcNow.ToString("o");
        MarkFinishedGoalies(match);
        AddEvent(match, CreateEvent(match, "MatchEnd", null, null, "Матч завершён: " + match.HomeScore + ":" + match.AwayScore, 5));
    }

    private static void ForceComplete(LiveMatchStateData match)
    {
        string winnerId = match.HomeScore >= match.AwayScore ? match.HomeTeamId : match.AwayTeamId;
        string winnerName = match.HomeScore >= match.AwayScore ? match.HomeTeamName : match.AwayTeamName;
        if (match.HomeScore == match.AwayScore)
        {
            match.HomeScore++;
            match.HomeStats.Score = match.HomeScore;
            winnerId = match.HomeTeamId;
            winnerName = match.HomeTeamName;
        }

        CompleteMatch(match, winnerId, winnerName, "SafetyComplete");
    }

    private static void MaybeAutoPullGoalie(LiveMatchStateData match, TeamData team)
    {
        if (LiveMatchGoalieService.ShouldAutoPullGoalie(match, team))
        {
            LiveMatchGoalieService.PullGoalie(match, team, out string message);
        }
    }

    private static PlayerData PickShooter(TeamData team)
    {
        List<PlayerData> skaters = LineupService.GetActiveSkaters(team);
        skaters.RemoveAll(player => player == null || !InjuryService.IsPlayerAvailable(player));
        if (skaters.Count == 0)
        {
            return null;
        }

        skaters.Sort((left, right) => right.Overall.CompareTo(left.Overall));
        int upper = Mathf.Min(8, skaters.Count);
        return skaters[UnityEngine.Random.Range(0, upper)];
    }

    private static PlayerData PickAssist(TeamData team, PlayerData scorer, PlayerData excluded = null)
    {
        List<PlayerData> skaters = LineupService.GetActiveSkaters(team);
        skaters.RemoveAll(player => player == null
            || player.Id == (scorer == null ? "" : scorer.Id)
            || player.Id == (excluded == null ? "" : excluded.Id)
            || !InjuryService.IsPlayerAvailable(player));
        if (skaters.Count == 0)
        {
            return null;
        }

        skaters.Sort((left, right) => right.Overall.CompareTo(left.Overall));
        int upper = Mathf.Min(10, skaters.Count);
        return skaters[UnityEngine.Random.Range(0, upper)];
    }

    private static void AddAssist(LiveMatchStateData match, PlayerData player)
    {
        LiveMatchPlayerStatData stat = GetOrCreatePlayerStat(match, player);
        if (stat != null)
        {
            stat.Assists++;
            stat.Points++;
        }
    }

    private static float GetShootoutChance(PlayerData shooter, TeamData opponent)
    {
        PlayerData goalie = LineupService.GetStartingGoalie(opponent);
        int shooterOverall = shooter == null ? 70 : shooter.Overall;
        int goalieOverall = goalie == null ? 74 : goalie.Overall;
        return Mathf.Clamp(0.32f + ((shooterOverall - goalieOverall) * 0.008f), 0.18f, 0.52f);
    }

    private static void InitializeStats(LiveMatchTeamStatsData stats, TeamData team, PlayerData goalie)
    {
        if (stats == null)
        {
            return;
        }

        stats.TeamId = team == null ? "" : team.Id;
        stats.TeamName = TeamIdentityService.GetDisplayName(team);
        stats.CurrentGoaliePlayerId = goalie == null ? "" : goalie.Id;
        stats.CurrentGoalieName = GetPlayerName(goalie);
        stats.TacticName = team == null || team.Tactics == null ? "Balanced" : team.Tactics.PresetName;
    }

    private static void PrepareTeam(TeamData team)
    {
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        InjuryService.EnsureInjuryFieldsForTeam(team);
        PlayerFatigueService.EnsureFatigueForTeam(team);
        LineupService.EnsureLineup(team);
        SpecialTeamsService.EnsureSpecialTeams(team);
        TacticsService.EnsureTactics(team);
        JerseyNumberService.EnsureJerseyNumbersForTeam(team);
        IceTimeService.EnsureUsageForTeam(team);
    }

    private static void MarkActivePlayers(LiveMatchStateData match, TeamData team)
    {
        foreach (PlayerData player in LineupService.GetActivePlayers(team))
        {
            LiveMatchPlayerStatData stat = GetOrCreatePlayerStat(match, player);
            if (stat != null)
            {
                stat.TeamName = TeamIdentityService.GetDisplayName(team);
                stat.StartedGame = true;
                stat.FinishedGame = player != null && player.Position == "G" && player.Id == (team.Id == match.HomeTeamId ? match.HomeStats.CurrentGoaliePlayerId : match.AwayStats.CurrentGoaliePlayerId);
            }
        }
    }

    private static void MarkFinishedGoalies(LiveMatchStateData match)
    {
        foreach (LiveMatchPlayerStatData stat in match.PlayerStats)
        {
            if (stat == null || !stat.IsGoalie)
            {
                continue;
            }

            stat.FinishedGame = stat.PlayerId == match.HomeStats.CurrentGoaliePlayerId
                || stat.PlayerId == match.AwayStats.CurrentGoaliePlayerId;
        }
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : player.FirstName + " " + player.LastName;
    }
}
