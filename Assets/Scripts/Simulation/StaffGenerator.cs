using System;

public static class StaffGenerator
{
    public static StaffData GenerateStaffForTeam(TeamData team, string staffRole, string seed)
    {
        string role = StaffConfig.IsValidStaffRole(staffRole) ? staffRole : StaffConfig.RoleAssistantCoach;
        string safeSeed = (seed ?? "") + ":" + role;
        string teamId = team == null ? "" : team.Id;
        string teamName = team == null ? "" : GetTeamName(team);

        StaffData staff = new StaffData
        {
            StaffId = "staff-" + teamId + "-" + role.ToLowerInvariant(),
            FullName = StaffNameGenerator.GenerateStaffName(safeSeed),
            StaffRole = role,
            CoachingStyle = DetermineCoachingStyle(team, role, safeSeed),
            AssignedTeamId = teamId,
            AssignedTeamName = teamName,
            ContractYearsRemaining = StableRange(safeSeed + ":years", 1, 4),
            GeneratedAtUtc = DateTime.UtcNow.ToString("o")
        };

        if (role == StaffConfig.RoleHeadCoach)
        {
            staff.Age = StableRange(safeSeed + ":age", 42, 65);
            staff.Salary = StableRange(safeSeed + ":salary", 2000000, 6000000);
            FillRatings(staff, safeSeed, 74, 14);
            staff.MotivationRating = GenerateRating(safeSeed + ":motivation", 76, 14);
            staff.TacticalFitRating = GenerateRating(safeSeed + ":tactical", 76, 14);
            staff.LeadershipRating = GenerateRating(safeSeed + ":leadership", 78, 14);
        }
        else if (role == StaffConfig.RoleAssistantCoach)
        {
            staff.Age = StableRange(safeSeed + ":age", 35, 60);
            staff.Salary = StableRange(safeSeed + ":salary", 800000, 2000000);
            FillRatings(staff, safeSeed, 72, 14);
            staff.OffenseRating = GenerateRating(safeSeed + ":offense", 76, 16);
            staff.DefenseRating = GenerateRating(safeSeed + ":defense", 76, 16);
            staff.PowerPlayRating = GenerateRating(safeSeed + ":pp", 76, 16);
            staff.PenaltyKillRating = GenerateRating(safeSeed + ":pk", 76, 16);
        }
        else if (role == StaffConfig.RoleDevelopmentCoach)
        {
            staff.Age = StableRange(safeSeed + ":age", 32, 58);
            staff.Salary = StableRange(safeSeed + ":salary", 500000, 1500000);
            FillRatings(staff, safeSeed, 70, 14);
            staff.DevelopmentRating = GenerateRating(safeSeed + ":development", 80, 15);
            staff.MotivationRating = GenerateRating(safeSeed + ":motivation", 74, 12);
        }
        else
        {
            staff.Age = StableRange(safeSeed + ":age", 35, 62);
            staff.Salary = StableRange(safeSeed + ":salary", 500000, 1500000);
            FillRatings(staff, safeSeed, 70, 14);
            staff.GoalieDevelopmentRating = GenerateRating(safeSeed + ":goalie-development", 80, 15);
            staff.DefenseRating = GenerateRating(safeSeed + ":defense", 74, 12);
        }

        staff.Overall = CalculateStaffOverall(staff);
        return staff;
    }

    public static TeamStaffData GenerateTeamStaff(TeamData team)
    {
        TeamStaffData staff = new TeamStaffData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = team == null ? "" : GetTeamName(team)
        };

        string teamSeed = team == null ? "unknown-team" : team.Id;
        staff.HeadCoach = GenerateStaffForTeam(team, StaffConfig.RoleHeadCoach, teamSeed);
        staff.AssistantCoach = GenerateStaffForTeam(team, StaffConfig.RoleAssistantCoach, teamSeed);
        staff.DevelopmentCoach = GenerateStaffForTeam(team, StaffConfig.RoleDevelopmentCoach, teamSeed);
        staff.GoalieCoach = GenerateStaffForTeam(team, StaffConfig.RoleGoalieCoach, teamSeed);

        if (team != null)
        {
            team.Staff = staff;
            CoachingStaffService.CalculateTeamStaffEffects(team);
        }

        return staff;
    }

    public static string DetermineCoachingStyle(TeamData team, string staffRole, string seed)
    {
        if (staffRole == StaffConfig.RoleDevelopmentCoach)
        {
            return StaffConfig.StyleDevelopment;
        }

        if (staffRole == StaffConfig.RoleGoalieCoach)
        {
            return StaffConfig.StyleGoalieFocused;
        }

        string[] assistantStyles =
        {
            StaffConfig.StyleBalanced,
            StaffConfig.StyleOffensive,
            StaffConfig.StyleDefensive
        };

        string[] headCoachStyles =
        {
            StaffConfig.StyleBalanced,
            StaffConfig.StyleOffensive,
            StaffConfig.StyleDefensive,
            StaffConfig.StyleAggressive,
            StaffConfig.StyleDevelopment
        };

        string[] styles = staffRole == StaffConfig.RoleAssistantCoach ? assistantStyles : headCoachStyles;
        return styles[StableRange((seed ?? "") + ":style", 0, styles.Length - 1)];
    }

    public static int GenerateRating(string seed, int baseValue, int spread)
    {
        int halfSpread = Math.Max(1, spread);
        int value = baseValue + StableRange(seed, -halfSpread, halfSpread);
        return StaffConfig.ClampRating(value);
    }

    public static int CalculateStaffOverall(StaffData staff)
    {
        if (staff == null)
        {
            return StaffConfig.AverageRating;
        }

        if (staff.StaffRole == StaffConfig.RoleHeadCoach)
        {
            return Average(
                staff.OffenseRating,
                staff.DefenseRating,
                staff.MotivationRating,
                staff.DisciplineRating,
                staff.TacticalFitRating,
                staff.LeadershipRating);
        }

        if (staff.StaffRole == StaffConfig.RoleAssistantCoach)
        {
            return Average(
                staff.OffenseRating,
                staff.DefenseRating,
                staff.PowerPlayRating,
                staff.PenaltyKillRating,
                staff.DisciplineRating,
                staff.TacticalFitRating);
        }

        if (staff.StaffRole == StaffConfig.RoleDevelopmentCoach)
        {
            return Average(staff.DevelopmentRating, staff.MotivationRating, staff.LeadershipRating, staff.TacticalFitRating);
        }

        return Average(staff.GoalieDevelopmentRating, staff.DefenseRating, staff.MotivationRating, staff.TacticalFitRating);
    }

    private static void FillRatings(StaffData staff, string seed, int baseValue, int spread)
    {
        staff.OffenseRating = GenerateRating(seed + ":offense", baseValue, spread);
        staff.DefenseRating = GenerateRating(seed + ":defense", baseValue, spread);
        staff.PowerPlayRating = GenerateRating(seed + ":pp", baseValue, spread);
        staff.PenaltyKillRating = GenerateRating(seed + ":pk", baseValue, spread);
        staff.DevelopmentRating = GenerateRating(seed + ":development", baseValue, spread);
        staff.GoalieDevelopmentRating = GenerateRating(seed + ":goalie-development", baseValue, spread);
        staff.MotivationRating = GenerateRating(seed + ":motivation", baseValue, spread);
        staff.DisciplineRating = GenerateRating(seed + ":discipline", baseValue, spread);
        staff.TacticalFitRating = GenerateRating(seed + ":tactical", baseValue, spread);
        staff.LeadershipRating = GenerateRating(seed + ":leadership", baseValue, spread);
    }

    private static int Average(params int[] values)
    {
        if (values == null || values.Length == 0)
        {
            return StaffConfig.AverageRating;
        }

        int total = 0;
        for (int i = 0; i < values.Length; i++)
        {
            total += StaffConfig.ClampRating(values[i]);
        }

        return StaffConfig.ClampRating(total / values.Length);
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 23;
            string safeValue = value ?? "";
            for (int i = 0; i < safeValue.Length; i++)
            {
                hash = hash * 31 + safeValue[i];
            }

            return hash == int.MinValue ? 0 : Math.Abs(hash);
        }
    }

    private static int StableRange(string seed, int minInclusive, int maxInclusive)
    {
        if (maxInclusive <= minInclusive)
        {
            return minInclusive;
        }

        int range = maxInclusive - minInclusive + 1;
        return minInclusive + (StableHash(seed) % range);
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
