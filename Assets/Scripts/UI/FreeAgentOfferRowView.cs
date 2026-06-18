using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class FreeAgentOfferRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(FreeAgentOfferData offer)
    {
        if (offer == null)
        {
            _infoText.text = "Оффер недоступен";
            return;
        }

        string text = offer.PlayerName
            + " | " + offer.TeamName
            + " | " + FormatMoney(offer.OfferedSalary)
            + " x " + offer.OfferedYears
            + " | score " + offer.AcceptanceScore
            + " | " + offer.Decision
            + " | " + offer.Source;

        if (!string.IsNullOrEmpty(offer.DecisionReason))
        {
            text += " | " + offer.DecisionReason;
        }

        _infoText.text = text;
    }

    private static string FormatMoney(int value)
    {
        return "$" + value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
