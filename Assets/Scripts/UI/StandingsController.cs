using System.Collections.Generic;
using UnityEngine;

public class StandingsController : MonoBehaviour
{
    [SerializeField] private Transform _standingsContainer;
    [SerializeField] private StandingRowView _standingRowPrefab;

    public void Configure(Transform standingsContainer, StandingRowView standingRowPrefab)
    {
        _standingsContainer = standingsContainer;
        _standingRowPrefab = standingRowPrefab;
    }

    public void ShowStandings(SeasonData season)
    {
        if (_standingsContainer == null || _standingRowPrefab == null)
        {
            Debug.LogError("StandingsController: UI references are not configured.");
            return;
        }

        ClearRows();
        _standingRowPrefab.gameObject.SetActive(false);

        List<TeamStandingData> standings = StandingsService.GetSortedStandings(season);
        for (int i = 0; i < standings.Count; i++)
        {
            StandingRowView row = Instantiate(_standingRowPrefab, _standingsContainer);
            row.name = "standing-" + (i + 1).ToString("00") + "-row";
            row.gameObject.SetActive(true);
            row.Initialize(i + 1, standings[i]);
        }
    }

    private void ClearRows()
    {
        for (int i = _standingsContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _standingsContainer.GetChild(i);
            if (child == _standingRowPrefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }
}
