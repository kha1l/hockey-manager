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

        int rowsToShow = UiDisplayLimitConfig.ClampRowCount(sortedSchedule.Count, UiDisplayLimitConfig.MaxCalendarRows);
        int startIndex = FindFirstVisibleGameIndex(sortedSchedule, season.CurrentDay, rowsToShow);
        for (int i = startIndex; i < sortedSchedule.Count && i < startIndex + rowsToShow; i++)
        {
            ScheduleGameData game = sortedSchedule[i];
            ScheduleGameRowView row = Instantiate(_gameRowPrefab, _gamesContainer);
            row.name = "game-" + game.GameNumber.ToString("00") + "-row";
            row.gameObject.SetActive(true);
            row.Initialize(game);
        }

        string limitMessage = UiDisplayLimitConfig.BuildLimitMessage(rowsToShow, sortedSchedule.Count);
        if (!string.IsNullOrEmpty(limitMessage))
        {
            ScheduleGameRowView limitRow = Instantiate(_gameRowPrefab, _gamesContainer);
            limitRow.name = "calendar-limit-row";
            limitRow.gameObject.SetActive(true);
            limitRow.InitializeMessage(limitMessage);
        }
    }

    private static int FindFirstVisibleGameIndex(List<ScheduleGameData> sortedSchedule, int currentDay, int rowsToShow)
    {
        if (sortedSchedule == null || sortedSchedule.Count == 0 || rowsToShow <= 0)
        {
            return 0;
        }

        int targetDay = currentDay <= 0 ? 1 : currentDay - 2;
        if (targetDay < 1)
        {
            targetDay = 1;
        }

        int startIndex = 0;
        for (int i = 0; i < sortedSchedule.Count; i++)
        {
            ScheduleGameData game = sortedSchedule[i];
            if (game != null && game.DayNumber >= targetDay)
            {
                startIndex = i;
                break;
            }
        }

        int maxStart = sortedSchedule.Count - rowsToShow;
        if (maxStart < 0)
        {
            return 0;
        }

        return startIndex > maxStart ? maxStart : startIndex;
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
