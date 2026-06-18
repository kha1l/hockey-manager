using UnityEngine;
using UnityEngine.UI;

public class TradeDraftPickRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _button;

    private string _pickId;
    private GameScreenController _screenController;
    private bool _isUserPick;

    public void Configure(Text infoText, Button button)
    {
        _infoText = infoText;
        _button = button;
    }

    public void InitializeUserPick(DraftPickOwnershipData pick, GameScreenController screenController)
    {
        Initialize(pick, screenController, true);
    }

    public void InitializeOtherPick(DraftPickOwnershipData pick, GameScreenController screenController)
    {
        Initialize(pick, screenController, false);
    }

    private void Initialize(DraftPickOwnershipData pick, GameScreenController screenController, bool isUserPick)
    {
        _pickId = pick.PickId;
        _screenController = screenController;
        _isUserPick = isUserPick;

        int value = TradeValueCalculator.CalculateDraftPickValue(pick, GameSession.CurrentState);
        _infoText.text = pick.DraftYear
            + " | Round " + pick.Round
            + " | from " + pick.OriginalTeamName
            + " | owner " + pick.CurrentOwnerTeamName
            + " | Traded: " + pick.IsTraded
            + " | Value " + value;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (_screenController == null)
        {
            return;
        }

        if (_isUserPick)
        {
            _screenController.SelectUserTradePick(_pickId);
            return;
        }

        _screenController.SelectOtherTradePick(_pickId);
    }
}
