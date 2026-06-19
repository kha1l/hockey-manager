using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalendarController : MonoBehaviour
{
    private const int CalendarColumns = 7;
    private const int CalendarRows = 6;
    private const float CellSpacingX = 118f;
    private const float CellSpacingY = 105f;
    private const float CellWidth = 104f;
    private const float CellHeight = 92f;
    private const float FirstCellX = -354f;
    private const float FirstCellY = 320f;

    private static readonly string[] MonthNames =
    {
        "Январь",
        "Февраль",
        "Март",
        "Апрель",
        "Май",
        "Июнь",
        "Июль",
        "Август",
        "Сентябрь",
        "Октябрь",
        "Ноябрь",
        "Декабрь"
    };

    private static readonly string[] WeekdayNames = { "ПН", "ВТ", "СР", "ЧТ", "ПТ", "СБ", "ВС" };

    [SerializeField] private Text _statusText;
    [SerializeField] private Transform _gamesContainer;
    [SerializeField] private ScheduleGameRowView _gameRowPrefab;

    public void Configure(Transform gamesContainer, ScheduleGameRowView gameRowPrefab)
    {
        _gamesContainer = gamesContainer;
        _gameRowPrefab = gameRowPrefab;
    }

    public void Configure(Text statusText, Transform gamesContainer, ScheduleGameRowView gameRowPrefab)
    {
        _statusText = statusText;
        Configure(gamesContainer, gameRowPrefab);
    }

    public void ShowCalendar(SeasonData season)
    {
        int selectedDay = season == null ? 1 : season.CurrentDay;
        ShowCalendar(season, selectedDay);
    }

    public void ShowCalendar(SeasonData season, int selectedDay)
    {
        if (_gamesContainer == null)
        {
            Debug.LogError("CalendarController: UI references are not configured.");
            return;
        }

        PrepareCalendarContainer();
        ClearRows();
        if (_gameRowPrefab != null)
        {
            _gameRowPrefab.gameObject.SetActive(false);
        }

        if (season == null)
        {
            Debug.LogWarning("CalendarController: season is not available.");
            UpdateStatusText("Календарь пока недоступен.");
            return;
        }

        season.EnsureCollections();
        List<ScheduleGameData> sortedSchedule = new List<ScheduleGameData>(season.Schedule);
        sortedSchedule.Sort(CompareGames);

        int maxDay = GetMaxDay(sortedSchedule);
        selectedDay = Mathf.Clamp(selectedDay <= 0 ? season.CurrentDay : selectedDay, 1, Mathf.Max(1, maxDay));
        DateTime regularSeasonStart = GetRegularSeasonStart();
        DateTime regularSeasonEnd = GetRegularSeasonEnd(regularSeasonStart);
        DateTime selectedDate = GetDateForSeasonDay(selectedDay, maxDay, regularSeasonStart, regularSeasonEnd);
        DateTime currentDate = GetDateForSeasonDay(Mathf.Max(1, season.CurrentDay), maxDay, regularSeasonStart, regularSeasonEnd);
        Dictionary<DateTime, List<ScheduleGameData>> gamesByDate = BuildGamesByDate(sortedSchedule, maxDay, regularSeasonStart, regularSeasonEnd);

        string userTeamId = GetUserTeamId();
        string monthTitle = MonthNames[Mathf.Clamp(selectedDate.Month - 1, 0, MonthNames.Length - 1)] + " " + selectedDate.Year;
        UpdateStatusText(monthTitle + " | Текущий день: " + season.CurrentDay + " | Выбран день: " + selectedDay);

        CreateCalendarBackground();
        CreateText(_gamesContainer, "MonthTitle", monthTitle.ToUpperInvariant(), 30, new Vector2(0f, 455f), new Vector2(760f, 52f), TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
        CreateText(_gamesContainer, "CalendarLegend", "Логотип отмечает игровой день выбранной команды", 14, new Vector2(0f, -458f), new Vector2(760f, 34f), TextAnchor.MiddleCenter, new Color(0.78f, 0.86f, 0.94f, 0.92f), FontStyle.Normal);
        CreateWeekdayHeader();
        CreateMonthGrid(selectedDate, currentDate, selectedDay, maxDay, regularSeasonStart, regularSeasonEnd, gamesByDate, userTeamId);
        CreateSelectedDaySummary(selectedDate, selectedDay, gamesByDate, userTeamId);
    }

    private void ClearRows()
    {
        for (int i = _gamesContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _gamesContainer.GetChild(i);
            if (_gameRowPrefab != null && child == _gameRowPrefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private void PrepareCalendarContainer()
    {
        VerticalLayoutGroup verticalLayoutGroup = _gamesContainer.GetComponent<VerticalLayoutGroup>();
        if (verticalLayoutGroup != null)
        {
            verticalLayoutGroup.enabled = false;
        }

        ContentSizeFitter contentSizeFitter = _gamesContainer.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter != null)
        {
            contentSizeFitter.enabled = false;
        }
    }

    private void CreateCalendarBackground()
    {
        Image backdrop = _gamesContainer.GetComponent<Image>();
        if (backdrop != null)
        {
            backdrop.color = new Color(0.018f, 0.045f, 0.095f, 0.94f);
        }

        CreateImage(_gamesContainer, "IceBandTop", new Color(0.16f, 0.34f, 0.60f, 0.20f), new Vector2(0f, 355f), new Vector2(820f, 82f), 0f);
        CreateImage(_gamesContainer, "IceBandBottom", new Color(0.25f, 0.52f, 0.78f, 0.12f), new Vector2(0f, -360f), new Vector2(820f, 98f), 0f);
        CreateImage(_gamesContainer, "DiagonalIce1", new Color(0.82f, 0.92f, 1f, 0.10f), new Vector2(-280f, 85f), new Vector2(140f, 980f), -28f);
        CreateImage(_gamesContainer, "DiagonalIce2", new Color(0.82f, 0.92f, 1f, 0.08f), new Vector2(305f, 20f), new Vector2(120f, 1020f), 28f);
    }

    private void CreateWeekdayHeader()
    {
        for (int column = 0; column < CalendarColumns; column++)
        {
            Color color = column >= 5 ? new Color(1f, 0.34f, 0.34f, 0.95f) : new Color(0.80f, 0.89f, 1f, 0.92f);
            CreateText(
                _gamesContainer,
                "Weekday" + column,
                WeekdayNames[column],
                16,
                new Vector2(FirstCellX + column * CellSpacingX, 385f),
                new Vector2(CellWidth, 28f),
                TextAnchor.MiddleCenter,
                color,
                FontStyle.Bold);
        }
    }

    private void CreateMonthGrid(
        DateTime selectedDate,
        DateTime currentDate,
        int selectedDay,
        int maxDay,
        DateTime regularSeasonStart,
        DateTime regularSeasonEnd,
        Dictionary<DateTime, List<ScheduleGameData>> gamesByDate,
        string userTeamId)
    {
        DateTime firstOfMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
        int firstColumn = GetMondayBasedColumn(firstOfMonth.DayOfWeek);
        DateTime gridStart = firstOfMonth.AddDays(-firstColumn);

        for (int row = 0; row < CalendarRows; row++)
        {
            for (int column = 0; column < CalendarColumns; column++)
            {
                DateTime cellDate = gridStart.AddDays(row * CalendarColumns + column);
                int seasonDay = GetSeasonDayForDate(cellDate, maxDay, regularSeasonStart, regularSeasonEnd);
                List<ScheduleGameData> games = GetGamesForDate(gamesByDate, cellDate);
                ScheduleGameData userGame = FindUserGame(games, userTeamId);

                CreateDayCell(
                    cellDate,
                    currentDate,
                    selectedDate,
                    seasonDay,
                    selectedDay,
                    userGame,
                    games == null ? 0 : games.Count,
                    userTeamId,
                    new Vector2(FirstCellX + column * CellSpacingX, FirstCellY - row * CellSpacingY));
            }
        }
    }

    private void CreateDayCell(
        DateTime cellDate,
        DateTime currentDate,
        DateTime selectedDate,
        int seasonDay,
        int selectedDay,
        ScheduleGameData userGame,
        int gamesCount,
        string userTeamId,
        Vector2 position)
    {
        bool isCurrentMonth = cellDate.Month == selectedDate.Month && cellDate.Year == selectedDate.Year;
        bool isCurrentDay = cellDate.Date == currentDate.Date;
        bool isSelectedDay = seasonDay > 0 && seasonDay == selectedDay;
        bool isWeekend = cellDate.DayOfWeek == DayOfWeek.Saturday || cellDate.DayOfWeek == DayOfWeek.Sunday;

        Color cellColor = userGame != null
            ? new Color(0.86f, 0.91f, 0.96f, 0.30f)
            : new Color(0.03f, 0.07f, 0.13f, 0.18f);

        if (!isCurrentMonth)
        {
            cellColor.a = 0.07f;
        }

        if (isSelectedDay)
        {
            cellColor = new Color(0.05f, 0.70f, 0.74f, 0.40f);
        }
        else if (isCurrentDay)
        {
            cellColor = new Color(0.94f, 0.80f, 0.26f, 0.34f);
        }

        GameObject cell = CreateImage(_gamesContainer, "Day" + cellDate.Day.ToString("00"), cellColor, position, new Vector2(CellWidth, CellHeight), 0f);
        Color numberColor = isWeekend ? new Color(1f, 0.28f, 0.28f, 0.95f) : Color.white;
        if (!isCurrentMonth)
        {
            numberColor.a = 0.28f;
        }

        CreateText(cell.transform, "DayNumber", cellDate.Day.ToString(), 30, new Vector2(-30f, 25f), new Vector2(56f, 36f), TextAnchor.MiddleLeft, numberColor, FontStyle.Bold);

        if (userGame != null)
        {
            TeamData opponent = GetOpponentTeam(userGame, userTeamId);
            Image logo = CreateImageComponent(cell.transform, "OpponentLogo", new Vector2(0f, -8f), new Vector2(58f, 58f));
            logo.sprite = TeamAssetService.LoadLogo(opponent);
            logo.preserveAspect = true;
            logo.color = logo.sprite == null ? TeamIdentityService.GetPrimaryColor(opponent) : Color.white;

            string prefix = userGame.HomeTeamId == userTeamId ? "vs " : "@ ";
            CreateText(
                cell.transform,
                "GameLabel",
                prefix + TeamIdentityService.GetAbbreviation(opponent),
                11,
                new Vector2(0f, -38f),
                new Vector2(96f, 20f),
                TextAnchor.MiddleCenter,
                new Color(0.86f, 0.92f, 1f, 0.95f),
                FontStyle.Bold);
        }
        else if (gamesCount > 0 && isCurrentMonth)
        {
            CreateText(
                cell.transform,
                "LeagueGamesLabel",
                gamesCount + " игр",
                10,
                new Vector2(22f, -31f),
                new Vector2(56f, 18f),
                TextAnchor.MiddleRight,
                new Color(0.54f, 0.72f, 0.88f, 0.80f),
                FontStyle.Normal);
        }
    }

    private void CreateSelectedDaySummary(
        DateTime selectedDate,
        int selectedDay,
        Dictionary<DateTime, List<ScheduleGameData>> gamesByDate,
        string userTeamId)
    {
        List<ScheduleGameData> games = GetGamesForDate(gamesByDate, selectedDate);
        ScheduleGameData userGame = FindUserGame(games, userTeamId);
        string summary = "День " + selectedDay + " | " + selectedDate.ToString("yyyy-MM-dd");
        if (userGame != null)
        {
            TeamData opponent = GetOpponentTeam(userGame, userTeamId);
            summary += " | " + (userGame.HomeTeamId == userTeamId ? "дома против " : "в гостях против ")
                + TeamIdentityService.GetDisplayName(opponent);
        }
        else
        {
            summary += " | матчей выбранной команды нет";
        }

        CreateText(_gamesContainer, "SelectedDaySummary", summary, 18, new Vector2(0f, -410f), new Vector2(790f, 42f), TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
    }

    private static Dictionary<DateTime, List<ScheduleGameData>> BuildGamesByDate(
        List<ScheduleGameData> schedule,
        int maxDay,
        DateTime regularSeasonStart,
        DateTime regularSeasonEnd)
    {
        Dictionary<DateTime, List<ScheduleGameData>> gamesByDate = new Dictionary<DateTime, List<ScheduleGameData>>();
        if (schedule == null)
        {
            return gamesByDate;
        }

        foreach (ScheduleGameData game in schedule)
        {
            if (game == null)
            {
                continue;
            }

            DateTime date = GetDateForSeasonDay(game.DayNumber, maxDay, regularSeasonStart, regularSeasonEnd).Date;
            if (!gamesByDate.TryGetValue(date, out List<ScheduleGameData> games))
            {
                games = new List<ScheduleGameData>();
                gamesByDate.Add(date, games);
            }

            games.Add(game);
        }

        return gamesByDate;
    }

    private static DateTime GetRegularSeasonStart()
    {
        LeagueCalendarData calendar = GameSession.CurrentState == null || GameSession.CurrentState.LeagueCalendar == null
            ? LeagueCalendarConfig.CreateDefaultCalendar()
            : GameSession.CurrentState.LeagueCalendar;
        return TryParseDate(calendar.RegularSeasonStartDate, new DateTime(2026, 9, 28));
    }

    private static DateTime GetRegularSeasonEnd(DateTime regularSeasonStart)
    {
        LeagueCalendarData calendar = GameSession.CurrentState == null || GameSession.CurrentState.LeagueCalendar == null
            ? LeagueCalendarConfig.CreateDefaultCalendar()
            : GameSession.CurrentState.LeagueCalendar;
        DateTime fallback = regularSeasonStart.AddDays(202);
        DateTime end = TryParseDate(calendar.RegularSeasonEndDate, fallback);
        return end < regularSeasonStart ? fallback : end;
    }

    private static DateTime TryParseDate(string value, DateTime fallback)
    {
        if (DateTime.TryParse(value, out DateTime date))
        {
            return date.Date;
        }

        return fallback.Date;
    }

    private static DateTime GetDateForSeasonDay(int dayNumber, int maxDay, DateTime regularSeasonStart, DateTime regularSeasonEnd)
    {
        int totalCalendarDays = Mathf.Max(1, (int)(regularSeasonEnd.Date - regularSeasonStart.Date).TotalDays + 1);
        if (maxDay <= 1)
        {
            return regularSeasonStart;
        }

        float progress = Mathf.Clamp01((dayNumber - 1f) / (maxDay - 1f));
        int offset = Mathf.Clamp(Mathf.RoundToInt((totalCalendarDays - 1) * progress), 0, totalCalendarDays - 1);
        return regularSeasonStart.AddDays(offset).Date;
    }

    private static int GetSeasonDayForDate(DateTime date, int maxDay, DateTime regularSeasonStart, DateTime regularSeasonEnd)
    {
        if (date.Date < regularSeasonStart.Date || date.Date > regularSeasonEnd.Date)
        {
            return 0;
        }

        int totalCalendarDays = Mathf.Max(1, (int)(regularSeasonEnd.Date - regularSeasonStart.Date).TotalDays + 1);
        if (maxDay <= 1)
        {
            return 1;
        }

        int offset = Mathf.Clamp((int)(date.Date - regularSeasonStart.Date).TotalDays, 0, totalCalendarDays - 1);
        float progress = totalCalendarDays <= 1 ? 0f : offset / (float)(totalCalendarDays - 1);
        return Mathf.Clamp(1 + Mathf.RoundToInt((maxDay - 1) * progress), 1, maxDay);
    }

    private static int GetMondayBasedColumn(DayOfWeek dayOfWeek)
    {
        return dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;
    }

    private static List<ScheduleGameData> GetGamesForDate(Dictionary<DateTime, List<ScheduleGameData>> gamesByDate, DateTime date)
    {
        if (gamesByDate != null && gamesByDate.TryGetValue(date.Date, out List<ScheduleGameData> games))
        {
            return games;
        }

        return null;
    }

    private static ScheduleGameData FindUserGame(List<ScheduleGameData> games, string userTeamId)
    {
        if (games == null || string.IsNullOrEmpty(userTeamId))
        {
            return null;
        }

        foreach (ScheduleGameData game in games)
        {
            if (game == null)
            {
                continue;
            }

            if (game.HomeTeamId == userTeamId || game.AwayTeamId == userTeamId)
            {
                return game;
            }
        }

        return null;
    }

    private static TeamData GetOpponentTeam(ScheduleGameData game, string userTeamId)
    {
        if (game == null)
        {
            return null;
        }

        string opponentId = game.HomeTeamId == userTeamId ? game.AwayTeamId : game.HomeTeamId;
        return FindTeam(opponentId);
    }

    private static TeamData FindTeam(string teamId)
    {
        if (GameSession.CurrentState == null || GameSession.CurrentState.Teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in GameSession.CurrentState.Teams)
        {
            if (team != null && team.Id == teamId)
            {
                TeamIdentityService.EnsureTeamIdentity(team);
                return team;
            }
        }

        return null;
    }

    private static string GetUserTeamId()
    {
        if (GameSession.CurrentTeam != null && !string.IsNullOrEmpty(GameSession.CurrentTeam.Id))
        {
            return GameSession.CurrentTeam.Id;
        }

        return GameSession.CurrentState == null ? "" : GameSession.CurrentState.SelectedTeamId;
    }

    private static GameObject CreateImage(Transform parent, string objectName, Color color, Vector2 anchoredPosition, Vector2 size, float rotation)
    {
        GameObject imageObject = new GameObject(objectName);
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);

        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return imageObject;
    }

    private static Image CreateImageComponent(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject imageObject = new GameObject(objectName);
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        Image image = imageObject.AddComponent<Image>();
        image.color = Color.white;
        return image;
    }

    private static Text CreateText(
        Transform parent,
        string objectName,
        string value,
        int fontSize,
        Vector2 anchoredPosition,
        Vector2 size,
        TextAnchor alignment,
        Color color,
        FontStyle fontStyle)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.fontStyle = fontStyle;
        return text;
    }

    private static int CompareGames(ScheduleGameData left, ScheduleGameData right)
    {
        int dayComparison = left.DayNumber.CompareTo(right.DayNumber);
        return dayComparison != 0 ? dayComparison : left.GameNumber.CompareTo(right.GameNumber);
    }

    private void UpdateStatusText(string value)
    {
        if (_statusText != null)
        {
            _statusText.text = value;
        }
    }

    private static int GetMaxDay(List<ScheduleGameData> schedule)
    {
        int maxDay = 1;
        if (schedule == null)
        {
            return maxDay;
        }

        foreach (ScheduleGameData game in schedule)
        {
            if (game != null && game.DayNumber > maxDay)
            {
                maxDay = game.DayNumber;
            }
        }

        return maxDay;
    }
}
