using System.Collections.Generic;

public static class ProspectArchetypeConfig
{
    public const string EliteSniper = "EliteSniper";
    public const string TwoWayCenter = "TwoWayCenter";
    public const string PlaymakingWinger = "PlaymakingWinger";
    public const string PowerForward = "PowerForward";
    public const string CheckingForward = "CheckingForward";
    public const string DepthForward = "DepthForward";
    public const string BoomBustWinger = "BoomBustWinger";
    public const string RawScoringForward = "RawScoringForward";

    public const string OffensiveDefenseman = "OffensiveDefenseman";
    public const string PuckMovingDefenseman = "PuckMovingDefenseman";
    public const string ShutdownDefenseman = "ShutdownDefenseman";
    public const string TwoWayDefenseman = "TwoWayDefenseman";
    public const string StayAtHomeDefenseman = "StayAtHomeDefenseman";
    public const string RawDefenseman = "RawDefenseman";
    public const string LateBloomingDefenseman = "LateBloomingDefenseman";

    public const string FranchiseGoalie = "FranchiseGoalie";
    public const string StarterGoalie = "StarterGoalie";
    public const string SafeBackupGoalie = "SafeBackupGoalie";
    public const string RawGoalie = "RawGoalie";
    public const string BoomBustGoalie = "BoomBustGoalie";

    public static List<string> GetArchetypesForPosition(string position)
    {
        if (position == "D")
        {
            return new List<string>
            {
                OffensiveDefenseman,
                PuckMovingDefenseman,
                ShutdownDefenseman,
                TwoWayDefenseman,
                StayAtHomeDefenseman,
                RawDefenseman,
                LateBloomingDefenseman
            };
        }

        if (position == "G")
        {
            return new List<string>
            {
                FranchiseGoalie,
                StarterGoalie,
                SafeBackupGoalie,
                RawGoalie,
                BoomBustGoalie
            };
        }

        return new List<string>
        {
            EliteSniper,
            TwoWayCenter,
            PlaymakingWinger,
            PowerForward,
            CheckingForward,
            DepthForward,
            BoomBustWinger,
            RawScoringForward
        };
    }

    public static string GetDefaultRoleForArchetype(string archetype)
    {
        if (archetype == EliteSniper || archetype == PlaymakingWinger || archetype == RawScoringForward)
        {
            return "Scoring Forward";
        }

        if (archetype == TwoWayCenter || archetype == PowerForward || archetype == CheckingForward)
        {
            return "Two-way Forward";
        }

        if (archetype == OffensiveDefenseman || archetype == PuckMovingDefenseman)
        {
            return "Puck-moving Defenseman";
        }

        if (archetype == ShutdownDefenseman || archetype == StayAtHomeDefenseman)
        {
            return "Defensive Defenseman";
        }

        if (archetype == FranchiseGoalie || archetype == StarterGoalie)
        {
            return "Starting Goalie";
        }

        if (archetype == SafeBackupGoalie)
        {
            return "Backup Goalie";
        }

        return "Development Prospect";
    }

    public static int GetOverallModifierForArchetype(string archetype)
    {
        if (archetype == TwoWayCenter || archetype == ShutdownDefenseman || archetype == SafeBackupGoalie)
        {
            return 2;
        }

        if (archetype == EliteSniper || archetype == CheckingForward || archetype == FranchiseGoalie)
        {
            return 1;
        }

        if (archetype == BoomBustWinger || archetype == LateBloomingDefenseman || archetype == BoomBustGoalie)
        {
            return -2;
        }

        if (archetype == RawScoringForward || archetype == RawGoalie)
        {
            return -3;
        }

        return 0;
    }

    public static int GetPotentialModifierForArchetype(string archetype)
    {
        if (archetype == RawScoringForward || archetype == FranchiseGoalie || archetype == BoomBustGoalie)
        {
            return 5;
        }

        if (archetype == BoomBustWinger || archetype == RawGoalie)
        {
            return 4;
        }

        if (archetype == EliteSniper || archetype == OffensiveDefenseman)
        {
            return 3;
        }

        if (archetype == LateBloomingDefenseman)
        {
            return 2;
        }

        if (archetype == TwoWayCenter)
        {
            return 1;
        }

        if (archetype == CheckingForward)
        {
            return -2;
        }

        if (archetype == SafeBackupGoalie)
        {
            return -1;
        }

        return 0;
    }

    public static string GetDevelopmentTypeBiasForArchetype(string archetype)
    {
        if (archetype == BoomBustWinger || archetype == BoomBustGoalie)
        {
            return ProspectRiskConfig.DevelopmentTypeBoomBust;
        }

        if (archetype == RawScoringForward || archetype == RawGoalie || archetype == OffensiveDefenseman || archetype == EliteSniper)
        {
            return ProspectRiskConfig.DevelopmentTypeRawTalent;
        }

        if (archetype == LateBloomingDefenseman || archetype == FranchiseGoalie)
        {
            return ProspectRiskConfig.DevelopmentTypeLateBloomer;
        }

        if (archetype == TwoWayCenter || archetype == CheckingForward || archetype == ShutdownDefenseman || archetype == SafeBackupGoalie)
        {
            return ProspectRiskConfig.DevelopmentTypeHighFloor;
        }

        return "";
    }
}
