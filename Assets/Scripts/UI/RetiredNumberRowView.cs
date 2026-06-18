using UnityEngine;
using UnityEngine.UI;

public class RetiredNumberRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(RetiredNumberData retiredNumber)
    {
        if (_infoText == null)
        {
            return;
        }

        if (retiredNumber == null)
        {
            _infoText.text = "Retired number unavailable";
            return;
        }

        _infoText.text = SafeText(retiredNumber.TeamName)
            + " #" + retiredNumber.JerseyNumber
            + "\n" + SafeText(retiredNumber.PlayerName)
            + " | " + retiredNumber.RetiredNumberYear
            + " | Score " + retiredNumber.RetiredNumberScore;
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "n/a" : value;
    }
}
