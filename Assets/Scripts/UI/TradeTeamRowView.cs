using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class TradeTeamRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _button;

    private string _teamId;
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button button)
    {
        _infoText = infoText;
        _button = button;
    }

    public void Initialize(TeamData team, GameScreenController screenController)
    {
        _teamId = team.Id;
        _screenController = screenController;
        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        _infoText.text = TeamIdentityService.GetDisplayName(team)
            + " (" + team.Abbreviation + ")"
            + " | Payroll $" + FormatMoney(finance.Payroll)
            + " | Cap $" + FormatMoney(finance.CapSpace);

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (_screenController != null)
        {
            _screenController.SelectOtherTradeTeam(_teamId);
        }
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
