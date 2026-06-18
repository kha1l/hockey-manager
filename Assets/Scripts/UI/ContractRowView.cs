using UnityEngine;
using UnityEngine.UI;

public class ContractRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _extendButton;

    private string _playerId;
    private ContractsController _controller;

    public void Configure(Text infoText, Button extendButton)
    {
        _infoText = infoText;
        _extendButton = extendButton;
    }

    public void Initialize(PlayerData player, ContractsController controller)
    {
        if (player == null)
        {
            _playerId = "";
            _controller = controller;
            if (_infoText != null)
            {
                _infoText.text = "Контракт недоступен";
            }

            return;
        }

        _playerId = player.Id;
        _controller = controller;
        _infoText.text = PlayerDisplayFormatter.FormatPlayerWithContract(player);

        _extendButton.onClick.RemoveAllListeners();
        _extendButton.onClick.AddListener(OnExtendClicked);
    }

    private void OnExtendClicked()
    {
        if (_controller != null)
        {
            _controller.ExtendContract(_playerId);
        }
    }

}
