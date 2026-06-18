using System;

[Serializable]
public class AlphaBalanceMetricData
{
    public string MetricId = Guid.NewGuid().ToString("N");
    public string Category;
    public string Name;
    public int Value;
    public int MinTarget;
    public int MaxTarget;
    public string Status;
    public string Message;
    public bool Passed;
}
