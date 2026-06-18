using System;

[Serializable]
public class FreeAgentFitEvaluationData
{
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public int TeamFitScore;
    public int ContractFitScore;
    public int RoleFitScore;
    public int RosterOpportunityScore;
    public int ContenderScore;
    public int CoachingFitScore;
    public int CapFitScore;
    public int FinalInterestScore;
    public string BestProjectedRole;
    public string Summary;
    public string UpdatedAtUtc = DateTime.UtcNow.ToString("o");
}
