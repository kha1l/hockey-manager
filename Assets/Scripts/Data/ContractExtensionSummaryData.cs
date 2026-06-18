using System;

[Serializable]
public class ContractExtensionSummaryData
{
    public string TeamId;
    public string TeamName;
    public int EligiblePlayers;
    public int PendingUfaCount;
    public int PendingRfaCount;
    public int ElcExpiringCount;
    public int HighInterestCount;
    public int LowInterestCount;
    public int RefusingCount;
    public string MostImportantPlayerId;
    public string MostImportantPlayerName;
    public string Summary;
    public string UpdatedAtUtc;
}
