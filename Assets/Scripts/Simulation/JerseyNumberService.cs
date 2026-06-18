using System;
using System.Collections.Generic;

public static class JerseyNumberService
{
    public static void EnsureJerseyNumbersForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        team.EnsureRetiredNumbersData();
        HashSet<int> usedNumbers = new HashSet<int>();
        List<PlayerData> players = new List<PlayerData>(team.Players);
        players.Sort(CompareNumberPriority);

        foreach (PlayerData player in players)
        {
            if (player == null || player.IsRetired)
            {
                continue;
            }

            int number = player.JerseyNumber;
            if (number >= RetirementConfig.MinJerseyNumber
                && number <= RetirementConfig.MaxJerseyNumber
                && !usedNumbers.Contains(number)
                && IsNumberAvailable(team, number, player.Id))
            {
                usedNumbers.Add(number);
                continue;
            }

            player.JerseyNumber = GenerateJerseyNumber(player, team);
            usedNumbers.Add(player.JerseyNumber);
        }
    }

    public static void EnsureJerseyNumbersForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureJerseyNumbersForTeam(team);
        }
    }

    public static void EnsureJerseyNumber(PlayerData player, TeamData team)
    {
        if (player == null || team == null || player.IsRetired)
        {
            return;
        }

        if (player.JerseyNumber >= RetirementConfig.MinJerseyNumber
            && player.JerseyNumber <= RetirementConfig.MaxJerseyNumber
            && IsNumberAvailable(team, player.JerseyNumber, player.Id))
        {
            return;
        }

        player.JerseyNumber = GenerateJerseyNumber(player, team);
    }

    public static int GenerateJerseyNumber(PlayerData player, TeamData team)
    {
        if (player == null)
        {
            return RetirementConfig.MinJerseyNumber;
        }

        List<int> candidates = BuildPreferredNumbers(player);
        foreach (int number in candidates)
        {
            if (IsNumberAvailable(team, number, player.Id))
            {
                return number;
            }
        }

        for (int number = RetirementConfig.MinJerseyNumber; number <= RetirementConfig.MaxJerseyNumber; number++)
        {
            if (IsNumberAvailable(team, number, player.Id))
            {
                return number;
            }
        }

        return RetirementConfig.ClampJerseyNumber(StableRange(GetSeed(player), RetirementConfig.MinJerseyNumber, RetirementConfig.MaxJerseyNumber));
    }

    public static bool IsNumberAvailable(TeamData team, int number, string ignorePlayerId = "")
    {
        number = RetirementConfig.ClampJerseyNumber(number);
        if (GetRetiredNumbers(team).Contains(number))
        {
            return false;
        }

        return !GetUsedNumbers(team, ignorePlayerId).Contains(number);
    }

    public static HashSet<int> GetUsedNumbers(TeamData team, string ignorePlayerId = "")
    {
        HashSet<int> usedNumbers = new HashSet<int>();
        if (team == null || team.Players == null)
        {
            return usedNumbers;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player == null
                || player.IsRetired
                || player.Id == ignorePlayerId
                || player.JerseyNumber < RetirementConfig.MinJerseyNumber
                || player.JerseyNumber > RetirementConfig.MaxJerseyNumber)
            {
                continue;
            }

            usedNumbers.Add(player.JerseyNumber);
        }

        return usedNumbers;
    }

    public static HashSet<int> GetRetiredNumbers(TeamData team)
    {
        HashSet<int> retiredNumbers = new HashSet<int>();
        if (team == null)
        {
            return retiredNumbers;
        }

        team.EnsureRetiredNumbersData();
        if (team.RetiredNumbersData == null || team.RetiredNumbersData.RetiredNumbers == null)
        {
            return retiredNumbers;
        }

        foreach (RetiredNumberData retiredNumber in team.RetiredNumbersData.RetiredNumbers)
        {
            if (retiredNumber != null
                && retiredNumber.JerseyNumber >= RetirementConfig.MinJerseyNumber
                && retiredNumber.JerseyNumber <= RetirementConfig.MaxJerseyNumber)
            {
                retiredNumbers.Add(retiredNumber.JerseyNumber);
            }
        }

        return retiredNumbers;
    }

    private static List<int> BuildPreferredNumbers(PlayerData player)
    {
        List<int> numbers = new List<int>();
        string seed = GetSeed(player);
        if (player.Position == "G")
        {
            AddRange(numbers, 30, 39, seed);
            AddRange(numbers, 1, 98, seed + ":fallback");
        }
        else if (player.Position == "D")
        {
            AddRange(numbers, 2, 8, seed);
            AddRange(numbers, 22, 29, seed + ":d2");
            AddRange(numbers, 44, 59, seed + ":d3");
            AddRange(numbers, 1, 98, seed + ":fallback");
        }
        else
        {
            AddRange(numbers, 9, 21, seed);
            AddRange(numbers, 40, 98, seed + ":f2");
            AddRange(numbers, 1, 98, seed + ":fallback");
        }

        return numbers;
    }

    private static void AddRange(List<int> numbers, int min, int max, string seed)
    {
        int start = StableRange(seed, min, max);
        for (int number = start; number <= max; number++)
        {
            AddUnique(numbers, number);
        }

        for (int number = min; number < start; number++)
        {
            AddUnique(numbers, number);
        }
    }

    private static void AddUnique(List<int> numbers, int number)
    {
        if (!numbers.Contains(number))
        {
            numbers.Add(number);
        }
    }

    private static int CompareNumberPriority(PlayerData left, PlayerData right)
    {
        int ageComparison = (right == null ? 0 : right.Age).CompareTo(left == null ? 0 : left.Age);
        if (ageComparison != 0)
        {
            return ageComparison;
        }

        int overallComparison = (right == null ? 0 : right.Overall).CompareTo(left == null ? 0 : left.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        return string.Compare(left == null ? "" : left.Id, right == null ? "" : right.Id, StringComparison.Ordinal);
    }

    private static string GetSeed(PlayerData player)
    {
        return player == null
            ? "player"
            : (string.IsNullOrEmpty(player.Id) ? player.FirstName + "|" + player.LastName : player.Id);
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 23;
            string source = string.IsNullOrEmpty(value) ? "seed" : value;
            for (int i = 0; i < source.Length; i++)
            {
                hash = hash * 31 + source[i];
            }

            long positive = hash;
            if (positive < 0)
            {
                positive = -positive;
            }

            return (int)(positive % int.MaxValue);
        }
    }

    private static int StableRange(string seed, int minInclusive, int maxInclusive)
    {
        if (maxInclusive <= minInclusive)
        {
            return minInclusive;
        }

        int span = maxInclusive - minInclusive + 1;
        return minInclusive + StableHash(seed) % span;
    }
}
