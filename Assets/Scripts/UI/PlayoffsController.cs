using UnityEngine;

public class PlayoffsController : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Text _statusText;
    [SerializeField] private Transform _seriesContainer;
    [SerializeField] private PlayoffSeriesRowView _seriesRowPrefab;

    public void Configure(UnityEngine.UI.Text statusText, Transform seriesContainer, PlayoffSeriesRowView seriesRowPrefab)
    {
        _statusText = statusText;
        _seriesContainer = seriesContainer;
        _seriesRowPrefab = seriesRowPrefab;
    }

    public void ShowPlayoffs(SeasonData season)
    {
        if (_seriesContainer == null || _seriesRowPrefab == null)
        {
            Debug.LogError("PlayoffsController: UI references are not configured.");
            return;
        }

        ClearRows();
        _seriesRowPrefab.gameObject.SetActive(false);

        if (!PlayoffService.IsPlayoffAvailable(season))
        {
            SetStatus("Плей-офф станет доступен после завершения регулярного сезона");
            return;
        }

        if (season.Playoffs == null || !season.Playoffs.IsStarted)
        {
            GameSession.EnsurePlayoffs();
        }

        PlayoffData playoffs = season.Playoffs;
        if (playoffs == null || !playoffs.IsStarted)
        {
            SetStatus("Плей-офф пока не создан");
            return;
        }

        if (playoffs.IsCompleted)
        {
            SetStatus("Чемпион: " + playoffs.ChampionTeamName);
        }
        else
        {
            SetStatus("Текущий раунд: " + playoffs.CurrentRoundNumber);
        }

        playoffs.EnsureRounds();
        foreach (PlayoffRoundData round in playoffs.Rounds)
        {
            if (round == null)
            {
                continue;
            }

            round.EnsureSeries();
            foreach (PlayoffSeriesData series in round.Series)
            {
                PlayoffSeriesRowView row = Instantiate(_seriesRowPrefab, _seriesContainer);
                row.name = series.SeriesId + "-row";
                row.gameObject.SetActive(true);
                row.Initialize(series);
            }
        }
    }

    private void SetStatus(string status)
    {
        if (_statusText != null)
        {
            _statusText.text = status;
        }
    }

    private void ClearRows()
    {
        for (int i = _seriesContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _seriesContainer.GetChild(i);
            if (child == _seriesRowPrefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }
}
