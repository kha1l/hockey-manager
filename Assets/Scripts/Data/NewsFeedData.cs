using System;
using System.Collections.Generic;

[Serializable]
public class NewsFeedData
{
    public List<NewsItemData> Items = new List<NewsItemData>();
    public int TotalCreated;
    public string LastNewsAtUtc;

    public NewsFeedData()
    {
        EnsureItems();
    }

    public void EnsureItems()
    {
        if (Items == null)
        {
            Items = new List<NewsItemData>();
        }

        if (LastNewsAtUtc == null)
        {
            LastNewsAtUtc = "";
        }
    }
}
