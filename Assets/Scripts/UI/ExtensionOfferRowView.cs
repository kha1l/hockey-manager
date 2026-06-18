using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ExtensionOfferRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(ContractExtensionOfferData offer)
    {
        if (offer == null)
        {
            _infoText.text = "Предложение недоступно";
            return;
        }

        _infoText.text = offer.PlayerName
            + " | " + FormatMoney(offer.OfferedSalary)
            + " x " + offer.OfferedYears
            + " | score " + offer.AcceptanceScore
            + " | " + offer.Decision
            + " | " + offer.DecisionReason
            + " | " + offer.CreatedAtUtc;
    }

    private static string FormatMoney(int value)
    {
        return "$" + value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
