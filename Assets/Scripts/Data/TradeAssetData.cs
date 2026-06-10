using System;

[Serializable]
public class TradeAssetData
{
    public string AssetType;
    public string PlayerId;
    public string PlayerName;
    public string PickId;
    public int DraftYear;
    public int DraftRound;
    public string OriginalTeamId;
    public string OriginalTeamName;
    public string TeamId;
    public string TeamName;
    public string Position;
    public int Age;
    public int Overall;
    public int Salary;
    public int ContractYearsRemaining;
    public bool HasNoTradeClause;
    public int RetainedSalaryPercent;
    public int EstimatedTradeValue;
}
