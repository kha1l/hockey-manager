using System;

[Serializable]
public class FreeAgentSigningData
{
    public string SigningId;
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public string SignedAtUtc;
    public int Salary;
    public int ContractYears;
    public string Status;
    public string RejectionReason;
}
