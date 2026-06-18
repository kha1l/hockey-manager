using System;

[Serializable]
public class MoraleEventData
{
    public string EventId;
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public string EventType;
    public int MoraleBefore;
    public int MoraleAfter;
    public int Delta;
    public string Reason;
    public string CreatedAtUtc;

    public MoraleEventData()
    {
        EventId = Guid.NewGuid().ToString("N");
        CreatedAtUtc = DateTime.UtcNow.ToString("o");
    }
}
