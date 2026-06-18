using System;

[Serializable]
public class TeamStaffData
{
    public string TeamId;
    public string TeamName;
    public StaffData HeadCoach;
    public StaffData AssistantCoach;
    public StaffData DevelopmentCoach;
    public StaffData GoalieCoach;
    public int StaffOverall;
    public int StaffOffenseImpact;
    public int StaffDefenseImpact;
    public int StaffPowerPlayImpact;
    public int StaffPenaltyKillImpact;
    public int StaffDevelopmentImpact;
    public int StaffGoalieDevelopmentImpact;
    public int StaffMoraleImpact;
    public int StaffChemistryImpact;
    public int StaffDisciplineImpact;
    public int StaffTacticalFitImpact;
    public string StaffSummary;
    public string UpdatedAtUtc;
}
