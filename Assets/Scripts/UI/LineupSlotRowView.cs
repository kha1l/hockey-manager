using UnityEngine;
using UnityEngine.UI;

public class LineupSlotRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _button;

    private LineupSlotData _slot;
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button button)
    {
        _infoText = infoText;
        _button = button;
    }

    public void Initialize(LineupSlotData slot, GameScreenController screenController)
    {
        _slot = slot;
        _screenController = screenController;

        if (_infoText != null)
        {
            _infoText.text = FormatSlot(slot);
        }

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        if (_slot != null && _screenController != null)
        {
            _screenController.SelectLineupSlot(_slot.SlotType, _slot.LineOrPairNumber, _slot.SlotPosition);
        }
    }

    private static string FormatSlot(LineupSlotData slot)
    {
        if (slot == null)
        {
            return "Слот недоступен";
        }

        string injuryLabel = string.IsNullOrEmpty(slot.InjuryLabel) ? "" : " | " + slot.InjuryLabel;
        return slot.SlotType + " " + slot.LineOrPairNumber + " " + slot.SlotPosition
            + " | " + slot.PlayerName
            + " | " + slot.Position
            + " | OVR " + slot.Overall
            + " | EFF " + slot.EffectiveOverall
            + " | " + slot.PlayerRole
            + " | TOI " + IceTimeConfig.FormatSeconds(slot.EstimatedTimeOnIceSeconds)
            + " | COND " + slot.Condition
            + " | FAT " + slot.Fatigue
            + " | MOR " + slot.Morale
            + (slot.MoraleStatus == MoraleConfig.StatusUnhappy || slot.MoraleStatus == MoraleConfig.StatusVeryUnhappy ? " " + slot.MoraleStatus : "")
            + injuryLabel;
    }
}
