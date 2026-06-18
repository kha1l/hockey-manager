using System.Collections.Generic;

public static class AlphaPlaytestChecklistService
{
    public static AlphaPlaytestChecklistData BuildChecklist()
    {
        AlphaPlaytestChecklistData checklist = new AlphaPlaytestChecklistData
        {
            Summary = "Playable alpha checklist: run these steps before a manual playtest."
        };

        checklist.ChecklistItems = new List<string>
        {
            "Start new game.",
            "Pick team.",
            "Open Dashboard.",
            "Validate diagnostics.",
            "Simulate first day.",
            "Fix lineup alert if any.",
            "Open contracts/extensions.",
            "Simulate to trade deadline.",
            "Simulate to playoffs.",
            "Complete season.",
            "Review owner evaluation.",
            "Review awards/history/news.",
            "Start next season.",
            "Save/load.",
            "Run alpha balance report."
        };

        return checklist;
    }
}
