using System;
using System.Collections.Generic;

public static class DraftClassGenerator
{
    private static readonly string[] Nationalities =
    {
        "Canada",
        "USA",
        "Sweden",
        "Finland",
        "Czechia",
        "Slovakia",
        "Germany",
        "Switzerland"
    };

    public static List<ProspectData> CreateDraftClass(int draftYear)
    {
        List<ProspectData> prospects = new List<ProspectData>();
        for (int i = 1; i <= DraftConfig.DraftClassSize; i++)
        {
            prospects.Add(CreateProspect(draftYear, i));
        }

        prospects.Sort(CompareProspectsForProjection);
        for (int i = 0; i < prospects.Count; i++)
        {
            prospects[i].ProjectedPick = i + 1;
            prospects[i].ProjectedRound = Math.Min(DraftConfig.DraftRounds, (i / DraftConfig.PicksPerRound) + 1);
        }

        return prospects;
    }

    private static ProspectData CreateProspect(int draftYear, int index)
    {
        string id = "draft-" + draftYear + "-prospect-" + index.ToString("000");
        Random random = new Random(CreateStableSeed(draftYear, index));
        int overall = GenerateOverall(index, random);
        int potential = GeneratePotential(index, overall, random);

        return new ProspectData
        {
            Id = id,
            FirstName = "Prospect",
            LastName = index.ToString("000"),
            Position = GetPosition(index),
            Nationality = Nationalities[random.Next(0, Nationalities.Length)],
            Age = GenerateAge(random),
            Overall = overall,
            Potential = potential,
            ProjectedRound = 3,
            ProjectedPick = index,
            IsDrafted = false,
            DraftedByTeamId = "",
            DraftedByTeamName = "",
            DraftRound = 0,
            DraftPickOverall = 0
        };
    }

    private static int GenerateOverall(int index, Random random)
    {
        if (index <= 4)
        {
            return random.Next(79, 83);
        }

        if (index <= 24)
        {
            return random.Next(69, 79);
        }

        return random.Next(50, 69);
    }

    private static int GeneratePotential(int index, int overall, Random random)
    {
        if (index <= 24)
        {
            return random.Next(Math.Max(overall, 86), 96);
        }

        return random.Next(Math.Max(overall, 65), 86);
    }

    private static int GenerateAge(Random random)
    {
        int roll = random.Next(0, 100);
        if (roll < 82)
        {
            return 18;
        }

        return roll < 94 ? 19 : 20;
    }

    private static string GetPosition(int index)
    {
        int slot = (index - 1) % 20;
        if (slot < 9)
        {
            int forwardSlot = slot % 3;
            if (forwardSlot == 0)
            {
                return "C";
            }

            return forwardSlot == 1 ? "LW" : "RW";
        }

        if (slot < 16)
        {
            return "D";
        }

        return "G";
    }

    private static int CompareProspectsForProjection(ProspectData left, ProspectData right)
    {
        int potentialComparison = right.Potential.CompareTo(left.Potential);
        if (potentialComparison != 0)
        {
            return potentialComparison;
        }

        int overallComparison = right.Overall.CompareTo(left.Overall);
        if (overallComparison != 0)
        {
            return overallComparison;
        }

        return left.Age.CompareTo(right.Age);
    }

    private static int CreateStableSeed(int draftYear, int index)
    {
        return draftYear * 1000 + index * 37;
    }
}
