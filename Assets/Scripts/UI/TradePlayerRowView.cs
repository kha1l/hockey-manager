using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class TradePlayerRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _button;

    private string _playerId;
    private GameScreenController _screenController;
    private bool _isUserPlayer;

    public void Configure(Text infoText, Button button)
    {
        _infoText = infoText;
        _button = button;
    }

    public void InitializeUserPlayer(PlayerData player, GameScreenController screenController)
    {
        Initialize(player, screenController, true);
    }

    public void InitializeOtherPlayer(PlayerData player, GameScreenController screenController)
    {
        Initialize(player, screenController, false);
    }

    private void Initialize(PlayerData player, GameScreenController screenController, bool isUserPlayer)
    {
        _playerId = player.Id;
        _screenController = screenController;
        _isUserPlayer = isUserPlayer;
        _infoText.text = player.FirstName + " " + player.LastName
            + " | " + player.Position
            + " | " + player.Age
            + " лет | OVR " + player.Overall
            + " | " + player.RosterStatus
            + " | $" + FormatMoney(player.Salary)
            + " | " + player.ContractYearsRemaining + " г."
            + (player.HasNoTradeClause ? " | NTC" : "");

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (_screenController == null)
        {
            return;
        }

        if (_isUserPlayer)
        {
            _screenController.SelectUserTradePlayer(_playerId);
            return;
        }

        _screenController.SelectOtherTradePlayer(_playerId);
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
