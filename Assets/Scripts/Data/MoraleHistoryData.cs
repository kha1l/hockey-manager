using System;
using System.Collections.Generic;

[Serializable]
public class MoraleHistoryData
{
    public List<MoraleEventData> Events = new List<MoraleEventData>();
    public int TotalEvents;
    public string LastEventAtUtc;

    public MoraleHistoryData()
    {
        EnsureEvents();
    }

    public void EnsureEvents()
    {
        if (Events == null)
        {
            Events = new List<MoraleEventData>();
        }

        if (LastEventAtUtc == null)
        {
            LastEventAtUtc = "";
        }

        if (TotalEvents < Events.Count)
        {
            TotalEvents = Events.Count;
        }
    }
}
