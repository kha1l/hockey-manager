using System;

public static class LeagueDateService
{
    public static DateTime GetCurrentLeagueDate(GameState state)
    {
        if (state == null || state.LeagueCalendar == null || state.Season == null)
        {
            return DateTime.UtcNow.Date;
        }

        if (!DateTime.TryParse(state.LeagueCalendar.RegularSeasonStartDate, out DateTime startDate)
            || !DateTime.TryParse(state.LeagueCalendar.RegularSeasonEndDate, out DateTime endDate))
        {
            return DateTime.UtcNow.Date;
        }

        int currentDay = state.Season.CurrentDay;
        int maxDay = GetMaxDayNumber(state.Season);
        if (maxDay <= 1 || currentDay <= 1)
        {
            return startDate.Date;
        }

        if (currentDay >= maxDay)
        {
            return endDate.Date;
        }

        double progress = (currentDay - 1) / (double)(maxDay - 1);
        double totalDays = (endDate.Date - startDate.Date).TotalDays;
        return startDate.Date.AddDays(Math.Round(totalDays * progress));
    }

    public static bool IsPastTradeDeadline(GameState state)
    {
        if (state == null || state.LeagueCalendar == null)
        {
            return false;
        }

        if (!DateTime.TryParse(state.LeagueCalendar.TradeDeadlineDate, out DateTime tradeDeadlineDate))
        {
            return false;
        }

        DateTime currentDate = GetCurrentLeagueDate(state);
        return currentDate.Date > tradeDeadlineDate.Date;
    }

    private static int GetMaxDayNumber(SeasonData season)
    {
        if (season == null || season.Schedule == null)
        {
            return 1;
        }

        int maxDay = 1;
        foreach (ScheduleGameData game in season.Schedule)
        {
            if (game != null && game.DayNumber > maxDay)
            {
                maxDay = game.DayNumber;
            }
        }

        return maxDay;
    }
}
