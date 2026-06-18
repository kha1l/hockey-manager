using System;

[Serializable]
public class DashboardAlertData
{
    public string AlertId;
    public string Category;
    public string Title;
    public string Message;
    public int Priority;
    public string TargetPanel;
}
