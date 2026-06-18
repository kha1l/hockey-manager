using UnityEngine;
using UnityEngine.UI;

public class OwnerEvaluationRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(OwnerSeasonEvaluationData evaluation)
    {
        if (_infoText == null)
        {
            return;
        }

        if (evaluation == null)
        {
            _infoText.text = "Оценка сезона недоступна";
            return;
        }

        string delta = evaluation.TrustDelta >= 0 ? "+" + evaluation.TrustDelta : evaluation.TrustDelta.ToString();
        _infoText.text = evaluation.SeasonStartYear + "-" + (evaluation.SeasonEndYear % 100).ToString("D2")
            + " | " + evaluation.TeamName
            + " | Trust " + evaluation.TrustBefore + " -> " + evaluation.TrustAfter
            + " (" + delta + ")"
            + " | Satisfaction " + evaluation.OwnerSatisfaction
            + " | " + evaluation.JobSecurity
            + " | " + evaluation.EvaluationSummary;
    }
}
