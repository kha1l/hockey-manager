using System;
using System.Collections.Generic;
using UnityEngine;

public static class CoachingStaffService
{
    public static void EnsureStaffForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        if (team.Staff == null)
        {
            team.Staff = StaffGenerator.GenerateTeamStaff(team);
        }

        string seed = team.Id;
        if (team.Staff.HeadCoach == null)
        {
            team.Staff.HeadCoach = StaffGenerator.GenerateStaffForTeam(team, StaffConfig.RoleHeadCoach, seed);
        }

        if (team.Staff.AssistantCoach == null)
        {
            team.Staff.AssistantCoach = StaffGenerator.GenerateStaffForTeam(team, StaffConfig.RoleAssistantCoach, seed);
        }

        if (team.Staff.DevelopmentCoach == null)
        {
            team.Staff.DevelopmentCoach = StaffGenerator.GenerateStaffForTeam(team, StaffConfig.RoleDevelopmentCoach, seed);
        }

        if (team.Staff.GoalieCoach == null)
        {
            team.Staff.GoalieCoach = StaffGenerator.GenerateStaffForTeam(team, StaffConfig.RoleGoalieCoach, seed);
        }

        team.Staff.TeamId = team.Id;
        team.Staff.TeamName = GetTeamName(team);
        NormalizeStaff(team.Staff.HeadCoach, team, StaffConfig.RoleHeadCoach);
        NormalizeStaff(team.Staff.AssistantCoach, team, StaffConfig.RoleAssistantCoach);
        NormalizeStaff(team.Staff.DevelopmentCoach, team, StaffConfig.RoleDevelopmentCoach);
        NormalizeStaff(team.Staff.GoalieCoach, team, StaffConfig.RoleGoalieCoach);
        CalculateTeamStaffEffects(team);
    }

    public static void EnsureStaffForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureStaffForTeam(team);
        }
    }

    public static TeamStaffData CalculateTeamStaffEffects(TeamData team)
    {
        if (team == null)
        {
            return null;
        }

        if (team.Staff == null)
        {
            team.Staff = new TeamStaffData();
        }

        TeamStaffData staff = team.Staff;
        staff.TeamId = team.Id;
        staff.TeamName = GetTeamName(team);
        staff.StaffOffenseImpact = StaffConfig.RatingToModifier(SafeRating(staff.AssistantCoach, "OffenseRating"));
        staff.StaffDefenseImpact = StaffConfig.RatingToModifier(SafeRating(staff.AssistantCoach, "DefenseRating"));
        staff.StaffPowerPlayImpact = StaffConfig.RatingToModifier(SafeRating(staff.AssistantCoach, "PowerPlayRating"));
        staff.StaffPenaltyKillImpact = StaffConfig.RatingToModifier(SafeRating(staff.AssistantCoach, "PenaltyKillRating"));
        staff.StaffDevelopmentImpact = StaffConfig.RatingToModifier(SafeRating(staff.DevelopmentCoach, "DevelopmentRating"));
        staff.StaffGoalieDevelopmentImpact = StaffConfig.RatingToModifier(SafeRating(staff.GoalieCoach, "GoalieDevelopmentRating"));
        staff.StaffMoraleImpact = StaffConfig.RatingToModifier(SafeRating(staff.HeadCoach, "MotivationRating"));
        staff.StaffChemistryImpact = StaffConfig.RatingToModifier((SafeRating(staff.HeadCoach, "LeadershipRating") + SafeRating(staff.HeadCoach, "TacticalFitRating")) / 2);
        staff.StaffDisciplineImpact = GetDisciplineModifier(team);
        staff.StaffTacticalFitImpact = GetTacticalFitModifier(team);
        staff.StaffOverall = AverageOverall(staff.HeadCoach, staff.AssistantCoach, staff.DevelopmentCoach, staff.GoalieCoach);
        staff.StaffSummary = BuildStaffSummary(staff);
        staff.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
        team.Staff = staff;
        return staff;
    }

    public static StaffEffectSummaryData BuildStaffEffectSummary(TeamData team)
    {
        EnsureStaffForTeam(team);
        TeamStaffData staff = team == null ? null : team.Staff;
        StaffData headCoach = staff == null ? null : staff.HeadCoach;
        return new StaffEffectSummaryData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            HeadCoachName = headCoach == null ? "" : headCoach.FullName,
            CoachingStyle = headCoach == null ? StaffConfig.StyleBalanced : headCoach.CoachingStyle,
            TeamRatingModifier = GetTeamRatingModifier(team),
            OffenseModifier = GetOffenseModifier(team),
            DefenseModifier = GetDefenseModifier(team),
            PowerPlayModifier = GetPowerPlayModifier(team),
            PenaltyKillModifier = GetPenaltyKillModifier(team),
            DevelopmentModifier = GetDevelopmentModifier(team, null),
            GoalieDevelopmentModifier = GetGoalieDevelopmentModifier(team, new PlayerData { Position = "G" }),
            MoraleModifier = GetMoraleModifier(team),
            ChemistryModifier = GetChemistryModifier(team),
            DisciplineModifier = GetDisciplineModifier(team),
            Summary = staff == null ? "" : staff.StaffSummary,
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    public static int GetTeamRatingModifier(TeamData team)
    {
        EnsureStaffForTeam(team);
        if (team == null || team.Staff == null)
        {
            return 0;
        }

        int average = Mathf.RoundToInt((team.Staff.StaffOffenseImpact + team.Staff.StaffDefenseImpact + team.Staff.StaffTacticalFitImpact) / 3f);
        return Mathf.Clamp(average, -2, 2);
    }

    public static int GetOffenseModifier(TeamData team)
    {
        EnsureStaffForTeam(team);
        int modifier = team == null || team.Staff == null ? 0 : team.Staff.StaffOffenseImpact;
        if (GetHeadCoachStyle(team) == StaffConfig.StyleOffensive)
        {
            modifier++;
        }

        return ClampStaffModifier(modifier);
    }

    public static int GetDefenseModifier(TeamData team)
    {
        EnsureStaffForTeam(team);
        int modifier = team == null || team.Staff == null ? 0 : team.Staff.StaffDefenseImpact;
        if (GetHeadCoachStyle(team) == StaffConfig.StyleDefensive)
        {
            modifier++;
        }

        return ClampStaffModifier(modifier);
    }

    public static int GetPowerPlayModifier(TeamData team)
    {
        EnsureStaffForTeam(team);
        int modifier = team == null || team.Staff == null ? 0 : team.Staff.StaffPowerPlayImpact;
        if (GetHeadCoachStyle(team) == StaffConfig.StyleOffensive)
        {
            modifier++;
        }

        return ClampStaffModifier(modifier);
    }

    public static int GetPenaltyKillModifier(TeamData team)
    {
        EnsureStaffForTeam(team);
        int modifier = team == null || team.Staff == null ? 0 : team.Staff.StaffPenaltyKillImpact;
        if (GetHeadCoachStyle(team) == StaffConfig.StyleDefensive)
        {
            modifier++;
        }

        return ClampStaffModifier(modifier);
    }

    public static int GetDevelopmentModifier(TeamData team, PlayerData player)
    {
        EnsureStaffForTeam(team);
        if (team == null || team.Staff == null)
        {
            return 0;
        }

        int modifier = team.Staff.StaffDevelopmentImpact;
        StaffData coach = team.Staff.DevelopmentCoach;
        if (player != null && player.Age <= 23 && coach != null && coach.DevelopmentRating >= 80)
        {
            modifier++;
        }

        if (player != null
            && coach != null
            && coach.DevelopmentRating >= 82
            && (player.DevelopmentType == ProspectRiskConfig.DevelopmentTypeRawTalent
                || player.DevelopmentType == ProspectRiskConfig.DevelopmentTypeLateBloomer))
        {
            modifier++;
        }

        return ClampStaffModifier(modifier);
    }

    public static int GetGoalieDevelopmentModifier(TeamData team, PlayerData player)
    {
        if (player == null || player.Position != "G")
        {
            return 0;
        }

        EnsureStaffForTeam(team);
        if (team == null || team.Staff == null)
        {
            return 0;
        }

        int modifier = team.Staff.StaffGoalieDevelopmentImpact;
        if (team.Staff.GoalieCoach != null && team.Staff.GoalieCoach.GoalieDevelopmentRating >= 80)
        {
            modifier++;
        }

        return ClampStaffModifier(modifier);
    }

    public static int GetMoraleModifier(TeamData team)
    {
        EnsureStaffForTeam(team);
        if (team == null || team.Staff == null)
        {
            return 0;
        }

        int modifier = team.Staff.StaffMoraleImpact;
        if (team.Staff.HeadCoach != null && team.Staff.HeadCoach.MotivationRating < 55)
        {
            modifier--;
        }

        return ClampStaffModifier(modifier);
    }

    public static int GetChemistryModifier(TeamData team)
    {
        EnsureStaffForTeam(team);
        if (team == null || team.Staff == null)
        {
            return 0;
        }

        return ClampStaffModifier(team.Staff.StaffChemistryImpact + Mathf.Clamp(GetTacticalFitModifier(team), -1, 1));
    }

    public static int GetDisciplineModifier(TeamData team)
    {
        if (team == null || team.Staff == null)
        {
            return 0;
        }

        int rating = (SafeRating(team.Staff.HeadCoach, "DisciplineRating") + SafeRating(team.Staff.AssistantCoach, "DisciplineRating")) / 2;
        if (rating >= 80)
        {
            return 2;
        }

        if (rating >= 70)
        {
            return 1;
        }

        if (rating >= 60)
        {
            return 0;
        }

        if (rating >= 50)
        {
            return -1;
        }

        return -2;
    }

    public static int GetTacticalFitModifier(TeamData team)
    {
        if (team == null || team.Staff == null)
        {
            return 0;
        }

        TacticsService.EnsureTactics(team);
        string style = GetHeadCoachStyle(team);
        string preset = team.Tactics == null ? StaffConfig.StyleBalanced : team.Tactics.PresetName;
        int modifier = 0;

        if (style == StaffConfig.StyleOffensive && preset == "Offensive")
        {
            modifier = 2;
        }
        else if (style == StaffConfig.StyleDefensive && preset == "Defensive")
        {
            modifier = 2;
        }
        else if (style == StaffConfig.StyleAggressive && preset == "Aggressive")
        {
            modifier = GetDisciplineModifier(team) < 0 ? 0 : 1;
        }
        else if (style == StaffConfig.StyleBalanced && preset == "Balanced")
        {
            modifier = 1;
        }
        else if ((style == StaffConfig.StyleOffensive && preset == "Defensive")
            || (style == StaffConfig.StyleDefensive && preset == "Offensive"))
        {
            modifier = -1;
        }

        return Mathf.Clamp(modifier, -2, 2);
    }

    public static string BuildStaffSummary(TeamStaffData staff)
    {
        if (staff == null)
        {
            return "Staff unavailable";
        }

        string headCoachName = staff.HeadCoach == null ? "No head coach" : staff.HeadCoach.FullName;
        string style = staff.HeadCoach == null ? StaffConfig.StyleBalanced : staff.HeadCoach.CoachingStyle;
        return "Head Coach: " + headCoachName + " - " + style
            + " | Staff " + StaffConfig.GetStaffQualityLabel(staff.StaffOverall)
            + " | Off " + FormatSigned(staff.StaffOffenseImpact)
            + ", Def " + FormatSigned(staff.StaffDefenseImpact)
            + ", Dev " + FormatSigned(staff.StaffDevelopmentImpact)
            + ", Morale " + FormatSigned(staff.StaffMoraleImpact);
    }

    private static int SafeRating(StaffData staff, string ratingName)
    {
        if (staff == null)
        {
            return StaffConfig.AverageRating;
        }

        if (ratingName == "OffenseRating")
        {
            return StaffConfig.ClampRating(staff.OffenseRating);
        }

        if (ratingName == "DefenseRating")
        {
            return StaffConfig.ClampRating(staff.DefenseRating);
        }

        if (ratingName == "PowerPlayRating")
        {
            return StaffConfig.ClampRating(staff.PowerPlayRating);
        }

        if (ratingName == "PenaltyKillRating")
        {
            return StaffConfig.ClampRating(staff.PenaltyKillRating);
        }

        if (ratingName == "DevelopmentRating")
        {
            return StaffConfig.ClampRating(staff.DevelopmentRating);
        }

        if (ratingName == "GoalieDevelopmentRating")
        {
            return StaffConfig.ClampRating(staff.GoalieDevelopmentRating);
        }

        if (ratingName == "MotivationRating")
        {
            return StaffConfig.ClampRating(staff.MotivationRating);
        }

        if (ratingName == "DisciplineRating")
        {
            return StaffConfig.ClampRating(staff.DisciplineRating);
        }

        if (ratingName == "TacticalFitRating")
        {
            return StaffConfig.ClampRating(staff.TacticalFitRating);
        }

        if (ratingName == "LeadershipRating")
        {
            return StaffConfig.ClampRating(staff.LeadershipRating);
        }

        return StaffConfig.ClampRating(staff.Overall);
    }

    private static void NormalizeStaff(StaffData staff, TeamData team, string expectedRole)
    {
        if (staff == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(staff.StaffId))
        {
            staff.StaffId = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrEmpty(staff.FullName))
        {
            staff.FullName = StaffNameGenerator.GenerateStaffName(staff.StaffId);
        }

        if (!StaffConfig.IsValidStaffRole(staff.StaffRole))
        {
            staff.StaffRole = StaffConfig.IsValidStaffRole(expectedRole) ? expectedRole : StaffConfig.RoleAssistantCoach;
        }

        if (!StaffConfig.IsValidCoachingStyle(staff.CoachingStyle))
        {
            staff.CoachingStyle = staff.StaffRole == StaffConfig.RoleDevelopmentCoach
                ? StaffConfig.StyleDevelopment
                : (staff.StaffRole == StaffConfig.RoleGoalieCoach ? StaffConfig.StyleGoalieFocused : StaffConfig.StyleBalanced);
        }

        staff.AssignedTeamId = team == null ? staff.AssignedTeamId : team.Id;
        staff.AssignedTeamName = team == null ? staff.AssignedTeamName : GetTeamName(team);
        staff.OffenseRating = StaffConfig.ClampRating(staff.OffenseRating);
        staff.DefenseRating = StaffConfig.ClampRating(staff.DefenseRating);
        staff.PowerPlayRating = StaffConfig.ClampRating(staff.PowerPlayRating);
        staff.PenaltyKillRating = StaffConfig.ClampRating(staff.PenaltyKillRating);
        staff.DevelopmentRating = StaffConfig.ClampRating(staff.DevelopmentRating);
        staff.GoalieDevelopmentRating = StaffConfig.ClampRating(staff.GoalieDevelopmentRating);
        staff.MotivationRating = StaffConfig.ClampRating(staff.MotivationRating);
        staff.DisciplineRating = StaffConfig.ClampRating(staff.DisciplineRating);
        staff.TacticalFitRating = StaffConfig.ClampRating(staff.TacticalFitRating);
        staff.LeadershipRating = StaffConfig.ClampRating(staff.LeadershipRating);
        staff.Overall = StaffGenerator.CalculateStaffOverall(staff);
        if (string.IsNullOrEmpty(staff.GeneratedAtUtc))
        {
            staff.GeneratedAtUtc = DateTime.UtcNow.ToString("o");
        }
    }

    private static string GetHeadCoachStyle(TeamData team)
    {
        return team == null || team.Staff == null || team.Staff.HeadCoach == null
            ? StaffConfig.StyleBalanced
            : team.Staff.HeadCoach.CoachingStyle;
    }

    private static int AverageOverall(params StaffData[] staffMembers)
    {
        if (staffMembers == null || staffMembers.Length == 0)
        {
            return StaffConfig.AverageRating;
        }

        int total = 0;
        int count = 0;
        foreach (StaffData staff in staffMembers)
        {
            if (staff == null)
            {
                continue;
            }

            total += StaffConfig.ClampRating(staff.Overall);
            count++;
        }

        return count == 0 ? StaffConfig.AverageRating : StaffConfig.ClampRating(total / count);
    }

    private static int ClampStaffModifier(int modifier)
    {
        return Mathf.Clamp(modifier, StaffConfig.MinStaffRatingModifier, StaffConfig.MaxStaffRatingModifier);
    }

    private static string FormatSigned(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
