public static class LeagueCalendarConfig
{
    public static LeagueCalendarData CreateDefaultCalendar()
    {
        return new LeagueCalendarData
        {
            CalendarStatus = "Provisional",
            SeasonStartYear = 2026,
            SeasonEndYear = 2027,
            PreseasonStartDate = "2026-09-15",
            PreseasonEndDate = "2026-09-27",
            RegularSeasonStartDate = "2026-09-28",
            RegularSeasonEndDate = "2027-04-18",
            TradeDeadlineDate = "2027-03-05",
            PlayoffsStartDate = "2027-04-21",
            StanleyCupFinalExpectedEndDate = "2027-06-21",
            DraftStartDate = "2027-06-25",
            DraftEndDate = "2027-06-26",
            FreeAgencyStartDate = "2027-07-01"
        };
    }
}
