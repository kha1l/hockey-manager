using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ScratchPlayerRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(PlayerData player)
    {
        if (_infoText == null)
        {
            return;
        }

        if (player == null)
        {
            _infoText.text = "Запасной игрок недоступен";
            return;
        }

        PlayerFatigueService.EnsureFatigueFields(player);
        InjuryService.EnsureInjuryFields(player);
        PlayerRoleService.EnsureRole(player);
        _infoText.text = player.FirstName + " " + player.LastName
            + " | " + player.Position
            + " | " + player.Age
            + " лет | OVR " + player.Overall
            + " | EFF " + PlayerFatigueService.GetEffectiveOverall(player)
            + " | " + player.PlayerRole
            + " | TOI " + IceTimeConfig.FormatSeconds(player.EstimatedTimeOnIceSeconds)
            + " | COND " + player.Condition
            + " | FAT " + player.Fatigue
            + " | POT " + player.Potential
            + " | $" + FormatMoney(player.Salary)
            + (player.IsInjured ? " | INJ " + player.InjuryDaysRemaining + " дн." : "");
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
