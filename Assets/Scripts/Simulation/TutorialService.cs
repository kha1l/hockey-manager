using System;
using System.Collections.Generic;

public static class TutorialService
{
    public static void EnsureTutorial(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.Tutorial == null)
        {
            state.Tutorial = new TutorialData
            {
                IsTutorialEnabled = true,
                HasCompletedIntro = false,
                HasCompletedChecklist = false,
                CurrentStepId = TutorialConfig.StepOpenDashboard,
                TutorialVersion = TutorialConfig.CurrentTutorialVersion,
                StartedAtUtc = DateTime.UtcNow.ToString("o"),
                LastUpdatedAtUtc = DateTime.UtcNow.ToString("o")
            };
        }

        state.Tutorial.EnsureCollections();
        if (state.Tutorial.TutorialVersion < TutorialConfig.CurrentTutorialVersion)
        {
            state.Tutorial.TutorialVersion = TutorialConfig.CurrentTutorialVersion;
        }
    }

    public static List<TutorialStepData> GetTutorialSteps(GameState state)
    {
        EnsureTutorial(state);
        List<TutorialStepData> steps = TutorialConfig.GetDefaultSteps();
        if (state == null || state.Tutorial == null)
        {
            return steps;
        }

        foreach (TutorialStepData step in steps)
        {
            if (step == null)
            {
                continue;
            }

            step.IsCompleted = IsStepCompleted(state, step.StepId);
        }

        return steps;
    }

    public static TutorialHintData GetPanelHint(GameState state, string panelId)
    {
        EnsureTutorial(state);
        if (state == null
            || state.Tutorial == null
            || !state.Tutorial.IsTutorialEnabled
            || state.Tutorial.HasCompletedChecklist)
        {
            return null;
        }

        TutorialHintData hint = TutorialConfig.GetHintForPanel(panelId);
        return hint == null || IsHintDismissed(state, hint.HintId) ? null : hint;
    }

    public static void MarkStepCompleted(GameState state, string stepId)
    {
        EnsureTutorial(state);
        if (state == null || state.Tutorial == null || string.IsNullOrEmpty(stepId))
        {
            return;
        }

        if (!state.Tutorial.CompletedStepIds.Contains(stepId))
        {
            state.Tutorial.CompletedStepIds.Add(stepId);
        }

        state.Tutorial.HasCompletedIntro = true;
        state.Tutorial.CurrentStepId = FindNextIncompleteStepId(state);
        if (string.IsNullOrEmpty(state.Tutorial.CurrentStepId))
        {
            CompleteChecklist(state);
        }
        else
        {
            Touch(state);
        }
    }

    public static bool IsStepCompleted(GameState state, string stepId)
    {
        EnsureTutorial(state);
        return state != null
            && state.Tutorial != null
            && state.Tutorial.CompletedStepIds != null
            && !string.IsNullOrEmpty(stepId)
            && state.Tutorial.CompletedStepIds.Contains(stepId);
    }

    public static void MarkPanelVisited(GameState state, string panelId)
    {
        EnsureTutorial(state);
        if (state == null || state.Tutorial == null)
        {
            return;
        }

        string normalized = TutorialConfig.NormalizePanelId(panelId);
        state.Tutorial.LastShownPanel = normalized;
        if (normalized == TutorialConfig.PanelDashboard)
        {
            MarkStepCompleted(state, TutorialConfig.StepOpenDashboard);
        }
        else if (normalized == TutorialConfig.PanelRoster)
        {
            MarkStepCompleted(state, TutorialConfig.StepOpenRoster);
        }
        else if (normalized == TutorialConfig.PanelLineup)
        {
            MarkStepCompleted(state, TutorialConfig.StepOpenLineup);
        }
        else if (normalized == TutorialConfig.PanelStandings)
        {
            MarkStepCompleted(state, TutorialConfig.StepOpenStandings);
        }
        else if (normalized == TutorialConfig.PanelContracts)
        {
            MarkStepCompleted(state, TutorialConfig.StepOpenContracts);
        }
        else if (normalized == TutorialConfig.PanelOwner)
        {
            MarkStepCompleted(state, TutorialConfig.StepOpenOwner);
        }
        else
        {
            Touch(state);
        }
    }

    public static void DismissHint(GameState state, string hintId)
    {
        EnsureTutorial(state);
        if (state == null || state.Tutorial == null || string.IsNullOrEmpty(hintId))
        {
            return;
        }

        if (!state.Tutorial.DismissedHintIds.Contains(hintId))
        {
            state.Tutorial.DismissedHintIds.Add(hintId);
        }

        Touch(state);
    }

    public static bool IsHintDismissed(GameState state, string hintId)
    {
        EnsureTutorial(state);
        return state != null
            && state.Tutorial != null
            && state.Tutorial.DismissedHintIds != null
            && !string.IsNullOrEmpty(hintId)
            && state.Tutorial.DismissedHintIds.Contains(hintId);
    }

    public static void CompleteIntro(GameState state)
    {
        EnsureTutorial(state);
        if (state == null || state.Tutorial == null)
        {
            return;
        }

        state.Tutorial.HasCompletedIntro = true;
        Touch(state);
    }

    public static void CompleteChecklist(GameState state)
    {
        EnsureTutorial(state);
        if (state == null || state.Tutorial == null)
        {
            return;
        }

        state.Tutorial.HasCompletedChecklist = true;
        state.Tutorial.CurrentStepId = "";
        if (string.IsNullOrEmpty(state.Tutorial.CompletedAtUtc))
        {
            state.Tutorial.CompletedAtUtc = DateTime.UtcNow.ToString("o");
        }

        Touch(state);
    }

    public static void DisableTutorial(GameState state)
    {
        EnsureTutorial(state);
        if (state == null || state.Tutorial == null)
        {
            return;
        }

        state.Tutorial.IsTutorialEnabled = false;
        Touch(state);
    }

    public static void EnableTutorial(GameState state)
    {
        EnsureTutorial(state);
        if (state == null || state.Tutorial == null)
        {
            return;
        }

        state.Tutorial.IsTutorialEnabled = true;
        Touch(state);
    }

    public static void ResetTutorial(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.Tutorial = new TutorialData();
        Touch(state);
    }

    public static int GetChecklistProgressPercent(GameState state)
    {
        List<TutorialStepData> steps = GetTutorialSteps(state);
        if (steps.Count == 0)
        {
            return 100;
        }

        int completed = 0;
        foreach (TutorialStepData step in steps)
        {
            if (step != null && step.IsCompleted)
            {
                completed++;
            }
        }

        return completed * 100 / steps.Count;
    }

    public static string BuildTutorialSummary(GameState state)
    {
        EnsureTutorial(state);
        if (state == null || state.Tutorial == null)
        {
            return "Tutorial unavailable";
        }

        if (!state.Tutorial.IsTutorialEnabled)
        {
            return "Tutorial disabled";
        }

        List<TutorialStepData> steps = GetTutorialSteps(state);
        int completed = 0;
        foreach (TutorialStepData step in steps)
        {
            if (step != null && step.IsCompleted)
            {
                completed++;
            }
        }

        if (state.Tutorial.HasCompletedChecklist || completed >= steps.Count)
        {
            return "Tutorial completed";
        }

        return "Tutorial: " + completed + "/" + steps.Count + " completed";
    }

    private static string FindNextIncompleteStepId(GameState state)
    {
        foreach (TutorialStepData step in GetTutorialSteps(state))
        {
            if (step != null && !step.IsCompleted)
            {
                return step.StepId;
            }
        }

        return "";
    }

    private static void Touch(GameState state)
    {
        if (state != null && state.Tutorial != null)
        {
            state.Tutorial.LastUpdatedAtUtc = DateTime.UtcNow.ToString("o");
        }
    }
}
