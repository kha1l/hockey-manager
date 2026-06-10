public static class TacticsConfig
{
    public static TeamTacticsData CreateBalanced(string teamId)
    {
        return Create(teamId, "Balanced", 50, 50, 45, 50, 50, 45);
    }

    public static TeamTacticsData CreateOffensive(string teamId)
    {
        return Create(teamId, "Offensive", 75, 35, 50, 70, 75, 65);
    }

    public static TeamTacticsData CreateDefensive(string teamId)
    {
        return Create(teamId, "Defensive", 35, 75, 35, 40, 40, 25);
    }

    public static TeamTacticsData CreateAggressive(string teamId)
    {
        return Create(teamId, "Aggressive", 60, 45, 80, 70, 65, 75);
    }

    public static TeamTacticsData CreateByPreset(string teamId, string presetName)
    {
        if (presetName == "Offensive")
        {
            return CreateOffensive(teamId);
        }

        if (presetName == "Defensive")
        {
            return CreateDefensive(teamId);
        }

        if (presetName == "Aggressive")
        {
            return CreateAggressive(teamId);
        }

        return CreateBalanced(teamId);
    }

    private static TeamTacticsData Create(
        string teamId,
        string presetName,
        int offensiveFocus,
        int defensiveFocus,
        int aggressiveness,
        int tempo,
        int shootingFrequency,
        int riskLevel)
    {
        TeamTacticsData tactics = new TeamTacticsData
        {
            TeamId = teamId,
            PresetName = presetName,
            OffensiveFocus = offensiveFocus,
            DefensiveFocus = defensiveFocus,
            Aggressiveness = aggressiveness,
            Tempo = tempo,
            ShootingFrequency = shootingFrequency,
            RiskLevel = riskLevel
        };
        tactics.Touch();
        return tactics;
    }
}
