using System;
using System.Collections.Generic;

[Serializable]
public class OwnerSeasonEvaluationData
{
    public string EvaluationId = Guid.NewGuid().ToString("N");
    public int SeasonStartYear;
    public int SeasonEndYear;
    public string TeamId;
    public string TeamName;
    public string TeamDirection;
    public int RegularSeasonPoints;
    public int LeagueRank;
    public bool MadePlayoffs;
    public int PlayoffRoundsWon;
    public string PlayoffResult;
    public int GoalsCompleted;
    public int GoalsFailed;
    public int GoalsActive;
    public int TrustBefore;
    public int TrustAfter;
    public int TrustDelta;
    public int OwnerSatisfaction;
    public string JobSecurity;
    public string EvaluationSummary;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
    public List<OwnerGoalData> EvaluatedGoals = new List<OwnerGoalData>();

    public OwnerSeasonEvaluationData()
    {
        EnsureGoals();
    }

    public void EnsureGoals()
    {
        if (EvaluatedGoals == null)
        {
            EvaluatedGoals = new List<OwnerGoalData>();
        }
    }
}
