using UnityEngine;
using UnityEngine.UI;

public class LineupEligiblePlayerRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _button;

    private PlayerData _player;
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button button)
    {
        _infoText = infoText;
        _button = button;
    }

    public void Initialize(PlayerData player, GameScreenController screenController)
    {
        _player = player;
        _screenController = screenController;
        InjuryService.EnsureInjuryFields(_player);

        if (_infoText != null)
        {
            _infoText.text = FormatPlayer(player);
        }

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClicked);
            _button.interactable = player != null && !player.IsInjured;
        }
    }

    private void OnClicked()
    {
        if (_player == null || _screenController == null)
        {
            return;
        }

        InjuryService.EnsureInjuryFields(_player);
        if (_player.IsInjured)
        {
            Debug.LogWarning("Игрок травмирован");
            return;
        }

        _screenController.SelectLineupPlayer(_player.Id);
    }

    private static string FormatPlayer(PlayerData player)
    {
        if (player == null)
        {
            return "Игрок недоступен";
        }

        PlayerFatigueService.EnsureFatigueFields(player);
        InjuryService.EnsureInjuryFields(player);
        PlayerRoleService.EnsureRole(player);
        MoraleService.InitializePlayerMorale(player);
        string injuryText = player.IsInjured ? " | INJ " + player.InjuryDaysRemaining + " дн." : "";
        return player.FirstName + " " + player.LastName
            + " | " + player.Position
            + " | " + player.Age + " лет"
            + " | OVR " + player.Overall
            + " | POT " + player.Potential
            + " | EFF " + PlayerFatigueService.GetEffectiveOverall(player)
            + " | " + player.PlayerRole
            + " | TOI " + IceTimeConfig.FormatSeconds(player.EstimatedTimeOnIceSeconds)
            + " | COND " + player.Condition
            + " | FAT " + player.Fatigue
            + " | MOR " + player.Morale
            + (player.MoraleStatus == MoraleConfig.StatusUnhappy || player.MoraleStatus == MoraleConfig.StatusVeryUnhappy ? " " + player.MoraleStatus : "")
            + injuryText;
    }
}
