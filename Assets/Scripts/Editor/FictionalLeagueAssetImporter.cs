using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FictionalLeagueAssetImporter
{
    private const string StaticRoot = "Assets/Static";
    private const string ResourcesTeamsRoot = "Assets/Resources/Teams";

    [MenuItem("Tools/Continental Hockey Manager/Import Static Team Assets")]
    public static void ImportStaticTeamAssets()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder(ResourcesTeamsRoot);

        int copied = 0;
        List<string> warnings = new List<string>();
        foreach (TeamIdentityData identity in TeamIdentityService.GetAllIdentities())
        {
            if (identity == null)
            {
                continue;
            }

            string sourceFolder = FindSourceFolder(identity);
            if (string.IsNullOrEmpty(sourceFolder))
            {
                warnings.Add("Missing static folder: " + identity.AssetFolderName);
                continue;
            }

            string targetFolder = ResourcesTeamsRoot + "/" + identity.TeamId;
            EnsureFolder(targetFolder);
            copied += CopyAsset(sourceFolder, targetFolder, "logo", 1024, warnings);
            copied += CopyAsset(sourceFolder, targetFolder, "home", 2048, warnings);
            copied += CopyAsset(sourceFolder, targetFolder, "away", 2048, warnings);
            copied += CopyAsset(sourceFolder, targetFolder, "full", 2048, warnings);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        foreach (string warning in warnings)
        {
            Debug.LogWarning("FictionalLeagueAssetImporter: " + warning);
        }

        Debug.Log("Continental Hockey Manager: imported static team assets. Files copied: " + copied + ", warnings: " + warnings.Count);
    }

    [MenuItem("Tools/Continental Hockey Manager/Validate Team Assets")]
    public static void ValidateTeamAssets()
    {
        List<string> warnings = new List<string>();
        foreach (TeamIdentityData identity in TeamIdentityService.GetAllIdentities())
        {
            if (identity == null)
            {
                continue;
            }

            string targetFolder = ResourcesTeamsRoot + "/" + identity.TeamId;
            ValidateFile(targetFolder, "logo", warnings);
            ValidateFile(targetFolder, "home", warnings);
            ValidateFile(targetFolder, "away", warnings);
            ValidateFile(targetFolder, "full", warnings);
        }

        if (warnings.Count == 0)
        {
            Debug.Log("Continental Hockey Manager: all team assets are present.");
            return;
        }

        foreach (string warning in warnings)
        {
            Debug.LogWarning("FictionalLeagueAssetImporter: " + warning);
        }
    }

    private static string FindSourceFolder(TeamIdentityData identity)
    {
        List<string> candidates = new List<string>
        {
            identity.AssetFolderName,
            identity.DisplayName
        };

        if (identity.TeamId == "yekaterinburg_hammers")
        {
            candidates.Add("Ekaterinburg Hammers");
        }

        if (identity.TeamId == "belgorod_lions")
        {
            candidates.Add("Belgorod Lyons");
        }

        foreach (string candidate in candidates)
        {
            if (string.IsNullOrEmpty(candidate))
            {
                continue;
            }

            string path = StaticRoot + "/" + candidate;
            if (AssetDatabase.IsValidFolder(path))
            {
                return path;
            }
        }

        return "";
    }

    private static int CopyAsset(string sourceFolder, string targetFolder, string assetName, int maxTextureSize, List<string> warnings)
    {
        string sourcePath = sourceFolder + "/" + assetName + ".png";
        string targetPath = targetFolder + "/" + assetName + ".png";
        if (!File.Exists(sourcePath))
        {
            warnings.Add("Missing source asset: " + sourcePath);
            return 0;
        }

        if (File.Exists(targetPath))
        {
            AssetDatabase.DeleteAsset(targetPath);
        }

        if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
        {
            warnings.Add("Failed to copy " + sourcePath + " -> " + targetPath);
            return 0;
        }

        AssetDatabase.ImportAsset(targetPath);
        TextureImporter importer = AssetImporter.GetAtPath(targetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = maxTextureSize;
            importer.SaveAndReimport();
        }

        return 1;
    }

    private static void ValidateFile(string folder, string assetName, List<string> warnings)
    {
        string path = folder + "/" + assetName + ".png";
        if (!File.Exists(path))
        {
            warnings.Add("Missing imported asset: " + path);
        }
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
        string name = Path.GetFileName(folder);
        if (!string.IsNullOrEmpty(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, name);
    }
}
