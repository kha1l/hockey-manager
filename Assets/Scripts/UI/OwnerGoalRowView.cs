using UnityEngine;
using UnityEngine.UI;

public class OwnerGoalRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(OwnerGoalData goal)
    {
        if (_infoText == null)
        {
            return;
        }

        if (goal == null)
        {
            _infoText.text = "Цель владельца недоступна";
            return;
        }

        string target = string.IsNullOrEmpty(goal.TargetValueLabel)
            ? goal.TargetValue.ToString()
            : goal.TargetValueLabel;
        string result = string.IsNullOrEmpty(goal.ResultSummary) ? "" : " | " + goal.ResultSummary;
        _infoText.text = goal.GoalType
            + " | " + goal.Title
            + " | " + goal.CurrentValue + " / " + target
            + " | " + goal.ProgressPercent + "%"
            + " | " + goal.Status
            + result;
    }
}
