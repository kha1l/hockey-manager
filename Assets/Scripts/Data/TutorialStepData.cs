using System;

[Serializable]
public class TutorialStepData
{
    public string StepId;
    public string Title;
    public string Description;
    public string TargetPanel;
    public string ActionLabel;
    public bool IsCompleted;
    public bool IsOptional;
    public int Order;
    public string CompletedAtUtc;
}
