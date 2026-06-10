using UnityEngine;
using UnityEngine.UI;

public class DraftPickRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(DraftPickData pick)
    {
        if (pick == null)
        {
            _infoText.text = "Выбор недоступен";
            return;
        }

        string selected = string.IsNullOrEmpty(pick.SelectedProspectName)
            ? "Не выбран"
            : pick.SelectedProspectName;

        _infoText.text = "#" + pick.OverallPick
            + " | R" + pick.Round + "." + pick.PickInRound
            + " | original " + pick.OriginalTeamName
            + " | owner " + pick.TeamName
            + " | " + selected
            + " | " + (pick.IsCompleted ? "Done" : "Pending");
    }
}
