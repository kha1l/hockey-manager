using System;

[Serializable]
public class TeamFinanceData
{
    public string TeamId;
    public string TeamName;
    public int SalaryCapUpperLimit;
    public int SalaryCapLowerLimit;
    public int Payroll;
    public int CapSpace;
    public int FloorSpace;
    public int PlayerCount;
    public bool IsOverCap;
    public bool IsBelowFloor;
}
