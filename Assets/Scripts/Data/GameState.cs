using System;
using System.Collections.Generic;

[Serializable]
public class GameState
{
    public int SaveVersion;
    public string LeagueIdentityId;
    public int LeagueIdentityVersion;
    public string LeagueDisplayName;
    public string GameDisplayName;
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
    public WaiverWireData WaiverWire;
    public InjuryHistoryData InjuryHistory;
    public List<SeasonHistoryData> SeasonHistory = new List<SeasonHistoryData>();
    public PlayerDevelopmentHistoryData PlayerDevelopmentHistory;
    public CpuRosterManagementReportData LastCpuRosterManagementReport;
    public List<CpuRosterManagementReportData> CpuRosterManagementHistory = new List<CpuRosterManagementReportData>();
    public List<TeamTradeProfileData> TeamTradeProfiles = new List<TeamTradeProfileData>();
    public string LastTradeProfilesUpdatedAtUtc;
    public ScoutingHistoryData ScoutingHistory;
    public MoraleHistoryData MoraleHistory;
    public ContractExtensionHistoryData ContractExtensionHistory;
    public FreeAgencyOfferHistoryData FreeAgencyOfferHistory;
    public List<OwnerSeasonEvaluationData> OwnerEvaluationHistory = new List<OwnerSeasonEvaluationData>();
    public string LastOwnerGoalsUpdatedAtUtc;
    public List<LeagueSeasonHistoryData> LeagueHistory = new List<LeagueSeasonHistoryData>();
    public List<UserTeamSeasonHistoryData> UserTeamHistory = new List<UserTeamSeasonHistoryData>();
    public LeagueRecordsData LeagueRecords;
    public SeasonAwardsData LastSeasonAwards;
    public LeagueSeasonHistoryData LastLeagueSeasonHistory;
    public string LastHistoryUpdatedAtUtc;
    public NewsFeedData NewsFeed;
    public RetiredPlayersData RetiredPlayers;
    public HallOfFameData HallOfFame;
    public List<RetiredNumberData> LeagueRetiredNumbers = new List<RetiredNumberData>();
    public string LastRetirementUpdateAtUtc;
    public int LastRetirementProcessedSeasonStartYear;
    public TutorialData Tutorial;
    public GmCareerData GmCareer;
    public List<GmJobOfferData> ActiveGmJobOffers = new List<GmJobOfferData>();
    public List<GmCareerEventData> GmCareerEvents = new List<GmCareerEventData>();
    public string LastGmCareerUpdateUtc;
    public MigrationReportData LastMigrationReport;
    public GameStateValidationReportData LastValidationReport;
    public BalanceReportData LastBalanceReport;
    public AlphaBalanceReportData LastAlphaBalanceReport;
    public List<AlphaBalanceReportData> AlphaBalanceReportHistory = new List<AlphaBalanceReportData>();
    public string LastAlphaBalanceReportAtUtc;
    public AndroidPerformanceData AndroidPerformance;
    public AndroidReadinessChecklistData AndroidReadinessChecklist;
    public string LastAndroidReadinessCheckAtUtc;
    public string LastStabilityCheckAtUtc;

    public GameState()
    {
        EnsureLeagueIdentity();
        EnsureCareerProgress();
        EnsureMatchHistory();
        EnsureTradeHistory();
        EnsureFreeAgentHistory();
        EnsureDraftData();
        EnsureProspectSigningHistory();
        EnsureWaiverWire();
        EnsureInjuryHistory();
        EnsureSeasonHistory();
        EnsurePlayerDevelopmentHistory();
        EnsureCpuRosterManagementHistory();
        EnsureTeamTradeProfiles();
        EnsureScoutingHistory();
        EnsureMoraleHistory();
        EnsureContractExtensionHistory();
        EnsureFreeAgencyOfferHistory();
        EnsureOwnerEvaluationHistory();
        EnsureLeagueHistory();
        EnsureNewsFeed();
        EnsureRetirementHistory();
        EnsureTutorialData();
        EnsureGmCareerData();
        EnsureDiagnosticsData();
        EnsureAlphaBalanceReports();
        EnsureAndroidPerformanceData();
    }

    public void EnsureLeagueIdentity()
    {
        LeagueIdentityId = FictionalLeagueConfig.LeagueIdentityId;
        LeagueIdentityVersion = FictionalLeagueConfig.LeagueIdentityVersion;
        LeagueDisplayName = FictionalLeagueConfig.LeagueDisplayName;
        GameDisplayName = FictionalLeagueConfig.GameTitle;

        if (Teams == null)
        {
            Teams = new List<TeamData>();
        }

        TeamIdentityService.EnsureTeamIdentities(Teams);
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

    public void EnsureWaiverWire()
    {
        if (WaiverWire == null)
        {
            WaiverWire = new WaiverWireData();
        }

        WaiverWire.EnsureCollections();
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

    public void EnsureCpuRosterManagementHistory()
    {
        if (CpuRosterManagementHistory == null)
        {
            CpuRosterManagementHistory = new List<CpuRosterManagementReportData>();
        }

        foreach (CpuRosterManagementReportData report in CpuRosterManagementHistory)
        {
            if (report != null)
            {
                report.EnsureActions();
            }
        }

        if (LastCpuRosterManagementReport != null)
        {
            LastCpuRosterManagementReport.EnsureActions();
        }

        while (CpuRosterManagementHistory.Count > CpuRosterManagementConfig.MaxReportsToKeep)
        {
            CpuRosterManagementHistory.RemoveAt(0);
        }
    }

    public void EnsureTeamTradeProfiles()
    {
        if (TeamTradeProfiles == null)
        {
            TeamTradeProfiles = new List<TeamTradeProfileData>();
        }

        foreach (TeamTradeProfileData profile in TeamTradeProfiles)
        {
            if (profile != null)
            {
                profile.EnsureCollections();
            }
        }

        if (LastTradeProfilesUpdatedAtUtc == null)
        {
            LastTradeProfilesUpdatedAtUtc = "";
        }
    }

    public void EnsureScoutingHistory()
    {
        if (ScoutingHistory == null)
        {
            ScoutingHistory = new ScoutingHistoryData();
        }

        ScoutingHistory.EnsureReports();
    }

    public void EnsureMoraleHistory()
    {
        if (MoraleHistory == null)
        {
            MoraleHistory = new MoraleHistoryData();
        }

        MoraleHistory.EnsureEvents();
        while (MoraleHistory.Events.Count > MoraleConfig.MaxMoraleEventsToKeep)
        {
            MoraleHistory.Events.RemoveAt(0);
        }
    }

    public void EnsureContractExtensionHistory()
    {
        if (ContractExtensionHistory == null)
        {
            ContractExtensionHistory = new ContractExtensionHistoryData();
        }

        ContractExtensionHistory.EnsureOffers();
        while (ContractExtensionHistory.Offers.Count > ContractExtensionConfig.MaxOffersToKeep)
        {
            ContractExtensionHistory.Offers.RemoveAt(0);
        }
    }

    public void EnsureFreeAgencyOfferHistory()
    {
        if (FreeAgencyOfferHistory == null)
        {
            FreeAgencyOfferHistory = new FreeAgencyOfferHistoryData();
        }

        FreeAgencyOfferHistory.EnsureOffers();
        while (FreeAgencyOfferHistory.Offers.Count > BetterFreeAgencyConfig.MaxOffersToKeep)
        {
            FreeAgencyOfferHistory.Offers.RemoveAt(0);
        }

        FreeAgencyOfferHistory.EnsureOffers();
    }

    public void EnsureOwnerEvaluationHistory()
    {
        if (OwnerEvaluationHistory == null)
        {
            OwnerEvaluationHistory = new List<OwnerSeasonEvaluationData>();
        }

        foreach (OwnerSeasonEvaluationData evaluation in OwnerEvaluationHistory)
        {
            if (evaluation != null)
            {
                evaluation.EnsureGoals();
            }
        }

        while (OwnerEvaluationHistory.Count > OwnerGoalConfig.MaxGlobalEvaluationHistoryToKeep)
        {
            OwnerEvaluationHistory.RemoveAt(0);
        }

        if (LastOwnerGoalsUpdatedAtUtc == null)
        {
            LastOwnerGoalsUpdatedAtUtc = "";
        }
    }

    public void EnsureLeagueHistory()
    {
        if (LeagueHistory == null)
        {
            LeagueHistory = new List<LeagueSeasonHistoryData>();
        }

        foreach (LeagueSeasonHistoryData history in LeagueHistory)
        {
            if (history != null)
            {
                history.EnsureAwards();
            }
        }

        if (UserTeamHistory == null)
        {
            UserTeamHistory = new List<UserTeamSeasonHistoryData>();
        }

        if (LeagueRecords == null)
        {
            LeagueRecords = new LeagueRecordsData();
        }

        LeagueRecords.EnsureRecords();

        if (LastSeasonAwards != null)
        {
            LastSeasonAwards.EnsureAwards();
        }

        if (LastLeagueSeasonHistory != null)
        {
            LastLeagueSeasonHistory.EnsureAwards();
        }

        if (LastHistoryUpdatedAtUtc == null)
        {
            LastHistoryUpdatedAtUtc = "";
        }
    }

    public void EnsureNewsFeed()
    {
        if (NewsFeed == null)
        {
            NewsFeed = new NewsFeedData();
        }

        NewsFeed.EnsureItems();
        while (NewsFeed.Items.Count > NewsConfig.MaxNewsItemsToKeep)
        {
            NewsFeed.Items.RemoveAt(0);
        }
    }

    public void EnsureRetirementHistory()
    {
        if (RetiredPlayers == null)
        {
            RetiredPlayers = new RetiredPlayersData();
        }

        RetiredPlayers.EnsurePlayers();

        if (HallOfFame == null)
        {
            HallOfFame = new HallOfFameData();
        }

        HallOfFame.EnsureInductees();

        if (LeagueRetiredNumbers == null)
        {
            LeagueRetiredNumbers = new List<RetiredNumberData>();
        }

        if (LastRetirementUpdateAtUtc == null)
        {
            LastRetirementUpdateAtUtc = "";
        }

        if (Teams != null)
        {
            foreach (TeamData team in Teams)
            {
                if (team != null)
                {
                    team.EnsureRetiredNumbersData();
                }
            }
        }
    }

    public void EnsureTutorialData()
    {
        TutorialService.EnsureTutorial(this);
    }

    public void EnsureGmCareerData()
    {
        if (ActiveGmJobOffers == null)
        {
            ActiveGmJobOffers = new List<GmJobOfferData>();
        }

        if (GmCareerEvents == null)
        {
            GmCareerEvents = new List<GmCareerEventData>();
        }

        if (GmCareer != null)
        {
            GmCareer.EnsureCollections();
        }

        while (GmCareerEvents.Count > GmJobSecurityConfig.MaxCareerEventsToKeep)
        {
            GmCareerEvents.RemoveAt(0);
        }

        if (LastGmCareerUpdateUtc == null)
        {
            LastGmCareerUpdateUtc = "";
        }
    }

    public void EnsureDiagnosticsData()
    {
        if (LastMigrationReport != null)
        {
            LastMigrationReport.EnsureCollections();
        }

        if (LastValidationReport != null)
        {
            LastValidationReport.EnsureIssues();
        }

        if (LastStabilityCheckAtUtc == null)
        {
            LastStabilityCheckAtUtc = "";
        }
    }

    public void EnsureAndroidPerformanceData()
    {
        if (AndroidPerformance == null)
        {
            AndroidPerformance = new AndroidPerformanceData();
        }

        if (AndroidPerformance.LastRefreshedPanel == null)
        {
            AndroidPerformance.LastRefreshedPanel = "";
        }

        if (AndroidPerformance.LastUpdatedAtUtc == null)
        {
            AndroidPerformance.LastUpdatedAtUtc = "";
        }

        if (LastAndroidReadinessCheckAtUtc == null)
        {
            LastAndroidReadinessCheckAtUtc = "";
        }

        if (AndroidReadinessChecklist != null && AndroidReadinessChecklist.Summary == null)
        {
            AndroidReadinessChecklist.Summary = "";
        }
    }

    public void EnsureAlphaBalanceReports()
    {
        if (AlphaBalanceReportHistory == null)
        {
            AlphaBalanceReportHistory = new List<AlphaBalanceReportData>();
        }

        for (int i = AlphaBalanceReportHistory.Count - 1; i >= 0; i--)
        {
            AlphaBalanceReportData report = AlphaBalanceReportHistory[i];
            if (report == null)
            {
                AlphaBalanceReportHistory.RemoveAt(i);
                continue;
            }

            report.EnsureCollections();
        }

        if (LastAlphaBalanceReport != null)
        {
            LastAlphaBalanceReport.EnsureCollections();
        }

        while (AlphaBalanceReportHistory.Count > AlphaBalanceConfig.MaxReportsToKeep)
        {
            AlphaBalanceReportHistory.RemoveAt(0);
        }

        if (LastAlphaBalanceReportAtUtc == null)
        {
            LastAlphaBalanceReportAtUtc = "";
        }
    }
}
