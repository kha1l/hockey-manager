using System;

[Serializable]
public class ClubFinanceData
{
    public string TeamId;
    public string TeamName;
    public int Payroll;
    public int SalaryCapUpperLimit;
    public int SalaryCapSpace;
    public int Budget;
    public int RevenueEstimate;
    public int ExpensesEstimate;
    public int ProfitEstimate;
    public int PlayoffRevenueEstimate;
    public int StarPowerScore;
    public int FanInterestScore;
    public int FinancialHealthScore;
    public string FinancialHealthLabel;
    public string FinanceSummary;
    public string UpdatedAtUtc = DateTime.UtcNow.ToString("o");
}
