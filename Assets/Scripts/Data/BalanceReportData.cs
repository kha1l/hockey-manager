using System;

[Serializable]
public class BalanceReportData
{
    public string ReportId = Guid.NewGuid().ToString("N");
    public int TeamsCount;
    public int PlayersCount;
    public int AverageTeamOverall;
    public int AverageNhlRosterSize;
    public int InvalidRosterTeams;
    public int InvalidLineupTeams;
    public int CapViolationTeams;
    public int AveragePayroll;
    public int AverageCapSpace;
    public int FreeAgentsCount;
    public int WaiverPlayersCount;
    public int InjuredPlayersCount;
    public int AverageMorale;
    public int AverageChemistry;
    public int DraftClassSize;
    public string DraftClassSummary;
    public int NewsCount;
    public int HistorySeasonsCount;
    public int RetiredPlayersCount;
    public int HallOfFameInducteesCount;
    public int RetiredNumbersCount;
    public string Summary;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
}
