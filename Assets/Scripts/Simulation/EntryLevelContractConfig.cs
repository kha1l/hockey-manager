public static class EntryLevelContractConfig
{
    public const int MaxEligibleAge = 24;
    public const int SalaryPremiumForRound1 = 250000;
    public const int SalaryPremiumForRound2 = 125000;
    public const int SalaryPremiumForRound3 = 50000;

    public static int GetContractYearsByAge(int age)
    {
        if (age >= 18 && age <= 21)
        {
            return 3;
        }

        if (age >= 22 && age <= 23)
        {
            return 2;
        }

        if (age == 24)
        {
            return 1;
        }

        return 0;
    }

    public static bool IsEligibleForEntryLevelContract(int age)
    {
        return GetContractYearsByAge(age) > 0;
    }
}
