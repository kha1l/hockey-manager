using System;

[Serializable]
public class TutorialHintData
{
    public string HintId;
    public string PanelId;
    public string Title;
    public string Body;
    public int Priority;
    public bool CanDismiss = true;
}
