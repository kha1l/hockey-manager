using UnityEngine;
using UnityEngine.UI;

public class RolePlayerRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _button;

    private PlayerUsageData _usage;
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button button)
    {
        _infoText = infoText;
        _button = button;
    }

    public void Initialize(PlayerUsageData usage, GameScreenController screenController)
    {
        _usage = usage;
        _screenController = screenController;

        if (_infoText != null)
        {
            _infoText.text = FormatUsage(usage);
        }

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        if (_usage != null && _screenController != null)
        {
            _screenController.SelectRolePlayer(_usage.PlayerId);
        }
    }

    private static string FormatUsage(PlayerUsageData usage)
    {
        if (usage == null)
        {
            return "Игрок недоступен";
        }

        string status = usage.IsActive ? "Active" : "Scratch";
        string specialTeams = "";
        if (usage.IsOnPowerPlay)
        {
            specialTeams += " PP";
        }

        if (usage.IsOnPenaltyKill)
        {
            specialTeams += " PK";
        }

        return usage.PlayerName
            + " | " + usage.Position
            + " | EFF " + usage.EffectiveOverall
            + " | " + usage.PlayerRole
            + " | " + usage.UsageCategory
            + " | TOI " + IceTimeConfig.FormatSeconds(usage.EstimatedTimeOnIceSeconds)
            + " | COND " + usage.Condition
            + " | FAT " + usage.Fatigue
            + " | " + status
            + specialTeams
            + (usage.IsInjured ? " | INJ" : "");
    }
}
