using UnityEngine;
using UnityEngine.UI;

public class OrganizationPlayerRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _selectButton;

    private string _playerId;
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button selectButton)
    {
        _infoText = infoText;
        _selectButton = selectButton;
    }

    public void Initialize(PlayerData player, GameScreenController screenController)
    {
        _playerId = player == null ? "" : player.Id;
        _screenController = screenController;

        if (_infoText != null)
        {
            _infoText.text = BuildText(player);
        }

        if (_selectButton != null)
        {
            _selectButton.onClick.RemoveAllListeners();
            _selectButton.onClick.AddListener(OnSelectClicked);
        }
    }

    private void OnSelectClicked()
    {
        if (_screenController != null)
        {
            _screenController.SelectOrganizationPlayer(_playerId);
        }
    }

    private static string BuildText(PlayerData player)
    {
        if (player == null)
        {
            return "Игрок не найден";
        }

        WaiverEligibilityService.EnsureWaiverEligibility(player);

        return PlayerDisplayFormatter.FormatPlayerCompact(player)
            + "\n" + player.RosterStatus
            + " | " + MobileUiConfig.FormatMoney(player.Salary)
            + " | " + player.ContractYearsRemaining + "y"
            + " | " + UiBadgeService.FormatBadgesInline(UiBadgeService.BuildPlayerBadges(player), 5)
            + " | W " + MobileUiConfig.FormatShortStatus(player.WaiverStatus);
    }
}
