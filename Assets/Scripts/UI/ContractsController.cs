using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ContractsController : MonoBehaviour
{
    [SerializeField] private Text _financeText;
    [SerializeField] private Text _messageText;
    [SerializeField] private Transform _contractsContainer;
    [SerializeField] private ContractRowView _contractRowPrefab;

    public void Configure(
        Text financeText,
        Text messageText,
        Transform contractsContainer,
        ContractRowView contractRowPrefab)
    {
        _financeText = financeText;
        _messageText = messageText;
        _contractsContainer = contractsContainer;
        _contractRowPrefab = contractRowPrefab;
    }

    public void ShowContracts()
    {
        if (_contractsContainer == null || _contractRowPrefab == null || _financeText == null)
        {
            Debug.LogError("ContractsController: UI references are not configured.");
            return;
        }

        ClearRows();
        _contractRowPrefab.gameObject.SetActive(false);

        GameSession.EnsureContracts();
        TeamData team = GameSession.CurrentTeam;
        if (team == null)
        {
            _financeText.text = "Команда не выбрана";
            return;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        if (team.Players.Count == 0)
        {
            team.Players = PlayerSeedData.CreatePlayersForTeam(team.Id);
        }

        ContractGenerator.EnsureContractsForTeam(team);
        GameSession.EnsureContractExtensions();
        InjuryService.EnsureInjuryFieldsForTeam(team);
        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        ClubFinanceData clubFinances = GameSession.GetCurrentTeamClubFinances();
        ContractExtensionSummaryData extensionSummary = GameSession.GetCurrentTeamExtensionSummary();
        _financeText.text = BuildFinanceText(finance, extensionSummary, clubFinances);

        foreach (PlayerData player in team.Players)
        {
            ContractGenerator.NormalizeContract(player);

            ContractRowView row = Instantiate(_contractRowPrefab, _contractsContainer);
            row.name = player.Id + "-contract-row";
            row.gameObject.SetActive(true);
            row.Initialize(player, this);
        }
    }

    public void ExtendContract(string playerId)
    {
        bool extended = ContractService.TryExtendContract(GameSession.CurrentTeam, playerId, out string message);
        if (extended && GameSession.CurrentState != null)
        {
            GameSession.EnsureContractExtensions();
            SaveLoadService.Save(GameSession.CurrentState);
        }

        ShowContracts();

        if (_messageText != null)
        {
            _messageText.text = message;
        }

        Debug.Log(message);
    }

    private string BuildFinanceText(TeamFinanceData finance, ContractExtensionSummaryData extensionSummary, ClubFinanceData clubFinances)
    {
        string text =
            "TeamName: " + finance.TeamName + "\n"
            + "Season ruleset: 2026-27\n"
            + "SalaryCapUpperLimit: " + FormatMoney(finance.SalaryCapUpperLimit) + "\n"
            + "SalaryCapLowerLimit: " + FormatMoney(finance.SalaryCapLowerLimit) + "\n"
            + "LeagueMinimumSalary: " + FormatMoney(SalaryCapConfig.LeagueMinimumSalary) + "\n"
            + "MaximumPlayerSalary: " + FormatMoney(SalaryCapConfig.MaximumPlayerSalary) + "\n"
            + "Payroll: " + FormatMoney(finance.Payroll) + "\n"
            + "CapSpace: " + FormatMoney(finance.CapSpace) + "\n"
            + "Budget: " + FormatMoney(clubFinances == null ? 0 : clubFinances.Budget) + "\n"
            + "FinancialHealth: " + (clubFinances == null ? "нет данных" : clubFinances.FinancialHealthLabel) + "\n"
            + "FloorSpace: " + FormatMoney(finance.FloorSpace) + "\n"
            + "IsOverCap: " + finance.IsOverCap + "\n"
            + "IsBelowFloor: " + finance.IsBelowFloor + "\n"
            + "PlayerCount: " + finance.PlayerCount
            + "\nИстекающие контракты: " + (extensionSummary == null ? 0 : extensionSummary.EligiblePlayers)
            + " | UFA: " + (extensionSummary == null ? 0 : extensionSummary.PendingUfaCount)
            + " | RFA: " + (extensionSummary == null ? 0 : extensionSummary.PendingRfaCount)
            + " | ELC: " + (extensionSummary == null ? 0 : extensionSummary.ElcExpiringCount)
            + "\nНизкий интерес к продлению: " + (extensionSummary == null ? 0 : extensionSummary.LowInterestCount);

        if (finance.IsOverCap)
        {
            text += "\nКоманда выше потолка зарплат";
        }

        if (finance.IsBelowFloor)
        {
            text += "\nКоманда ниже минимального порога зарплат";
        }

        return text;
    }

    private void ClearRows()
    {
        for (int i = _contractsContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _contractsContainer.GetChild(i);
            if (child == _contractRowPrefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        if (_messageText != null)
        {
            _messageText.text = "";
        }
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
