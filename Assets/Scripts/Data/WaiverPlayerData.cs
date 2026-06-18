using System;

[Serializable]
public class WaiverPlayerData
{
    public string WaiverId;
    public string PlayerId;
    public string PlayerName;
    public string Position;
    public int Age;
    public int Overall;
    public int Potential;
    public int Salary;
    public int ContractYearsRemaining;
    public string OriginalTeamId;
    public string OriginalTeamName;
    public string IntendedDestination;
    public string Status;
    public string PlacedAtUtc;
    public string ExpiresAtUtc;
    public int DaysRemaining;
    public bool ClaimedByUser;
    public string ClaimedByTeamId;
    public string ClaimedByTeamName;
    public string ResolvedAtUtc;
    public string Resolution;
}
