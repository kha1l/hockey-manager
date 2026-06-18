using System;

public static class StaffNameGenerator
{
    private static readonly string[] FirstNames =
    {
        "Alex", "Ryan", "Mark", "Daniel", "Victor", "Eric", "Thomas", "Jason",
        "Adam", "Lucas", "Nathan", "Cole", "Grant", "Owen", "Miles", "Evan"
    };

    private static readonly string[] LastNames =
    {
        "Carter", "Miller", "Brooks", "Lawson", "Hayes", "Turner", "Bennett", "Foster",
        "Graham", "Walsh", "Reed", "Porter", "Hughes", "Barker", "Sullivan", "Morgan"
    };

    public static string GenerateStaffName(string seed)
    {
        string safeSeed = seed ?? "";
        string firstName = FirstNames[StableRange(safeSeed + ":first", 0, FirstNames.Length - 1)];
        string lastName = LastNames[StableRange(safeSeed + ":last", 0, LastNames.Length - 1)];
        return firstName + " " + lastName;
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
}
