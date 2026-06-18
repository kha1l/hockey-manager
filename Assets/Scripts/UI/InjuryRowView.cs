using UnityEngine;
using UnityEngine.UI;

public class InjuryRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(PlayerData player, TeamData team)
    {
        if (_infoText == null)
        {
            return;
        }

        if (player == null)
        {
            _infoText.text = "Травма недоступна";
            return;
        }

        InjuryService.EnsureInjuryFields(player);
        string teamName = team == null ? "" : TeamIdentityService.GetDisplayName(team) + " | ";
        _infoText.text = teamName + PlayerDisplayFormatter.FormatPlayerCompact(player)
            + "\n" + player.InjuryType
            + " | " + player.InjurySeverity
            + " | " + player.InjuryDaysRemaining + " дн."
            + " | Возврат: " + player.ExpectedReturnDate;
    }

    public void Initialize(InjuryRecordData injury)
    {
        if (_infoText == null)
        {
            return;
        }

        if (injury == null)
        {
            _infoText.text = "Запись травмы недоступна";
            return;
        }

        string status = string.IsNullOrEmpty(injury.Status) ? "Active" : injury.Status;
        _infoText.text = injury.PlayerName
            + " | " + injury.TeamName
            + " | " + injury.Position
            + "\n" + injury.InjuryType
            + " | " + injury.InjurySeverity
            + " | " + injury.InjuryDays + " дн."
            + " | " + status
            + " | " + injury.Source;
    }
}
