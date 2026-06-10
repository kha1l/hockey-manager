using System;
using System.Collections.Generic;

[Serializable]
public class GameState
{
    public string SelectedTeamId;
    public List<TeamData> Teams = new List<TeamData>();
}
