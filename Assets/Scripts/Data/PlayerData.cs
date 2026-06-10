using System;

[Serializable]
public class PlayerData
{
    public string Id;
    public string FirstName;
    public string LastName;
    public string TeamId;
    public string Position;
    public int Age;
    public int Overall;
    public int Potential;
    public int Salary;
    public int ContractYearsRemaining;
    public string ContractStatus;
    public bool HasNoTradeClause;
    public bool IsGeneratedContract;
    public bool IsEntryLevelContract;
    public string SourceProspectId;
    public int DraftRound;
    public int DraftPickOverall;
    public int LastSeasonOverall;
    public int LastSeasonPotential;
    public int LastDevelopmentDelta;
    public string LastDevelopmentType;
    public int Condition;
    public int Fatigue;
    public int ConsecutiveGamesPlayed;
    public int GamesRested;
    public bool IsResting;
    public int LastGameFatigueChange;
    public int LastGameConditionChange;
    public bool IsInjured;
    public string InjuryType;
    public string InjurySeverity;
    public int InjuryDaysRemaining;
    public bool CanPlayThroughInjury;
    public string InjuredAtUtc;
    public string ExpectedReturnDate;
    public int TotalInjuries;
    public string PlayerRole;
    public string UsageCategory;
    public bool IsRoleManual;
    public int EstimatedTimeOnIceSeconds;
    public int LastGameTimeOnIceSeconds;
    public int AverageTimeOnIceSeconds;
    public int TotalTimeOnIceSeconds;
    public int GamesWithTimeOnIce;
}
