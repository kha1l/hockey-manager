using System.Collections.Generic;
using UnityEngine;

public class CalendarController : MonoBehaviour
{
    [SerializeField] private Transform _gamesContainer;
    [SerializeField] private ScheduleGameRowView _gameRowPrefab;

    public void Configure(Transform gamesContainer, ScheduleGameRowView gameRowPrefab)
    {
        _gamesContainer = gamesContainer;
        _gameRowPrefab = gameRowPrefab;
    }

    public void ShowCalendar(SeasonData season)
    {
        if (_gamesContainer == null || _gameRowPrefab == null)
        {
            Debug.LogError("CalendarController: UI references are not configured.");
            return;
        }

        ClearRows();
        _gameRowPrefab.gameObject.SetActive(false);

        if (season == null)
        {
            Debug.LogWarning("CalendarController: season is not available.");
            return;
        }

        season.EnsureCollections();
        List<ScheduleGameData> sortedSchedule = new List<ScheduleGameData>(season.Schedule);
        sortedSchedule.Sort(CompareGames);

        foreach (ScheduleGameData game in sortedSchedule)
        {
            ScheduleGameRowView row = Instantiate(_gameRowPrefab, _gamesContainer);
            row.name = "game-" + game.GameNumber.ToString("00") + "-row";
            row.gameObject.SetActive(true);
            row.Initialize(game);
        }
    }

    private void ClearRows()
    {
        for (int i = _gamesContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _gamesContainer.GetChild(i);
            if (child == _gameRowPrefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private static int CompareGames(ScheduleGameData left, ScheduleGameData right)
    {
        int dayComparison = left.DayNumber.CompareTo(right.DayNumber);
        return dayComparison != 0 ? dayComparison : left.GameNumber.CompareTo(right.GameNumber);
    }
}
