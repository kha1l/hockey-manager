using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class OrganizationController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _selectedPlayerText;
    [SerializeField] private Transform _nhlContainer;
    [SerializeField] private Transform _farmContainer;
    [SerializeField] private Transform _reserveContainer;
    [SerializeField] private OrganizationPlayerRowView _nhlRowPrefab;
    [SerializeField] private OrganizationPlayerRowView _farmRowPrefab;
    [SerializeField] private OrganizationPlayerRowView _reserveRowPrefab;

    private GameScreenController _screenController;

    public void Configure(
        Text summaryText,
        Text selectedPlayerText,
        Transform nhlContainer,
        Transform farmContainer,
        Transform reserveContainer,
        OrganizationPlayerRowView nhlRowPrefab,
        OrganizationPlayerRowView farmRowPrefab,
        OrganizationPlayerRowView reserveRowPrefab)
    {
        _summaryText = summaryText;
        _selectedPlayerText = selectedPlayerText;
        _nhlContainer = nhlContainer;
        _farmContainer = farmContainer;
        _reserveContainer = reserveContainer;
        _nhlRowPrefab = nhlRowPrefab;
        _farmRowPrefab = farmRowPrefab;
        _reserveRowPrefab = reserveRowPrefab;
    }

    public void ShowOrganization(GameState state, string selectedPlayerId, GameScreenController screenController)
    {
        _screenController = screenController;

        if (_nhlContainer == null || _farmContainer == null || _reserveContainer == null
            || _nhlRowPrefab == null || _farmRowPrefab == null || _reserveRowPrefab == null)
        {
            Debug.LogError("OrganizationController: UI references are not configured.");
            return;
        }

        ClearRows(_nhlContainer, _nhlRowPrefab);
        ClearRows(_farmContainer, _farmRowPrefab);
        ClearRows(_reserveContainer, _reserveRowPrefab);
        _nhlRowPrefab.gameObject.SetActive(false);
        _farmRowPrefab.gameObject.SetActive(false);
        _reserveRowPrefab.gameObject.SetActive(false);

        TeamData team = GameSession.CurrentTeam;
        if (team == null)
        {
            SetText(_summaryText, "Команда не выбрана");
            SetText(_selectedPlayerText, "Игрок не выбран");
            return;
        }

        TeamRosterSummaryData summary = TeamRosterService.GetRosterSummary(team);
        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        SetText(_summaryText, BuildSummaryText(summary, finance));
        SetText(_selectedPlayerText, BuildSelectedPlayerText(team, selectedPlayerId));

        foreach (PlayerData player in TeamRosterService.GetNhlPlayers(team))
        {
            CreateRow(_nhlContainer, _nhlRowPrefab, player, screenController);
        }

        foreach (PlayerData player in TeamRosterService.GetFarmPlayers(team))
        {
            CreateRow(_farmContainer, _farmRowPrefab, player, screenController);
        }

        foreach (PlayerData player in TeamRosterService.GetReservePlayers(team))
        {
            CreateRow(_reserveContainer, _reserveRowPrefab, player, screenController);
        }
    }

    public void ShowOrganization(GameState state, string selectedPlayerId)
    {
        ShowOrganization(state, selectedPlayerId, _screenController);
    }

    private static string BuildSummaryText(TeamRosterSummaryData summary, TeamFinanceData finance)
    {
        if (summary == null)
        {
            return "Организация недоступна";
        }

        return summary.TeamName
            + "\nPro roster: " + summary.NhlPlayers + " / " + RosterStatusConfig.MaxNhlRosterSize
            + " | Farm: " + summary.FarmPlayers
            + " | Reserve: " + summary.ReservePlayers
            + "\nПозиции Pro: F " + summary.NhlForwards
            + " / D " + summary.NhlDefensemen
            + " / G " + summary.NhlGoalies
            + " | Доступны Pro: " + summary.AvailableNhlPlayers
            + "\nPayroll Pro: " + FormatMoney(finance == null ? 0 : finance.Payroll)
            + " | Cap space: " + FormatMoney(finance == null ? 0 : finance.CapSpace)
            + "\nСтатус: " + summary.ValidationMessage;
    }

    private static string BuildSelectedPlayerText(TeamData team, string selectedPlayerId)
    {
        PlayerData player = FindPlayer(team, selectedPlayerId);
        if (player == null)
        {
            return "Игрок не выбран";
        }

        PlayerFatigueService.EnsureFatigueFields(player);
        InjuryService.EnsureInjuryFields(player);
        WaiverEligibilityService.EnsureWaiverEligibility(player);
        string waiverDetails = player.IsOnWaivers
            ? "\nНа waivers до: " + player.WaiverExpiresAtUtc
                + " | Назначение: " + player.WaiverIntendedDestination
            : "";
        return "Выбран: " + player.FirstName + " " + player.LastName
            + " | " + player.Position
            + " | " + player.RosterStatus
            + " | OVR " + player.Overall
            + " | POT " + player.Potential
            + " | Pro GP " + player.NHLGamesThisSeason
            + " | Farm days " + player.FarmDaysThisSeason
            + " | Reserve days " + player.ReserveDaysThisSeason
            + "\nWaivers: " + player.WaiverStatus
            + " | Requires: " + (player.RequiresWaivers ? "yes" : "no")
            + " | " + WaiverEligibilityService.GetWaiverEligibilityReason(player)
            + waiverDetails
            + (player.IsInjured ? "\nINJ " + player.InjuryDaysRemaining + " дн." : "");
    }

    private static void CreateRow(
        Transform container,
        OrganizationPlayerRowView prefab,
        PlayerData player,
        GameScreenController screenController)
    {
        OrganizationPlayerRowView row = Instantiate(prefab, container);
        row.name = player.Id + "-organization-row";
        row.gameObject.SetActive(true);
        row.Initialize(player, screenController);
    }

    private static void ClearRows(Transform container, OrganizationPlayerRowView prefab)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child == prefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
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

    private static void SetText(Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
