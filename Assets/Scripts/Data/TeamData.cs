using System;
using System.Collections.Generic;

[Serializable]
public class TeamData
{
    public string Id;
    public string Name;
    public string City;
    public string Abbreviation;
    public List<PlayerData> Players = new List<PlayerData>();
}
