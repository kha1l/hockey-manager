using System;
using System.Collections.Generic;

[Serializable]
public class OwnerProfileData
{
    public string TeamId;
    public string TeamName;
    public int GmTrust;
    public int OwnerSatisfaction;
    public string JobSecurity;
    public string TeamDirection;
    public string ExpectationsSummary;
    public List<OwnerGoalData> CurrentGoals = new List<OwnerGoalData>();
    public OwnerSeasonEvaluationData LastSeasonEvaluation;
    public List<OwnerSeasonEvaluationData> EvaluationHistory = new List<OwnerSeasonEvaluationData>();
    public ClubFinanceData Finances;
    public string UpdatedAtUtc = DateTime.UtcNow.ToString("o");

    public OwnerProfileData()
    {
        EnsureCollections();
    }

    public void EnsureCollections()
    {
        if (CurrentGoals == null)
        {
            CurrentGoals = new List<OwnerGoalData>();
        }

        if (EvaluationHistory == null)
        {
            EvaluationHistory = new List<OwnerSeasonEvaluationData>();
        }

        foreach (OwnerSeasonEvaluationData evaluation in EvaluationHistory)
        {
            if (evaluation != null)
            {
                evaluation.EnsureGoals();
            }
        }

        while (EvaluationHistory.Count > OwnerGoalConfig.MaxEvaluationHistoryToKeep)
        {
            EvaluationHistory.RemoveAt(0);
        }
    }
}
