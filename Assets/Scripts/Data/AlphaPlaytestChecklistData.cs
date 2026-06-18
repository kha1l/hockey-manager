using System;
using System.Collections.Generic;

[Serializable]
public class AlphaPlaytestChecklistData
{
    public List<string> ChecklistItems = new List<string>();
    public string Summary;

    public AlphaPlaytestChecklistData()
    {
        EnsureItems();
    }

    public void EnsureItems()
    {
        if (ChecklistItems == null)
        {
            ChecklistItems = new List<string>();
        }
    }
}
