using UnityEngine;
using UnityEngine.UI;

public class SummaryCardView : MonoBehaviour
{
    public Text TitleText;
    public Text ValueText;
    public Text DetailText;

    public void Initialize(string title, string value, string detail)
    {
        if (TitleText != null)
        {
            TitleText.text = string.IsNullOrEmpty(title) ? "" : title;
        }

        if (ValueText != null)
        {
            ValueText.text = string.IsNullOrEmpty(value) ? "" : value;
        }

        if (DetailText != null)
        {
            DetailText.text = string.IsNullOrEmpty(detail) ? "" : detail;
        }
    }
}
