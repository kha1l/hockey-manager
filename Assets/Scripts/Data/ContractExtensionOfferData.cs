using System;

[Serializable]
public class ContractExtensionOfferData
{
    public string OfferId;
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public int OfferedSalary;
    public int OfferedYears;
    public int ExpectedSalary;
    public int MinimumSalary;
    public int ExpectedYears;
    public int ExtensionInterest;
    public int AcceptanceScore;
    public bool Accepted;
    public string Decision;
    public string DecisionReason;
    public string CreatedAtUtc;
}
