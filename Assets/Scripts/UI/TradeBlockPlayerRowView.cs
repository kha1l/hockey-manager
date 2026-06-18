using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class TradeBlockPlayerRowView : MonoBehaviour
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

    public void Initialize(TradeBlockPlayerData player, GameScreenController screenController)
    {
        if (player == null)
        {
            _playerId = "";
            _screenController = null;
            _infoText.text = "Trade block player недоступен";
            _button.onClick.RemoveAllListeners();
            return;
        }

        _playerId = player.PlayerId;
        _screenController = screenController;
        _infoText.text = FormatCaptaincyMarker(player) + player.PlayerName
            + " | " + player.Position
            + " | " + player.Age
            + " лет | OVR " + player.Overall
            + " / POT " + player.Potential
            + " | $" + FormatMoney(player.Salary)
            + " | " + player.RosterStatus
            + " | LD " + player.Leadership
            + " | MOR " + player.Morale + " " + player.MoraleStatus
            + (player.WantsTrade ? " | TRADE REQ" : "")
            + " | Avail " + player.AvailabilityScore
            + "\n" + player.Reason;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (_screenController != null && !string.IsNullOrEmpty(_playerId))
        {
            _screenController.SelectOtherTradePlayer(_playerId);
        }
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }

    private static string FormatCaptaincyMarker(TradeBlockPlayerData player)
    {
        if (player == null)
        {
            return "";
        }

        if (player.IsCaptain)
        {
            return "C | ";
        }

        return player.IsAlternateCaptain ? "A | " : "";
    }
}
