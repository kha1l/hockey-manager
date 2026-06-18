using UnityEngine;
using UnityEngine.UI;

public class ValidationIssueRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(ValidationIssueData issue)
    {
        if (_infoText == null)
        {
            return;
        }

        if (issue == null)
        {
            _infoText.text = "Validation issue unavailable";
            return;
        }

        _infoText.text = issue.Severity
            + " | " + issue.Category
            + " | " + issue.Message
            + (string.IsNullOrEmpty(issue.SuggestedRepair) ? "" : "\nFix: " + issue.SuggestedRepair)
            + (issue.WasRepaired ? " | repaired" : "");
    }
}
