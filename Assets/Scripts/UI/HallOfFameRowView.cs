using UnityEngine;
using UnityEngine.UI;

public class HallOfFameRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(HallOfFameInducteeData inductee)
    {
        if (_infoText == null)
        {
            return;
        }

        if (inductee == null)
        {
            _infoText.text = "Hall of Fame inductee unavailable";
            return;
        }

        string number = inductee.JerseyNumber > 0 ? "#" + inductee.JerseyNumber + " " : "";
        _infoText.text = inductee.InductionYear
            + " | " + number + SafeText(inductee.PlayerName)
            + "\n" + SafeText(inductee.PrimaryTeamName)
            + " | " + SafeText(inductee.Position)
            + " | Score " + inductee.HallOfFameScore
            + " | " + inductee.CareerPoints + "P";
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "n/a" : value;
    }
}
