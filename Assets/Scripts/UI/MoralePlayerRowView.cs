using UnityEngine;
using UnityEngine.UI;

public class MoralePlayerRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _button;

    private PlayerMoraleSnapshotData _snapshot;
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button button)
    {
        _infoText = infoText;
        _button = button;
    }

    public void Initialize(PlayerMoraleSnapshotData snapshot, GameScreenController screenController)
    {
        _snapshot = snapshot;
        _screenController = screenController;

        if (_infoText != null)
        {
            _infoText.text = FormatSnapshot(snapshot);
        }

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        if (_snapshot != null && _screenController != null)
        {
            _screenController.SelectMoralePlayer(_snapshot.PlayerId);
        }
    }

    private static string FormatSnapshot(PlayerMoraleSnapshotData snapshot)
    {
        if (snapshot == null)
        {
            return "Игрок недоступен";
        }

        return snapshot.PlayerName
            + " | " + snapshot.Position
            + " | OVR " + snapshot.Overall
            + " | [" + snapshot.RosterStatus + "]"
            + (string.IsNullOrEmpty(snapshot.CaptaincyRole) ? "" : " [" + snapshot.CaptaincyRole + "]")
            + "\nMOR " + snapshot.Morale
            + " | " + MobileUiConfig.FormatShortStatus(snapshot.MoraleStatus)
            + (snapshot.WantsTrade ? " | TRADE REQ" : "")
            + " | " + MobileUiConfig.FormatShortStatus(snapshot.MoraleSummary);
    }
}
