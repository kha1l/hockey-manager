using UnityEngine;
using UnityEngine.UI;

public class RetiredPlayerRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(RetiredPlayerData player)
    {
        if (_infoText == null)
        {
            return;
        }

        if (player == null)
        {
            _infoText.text = "Retired player unavailable";
            return;
        }

        string number = player.JerseyNumber > 0 ? "#" + player.JerseyNumber + " " : "";
        string badges = (player.IsHallOfFameInducted ? " [HOF]" : "")
            + (player.HasRetiredNumber ? " [# RET]" : "");

        _infoText.text = number + SafeText(player.PlayerName)
            + badges
            + "\n" + SafeText(player.Position)
            + " | " + SafeText(player.PrimaryTeamName)
            + " | " + player.RetirementSeasonEndYear
            + " | " + player.CareerGamesPlayed + " GP"
            + " | " + player.CareerPoints + "P"
            + " | HOF " + player.HallOfFameScore;
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "n/a" : value;
    }
}
