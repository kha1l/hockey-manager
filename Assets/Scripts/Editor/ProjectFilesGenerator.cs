using System.IO;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

public static class ProjectFilesGenerator
{
    private static readonly string[] CodeEditorCandidates =
    {
        "/snap/code/current/usr/share/code/bin/code",
        "/snap/code/current/usr/share/code/code",
        "/usr/share/code/bin/code",
        "/usr/bin/code",
        "/snap/bin/code"
    };

    [MenuItem("Tools/NHL Manager/Generate Project Files")]
    public static void GenerateProjectFiles()
    {
        EnsureExternalScriptEditor();
        CodeEditor.CurrentEditor.SyncAll();

        AssetDatabase.Refresh();

        Debug.Log("NHL Manager: project files generated.");
    }

    private static void EnsureExternalScriptEditor()
    {
        foreach (string candidate in CodeEditorCandidates)
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            CodeEditor.SetExternalScriptEditor(candidate);
            return;
        }
    }
}
