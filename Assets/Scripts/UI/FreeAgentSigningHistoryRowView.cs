using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class FreeAgentSigningHistoryRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(FreeAgentSigningData signing)
    {
        if (signing == null)
        {
            _infoText.text = "История подписания недоступна";
            return;
        }

        string text = signing.PlayerName
            + " | " + signing.TeamName
            + " | $" + FormatMoney(signing.Salary)
            + " | " + signing.ContractYears + " г."
            + " | " + signing.Status;

        if (signing.Status == "Rejected" && !string.IsNullOrEmpty(signing.RejectionReason))
        {
            text += " | " + signing.RejectionReason;
        }

        _infoText.text = text;
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
