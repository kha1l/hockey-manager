using System;

[Serializable]
public class StaffEffectSummaryData
{
    public string TeamId;
    public string TeamName;
    public string HeadCoachName;
    public string CoachingStyle;
    public int TeamRatingModifier;
    public int OffenseModifier;
    public int DefenseModifier;
    public int PowerPlayModifier;
    public int PenaltyKillModifier;
    public int DevelopmentModifier;
    public int GoalieDevelopmentModifier;
    public int MoraleModifier;
    public int ChemistryModifier;
    public int DisciplineModifier;
    public string Summary;
    public string UpdatedAtUtc;
}
