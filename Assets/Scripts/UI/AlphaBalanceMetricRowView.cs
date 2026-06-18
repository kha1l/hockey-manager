using UnityEngine;
using UnityEngine.UI;

public class AlphaBalanceMetricRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(AlphaBalanceMetricData metric)
    {
        if (_infoText == null)
        {
            return;
        }

        if (metric == null)
        {
            _infoText.text = "Metric unavailable";
            return;
        }

        string target = metric.MaxTarget <= 0
            ? "min " + metric.MinTarget
            : metric.MinTarget + ".." + metric.MaxTarget;
        _infoText.text = metric.Category
            + " | " + metric.Name
            + " | value " + metric.Value
            + " | target " + target
            + " | " + metric.Status
            + " | " + metric.Message;
    }
}
