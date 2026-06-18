using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ExtensionCandidateRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _button;

    private string _playerId;
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button button)
    {
        _infoText = infoText;
        _button = button;
    }

    public void Initialize(ContractExtensionCandidateData candidate, GameScreenController screenController)
    {
        _playerId = candidate == null ? "" : candidate.PlayerId;
        _screenController = screenController;

        if (candidate == null)
        {
            _infoText.text = "Игрок недоступен";
            return;
        }

        string marker = candidate.IsExtensionEligible ? "ELIGIBLE" : "BLOCKED";
        if (candidate.WantsTrade)
        {
            marker += " | WANTS TRADE";
        }

        _infoText.text = candidate.PlayerName
            + " | " + candidate.Position
            + " | " + candidate.Age
            + " | OVR " + candidate.Overall
            + " | " + candidate.ContractStatus
            + " | " + FormatMoney(candidate.CurrentSalary)
            + " | " + candidate.ContractYearsRemaining + " г."
            + " | " + candidate.Category
            + " | EXT INT " + candidate.ExtensionInterest
            + " | ask " + FormatMoney(candidate.ExpectedSalary)
            + " x " + candidate.ExpectedYears
            + " | " + marker;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (_screenController != null)
        {
            _screenController.SelectExtensionPlayer(_playerId);
        }
    }

    private static string FormatMoney(int value)
    {
        return "$" + value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
