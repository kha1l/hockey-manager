using UnityEngine;
using UnityEngine.UI;

public class PlayoffSeriesRowView : MonoBehaviour
{
    [SerializeField] private Text _text;

    public void Configure(Text text)
    {
        _text = text;
    }

    public void Initialize(PlayoffSeriesData series)
    {
        string status = series.IsCompleted ? "Завершена" : "Идёт";
        _text.text = series.RoundName
            + " | " + series.TeamAName + " " + series.TeamAWins
            + " - " + series.TeamBWins + " " + series.TeamBName
            + " | " + status;
    }
}
