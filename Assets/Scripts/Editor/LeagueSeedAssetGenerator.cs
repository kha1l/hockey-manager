using System.IO;
using UnityEditor;
using UnityEngine;

public static class LeagueSeedAssetGenerator
{
    private const string SeedDirectoryPath = "Assets/Resources/Seeds";
    private const string SeedAssetPath = SeedDirectoryPath + "/league_seed.json";
    private const string SeedVersionAssetPath = SeedDirectoryPath + "/league_seed_version.txt";

    [MenuItem("Tools/Continental Hockey Manager/Generate League Seed")]
    public static void GenerateLeagueSeed()
    {
        Directory.CreateDirectory(SeedDirectoryPath);
        LeagueSeedData seed = LeagueSeedGenerator.CreateLeagueSeed();
        string json = JsonUtility.ToJson(seed, false);
        File.WriteAllText(SeedAssetPath, json);
        File.WriteAllText(SeedVersionAssetPath, LeagueSeedGenerator.CurrentSeedVersion);
        AssetDatabase.ImportAsset(SeedAssetPath);
        AssetDatabase.ImportAsset(SeedVersionAssetPath);
        AssetDatabase.Refresh();

        Debug.Log("League seed generated: " + SeedAssetPath
            + " | teams: " + (seed.Teams == null ? 0 : seed.Teams.Count)
            + " | players: " + CountPlayers(seed));
    }

    private static int CountPlayers(LeagueSeedData seed)
    {
        if (seed == null || seed.Teams == null)
        {
            return 0;
        }

        int count = 0;
        foreach (TeamData team in seed.Teams)
        {
            if (team != null && team.Players != null)
            {
                count += team.Players.Count;
            }
        }

        return count;
    }
}
