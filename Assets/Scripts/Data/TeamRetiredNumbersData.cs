using System;
using System.Collections.Generic;

[Serializable]
public class TeamRetiredNumbersData
{
    public string TeamId;
    public string TeamName;
    public List<RetiredNumberData> RetiredNumbers = new List<RetiredNumberData>();
    public string UpdatedAtUtc;

    public TeamRetiredNumbersData()
    {
        EnsureRetiredNumbers();
    }

    public void EnsureRetiredNumbers()
    {
        if (RetiredNumbers == null)
        {
            RetiredNumbers = new List<RetiredNumberData>();
        }

        if (UpdatedAtUtc == null)
        {
            UpdatedAtUtc = "";
        }
    }
}
