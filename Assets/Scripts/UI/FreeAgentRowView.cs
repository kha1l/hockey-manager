using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class FreeAgentRowView : MonoBehaviour
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

    public void Initialize(PlayerData player, GameScreenController screenController)
    {
        _playerId = player.Id;
        _screenController = screenController;
        _infoText.text = player.FirstName + " " + player.LastName
            + " | " + player.Position
            + " | " + player.Age
            + " лет | OVR " + player.Overall
            + " | POT " + player.Potential
            + " | $" + FormatMoney(player.Salary)
            + " | " + player.ContractYearsRemaining + " г."
            + " | " + player.ContractStatus;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (_screenController != null)
        {
            _screenController.SelectFreeAgent(_playerId);
        }
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
