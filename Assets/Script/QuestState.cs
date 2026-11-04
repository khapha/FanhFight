using UnityEngine;
using UnityEngine.SceneManagement;

// Tiến trình nhiệm vụ của màn phiêu lưu: chìa khóa, Vua Gián, dash, bí mật.
// Một nguồn sự thật duy nhất để Gate / Door / HUD / pickup cùng đọc, tránh mỗi chỗ tự đếm.
public static class QuestState
{
    public const int KeysRequired = 3;
    public const int BossLevel = 3;
    public const int MiniBossLevel = 4;
    public const int BossMaxHp = 6;
    public const int MiniBossMaxHp = 4;

    public static int Level { get; private set; }
    public static int KeysCollected { get; private set; }
    public static bool BossDefeated { get; private set; }
    public static bool MiniBossDefeated { get; private set; }
    public static int BossHp { get; set; } = -1;   // -1: chưa đánh nhau
    public static int RunSquished { get; set; }    // số gián được "kết bạn" trong lượt chơi này
    public static int RunSecrets { get; set; }

    public static bool DashUnlocked => PlayerPrefs.GetInt("dashUnlocked", 0) == 1;
    public static bool HasAllKeys => KeysCollected >= KeysRequired;

    public static void BeginRun()
    {
        RunSquished = 0;
        RunSecrets = 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Level = GameHUD.LevelNumber(scene.name);
        KeysCollected = 0;
        BossDefeated = false;
        BossHp = -1;
        MiniBossDefeated = false;
        // lượt chơi mới bắt đầu từ màn 1: đếm lại thành tích chạy nước
        if (Level == 1) BeginRun();
    }

    public static void CollectKey()
    {
        KeysCollected++;
        if (HasAllKeys) GameHUD.Toast("All keys collected!");
    }

    public static void DefeatBoss()
    {
        BossDefeated = true;
        BossHp = -1;
        GameHUD.Toast("The Roach King gave up!");
    }

    // boss phụ màn Sunset Meadow: Gián Vũ Công khuấy sân khấu
    public static void DefeatMiniBoss()
    {
        MiniBossDefeated = true;
        GameHUD.Toast("The Roach Dancer took a bow!");
    }

    public static void UnlockDash()
    {
        PlayerPrefs.SetInt("dashUnlocked", 1);
        PlayerPrefs.Save();
    }

    // đủ điều kiện qua cửa: đủ chìa khóa, màn boss cần Vua Gián đầu hàng, màn mini-boss cần Vũ Công cúi chào
    public static bool ReadyToFinish(int level) => HasAllKeys
        && (level != BossLevel || BossDefeated)
        && (level != MiniBossLevel || MiniBossDefeated);

    public static string BlockerHint()
    {
        if (!HasAllKeys) return "Find the golden keys! (" + KeysCollected + "/" + KeysRequired + ")";
        if (Level == BossLevel && !BossDefeated) return "The Roach King still blocks the way!";
        if (Level == MiniBossLevel && !MiniBossDefeated) return "The Roach Dancer still owns the stage!";
        return "";
    }

    // dòng nhiệm vụ ngắn hiển thị trên HUD
    public static string ObjectiveText
    {
        get
        {
            if (Level < 1) return "";
            if (!HasAllKeys) return "Keys " + KeysCollected + "/" + KeysRequired;
            if (Level == BossLevel && !BossDefeated) return "Bop the Roach King!";
            if (Level == MiniBossLevel && !MiniBossDefeated) return "Outdance the Roach Dancer!";
            return "Reach the door!";
        }
    }

    public static void FindSecret()
    {
        RunSecrets++;
        PlayerPrefs.SetInt("adventureSecret" + Level, 1);
        PlayerPrefs.Save();
    }

    public static bool SecretFound(int level) => PlayerPrefs.GetInt("adventureSecret" + level, 0) == 1;
}
