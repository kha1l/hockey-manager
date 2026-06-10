using System;
using System.Collections.Generic;

[Serializable]
public class GameState
{
    public int SaveVersion;
    public string SelectedTeamId;
    public string LastSavedUtc;
    public int TotalGamesSimulated;
    public int CurrentSeasonStartYear;
    public int CurrentSeasonEndYear;
    public int CareerSeasonNumber;
    public MatchResultData LastMatchResult;
    public LeagueRulesData LeagueRules;
    public LeagueCalendarData LeagueCalendar;
    public SeasonData Season;
    public List<TeamData> Teams = new List<TeamData>();
    public List<MatchResultData> MatchHistory = new List<MatchResultData>();
    public TradeHistoryData TradeHistory;
    public FreeAgentPoolData FreeAgentPool;
    public FreeAgentHistoryData FreeAgentHistory;
    public DraftData Draft;
    public DraftHistoryData DraftHistory;
    public List<DraftPickOwnershipData> DraftPickOwnership = new List<DraftPickOwnershipData>();
    public ProspectSigningHistoryData ProspectSigningHistory;
    public InjuryHistoryData InjuryHistory;
    public List<SeasonHistoryData> SeasonHistory = new List<SeasonHistoryData>();
    public PlayerDevelopmentHistoryData PlayerDevelopmentHistory;

    public GameState()
    {
        EnsureCareerProgress();
        EnsureMatchHistory();
        EnsureTradeHistory();
        EnsureFreeAgentHistory();
        EnsureDraftData();
        EnsureProspectSigningHistory();
        EnsureInjuryHistory();
        EnsureSeasonHistory();
        EnsurePlayerDevelopmentHistory();
    }

    public void EnsureCareerProgress()
    {
        if (CurrentSeasonStartYear == 0)
        {
            CurrentSeasonStartYear = LeagueRules != null && LeagueRules.RulesSeasonStartYear > 0
                ? LeagueRules.RulesSeasonStartYear
                : SalaryCapConfig.RulesSeasonStartYear;
        }

        if (CurrentSeasonEndYear == 0)
        {
            CurrentSeasonEndYear = CurrentSeasonStartYear + 1;
        }

        if (CareerSeasonNumber <= 0)
        {
            CareerSeasonNumber = 1;
        }

        EnsureSeasonHistory();
    }

    public void EnsureMatchHistory()
    {
        if (MatchHistory == null)
        {
            MatchHistory = new List<MatchResultData>();
        }
    }

    public void EnsureTradeHistory()
    {
        if (TradeHistory == null)
        {
            TradeHistory = new TradeHistoryData();
        }

        TradeHistory.EnsureTrades();
    }

    public void EnsureFreeAgentHistory()
    {
        if (FreeAgentHistory == null)
        {
            FreeAgentHistory = new FreeAgentHistoryData();
        }

        FreeAgentHistory.EnsureSignings();
    }

    public void EnsureDraftData()
    {
        if (Draft == null)
        {
            Draft = new DraftData();
        }

        Draft.EnsureCollections();

        if (Draft.TotalRounds != 0 && Draft.TotalRounds != DraftConfig.DraftRounds)
        {
            Draft = new DraftData();
            DraftPickOwnership = new List<DraftPickOwnershipData>();
        }

        if (DraftHistory == null)
        {
            DraftHistory = new DraftHistoryData();
        }

        DraftHistory.EnsureCompletedPicks();

        if (DraftPickOwnership == null)
        {
            DraftPickOwnership = new List<DraftPickOwnershipData>();
        }
    }

    public void EnsureProspectSigningHistory()
    {
        if (ProspectSigningHistory == null)
        {
            ProspectSigningHistory = new ProspectSigningHistoryData();
        }

        ProspectSigningHistory.EnsureSignings();
    }

    public void EnsureInjuryHistory()
    {
        if (InjuryHistory == null)
        {
            InjuryHistory = new InjuryHistoryData();
        }

        InjuryHistory.EnsureInjuries();
    }

    public void EnsureSeasonHistory()
    {
        if (SeasonHistory == null)
        {
            SeasonHistory = new List<SeasonHistoryData>();
        }

        foreach (SeasonHistoryData history in SeasonHistory)
        {
            if (history != null)
            {
                history.EnsureCollections();
            }
        }
    }

    public void EnsurePlayerDevelopmentHistory()
    {
        if (PlayerDevelopmentHistory == null)
        {
            PlayerDevelopmentHistory = new PlayerDevelopmentHistoryData();
        }

        PlayerDevelopmentHistory.EnsureChanges();
    }
}
