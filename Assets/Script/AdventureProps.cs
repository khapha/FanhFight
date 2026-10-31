using UnityEngine;
using UnityEngine.SceneManagement;

// Dựng toàn bộ nội dung phiêu lưu mới lúc scene load: NPC bướm tiên, chìa khóa,
// cổng đá, gián bộ hành, orb dash, kho báu bí mật và Vua Gián (màn cuối).
// Không phải sửa gì trong Editor - mọi vật thể đều sinh bằng code như Checkpoint.
public static class AdventureProps
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int level = GameHUD.LevelNumber(scene.name);
        if (level < 1) return;
        switch (level)
        {
            case 1:
                // tọa độ props đã né các bãi spikes (46-49, 84-95, 150-165) và tháp pháo (142)
                Populate(1, maxX: 190, npc: 8, orb: 105, secret: 175,
                    keys: new[] { 55f, 100f, 136f }, walkers: new[] { 60f, 78f, 120f, 168f });
                break;
            case 2:
                // né spikes 104-119 và tháp pháo 46, 134
                Populate(2, maxX: 178, npc: 8, orb: -1, secret: 155,
                    keys: new[] { 60f, 124f, 165f }, walkers: new[] { 70f, 100f, 140f });
                break;
            case 3:
                // né spikes 20-23, 40-43, 64-79, 100-124, 172-175 và tháp pháo 61, 182, 198
                Populate(3, maxX: 212, npc: 8, orb: -1, secret: 145,
                    keys: new[] { 55f, 128f, 185f }, walkers: new[] { 60f, 95f, 130f, 168f }, boss: 202f);
                break;
        }
    }

    static void Populate(int level, float maxX, float npc, float orb, float secret, float[] keys, float[] walkers, float boss = -1)
    {
        if (npc > 0) ButterflyNPC.SpawnAt(npc, level);
        foreach (float x in keys) KeyPickup.SpawnAt(x);
        foreach (float x in walkers) RoachWalker.SpawnAt(x);
        // orb dash chỉ xuất hiện khi chưa mở khóa; mở khóa rồi thì giữ vĩnh viễn
        if (orb > 0 && !QuestState.DashUnlocked) DashOrb.SpawnAt(orb);
        if (secret > 0 && !QuestState.SecretFound(level)) SecretSparkle.SpawnAt(secret, level);
        Gate.SpawnAt(maxX + 2, level);
        if (boss > 0) RoachKing.SpawnAt(boss, boss - 14f);
        Decorate(maxX);
    }

    // vài bông hoa dọc đường cho đỡ trống
    static void Decorate(float maxX)
    {
        for (int i = 0; i < 7; i++)
        {
            float x = Random.Range(12f, Mathf.Max(14f, maxX - 10f));
            if (!TryGround(x, out float y)) continue;
            GameObject go = new GameObject("PathFlower");
            go.transform.position = new Vector3(x, y + 0.35f, 0);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CuteArt.Flower;
            sr.sortingOrder = 2;
        }
    }

    // tìm mặt đất tại x bằng tia chiếu từ trên xuống (giống Checkpoint),
    // nhưng từ chối chỗ đất nằm sâu dưới mặt nước/trigger (không đặt đồ dưới hồ)
    public static bool TryGround(float x, out float y)
    {
        y = float.NaN;
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, 30f), Vector2.down, 60f);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        float triggerTop = float.PositiveInfinity;
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider.isTrigger) { triggerTop = Mathf.Min(triggerTop, h.point.y); continue; }
            if (h.collider.attachedRigidbody != null) continue;
            y = h.point.y;
            break;
        }
        if (float.IsNaN(y)) return false;
        if (triggerTop < y - 1.2f) return false; // có mặt nước phía trên, đất quá sâu
        return true;
    }

    public static float GroundY(float x) => TryGround(x, out float y) ? y : float.NaN;

    // thử vài vị trí lân cận nếu chỗ chính không đặt được (hố, nước...)
    public static float GroundYNear(float x)
    {
        foreach (float dx in new[] { 0f, 5f, -5f, 10f, -10f })
            if (TryGround(x + dx, out float y)) return y;
        return float.NaN;
    }
}
