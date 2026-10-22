using UnityEngine;
using UnityEngine.SceneManagement;

public static class AdventureProgress
{
    public static readonly string[] Scenes = { "Scene 1", "Scene 2", "Scene 4" };
    public static readonly string[] Titles = { "Woodland", "Lakeside", "Rocky Cave" };
    public static int Count => Scenes.Length;
    public static int Unlocked => Mathf.Clamp(PlayerPrefs.GetInt("adventureUnlocked", 1), 1, Count);

    public static int LevelNumber(string sceneName) => System.Array.IndexOf(Scenes, sceneName) + 1;
    public static bool IsCompleted(int level) => PlayerPrefs.GetInt("adventureComplete" + level, 0) == 1;
    public static int BestScore(int level) => PlayerPrefs.GetInt("adventureBest" + level, 0);

    public static void Complete(int level, int score)
    {
        if (level < 1 || level > Count) return;
        PlayerPrefs.SetInt("adventureComplete" + level, 1);
        PlayerPrefs.SetInt("adventureBest" + level, Mathf.Max(BestScore(level), score));
        PlayerPrefs.SetInt("adventureUnlocked", Mathf.Max(Unlocked, Mathf.Min(level + 1, Count)));
        PlayerPrefs.Save();
    }

    public static void StartLevel(int level)
    {
        if (level < 1 || level > Unlocked) return;
        int index = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + Scenes[level - 1] + ".unity");
        if (index < 0) return;
        // Selecting a level starts a fresh run; door transitions keep the run's points.
        PlayerPrefs.DeleteKey("points");
        PlayerPrefs.Save();
        GameSession.LoadScene(index);
    }
}
