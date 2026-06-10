using System;

[Serializable]
public class ProspectSigningData
{
    public string SigningId;
    public string ProspectId;
    public string ProspectName;
    public string TeamId;
    public string TeamName;
    public string SignedAtUtc;
    public int Salary;
    public int ContractYears;
    public string Status;
    public string RejectionReason;
}
