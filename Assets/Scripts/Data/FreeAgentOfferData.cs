using System;

[Serializable]
public class FreeAgentOfferData
{
    public string OfferId = Guid.NewGuid().ToString("N");
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public int OfferedSalary;
    public int OfferedYears;
    public int ExpectedSalary;
    public int MinimumSalary;
    public int ExpectedYears;
    public int TeamFitScore;
    public int ContractFitScore;
    public int RoleFitScore;
    public int RosterOpportunityScore;
    public int ContenderScore;
    public int CoachingFitScore;
    public int CapFitScore;
    public int FinalInterestScore;
    public int AcceptanceScore;
    public bool Accepted;
    public string Decision;
    public string DecisionReason;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
    public string Source;

    public FreeAgentOfferData()
    {
        if (string.IsNullOrEmpty(OfferId))
        {
            OfferId = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrEmpty(CreatedAtUtc))
        {
            CreatedAtUtc = DateTime.UtcNow.ToString("o");
        }
    }
}
