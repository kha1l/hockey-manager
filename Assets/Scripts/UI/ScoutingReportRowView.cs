using UnityEngine;
using UnityEngine.UI;

public class ScoutingReportRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(ScoutingReportData report)
    {
        if (report == null)
        {
            _infoText.text = "Scouting report недоступен";
            return;
        }

        _infoText.text = report.ProspectName
            + " | " + report.Source
            + " | " + report.ProspectArchetype
            + " | ACC " + report.AccuracyBefore + "% -> " + report.AccuracyAfter + "%"
            + " | OVR " + FormatRange(report.EstimatedOverallMin, report.EstimatedOverallMax)
            + " | POT " + FormatRange(report.EstimatedPotentialMin, report.EstimatedPotentialMax)
            + " | " + report.ScoutingGrade
            + " | " + report.RiskHint
            + " | " + report.DevelopmentTypeHint
            + " | " + report.CeilingHint
            + " | " + report.FloorHint
            + "\n" + report.CreatedAtUtc;
    }

    private static string FormatRange(int min, int max)
    {
        return min == max ? min.ToString() : min + "-" + max;
    }
}
