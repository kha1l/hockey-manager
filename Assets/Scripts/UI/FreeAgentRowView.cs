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
        _playerId = player == null ? "" : player.Id;
        _screenController = screenController;
        _infoText.text = PlayerDisplayFormatter.FormatFreeAgent(player);

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

}
