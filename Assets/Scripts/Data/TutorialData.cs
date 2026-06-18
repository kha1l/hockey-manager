using System;
using System.Collections.Generic;

[Serializable]
public class TutorialData
{
    public bool IsTutorialEnabled = true;
    public bool HasCompletedIntro;
    public bool HasCompletedChecklist;
    public string CurrentStepId = TutorialConfig.StepOpenDashboard;
    public string LastShownPanel = "";
    public List<string> CompletedStepIds = new List<string>();
    public List<string> DismissedHintIds = new List<string>();
    public int TutorialVersion = TutorialConfig.CurrentTutorialVersion;
    public string StartedAtUtc = DateTime.UtcNow.ToString("o");
    public string CompletedAtUtc = "";
    public string LastUpdatedAtUtc = DateTime.UtcNow.ToString("o");

    public TutorialData()
    {
        EnsureCollections();
    }

    public void EnsureCollections()
    {
        if (CompletedStepIds == null)
        {
            CompletedStepIds = new List<string>();
        }

        if (DismissedHintIds == null)
        {
            DismissedHintIds = new List<string>();
        }

        if (string.IsNullOrEmpty(CurrentStepId))
        {
            CurrentStepId = TutorialConfig.StepOpenDashboard;
        }

        if (TutorialVersion <= 0)
        {
            TutorialVersion = TutorialConfig.CurrentTutorialVersion;
        }

        if (StartedAtUtc == null)
        {
            StartedAtUtc = "";
        }

        if (CompletedAtUtc == null)
        {
            CompletedAtUtc = "";
        }

        if (LastUpdatedAtUtc == null)
        {
            LastUpdatedAtUtc = "";
        }
    }
}
