using System;
using System.Collections.Generic;

public static class OwnerGoalGenerator
{
    public static List<OwnerGoalData> GenerateSeasonGoals(GameState state, TeamData team)
    {
        string direction = DetermineTeamDirection(state, team);
        List<OwnerGoalData> goals = new List<OwnerGoalData>
        {
            CreatePrimaryGoal(direction),
            CreateSecondaryGoal(direction),
            CreateFinancialGoal(state, team, direction),
            CreateDevelopmentGoal(direction),
            CreateRosterGoal(state, team, direction)
        };

        return goals;
    }

    public static OwnerGoalData CreateGoal(
        string goalType,
        string title,
        string description,
        int targetValue,
        string targetValueLabel,
        int trustImpactOnSuccess,
        int trustImpactOnFailure)
    {
        string now = DateTime.UtcNow.ToString("o");
        return new OwnerGoalData
        {
            GoalId = Guid.NewGuid().ToString("N"),
            GoalType = goalType,
            Title = title,
            Description = description,
            TargetValue = targetValue,
            TargetValueLabel = targetValueLabel,
            CurrentValue = 0,
            ProgressPercent = 0,
            IsCompleted = false,
            IsFailed = false,
            TrustImpactOnSuccess = trustImpactOnSuccess,
            TrustImpactOnFailure = trustImpactOnFailure,
            Status = OwnerGoalConfig.StatusActive,
            ResultSummary = "",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public static string DetermineTeamDirection(GameState state, TeamData team)
    {
        string direction = TeamDirectionService.DetermineTeamDirection(state, team);
        if (!string.IsNullOrEmpty(direction))
        {
            return direction;
        }

        int overall = TeamRatingCalculator.CalculateOverall(team);
        if (overall >= 82)
        {
            return TradeAiConfig.DirectionContender;
        }

        if (overall >= 79)
        {
            return TradeAiConfig.DirectionPlayoffTeam;
        }

        if (overall >= 76)
        {
            return TradeAiConfig.DirectionBubbleTeam;
        }

        if (overall >= 73)
        {
            return TradeAiConfig.DirectionRetool;
        }

        return TradeAiConfig.DirectionRebuild;
    }

    private static OwnerGoalData CreatePrimaryGoal(string direction)
    {
        if (direction == TradeAiConfig.DirectionContender)
        {
            return CreateGoal(
                OwnerGoalConfig.GoalTypePrimary,
                OwnerGoalConfig.GoalWinPlayoffRound,
                "Владелец ждёт победы хотя бы в одной серии плей-офф.",
                1,
                "1 playoff round",
                10,
                -14);
        }

        if (direction == TradeAiConfig.DirectionPlayoffTeam)
        {
            return CreateGoal(
                OwnerGoalConfig.GoalTypePrimary,
                OwnerGoalConfig.GoalMakePlayoffs,
                "Команда должна попасть в плей-офф.",
                1,
                "Playoff berth",
                8,
                -10);
        }

        if (direction == TradeAiConfig.DirectionBubbleTeam)
        {
            return CreateGoal(
                OwnerGoalConfig.GoalTypePrimary,
                OwnerGoalConfig.GoalMakePlayoffs,
                "Боритесь за плей-офф до конца сезона.",
                1,
                "Playoff berth",
                7,
                -7);
        }

        if (direction == TradeAiConfig.DirectionRetool)
        {
            return CreateGoal(
                OwnerGoalConfig.GoalTypePrimary,
                OwnerGoalConfig.GoalImproveTeam,
                "Улучшите фундамент команды и приблизьтесь к зоне плей-офф.",
                82,
                "82 points pace",
                7,
                -6);
        }

        return CreateGoal(
            OwnerGoalConfig.GoalTypePrimary,
            OwnerGoalConfig.GoalBuildForFuture,
            "Соберите основу будущего через молодых игроков, проспектов и пики.",
            3,
            "3 future assets",
            8,
            -6);
    }

    private static OwnerGoalData CreateSecondaryGoal(string direction)
    {
        if (direction == TradeAiConfig.DirectionContender)
        {
            return CreateGoal(
                OwnerGoalConfig.GoalTypeSecondary,
                OwnerGoalConfig.GoalImproveMorale,
                "Сохраните высокий моральный фон в раздевалке.",
                65,
                "65 average morale",
                5,
                -5);
        }

        if (direction == TradeAiConfig.DirectionPlayoffTeam)
        {
            return CreateGoal(
                OwnerGoalConfig.GoalTypeSecondary,
                OwnerGoalConfig.GoalImproveTeam,
                "Держите темп команды на уровне уверенного претендента на плей-офф.",
                90,
                "90 points pace",
                5,
                -5);
        }

        if (direction == TradeAiConfig.DirectionBubbleTeam || direction == TradeAiConfig.DirectionRetool)
        {
            return CreateGoal(
                OwnerGoalConfig.GoalTypeSecondary,
                OwnerGoalConfig.GoalDevelopYoungPlayers,
                "Дайте прогресс молодым игрокам организации.",
                2,
                "2 young players improved",
                5,
                -4);
        }

        return CreateGoal(
            OwnerGoalConfig.GoalTypeSecondary,
            OwnerGoalConfig.GoalAcquireDraftPicks,
            "Нарастите капитал драфта для перестройки.",
            4,
            "4 owned picks",
            5,
            -4);
    }

    private static OwnerGoalData CreateFinancialGoal(GameState state, TeamData team, string direction)
    {
        LeagueRulesData rules = state == null || state.LeagueRules == null
            ? LeagueRulesConfig.CreateDefaultRules()
            : state.LeagueRules;
        int payroll = SalaryCapService.CalculatePayroll(team);

        if (payroll > rules.SalaryCapUpperLimit * 95 / 100)
        {
            return CreateGoal(
                OwnerGoalConfig.GoalTypeFinancial,
                OwnerGoalConfig.GoalStayUnderCap,
                "Не превышайте верхний предел потолка зарплат.",
                rules.SalaryCapUpperLimit,
                "Under salary cap",
                5,
                -8);
        }

        if (direction == TradeAiConfig.DirectionRebuild || direction == TradeAiConfig.DirectionRetool)
        {
            return CreateGoal(
                OwnerGoalConfig.GoalTypeFinancial,
                OwnerGoalConfig.GoalReducePayroll,
                "Держите платёжку ниже бюджета перестройки.",
                rules.SalaryCapUpperLimit * 92 / 100,
                "Payroll under rebuild budget",
                4,
                -5);
        }

        return CreateGoal(
            OwnerGoalConfig.GoalTypeFinancial,
            OwnerGoalConfig.GoalStayUnderCap,
            "Сохраните финансовую гибкость клуба.",
            rules.SalaryCapUpperLimit,
            "Under salary cap",
            4,
            -5);
    }

    private static OwnerGoalData CreateDevelopmentGoal(string direction)
    {
        int target = direction == TradeAiConfig.DirectionRebuild ? 3 : 2;
        return CreateGoal(
            OwnerGoalConfig.GoalTypeDevelopment,
            OwnerGoalConfig.GoalDevelopYoungPlayers,
            "Развивайте молодых игроков с потенциалом.",
            target,
            target + " young players improved",
            5,
            -4);
    }

    private static OwnerGoalData CreateRosterGoal(GameState state, TeamData team, string direction)
    {
        if (direction == TradeAiConfig.DirectionRebuild)
        {
            return CreateGoal(
                OwnerGoalConfig.GoalTypeRoster,
                OwnerGoalConfig.GoalAcquireDraftPicks,
                "Получите дополнительные пики или сохраните сильный набор пиков.",
                4,
                "4 owned picks",
                5,
                -4);
        }

        if (HasExpiringCorePlayer(team))
        {
            return CreateGoal(
                OwnerGoalConfig.GoalTypeRoster,
                OwnerGoalConfig.GoalReSignCorePlayer,
                "Продлите хотя бы одного важного игрока ядра.",
                1,
                "1 core extension",
                6,
                -6);
        }

        return CreateGoal(
            OwnerGoalConfig.GoalTypeRoster,
            OwnerGoalConfig.GoalImproveMorale,
            "Сохраните раздевалку стабильной и управляемой.",
            60,
            "60 average morale",
            4,
            -4);
    }

    private static bool HasExpiringCorePlayer(TeamData team)
    {
        if (team == null)
        {
            return false;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null
                && RosterStatusConfig.IsNhlRoster(player)
                && player.Overall >= 82
                && player.ContractYearsRemaining <= 1)
            {
                return true;
            }
        }

        return false;
    }
}
