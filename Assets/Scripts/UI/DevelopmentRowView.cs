using UnityEngine;
using UnityEngine.UI;

public class DevelopmentRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(PlayerDevelopmentChangeData change)
    {
        if (_infoText == null)
        {
            return;
        }

        if (change == null)
        {
            _infoText.text = "Development change unavailable";
            return;
        }

        _infoText.text = change.PlayerName
            + " | " + change.TeamName
            + " | " + change.EntityType
            + " | " + change.Position
            + " | " + change.Age
            + " | OVR " + change.OldOverall + " -> " + change.NewOverall
            + " (" + FormatSigned(change.OverallDelta) + ")"
            + " | POT " + change.OldPotential + " -> " + change.NewPotential
            + " (" + FormatSigned(change.PotentialDelta) + ")"
            + " | " + change.DevelopmentType
            + " | " + (string.IsNullOrEmpty(change.DevelopmentEvent) ? "Normal" : change.DevelopmentEvent)
            + " | Risk " + change.DevelopmentRiskAtTime
            + " | Staff " + FormatSigned(change.StaffDevelopmentModifier)
            + (string.IsNullOrEmpty(change.StaffDevelopmentSummary) ? "" : " " + change.StaffDevelopmentSummary)
            + " | " + change.Reason;
    }

    private static string FormatSigned(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }
}
