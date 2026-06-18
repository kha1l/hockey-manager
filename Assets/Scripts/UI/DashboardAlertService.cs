using System;
using System.Collections.Generic;

public static class DashboardAlertService
{
    public static List<DashboardAlertData> BuildAlerts(GameState state, TeamData team)
    {
        List<DashboardAlertData> alerts = new List<DashboardAlertData>();
        AddRosterAlerts(alerts, state, team);
        AddLineupAlerts(alerts, team);
        AddCapAlerts(alerts, team);
        AddContractAlerts(alerts, state, team);
        AddMoraleAlerts(alerts, state, team);
        AddInjuryAlerts(alerts, team);
        AddOwnerAlerts(alerts, state, team);
        AddGmCareerAlerts(alerts, state);

        alerts.Sort(CompareAlerts);
        while (alerts.Count > MobileUiConfig.MaxDashboardAlerts)
        {
            alerts.RemoveAt(alerts.Count - 1);
        }

        return alerts;
    }

    public static DashboardAlertData CreateAlert(string category, string title, string message, int priority, string targetPanel)
    {
        return new DashboardAlertData
        {
            AlertId = Guid.NewGuid().ToString("N"),
            Category = string.IsNullOrEmpty(category) ? "Info" : category,
            Title = string.IsNullOrEmpty(title) ? "Alert" : title,
            Message = string.IsNullOrEmpty(message) ? "" : message,
            Priority = priority,
            TargetPanel = string.IsNullOrEmpty(targetPanel) ? "Dashboard" : targetPanel
        };
    }

    private static void AddRosterAlerts(List<DashboardAlertData> alerts, GameState state, TeamData team)
    {
        if (team == null)
        {
            return;
        }

        TeamRosterService.EnsureRosterStatusesForTeam(team);
        if (!TeamRosterService.ValidateNhlRoster(team, out string message))
        {
            alerts.Add(CreateAlert("Roster", "Проблема состава", message, 100, "Organization"));
        }
    }

    private static void AddLineupAlerts(List<DashboardAlertData> alerts, TeamData team)
    {
        if (team == null)
        {
            return;
        }

        bool valid = LineupService.ValidateLineup(team, out string message);
        if (!valid)
        {
            alerts.Add(CreateAlert("Lineup", "Состав на матч невалиден", message, 95, "Lineup"));
            return;
        }

        if (LineupService.HasInjuredActivePlayers(team, out string injuryMessage))
        {
            alerts.Add(CreateAlert("Injury", "В составе есть травмированный игрок", injuryMessage, 90, "Lineup"));
        }
    }

    private static void AddContractAlerts(List<DashboardAlertData> alerts, GameState state, TeamData team)
    {
        if (team == null)
        {
            return;
        }

        ContractExtensionSummaryData summary = ContractExtensionService.BuildSummary(state, team);
        if (summary == null)
        {
            return;
        }

        if (summary.PendingUfaCount > 0 || summary.PendingRfaCount > 0 || summary.EligiblePlayers > 0)
        {
            alerts.Add(CreateAlert(
                "Contracts",
                "Истекающие контракты",
                "Eligible: " + summary.EligiblePlayers + " | UFA: " + summary.PendingUfaCount + " | RFA: " + summary.PendingRfaCount,
                70,
                "Extensions"));
        }
    }

    private static void AddMoraleAlerts(List<DashboardAlertData> alerts, GameState state, TeamData team)
    {
        if (team == null)
        {
            return;
        }

        TeamMoraleSummaryData summary = MoraleService.BuildTeamMoraleSummary(state, team);
        if (summary == null)
        {
            return;
        }

        if (summary.TradeRequests > 0)
        {
            alerts.Add(CreateAlert("Morale", "Запросы на обмен", "Игроков с trade request: " + summary.TradeRequests, 75, "Morale"));
        }

        if (summary.VeryUnhappyPlayers > 0 || summary.UnhappyPlayers > 0)
        {
            alerts.Add(CreateAlert(
                "Morale",
                "Недовольство игроков",
                "Unhappy: " + summary.UnhappyPlayers + " | Very unhappy: " + summary.VeryUnhappyPlayers,
                65,
                "Morale"));
        }
    }

    private static void AddInjuryAlerts(List<DashboardAlertData> alerts, TeamData team)
    {
        if (team == null)
        {
            return;
        }

        InjuryService.EnsureInjuryFieldsForTeam(team);
        int injured = 0;
        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.IsInjured)
            {
                injured++;
            }
        }

        if (injured > 0)
        {
            alerts.Add(CreateAlert("Injury", "Травмы", "Травмированных игроков: " + injured, 80, "Injuries"));
        }
    }

    private static void AddOwnerAlerts(List<DashboardAlertData> alerts, GameState state, TeamData team)
    {
        OwnerProfileData owner = OwnerGoalService.GetOwnerProfile(state, team);
        if (owner == null)
        {
            alerts.Add(CreateAlert("Owner", "Цели владельца создаются", "Профиль владельца ещё не готов", 40, "Owner"));
            return;
        }

        if (owner.GmTrust < 45)
        {
            alerts.Add(CreateAlert("Owner", "Давление владельца", "GM trust: " + owner.GmTrust + " | " + owner.JobSecurity, 65, "Owner"));
        }
    }

    private static void AddGmCareerAlerts(List<DashboardAlertData> alerts, GameState state)
    {
        GmCareerService.EnsureGmCareer(state);
        GmCareerData career = state == null ? null : state.GmCareer;
        if (career == null)
        {
            return;
        }

        if (career.IsUnemployed)
        {
            alerts.Add(CreateAlert("Owner", "Вы уволены", "Выберите новую команду в GM Career", 100, "GmCareer"));
            return;
        }

        if (career.CurrentJobSecurity < GmJobSecurityConfig.PressureThreshold)
        {
            alerts.Add(CreateAlert(
                "Owner",
                "Риск увольнения",
                "Job Security: " + GmJobSecurityConfig.GetJobSecurityLabel(career.CurrentJobSecurity) + " " + career.CurrentJobSecurity,
                88,
                "GmCareer"));
        }
    }

    private static void AddCapAlerts(List<DashboardAlertData> alerts, TeamData team)
    {
        if (team == null)
        {
            return;
        }

        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        if (finance == null)
        {
            return;
        }

        if (finance.IsOverCap)
        {
            alerts.Add(CreateAlert("Cap", "Превышен потолок зарплат", "Payroll: " + MobileUiConfig.FormatMoney(finance.Payroll), 85, "Contracts"));
        }
        else if (finance.CapSpace < 2000000)
        {
            alerts.Add(CreateAlert("Cap", "Мало места под потолком", "Cap space: " + MobileUiConfig.FormatMoney(finance.CapSpace), 60, "Contracts"));
        }
    }

    private static int CompareAlerts(DashboardAlertData left, DashboardAlertData right)
    {
        int leftPriority = left == null ? 0 : left.Priority;
        int rightPriority = right == null ? 0 : right.Priority;
        return rightPriority.CompareTo(leftPriority);
    }
}
