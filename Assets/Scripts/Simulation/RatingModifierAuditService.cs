using System.Text;

public static class RatingModifierAuditService
{
    public static string BuildTeamRatingModifierSummary(TeamData team)
    {
        if (team == null)
        {
            return "Rating modifiers: team unavailable";
        }

        StringBuilder builder = new StringBuilder();
        builder.Append("Rating modifiers: tactics ")
            .Append(TacticsService.GetTacticalFitModifier(team))
            .Append(" | chemistry ")
            .Append(ChemistryService.GetTeamChemistryRatingModifier(team))
            .Append(" | staff ")
            .Append(CoachingStaffService.GetTeamRatingModifier(team))
            .Append(" | morale ")
            .Append(EstimateAverageMoraleEffect(team))
            .Append(" | fatigue ")
            .Append(EstimateAverageFatigueEffect(team))
            .Append(" | positive ")
            .Append(EstimateTotalPositiveModifier(team))
            .Append(" | negative ")
            .Append(EstimateTotalNegativeModifier(team));
        return builder.ToString();
    }

    public static int EstimateTotalPositiveModifier(TeamData team)
    {
        int total = 0;
        AddPositive(ref total, TacticsService.GetTacticalFitModifier(team));
        AddPositive(ref total, ChemistryService.GetTeamChemistryRatingModifier(team));
        AddPositive(ref total, CoachingStaffService.GetTeamRatingModifier(team));
        AddPositive(ref total, EstimateAverageMoraleEffect(team));
        AddPositive(ref total, EstimateAverageFatigueEffect(team));
        return total;
    }

    public static int EstimateTotalNegativeModifier(TeamData team)
    {
        int total = 0;
        AddNegative(ref total, TacticsService.GetTacticalFitModifier(team));
        AddNegative(ref total, ChemistryService.GetTeamChemistryRatingModifier(team));
        AddNegative(ref total, CoachingStaffService.GetTeamRatingModifier(team));
        AddNegative(ref total, EstimateAverageMoraleEffect(team));
        AddNegative(ref total, EstimateAverageFatigueEffect(team));
        return total;
    }

    private static int EstimateAverageMoraleEffect(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return 0;
        }

        int total = 0;
        int count = 0;
        foreach (PlayerData player in team.Players)
        {
            if (player == null || player.RosterStatus != RosterStatusConfig.NHL)
            {
                continue;
            }

            int morale = player.HasMoraleInitialized ? player.Morale : MoraleConfig.DefaultMorale;
            total -= MoraleConfig.GetEffectiveOverallPenalty(morale);
            count++;
        }

        return count == 0 ? 0 : total / count;
    }

    private static int EstimateAverageFatigueEffect(TeamData team)
    {
        if (team == null || team.Players == null)
        {
            return 0;
        }

        int total = 0;
        int count = 0;
        foreach (PlayerData player in team.Players)
        {
            if (player == null || player.RosterStatus != RosterStatusConfig.NHL)
            {
                continue;
            }

            total -= PlayerFatigueService.GetOverallPenaltyFromCondition(player);
            count++;
        }

        return count == 0 ? 0 : total / count;
    }

    private static void AddPositive(ref int total, int value)
    {
        if (value > 0)
        {
            total += value;
        }
    }

    private static void AddNegative(ref int total, int value)
    {
        if (value < 0)
        {
            total += value;
        }
    }
}
