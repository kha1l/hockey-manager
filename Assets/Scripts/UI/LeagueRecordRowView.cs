using UnityEngine;
using UnityEngine.UI;

public class LeagueRecordRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(LeagueRecordData record)
    {
        if (_infoText == null)
        {
            return;
        }

        if (record == null)
        {
            _infoText.text = "Record unavailable";
            return;
        }

        _infoText.text = record.RecordName
            + " | " + record.ValueLabel
            + "\n" + record.PlayerName
            + " | " + record.TeamName
            + " | " + record.SeasonStartYear + "-" + (record.SeasonEndYear % 100).ToString("D2");
    }
}
